using Tavstal.BeaconSDK.Models;

namespace Tavstal.KonkordLauncher.Desktop.Models;

public interface IKonkordProgress : IProgressReporter
{
    void Report(string key);

    void Report(double progress, string key, params object[]? args);

    void OpenReporter() { }

    void CloseReporter() { }
}
