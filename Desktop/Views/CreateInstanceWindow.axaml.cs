using System.Reactive;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using ReactiveUI;
using Tavstal.KonkordLauncher.Desktop.Models.Avalonia;
using Tavstal.KonkordLauncher.Desktop.Views.Dialogs;
using Tavstal.KonkordLauncher.Desktop.Views.Models;

namespace Tavstal.KonkordLauncher.Desktop.Views;

/// <summary>
/// Represents the window for creating a new instance in the Konkord Launcher.
/// </summary>
public partial class CreateInstanceWindow : KonkordWindow<CreateInstanceViewModel>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CreateInstanceWindow"/> class.
    /// Sets up the data context, initializes components, and registers reactive handlers.
    /// </summary>
    public CreateInstanceWindow()
    {
        InitializeComponent();

#if DEBUG
        // Attaches Avalonia Dev Tools for debugging purposes.
        this.AttachDevTools();
#endif

        // Sets the data context to a new instance of CreateInstanceViewModel.
        DataContext = new CreateInstanceViewModel();

        // Registers reactive handlers for various interactions when the window is activated.
        this.WhenActivated(disposables =>
        {
            // Registers a handler to close the window when the CloseWindow interaction is triggered.
            DataContext.CloseWindow.RegisterHandler(action =>
            {
                Close();
                action.SetOutput(Unit.Default);
                return Task.CompletedTask;
            }).DisposeWith(disposables);

            // Registers a handler to show an alert dialog when the ShowAlertDialog interaction is triggered.
            DataContext.ShowAlertDialog.RegisterHandler(async action =>
            {
                AlertWindow alertWindow = new(action.Input.Title, action.Input.Message, action.Input.Type);
                await alertWindow.ShowDialog(this);
                action.SetOutput(Unit.Default);
            }).DisposeWith(disposables);

            // Registers a handler to show the icon selector dialog when the ShowIconSelector interaction is triggered.
            DataContext.ShowIconSelector.RegisterHandler(async action =>
            {
                IconSelectorWindow window = new();
                var result = await window.ShowDialog<string?>(this);
                action.SetOutput(result);
            }).DisposeWith(disposables);
        });
    }
}