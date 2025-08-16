using System.Reactive;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using ReactiveUI;
using Tavstal.KonkordLauncher.Desktop.Models;
using Tavstal.KonkordLauncher.Desktop.Models.Avalonia;
using Tavstal.KonkordLauncher.Desktop.Models.Enums;
using Tavstal.KonkordLauncher.Desktop.Views.Dialogs.Models;

namespace Tavstal.KonkordLauncher.Desktop.Views.Dialogs;

/// <summary>
/// Represents a window for displaying alert dialogs with customizable title, message, and alert type.
/// </summary>
public partial class AlertWindow : KonkordWindow<AlertViewModel>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AlertWindow"/> class.
    /// Sets up the DataContext for design mode or attaches Avalonia Dev Tools in debug mode.
    /// </summary>
    public AlertWindow()
    {
        InitializeComponent();

        if (Design.IsDesignMode)
            DataContext = new AlertViewModel( "Design Time Title","This is a design time message.", EAlertType.Info);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AlertWindow"/> class with the specified title, message, and alert type.
    /// </summary>
    /// <param name="title">The title of the alert dialog.</param>
    /// <param name="message">The message content of the alert dialog.</param>
    /// <param name="type">The type of the alert, determining its appearance and behavior.</param>
    public AlertWindow(string title, string message, EAlertType type)
    {
        InitializeComponent();

#if DEBUG
        // Attaches Avalonia Dev Tools for debugging purposes.
        this.AttachDevTools();
#endif

        if (Design.IsDesignMode)
        {
            DataContext = new AlertViewModel("Design Time Title","This is a design time message.", EAlertType.Info);
            return;
        }
        
        // Sets the DataContext to an instance of AlertViewModel with the provided parameters.
        DataContext = new AlertViewModel(title, message, type);
        this.WhenActivated(disposables =>
        {
            DataContext.CloseWindow.RegisterHandler(action =>
            {
                Close(action.Input);
                action.SetOutput(Unit.Default);
                return Task.CompletedTask;
            }).DisposeWith(disposables);
        });
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        if (Design.IsDesignMode)
            return;
        
        if (DataContext == null)
            return;

        // Retrieves the color resource associated with the alert type and applies it to the icon.
        if (this.FindResource(DataContext.GetIconColor) is SolidColorBrush brush)
            AlertIcon.Foreground = brush;
    }
}