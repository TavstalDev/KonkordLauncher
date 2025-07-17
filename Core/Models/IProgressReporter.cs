namespace Tavstal.KonkordLauncher.Core.Models;

public interface IProgressReporter
{
    void SetProgress(double progress);
    
    void SetStatus(string status);

    void Show();
    
    void Hide();
}