using Microsoft.Extensions.Logging;

namespace Tavstal.KonkordLauncher.Core.Models.Logging;


/// <summary>
/// Provides a custom logging abstraction for modules that emit structured log entries.
/// </summary>
public interface ICustomLogger
{
    /// <summary>
    /// Writes a log entry using the specified log level, entry data, and optional exception.
    /// </summary>
    /// <param name="logLevel">The severity of the log message.</param>
    /// <param name="entry">The log entry payload to write.</param>
    /// <param name="exception">An optional exception associated with the log entry.</param>
    /// <param name="formatter">A function that formats the <paramref name="entry"/> and <paramref name="exception"/> into a string message.</param>
    void Log(
        LogLevel logLevel,
        LogEntry entry,
        Exception? exception,
        Func<LogEntry, Exception?, string> formatter);
    
    /// <summary>
    /// Gets the name of the module or component associated with this logger.
    /// </summary>
    /// <returns>The logger's module name.</returns>
    string GetModuleName();
    
    /// <summary>
    /// Gets the minimum log level supported by this logger.
    /// </summary>
    /// <returns>The current log level threshold.</returns>
    LogLevel GetLogLevel();
}

/// <summary>
/// Represents a strongly typed custom logger for the specified component type.
/// </summary>
/// <typeparam name="T">The component type associated with the logger.</typeparam>
public interface ICustomLogger<T> : ICustomLogger where T : class { }