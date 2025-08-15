using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;
using Tavstal.KonkordLauncher.Common.Translation;
using Tavstal.KonkordLauncher.Core.Models;
using Tavstal.KonkordLauncher.Desktop.Models;
using Tavstal.KonkordLauncher.Desktop.Views.Models;

namespace Tavstal.KonkordLauncher.Desktop.Views;

/// <summary>
/// Represents the AccountsWindow, a partial class that serves as the main window for managing accounts.
/// Implements the IProgressReporter interface to handle progress updates.
/// </summary>
public partial class AccountsWindow : KonkordWindow, IProgressReporter
{
    private readonly CoreLogger _logger = CoreLogger.WithModuleType(typeof(AccountsWindow));
    
    /// <summary>
    /// Initializes a new instance of the AccountsWindow class.
    /// Sets up the DataContext and attaches developer tools in debug mode.
    /// </summary>
    public AccountsWindow()
    {
        InitializeComponent();

#if DEBUG
        // Attaches Avalonia Dev Tools for debugging purposes.
        this.AttachDevTools();
#endif

        this.DataContext = new AccountsViewModel(this);
        OfflineUsernameInput.TextChanged += OfflineUsername_OnTextChanged;
    }

    /// <summary>
    /// Frees any resources or memory used by the AccountsWindow.
    /// This method is intended to be overridden in derived classes to release resources.
    /// </summary>
    protected override void FreeMemory()
    {
        OfflineUsernameInput.TextChanged -= OfflineUsername_OnTextChanged;
        _logger.Debug("AccountsWindow memory freed.");
    }
    
    /// <summary>
    /// Asynchronously sets the specified text to the system clipboard.
    /// Ensures that the clipboard is accessible and logs any errors encountered during the operation.
    /// </summary>
    /// <param name="text">The text to set to the clipboard. If null or empty, the method returns immediately.</param>
    public async Task SetClipboardTextAsync(string text)
    {
        if (string.IsNullOrEmpty(text))
            return;
        
        var topLevel = GetTopLevel(this);
        if (topLevel?.Clipboard == null)
            return;

        try
        {
            await topLevel.Clipboard.SetTextAsync(text);
        }
        catch (Exception ex)
        {
            _logger.Exc("Failed to set clipboard text");
            _logger.Error(ex);
        }
    }
    
    /// <summary>
    /// Handles the text changed event for the offline username input field. 
    /// Ensures that the input contains only alphanumeric characters and underscores.
    /// If invalid characters are detected, they are removed, and the caret position is adjusted accordingly.
    /// </summary>
    /// <param name="sender">The source of the event, expected to be a TextBox.</param>
    /// <param name="e">The event data.</param>
    private void OfflineUsername_OnTextChanged(object? sender, TextChangedEventArgs e)
    {
        if (sender is not TextBox textBox)
            return;
        
        if (textBox.Text == null)
            return;

        string allowed = Regex.Replace(textBox.Text, @"[^A-Za-z0-9_]", "");
        if (textBox.Text != allowed)
        {
            int caret = textBox.CaretIndex;
            textBox.Text = allowed;
            textBox.CaretIndex = Math.Min(caret - 1, allowed.Length);
        }
    }
    
    #region Progress Reporter

    /// <summary>
    /// Updates the progress value in the associated view model.
    /// </summary>
    /// <param name="progress">The progress value to set, typically between 0 and 1.</param>
    public void SetProgress(double progress)
    {
        Dispatcher.UIThread.Post(() =>
        {
            if (this.DataContext is not AccountsViewModel viewModel)
                return;

            viewModel.Progress = progress;
        });
    }

    /// <summary>
    /// Updates the status text in the associated view model.
    /// </summary>
    /// <param name="status">The status message to display.</param>
    public void SetStatus(string status)
    {
        Dispatcher.UIThread.Post(() =>
        {
            if (this.DataContext is not AccountsViewModel viewModel)
                return;

            viewModel.ProgressText = status;
        });
    }

    /// <summary>
    /// Updates the status text in the associated view model using a translated string.
    /// </summary>
    /// <param name="statusKey">The translation key for the status message.</param>
    /// <param name="args">Optional arguments to format the translated string.</param>
    public void SetStatusTranslated(string statusKey, params object[]? args)
    {
        Dispatcher.UIThread.Post(() =>
        {
            if (this.DataContext is not AccountsViewModel viewModel)
                return;

            viewModel.ProgressText = TranslationManager.Translate(statusKey, args);
        });
    }

    #endregion
}