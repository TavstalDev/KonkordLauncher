using Microsoft.Extensions.Logging;

namespace Tavstal.KonkordLauncher.Core.Models.Logging;

public interface ICustomLogger
{
    void Log(
        LogLevel logLevel,
        LogEntry entry,
        Exception? exception,
        Func<LogEntry, Exception?, string> formatter);
    
    string GetModuleName();
}

public interface ICustomLogger<T> : ICustomLogger where T : class { }