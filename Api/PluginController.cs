using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Security.Claims;
using Jellyfin.Data.Enums;
using Jellyfin.Plugin.AniFin.Configuration;
using Jellyfin.Plugin.AniFin.Modules;
using MediaBrowser.Controller.Library;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.AniFin.Api
{
    /// <summary>
    /// API Controller for managing anime downloads via AniWorld.
    /// Uses Jellyfin authentication only.
    /// </summary>
    [ApiController]
    [Route("api/anifin")]
    [Authorize]
    public class AniFinController : ControllerBase
    {
        private readonly ILogger<AniFinController> _logger;
        private readonly IUserManager _userManager;
        private static DownloadQueue? _downloadQueue;
        private static readonly object _queueLock = new object();

        public AniFinController(ILogger<AniFinController> logger, IUserManager userManager)
        {
            _logger = logger;
            _userManager = userManager;
            
            lock (_queueLock)
            {
                if (_downloadQueue == null)
                {
                    var config = Plugin.Instance?.Configuration ?? new PluginConfiguration();
                    var outputDirectory = config.DownloadPath ?? "/downloads";
                    _downloadQueue = new DownloadQueue(outputDirectory);
                    
                    _downloadQueue.DownloadStarted += (s, item) => 
                        _logger.LogInformation("Download started: {Url}", item.Url);
                    
                    _downloadQueue.DownloadCompleted += (s, item) => 
                        _logger.LogInformation("Download completed: {Url}", item.Url);
                    
                    _downloadQueue.DownloadFailed += (s, item) => 
                        _logger.LogError("Download failed: {Url} - {Error}", item.Url, item.ErrorMessage);
                    
                    _downloadQueue.QueueCompleted += (s, e) => 
                        _logger.LogInformation("All downloads completed");
                }
            }
        }

        #region Configuration

        /// <summary>
        /// Gets the current plugin configuration.
        /// </summary>
        [HttpGet("config")]
        [ProducesResponseType(typeof(ConfigurationResponse), 200)]
        [ProducesResponseType(401)]
        public ActionResult<ConfigurationResponse> GetConfiguration()
        {
            var config = Plugin.Instance?.Configuration ?? new PluginConfiguration();

            return Ok(new ConfigurationResponse
            {
                DownloadPath = config.DownloadPath,
                AvailableLanguages = config.AvailablyLanguages,
                PreferredLanguage = config.PreferredLanguage,
                PreferredProvider = config.PreferredProvider,
                AvailableProviders = config.AvailablyProviders,
                MaxConcurrentDownloads = config.MaxConcurrentDownloads,
                AutoProcessQueue = config.AutoProcessQueue
            });
        }

        /// <summary>
        /// Updates the plugin configuration
        /// </summary>
        [HttpPut("config")]
        [ProducesResponseType(typeof(ConfigurationResponse), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public ActionResult<ConfigurationResponse> UpdateConfiguration([FromBody] UpdateConfigurationRequest request)
        {
            var config = Plugin.Instance?.Configuration;
            if (config == null)
            {
                return BadRequest(new ErrorResponse { Error = "Configuration not found" });
            }

            bool changed = false;

            if (!string.IsNullOrWhiteSpace(request.DownloadPath) && request.DownloadPath != config.DownloadPath)
            {
                config.DownloadPath = request.DownloadPath;
                changed = true;
                _logger.LogInformation("DownloadPath updated to: {Path}", request.DownloadPath);
            }

            if (!string.IsNullOrWhiteSpace(request.PreferredLanguage) && request.PreferredLanguage != config.PreferredLanguage)
            {
                if (!config.AvailablyLanguages.Contains(request.PreferredLanguage))
                {
                    return BadRequest(new ErrorResponse 
                    { 
                        Error = $"Invalid language. Available languages: {string.Join(", ", config.AvailablyLanguages)}" 
                    });
                }
                config.PreferredLanguage = request.PreferredLanguage;
                changed = true;
                _logger.LogInformation("PreferredLanguage updated to: {Language}", request.PreferredLanguage);
            }

            if (!string.IsNullOrWhiteSpace(request.PreferredProvider) && request.PreferredProvider != config.PreferredProvider)
            {
                if (!config.AvailablyProviders.Contains(request.PreferredProvider))
                {
                    return BadRequest(new ErrorResponse 
                    { 
                        Error = $"Invalid provider. Available providers: {string.Join(", ", config.AvailablyProviders)}" 
                    });
                }
                config.PreferredProvider = request.PreferredProvider;
                changed = true;
                _logger.LogInformation("PreferredProvider updated to: {Provider}", request.PreferredProvider);
            }

            if (request.MaxConcurrentDownloads.HasValue && request.MaxConcurrentDownloads.Value != config.MaxConcurrentDownloads)
            {
                if (request.MaxConcurrentDownloads.Value < 1 || request.MaxConcurrentDownloads.Value > 10)
                {
                    return BadRequest(new ErrorResponse { Error = "MaxConcurrentDownloads must be between 1 and 10" });
                }
                config.MaxConcurrentDownloads = request.MaxConcurrentDownloads.Value;
                changed = true;
                _logger.LogInformation("MaxConcurrentDownloads updated to: {Max}", request.MaxConcurrentDownloads);
            }

            if (changed)
            {
                Plugin.Instance?.SaveConfiguration();
            }

            return Ok(new ConfigurationResponse
            {
                DownloadPath = config.DownloadPath,
                AvailableLanguages = config.AvailablyLanguages,
                PreferredLanguage = config.PreferredLanguage,
                PreferredProvider = config.PreferredProvider,
                AvailableProviders = config.AvailablyProviders,
                MaxConcurrentDownloads = config.MaxConcurrentDownloads,
                AutoProcessQueue = config.AutoProcessQueue
            });
        }

        /// <summary>
        /// Gets only the editable configuration fields.
        /// </summary>
        [HttpGet("config/editable")]
        [ProducesResponseType(typeof(EditableConfigurationResponse), 200)]
        [ProducesResponseType(401)]
        public ActionResult<EditableConfigurationResponse> GetEditableConfiguration()
        {
            var config = Plugin.Instance?.Configuration ?? new PluginConfiguration();

            return Ok(new EditableConfigurationResponse
            {
                DownloadPath = config.DownloadPath,
                PreferredLanguage = config.PreferredLanguage,
                PreferredProvider = config.PreferredProvider,
                MaxConcurrentDownloads = config.MaxConcurrentDownloads
            });
        }

        #endregion

        #region Browser Extension

        /// <summary>
        /// Downloads the browser extension package.
        /// </summary>
        [HttpGet("extension/download")]
        [ProducesResponseType(typeof(FileResult), 200)]
        [ProducesResponseType(404)]
        public ActionResult DownloadExtension()
        {
            try
            {
                var pluginPath = Plugin.Instance?.GetType().Assembly.Location;
                if (string.IsNullOrEmpty(pluginPath))
                {
                    return NotFound(new ErrorResponse { Error = "Plugin path not found" });
                }

                var pluginDir = Path.GetDirectoryName(pluginPath);
                if (string.IsNullOrEmpty(pluginDir))
                {
                    return NotFound(new ErrorResponse { Error = "Plugin directory not found" });
                }

                var extensionPath = Path.Combine(pluginDir, "anifin-extension.zip");
                
                if (!System.IO.File.Exists(extensionPath))
                {
                    _logger.LogWarning("Browser extension not found at: {Path}", extensionPath);
                    return NotFound(new ErrorResponse { Error = "Browser extension package not found" });
                }

                _logger.LogInformation("Serving browser extension from: {Path}", extensionPath);

                var fileBytes = System.IO.File.ReadAllBytes(extensionPath);
                return File(fileBytes, "application/zip", "anifin-extension.zip");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to serve browser extension");
                return StatusCode(500, new ErrorResponse { Error = "Failed to download extension" });
            }
        }

        /// <summary>
        /// Gets information about the browser extension.
        /// </summary>
        [HttpGet("extension/info")]
        [ProducesResponseType(typeof(ExtensionInfoResponse), 200)]
        public ActionResult<ExtensionInfoResponse> GetExtensionInfo()
        {
            try
            {
                var pluginPath = Plugin.Instance?.GetType().Assembly.Location;
                var pluginDir = Path.GetDirectoryName(pluginPath);
                var extensionPath = pluginDir != null ? Path.Combine(pluginDir, "anifin-extension.zip") : null;
                
                bool isAvailable = extensionPath != null && System.IO.File.Exists(extensionPath);
                long? fileSize = null;
                DateTime? lastModified = null;

                if (isAvailable && extensionPath != null)
                {
                    var fileInfo = new FileInfo(extensionPath);
                    fileSize = fileInfo.Length;
                    lastModified = fileInfo.LastWriteTimeUtc;
                }

                return Ok(new ExtensionInfoResponse
                {
                    Available = isAvailable,
                    Version = "1.0.0",
                    FileName = "anifin-extension.zip",
                    DownloadUrl = $"{Request.Scheme}://{Request.Host}/api/anifin/extension/download",
                    FileSizeBytes = fileSize,
                    LastModified = lastModified,
                    SupportedBrowsers = new List<string> { "Chrome", "Edge", "Brave", "Opera" },
                    InstallInstructions = new List<string>
                    {
                        "1. Download the extension package",
                        "2. Unzip the downloaded file",
                        "3. Open your browser's extension page (chrome://extensions or edge://extensions)",
                        "4. Enable 'Developer mode'",
                        "5. Click 'Load unpacked' and select the unzipped folder",
                        "6. Click on the AniFin extension icon and enter your Jellyfin credentials"
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get extension info");
                return Ok(new ExtensionInfoResponse
                {
                    Available = false,
                    Version = "1.0.0",
                    FileName = "anifin-extension.zip",
                    DownloadUrl = $"{Request.Scheme}://{Request.Host}/api/anifin/extension/download"
                });
            }
        }

        #endregion

        #region Queue Operations

        [HttpPost("queue/add")]
        [ProducesResponseType(typeof(DownloadResponse), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(400)]
        public ActionResult<DownloadResponse> AddDownload([FromBody] DownloadRequest request)
        {
            FileLogger.Info("=== AddDownload called ===");
            FileLogger.Info($"Received request: URL={request?.Url}, Language={request?.Language}, Provider={request?.Provider}, UrlType={request?.UrlType}, WithSpecials={request?.WithSpecials}");
    
            if (string.IsNullOrWhiteSpace(request?.Url))
            {
                FileLogger.Warning("AddDownload rejected: URL is empty");
                return BadRequest(new ErrorResponse { Error = "URL is required" });
            }

            try
            {
                FileLogger.Info($"Enqueueing download: {request.Url}, WithSpecials: {request.WithSpecials}");
        
                if (_downloadQueue == null)
                {
                    FileLogger.Error("Download queue is NULL!");
                    return StatusCode(500, new ErrorResponse { Error = "Download queue not initialized" });
                }
        
                var id = _downloadQueue.Enqueue(
                    url: request.Url,
                    language: request.Language ?? string.Empty,
                    provider: request.Provider ?? string.Empty,
                    urlType: request.UrlType ?? "series",
                    withSpecials: request.WithSpecials
                );

                FileLogger.Info($"Download enqueued successfully with ID: {id}");
                _logger.LogInformation("Added download to queue: {Url} (ID: {Id}), WithSpecials: {WithSpecials}", request.Url, id, request.WithSpecials);

                return Ok(new DownloadResponse
                {
                    Success = true,
                    Message = "Download added to queue",
                    DownloadId = id.ToString()
                });
            }
            catch (Exception ex)
            {
                FileLogger.Error($"Failed to add download: {ex.Message}");
                FileLogger.Error($"Stack trace: {ex.StackTrace}");
                _logger.LogError(ex, "Failed to add download: {Url}", request.Url);
                return BadRequest(new ErrorResponse { Error = ex.Message });
            }
        }

        /// <summary>
        /// Gets queue statistics.
        /// </summary>
        [HttpGet("queue/stats")]
        [ProducesResponseType(typeof(QueueStatistics), 200)]
        [ProducesResponseType(401)]
        public ActionResult<QueueStatistics> GetStatistics()
        {
            var stats = _downloadQueue!.GetStatistics();
            return Ok(stats);
        }

        /// <summary>
        /// Gets the current download.
        /// </summary>
        [HttpGet("queue/current")]
        [ProducesResponseType(typeof(DownloadQueueItem), 200)]
        [ProducesResponseType(204)]
        [ProducesResponseType(401)]
        public ActionResult<DownloadQueueItem?> GetCurrentDownload()
        {
            var current = _downloadQueue!.CurrentDownload;
            
            if (current == null)
            {
                return NoContent();
            }

            return Ok(current);
        }

        /// <summary>
        /// Gets the queue history.
        /// </summary>
        [HttpGet("queue/history")]
        [ProducesResponseType(typeof(List<DownloadQueueItem>), 200)]
        [ProducesResponseType(401)]
        public ActionResult<List<DownloadQueueItem>> GetHistory()
        {
            var history = _downloadQueue!.GetHistory();
            return Ok(history);
        }

        /// <summary>
        /// Gets pending downloads.
        /// </summary>
        [HttpGet("queue/pending")]
        [ProducesResponseType(typeof(List<DownloadQueueItem>), 200)]
        [ProducesResponseType(401)]
        public ActionResult<List<DownloadQueueItem>> GetPending()
        {
            var pending = _downloadQueue!.GetPendingItems();
            return Ok(pending);
        }

        /// <summary>
        /// Cancels a specific download
        /// </summary>
        [HttpPost("queue/{id}/cancel")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public ActionResult CancelDownload(Guid id)
        {

            bool success = _downloadQueue!.CancelDownload(id);
            
            if (!success)
            {
                return NotFound(new ErrorResponse { Error = "Download not found or cannot be cancelled" });
            }

            _logger.LogInformation("Cancelled download: {Id}", id);
            return Ok(new { Message = "Download cancelled" });
        }

        /// <summary>
        /// Retries a failed download
        /// </summary>
        [HttpPost("queue/{id}/retry")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public ActionResult RetryDownload(Guid id)
        {
            bool success = _downloadQueue!.RetryDownload(id);
            
            if (!success)
            {
                return NotFound(new ErrorResponse { Error = "Download not found or cannot be retried" });
            }

            _logger.LogInformation("Retrying download: {Id}", id);
            return Ok(new { Message = "Download queued for retry" });
        }

        /// <summary>
        /// Pauses the download queue
        /// </summary>
        [HttpPost("queue/pause")]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public ActionResult PauseQueue()
        {
            _downloadQueue!.Pause();
            _logger.LogInformation("Queue paused");
            return Ok(new { Message = "Queue paused" });
        }

        /// <summary>
        /// Resumes the download queue
        /// </summary>
        [HttpPost("queue/resume")]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public ActionResult ResumeQueue()
        {
            _downloadQueue!.Resume();
            _logger.LogInformation("Queue resumed");
            return Ok(new { Message = "Queue resumed" });
        }

        /// <summary>
        /// Clears all pending downloads
        /// </summary>
        [HttpPost("queue/clear")]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public ActionResult ClearQueue()
        {
            _downloadQueue!.ClearQueue();
            _logger.LogInformation("Queue cleared");
            return Ok(new { Message = "Queue cleared" });
        }

        /// <summary>
        /// Gets the queue status.
        /// </summary>
        [HttpGet("queue/status")]
        [ProducesResponseType(typeof(QueueStatusResponse), 200)]
        [ProducesResponseType(401)]
        public ActionResult<QueueStatusResponse> GetQueueStatus()
        {
            return Ok(new QueueStatusResponse
            {
                IsPaused = _downloadQueue!.IsPaused,
                IsProcessing = _downloadQueue.IsProcessing,
                CurrentDownload = _downloadQueue.CurrentDownload
            });
        }

        #endregion

        #region Request/Response Models

        public class DownloadRequest
        {
            [Required]
            public string Url { get; set; } = string.Empty;
            public string Language { get; set; } = string.Empty;
            public string Provider { get; set; } = string.Empty;
            public string UrlType { get; set; } = "series";
            public bool WithSpecials { get; set; } = false;
        }

        public class DownloadResponse
        {
            public bool Success { get; set; }
            public string Message { get; set; } = string.Empty;
            public string DownloadId { get; set; } = string.Empty;
        }

        public class ConfigurationResponse
        {
            public string DownloadPath { get; set; } = string.Empty;
            public List<string> AvailableLanguages { get; set; } = new();
            public string PreferredLanguage { get; set; } = string.Empty;
            public string PreferredProvider { get; set; } = string.Empty;
            public List<string> AvailableProviders { get; set; } = new();
            public int MaxConcurrentDownloads { get; set; }
            public bool AutoProcessQueue { get; set; }
            public int TokenValidityMinutes { get; set; }
        }

        public class EditableConfigurationResponse
        {
            public string DownloadPath { get; set; } = string.Empty;
            public string PreferredLanguage { get; set; } = string.Empty;
            public string PreferredProvider { get; set; } = string.Empty;
            public int MaxConcurrentDownloads { get; set; }
            public int TokenValidityMinutes { get; set; }
        }

        public class UpdateConfigurationRequest
        {
            public string? DownloadPath { get; set; }
            public string? PreferredLanguage { get; set; }
            public string? PreferredProvider { get; set; }
            public int? MaxConcurrentDownloads { get; set; }
            public int? TokenValidityMinutes { get; set; }
        }

        public class QueueStatusResponse
        {
            public bool IsPaused { get; set; }
            public bool IsProcessing { get; set; }
            public DownloadQueueItem? CurrentDownload { get; set; }
        }

        public class ExtensionInfoResponse
        {
            public bool Available { get; set; }
            public string Version { get; set; } = string.Empty;
            public string FileName { get; set; } = string.Empty;
            public string DownloadUrl { get; set; } = string.Empty;
            public long? FileSizeBytes { get; set; }
            public DateTime? LastModified { get; set; }
            public List<string> SupportedBrowsers { get; set; } = new();
            public List<string> InstallInstructions { get; set; } = new();
        }

        public class ErrorResponse
        {
            public string Error { get; set; } = string.Empty;
        }

        #endregion
    }
}