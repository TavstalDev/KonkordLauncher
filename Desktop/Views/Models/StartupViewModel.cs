using CommunityToolkit.Mvvm.ComponentModel;
using Tavstal.KonkordLauncher.Desktop.Models;

namespace Tavstal.KonkordLauncher.Desktop.Views.Models;

/// <summary>
/// Represents the ViewModel for the startup window, managing progress and status text.
/// Inherits from KonkordObservableObject and provides functionality to reset memory resources.
/// </summary>
public partial class StartupViewModel : KonkordObservableObject
{
    /// <summary>
    /// The progress value, represented as a double.
    /// </summary>
    [ObservableProperty] private double _progress;

    /// <summary>
    /// The progress text, initialized with a default value of "Starting...".
    /// </summary>
    [ObservableProperty] private string _progressText = "Starting...";

    /// <summary>
    /// Overrides the FreeMemory method to reset progress and progress text.
    /// </summary>
    public override void FreeMemory()
    {
        Progress = 0;
        ProgressText = string.Empty;
    }
}