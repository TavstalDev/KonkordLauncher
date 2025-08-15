using System;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Tavstal.KonkordLauncher.Common.Helpers;
using Tavstal.KonkordLauncher.Common.Models;
using Tavstal.KonkordLauncher.Common.Translation;
using Tavstal.KonkordLauncher.Core.Enums;
using Tavstal.KonkordLauncher.Core.Helpers;
using Tavstal.KonkordLauncher.Core.Models;
using Tavstal.KonkordLauncher.Core.Services;
using Tavstal.KonkordLauncher.Desktop.Models;
using Tavstal.KonkordLauncher.Desktop.Models.Enums;
using Tavstal.KonkordLauncher.Desktop.Views.Dialogs;

namespace Tavstal.KonkordLauncher.Desktop.Views.Models;

/// <summary>
/// Represents the ViewModel for managing accounts in the application.
/// Provides functionality for logging in with Microsoft and offline accounts,
/// and handles related operations such as memory cleanup and progress tracking.
/// </summary>
public partial class AccountsViewModel : KonkordObservableObject
{
    private readonly CoreLogger _logger = CoreLogger.WithModuleType(typeof(AccountsViewModel));
    private readonly AccountsWindow _accountsWindow;
    [ObservableProperty] private bool isLoggingInMicrosoftAccount;
    [ObservableProperty] private double _progress = 0;
    [ObservableProperty] private string _progressText = "Loading...";
    [ObservableProperty] private string? _offlineUsername;

    /// <summary>
    /// Initializes a new instance of the <see cref="AccountsViewModel"/> class.
    /// </summary>
    /// <param name="parentWindow">The parent window associated with this ViewModel.</param>
    public AccountsViewModel(AccountsWindow parentWindow)
    {
        _accountsWindow = parentWindow;
    }
    
    /// <summary>
    /// Frees memory resources by resetting progress text and offline username.
    /// </summary>
    public override void FreeMemory()
    {
        ProgressText = string.Empty;
        OfflineUsername = null;
        _logger.Debug("AccountsViewModel memory freed.");
    }
    
    /// <summary>
    /// Stops the Microsoft authentication process and resets related states.
    /// </summary>
    private void StopMicrosoftAuth()
    {
        MicrosoftAuthService.Reset();
        IsLoggingInMicrosoftAccount = false;
    }

    #region Microsoft Commands

    /// <summary>
    /// Initiates the Microsoft account login process asynchronously.
    /// Handles authentication, error reporting, and account data updates.
    /// </summary>
    [RelayCommand]
    public async Task LoginMicrosoftAccountAsync()
    {
        IsLoggingInMicrosoftAccount = true;
        MicrosoftAuthService.OpenAuthenticationUrl();

        await AuthService.StartListening(_accountsWindow);
        _logger.Debug($"Microsoft Status result: {MicrosoftAuthService.AuthStatus}");
        if (MicrosoftAuthService.AuthStatus == EAuthStatus.FAILED)
        {
            IsLoggingInMicrosoftAccount = false;
            AlertWindow alert = new AlertWindow(
                TranslationManager.Translate("account.login.failed"),
                TranslationManager.Translate("account.login.microsoft.failed"),
                EAlertType.Error);
            await alert.ShowDialog(_accountsWindow);
            return;
        }

        if (MicrosoftAuthService.AuthStatus != EAuthStatus.SUCCESS)
            return;

        var microsoftAccount = MicrosoftAuthService.Account;
        if (microsoftAccount == null)
        {
            AlertWindow window = new AlertWindow(
                TranslationManager.Translate("account.login.failed"),
                TranslationManager.Translate("account.login.microsoft.null"),
                EAlertType.Error
            );
            await window.ShowDialog(_accountsWindow);
            StopMicrosoftAuth();
            return;
        }

        AccountData accountData = await LauncherHelper.GetAccountDataAsync();
        var account = accountData.Accounts.FirstOrDefault(x => x.Uuid == microsoftAccount.Uuid);
        if (account != null)
        {
            AlertWindow window = new AlertWindow(
                TranslationManager.Translate("account.duplicate"),
                TranslationManager.Translate("account.duplicate.microsoft"),
                EAlertType.Error
            );
            await window.ShowDialog(_accountsWindow);
            StopMicrosoftAuth();
            return;
        }

        if (string.IsNullOrEmpty(accountData.SelectedAccountId))
            accountData.SelectedAccountId = microsoftAccount.Id;
        accountData.Accounts.Add(microsoftAccount);
        await JsonHelper.WriteJsonFileAsync(PathHelper.LauncherAccountsPath, accountData);
        App.InvokeAccountsChanged();
        MicrosoftAuthService.Reset();
        _accountsWindow.Close();
    }

    /// <summary>
    /// Opens the Microsoft login URL in the default browser.
    /// </summary>
    [RelayCommand]
    public void MicrosoftOpenLoginLink() => MicrosoftAuthService.OpenAuthenticationUrl();

    /// <summary>
    /// Copies the Microsoft login URL to the system clipboard asynchronously.
    /// </summary>
    [RelayCommand]
    public async Task MicrosoftCopyLoginLinkAsync() => await _accountsWindow.SetClipboardTextAsync(MicrosoftAuthService.GetAuthenticationUrl());

    /// <summary>
    /// Cancels the Microsoft login process and stops the authentication listener.
    /// </summary>
    [RelayCommand]
    public void MicrosoftCancelLogin()
    {
        AuthService.StopListening();
        StopMicrosoftAuth();
    }
    
    #endregion

    #region Offline Commands
    /// <summary>
    /// Logs in with an offline account asynchronously.
    /// Validates the username, checks for duplicate accounts, and updates account data.
    /// </summary>
    [RelayCommand]
    public async Task OfflineLoginAsync()
    {
        if (string.IsNullOrEmpty(OfflineUsername))
        {
            AlertWindow window = new AlertWindow(
                TranslationManager.Translate("account.empty.name"),
                TranslationManager.Translate("account.empty.name.desc"),
                EAlertType.Warning
            );
            await window.ShowDialog(_accountsWindow);
            return;
        }

        string uuid = GameHelper.GetOfflinePlayerUUID(OfflineUsername);
        AccountData? accountData = await LauncherHelper.GetAccountDataAsync();
        var account = accountData.Accounts.FirstOrDefault(x => x.Uuid == uuid);
        if (account != null)
        {
            // Free memory
            uuid = string.Empty;
            accountData = null;
            AlertWindow window = new AlertWindow(
                TranslationManager.Translate("account.duplicate"),
                TranslationManager.Translate("account.duplicate.offline"),
                EAlertType.Error
            );
            await window.ShowDialog(_accountsWindow);
            return;
        }
        
        var id = Guid.NewGuid().ToString();
        if (string.IsNullOrEmpty(accountData.SelectedAccountId))
            accountData.SelectedAccountId = id;

        account = new Account(id, uuid, OfflineUsername, EAccountType.OFFLINE, "eyJhYiI6IkNkIiwidHlwIjoiSldUIn0.eyJoZWxsbyI6IndvcmxkIn0.F4k3-t0k3n_th1s-1s-n0t-v4l1d-51gn4tvr3", "no_refresh_token_needed",
            DateTime.Now);
        accountData.Accounts.Add(account);
        await JsonHelper.WriteJsonFileAsync(PathHelper.LauncherAccountsPath, accountData);
        App.InvokeAccountsChanged();
        
        // Free memory and close the window
        uuid = string.Empty;
        accountData = null;
        account = null;
        id = string.Empty;
        _accountsWindow.Close();
    }

    #endregion
}