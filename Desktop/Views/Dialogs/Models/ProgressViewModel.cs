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
public partial class ProgressViewModel : ObservableObject
{
    /// <summary>
    /// Gets or sets the progress text displayed during the installation process.
    /// Default value is "Initializing...".
    /// </summary>
    [ObservableProperty]
    public partial string ProgressText { get; set; } = "Initializing...";

    /// <summary>
    /// Gets or sets the progress value of the installation process.
    /// Default value is 0, representing no progress.
    /// </summary>
    [ObservableProperty]
    public partial double ProgressValue { get; set; }
    
    /// <summary>
    /// An interaction representing the close window action.
    /// </summary>
    public Interaction<Unit, Unit> CloseWindowInteraction { get; } = new();
    
    /// <summary>
    /// Closes the window by invoking the close window interaction.
    /// </summary>
    [RelayCommand]
    public async Task CloseWindow() => await CloseWindowInteraction.Handle(Unit.Default);
}