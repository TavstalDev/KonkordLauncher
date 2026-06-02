using Microsoft.Extensions.Logging;
using Tavstal.KonkordLauncher.Core.Helpers;

namespace Tavstal.KonkordLauncher.Core.Models.Logging;

/// <inheritdoc/>
public class CustomLogger : ICustomLogger 
{
    private readonly Lock _logLock = new();
    private readonly LogLevel _logLevel;
    private readonly bool _printLogs;
    private readonly string _moduleName;
    private readonly string? _customLogFilePath;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="CustomLogger"/> class.
    /// </summary>
    /// <param name="moduleName">Name of the module or component producing logs (used in output).</param>
    /// <param name="logLevel">Minimum <see cref="LogLevel"/> that will be emitted by this logger.</param>
    /// <param name="printLogs">If true, log messages are also printed to the console.</param>
    /// <param name="customLogFilePath">Optional path to a custom log file (passed to <see cref="LoggerHelper"/>).</param>
    public CustomLogger(string moduleName, LogLevel logLevel, bool printLogs = true, string? customLogFilePath = null)
    {
        _moduleName = moduleName;
        _logLevel = logLevel;
        _printLogs = printLogs;
        _customLogFilePath = customLogFilePath;
    }
    
    /// <inheritdoc/>
    public void Log(LogLevel logLevel, LogEntry entry, Exception? exception, Func<LogEntry, Exception?, string> formatter)
    {
        if (_logLevel > logLevel)
            return;
        
        string text = formatter(entry, exception);
        if (_printLogs)
        {
            lock (_logLock)
            {
                try
                {
                    Console.ForegroundColor = GetLogLevelColor(logLevel);
                    Console.WriteLine(text);
                    Console.ResetColor();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[Logger Console Error] {ex.Message}");
                }
            }
        }
        
        LoggerHelper.EnqueueLog(text, _customLogFilePath);
    }

    /// <inheritdoc/>
    public string GetModuleName() => _moduleName;
    
    /// <inheritdoc/>
    public LogLevel GetLogLevel() => _logLevel;

    /// <summary>
    /// Maps a <see cref="LogLevel"/> to a <see cref="ConsoleColor"/> for console output.
    /// </summary>
    /// <param name="logLevel">The log level to map.</param>
    /// <returns>A <see cref="ConsoleColor"/> suitable for the given level.</returns>
    private static ConsoleColor GetLogLevelColor(LogLevel logLevel)
    {
        return logLevel switch
        {
            LogLevel.Trace => ConsoleColor.Gray,
            LogLevel.Debug => ConsoleColor.Magenta,
            LogLevel.Information => ConsoleColor.Cyan,
            LogLevel.Warning => ConsoleColor.Yellow,
            LogLevel.Error => ConsoleColor.Red,
            LogLevel.Critical => ConsoleColor.DarkRed,
            _ => ConsoleColor.White
        };
    }
}

/// <inheritdoc cref="ICustomLogger" />
public class CustomLogger<T> : CustomLogger, ICustomLogger<T> where T : class
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CustomLogger{T}"/> class with default settings.
    /// Used by dependency injection to instantiate loggers without explicit parameter passing.
    /// </summary>
#if DEBUG
    public CustomLogger() 
        : this(LogLevel.Debug, true, null) { }
#else 
    public CustomLogger() 
        : this(LogLevel.Information, true, null) { }
#endif

    /// <summary>
    /// Initializes a new instance of the <see cref="CustomLogger{T}"/> class.
    /// </summary>
    /// <param name="logLevel">Minimum level to log.</param>
    /// <param name="printLogs">Whether messages are printed to the console.</param>
    /// <param name="customLogFilePath">Optional custom file path for persisted logs.</param>
    public CustomLogger(LogLevel logLevel, bool printLogs = true, string? customLogFilePath = null) 
        : base(typeof(T).Name, logLevel, printLogs, customLogFilePath) { }
}