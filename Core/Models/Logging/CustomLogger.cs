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
    public CustomLogger() 
        : this(LogLevel.Information, true, null) { }

    public CustomLogger(LogLevel logLevel, bool printLogs = true, string? customLogFilePath = null) 
        : base(typeof(T).Name, logLevel, printLogs, customLogFilePath) { }
}