using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Tavstal.KonkordLauncher.Desktop.Enums;
using Tavstal.KonkordLauncher.Desktop.ViewModels;

namespace Tavstal.KonkordLauncher.Desktop.Views;

/// <summary>
/// Represents a window for displaying alert dialogs with customizable title, message, and alert type.
/// </summary>
public partial class AlertWindow : Window
{
    /// <summary>
    /// Delegate for handling button click responses in the alert dialog.
    /// </summary>
    /// <param name="accepted">Indicates whether the accepted button was clicked (true) or the cancel button (false).</param>
    public delegate void ButtonClicked(bool accepted);

    /// <summary>
    /// Event triggered when a button in the alert dialog is clicked.
    /// </summary>
    public event ButtonClicked? OnButtonResponse;

    public AlertWindow() {}
    
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

        // TODO: Add accept & deny texts
        
        // Sets the DataContext to an instance of AlertViewModel with the provided parameters.
        this.DataContext = new AlertViewModel
        {
            Title = title,
            Message = message,
            AlertType = type
        };
    }

    /// <summary>
    /// Handles the Loaded event of the alert window to set the icon's foreground color based on the alert type.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The event data.</param>
    private void AlertWindow_Loaded(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not AlertViewModel vm)
            return;

        // Retrieves the color resource associated with the alert type and applies it to the icon.
        if (this.FindResource(vm.GetIconColor) is SolidColorBrush brush)
        {
            Icon.Foreground = brush;
        }

        switch (vm.AlertType)
        {
            case EAlertType.Info:
            case EAlertType.Success:
            {
                CancelBtn.IsVisible = false;
                break;
            }    
        }
    }

    /// <summary>
    /// Handles the click event of the accept button, triggering the response event and closing the window.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The event data.</param>
    private void Accept_OnClick(object? sender, RoutedEventArgs e)
    {
        OnButtonResponse?.Invoke(true);
        this.Close();
    }

    /// <summary>
    /// Handles the click event of the cancel button, triggering the response event and closing the window.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The event data.</param>
    private void Cancel_OnClick(object? sender, RoutedEventArgs e)
    {
        OnButtonResponse?.Invoke(false);
        this.Close();
    }
}