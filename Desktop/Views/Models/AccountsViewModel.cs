using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ReactiveUI;
using Tavstal.KonkordLauncher.Common.Helpers;
using Tavstal.KonkordLauncher.Common.Models;
using Tavstal.KonkordLauncher.Common.Translation;
using Tavstal.KonkordLauncher.Core.Enums;
using Tavstal.KonkordLauncher.Core.Helpers;
using Tavstal.KonkordLauncher.Core.Models;
using Tavstal.KonkordLauncher.Core.Services;
using Tavstal.KonkordLauncher.Desktop.Models;
using Tavstal.KonkordLauncher.Desktop.Models.Enums;

namespace Tavstal.KonkordLauncher.Desktop.Views.Models;

/// <summary>
/// Represents the ViewModel for managing accounts in the application.
/// Provides functionality for logging in with Microsoft and offline accounts,
/// and handles related operations such as memory cleanup and progress tracking.
/// </summary>
public partial class AccountsViewModel : ObservableObject
{
    private readonly CoreLogger _logger = CoreLogger.WithModuleType(typeof(AccountsViewModel));
    private readonly IProgressReporter _progressReporter;

    public Interaction<Unit, Unit> CloseWindow { get; }  = new();
    public Interaction<Alert, Unit> ShowAlertDialog { get; } = new();
    public Interaction<string, Unit> SetClipboardText { get; } = new();
    
    [ObservableProperty] private bool isLoggingInMicrosoftAccount;
    [ObservableProperty] private double _progress;
    [ObservableProperty] private string _progressText = "Loading...";
    [ObservableProperty] private string? _offlineUsername;

    /// <summary>
    /// Initializes a new instance of the <see cref="AccountsViewModel"/> class.
    /// </summary>
    /// <param name="progressReporter">
    /// An instance of <see cref="IProgressReporter"/> used to report progress during operations.
    /// </param>
    public AccountsViewModel(IProgressReporter progressReporter)
    {
        _progressReporter = progressReporter;
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

        await AuthService.StartListening(_progressReporter);
        _logger.Debug($"Microsoft Status result: {MicrosoftAuthService.AuthStatus}");
        if (MicrosoftAuthService.AuthStatus == EAuthStatus.FAILED)
        {
            IsLoggingInMicrosoftAccount = false;
            await ShowAlertDialog.Handle(new Alert(TranslationManager.Translate("account.login.failed"),
                TranslationManager.Translate("account.login.microsoft.failed"),
                EAlertType.Error));
            return;
        }

        if (MicrosoftAuthService.AuthStatus != EAuthStatus.SUCCESS)
            return;

        var microsoftAccount = MicrosoftAuthService.Account;
        if (microsoftAccount == null)
        {
            await ShowAlertDialog.Handle(new Alert(TranslationManager.Translate("account.login.failed"),
                TranslationManager.Translate("account.login.microsoft.null"),
                EAlertType.Error));
            StopMicrosoftAuth();
            return;
        }

        AccountData accountData = await LauncherHelper.GetAccountDataAsync();
        var account = accountData.Accounts.FirstOrDefault(x => x.Uuid == microsoftAccount.Uuid);
        if (account != null)
        {
            await ShowAlertDialog.Handle(new Alert(TranslationManager.Translate("account.duplicate"),
                TranslationManager.Translate("account.duplicate.microsoft"),
                EAlertType.Error));
            StopMicrosoftAuth();
            return;
        }

        if (string.IsNullOrEmpty(accountData.SelectedAccountId))
            accountData.SelectedAccountId = microsoftAccount.Id;
        accountData.Accounts.Add(microsoftAccount);
        await JsonHelper.WriteJsonFileAsync(PathHelper.LauncherAccountsPath, accountData);
        App.InvokeAccountsChanged();
        MicrosoftAuthService.Reset();
        await CloseWindow.Handle(Unit.Default);
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
    public async Task MicrosoftCopyLoginLinkAsync() => await SetClipboardText.Handle(MicrosoftAuthService.GetAuthenticationUrl());

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
            await ShowAlertDialog.Handle(new Alert(TranslationManager.Translate("account.empty.name"),
                TranslationManager.Translate("account.empty.name.desc"),
                EAlertType.Warning));
            return;
        }

        string uuid = GameHelper.GetOfflinePlayerUUID(OfflineUsername);
        AccountData accountData = await LauncherHelper.GetAccountDataAsync();
        var account = accountData.Accounts.FirstOrDefault(x => x.Uuid == uuid);
        if (account != null)
        {
            await ShowAlertDialog.Handle(new Alert(TranslationManager.Translate("account.duplicate"),
                TranslationManager.Translate("account.duplicate.offline"),
                EAlertType.Error));
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
        
        await CloseWindow.Handle(Unit.Default);
    }

    #endregion
}