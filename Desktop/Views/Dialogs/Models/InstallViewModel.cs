using CommunityToolkit.Mvvm.ComponentModel;

namespace Tavstal.KonkordLauncher.Desktop.Views.Dialogs.Models;

/// <summary>
/// Represents the view model for the installation process, providing properties
/// to track progress and display status messages.
/// </summary>
public partial class InstallViewModel : ObservableObject
{
    /// <summary>
    /// Gets or sets the progress text displayed during the installation process.
    /// Default value is "Initializing...".
    /// </summary>
    [ObservableProperty] 
    private string _progressText = "Initializing...";

    /// <summary>
    /// Gets or sets the progress value of the installation process.
    /// Default value is 0, representing no progress.
    /// </summary>
    [ObservableProperty] 
    private double _progressValue;
}