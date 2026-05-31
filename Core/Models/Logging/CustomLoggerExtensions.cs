using Microsoft.Extensions.Logging;

namespace Tavstal.KonkordLauncher.Core.Models.Logging;

/// <summary>
/// Provides extension helpers for the custom logging system used by the launcher.
/// </summary>
public static class CustomLoggerExtensions
{
    /// <summary>
    /// Formats a <see cref="LogEntry"/> and an optional <see cref="Exception"/> into a single string
    /// suitable for output to the configured log sinks.
    /// </summary>
    private static readonly Func<LogEntry, Exception?, string> _messageFormatter = (state, exception) =>
    {
        var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

        var sb = new System.Text.StringBuilder();
        sb.Append($"[{timestamp}] [{state.LogLevel}] [{state.ModuleName}] {state.Message}");

        if (exception != null)
        {
            sb.AppendLine();
            sb.Append($"└── Exception: {exception.GetType().Name}: {exception.Message}");
            sb.AppendLine();
            sb.Append(exception.StackTrace);
        }

        return sb.ToString();
    };

    /// <param name="logger">The <see cref="ICustomLogger"/> to write to.</param>
    extension(ICustomLogger logger)
    {
        /// <summary>
        /// Formats and writes a debug log message.
        /// </summary>
        /// <param name="exception">The exception to log.</param>
        /// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c>.</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        /// <example>
        /// <code language="csharp">
        /// logger.LogDebug(exception, "Error while processing request from {Address}", address)
        /// </code>
        /// </example>
        public void LogDebug(Exception? exception, string? message, params object?[] args)
        {
            logger.Log(LogLevel.Debug, exception, message, args);
        }

        /// <summary>
        /// Formats and writes a debug log message.
        /// </summary>
        /// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c>.</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        /// <example>
        /// <code language="csharp">
        /// logger.LogDebug("Processing request from {Address}", address)
        /// </code>
        /// </example>
        public void LogDebug(string? message, params object?[] args)
        {
            logger.Log(LogLevel.Debug, message, args);
        }

        /// <summary>
        /// Formats and writes a trace log message.
        /// </summary>
        /// <param name="exception">The exception to log.</param>
        /// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c>.</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        /// <example>
        /// <code language="csharp">
        /// logger.LogTrace(exception, "Error while processing request from {Address}", address)
        /// </code>
        /// </example>
        public void LogTrace(Exception? exception, string? message, params object?[] args)
        {
            logger.Log(LogLevel.Trace, exception, message, args);
        }

        /// <summary>
        /// Formats and writes a trace log message.
        /// </summary>
        /// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c>.</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        /// <example>
        /// <code language="csharp">
        /// logger.LogTrace("Processing request from {Address}", address)
        /// </code>
        /// </example>
        public void LogTrace(string? message, params object?[] args)
        {
            logger.Log(LogLevel.Trace, message, args);
        }

        /// <summary>
        /// Formats and writes an informational log message.
        /// </summary>
        /// <param name="exception">The exception to log.</param>
        /// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c>.</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        /// <example>
        /// <code language="csharp">
        /// logger.LogInformation(exception, "Error while processing request from {Address}", address)
        /// </code>
        /// </example>
        public void LogInformation(Exception? exception, string? message,
            params object?[] args)
        {
            logger.Log(LogLevel.Information, exception, message, args);
        }

        /// <summary>
        /// Formats and writes an informational log message.
        /// </summary>
        /// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c>.</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        /// <example>
        /// <code language="csharp">
        /// logger.LogInformation("Processing request from {Address}", address)
        /// </code>
        /// </example>
        public void LogInformation(string? message, params object?[] args)
        {
            logger.Log(LogLevel.Information, message, args);
        }

        /// <summary>
        /// Formats and writes a warning log message.
        /// </summary>
        /// <param name="exception">The exception to log.</param>
        /// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c>.</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        /// <example>
        /// <code language="csharp">
        /// logger.LogWarning(exception, "Error while processing request from {Address}", address)
        /// </code>
        /// </example>
        public void LogWarning(Exception? exception, string? message,
            params object?[] args)
        {
            logger.Log(LogLevel.Warning, exception, message, args);
        }

        /// <summary>
        /// Formats and writes a warning log message.
        /// </summary>
        /// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c>.</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        /// <example>
        /// <code language="csharp">
        /// logger.LogWarning("Processing request from {Address}", address)
        /// </code>
        /// </example>
        public void LogWarning(string? message, params object?[] args)
        {
            logger.Log(LogLevel.Warning, message, args);
        }

        /// <summary>
        /// Formats and writes an error log message.
        /// </summary>
        /// <param name="exception">The exception to log.</param>
        /// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c>.</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        /// <example>
        /// <code language="csharp">
        /// logger.LogError(exception, "Error while processing request from {Address}", address)
        /// </code>
        /// </example>
        public void LogError(Exception? exception, string? message, params object?[] args)
        {
            logger.Log(LogLevel.Error, exception, message, args);
        }

        /// <summary>
        /// Formats and writes an error log message.
        /// </summary>
        /// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c>.</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        /// <example>
        /// <code language="csharp">
        /// logger.LogError("Processing request from {Address}", address)
        /// </code>
        /// </example>
        public void LogError(string? message, params object?[] args)
        {
            logger.Log(LogLevel.Error, message, args);
        }

        /// <summary>
        /// Formats and writes a critical log message.
        /// </summary>
        /// <param name="exception">The exception to log.</param>
        /// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c>.</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        /// <example>
        /// <code language="csharp">
        /// logger.LogCritical(exception, "Error while processing request from {Address}", address)
        /// </code>
        /// </example>
        public void LogCritical(Exception? exception, string? message,
            params object?[] args)
        {
            logger.Log(LogLevel.Critical, exception, message, args);
        }

        /// <summary>
        /// Formats and writes a critical log message.
        /// </summary>
        /// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c>.</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        /// <example>
        /// <code language="csharp">
        /// logger.LogCritical(ex, "Processing request from {Address}", address)
        /// </code>
        /// </example>
        public void LogCritical(string? message, params object?[] args)
        {
            logger.Log(LogLevel.Critical, message, args);
        }

        /// <summary>
        /// Formats and writes a log message at the specified log level.
        /// </summary>
        /// <param name="logLevel">Entry will be written on this level.</param>
        /// <param name="message">Format string of the log message.</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        public void Log(LogLevel logLevel, string? message, params object?[] args)
        {
            logger.Log(logLevel, null, message, args);
        }

        /// <summary>
        /// Formats and writes a log message at the specified log level.
        /// </summary>
        /// <param name="logLevel">Entry will be written on this level.</param>
        /// <param name="exception">The exception to log.</param>
        /// <param name="message">Format string of the log message.</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        public void Log(LogLevel logLevel, Exception? exception,
            string? message, params object?[] args)
        {
            ArgumentNullException.ThrowIfNull(logger);

            string? formattedMessage = message != null ? string.Format(message, args) : null;
            logger.Log(logLevel,
                new LogEntry(logLevel.ToString("G").ToUpper(), logger.GetModuleName(), formattedMessage), exception,
                _messageFormatter);
        }
    }
}