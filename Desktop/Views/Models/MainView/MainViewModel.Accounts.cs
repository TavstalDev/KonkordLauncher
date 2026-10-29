using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using MinecraftSkinRender;
using MinecraftSkinRender.Image;
using SkiaSharp;
using Tavstal.BeaconSDK.Enums;
using Tavstal.BeaconSDK.Models.Accounts;
using Tavstal.BeaconSDK.Models.Logging;
using Tavstal.BeaconSDK.Models.MojangApi.User;
using Tavstal.BeaconSDK.Services.Abstractions;
using Tavstal.BeaconSDK.Services.Abstractions.Auth;
using Tavstal.KonkordLauncher.Desktop.Constants;
using Tavstal.KonkordLauncher.Desktop.Models.Avalonia;
using Tavstal.KonkordLauncher.Desktop.Models.Domain;
using Tavstal.KonkordLauncher.Desktop.Models.DTOs;
using Tavstal.KonkordLauncher.Desktop.Models.Enums;
using Tavstal.KonkordLauncher.Desktop.Services;
using Tavstal.KonkordLauncher.Desktop.Services.Localization;
using Tavstal.KonkordLauncher.Desktop.Services.Presentation.Abstractions;
using Tavstal.KonkordLauncher.Desktop.Services.Storage;
using ImageHelper = Tavstal.KonkordLauncher.Desktop.Helpers.ImageHelper;

namespace Tavstal.KonkordLauncher.Desktop.Views.Models.MainView;

public partial class MainViewModel_Accounts : KonkordObservableObject
{
    private readonly IMicrosoftAuthService _authService = null!;
    private readonly IBitmapService _bitmapService = null!;
    private readonly IDialogService _dialogService = null!;
    private readonly IEncryptionService _encryptionService = null!;
    private readonly IEventService _eventService = null!;
    private readonly IFilePickerService _filePickerService = null!;
    private readonly ILauncherStore _launcherStore = null!;
    private readonly IBeaconLogger _logger = null!;
    private readonly IMojangSkinService _mojangSkinService = null!;
    private readonly ITranslationService _translationService = null!;

    public MainViewModel_Accounts()
    {
        if (!Design.IsDesignMode)
            return;
        AccountAvatar =
            ImageHelper.LoadDesignTime("avares://KonkordLauncher/Assets/Images/Placeholders/steve_head.png");
    }

    public MainViewModel_Accounts(ILauncherStore launcherStore, ITranslationService translationService,
        IMicrosoftAuthService microsoftAuthService, IMojangSkinService mojangSkinService, IBitmapService bitmapService,
        IEventService eventService, IEncryptionService encryptionService, IDialogService dialogService,
        IFilePickerService filePickerService)
    {
        if (Design.IsDesignMode)
        {
            AccountAvatar =
                ImageHelper.LoadDesignTime("avares://KonkordLauncher/Assets/Images/Placeholders/steve_head.png");
            return;
        }

        _logger = Program.Services.GetRequiredService<IBeaconLogger<MainViewModel_Accounts>>();
        _launcherStore = launcherStore;
        _translationService = translationService;
        _authService = microsoftAuthService;
        _mojangSkinService = mojangSkinService;
        _bitmapService = bitmapService;
        _eventService = eventService;
        _encryptionService = encryptionService;
        _dialogService = dialogService;
        _filePickerService = filePickerService;

        AccountAvatar = _bitmapService.GetBitmap("avares://KonkordLauncher/Assets/Images/Placeholders/steve_head.png",
            BitmapSizes.AccountAvatar);
    }

    [ObservableProperty] public partial BitmapEntry AccountAvatar { get; set; }

    [ObservableProperty] public partial BitmapEntry AccountSkinPreview { get; set; } = null!;

    [ObservableProperty] public partial bool IsAccountHasWideModel { get; set; }

    [ObservableProperty] public partial bool IsAccountSkinProcessing { get; set; }

    [ObservableProperty] public partial AccountSkin SelectedSkin { get; set; } = null!;

    [ObservableProperty] public partial AccountDataModel AccountData { get; set; } = null!;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(AccountName))]
    [NotifyPropertyChangedFor(nameof(IsMojangAccount))]
    public partial AccountModel? SelectedAccount { get; set; }

    public ObservableCollection<SkinDataModel> Skins { get; } = [];
    public ObservableCollection<CapeDataModel> Capes { get; } = [];

    public string AccountName => SelectedAccount != null
        ? SelectedAccount.DisplayName
        : Design.IsDesignMode
            ? "Guest"
            : _translationService.Translate("main.sidebar.accounts.guest");

    public bool IsMojangAccount => SelectedAccount is { Type: EAccountType.MICROSOFT };

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        _eventService.OnAccountsChanged -= OnAccountUpdated;
    }

    public Task InitAsync(AccountData accountData)
    {
        AccountData = new AccountDataModel(accountData, _encryptionService);

        Account? selectedAccount = AccountData.Accounts.FirstOrDefault(x => x.Id == AccountData.SelectedAccountId);
        SelectedAccount = selectedAccount != null ? new AccountModel(selectedAccount, true, _encryptionService) : null;

        Dispatcher.UIThread.Invoke(() =>
        {
            OnAccountUpdated();
            _eventService.OnAccountsChanged += OnAccountUpdated;
            SubscribeToAccountDataChildren(AccountData);
        });
        return Task.CompletedTask;
    }

    #region Commands

    #region Accounts

    [RelayCommand]
    private async Task AddAccountBtnAsync()
    {
        await _dialogService.ShowAccountsAsync();
    }

    [RelayCommand]
    private void SelectAccountBtn(string accountId)
    {
        if (AccountData.SelectedAccountId == accountId)
            return;

        AccountData.SelectedAccountId = accountId;
        _eventService.RaiseAccountsChanged();
    }

    [RelayCommand]
    private async Task RefreshAccountBtnAsync(Account account, CancellationToken cancellationToken = default)
    {
        if (!account.CanExpire)
            return;

        var refreshToken = Encoding.UTF8.GetString(account.GetRefreshToken(_encryptionService));
        if (string.IsNullOrEmpty(refreshToken))
            return;

        try
        {
            if (_authService.AuthStatus != EAuthStatus.NONE)
                return;

            if (account.AccessTokenExpireDate > DateTime.UtcNow)
                return;

            if (!await _authService.RefreshLoginAsync(refreshToken, cancellationToken))
            {
                _logger.LogError($"Failed to refresh account {account.DisplayName} ({account.Id}).");
                return;
            }

            if (_authService.Account == null)
            {
                _logger.LogError(
                    $"Failed to refresh account {account.DisplayName} ({account.Id}) after successful api call.");
                return;
            }

            var updatedAccount = _authService.Account;
            updatedAccount.Id = account.Id; // Ensure the ID remains the same
            _logger.LogInformation($"Successfully refreshed account {account.DisplayName} ({account.Id}).");

            var accountData = await _launcherStore.GetAccountDataAsync(cancellationToken);
            var index = accountData.Accounts.FindIndex(x => x.Id == account.Id);
            accountData.Accounts[index] = updatedAccount;

            await _launcherStore.SaveAccountDataAsync(accountData, cancellationToken);
            _eventService.RaiseAccountsChanged();
            _authService.Reset();
        }
        finally
        {
            // ReSharper disable once RedundantAssignment
            refreshToken = string.Empty;
        }
    }

    [RelayCommand]
    private void RemoveAccountBtn(AccountModel account)
    {
        AccountData.Accounts.Remove(account);
        if (account.Id != AccountData.SelectedAccountId)
            return;

        AccountData.SelectedAccountId = AccountData.HasAccounts ? AccountData.Accounts.FirstOrDefault()?.Id : null;
    }

    #endregion

    #region Skins

    [RelayCommand]
    private async Task SkinUpload(CancellationToken cancellationToken = default)
    {
        try
        {
            // Prevent re-uploading while processing
            if (IsAccountSkinProcessing)
                return;

            IsAccountSkinProcessing = true;
            // Ensure an account is selected
            if (SelectedAccount == null)
                return;

            var filePath = await _filePickerService.OpenFilePickerAsync(
                _translationService.Translate("common.select.file"), "PNGs",
                ["*.png"]);
            if (filePath == null || !File.Exists(filePath))
                return;

            var skinId = Guid.NewGuid().ToString();
            var settings = await _launcherStore.GetSettingsAsync(cancellationToken: cancellationToken);
            var skinDir = Path.Combine(settings.Launcher.CacheDirectoryPath, "skins", SelectedAccount.Id, skinId);
            if (!Directory.Exists(skinDir))
                Directory.CreateDirectory(skinDir);
            var skinPath = Path.Combine(skinDir, "texture.png");
            File.Copy(filePath, skinPath, true);
            var accountIndex = AccountData.Accounts.IndexOf(SelectedAccount);
            SelectedAccount.Skins.Add(new AccountSkin(skinId, IsAccountHasWideModel ? "classic" : "slim",
                SelectedSkin.CapeId));
            AccountData.Accounts[accountIndex] = SelectedAccount;

            var previewPath = Path.Combine(skinDir, "preview.png");
            var headshotPath = Path.Combine(skinDir, "head.png");

            await using var skinStream = File.OpenRead(skinPath);
            using var skinBitmap = SKBitmap.Decode(skinStream);
            Skin3DHeadTypeB.MakeHeadImage(skinBitmap, 15, 65).SavePng(headshotPath);
            Skin2DTypeB.MakeSkinImage(skinBitmap, IsAccountHasWideModel ? SkinType.New : SkinType.NewSlim)
                .SavePng(previewPath);

            OnAccountUpdated();
        }
        catch (Exception ex)
        {
            _logger.LogError("Error while uploading skin: " + ex);
        }
        finally
        {
            IsAccountSkinProcessing = false;
        }
    }

    [RelayCommand]
    private async Task SkinSelect(SkinDataModel model, CancellationToken cancellationToken = default)
    {
        try
        {
            // Prevent re-selecting the same skin
            if (model.IsSelected || IsAccountSkinProcessing)
                return;
            IsAccountSkinProcessing = true;


            // Ensure an account is selected
            if (SelectedAccount == null)
                return;

            var settings = await _launcherStore.GetSettingsAsync(cancellationToken: cancellationToken);
            var skinPath = Path.Combine(settings.Launcher.CacheDirectoryPath, "skins", SelectedAccount.Id, model.Id,
                "texture.png");
            if (!File.Exists(skinPath))
            {
                _logger.LogError("Skin file does not exist: " + skinPath);
                return;
            }

            var accessToken = Encoding.UTF8.GetString(SelectedAccount.GetAccessToken(_encryptionService));
            var profile =
                await _mojangSkinService.UploadSkinAsync(accessToken, model.Variant, skinPath, cancellationToken);
            if (profile == null)
            {
                await _dialogService.ShowAlertAsync(
                    _translationService.Translate("common.error"),
                    _translationService.Translate("main.page.skins.alert.error"),
                    EAlertType.Error);
                return;
            }

            var newSkin = SelectedAccount.Skins.Find(x => x.Id == model.Id);
            if (SelectedSkin.CapeId != newSkin?.CapeId && newSkin is { CapeId: not null })
            {
                profile = await _mojangSkinService.ShowCapeAsync(accessToken, newSkin.CapeId, cancellationToken);
                if (profile == null)
                {
                    await _dialogService.ShowAlertAsync(
                        _translationService.Translate("common.error"),
                        _translationService.Translate("main.page.skins.alert.cape.change"),
                        EAlertType.Error);
                    return;
                }
            }

            var accountIndex = AccountData.Accounts.IndexOf(SelectedAccount);
            var skinIndex = SelectedAccount.Skins.FindIndex(x => x.Id == model.Id);
            if (skinIndex >= 0) SelectedAccount.Skins[skinIndex].MojangId = profile.Skins[0].Id;

            SelectedAccount.MojangProfile = profile;
            AccountData.Accounts[accountIndex] = SelectedAccount;
            IsAccountHasWideModel = model.Variant.Equals("classic", StringComparison.InvariantCultureIgnoreCase);
            OnAccountUpdated();
        }
        catch (Exception ex)
        {
            _logger.LogError("Error while selecting skin: " + ex);
        }
        finally
        {
            IsAccountSkinProcessing = false;
        }
    }

    [RelayCommand]
    private async Task CapeSelect(CapeDataModel model, CancellationToken cancellationToken = default)
    {
        try
        {
            // Prevent re-selecting the same cape
            if (model.IsSelected || IsAccountSkinProcessing)
                return;
            IsAccountSkinProcessing = true;

            // Ensure an account is selected
            if (SelectedAccount == null)
                return;

            MojangProfile? result;
            var accessToken = Encoding.UTF8.GetString(SelectedAccount.GetAccessToken(_encryptionService));
            if (model.Id.Equals("none", StringComparison.InvariantCultureIgnoreCase))
                result = await _mojangSkinService.HideCapeAsync(accessToken, cancellationToken);
            else
                result = await _mojangSkinService.ShowCapeAsync(accessToken, model.Id, cancellationToken);

            if (result == null)
            {
                await _dialogService.ShowAlertAsync(
                    _translationService.Translate("common.error"),
                    _translationService.Translate("main.page.skins.alert.cape.change"),
                    EAlertType.Error);
                foreach (var cape in Capes.ToList())
                {
                    var index = Capes.IndexOf(cape);
                    Capes.RemoveAt(index);
                    cape.IsSelected = cape.Id == SelectedSkin.CapeId && SelectedSkin.CapeId != null;
                    Capes.Insert(index, cape);
                }

                return;
            }

            foreach (var cape in Capes.ToList())
            {
                var index = Capes.IndexOf(cape);
                Capes.RemoveAt(index);
                cape.IsSelected = cape.Id == SelectedSkin.CapeId && SelectedSkin.CapeId != null;
                Capes.Insert(index, cape);
            }

            var accountIndex = AccountData.Accounts.IndexOf(SelectedAccount);
            var skinIndex = SelectedAccount.Skins.FindIndex(x => x.Id == SelectedSkin.Id);
            if (skinIndex >= 0)
            {
                SelectedAccount.Skins[skinIndex].MojangId = result.Skins[0].Id;
                SelectedAccount.Skins[skinIndex].CapeId = model.Id;
            }

            SelectedAccount.MojangProfile = result;
            AccountData.Accounts[accountIndex] = SelectedAccount;
            OnAccountUpdated();
        }
        catch (Exception ex)
        {
            _logger.LogError("Error while selecting cape: " + ex);
            await _dialogService.ShowAlertAsync(
                _translationService.Translate("common.error"),
                _translationService.Translate("main.page.skins.alert.cape.unexpected"),
                EAlertType.Error);
        }
        finally
        {
            IsAccountSkinProcessing = false;
        }
    }

    [RelayCommand]
    private async Task ModelSelect(string model, CancellationToken cancellationToken = default)
    {
        try
        {
            var newValue = model == "classic";
            // Prevent re-selecting the same model
            if (newValue == IsAccountHasWideModel || IsAccountSkinProcessing)
            {
                _logger.LogDebug("Skipped re-selecting the same skin model.");
                return;
            }

            IsAccountSkinProcessing = true;

            // Ensure an account is selected
            if (SelectedAccount == null)
            {
                _logger.LogDebug("No account selected while selecting skin model.");
                return;
            }

            var skin = SelectedAccount.MojangProfile?.Skins.FirstOrDefault(x => x.Id == SelectedSkin.MojangId);
            if (skin == null)
            {
                _logger.LogDebug("No skin found while selecting skin model.");
                return;
            }

            var accessToken = Encoding.UTF8.GetString(SelectedAccount.GetAccessToken(_encryptionService));
            var result = await _mojangSkinService.ChangeSkinAsync(accessToken, model, skin.Url, cancellationToken);
            if (result == null)
            {
                await _dialogService.ShowAlertAsync(
                    _translationService.Translate("common.error"),
                    _translationService.Translate("main.page.skins.alert.model.change"),
                    EAlertType.Error);
                return;
            }

            var accountIndex = AccountData.Accounts.IndexOf(SelectedAccount);
            SelectedAccount.MojangProfile = result;
            var skinIndex = SelectedAccount.Skins.FindIndex(x => x.Id == SelectedSkin.Id);
            if (skinIndex >= 0)
            {
                SelectedAccount.Skins[skinIndex].MojangId = result.Skins[0].Id;
                SelectedAccount.Skins[skinIndex].Model = model;
            }

            AccountData.Accounts[accountIndex] = SelectedAccount;
            IsAccountHasWideModel = newValue;
            SelectedSkin.Model = model;


            /*
             The preview should be updated here, but until it is 2D there is not much point in updating it since there is barely any changes that are visible.


            var settings = await _launcherStore.GetSettingsAsync(cancellationToken: cancellationToken);
            string skinsDir = Path.Combine(settings.Launcher.CacheDirectoryPath, "skins");
            // Get the new model image
            string path = Path.Combine(skinsDir, SelectedAccount.Id, skin.Id, "preview.png");
            Bitmap? img;
            try
            {
                img = new Bitmap(path);
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to load skin image: " + ex);
                return;
            }

            var oldSkin = AccountSkinPreview;
            AccountSkinPreview = img;
            oldSkin?.Dispose();*/
        }
        catch (Exception ex)
        {
            _logger.LogError("Error while selecting skin model: " + ex);
        }
        finally
        {
            IsAccountSkinProcessing = false;
        }
    }

    #endregion

    #endregion

    #region Account functions

    private void OnAccountUpdated()
    {
        Dispatcher.UIThread.Invoke(async () => await HandleAccountUpdatedAsync());
    }

    private async Task HandleAccountUpdatedAsync(CancellationToken cancellationToken = default)
    {
        var accountData = await _launcherStore.GetAccountDataAsync(cancellationToken);
        AccountData = new AccountDataModel(accountData, _encryptionService);
        var selectedAccount = accountData.Accounts.FirstOrDefault(x => x.Id == AccountData.SelectedAccountId);

        if (SelectedAccount?.Id != selectedAccount?.Id)
        {
            var skinCopies = Skins.ToList();
            foreach (var skin in skinCopies)
                skin.Dispose(_bitmapService);
            Skins.Clear();
            var capeCopies = Capes.ToList();
            foreach (var cape in capeCopies)
                cape.Dispose(_bitmapService);
            Capes.Clear();
        }

        SelectedAccount = selectedAccount != null ? new AccountModel(selectedAccount, true, _encryptionService) : null;
        if (SelectedAccount?.MojangProfile != null)
        {
            var settings = await _launcherStore.GetSettingsAsync(cancellationToken: cancellationToken);
            var skinsDir = Path.Combine(settings.Launcher.CacheDirectoryPath, "skins");
            var capesDir = Path.Combine(settings.Launcher.CacheDirectoryPath, "capes");

            var activeSkin = SelectedAccount.MojangProfile.Skins.FirstOrDefault(x =>
                x.State.Equals("active", StringComparison.InvariantCultureIgnoreCase));

            foreach (var skin in SelectedAccount.Skins)
            {
                var path = Path.Combine(skinsDir, SelectedAccount.Id, skin.Id, "preview.png");
                var img = new BitmapEntry(null, null);
                try
                {
                    img = await _bitmapService.GetBitmapAsync(path, BitmapSizes.SkinPreview);
                }
                catch (Exception ex)
                {
                    _logger.LogError("Failed to load skin image: " + ex);
                }

                var isActive = skin.MojangId == activeSkin?.Id && activeSkin != null;
                Skins.Add(new SkinDataModel(skin.Id, skin.Model, img, isActive));
                if (isActive)
                {
                    IsAccountHasWideModel = skin.Model.Equals("classic", StringComparison.InvariantCultureIgnoreCase);
                    SelectedSkin = skin;
                    AccountSkinPreview = img;
                }
            }


            var hadActiveCape = false;
            var noCapeImg = new BitmapEntry(null, null);
            try
            {
                noCapeImg = await _bitmapService.GetBitmapAsync(
                    "avares://KonkordLauncher/Assets/Images/Placeholders/no_cape.png", BitmapSizes.CapePreview);
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to load cape image: " + ex);
            }

            Capes.Add(new CapeDataModel("none", "None", noCapeImg, false));
            foreach (var cape in SelectedAccount.MojangProfile.Capes)
            {
                var path = Path.Combine(capesDir, $"{cape.Id}.png");
                var img = new BitmapEntry(null, null);
                try
                {
                    img = await _bitmapService.GetBitmapAsync(path, BitmapSizes.CapePreview);
                }
                catch (Exception ex)
                {
                    _logger.LogError("Failed to load cape image: " + ex);
                }

                var isActive = cape.State.Equals("active", StringComparison.InvariantCultureIgnoreCase);
                Capes.Add(new CapeDataModel(cape.Id, cape.Alias, img, isActive));
                if (isActive)
                    hadActiveCape = true;
            }

            if (!hadActiveCape)
                Capes[0].IsSelected = true;
        }

        await UpdateSelectedAccountAvatarAsync(cancellationToken);
    }


    private async Task UpdateSelectedAccountAvatarAsync(CancellationToken cancellationToken = default)
    {
        AccountAvatar.Dispose(_bitmapService);
        var settings = await _launcherStore.GetSettingsAsync(cancellationToken: cancellationToken);
        var skinsDir = Path.Combine(settings.Launcher.CacheDirectoryPath, "skins");
        string? avatarPath;
        if (SelectedAccount != null)
        {
            var avatarDir = Path.Combine(skinsDir, SelectedAccount.Id);
            if (!Directory.Exists(avatarDir))
                Directory.CreateDirectory(avatarDir);
            if (SelectedAccount.Type == EAccountType.OFFLINE)
            {
                avatarPath = Path.Combine(avatarDir, "head.png");
            }
            else
            {
                var selectedSkin =
                    SelectedAccount.Skins.FirstOrDefault(x =>
                        x.MojangId == SelectedAccount.MojangProfile?.Skins[0].Id);
                avatarPath = selectedSkin == null ? null : Path.Combine(avatarDir, selectedSkin.Id, "head.png");
            }
        }
        else
        {
            avatarPath = null;
        }

        AccountAvatar = File.Exists(avatarPath)
            ? await _bitmapService.GetBitmapAsync(avatarPath, BitmapSizes.AccountAvatar)
            : await _bitmapService.GetBitmapAsync(
                "avares://KonkordLauncher/Assets/Images/Placeholders/steve_head.png", BitmapSizes.AccountAvatar);
    }


    private void SubscribeToAccountDataChildren(AccountDataModel accountData)
    {
        accountData.PropertyChanged += OnChildAccountDataPropertyChanged;
    }

    private void UnsubscribeFromAccountDataChildren(AccountDataModel accountData)
    {
        accountData.PropertyChanged -= OnChildAccountDataPropertyChanged;
    }

    partial void OnAccountDataChanged(AccountDataModel? oldValue, AccountDataModel newValue)
    {
        _logger.LogDebug("AccountData changed with old and new value. Unsubscribing from old, subscribing to new.");

        if (oldValue != null)
            UnsubscribeFromAccountDataChildren(oldValue);

        SubscribeToAccountDataChildren(newValue);
        SaveAccountDataToFile(newValue);
    }


    private void OnChildAccountDataPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (sender is not AccountDataModel accountData)
            return;

        _logger.LogDebug($"Inner property '{e.PropertyName}' changed on {sender.GetType().Name}. Saving to file...");
        SaveAccountDataToFile(accountData);
    }

    private void SaveAccountDataToFile(AccountDataModel newValue)
    {
        var accounts = new AccountData
        {
            SelectedAccountId = newValue.SelectedAccountId ?? string.Empty,
            Accounts =
            [
                .. newValue.Accounts.Select(a =>
                {
                    Account account = new()
                    {
                        Id = a.Id,
                        Uuid = a.Uuid,
                        DisplayName = a.DisplayName,
                        Type = a.Type,
                        AccessTokenExpireDate = a.AccessTokenExpireDate,
                        MojangProfile = a.MojangProfile,
                        Skins = a.Skins
                    };
                    account.SetAccessToken(a.GetAccessToken(_encryptionService), _encryptionService);
                    account.SetRefreshToken(a.GetRefreshToken(_encryptionService), _encryptionService);
                    return account;
                })
            ]
        };

        _launcherStore.SaveAccountData(accounts);
    }

    #endregion
}
