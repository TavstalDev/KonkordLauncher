using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ReactiveUI;
using Tavstal.KonkordLauncher.Common.Helpers;
using Tavstal.KonkordLauncher.Common.Models;
using Tavstal.KonkordLauncher.Common.Translation;
using Tavstal.KonkordLauncher.Core.Enums;
using Tavstal.KonkordLauncher.Core.Helpers;
using Tavstal.KonkordLauncher.Core.Models;
using Tavstal.KonkordLauncher.Core.Models.Microsoft;
using Tavstal.KonkordLauncher.Core.Services;
using Tavstal.KonkordLauncher.Desktop.Helpers;
using Tavstal.KonkordLauncher.Desktop.Models;
using Tavstal.KonkordLauncher.Desktop.Models.Avalonia;
using Tavstal.KonkordLauncher.Desktop.Models.Enums;

namespace Tavstal.KonkordLauncher.Desktop.Views.Models;

/// <summary>
/// Represents the ViewModel for managing accounts in the application.
/// Provides functionality for logging in with Microsoft and offline accounts,
/// and handles related operations such as memory cleanup and progress tracking.
/// </summary>
public partial class AccountsViewModel : KonkordObservableObject
{
    private readonly CoreLogger _logger = CoreLogger.WithModuleType(typeof(AccountsViewModel));
    private readonly IProgressReporter _progressReporter;

    public Interaction<Unit, Unit> MinimizeWindowInteraction { get; } = new();
    public Interaction<Unit, Unit> MaximizeWindowInteraction { get; } = new();
    public Interaction<Unit, Unit> CloseWindowInteraction { get; } = new();
    public Interaction<Alert, Unit> ShowAlertDialog { get; } = new();
    public Interaction<string, Unit> SetClipboardText { get; } = new();
    
    [ObservableProperty] private bool isLoggingInMicrosoftAccount;
    [ObservableProperty] private double _progress;
    [ObservableProperty] private string _progressText = "Loading...";
    [ObservableProperty] private string? _offlineUsername;
    
    [ObservableProperty] private DeviceCodeResult _deviceData = new DeviceCodeResult()
    {
        UserCode = TranslationManager.Translate("common.loading"),
    };
    [ObservableProperty] private Bitmap? _qrCode = ImageHelper.Load("avares://Desktop/Assets/creeper.jpg").Result;

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
    /// Releases the resources used by the ViewModel, including the QR code bitmap.
    /// Ensures proper cleanup of unmanaged resources when disposing.
    /// </summary>
    /// <param name="disposing">
    /// A boolean value indicating whether the method is called explicitly 
    /// (true) or by the garbage collector (false).
    /// </param>
    protected override void Dispose(bool disposing)
    {
        QrCode?.Dispose();
        base.Dispose(disposing);
    }

    /// <summary>
    /// Stops the Microsoft authentication process and resets related states.
    /// </summary>
    public void StopMicrosoftAuth()
    {
        MicrosoftAuthService.Reset();
        IsLoggingInMicrosoftAccount = false;
    }
    
    #region Window Commands
    [RelayCommand]
    public async Task MinimizeWindow()
    {
        await MinimizeWindowInteraction.Handle(Unit.Default);
    }

    [RelayCommand]
    public async Task MaximizeWindow()
    {
        await MaximizeWindowInteraction.Handle(Unit.Default);
    }

    [RelayCommand]
    public async Task CloseWindow()
    {
        await CloseWindowInteraction.Handle(Unit.Default);
    }
    #endregion

    #region Microsoft Commands

    /// <summary>
    /// Initiates the Microsoft account login process asynchronously.
    /// Handles authentication, error reporting, and account data updates.
    /// </summary>
    [RelayCommand]
    private async Task LoginMicrosoftAccountAsync()
    {
        IsLoggingInMicrosoftAccount = true;
        var codeResult = await MicrosoftAuthService.CreateDeviceCodeAsync(_progressReporter);
        if (codeResult == null)
        {
            _logger.Error("Failed to create Microsoft device code.");
            return;
        }

        DeviceData = codeResult;
        QrCode?.Dispose();
        QrCode = ImageHelper.GenerateQrCode(DeviceData.VerificationUri);
        
        _ = Task.Run(async () =>
        {
            await AuthHttpListener.StartListening();
        });
        _ = Task.Run(async () =>
        {
            await MicrosoftDeviceListener.StartListening(DeviceData.DeviceCode, DeviceData.Interval);
        });
    }

    /// <summary>
    /// Opens the Microsoft login URL in the default browser.
    /// </summary>
    [RelayCommand]
    private void MicrosoftOpenLoginLink() => MicrosoftAuthService.OpenAuthenticationUrl();

    /// <summary>
    /// Opens the Microsoft device code verification URL in the default browser.
    /// </summary>
    [RelayCommand]
    private void MicrosoftOpenCodeLink()
    {
        MicrosoftAuthService.OpenUrl(DeviceData.VerificationUri);
    }

    /// <summary>
    /// Copies the Microsoft device code to the clipboard.
    /// </summary>
    [RelayCommand]
    private async Task MicrosoftCopyCode()
    {
        try
        {
            await SetClipboardText.Handle(DeviceData.UserCode);
        }
        catch (Exception ex)
        {
            _logger.Error(ex);
        }
    }
    
    /// <summary>
    /// Cancels the Microsoft login process and stops the authentication listener.
    /// </summary>
    [RelayCommand]
    private void MicrosoftCancelLogin()
    {
        AuthHttpListener.StopListening();
        MicrosoftDeviceListener.StopListening();
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

        account = new Account(id, uuid, OfflineUsername, EAccountType.OFFLINE, "0", "0",
            DateTime.Now, null);
        accountData.Accounts.Add(account);
        await JsonHelper.WriteJsonFileAsync(PathHelper.LauncherAccountsPath, accountData);
        GlobalEvents.InvokeAccountsChanged();
        
        await CloseWindowInteraction.Handle(Unit.Default);
    }

    #endregion
}