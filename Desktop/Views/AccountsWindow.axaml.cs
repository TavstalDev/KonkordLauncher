using System;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using ReactiveUI;
using Tavstal.KonkordLauncher.Common.Helpers;
using Tavstal.KonkordLauncher.Common.Models;
using Tavstal.KonkordLauncher.Common.Translation;
using Tavstal.KonkordLauncher.Core.Enums;
using Tavstal.KonkordLauncher.Core.Helpers;
using Tavstal.KonkordLauncher.Core.Models;
using Tavstal.KonkordLauncher.Core.Services;
using Tavstal.KonkordLauncher.Desktop.Models;
using Tavstal.KonkordLauncher.Desktop.Models.Avalonia;
using Tavstal.KonkordLauncher.Desktop.Models.Enums;
using Tavstal.KonkordLauncher.Desktop.Views.Dialogs;
using Tavstal.KonkordLauncher.Desktop.Views.Models;
using Path = System.IO.Path;

namespace Tavstal.KonkordLauncher.Desktop.Views;

/// <summary>
/// Represents the AccountsWindow, a partial class that serves as the main window for managing accounts.
/// Implements the IProgressReporter interface to handle progress updates.
/// </summary>
public partial class AccountsWindow : KonkordWindow<AccountsViewModel>, IProgressReporter
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

        DataContext = new AccountsViewModel(this);

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
        MicrosoftAuthService.OnAuthStatusChanged += OnAuthStatusChanged;
    }

    /// <summary>
    /// Handles the cleanup and resource deallocation when the window is closing.
    /// Unsubscribes from events and stops any active listeners to ensure proper disposal.
    /// </summary>
    /// <param name="e">Provides data for the window closing event.</param>
    protected override void OnClosing(WindowClosingEventArgs e)
    {
        OfflineUsernameInput.TextChanged -= OfflineUsername_OnTextChanged;
        MicrosoftAuthService.OnAuthStatusChanged -= OnAuthStatusChanged;
        AuthHttpListener.StopListening();
        MicrosoftDeviceListener.StopListening();
        base.OnClosing(e);
    }
    
    private void DragStart_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        // Start moving the window when left mouse button is pressed
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            BeginMoveDrag(e);
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

    /// <summary>
    /// Handles changes in the authentication status for Microsoft accounts.
    /// Updates the UI and performs necessary actions based on the new status.
    /// </summary>
    /// <param name="status">The new authentication status.</param>
    private void OnAuthStatusChanged(EAuthStatus status)
    {
        Dispatcher.UIThread.Invoke(async () =>
        {
            if (DataContext == null)
                return;
            
            _logger.Debug($"Microsoft Status result: {MicrosoftAuthService.AuthStatus}");
            if (MicrosoftAuthService.AuthStatus == EAuthStatus.FAILED)
            {
                DataContext.IsLoggingInMicrosoftAccount = false;
                AlertWindow alertWindow = new(TranslationManager.Translate("account.login.failed"),
                    TranslationManager.Translate("account.login.microsoft.failed"),
                    EAlertType.Error);
                await alertWindow.ShowDialog(this);
                return;
            }

            if (MicrosoftAuthService.AuthStatus != EAuthStatus.SUCCESS)
                return;

            var microsoftAccount = MicrosoftAuthService.Account;
            if (microsoftAccount == null)
            {
                AlertWindow alertWindow = new(TranslationManager.Translate("account.login.failed"),
                    TranslationManager.Translate("account.login.microsoft.null"),
                    EAlertType.Error);
                await alertWindow.ShowDialog(this);
                DataContext.StopMicrosoftAuth();
                return;
            }

            AccountData accountData = await LauncherHelper.GetAccountDataAsync();
            var account = accountData.Accounts.FirstOrDefault(x => x.Uuid == microsoftAccount.Uuid);
            if (account != null)
            {
                AlertWindow alertWindow = new(TranslationManager.Translate("account.duplicate"),
                    TranslationManager.Translate("account.duplicate.microsoft"),
                    EAlertType.Error);
                await alertWindow.ShowDialog(this);
                DataContext.StopMicrosoftAuth();
                return;
            }

            if (string.IsNullOrEmpty(accountData.SelectedAccountId))
                accountData.SelectedAccountId = microsoftAccount.Id;
            accountData.Accounts.Add(microsoftAccount);
            var settings = await LauncherHelper.GetLauncherSettingsAsync();
            await JsonHelper.WriteJsonFileAsync(PathHelper.LauncherAccountsPath, accountData);
            
            foreach (var skin in microsoftAccount.Skins) 
                await SkinService.FetchSkins(settings.Launcher.CacheDirectoryPath, microsoftAccount.Id, microsoftAccount.Uuid, skin);
            await SkinService.FetchCapes(settings.Launcher.CacheDirectoryPath, microsoftAccount.MojangProfile?.Capes ?? []);
            
            GlobalEvents.InvokeAccountsChanged();
            MicrosoftAuthService.Reset(); 
            Close();
        });
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
            if (DataContext == null)
                return;

            DataContext.Progress = progress;
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
            if (DataContext == null)
                return;

            DataContext.ProgressText = status;
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
            if (DataContext == null)
                return;

            DataContext.ProgressText = TranslationManager.Translate(statusKey, args);
        });
    }

    #endregion
}