using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ReactiveUI;
using Tavstal.KonkordLauncher.Desktop.Models.Enums;

namespace Tavstal.KonkordLauncher.Desktop.Views.Dialogs.Models;

/// <summary>
/// Represents the view model for an input dialog in the application.
/// Provides properties and commands for handling user input and dialog interactions.
/// </summary>
public partial class InputViewModel : ObservableObject
{
    /// <summary>
    /// Interaction used to signal the closing of the input dialog window.
    /// </summary>
    public Interaction<string?, Unit> CloseWindow { get; }  = new();

    /// <summary>
    /// The title of the input dialog.
    /// </summary>
    [ObservableProperty] private string _title;

    /// <summary>
    /// The text input provided by the user in the dialog.
    /// Notifies changes to <see cref="CanClickOnOk"/>.
    /// </summary>
    [ObservableProperty] [NotifyPropertyChangedFor(nameof(CanClickOnOk))] private string _inputText = string.Empty;

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
        _title = title;
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

        await CloseWindow.Handle(InputText);
    }

    /// <summary>
    /// Command executed when the "Cancel" button is clicked.
    /// Closes the dialog without passing any input.
    /// </summary>
    [RelayCommand]
    public async Task Cancel() => await CloseWindow.Handle(null);
}