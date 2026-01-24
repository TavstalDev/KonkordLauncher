using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ReactiveUI;

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
    
    public Interaction<Unit, Unit> MinimizeWindowInteraction { get; } = new();
    public Interaction<Unit, Unit> MaximizeWindowInteraction { get; } = new();
    public Interaction<Unit, Unit> CloseWindowInteraction { get; } = new();
    
    #region Window Commands
    [RelayCommand]
    public async Task MinimizeWindow()
    {
        await MinimizeWindowInteraction.Handle(Unit.Default);
    }

    [RelayCommand]
    public async Task MaximizeWindow()
    {
        await MaximizeWindowInteraction.Handle(Unit.Default);
    }

    [RelayCommand]
    public async Task CloseWindow()
    {
        await CloseWindowInteraction.Handle(Unit.Default);
    }
    #endregion
}