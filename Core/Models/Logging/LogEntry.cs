namespace Tavstal.KonkordLauncher.Core.Models.Logging;

/// <summary>
/// Represents a single log entry produced by the launcher or its subsystems.
/// </summary>
/// <param name="LogLevel">The severity level of the log (e.g. "Debug", "Info", "Warn", "Error").</param>
/// <param name="ModuleName">The name of the module or component that emitted the log.</param>
/// <param name="Message">An optional message describing the log event; may be null for structured logs.</param>
public record LogEntry(
    string LogLevel,
    string ModuleName,
    string? Message
);