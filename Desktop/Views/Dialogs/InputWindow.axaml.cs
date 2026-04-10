using System.Reactive;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using ReactiveUI;
using Tavstal.KonkordLauncher.Desktop.Models.Avalonia;
using Tavstal.KonkordLauncher.Desktop.Views.Dialogs.Models;

namespace Tavstal.KonkordLauncher.Desktop.Views.Dialogs;

/// <summary>
/// Represents a window for user input in the application.
/// Provides constructors for design-time and runtime initialization.
/// </summary>
public partial class InputWindow : KonkordWindow<InputViewModel>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="InputWindow"/> class for design-time use.
    /// Sets the DataContext to a design-time instance of <see cref="InputViewModel"/>.
    /// </summary>
    public InputWindow()
    {
        InitializeComponent();

        if (Design.IsDesignMode)
            DataContext = new InputViewModel("Design Time Title");
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="InputWindow"/> class with the specified title.
    /// Sets up the DataContext and handles window activation events.
    /// </summary>
    /// <param name="title">The title to be displayed in the input window.</param>
    public InputWindow(string title)
    {
        InitializeComponent();

#if DEBUG
        // Attaches Avalonia Dev Tools for debugging purposes.
        this.AttachDevTools();
#endif

        if (Design.IsDesignMode)
        {
            DataContext = new InputViewModel("Design Time Title");
            return;
        }

        // Sets the DataContext to an instance of InputViewModel with the provided title.
        DataContext = new InputViewModel(title);
        this.WhenActivated(disposables =>
        {
            DataContext.MinimizeWindowInteraction.RegisterHandler(action =>
            {
                WindowState = WindowState.Minimized;
                action.SetOutput(Unit.Default);
                return Task.CompletedTask;
            }).DisposeWith(disposables);
            DataContext.MaximizeWindowInteraction.RegisterHandler(action =>
            {
                WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
                action.SetOutput(Unit.Default);
                return Task.CompletedTask;
            }).DisposeWith(disposables);
            DataContext.CloseWindowInteraction.RegisterHandler(action =>
            {
                Close(action.Input);
                action.SetOutput(Unit.Default);
                return Task.CompletedTask;
            }).DisposeWith(disposables);
        });
    }
}