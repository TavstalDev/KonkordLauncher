using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Tavstal.KonkordLauncher.Desktop.Models;
using Tavstal.KonkordLauncher.Desktop.Models.Enums;
using Tavstal.KonkordLauncher.Desktop.Views.Dialogs.Models;

namespace Tavstal.KonkordLauncher.Desktop.Views.Dialogs;

/// <summary>
/// Represents a window for displaying alert dialogs with customizable title, message, and alert type.
/// </summary>
public partial class AlertWindow : KonkordWindow
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AlertWindow"/> class.
    /// Sets up the DataContext for design mode or attaches Avalonia Dev Tools in debug mode.
    /// </summary>
    public AlertWindow()
    {
        InitializeComponent();

        if (Design.IsDesignMode)
            this.DataContext = new AlertViewModel(this, "Design Time Title","This is a design time message.", EAlertType.Info);
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
            this.DataContext = new AlertViewModel(this, "Design Time Title","This is a design time message.", EAlertType.Info);
            return;
        }
        
        this.Loaded += Window_Loaded;
        // Sets the DataContext to an instance of AlertViewModel with the provided parameters.
        this.DataContext = new AlertViewModel(this, title, message, type);
    }
    
    /// <summary>
    /// Releases resources associated with the alert window by detaching the Loaded event handler.
    /// This helps prevent memory leaks by ensuring the event handler is no longer referenced.
    /// </summary>
    protected override void FreeMemory()
    {
        this.Loaded -= Window_Loaded;
    }

    /// <summary>
    /// Handles the Loaded event of the alert window to set the icon's foreground color based on the alert type.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The event data.</param>
    private void Window_Loaded(object? sender, RoutedEventArgs e)
    {
        if (Design.IsDesignMode)
            return;
        
        if (DataContext is not AlertViewModel vm)
            return;

        // Retrieves the color resource associated with the alert type and applies it to the icon.
        if (this.FindResource(vm.GetIconColor) is SolidColorBrush brush)
            AlertIcon.Foreground = brush;
    }
}