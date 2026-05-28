namespace Tavstal.KonkordLauncher.Core.Models.Logging;

public record LogEntry(
    string LogLevel,
    string ModuleName,
    string? Message
);