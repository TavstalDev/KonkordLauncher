using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ReactiveUI;
using Tavstal.KonkordLauncher.Desktop.Models.Avalonia;

namespace Tavstal.KonkordLauncher.Desktop.Views.Models;

public partial class StartupViewModel : KonkordObservableObject
{
    /// <summary>
    /// The progress value, represented as a double.
    /// </summary>
    [ObservableProperty]
    public partial double Progress { get; set; }

    /// <summary>
    /// The progress text, initialized with a default value of "Starting...".
    /// </summary>
    [ObservableProperty]
    public partial string ProgressText { get; set; } = "Starting...";
    public Interaction<Unit, Unit> MinimizeWindowInteraction { get; } = new();
    public Interaction<Unit, Unit> MaximizeWindowInteraction { get; } = new();
    public Interaction<Unit, Unit> CloseWindowInteraction { get; } = new();
    
    #region Window Commands
    [RelayCommand]
    public async Task MinimizeWindow() => await MinimizeWindowInteraction.Handle(Unit.Default);

    [RelayCommand]
    public async Task MaximizeWindow() =>  await MaximizeWindowInteraction.Handle(Unit.Default);

    [RelayCommand]
    public async Task CloseWindow() => await CloseWindowInteraction.Handle(Unit.Default);
    #endregion
}