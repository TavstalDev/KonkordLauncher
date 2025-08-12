using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Tavstal.KonkordLauncher.Common.Translation;
using Tavstal.KonkordLauncher.Desktop.Models.Enums;
using Tavstal.KonkordLauncher.Desktop.Views.Dialogs.Models;

namespace Tavstal.KonkordLauncher.Desktop.Views.Dialogs;

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
    /// Initializes a new instance of the <see cref="AlertWindow"/> class.
    /// Sets up the DataContext for design mode or attaches Avalonia Dev Tools in debug mode.
    /// </summary>
    public AlertWindow()
    {
        InitializeComponent();

        if (Design.IsDesignMode)
        {
            this.DataContext = new AlertViewModel
            {
                Title = "Design Time Title",
                Message = "This is a design time message.",
                AlertType = EAlertType.Info,
                AcceptedButtonText = "OK",
                CancelButtonText = "Cancel"
            };
        }
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
            this.DataContext = new AlertViewModel
            {
                Title = "Design Time Title",
                Message = "This is a design time message.",
                AlertType = EAlertType.Info,
                AcceptedButtonText = "OK",
                CancelButtonText = "Cancel"
            };
            return;
        }
        
        this.AlertTitle.Text = title;
        
        // Sets the DataContext to an instance of AlertViewModel with the provided parameters.
        this.DataContext = new AlertViewModel
        {
            Title = title,
            Message = message,
            AlertType = type
        };
        
        App.OnLanguageChanged += HandleLanguageChanged;
        HandleLanguageChanged(string.Empty);
    }

    /// <summary>
    /// Updates the text of the accept and cancel buttons in the alert dialog
    /// based on the current language settings.
    /// </summary>
    /// <param name="language">The current language code (not used in this implementation).</param>
    private void HandleLanguageChanged(string language)
    {
        if (this.DataContext is not AlertViewModel viewModel)
            return;
        
        viewModel.AcceptedButtonText = TranslationManager.Translate("common.ok");
        viewModel.CancelButtonText = TranslationManager.Translate("common.cancel");
    }

    /// <summary>
    /// Handles the Loaded event of the alert window to set the icon's foreground color based on the alert type.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The event data.</param>
    private void AlertWindow_Loaded(object? sender, RoutedEventArgs e)
    {
        if (Design.IsDesignMode)
            return;
        
        if (DataContext is not AlertViewModel vm)
            return;

        // Retrieves the color resource associated with the alert type and applies it to the icon.
        if (this.FindResource(vm.GetIconColor) is SolidColorBrush brush)
        {
            AlertIcon.Foreground = brush;
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
    /// Handles the click event for the accept button in the alert dialog.
    /// Closes the window and indicates that the accept button was clicked.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The event data.</param>
    private void Accept_OnClick(object? sender, RoutedEventArgs e) => Close(true);

    /// <summary>
    /// Handles the click event for the cancel button in the alert dialog.
    /// Closes the window and indicates that the cancel button was clicked.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The event data.</param>
    private void Cancel_OnClick(object? sender, RoutedEventArgs e) => Close(false);
}