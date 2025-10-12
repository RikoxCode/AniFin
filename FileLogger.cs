using System;
using System.IO;

namespace Jellyfin.Plugin.AniFin;

public static class FileLogger
{
    private static string? _logPath;
    private static readonly object _lock = new object();

    public static void Initialize()
    {
        try
        {
            var pluginPath = Plugin.Instance?.GetType().Assembly.Location;
            if (!string.IsNullOrEmpty(pluginPath))
            {
                var pluginDir = Path.GetDirectoryName(pluginPath);
                if (!string.IsNullOrEmpty(pluginDir))
                {
                    var logsDir = Path.Combine(pluginDir, "logs");
                    Directory.CreateDirectory(logsDir);
                    _logPath = Path.Combine(logsDir, $"anifin-{DateTime.Now:yyyy-MM-dd}.log");
                    Log("INFO", "FileLogger initialized");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to initialize FileLogger: {ex.Message}");
        }
    }

    public static void Log(string level, string message)
    {
        if (string.IsNullOrEmpty(_logPath)) Initialize();
        if (string.IsNullOrEmpty(_logPath)) return;

        try
        {
            lock (_lock)
            {
                var logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{level}] {message}{Environment.NewLine}";
                File.AppendAllText(_logPath, logEntry);
            }
        }
        catch
        {
            // Ignore logging errors
        }
    }

    public static void Info(string message) => Log("INFO", message);
    public static void Error(string message) => Log("ERROR", message);
    public static void Debug(string message) => Log("DEBUG", message);
    public static void Warning(string message) => Log("WARN", message);
}