using CommunityToolkit.Mvvm.ComponentModel;

namespace Tavstal.KonkordLauncher.Desktop.Views.Models;

public partial class StartupViewModel : ObservableObject
{
    /// <summary>
    /// The progress value, represented as a double.
    /// </summary>
    [ObservableProperty] private double _progress;

    /// <summary>
    /// The progress text, initialized with a default value of "Starting...".
    /// </summary>
    [ObservableProperty] private string _progressText = "Starting...";
}