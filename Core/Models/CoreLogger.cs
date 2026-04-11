using Tavstal.KonkordLauncher.Core.Helpers.IO;

namespace Tavstal.KonkordLauncher.Core.Models;

/// <summary>
/// Provides logging functionality for different modules in the application.
/// </summary>
public class CoreLogger
{
    /// <summary>
    /// The name of the module associated with the logger.
    /// </summary>
    private readonly string _moduleName;
    private static readonly Lock _logLock = new();
    private static readonly Queue<string> _logQueue = new();
    private static readonly SemaphoreSlim _signal = new(0);
    private static readonly CancellationTokenSource _logCts = new();
    public static DateTime StartTime { get; } = DateTime.Now;

    /// <summary>
    /// Initializes a new instance of the <see cref="CoreLogger"/> class with the specified module name.
    /// </summary>
    /// <param name="moduleName">The name of the module to associate with the logger.</param>
    public CoreLogger(string moduleName)
    {
        _moduleName = moduleName;
        Task.Run(() => ProcessLogQueueAsync(_logCts.Token));
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CoreLogger"/> class with the specified module type.
    /// </summary>
    /// <param name="moduleType">The type of the module to associate with the logger.</param>
    public CoreLogger(Type moduleType) : this(moduleType.Name) { }

    /// <summary>
    /// Creates a new <see cref="CoreLogger"/> instance with the specified module name.
    /// </summary>
    /// <param name="moduleName">The name of the module to associate with the logger.</param>
    /// <returns>A new instance of <see cref="CoreLogger"/>.</returns>
    public static CoreLogger WithModuleName(string moduleName)
    {
        return new CoreLogger(moduleName);
    }

    /// <summary>
    /// Creates a new <see cref="CoreLogger"/> instance with the specified module type.
    /// </summary>
    /// <param name="moduleType">The type of the module to associate with the logger.</param>
    /// <returns>A new instance of <see cref="CoreLogger"/>.</returns>
    public static CoreLogger WithModuleType(Type moduleType)
    {
        return new CoreLogger(moduleType.Name);
    }

    /// <summary>
    /// Continuously processes queued log entries and writes them to the log file.
    /// </summary>
    /// <param name="token">Cancellation token used to stop the processing loop.</param>
    private static async Task ProcessLogQueueAsync(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            await _signal.WaitAsync(token);

            if (_logQueue.TryDequeue(out var logEntry))
            {
                try
                {
                    string logsFilePath = Path.Combine(PathHelper.LauncherLogsDir, string.Format(PathHelper.LogsFileFormat, StartTime));
                    await File.AppendAllLinesAsync(logsFilePath, [logEntry], token);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Logger Error] Logging failed: {ex.Message}");
                    Console.WriteLine($"[Logger Error] Failed log entry: {logEntry}");
                }
            }
        }
    }
    
    /// <summary>
    /// Logs a message to the console with a specified color and optional prefix.
    /// </summary>
    /// <param name="message">The message to log.</param>
    /// <param name="color">The console color for the message (default is white).</param>
    /// <param name="prefix">An optional prefix for the message.</param>
    public void Log(object message, ConsoleColor color = ConsoleColor.White, string prefix = "")
    {
        lock (_logLock)
        {
            string text = $"{prefix}{message}";
            if (!string.IsNullOrEmpty(_moduleName))
                text = $"[{_moduleName}] {text}";

            try
            {
                Console.ForegroundColor = color;
                Console.WriteLine(text);

                _logQueue.Enqueue(string.Concat("[", DateTime.Now.ToString("g"), "] ", text));
                _signal.Release();
            }
            catch (Exception ex)
            {
                // If console output fails, fallback to Debug.WriteLine
                System.Diagnostics.Debug.WriteLine($"[Logger Error] Failed to log: {text}");
                System.Diagnostics.Debug.WriteLine($"[Logger Error] Exception: {ex}");
            }
            finally
            {
                // Ensure the console color is reset
                Console.ResetColor();
            }
        }
    }

    /// <summary>
    /// Logs an informational message to the console.
    /// </summary>
    /// <param name="message">The message to log.</param>
    /// <param name="color">The console color for the message (default is dark cyan).</param>
    public void Info(object message, ConsoleColor color = ConsoleColor.DarkCyan)
    {
        Log(message, color, "[INFO] : ");
    }

    /// <summary>
    /// Logs a success message to the console.
    /// </summary>
    /// <param name="message">The message to log.</param>
    /// <param name="color">The console color for the message (default is green).</param>
    public void Ok(object message, ConsoleColor color = ConsoleColor.Green)
    {
        Log(message, color, "[OK] : ");
    }

    /// <summary>
    /// Logs a warning message to the console.
    /// </summary>
    /// <param name="message">The message to log.</param>
    /// <param name="color">The console color for the message (default is yellow).</param>
    public void Warn(object message, ConsoleColor color = ConsoleColor.Yellow)
    {
        Log(message, color, "[WARNING] : ");
    }

    /// <summary>
    /// Logs an exception message to the console.
    /// </summary>
    /// <param name="message">The exception message to log.</param>
    /// <param name="color">The console color for the message (default is dark yellow).</param>
    public void Exc(object message, ConsoleColor color = ConsoleColor.DarkYellow)
    {
        Log(message, color, "[EXCEPTION] : ");
    }

    /// <summary>
    /// Logs an error message to the console.
    /// </summary>
    /// <param name="message">The error message to log.</param>
    /// <param name="color">The console color for the message (default is red).</param>
    public void Error(object message, ConsoleColor color = ConsoleColor.Red)
    {
        Log(message, color, "[ERROR] : ");
    }

    /// <summary>
    /// Logs a debug message to the console if debug logging is enabled.
    /// </summary>
    /// <param name="message">The debug message to log.</param>
    /// <param name="color">The console color for the message (default is magenta).</param>
    public void Debug(object message, ConsoleColor color = ConsoleColor.Magenta)
    {
#if DEBUG
        Log(message, color, "[DEBUG] : ");
#endif
    }
}