using CommunityToolkit.Mvvm.ComponentModel;

namespace Tavstal.KonkordLauncher.DesktopUpdater.Views;

public partial class MainViewModel : ObservableObject
{
    /// <summary>
    /// The progress value, represented as a double.
    /// </summary>
    [ObservableProperty] private double _progress;

    /// <summary>
    /// The progress text, initialized with a default value of "Starting...".
    /// </summary>
    [ObservableProperty] private string _progressText = "...";
}