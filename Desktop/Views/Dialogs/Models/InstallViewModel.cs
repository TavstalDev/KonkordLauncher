using CommunityToolkit.Mvvm.ComponentModel;
using Tavstal.KonkordLauncher.Desktop.Models;

namespace Tavstal.KonkordLauncher.Desktop.Views.Dialogs.Models;

/// <summary>
/// Represents the view model for the installation process, providing properties
/// to track progress and display status messages.
/// </summary>
public partial class InstallViewModel : KonkordObservableObject
{
    /// <summary>
    /// Gets or sets the progress text displayed during the installation process.
    /// Default value is "Initializing...".
    /// </summary>
    [ObservableProperty] 
    private string _progressText = "Initializing...";

    /// <summary>
    /// Gets or sets the progress value of the installation process.
    /// Default value is 0.0, representing no progress.
    /// </summary>
    [ObservableProperty] 
    private double _progressValue = 0.0;

    /// <summary>
    /// Releases resources associated with the installation process by resetting
    /// the progress text and progress value to their default states.
    /// </summary>
    public override void FreeMemory()
    {
        ProgressText = string.Empty;
        ProgressValue = 0;
    }
}