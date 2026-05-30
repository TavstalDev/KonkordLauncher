using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;
using Tavstal.KonkordLauncher.Common.Models;
using Tavstal.KonkordLauncher.Common.Services.Abstractions;
using Tavstal.KonkordLauncher.Core.Enums;
using Tavstal.KonkordLauncher.Core.Helpers.Domain;
using Tavstal.KonkordLauncher.Core.Helpers.IO;
using Tavstal.KonkordLauncher.Core.Helpers.Platform;
using Tavstal.KonkordLauncher.Core.Helpers.Serialization;
using Tavstal.KonkordLauncher.Core.Models;
using Tavstal.KonkordLauncher.Core.Models.Accounts;
using Tavstal.KonkordLauncher.Core.Models.Logging;
using Tavstal.KonkordLauncher.Core.Models.Microsoft;
using Tavstal.KonkordLauncher.Core.Services.Abstractions;
using Tavstal.KonkordLauncher.Core.Services.Abstractions.Auth;
using Tavstal.KonkordLauncher.Core.Services.Implementations;
using Tavstal.KonkordLauncher.Core.Services.Implementations.Auth;
using Tavstal.KonkordLauncher.Desktop.Helpers;
using Tavstal.KonkordLauncher.Desktop.Models.Avalonia;
using Tavstal.KonkordLauncher.Desktop.Models.Domain;
using Tavstal.KonkordLauncher.Desktop.Models.Enums;

namespace Tavstal.KonkordLauncher.Desktop.Views.Models;

/// <summary>
/// Represents the ViewModel for managing accounts in the application.
/// Provides functionality for logging in with Microsoft and offline accounts,
/// and handles related operations such as memory cleanup and progress tracking.
/// </summary>
public partial class AccountsViewModel : KonkordObservableObject
{
    private readonly ICustomLogger _logger;
    private readonly ITranslationService _translationService;
    private readonly ILauncherStore _launcherStore;
    private readonly IMicrosoftAuthService _authService;
    private readonly IMicrosoftDeviceAuthService _deviceAuthService;
    private readonly IMicrosoftHttpAuthService _httpAuthService;
    private readonly ISkinService _skinService;
    private readonly IProgressReporter _progressReporter;

    #region Interactions
    public Interaction<Unit, Unit> MinimizeWindowInteraction { get; } = new();
    public Interaction<Unit, Unit> MaximizeWindowInteraction { get; } = new();
    public Interaction<Unit, Unit> CloseWindowInteraction { get; } = new();
    public Interaction<Alert, Unit> ShowAlertDialog { get; } = new();
    public Interaction<string, Unit> SetClipboardText { get; } = new();
    #endregion

    #region Observable Properties
    [ObservableProperty]
    public partial bool IsLoggingInMicrosoftAccount { get; set; }

    [ObservableProperty]
    public partial bool IsProcessingLogin { get; set; }

    [ObservableProperty]
    public partial double Progress { get; set; }

    [ObservableProperty]
    public partial string ProgressText { get; set; } = "Loading...";

    [ObservableProperty]
    public partial string? OfflineUsername { get; set; }

    [ObservableProperty]
    public partial DeviceCodeResult? DeviceData { get; set; }

    [ObservableProperty]
    public partial Bitmap? QrCode { get; set; }

    [ObservableProperty] 
    public partial bool IsQrCodeLoading { get; set; } = true;
    #endregion

    /// <summary>
    /// Initializes a new instance of the <see cref="AccountsViewModel"/> class.
    /// </summary>
    /// <param name="progressReporter">
    /// An instance of <see cref="IProgressReporter"/> used to report progress during operations.
    /// </param>
    public AccountsViewModel(IProgressReporter progressReporter)
    {
        _progressReporter = progressReporter;

        if (Design.IsDesignMode)
        {
            DeviceData = new DeviceCodeResult
            {
                UserCode = "DEBUG",
            };
            return;
        }
        
        var services = Program.ServiceProvider;
        _logger = services.GetRequiredService<ICustomLogger<AccountsViewModel>>();
        _launcherStore = services.GetRequiredService<ILauncherStore>();
        _translationService = services.GetRequiredService<ITranslationService>();
        _authService = services.GetRequiredService<IMicrosoftAuthService>();
        _deviceAuthService = services.GetRequiredService<IMicrosoftDeviceAuthService>();
        _httpAuthService = services.GetRequiredService<IMicrosoftHttpAuthService>();
        _skinService = services.GetRequiredService<ISkinService>();
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
        _authService.Reset();
        IsLoggingInMicrosoftAccount = false;
        IsQrCodeLoading = true;
        var qrCodeCopy = QrCode;
        QrCode = null;
        qrCodeCopy?.Dispose();
    }

    public void OnAuthStatusChange(EAuthStatus status)
    {
        _ = HandleAuthStatusChange(status);
    }

    private async Task HandleAuthStatusChange(EAuthStatus status)
    {
        _logger.LogDebug($"Microsoft Status result: {status}");
        IsProcessingLogin = status == EAuthStatus.PROCESSING || status ==  EAuthStatus.SUCCESS;
        if (status == EAuthStatus.FAILED)
        {
            IsLoggingInMicrosoftAccount = false;
            await ShowAlertDialog.Handle(new Alert(_translationService.Translate("account.login.failed"),
                _translationService.Translate("account.login.microsoft.failed"),
                EAlertType.Error));
            return;
        }

        if (status != EAuthStatus.SUCCESS)
            return;

        var microsoftAccount = _authService.Account;
        if (microsoftAccount == null)
        {
            await ShowAlertDialog.Handle(new Alert(_translationService.Translate("account.login.failed"),
                _translationService.Translate("account.login.microsoft.null"),
                EAlertType.Error));
            StopMicrosoftAuth();
            return;
        }

        AccountData accountData = await _launcherStore.GetAccountDataAsync();
        var account = accountData.Accounts.FirstOrDefault(x => x.Uuid == microsoftAccount.Uuid);
        if (account != null)
        {
            await ShowAlertDialog.Handle(new Alert(_translationService.Translate("account.login.failed"),
                _translationService.Translate("account.duplicate.microsoft"),
                EAlertType.Error));
            StopMicrosoftAuth();
            return;
        }

        if (string.IsNullOrEmpty(accountData.SelectedAccountId))
            accountData.SelectedAccountId = microsoftAccount.Id;
        accountData.Accounts.Add(microsoftAccount);
        var settings = await _launcherStore.GetSettingsAsync();
        await JsonHelper.WriteJsonFileAsync(PathHelper.LauncherAccountsPath, accountData);

        foreach (var skin in microsoftAccount.Skins)
            await _skinService.FetchSkinsAsync(settings.Launcher.CacheDirectoryPath, microsoftAccount.Id,
                microsoftAccount.Uuid, skin);
        await _skinService.FetchCapesAsync(settings.Launcher.CacheDirectoryPath, microsoftAccount.MojangProfile?.Capes ?? []);

        GlobalEvents.InvokeAccountsChanged();
        _authService.Reset();
        await CloseWindowInteraction.Handle(Unit.Default);
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
        var codeResult = await _authService.CreateDeviceCodeAsync(_progressReporter);
        if (codeResult == null)
        {
            _logger.LogError("Failed to create Microsoft device code.");
            return;
        }
        
        IsQrCodeLoading = true;
        QrCode?.Dispose();
        DeviceData = codeResult;
        QrCode = ImageHelper.GenerateQrCode(DeviceData.VerificationUri);
        await Task.Delay(200); // Small delay to ensure the code is loaded
        IsQrCodeLoading = false;
        
        await Task.WhenAny(
            Task.Run(async () => await _httpAuthService.StartListeningAsync()),
            Task.Run(async () => await _deviceAuthService.StartListeningAsync(DeviceData.DeviceCode, DeviceData.Interval))
        );
    }

    /// <summary>
    /// Opens the Microsoft login URL in the default browser.
    /// </summary>
    [RelayCommand]
    private void MicrosoftOpenLoginLink() => _authService.OpenAuthenticationUrl();

    /// <summary>
    /// Opens the Microsoft device code verification URL in the default browser.
    /// </summary>
    [RelayCommand]
    private void MicrosoftOpenCodeLink() => OSHelper.OpenUrl(DeviceData!.VerificationUri);

    /// <summary>
    /// Copies the Microsoft device code to the clipboard.
    /// </summary>
    [RelayCommand]
    private async Task MicrosoftCopyCode() => await SetClipboardText.Handle(DeviceData!.UserCode);
    
    /// <summary>
    /// Cancels the Microsoft login process and stops the authentication listener.
    /// </summary>
    [RelayCommand]
    private async Task MicrosoftCancelLogin()
    {
        await _httpAuthService.StopListeningAsync();
        await _deviceAuthService.StopListeningAsync();
        StopMicrosoftAuth();
    }
    
    #endregion

    #region Offline Commands
    /// <summary>
    /// Logs in with an offline account asynchronously.
    /// Validates the username, checks for duplicate accounts, and updates account data.
    /// </summary>
    [RelayCommand]
    public async Task OfflineLoginAsync(CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(OfflineUsername))
        {
            await ShowAlertDialog.Handle(new Alert(_translationService.Translate("account.empty.name"),
                _translationService.Translate("account.empty.name.desc"),
                EAlertType.Warning));
            return;
        }

        string uuid = GameHelper.GetOfflinePlayerUUID(OfflineUsername);
        AccountData accountData = await _launcherStore.GetAccountDataAsync(cancellationToken);
        var account = accountData.Accounts.FirstOrDefault(x => x.Uuid == uuid);
        if (account != null)
        {
            await ShowAlertDialog.Handle(new Alert(_translationService.Translate("account.duplicate"),
                _translationService.Translate("account.duplicate.offline"),
                EAlertType.Error));
            return;
        }
        
        var id = Guid.NewGuid().ToString();
        if (string.IsNullOrEmpty(accountData.SelectedAccountId))
            accountData.SelectedAccountId = id;

        account = new Account
        {
            Id = id,
            Uuid = uuid,
            DisplayName = OfflineUsername,
            Type = EAccountType.OFFLINE,
            AccessTokenExpireDate = DateTime.Now,
            Skins = [],
            MojangProfile = null
        };
        account.SetAccessToken("0");
        account.SetRefreshToken("0");

        accountData.Accounts.Add(account);
        await JsonHelper.WriteJsonFileAsync(PathHelper.LauncherAccountsPath, accountData, cancellationToken);
        GlobalEvents.InvokeAccountsChanged();
        var settings = await _launcherStore.GetSettingsAsync(cancellationToken: cancellationToken);
        await _skinService.FetchOfflineSkinsAsync(settings.Launcher.CacheDirectoryPath, id, OfflineUsername, cancellationToken);
        
        await CloseWindowInteraction.Handle(Unit.Default);
    }

    #endregion
}