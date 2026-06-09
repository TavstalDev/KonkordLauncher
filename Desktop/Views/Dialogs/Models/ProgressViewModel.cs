using System.Reactive;
using System.Reactive.Linq;
using System.Threading;
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
    private readonly CancellationTokenSource? _cancellationTokenSource;
    
    public ProgressViewModel(CancellationTokenSource? cancellationTokenSource = null)
    {
        _cancellationTokenSource = cancellationTokenSource;
        IsCancellable = cancellationTokenSource != null;
    }
    
    /// <summary>
    /// Gets or sets a value indicating whether the installation process can be cancelled.
    /// </summary>
    [ObservableProperty]
    public partial bool IsCancellable { get; set; }
    
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
    public Interaction<bool, Unit> CloseWindowInteraction { get; } = new();

    /// <summary>
    /// Closes the window by invoking the close window interaction.
    /// </summary>
    [RelayCommand]
    public async Task CloseWindow()
    {
        if (_cancellationTokenSource != null)
            await _cancellationTokenSource.CancelAsync();
        await CloseWindowInteraction.Handle(_cancellationTokenSource != null);
    }
}