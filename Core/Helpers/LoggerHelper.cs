using System.Collections.Concurrent;

namespace Tavstal.KonkordLauncher.Core.Helpers;

/// <summary>
/// Provides a static, thread-safe log buffering and file writing pipeline.
/// </summary>
public static class LoggerHelper
{
    private static readonly ConcurrentDictionary<string, ConcurrentQueue<string>> _queues = new();
    private static readonly ConcurrentDictionary<string, SemaphoreSlim> _fileLocks = new();
    private static readonly SemaphoreSlim _signal = new(0);
    public static string DefaultLogFilePath { get; set; } = string.Empty;
    public static DateTime StartTime { get; } = DateTime.Now;
    
    /// <summary>
    /// Enqueues a log entry for asynchronous file writing.
    /// </summary>
    /// <param name="entry">The raw log message (without timestamp) to enqueue.</param>
    /// <param name="logFilePath">Optional target file path. If <see langword="null"/>, <see cref="DefaultLogFilePath"/> is used.</param>
    public static void EnqueueLog(string entry, string? logFilePath = null)
    {
        if (string.IsNullOrEmpty(logFilePath))
            return;
        
        string logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] {entry}";
        var queue = _queues.GetOrAdd(logFilePath ?? DefaultLogFilePath, _ => new ConcurrentQueue<string>());
        queue.Enqueue(logEntry);
        _signal.Release();
    }
    
    /// <summary>
    /// Continuously processes queued log entries and appends them to their target files.
    /// </summary>
    /// <param name="token">Cancellation token used to stop the processing loop.</param>
    /// <returns>A task representing the background writer loop.</returns>
    public static async Task ProcessLogQueueAsync(CancellationToken token = default)
    {
        while (!token.IsCancellationRequested)
        {
            await _signal.WaitAsync(token);

            foreach (var (path, queue) in _queues)
            {
                if (!queue.TryDequeue(out var line))
                    continue;

                var fileLock = _fileLocks.GetOrAdd(path, _ => new SemaphoreSlim(1, 1));
                await fileLock.WaitAsync(token);
                try
                {
                    await File.AppendAllTextAsync(path, line + Environment.NewLine, token);
                }
                finally
                {
                    fileLock.Release();
                }
            }
        }
    }
}