using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using Jellyfin.Plugin.AniFin.Configuration;

namespace Jellyfin.Plugin.AniFin.Modules;

/// <summary>
/// Executes AniWorld CLI commands to download anime series, seasons, and episodes.
/// </summary>
public class DownloadExecuter
{
    private readonly string _outputDirectory;

    /// <summary>
    /// Initializes a new instance of the <see cref="DownloadExecuter"/> class.
    /// </summary>
    /// <param name="outputDirectory">Directory where downloaded files should be saved.</param>
    public DownloadExecuter(string outputDirectory)
    {
        _outputDirectory = outputDirectory ?? throw new ArgumentNullException(nameof(outputDirectory));

        FileLogger.Info($"[DownloadExecuter] Initializing with output directory: {_outputDirectory}");

        if (!Directory.Exists(_outputDirectory))
        {
            FileLogger.Info($"[DownloadExecuter] Creating output directory: {_outputDirectory}");
            Directory.CreateDirectory(_outputDirectory);
        }
    }

    /// <summary>
    /// Validates the URL format and checks if it matches the expected domain and path structure.
    /// </summary>
    /// <param name="url">The URL to validate.</param>
    /// <param name="type">The expected URL type: 1 = series, 2 = season, 3 = episode.</param>
    /// <returns>The validated URL if valid.</returns>
    /// <exception cref="ArgumentException">Thrown when the URL is invalid or doesn't match the expected format.</exception>
    private string ValidateUrl(string url, int type = 1)
    {
        FileLogger.Debug($"[DownloadExecuter.ValidateUrl] Validating URL: {url}, Type: {type}");

        if (string.IsNullOrWhiteSpace(url))
            throw new ArgumentException("URL cannot be empty.", nameof(url));

        if (!url.StartsWith("https://aniworld.to", StringComparison.OrdinalIgnoreCase) &&
            !url.StartsWith("https://s.to", StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("URL must start with 'https://aniworld.to' or 'https://s.to'.", nameof(url));

        string pattern = type switch
        {
            1 => @"^https://(aniworld\.to|s\.to)/anime/stream/[^/]+$", // series
            2 => @"^https://(aniworld\.to|s\.to)/anime/stream/[^/]+/staffel-\d+$", // season
            3 => @"^https://(aniworld\.to|s\.to)/anime/stream/[^/]+/staffel-\d+/episode-\d+$", // episode
            4 => @"^https://(aniworld\.to|s\.to)/anime/stream/[^/]+/filme$", // specials/movies
            _ => throw new ArgumentException($"Invalid type: {type}. Allowed values: 1..4.", nameof(type))
        };

        if (!System.Text.RegularExpressions.Regex.IsMatch(url, pattern,
                System.Text.RegularExpressions.RegexOptions.IgnoreCase))
        {
            string expectedFormat = type switch
            {
                1 => "/anime/stream/<anime-name>",
                2 => "/anime/stream/<anime-name>/staffel-<number>",
                3 => "/anime/stream/<anime-name>/staffel-<number>/episode-<number>",
                4 => "/anime/stream/<anime-name>/filme",
                _ => "unknown"
            };
            throw new ArgumentException($"URL doesn't match the expected format for type {type}: {expectedFormat}",
                nameof(url));
        }

        FileLogger.Info($"[DownloadExecuter.ValidateUrl] URL validated successfully: {url}");
        return url;
    }

    /// <summary>
    /// Builds the command-line arguments for the AniWorld CLI tool.
    /// </summary>
    private List<string> GetDownloaderArgs(string url, string language = "", string provider = "",
        string urlType = "series")
    {
        var args = new List<string>();
        if (string.IsNullOrWhiteSpace(url))
            throw new ArgumentException("URL cannot be empty", nameof(url));

        if (string.IsNullOrWhiteSpace(language))
            language = new PluginConfiguration().PreferredLanguage;

        if (string.IsNullOrWhiteSpace(provider))
            provider = new PluginConfiguration().PreferredProvider;

        int validationType = urlType.ToLower() switch
        {
            "series" => 1,
            "season" => 2,
            "episode" => 3,
            "specials" => 4,
            _ => 1
        };

        url = ValidateUrl(url, validationType);

        args.Add("--episode");
        args.Add(url);
        args.Add("-L");
        args.Add($"\"{language}\"");
        args.Add("--provider");
        args.Add(provider);
        args.Add("--output");
        args.Add(_outputDirectory);

        FileLogger.Info($"[DownloadExecuter.GetDownloaderArgs] Built command: aniworld {string.Join(" ", args)}");
        return args;
    }

    /// <summary>
    /// Executes the AniWorld CLI tool with the specified arguments.
    /// </summary>
    private (bool success, string output, string error) ExecuteDownloader(List<string> args)
    {
        FileLogger.Info($"[DownloadExecuter.ExecuteDownloader] Executing: aniworld {string.Join(" ", args)}");

        try
        {
            var processStartInfo = new ProcessStartInfo
            {
                FileName = "/root/.local/bin/aniworld",
                Arguments = string.Join(" ", args),
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            var outputBuilder = new StringBuilder();
            var errorBuilder = new StringBuilder();

            using (var process = new Process { StartInfo = processStartInfo })
            {
                process.OutputDataReceived += (sender, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                    {
                        outputBuilder.AppendLine(e.Data);
                    }
                };

                process.ErrorDataReceived += (sender, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                    {
                        errorBuilder.AppendLine(e.Data);
                        FileLogger.Warning($"[AniWorld CLI Error] {e.Data}");
                    }
                };

                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                process.WaitForExit();

                bool success = process.ExitCode == 0;

                FileLogger.Info(
                    $"[DownloadExecuter.ExecuteDownloader] Process exited with code: {process.ExitCode}, Success: {success}");

                if (!success)
                {
                    FileLogger.Error($"[DownloadExecuter.ExecuteDownloader] Error output: {errorBuilder}");
                }

                return (success, outputBuilder.ToString(), errorBuilder.ToString());
            }
        }
        catch (Exception ex)
        {
            FileLogger.Error($"[DownloadExecuter.ExecuteDownloader] Exception: {ex.Message}");
            FileLogger.Error($"[DownloadExecuter.ExecuteDownloader] Stack trace: {ex.StackTrace}");
            return (false, string.Empty, $"Failed to execute aniworld CLI: {ex.Message}");
        }
    }

    /// <summary>
    /// Downloads an entire anime series.
    /// </summary>
    public bool DownloadSeries(string url, string language = "", string provider = "", bool withSpecials = false)
    {
        FileLogger.Info($"[DownloadExecuter.DownloadSeries] Starting series download: {url}, WithSpecials: {withSpecials}");

        try
        {
            // 1) Download series
            var argsSeries = GetDownloaderArgs(url, language, provider, "series");
            var (okSeries, outSeries, errSeries) = ExecuteDownloader(argsSeries);

            if (!okSeries)
            {
                FileLogger.Error($"[DownloadExecuter.DownloadSeries] Series download failed. Error: {errSeries}");
                return false;
            }

            FileLogger.Info("[DownloadExecuter.DownloadSeries] Series download completed successfully");

            // 2) Optional: Download series specials
            if (withSpecials)
            {
                FileLogger.Info("[DownloadExecuter.DownloadSeries] WithSpecials is TRUE, downloading specials...");
                
                string specialsUrl = url.EndsWith("/filme", StringComparison.OrdinalIgnoreCase)
                    ? url
                    : (url.TrimEnd('/') + "/filme");

                FileLogger.Info($"[DownloadExecuter.DownloadSeries] Specials URL: {specialsUrl}");

                try
                {
                    var argsSpecials = GetDownloaderArgs(specialsUrl, language, provider, "specials");
                    var (okSpec, outSpec, errSpec) = ExecuteDownloader(argsSpecials);

                    if (!okSpec)
                    {
                        FileLogger.Warning($"[DownloadExecuter.DownloadSeries] Specials download failed or not available. Error: {errSpec}");
                    }
                    else
                    {
                        FileLogger.Info("[DownloadExecuter.DownloadSeries] Specials download completed successfully");
                    }
                }
                catch (Exception ex)
                {
                    FileLogger.Warning($"[DownloadExecuter.DownloadSeries] Specials download exception: {ex.Message}");
                }
            }
            else
            {
                FileLogger.Info("[DownloadExecuter.DownloadSeries] WithSpecials is FALSE, skipping specials");
            }

            // 3) Organize files into proper structure
            OrganizeDownloadedFiles(url, withSpecials);
            
            return true;
        }
        catch (Exception e)
        {
            FileLogger.Error($"[DownloadExecuter.DownloadSeries] Exception: {e.Message}");
            FileLogger.Error($"[DownloadExecuter.DownloadSeries] Stack trace: {e.StackTrace}");
            return false;
        }
    }

    /// <summary>
    /// Downloads a specific season of an anime series.
    /// </summary>
    public bool DownloadSeason(string url, string language = "", string provider = "")
    {
        FileLogger.Info($"[DownloadExecuter.DownloadSeason] Starting season download: {url}");

        try
        {
            List<string> args = GetDownloaderArgs(url, language, provider, "season");
            var (success, output, error) = ExecuteDownloader(args);

            if (!success)
            {
                FileLogger.Error($"[DownloadExecuter.DownloadSeason] Season download failed. Error: {error}");
            }
            else
            {
                FileLogger.Info($"[DownloadExecuter.DownloadSeason] Season download completed successfully");
                OrganizeDownloadedFiles(url, false);
            }

            return success;
        }
        catch (Exception e)
        {
            FileLogger.Error($"[DownloadExecuter.DownloadSeason] Exception: {e.Message}");
            FileLogger.Error($"[DownloadExecuter.DownloadSeason] Stack trace: {e.StackTrace}");
            return false;
        }
    }

    /// <summary>
    /// Downloads a specific episode of an anime series.
    /// </summary>
    public bool DownloadEpisode(string url, string language = "", string provider = "")
    {
        FileLogger.Info($"[DownloadExecuter.DownloadEpisode] Starting episode download: {url}");

        try
        {
            List<string> args = GetDownloaderArgs(url, language, provider, "episode");
            var (success, output, error) = ExecuteDownloader(args);

            if (!success)
            {
                FileLogger.Error($"[DownloadExecuter.DownloadEpisode] Episode download failed. Error: {error}");
            }
            else
            {
                FileLogger.Info($"[DownloadExecuter.DownloadEpisode] Episode download completed successfully");
                OrganizeDownloadedFiles(url, false);
            }

            return success;
        }
        catch (Exception e)
        {
            FileLogger.Error($"[DownloadExecuter.DownloadEpisode] Exception: {e.Message}");
            FileLogger.Error($"[DownloadExecuter.DownloadEpisode] Stack trace: {e.StackTrace}");
            return false;
        }
    }

    /// <summary>
    /// Extracts the anime slug from an AniWorld URL.
    /// </summary>
    private static string ExtractAnimeSlugFromUrl(string url)
    {
        var m = Regex.Match(url, @"/anime/stream/([^/]+)", RegexOptions.IgnoreCase);
        if (!m.Success) throw new ArgumentException("Anime slug not found in URL.", nameof(url));
        return m.Groups[1].Value;
    }

    /// <summary>
    /// Converts a slug like "one-piece" to a display name like "One Piece".
    /// </summary>
    private static string ToPrettyAnimeName(string slug)
    {
        var decoded = HttpUtility.UrlDecode(slug)?.Trim() ?? string.Empty;
        var spaced = Regex.Replace(decoded.Replace('-', ' '), @"\s+", " ").Trim();
        if (spaced.Length == 0) return "Unknown";
        return string.Join(" ", spaced.Split(' ')
            .Where(w => w.Length > 0)
            .Select(w => char.ToUpper(w[0]) + (w.Length > 1 ? w[1..] : string.Empty)));
    }

    /// <summary>
    /// Extracts season number from URL if present. Returns null if not found.
    /// </summary>
    private static int? ExtractSeasonFromUrl(string url)
    {
        var m = Regex.Match(url, @"/staffel-(\d+)", RegexOptions.IgnoreCase);
        return m.Success ? int.Parse(m.Groups[1].Value) : (int?)null;
    }

    /// <summary>
    /// Tries to parse SxxExxx pattern from a filename (case-insensitive).
    /// </summary>
    private static (int? season, int? episode) TryParseSeasonEpisodeFromName(string filename)
    {
        var m = Regex.Match(filename, @"S(\d{1,2})E(\d{1,3})", RegexOptions.IgnoreCase);
        if (!m.Success) return (null, null);
        return (int.Parse(m.Groups[1].Value), int.Parse(m.Groups[2].Value));
    }

    /// <summary>
    /// Builds "<output>/<Anime>/Season X" and ensures the directory exists.
    /// </summary>
    private string EnsureSeasonDirectory(string animeName, int season)
    {
        var seasonDir = Path.Combine(_outputDirectory, animeName, $"Season {season}");
        Directory.CreateDirectory(seasonDir);
        return seasonDir;
    }

    /// <summary>
    /// Organizes downloaded files into proper Jellyfin structure.
    /// AniWorld CLI creates files in format: "Anime Name - SxxExx - (Language).mp4"
    /// We organize them into: "Anime Name/Season X/Anime Name - SxxExx - (Language).mp4"
    /// </summary>
    private void OrganizeDownloadedFiles(string sourceUrl, bool hasSpecials)
    {
        try
        {
            var animeName = ToPrettyAnimeName(ExtractAnimeSlugFromUrl(sourceUrl));
            FileLogger.Info($"[Organizer] Starting organization for: {animeName}");

            // Search recursively for all video files
            var videoExtensions = new[] { ".mp4", ".mkv", ".avi", ".webm" };
            var allFiles = Directory.EnumerateFiles(_outputDirectory, "*.*", SearchOption.AllDirectories)
                .Where(f => videoExtensions.Contains(Path.GetExtension(f).ToLower()))
                .ToList();

            FileLogger.Info($"[Organizer] Found {allFiles.Count} video files to organize");

            if (!allFiles.Any())
            {
                FileLogger.Debug("[Organizer] No files to organize.");
                return;
            }

            foreach (var srcPath in allFiles)
            {
                var fileName = Path.GetFileName(srcPath) ?? "";
                var nameNoExt = Path.GetFileNameWithoutExtension(srcPath) ?? "";
                var currentDir = Path.GetDirectoryName(srcPath) ?? "";

                FileLogger.Debug($"[Organizer] Processing: {fileName}");

                // Check if file belongs to our anime (contains anime name)
                if (!fileName.Contains(animeName, StringComparison.OrdinalIgnoreCase))
                {
                    FileLogger.Debug($"[Organizer] Skipping file (different anime): {fileName}");
                    continue;
                }

                // Check if this is a special/movie
                bool isSpecial = nameNoExt.Contains("Film", StringComparison.OrdinalIgnoreCase) ||
                                nameNoExt.Contains("Movie", StringComparison.OrdinalIgnoreCase) ||
                                nameNoExt.Contains("Special", StringComparison.OrdinalIgnoreCase) ||
                                nameNoExt.Contains("OVA", StringComparison.OrdinalIgnoreCase);

                string targetDir;
                
                if (isSpecial && hasSpecials)
                {
                    // Move specials to "Specials" folder
                    targetDir = Path.Combine(_outputDirectory, animeName, "Specials");
                    Directory.CreateDirectory(targetDir);
                    FileLogger.Info($"[Organizer] File identified as SPECIAL: {fileName}");
                }
                else
                {
                    // Parse season from filename (SxxExx pattern)
                    var (seasonFromName, _) = TryParseSeasonEpisodeFromName(nameNoExt);
                    var season = seasonFromName ?? 1;
                    targetDir = EnsureSeasonDirectory(animeName, season);
                    FileLogger.Debug($"[Organizer] File identified as Season {season} episode: {fileName}");
                }

                var dstPath = Path.Combine(targetDir, fileName);

                // Check if already in correct location
                if (string.Equals(Path.GetDirectoryName(srcPath), targetDir, StringComparison.OrdinalIgnoreCase))
                {
                    FileLogger.Debug($"[Organizer] File already in correct location: {fileName}");
                    continue;
                }

                // Handle name collisions
                if (File.Exists(dstPath))
                {
                    FileLogger.Warning($"[Organizer] File already exists at destination: {dstPath}");
                    var baseName = Path.GetFileNameWithoutExtension(fileName) ?? "file";
                    var ext = Path.GetExtension(fileName);
                    dstPath = Path.Combine(targetDir, $"{baseName}_{DateTime.UtcNow:yyyyMMddHHmmss}{ext}");
                }

                // Move file
                File.Move(srcPath, dstPath);
                FileLogger.Info($"[Organizer] Moved: {fileName} -> {Path.GetDirectoryName(dstPath)}");
            }

            // Cleanup: Remove empty directories
            CleanupEmptyDirectories(_outputDirectory);
            
            FileLogger.Info($"[Organizer] Organization complete for: {animeName}");
        }
        catch (Exception ex)
        {
            FileLogger.Error($"[Organizer] Exception: {ex.Message}");
            FileLogger.Error($"[Organizer] Stack trace: {ex.StackTrace}");
        }
    }

    /// <summary>
    /// Removes empty directories recursively.
    /// </summary>
    private void CleanupEmptyDirectories(string path)
    {
        try
        {
            foreach (var directory in Directory.GetDirectories(path))
            {
                CleanupEmptyDirectories(directory);
                
                if (!Directory.EnumerateFileSystemEntries(directory).Any())
                {
                    Directory.Delete(directory, false);
                    FileLogger.Debug($"[Organizer] Removed empty directory: {directory}");
                }
            }
        }
        catch (Exception ex)
        {
            FileLogger.Warning($"[Organizer] Failed to cleanup directory: {ex.Message}");
        }
    }
}