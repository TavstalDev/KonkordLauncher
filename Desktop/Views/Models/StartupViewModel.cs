using ReactiveUI;
using Tavstal.KonkordLauncher.Desktop.Models;

namespace Tavstal.KonkordLauncher.Desktop.Views.Models;

/// <summary>
/// Represents the ViewModel for the startup window, managing progress and status text.
/// </summary>
public class StartupViewModel : ViewModelBase
{
    /// <summary>
    /// The progress value, typically between 0.0 and 1.0.
    /// </summary>
    private double _progress = 0;

    /// <summary>
    /// Gets or sets the progress value.
    /// </summary>
    public double Progress
    {
        get => _progress;
        set => this.RaiseAndSetIfChanged(ref _progress, value);
    }

    /// <summary>
    /// The text displayed to indicate the current progress status.
    /// </summary>
    private string _progressText = "Starting...";

    /// <summary>
    /// Gets or sets the progress status text.
    /// </summary>
    public string ProgressText
    {
        get => _progressText;
        set => this.RaiseAndSetIfChanged(ref _progressText, value);
    }
}