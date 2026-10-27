using System;
using System.Linq;
using System.Reactive;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Tavstal.BeaconSDK.Enums;
using Tavstal.BeaconSDK.Helpers.Domain;
using Tavstal.BeaconSDK.Helpers.Platform;
using Tavstal.BeaconSDK.Models.Accounts;
using Tavstal.BeaconSDK.Models.Logging;
using Tavstal.BeaconSDK.Models.Microsoft;
using Tavstal.BeaconSDK.Services.Abstractions;
using Tavstal.BeaconSDK.Services.Abstractions.Auth;
using Tavstal.KonkordLauncher.Desktop.Helpers;
using Tavstal.KonkordLauncher.Desktop.Models.Avalonia;
using Tavstal.KonkordLauncher.Desktop.Models.Enums;
using Tavstal.KonkordLauncher.Desktop.Services;
using Tavstal.KonkordLauncher.Desktop.Services.Accounts;
using Tavstal.KonkordLauncher.Desktop.Services.Localization;
using Tavstal.KonkordLauncher.Desktop.Services.Presentation.Abstractions;
using Tavstal.KonkordLauncher.Desktop.Services.Storage;

namespace Tavstal.KonkordLauncher.Desktop.Views.Models;

public partial class AccountsViewModel : KonkordObservableObject
{
    private readonly IMicrosoftAuthService _authService = null!;
    private readonly IMicrosoftDeviceAuthService _deviceAuthService = null!;
    private readonly IEncryptionService _encryptionService = null!;
    private readonly IEventService _eventService = null!;
    private readonly IMicrosoftHttpAuthService _httpAuthService = null!;
    private readonly ILauncherStore _launcherStore = null!;
    private readonly IBeaconLogger _logger = null!;
    private readonly ISkinService _skinService = null!;
    private readonly ITranslationService _translationService = null!;
    private readonly IDialogService _dialogService = null!;
    private readonly IFilePickerService _filePickerService = null!;

    public AccountsViewModel()
    {
        DeviceData = new DeviceCodeResult { UserCode = "DEBUG" };
    }

    public AccountsViewModel(IBeaconLogger<AccountsViewModel> logger,
        ILauncherStore launcherStore, ITranslationService translationService,
        IMicrosoftAuthService microsoftAuthService,
        IMicrosoftDeviceAuthService microsoftDeviceAuthService, IMicrosoftHttpAuthService microsoftHttpAuthService,
        ISkinService skinService, IEncryptionService encryptionService, IEventService eventService,
        IDialogService dialogService, IFilePickerService filePickerService)
    {
        if (Design.IsDesignMode)
        {
            DeviceData = new DeviceCodeResult { UserCode = "DEBUG" };
            return;
        }

        _logger = logger;
        _launcherStore = launcherStore;
        _translationService = translationService;
        _authService = microsoftAuthService;
        _deviceAuthService = microsoftDeviceAuthService;
        _httpAuthService = microsoftHttpAuthService;
        _skinService = skinService;
        _encryptionService = encryptionService;
        _eventService = eventService;
        _dialogService = dialogService;
        _filePickerService = filePickerService;

        ProgressText = _translationService.Translate("auth.logging_in");
    }

    /// <summary>
    ///     Releases the resources used by the ViewModel, including the QR code bitmap.
    ///     Ensures proper cleanup of unmanaged resources when disposing.
    /// </summary>
    /// <param name="disposing">
    ///     A boolean value indicating whether the method is called explicitly
    ///     (true) or by the garbage collector (false).
    /// </param>
    protected override void Dispose(bool disposing)
    {
        QrCode?.Dispose();
        base.Dispose(disposing);
    }

    /// <summary>
    ///     Stops the Microsoft authentication process and resets related states.
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
        IsProcessingLogin = status == EAuthStatus.PROCESSING || status == EAuthStatus.SUCCESS;
        if (status == EAuthStatus.FAILED)
        {
            IsLoggingInMicrosoftAccount = false;
            await _dialogService.ShowAlertAsync(
                _translationService.Translate("account.login.failed"),
                _translationService.Translate("account.login.microsoft.failed"),
                EAlertType.Error);
            return;
        }

        if (status != EAuthStatus.SUCCESS)
            return;

        var microsoftAccount = _authService.Account;
        if (microsoftAccount == null)
        {
            await _dialogService.ShowAlertAsync(
                _translationService.Translate("account.login.failed"),
                _translationService.Translate("account.login.microsoft.null"),
                EAlertType.Error);
            StopMicrosoftAuth();
            return;
        }

        var accountData = await _launcherStore.GetAccountDataAsync();
        var account = accountData.Accounts.FirstOrDefault(x => x.Uuid == microsoftAccount.Uuid);
        if (account != null)
        {
            await _dialogService.ShowAlertAsync(
                _translationService.Translate("account.login.failed"),
                _translationService.Translate("account.duplicate.microsoft"),
                EAlertType.Error);
            StopMicrosoftAuth();
            return;
        }

        if (string.IsNullOrEmpty(accountData.SelectedAccountId))
            accountData.SelectedAccountId = microsoftAccount.Id;
        accountData.Accounts.Add(microsoftAccount);
        var settings = await _launcherStore.GetSettingsAsync();
        await _launcherStore.SaveAccountDataAsync(accountData);

        foreach (var skin in microsoftAccount.Skins)
            await _skinService.FetchSkinsAsync(settings.Launcher.CacheDirectoryPath, microsoftAccount.Id,
                microsoftAccount.Uuid, skin);
        await _skinService.FetchCapesAsync(settings.Launcher.CacheDirectoryPath,
            microsoftAccount.MojangProfile?.Capes ?? []);

        _eventService.RaiseAccountsChanged();
        _authService.Reset();
        await CloseWindowInteraction.Handle(Unit.Default);
    }

    #region Window Commands

    [RelayCommand]
    private async Task CloseWindow()
    {
        await CloseWindowInteraction.Handle(Unit.Default);
    }

    #endregion

    #region Offline Commands

    /// <summary>
    ///     Logs in with an offline account asynchronously.
    ///     Validates the username, checks for duplicate accounts, and updates account data.
    /// </summary>
    [RelayCommand]
    private async Task OfflineLoginAsync(CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(OfflineUsername))
        {
            await _dialogService.ShowAlertAsync(
                _translationService.Translate("account.empty.name.title"),
                _translationService.Translate("account.empty.name.message"),
                EAlertType.Warning);
            return;
        }

        var uuid = GameHelper.GetOfflinePlayerUUID(OfflineUsername);
        var accountData = await _launcherStore.GetAccountDataAsync(cancellationToken);
        var account = accountData.Accounts.FirstOrDefault(x => x.Uuid == uuid);
        if (account != null)
        {
            await _dialogService.ShowAlertAsync(
                _translationService.Translate("account.duplicate.title"),
                _translationService.Translate("account.duplicate.offline"),
                EAlertType.Error);
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
            AccessTokenExpireDate = DateTime.UtcNow,
            Skins = [],
            MojangProfile = null
        };
        account.SetAccessToken([], _encryptionService);
        account.SetRefreshToken([], _encryptionService);

        accountData.Accounts.Add(account);
        await _launcherStore.SaveAccountDataAsync(accountData, cancellationToken);
        _eventService.RaiseAccountsChanged();
        var settings = await _launcherStore.GetSettingsAsync(cancellationToken: cancellationToken);
        await _skinService.FetchOfflineSkinsAsync(settings.Launcher.CacheDirectoryPath, id, OfflineUsername,
            cancellationToken);

        await CloseWindowInteraction.Handle(Unit.Default);
    }

    #endregion

    #region Observable Properties

    [ObservableProperty] public partial bool IsLoggingInMicrosoftAccount { get; set; }

    [ObservableProperty] public partial bool IsProcessingLogin { get; set; }

    [ObservableProperty] public partial double Progress { get; set; }

    [ObservableProperty] public partial string ProgressText { get; set; } = "Loading...";

    [ObservableProperty] public partial string? OfflineUsername { get; set; }

    [ObservableProperty] public partial DeviceCodeResult? DeviceData { get; set; }

    [ObservableProperty] public partial Bitmap? QrCode { get; set; }

    [ObservableProperty] public partial bool IsQrCodeLoading { get; set; } = true;

    #endregion

    #region Interactions

    public Interaction<Unit, Unit> CloseWindowInteraction { get; } = new();

    #endregion

    #region Microsoft Commands

    /// <summary>
    ///     Initiates the Microsoft account login process asynchronously.
    ///     Handles authentication, error reporting, and account data updates.
    /// </summary>
    [RelayCommand]
    private async Task LoginMicrosoftAccountAsync()
    {
        IsLoggingInMicrosoftAccount = true;
        var codeResult = await _authService.CreateDeviceCodeAsync();
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
            Task.Run(async () =>
                await _deviceAuthService.StartListeningAsync(DeviceData.DeviceCode,
                    TimeSpan.FromSeconds(DeviceData.Interval)))
        );
    }

    /// <summary>
    ///     Opens the Microsoft login URL in the default browser.
    /// </summary>
    [RelayCommand]
    private void MicrosoftOpenLoginLink()
    {
        _authService.OpenAuthenticationUrl();
    }

    /// <summary>
    ///     Opens the Microsoft device code verification URL in the default browser.
    /// </summary>
    [RelayCommand]
    private void MicrosoftOpenCodeLink()
    {
        OSHelper.OpenUrl(DeviceData!.VerificationUri);
    }

    /// <summary>
    ///     Copies the Microsoft device code to the clipboard.
    /// </summary>
    [RelayCommand]
    private async Task MicrosoftCopyCode()
    {
        await _filePickerService.SetClipboardTextAsync(DeviceData!.UserCode);
    }

    /// <summary>
    ///     Cancels the Microsoft login process and stops the authentication listener.
    /// </summary>
    [RelayCommand]
    private async Task MicrosoftCancelLogin()
    {
        await _httpAuthService.StopListeningAsync();
        await _deviceAuthService.StopListeningAsync();
        StopMicrosoftAuth();
    }

    #endregion
}
