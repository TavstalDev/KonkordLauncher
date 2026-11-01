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

/// <summary>
///     The view model for the accounts window. It handles Microsoft and offline sign-in.
/// </summary>
public partial class AccountsViewModel : KonkordObservableObject
{
    private readonly IMicrosoftAuthService _authService = null!;
    private readonly IMicrosoftDeviceAuthService _deviceAuthService = null!;
    private readonly IDialogService _dialogService = null!;
    private readonly IEncryptionService _encryptionService = null!;
    private readonly IEventService _eventService = null!;
    private readonly IFilePickerService _filePickerService = null!;
    private readonly IMicrosoftHttpAuthService _httpAuthService = null!;
    private readonly ILauncherStore _launcherStore = null!;
    private readonly IBeaconLogger _logger = null!;
    private readonly ISkinService _skinService = null!;
    private readonly ITranslationService _translationService = null!;

    /// <summary>
    ///     Creates a view model for the XAML designer.
    /// </summary>
    public AccountsViewModel()
    {
        DeviceData = new DeviceCodeResult { UserCode = "DEBUG" };
    }

    /// <summary>
    ///     Creates the accounts view model with its services.
    /// </summary>
    /// <param name="logger">The logger for the view model.</param>
    /// <param name="launcherStore">The store that reads and saves launcher data.</param>
    /// <param name="translationService">The service that translates text.</param>
    /// <param name="microsoftAuthService">The Microsoft sign-in service.</param>
    /// <param name="microsoftDeviceAuthService">The service that listens for the device code sign-in.</param>
    /// <param name="microsoftHttpAuthService">The service that listens for the browser sign-in.</param>
    /// <param name="skinService">The service that fetches skins and capes.</param>
    /// <param name="encryptionService">The service that encrypts and decrypts data.</param>
    /// <param name="eventService">The service that raises launcher events.</param>
    /// <param name="dialogService">The service that shows dialogs.</param>
    /// <param name="filePickerService">The service that opens file pickers and uses the clipboard.</param>
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

    #region Interactions

    /// <summary>
    ///     Gets the interaction that asks the window to close.
    /// </summary>
    public Interaction<Unit, Unit> CloseWindowInteraction { get; } = new();

    #endregion

    /// <summary>
    ///     Releases the QR code bitmap and other resources used by the view model.
    /// </summary>
    /// <param name="disposing">
    ///     A value that indicates whether the method was called by user code (true)
    ///     or by the garbage collector (false).
    /// </param>
    protected override void Dispose(bool disposing)
    {
        QrCode?.Dispose();
        base.Dispose(disposing);
    }

    /// <summary>
    ///     Stops the Microsoft sign-in and resets the login state.
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

    /// <summary>
    ///     Handles a change in the Microsoft sign-in status.
    /// </summary>
    /// <param name="status">The new sign-in status.</param>
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
    ///     Logs in with an offline account. The user name must not be empty or already in use.
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

    /// <summary>
    ///     Gets or sets a value that indicates whether a Microsoft sign-in is running.
    /// </summary>
    [ObservableProperty] public partial bool IsLoggingInMicrosoftAccount { get; set; }

    /// <summary>
    ///     Gets or sets a value that indicates whether the sign-in is being processed.
    /// </summary>
    [ObservableProperty] public partial bool IsProcessingLogin { get; set; }

    /// <summary>
    ///     Gets or sets the sign-in progress value.
    /// </summary>
    [ObservableProperty] public partial double Progress { get; set; }

    /// <summary>
    ///     Gets or sets the text that describes the current sign-in step.
    /// </summary>
    [ObservableProperty] public partial string ProgressText { get; set; } = "Loading...";

    /// <summary>
    ///     Gets or sets the user name for an offline account.
    /// </summary>
    [ObservableProperty] public partial string? OfflineUsername { get; set; }

    /// <summary>
    ///     Gets or sets the Microsoft device code data.
    /// </summary>
    [ObservableProperty] public partial DeviceCodeResult? DeviceData { get; set; }

    /// <summary>
    ///     Gets or sets the QR code image for the Microsoft sign-in link.
    /// </summary>
    [ObservableProperty] public partial Bitmap? QrCode { get; set; }

    /// <summary>
    ///     Gets or sets a value that indicates whether the QR code is still loading.
    /// </summary>
    [ObservableProperty] public partial bool IsQrCodeLoading { get; set; } = true;

    #endregion

    #region Microsoft Commands

    /// <summary>
    ///     Starts the Microsoft sign-in and shows the device code and QR code.
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
    ///     Opens the Microsoft sign-in page in the default browser.
    /// </summary>
    [RelayCommand]
    private void MicrosoftOpenLoginLink()
    {
        _authService.OpenAuthenticationUrl();
    }

    /// <summary>
    ///     Opens the device code page in the default browser.
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
    ///     Cancels the Microsoft sign-in and stops the listeners.
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
