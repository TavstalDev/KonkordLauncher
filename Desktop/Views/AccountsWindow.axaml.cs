using System;
using System.Diagnostics.CodeAnalysis;
using System.Reactive;
using System.Reactive.Disposables.Fluent;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;
using Tavstal.KonkordLauncher.Common.Services.Abstractions;
using Tavstal.KonkordLauncher.Core.Enums;
using Tavstal.KonkordLauncher.Core.Models;
using Tavstal.KonkordLauncher.Core.Services.Abstractions.Auth;
using Tavstal.KonkordLauncher.Desktop.Models.Avalonia;
using Tavstal.KonkordLauncher.Desktop.Views.Dialogs;
using Tavstal.KonkordLauncher.Desktop.Views.Models;

namespace Tavstal.KonkordLauncher.Desktop.Views;

/// <summary>
/// Represents the AccountsWindow, a partial class that serves as the main window for managing accounts.
/// Implements the IProgressReporter interface to handle progress updates.
/// </summary>
public partial class AccountsWindow : KonkordWindow<AccountsViewModel>, IProgressReporter
{
    private readonly ITranslationService _translationService = null!;
    private readonly IMicrosoftAuthService _microsoftAuthService = null!;
    private readonly IMicrosoftDeviceAuthService _microsoftDeviceAuthService = null!;
    private readonly IMicrosoftHttpAuthService _microsoftHttpAuthService = null!;

    /// <summary>
    /// Initializes a new instance of the AccountsWindow class.
    /// Sets up the DataContext and attaches developer tools in debug mode.
    /// </summary>
    [RequiresUnreferencedCode( "Trimming may break this functionality if not configured to preserve the necessary members.")]
    public AccountsWindow()
    {
        InitializeComponent();
        DataContext = new AccountsViewModel(this);

        if (Design.IsDesignMode)
            return;

        var services = Program.ServiceProvider;
        _translationService = services.GetRequiredService<ITranslationService>();
        _microsoftAuthService = services.GetRequiredService<IMicrosoftAuthService>();
        _microsoftDeviceAuthService = services.GetRequiredService<IMicrosoftDeviceAuthService>();
        _microsoftHttpAuthService = services.GetRequiredService<IMicrosoftHttpAuthService>();
        _microsoftAuthService.OnAuthStatusChanged += OnAuthStatusChanged;

        this.WhenActivated(disposables =>
        {
            DataContext.CloseWindowInteraction.RegisterHandler(action =>
            {
                Close();
                action.SetOutput(Unit.Default);
                return Task.CompletedTask;
            }).DisposeWith(disposables);
            DataContext.ShowAlertDialog.RegisterHandler(async action =>
            {
                AlertWindow alertWindow = new(action.Input.Title, action.Input.Message, action.Input.Type);
                await alertWindow.ShowDialog(this);
                action.SetOutput(Unit.Default);
            }).DisposeWith(disposables);
            DataContext.SetClipboardText.RegisterHandler(async action =>
            {
                await SetClipboardTextAsync(action.Input);
                action.SetOutput(Unit.Default);
            }).DisposeWith(disposables);
        });

        OfflineUsernameInput.TextChanged += OfflineUsername_OnTextChanged;
    }

    #region Events
    
    /// <summary>
    /// Handles the cleanup and resource deallocation when the window is closing.
    /// Unsubscribes from events and stops any active listeners to ensure proper disposal.
    /// </summary>
    /// <param name="e">Provides data for the window closing event.</param>
    protected override void OnClosing(WindowClosingEventArgs e)
    {
        OfflineUsernameInput.TextChanged -= OfflineUsername_OnTextChanged;
        _microsoftAuthService.OnAuthStatusChanged -= OnAuthStatusChanged;
        Task.Run(async () =>
        {
            await _microsoftHttpAuthService.StopListeningAsync();
            await _microsoftDeviceAuthService.StopListeningAsync();
        });
        base.OnClosing(e);
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
    #endregion

    /// <summary>
    /// Handles changes in the authentication status for Microsoft accounts.
    /// Updates the UI and performs necessary actions based on the new status.
    /// </summary>
    /// <param name="status">The new authentication status.</param>
    private void OnAuthStatusChanged(EAuthStatus status)
    {
        Dispatcher.UIThread.Invoke(() =>
        {
            if (DataContext == null)
                return;
            DataContext.OnAuthStatusChange(status);
        });
    }

    #region Progress Reporter

    /// <summary>
    /// Updates the progress value in the associated view model.
    /// </summary>
    /// <param name="progress">The progress value to set, typically between 0 and 1.</param>
    public void ReportProgress(double progress)
    {
        Dispatcher.UIThread.Post(() =>
        {
            if (DataContext == null)
                return;

            DataContext.Progress = progress;
        });
    }

    /// <summary>
    /// Updates the status text in the associated view model.
    /// </summary>
    /// <param name="status">The status message to display.</param>
    public void UpdateStatus(string status)
    {
        Dispatcher.UIThread.Post(() =>
        {
            if (DataContext == null)
                return;

            DataContext.ProgressText = status;
        });
    }

    /// <summary>
    /// Updates the status text in the associated view model using a translated string.
    /// </summary>
    /// <param name="key">The translation key for the status message.</param>
    /// <param name="args">Optional arguments to format the translated string.</param>
    public void UpdateStatusTranslated(string key, params object[]? args)
    {
        Dispatcher.UIThread.Post(() =>
        {
            if (DataContext == null)
                return;

            DataContext.ProgressText = _translationService.Translate(key, args);
        });
    }
    
    /// <summary>
    /// Opens or displays the progress reporter UI for this view model.
    /// </summary>
    public void OpenReporter() { /* unused */ } 
    
    /// <summary>
    /// Closes or hides the progress reporter UI for this view model.
    /// </summary>
    public void CloseReporter() { /* unused */ }

    #endregion
}