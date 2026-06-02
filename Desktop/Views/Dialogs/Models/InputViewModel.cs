using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ReactiveUI;

namespace Tavstal.KonkordLauncher.Desktop.Views.Dialogs.Models;

/// <summary>
/// Represents the view model for an input dialog in the application.
/// Provides properties and commands for handling user input and dialog interactions.
/// </summary>
public partial class InputViewModel : ObservableObject
{
    
    
    public Interaction<string?, Unit> CloseWindowInteraction { get; } = new();

    /// <summary>
    /// The title of the input dialog.
    /// </summary>
    [ObservableProperty]
    public partial string Title { get; set; }

    /// <summary>
    /// The text input provided by the user in the dialog.
    /// Notifies changes to <see cref="CanClickOnOk"/>.
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanClickOnOk))]
    public partial string InputText { get; set; } = string.Empty;

    /// <summary>
    /// Gets a value indicating whether the "OK" button can be clicked.
    /// Returns true if <see cref="InputText"/> is not null or empty.
    /// </summary>
    public bool CanClickOnOk => !string.IsNullOrEmpty(InputText);

    /// <summary>
    /// Initializes a new instance of the <see cref="InputViewModel"/> class with the specified title.
    /// </summary>
    /// <param name="title">The title to be displayed in the input dialog.</param>
    public InputViewModel(string title)
    {
        Title = title;
    }

    /// <summary>
    /// Command executed when the "OK" button is clicked.
    /// Closes the dialog and passes the user input to the handler.
    /// </summary>
    [RelayCommand]
    public async Task Ok()
    {
        if (string.IsNullOrEmpty(InputText))
            return;

        await CloseWindowInteraction.Handle(InputText);
    }
    
    [RelayCommand]
    public async Task CloseWindow() => await CloseWindowInteraction.Handle(null);
}