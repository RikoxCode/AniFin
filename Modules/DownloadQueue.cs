using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Plugin.AniFin.Configuration;

namespace Jellyfin.Plugin.AniFin.Modules;

/// <summary>
/// Represents a single download item in the queue.
/// </summary>
public class DownloadQueueItem
{
    /// <summary>
    /// Gets or sets the unique identifier for this download item.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();
    
    /// <summary>
    /// Gets or sets the URL to download from.
    /// </summary>
    public string Url { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the preferred language.
    /// </summary>
    public string Language { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the preferred provider.
    /// </summary>
    public string Provider { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the URL type (series, season, episode).
    /// </summary>
    public string UrlType { get; set; } = "series";
    
    /// <summary>
    /// Gets or sets the current status of this download.
    /// </summary>
    public DownloadStatus Status { get; set; } = DownloadStatus.Pending;
    
    /// <summary>
    /// Gets or sets the download progress (0-100).
    /// </summary>
    public int Progress { get; set; } = 0;
    
    /// <summary>
    /// Gets or sets the error message if the download failed.
    /// </summary>
    public string? ErrorMessage { get; set; }
    
    /// <summary>
    /// Gets or sets when this item was added to the queue.
    /// </summary>
    public DateTime AddedAt { get; set; } = DateTime.Now;
    
    /// <summary>
    /// Gets or sets when this item started downloading.
    /// </summary>
    public DateTime? StartedAt { get; set; }
    
    /// <summary>
    /// Gets or sets when this item finished downloading.
    /// </summary>
    public DateTime? CompletedAt { get; set; }
    
    /// <summary>
    /// Gets or sets the number of retry attempts.
    /// </summary>
    public int RetryCount { get; set; } = 0;
    
    /// <summary>
    /// Gets or sets a bool to include series specials in the download
    /// </summary>
    public bool WithSpecials { get; set; } = false;
}

/// <summary>
/// Represents the status of a download.
/// </summary>
public enum DownloadStatus
{
    Pending,
    Downloading,
    Completed,
    Failed,
    Cancelled,
    Paused
}

/// <summary>
/// Represents statistics about the download queue.
/// </summary>
public class QueueStatistics
{
    /// <summary>
    /// Gets or sets the total number of items in the queue.
    /// </summary>
    public int TotalItems { get; set; }
    
    /// <summary>
    /// Gets or sets the number of pending items.
    /// </summary>
    public int PendingItems { get; set; }
    
    /// <summary>
    /// Gets or sets the number of downloading items.
    /// </summary>
    public int DownloadingItems { get; set; }
    
    /// <summary>
    /// Gets or sets the number of completed items.
    /// </summary>
    public int CompletedItems { get; set; }
    
    /// <summary>
    /// Gets or sets the number of failed items.
    /// </summary>
    public int FailedItems { get; set; }
    
    /// <summary>
    /// Gets or sets the number of cancelled items.
    /// </summary>
    public int CancelledItems { get; set; }
    
    /// <summary>
    /// Gets or sets the number of paused items.
    /// </summary>
    public int PausedItems { get; set; }
    
    /// <summary>
    /// Gets or sets the estimated time remaining in seconds.
    /// </summary>
    public double EstimatedTimeRemainingSeconds { get; set; }
    
    /// <summary>
    /// Gets or sets the average download time per item in seconds.
    /// </summary>
    public double AverageDownloadTimeSeconds { get; set; }
}

/// <summary>
/// Manages a queue of anime downloads with automatic processing and progress tracking.
/// </summary>
public class DownloadQueue
{
    private readonly ConcurrentQueue<DownloadQueueItem> _queue = new();
    private readonly List<DownloadQueueItem> _history = new();
    private readonly DownloadExecuter _downloader;
    private readonly SemaphoreSlim _queueLock = new(1, 1);
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    
    private DownloadQueueItem? _currentDownload;
    private Task? _processingTask;
    private bool _isPaused = false;
    private readonly object _pauseLock = new();
    
    /// <summary>
    /// Gets the current download item, if any.
    /// </summary>
    public DownloadQueueItem? CurrentDownload => _currentDownload;
    
    /// <summary>
    /// Gets a value indicating whether the queue is currently paused.
    /// </summary>
    public bool IsPaused => _isPaused;
    
    /// <summary>
    /// Gets a value indicating whether the queue is currently processing.
    /// </summary>
    public bool IsProcessing => _processingTask != null && !_processingTask.IsCompleted;
    
    /// <summary>
    /// Event raised when a download starts.
    /// </summary>
    public event EventHandler<DownloadQueueItem>? DownloadStarted;
    
    /// <summary>
    /// Event raised when a download completes.
    /// </summary>
    public event EventHandler<DownloadQueueItem>? DownloadCompleted;
    
    /// <summary>
    /// Event raised when a download fails.
    /// </summary>
    public event EventHandler<DownloadQueueItem>? DownloadFailed;
    
    /// <summary>
    /// Event raised when a download is cancelled.
    /// </summary>
    public event EventHandler<DownloadQueueItem>? DownloadCancelled;
    
    /// <summary>
    /// Event raised when download progress is updated.
    /// </summary>
    public event EventHandler<DownloadQueueItem>? ProgressUpdated;
    
    /// <summary>
    /// Event raised when the entire queue is completed.
    /// </summary>
    public event EventHandler? QueueCompleted;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="DownloadQueue"/> class.
    /// </summary>
    /// <param name="outputDirectory">Directory where downloaded files should be saved.</param>
    public DownloadQueue(string outputDirectory)
    {
        _downloader = new DownloadExecuter(outputDirectory);
    }

    /// <summary>
    /// Adds a new download to the queue.
    /// </summary>
    /// <param name="url">The URL to download from.</param>
    /// <param name="language">The preferred language.</param>
    /// <param name="provider">The preferred provider.</param>
    /// <param name="urlType">The URL type (series, season, episode).</param>
    /// <param name="withSpecials">Should the specials be included in the download</param>
    /// <returns>The ID of the queued download item.</returns>
    public Guid Enqueue(string url, string language = "", string provider = "", string urlType = "series", bool withSpecials = false)
    {
        FileLogger.Info($"[Queue.Enqueue] URL: {url}, Language: {language}, Provider: {provider}, UrlType: {urlType}");
    
        var item = new DownloadQueueItem
        {
            Url = url,
            Language = language,
            Provider = provider,
            UrlType = urlType,
            WithSpecials = withSpecials
        };
    
        _queue.Enqueue(item);
        _history.Add(item);
    
        FileLogger.Info($"[Queue.Enqueue] Item added with ID: {item.Id}");
        Console.WriteLine($"[Queue] Added: {url}");
    
        // Auto-start processing if not already running
        if (_processingTask == null || _processingTask.IsCompleted)
        {
            FileLogger.Info("[Queue.Enqueue] Starting processing task");
            StartProcessing();
        }
    
        return item.Id;
    }
    
    /// <summary>
    /// Starts processing the queue.
    /// </summary>
    private void StartProcessing()
    {
        _processingTask = Task.Run(async () => await ProcessQueueAsync());
    }
    
    /// <summary>
    /// Processes the download queue.
    /// </summary>
    private async Task ProcessQueueAsync()
    {
        Console.WriteLine("[Queue] Processing started");
    
        while (!_cancellationTokenSource.Token.IsCancellationRequested)
        {
            // Check if paused
            bool isPaused;
            lock (_pauseLock)
            {
                isPaused = _isPaused;
            }
        
            if (isPaused)
            {
                Console.WriteLine("[Queue] Paused");
                await Task.Delay(1000);
                continue;
            }
        
            // Get next item (FIFO - first in, first out)
            if (!_queue.TryDequeue(out var nextItem))
            {
                Console.WriteLine("[Queue] No more items to process");
                QueueCompleted?.Invoke(this, EventArgs.Empty);
                break;
            }
        
            // Skip if already processed or cancelled
            if (nextItem.Status != DownloadStatus.Pending)
            {
                continue;
            }
        
            await ProcessDownloadAsync(nextItem);
        }
    
        Console.WriteLine("[Queue] Processing stopped");
    }
    
    /// <summary>
    /// Processes a single download item.
    /// </summary>
    /// <param name="item">The download item to process.</param>
    private async Task ProcessDownloadAsync(DownloadQueueItem item)
    {
        _currentDownload = item;
        item.Status = DownloadStatus.Downloading;
        item.StartedAt = DateTime.Now;
        
        Console.WriteLine($"[Queue] Starting download: {item.Url}");
        DownloadStarted?.Invoke(this, item);
        
        try
        {
            // Simulate progress updates (in reality, you'd parse the CLI output)
            var progressTask = Task.Run(async () =>
            {
                for (int i = 0; i <= 100 && item.Status == DownloadStatus.Downloading; i += 10)
                {
                    item.Progress = i;
                    ProgressUpdated?.Invoke(this, item);
                    await Task.Delay(1000);
                }
            });
            
            // Execute the actual download
            bool success = item.UrlType.ToLower() switch
            {
                "series" => _downloader.DownloadSeries(item.Url, item.Language, item.Provider, item.WithSpecials),
                "season" => _downloader.DownloadSeason(item.Url, item.Language, item.Provider),
                "episode" => _downloader.DownloadEpisode(item.Url, item.Language, item.Provider),
                _ => _downloader.DownloadSeries(item.Url, item.Language, item.Provider, item.WithSpecials)
            };
            
            await progressTask;
            
            if (success)
            {
                item.Status = DownloadStatus.Completed;
                item.Progress = 100;
                item.CompletedAt = DateTime.Now;
                Console.WriteLine($"[Queue] Completed: {item.Url}");
                DownloadCompleted?.Invoke(this, item);
            }
            else
            {
                item.Status = DownloadStatus.Failed;
                item.ErrorMessage = "Download failed";
                Console.WriteLine($"[Queue] Failed: {item.Url}");
                DownloadFailed?.Invoke(this, item);
            }
        }
        catch (Exception ex)
        {
            item.Status = DownloadStatus.Failed;
            item.ErrorMessage = ex.Message;
            Console.WriteLine($"[Queue] Error: {item.Url} - {ex.Message}");
            DownloadFailed?.Invoke(this, item);
        }
        finally
        {
            _currentDownload = null;
        }
    }
    
    /// <summary>
    /// Pauses the queue processing.
    /// </summary>
    public void Pause()
    {
        lock (_pauseLock)
        {
            _isPaused = true;
            Console.WriteLine("[Queue] Paused");
        }
    }
    
    /// <summary>
    /// Resumes the queue processing.
    /// </summary>
    public void Resume()
    {
        lock (_pauseLock)
        {
            _isPaused = false;
            Console.WriteLine("[Queue] Resumed");
        }
    }
    
    /// <summary>
    /// Cancels a specific download by its ID.
    /// </summary>
    /// <param name="id">The ID of the download to cancel.</param>
    /// <returns>True if the download was cancelled; otherwise, false.</returns>
    public bool CancelDownload(Guid id)
    {
        var item = _history.FirstOrDefault(i => i.Id == id);
        if (item == null)
        {
            return false;
        }
        
        if (item.Status == DownloadStatus.Pending || item.Status == DownloadStatus.Paused)
        {
            item.Status = DownloadStatus.Cancelled;
            Console.WriteLine($"[Queue] Cancelled: {item.Url}");
            DownloadCancelled?.Invoke(this, item);
            return true;
        }
        
        if (item.Status == DownloadStatus.Downloading && _currentDownload?.Id == id)
        {
            item.Status = DownloadStatus.Cancelled;
            Console.WriteLine($"[Queue] Cancelled current download: {item.Url}");
            DownloadCancelled?.Invoke(this, item);
            // Note: Actual process cancellation would require killing the CLI process
            return true;
        }
        
        return false;
    }
    
    /// <summary>
    /// Clears all pending downloads from the queue.
    /// </summary>
    public void ClearQueue()
    {
        var items = new List<DownloadQueueItem>();
        while (_queue.TryDequeue(out var item))
        {
            if (item.Status == DownloadStatus.Pending)
            {
                item.Status = DownloadStatus.Cancelled;
                DownloadCancelled?.Invoke(this, item);
            }
        }
        
        Console.WriteLine("[Queue] Cleared all pending downloads");
    }
    
    /// <summary>
    /// Retries a failed download.
    /// </summary>
    /// <param name="id">The ID of the download to retry.</param>
    /// <returns>True if the download was queued for retry; otherwise, false.</returns>
    public bool RetryDownload(Guid id)
    {
        var item = _history.FirstOrDefault(i => i.Id == id);
        if (item == null || item.Status != DownloadStatus.Failed)
        {
            return false;
        }
        
        item.Status = DownloadStatus.Pending;
        item.Progress = 0;
        item.ErrorMessage = null;
        item.RetryCount++;
        
        _queue.Enqueue(item);
        
        Console.WriteLine($"[Queue] Retry queued: {item.Url} (Attempt: {item.RetryCount})");
        
        // Auto-start processing if not already running
        if (_processingTask == null || _processingTask.IsCompleted)
        {
            StartProcessing();
        }
        
        return true;
    }
    
    /// <summary>
    /// Gets statistics about the download queue.
    /// </summary>
    /// <returns>Queue statistics.</returns>
    public QueueStatistics GetStatistics()
    {
        var stats = new QueueStatistics
        {
            TotalItems = _history.Count,
            PendingItems = _history.Count(i => i.Status == DownloadStatus.Pending),
            DownloadingItems = _history.Count(i => i.Status == DownloadStatus.Downloading),
            CompletedItems = _history.Count(i => i.Status == DownloadStatus.Completed),
            FailedItems = _history.Count(i => i.Status == DownloadStatus.Failed),
            CancelledItems = _history.Count(i => i.Status == DownloadStatus.Cancelled),
            PausedItems = _history.Count(i => i.Status == DownloadStatus.Paused)
        };
        
        // Calculate average download time
        var completedItems = _history.Where(i => i.Status == DownloadStatus.Completed && 
                                                i.StartedAt.HasValue && 
                                                i.CompletedAt.HasValue).ToList();
        
        if (completedItems.Any())
        {
            stats.AverageDownloadTimeSeconds = completedItems
                .Average(i => (i.CompletedAt!.Value - i.StartedAt!.Value).TotalSeconds);
            
            // Estimate remaining time
            int remainingItems = stats.PendingItems + stats.DownloadingItems;
            stats.EstimatedTimeRemainingSeconds = remainingItems * stats.AverageDownloadTimeSeconds;
        }
        
        return stats;
    }
    
    /// <summary>
    /// Gets all items in the queue history.
    /// </summary>
    /// <returns>A list of all download items.</returns>
    public List<DownloadQueueItem> GetHistory()
    {
        return _history.ToList();
    }
    
    /// <summary>
    /// Gets all pending items in the queue.
    /// </summary>
    /// <returns>A list of pending download items.</returns>
    public List<DownloadQueueItem> GetPendingItems()
    {
        return _history.Where(i => i.Status == DownloadStatus.Pending).ToList();
    }
    
    /// <summary>
    /// Disposes of the queue and cancels all processing.
    /// </summary>
    public void Dispose()
    {
        _cancellationTokenSource.Cancel();
        _processingTask?.Wait();
        _cancellationTokenSource.Dispose();
        _queueLock.Dispose();
    }
}