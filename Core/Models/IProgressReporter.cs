namespace Tavstal.KonkordLauncher.Core.Models;

public interface IProgressReporter
{
    void SetProgress(double progress);
    
    void SetStatus(string status);
    
    void SetStatusTranslated(string statusKey, params object[]? args);

    void Show();
    
    void Hide();
}