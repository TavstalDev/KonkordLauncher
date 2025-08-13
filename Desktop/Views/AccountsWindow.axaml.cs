using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Tavstal.KonkordLauncher.Common.Helpers;
using Tavstal.KonkordLauncher.Common.Models;
using Tavstal.KonkordLauncher.Common.Translation;
using Tavstal.KonkordLauncher.Core.Enums;
using Tavstal.KonkordLauncher.Core.Helpers;
using Tavstal.KonkordLauncher.Core.Models;
using Tavstal.KonkordLauncher.Core.Services;
using Tavstal.KonkordLauncher.Desktop.Models.Enums;
using Tavstal.KonkordLauncher.Desktop.Views.Dialogs;
using Tavstal.KonkordLauncher.Desktop.Views.Models;

namespace Tavstal.KonkordLauncher.Desktop.Views;

/// <summary>
/// Represents the AccountsWindow, a partial class that serves as the main window for managing accounts.
/// Implements the IProgressReporter interface to handle progress updates.
/// </summary>
public partial class AccountsWindow : Window, IProgressReporter
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

        this.DataContext = new AccountsViewModel();
    }

    /// <summary>
    /// Stops the Microsoft authentication process by resetting the authentication service,
    /// notifying the application of account changes, and updating the view model state.
    /// </summary>
    private void StopMicrosoftAuth()
    {
        if (this.DataContext is not AccountsViewModel vm)
            return;
        
        MicrosoftAuthService.Reset();
        vm.IsLoggingInMicrosoftAccount = false;
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

    #region Event Handlers
    /// <summary>
    /// Handles the click event for initiating the Microsoft login process.
    /// Opens the Microsoft authentication URL, starts listening for authentication status,
    /// and processes the result to add the account or display appropriate error messages.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The event data.</param>
    private void MicrosoftLogin_OnClick(object? sender, RoutedEventArgs e)
    {
        if (this.DataContext is not AccountsViewModel vm)
            return;
        
        vm.IsLoggingInMicrosoftAccount = true;
        MicrosoftAuthService.OpenAuthenticationUrl();
        
        Task.Run(async () =>
        {
            await AuthService.StartListening(this);
            _logger.Debug($"Status result: {MicrosoftAuthService.AuthStatus}");
            if (MicrosoftAuthService.AuthStatus == EAuthStatus.FAILED)
            {
                vm.IsLoggingInMicrosoftAccount = false;
                Dispatcher.UIThread.Post(() =>
                {
                    AlertWindow alert = new AlertWindow("Login Failed",
                        "Failed to login to Microsoft account. Please try again later.",
                        EAlertType.Error);
                    alert.ShowDialog(this);
                });
                return;
            }
            
            if (MicrosoftAuthService.AuthStatus != EAuthStatus.SUCCESS)
                return;
            
            var microsoftAccount = MicrosoftAuthService.Account;
            if (microsoftAccount == null)
            {
                Dispatcher.UIThread.Post(() =>
                {
                    AlertWindow window = new AlertWindow(
                        "Login Failed",
                        "Failed to retrieve Microsoft account information. Please try again later.",
                        EAlertType.Error
                    );
                    window.ShowDialog(this);
                    StopMicrosoftAuth();
                });
                return;
            }
            
            AccountData accountData = await LauncherHelper.GetAccountDataAsync();
            var account = accountData.Accounts.FirstOrDefault(x => x.Uuid == microsoftAccount.Uuid);
            if (account != null)
            {
                Dispatcher.UIThread.Post(() =>
                {
                    AlertWindow window = new AlertWindow(
                        "Account Already Exists",
                        "An account with this username already exists. Please choose a different username.",
                        EAlertType.Error
                    );
                    window.ShowDialog(this);
                    StopMicrosoftAuth();
                });
                return;
            }
            
            if (string.IsNullOrEmpty(accountData.SelectedAccountId))
                accountData.SelectedAccountId = microsoftAccount.Id;
            accountData.Accounts.Add(microsoftAccount);
            await JsonHelper.WriteJsonFileAsync(PathHelper.LauncherAccountsPath, accountData);
            App.InvokeAccountsChanged();
            MicrosoftAuthService.Reset();

            Dispatcher.UIThread.Post(this.Close);
        });
    }

    /// <summary>
    /// Handles the click event to open the Microsoft authentication URL in the default web browser.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The event data.</param>
    private void OpenLink_OnClick(object? sender, RoutedEventArgs e)
    {
        if (this.DataContext is not AccountsViewModel vm)
            return;
    
        MicrosoftAuthService.OpenAuthenticationUrl();
    }

    /// <summary>
    /// Handles the click event to copy the Microsoft authentication URL to the clipboard.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The event data.</param>
    private void CopyLink_OnClick(object? sender, RoutedEventArgs e)
    {
        if (this.DataContext is not AccountsViewModel vm)
            return;
    
        var topLevel = GetTopLevel(this);

        if (topLevel?.Clipboard == null)
            return;

        Task.Run(async () => await topLevel.Clipboard.SetTextAsync(MicrosoftAuthService.GetAuthenticationUrl()));
    }

    /// <summary>
    /// Handles the click event to cancel the Microsoft login process.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The event data.</param>
    private void CancelLogin_OnClick(object? sender, RoutedEventArgs e)
    {
        AuthService.StopListening();
        StopMicrosoftAuth();
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
    /// Handles the click event for offline login. Validates the username, checks for duplicate accounts,
    /// and creates a new offline account if valid. Displays appropriate alerts for errors or success.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The event data.</param>
    private void OfflineLogin_OnClick(object? sender, RoutedEventArgs e)
    {
        if (string.IsNullOrEmpty(OfflineUsernameInput.Text))
        {
            AlertWindow window = new AlertWindow(
                "Name Required",
                "Please enter a username for the account.",
                EAlertType.Warning
            );
            window.ShowDialog(this);
            return;
        }

        string uuid = GameHelper.GetOfflinePlayerUUID(OfflineUsernameInput.Text);
        AccountData accountData = LauncherHelper.GetAccountData();
        var account = accountData.Accounts.FirstOrDefault(x => x.Uuid == uuid);
        if (account != null)
        {
            AlertWindow window = new AlertWindow(
                "Account Already Exists",
                "An account with this username already exists. Please choose a different username.",
                EAlertType.Error
                );
            window.ShowDialog(this);
            return;
        }
        
        var id = Guid.NewGuid().ToString();
        if (string.IsNullOrEmpty(accountData.SelectedAccountId))
            accountData.SelectedAccountId = id;

        account = new Account(id, uuid, OfflineUsernameInput.Text, EAccountType.OFFLINE, "no_access_token_needed", "no_refresh_token_needed",
            DateTime.Now);
        accountData.Accounts.Add(account);
        JsonHelper.WriteJsonFile(PathHelper.LauncherAccountsPath, accountData);
        App.InvokeAccountsChanged();
        this.Close();
    }
    #endregion
}