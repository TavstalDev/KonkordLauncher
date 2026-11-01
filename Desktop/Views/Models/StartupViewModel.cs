using System.Reactive;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Tavstal.KonkordLauncher.Desktop.Models.Avalonia;

namespace Tavstal.KonkordLauncher.Desktop.Views.Models;

/// <summary>
///     The view model for the startup window that is shown while the launcher starts.
/// </summary>
public partial class StartupViewModel : KonkordObservableObject
{
    /// <summary>
    ///     Gets or sets the progress value.
    /// </summary>
    [ObservableProperty]
    public partial double Progress { get; set; }

    /// <summary>
    ///     Gets or sets the text that describes the current startup step.
    /// </summary>
    [ObservableProperty]
    public partial string ProgressText { get; set; } = "Starting...";

    /// <summary>
    ///     Gets the interaction that asks the window to close.
    /// </summary>
    public Interaction<Unit, Unit> CloseWindowInteraction { get; } = new();

    [RelayCommand]
    private async Task CloseWindow()
    {
        await CloseWindowInteraction.Handle(Unit.Default);
    }
}
