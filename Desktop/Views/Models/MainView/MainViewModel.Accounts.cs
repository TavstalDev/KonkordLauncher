using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MinecraftSkinRender;
using MinecraftSkinRender.Image;
using SkiaSharp;
using Tavstal.KonkordLauncher.Common.Helpers;
using Tavstal.KonkordLauncher.Common.Models;
using Tavstal.KonkordLauncher.Common.Translation;
using Tavstal.KonkordLauncher.Core.Enums;
using Tavstal.KonkordLauncher.Core.Helpers.IO;
using Tavstal.KonkordLauncher.Core.Helpers.Serialization;
using Tavstal.KonkordLauncher.Core.Models;
using Tavstal.KonkordLauncher.Core.Models.Accounts;
using Tavstal.KonkordLauncher.Core.Models.MojangApi.User;
using Tavstal.KonkordLauncher.Core.Services;
using Tavstal.KonkordLauncher.Desktop.Models.Avalonia;
using Tavstal.KonkordLauncher.Desktop.Models.Domain;
using Tavstal.KonkordLauncher.Desktop.Models.Enums;
using ImageHelper = Tavstal.KonkordLauncher.Desktop.Helpers.ImageHelper;

namespace Tavstal.KonkordLauncher.Desktop.Views.Models.MainView;

/// <summary>
/// Partial view-model that encapsulates account-related state and actions for the main view.
/// </summary>
public partial class MainViewModel_Accounts : KonkordObservableObject
{
    private readonly CoreLogger _logger = CoreLogger.WithModuleType(typeof(MainViewModel_Accounts));
    private readonly MainViewModel _parent;
    
    [ObservableProperty] private Bitmap _accountAvatar;
    [ObservableProperty] private Bitmap? _accountSkinPreview;
    [ObservableProperty] private bool _isAccountHasWideModel;
    [ObservableProperty] private bool _isAccountSkinProcessing;
    [ObservableProperty] private AccountSkin _selectedSkin;
    [ObservableProperty] private AccountDataModel _accountData;
    [ObservableProperty, NotifyPropertyChangedFor(nameof(AccountName)), NotifyPropertyChangedFor(nameof(IsMojangAccount))]  private Account? _selectedAccount;
    
    public ObservableCollection<SkinDataModel> Skins { get; } = new();
    public ObservableCollection<CapeDataModel> Capes { get; } = new();

    public string AccountName => SelectedAccount != null
        ? SelectedAccount.DisplayName
        : TranslationManager.Translate("main.sidebar.accounts.guest");
    
    public bool IsMojangAccount => SelectedAccount is { Type: EAccountType.MICROSOFT };
    
    /// <summary>
    /// Creates a new instance of the accounts sub-viewmodel.
    /// </summary>
    /// <param name="parent">The owning <see cref="MainViewModel"/>. Required for raising parent interactions and dialogs.</param>
    public MainViewModel_Accounts(MainViewModel parent)
    {
        _parent = parent;
    }
    
    /// <summary>
    /// Releases resources used by this view model and unsubscribes from global account change events.
    /// </summary>
    /// <param name="disposing">Indicates whether the method is being called from managed code (<c>true</c>) or from a finalizer (<c>false</c>).</param>
    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        GlobalEvents.OnAccountsChanged -= OnAccountUpdated;
    }
    
    /// <summary>
    /// Initializes the accounts sub-view-model with the provided account data.
    /// </summary>
    /// <param name="accountData">
    /// The account data container (typically read from disk) which includes the list of accounts
    /// and the currently selected account id.
    /// </param>
    public async Task InitAsync(AccountData accountData)
    {
        AccountData = new AccountDataModel(accountData);
        
        Account? selectedAccount = AccountData.Accounts.FirstOrDefault(x => x.Id == AccountData.SelectedAccountId);
        SelectedAccount = selectedAccount;
            
        OnAccountUpdated();
        
        GlobalEvents.OnAccountsChanged += OnAccountUpdated;
    }

    #region Commands

    #region Accounts

    /// <summary>
    /// Opens the account management window to add a new account asynchronously.
    /// </summary>
    [RelayCommand]
    private async Task AddAccountBtnAsync() => await _parent.ShowAccountsDialogInteraction.Handle(Unit.Default);
    
    /// <summary>
    /// Selects the specified account by its ID and updates the selected account in the application.
    /// </summary>
    /// <param name="accountId">The ID of the account to select.</param>
    [RelayCommand]
    private void SelectAccountBtn(string accountId)
    {
        if (AccountData.SelectedAccountId == accountId)
            return;

        AccountData.SelectedAccountId = accountId;
        GlobalEvents.InvokeAccountsChanged();
    }

    /// <summary>
    /// Refreshes the specified account's authentication token asynchronously if it has expired.
    /// Logs errors if the refresh process fails.
    /// </summary>
    /// <param name="account">The account to refresh.</param>
    ///  <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    [RelayCommand]
    private async Task RefreshAccountBtnAsync(Account account, CancellationToken cancellationToken = default)
    {
        if (!account.CanExpire || string.IsNullOrEmpty(account.GetAccessToken()))
            return;

        if (MicrosoftAuthService.AuthStatus != EAuthStatus.NONE)
            return;

        if (account.AccessTokenExpireDate > DateTime.Now)
            return;

        if (!await MicrosoftAuthService.RefreshLoginAsync(account.GetRefreshToken(), cancellationToken))
        {
            _logger.Error($"Failed to refresh account {account.DisplayName} ({account.Id}).");
            return;
        }

        if (MicrosoftAuthService.Account == null)
        {
            _logger.Error($"Failed to refresh account {account.DisplayName} ({account.Id}) after successful api call.");
            return;
        }

        var updatedAccount = MicrosoftAuthService.Account;
        updatedAccount.Id = account.Id; // Ensure the ID remains the same
        _logger.Info($"Successfully refreshed account {account.DisplayName} ({account.Id}).");

        AccountData accountData = await LauncherHelper.GetAccountDataAsync(cancellationToken);
        var index = accountData.Accounts.FindIndex(x => x.Id == account.Id);
        accountData.Accounts[index] = updatedAccount;

        await JsonHelper.WriteJsonFileAsync(PathHelper.LauncherAccountsPath, accountData, cancellationToken);
        GlobalEvents.InvokeAccountsChanged();
        MicrosoftAuthService.Reset();
    }

    /// <summary>
    /// Removes the specified account from the account list.
    /// If the removed account is the currently selected account, updates the selected account to the next available one.
    /// </summary>
    /// <param name="account">The account to remove.</param>
    [RelayCommand]
    private void RemoveAccountBtn(Account account)
    {
        AccountData.Accounts.Remove(account);
        if (account.Id != AccountData.SelectedAccountId)
            return;

        AccountData.SelectedAccountId = AccountData.HasAccounts ? AccountData.Accounts.FirstOrDefault()?.Id : null;
    }

    #endregion

    #region Skins

    /// <summary>
    /// Imports a custom skin image from disk, stores it in the selected account's cache directory,
    /// and generates derived preview/head images for the launcher UI.
    /// </summary>
    /// <param name="cancellationToken">A token used to cancel the upload operation.</param>
    [RelayCommand]
    private async Task SkinUpload(CancellationToken  cancellationToken = default)
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

            string? filePath = await _parent.OpenImagePickerInteraction.Handle(Unit.Default);
            if (filePath == null || !File.Exists(filePath))
                return;

            string skinId = Guid.NewGuid().ToString();
            var settings = await LauncherHelper.GetLauncherSettingsAsync(cancellationToken: cancellationToken);
            string skinDir = Path.Combine(settings.Launcher.CacheDirectoryPath, "skins", SelectedAccount.Id, skinId);
            if (!Directory.Exists(skinDir))
                Directory.CreateDirectory(skinDir);
            string skinPath = Path.Combine(skinDir, "texture.png");
            File.Copy(filePath, skinPath, true);
            int accountIndex = AccountData.Accounts.IndexOf(SelectedAccount);
            SelectedAccount.Skins.Add(new AccountSkin(skinId, IsAccountHasWideModel ? "classic" : "slim", SelectedSkin.CapeId));
            AccountData.Accounts[accountIndex] = SelectedAccount;
           
            string previewPath = Path.Combine(skinDir, "preview.png");
            string headshotPath = Path.Combine(skinDir, "head.png");
            
            await using var skinStream = File.OpenRead(skinPath);
            using var skinBitmap = SKBitmap.Decode(skinStream);
            Skin3DHeadTypeB.MakeHeadImage(skinBitmap, 15, 65).SavePng(headshotPath);
            Skin2DTypeB.MakeSkinImage(skinBitmap, IsAccountHasWideModel ? SkinType.New : SkinType.NewSlim).SavePng(previewPath);
            
            OnAccountUpdated();
        }
        catch (Exception ex)
        {
            _logger.Error("Error while uploading skin: " + ex);
        }
        finally
        {
            IsAccountSkinProcessing = false;
        }
    }

    /// <summary>
    /// Applies the selected existing skin to the current account by uploading the cached skin
    /// texture to Mojang and syncing the returned profile data back into the launcher state.
    /// </summary>
    /// <param name="model">The skin entry selected by the user.</param>
    /// <param name="cancellationToken">A token used to cancel the skin selection operation.</param>
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
            
            var settings = await LauncherHelper.GetLauncherSettingsAsync(cancellationToken: cancellationToken);
            string skinPath = Path.Combine(settings.Launcher.CacheDirectoryPath, "skins", SelectedAccount.Id, model.Id, "texture.png");
            if (!File.Exists(skinPath))
            {
                _logger.Error("Skin file does not exist: " + skinPath);
                return;
            }
            
            MojangProfile? profile = await MojangSkinService.UploadSkin(SelectedAccount.GetAccessToken(), model.Variant, skinPath, cancellationToken);
            if (profile == null)
            {
                await _parent.ShowAlertDialogInteraction.Handle(new Alert(TranslationManager.Translate("common.error"), TranslationManager.Translate("main.page.skins.alert.error"), EAlertType.Error));
                return;
            }

            var newSkin = SelectedAccount.Skins.Find(x => x.Id == model.Id);
            if (SelectedSkin.CapeId != newSkin?.CapeId && newSkin is { CapeId: not null })
            {
                profile = await MojangSkinService.ShowCape(SelectedAccount.GetAccessToken(), newSkin.CapeId, cancellationToken);
                if (profile == null)
                {
                    await _parent.ShowAlertDialogInteraction.Handle(new Alert(TranslationManager.Translate("common.error"), TranslationManager.Translate("main.page.skins.alert.cape.change"), EAlertType.Error));
                    return;
                }
            }
            
            int accountIndex = AccountData.Accounts.IndexOf(SelectedAccount);
            int skinIndex = SelectedAccount.Skins.FindIndex(x => x.Id == model.Id);
            if (skinIndex >= 0)
            {
                SelectedAccount.Skins[skinIndex].MojangId = profile.Skins[0].Id;
            }

            SelectedAccount.MojangProfile = profile;
            AccountData.Accounts[accountIndex] = SelectedAccount;
            IsAccountHasWideModel = model.Variant.Equals("classic", StringComparison.InvariantCultureIgnoreCase);
            OnAccountUpdated();
        }
        catch (Exception ex)
        {
            _logger.Error("Error while selecting skin: " + ex);
        }
        finally
        {
            IsAccountSkinProcessing = false;
        }
    }

    /// <summary>
    /// Changes the currently selected cape for the active account and syncs the new cape state
    /// back to Mojang and the launcher collections.
    /// </summary>
    /// <param name="model">The cape entry selected by the user.</param>
    /// <param name="cancellationToken">A token used to cancel the cape selection operation.</param>
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
            if (model.Id.Equals("none", StringComparison.InvariantCultureIgnoreCase))
                result = await MojangSkinService.HideCape(SelectedAccount.GetAccessToken(), cancellationToken);
            else
                result = await MojangSkinService.ShowCape(SelectedAccount.GetAccessToken(), model.Id, cancellationToken);
            
            if (result == null)
            {
                await _parent.ShowAlertDialogInteraction.Handle(new Alert(TranslationManager.Translate("common.error"), TranslationManager.Translate("main.page.skins.alert.cape.change"), EAlertType.Error));
                foreach (CapeDataModel cape in Capes.ToList())
                {
                    int index = Capes.IndexOf(cape);
                    Capes.RemoveAt(index);
                    cape.IsSelected = cape.Id == SelectedSkin.CapeId && SelectedSkin.CapeId != null;
                    Capes.Insert(index, cape);
                }
                return;
            }

            foreach (CapeDataModel cape in Capes.ToList())
            {
                int index = Capes.IndexOf(cape);
                Capes.RemoveAt(index);
                cape.IsSelected = cape.Id == SelectedSkin.CapeId && SelectedSkin.CapeId != null;
                Capes.Insert(index, cape);
            }

            int accountIndex = AccountData.Accounts.IndexOf(SelectedAccount);
            int skinIndex = SelectedAccount.Skins.FindIndex(x => x.Id == SelectedSkin.Id);
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
            _logger.Error("Error while selecting cape: " + ex);
            await _parent.ShowAlertDialogInteraction.Handle(new Alert(TranslationManager.Translate("common.error"), TranslationManager.Translate("main.page.skins.alert.cape.unexpected"), EAlertType.Error));
        }
        finally
        {
            IsAccountSkinProcessing = false;
        }
    }
    
    /// <summary>
    /// Changes the current skin model variant (classic or slim) for the active Mojang profile,
    /// updates the launcher state, and keeps the selected skin/model in sync.
    /// </summary>
    /// <param name="model">The skin model name to apply. Expected values are typically <c>"classic"</c> or non-classic variants.</param>
    /// <param name="cancellationToken">A token used to cancel the model change operation.</param>
    [RelayCommand]
    private async Task ModelSelect(string model, CancellationToken cancellationToken = default)
    {
        try
        {
            bool newValue = model == "classic";
            // Prevent re-selecting the same model
            if (newValue == IsAccountHasWideModel || IsAccountSkinProcessing)
            {
                _logger.Debug("Skipped re-selecting the same skin model.");
                return;
            }
            IsAccountSkinProcessing = true;
            
            // Ensure an account is selected
            if (SelectedAccount == null)
            {
                _logger.Debug("No account selected while selecting skin model.");
                return;
            }
            
            Skin? skin = SelectedAccount.MojangProfile?.Skins.FirstOrDefault(x => x.Id == SelectedSkin.MojangId);
            if (skin == null)
            {
                _logger.Debug("No skin found while selecting skin model.");
                return;
            }

            MojangProfile? result = await MojangSkinService.ChangeSkin(SelectedAccount.GetAccessToken(), model, skin.Url, cancellationToken);
            if (result == null)
            {
                await _parent.ShowAlertDialogInteraction.Handle(new Alert(TranslationManager.Translate("common.error"), TranslationManager.Translate("main.page.skins.alert.model.change"), EAlertType.Error));
                return;
            }

            int accountIndex = AccountData.Accounts.IndexOf(SelectedAccount);
            SelectedAccount.MojangProfile = result;
            int skinIndex = SelectedAccount.Skins.FindIndex(x => x.Id == SelectedSkin.Id);
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
             
             
            var settings = await LauncherHelper.GetLauncherSettingsAsync(cancellationToken: cancellationToken);
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
                _logger.Error("Failed to load skin image: " + ex);
                return;
            }

            var oldSkin = AccountSkinPreview;
            AccountSkinPreview = img;
            oldSkin?.Dispose();*/
        }
        catch (Exception ex)
        {
            _logger.Error("Error while selecting skin model: " + ex);
        }
        finally
        {
            IsAccountSkinProcessing = false;
        }
    }

    #endregion

    #endregion

    #region Account functions

    /// <summary>
    /// Updates the account data by fetching the latest data from the launcher helper.
    /// </summary>
    private void OnAccountUpdated() => Dispatcher.UIThread.Invoke(async () => await HandleAccountUpdatedAsync());

    /// <summary>
    /// Reloads the currently selected account and its related skin/cape preview data from disk,
    /// then rebuilds the UI-bound collections and avatar image.
    /// </summary>
    /// <param name="cancellationToken">A token used to cancel the account refresh operation.</param>
    private async Task HandleAccountUpdatedAsync(CancellationToken cancellationToken = default)
    {
        var accountData = await LauncherHelper.GetAccountDataAsync(cancellationToken);
        AccountData = new AccountDataModel(accountData);
        Account? selectedAccount = accountData.Accounts.FirstOrDefault(x => x.Id == AccountData.SelectedAccountId);

        if (SelectedAccount != selectedAccount)
        {
            List<SkinDataModel> skinCopies = Skins.ToList();
            foreach (SkinDataModel skin in skinCopies)
                skin.Dispose();
            Skins.Clear();
            List<CapeDataModel> capeCopies = Capes.ToList();
            foreach (CapeDataModel cape in capeCopies)
                cape.Dispose();
            Capes.Clear();
        }

        SelectedAccount = selectedAccount;
        if (SelectedAccount?.MojangProfile != null)
        {
            var settings = await LauncherHelper.GetLauncherSettingsAsync(cancellationToken: cancellationToken);
            string skinsDir = Path.Combine(settings.Launcher.CacheDirectoryPath, "skins");
            string capesDir = Path.Combine(settings.Launcher.CacheDirectoryPath, "capes");

            var activeSkin = SelectedAccount.MojangProfile.Skins.FirstOrDefault(x =>
                x.State.Equals("active", StringComparison.InvariantCultureIgnoreCase));

            foreach (AccountSkin skin in SelectedAccount.Skins)
            {
                string path = Path.Combine(skinsDir, SelectedAccount.Id, skin.Id, "preview.png");
                Bitmap? img = null;
                try
                {
                    img = new Bitmap(path);
                }
                catch (Exception ex)
                {
                    _logger.Error("Failed to load skin image: " + ex);
                }

                bool isActive = skin.MojangId == activeSkin?.Id && activeSkin != null;
                Skins.Add(new SkinDataModel(skin.Id, skin.Model, img, isActive));
                if (isActive)
                {
                    IsAccountHasWideModel = skin.Model.Equals("classic", StringComparison.InvariantCultureIgnoreCase);
                    SelectedSkin = skin;
                    AccountSkinPreview = img;
                }
            }


            bool hadActiveCape = false;
            Bitmap? noCapeImg = null;
            try
            {
                noCapeImg = ImageHelper.LoadFromResource(
                    new Uri("avares://Desktop/Assets/Images/placeholders/no_cape.png"));
            }
            catch (Exception ex)
            {
                _logger.Error("Failed to load cape image: " + ex);
            }

            Capes.Add(new CapeDataModel("none", "None", noCapeImg, false));
            foreach (Cape cape in SelectedAccount.MojangProfile.Capes)
            {
                string path = Path.Combine(capesDir, $"{cape.Id}.png");
                Bitmap? img = null;
                try
                {
                    img = new Bitmap(path);
                }
                catch (Exception ex)
                {
                    _logger.Error("Failed to load cape image: " + ex);
                }

                bool isActive = cape.State.Equals("active", StringComparison.InvariantCultureIgnoreCase);
                Capes.Add(new CapeDataModel(cape.Id, cape.Alias, img, isActive));
                if (isActive)
                    hadActiveCape = true;
            }

            if (!hadActiveCape)
                Capes[0].IsSelected = true;
        }

        await UpdateSelectedAccountAvatarAsync(cancellationToken);
    }

    /// <summary>
    /// Updates the avatar image for the currently selected account using cached skin assets,
    /// or falls back to a default placeholder image when no avatar can be resolved.
    /// </summary>
    /// <param name="cancellationToken">A token used to cancel the avatar refresh operation.</param>
    private async Task UpdateSelectedAccountAvatarAsync(CancellationToken cancellationToken = default)
    {
        AccountAvatar?.Dispose();
        var settings = await LauncherHelper.GetLauncherSettingsAsync(cancellationToken: cancellationToken);
        string skinsDir = Path.Combine(settings.Launcher.CacheDirectoryPath, "skins");
        string? avatarPath;
        if (SelectedAccount != null)
        {
            string avatarDir = Path.Combine(skinsDir, SelectedAccount.Id);
            if (!Directory.Exists(avatarDir))
                Directory.CreateDirectory(avatarDir);
            if (SelectedAccount.Type == EAccountType.OFFLINE)
                avatarPath = Path.Combine(avatarDir, "head.png");
            else
            {
                AccountSkin? selectedSkin =
                    SelectedAccount.Skins.FirstOrDefault(x =>
                        x.MojangId == SelectedAccount.MojangProfile?.Skins[0]?.Id);
                avatarPath = selectedSkin == null ? null : Path.Combine(avatarDir, selectedSkin.Id, "head.png");
            }
        }
        else
            avatarPath = null;
        
        AccountAvatar = File.Exists(avatarPath)
            ? new Bitmap(avatarPath)
            : ImageHelper.LoadFromResource(
                new Uri("avares://Desktop/Assets/Images/placeholders/steve_head.png"));
    }

    /// <summary>
    /// Subscribes to the PropertyChanged event of the provided AccountDataModel instance.
    /// </summary>
    /// <param name="accountData">The account data model to subscribe to.</param>
    private void SubscribeToAccountDataChildren(AccountDataModel accountData)
    {
        accountData.PropertyChanged += OnChildAccountDataPropertyChanged;
    }

    /// <summary>
    /// Unsubscribes from the PropertyChanged event of the provided AccountDataModel instance.
    /// </summary>
    /// <param name="accountData">The account data model to unsubscribe from.</param>
    private void UnsubscribeFromAccountDataChildren(AccountDataModel accountData)
    {
        accountData.PropertyChanged -= OnChildAccountDataPropertyChanged;
    }

    /// <summary>
    /// Handles changes to the AccountData property, unsubscribing from the old value and subscribing to the new value.
    /// Saves the new account data to a file if the view model is initialized.
    /// </summary>
    /// <param name="oldValue">The previous AccountDataModel instance.</param>
    /// <param name="newValue">The new AccountDataModel instance.</param>
    partial void OnAccountDataChanged(AccountDataModel? oldValue, AccountDataModel newValue)
    {
        if (_parent.IsLoading || !_parent.Initialization.IsCompletedSuccessfully)
            return;
        
        _logger.Debug("AccountData changed with old and new value. Unsubscribing from old, subscribing to new.");

        if (oldValue != null)
            UnsubscribeFromAccountDataChildren(oldValue);

        SubscribeToAccountDataChildren(newValue);
        SaveAccountDataToFile(newValue);
    }

    /// <summary>
    /// Handles the PropertyChanged event for child properties of the AccountDataModel.
    /// Saves the updated account data to a file if the view model is initialized.
    /// </summary>
    /// <param name="sender">The object that triggered the event.</param>
    /// <param name="e">The event data containing the name of the changed property.</param>
    private void OnChildAccountDataPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (_parent.IsLoading || !_parent.Initialization.IsCompletedSuccessfully)
            return;
     
        if (sender is not AccountDataModel accountData)
            return;
        
        _logger.Debug($"Inner property '{e.PropertyName}' changed on {sender?.GetType().Name}. Saving to file...");
        SaveAccountDataToFile(accountData);
    }

    /// <summary>
    /// Saves the provided AccountDataModel instance to a file in JSON format.
    /// </summary>
    /// <param name="newValue">The AccountDataModel instance to save.</param>
    private void SaveAccountDataToFile(AccountDataModel newValue)
    {
        var accounts = new AccountData
        {
            SelectedAccountId = newValue.SelectedAccountId ?? string.Empty,
            Accounts = newValue.Accounts.Select(a =>
            {
                Account account = new()
                {
                    Id = a.Id,
                    Uuid = a.Uuid,
                    DisplayName = a.DisplayName,
                    Type = a.Type,
                    AccessTokenExpireDate = a.AccessTokenExpireDate,
                    MojangProfile = a.MojangProfile,
                    Skins = a.Skins,
                };
                account.SetAccessToken(a.GetAccessToken());
                account.SetRefreshToken(a.GetRefreshToken());
                return account;
            }).ToList()
        };

        JsonHelper.WriteJsonFile(PathHelper.LauncherAccountsPath, accounts);
    }

    #endregion
}