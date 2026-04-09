using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DynamicData;
using MinecraftSkinRender;
using MinecraftSkinRender.Image;
using ReactiveUI;
using SkiaSharp;
using Tavstal.KonkordLauncher.Common.Helpers;
using Tavstal.KonkordLauncher.Common.Models;
using Tavstal.KonkordLauncher.Common.Models.Config;
using Tavstal.KonkordLauncher.Common.Models.Translation;
using Tavstal.KonkordLauncher.Common.Translation;
using Tavstal.KonkordLauncher.Core.Enums;
using Tavstal.KonkordLauncher.Core.Helpers;
using Tavstal.KonkordLauncher.Core.Models;
using Tavstal.KonkordLauncher.Core.Models.MojangApi.User;
using Tavstal.KonkordLauncher.Core.Services;
using Tavstal.KonkordLauncher.Desktop.Models;
using Tavstal.KonkordLauncher.Desktop.Models.Avalonia;
using Tavstal.KonkordLauncher.Desktop.Models.Config.Launcher;
using Tavstal.KonkordLauncher.Desktop.Models.Enums;
using ImageHelper = Tavstal.KonkordLauncher.Desktop.Helpers.ImageHelper;

namespace Tavstal.KonkordLauncher.Desktop.Views.Models;

/// <summary>
/// Represents the main view model for the application, managing the state and behavior of the UI.
/// </summary>
public partial class MainViewModel : KonkordObservableObject
{
    private readonly bool _isInitialized;
    private readonly CoreLogger _logger = CoreLogger.WithModuleType(typeof(MainViewModel));
    public bool IsLinux => OSHelper.GetOperatingSystem() == EOperatingSystem.Linux;

    #region Interactions

    public Interaction<Unit, Unit> MinimizeWindowInteraction { get; } = new();
    public Interaction<Unit, Unit> MaximizeWindowInteraction { get; } = new();
    public Interaction<Unit, Unit> CloseWindowInteraction { get; } = new();
    public Interaction<Alert, Unit> ShowAlertDialog { get; } = new();
    public Interaction<Alert, bool> ShowConfirmDialog { get; } = new();
    public Interaction<ESidebarType, Unit> UpdateSidebarButton { get; } = new();
    public Interaction<Unit, string?> OpenFolderPicker { get; } = new();
    public Interaction<Unit, string?> OpenImagePicker { get; } = new();
    public Interaction<Unit, Unit> ShowInstanceCreationDialog { get; } = new();
    public Interaction<string, Unit> ShowInstanceEditDialog { get; } = new();
    public Interaction<Unit, Unit> ShowAccountsDialog { get; } = new();
    public Interaction<Unit, JavaVersionModel> ShowJavaSelectorDialog { get; } = new();
    public Interaction<string, Unit> ShowLogsWindow { get; } = new();
    public Interaction<string, Unit> CloseLogsWindow { get; } = new();
    public Interaction<string, string?> ShowTextInputDialog { get; } = new();
    public Interaction<Unit, string?> ShowIconSelectorDialog { get; } = new();
    public Interaction<ESettingsTab, Unit> UpdateSettingsTabButton { get; } = new();
    #endregion

    [ObservableProperty] private ESidebarType _currentPageIndex;
    [ObservableProperty] private ESettingsTab _currentSettingsTab;
    private readonly SourceCache<InstanceModel, string> _instanceCache = new(x => x.Id);
    private readonly SourceCache<InstanceGroup, string> _groupCache = new(x => x.GroupName);

    public ReadOnlyObservableCollection<InstanceGroup> InstanceGroups { get; }
    [ObservableProperty] private bool _hasInstances;

    private readonly SourceCache<PatchNote, string> _patchCache = new(x => x.Title);
    public ReadOnlyObservableCollection<PatchNote> Patches { get; }
    [ObservableProperty] private bool _hasPatches;

    [ObservableProperty] private AccountDataModel _accountData;

    [ObservableProperty] [NotifyPropertyChangedFor(nameof(AccountName))] [NotifyPropertyChangedFor(nameof(IsMojangAccount))]
    private Account? _selectedAccount;

    public string AccountName => SelectedAccount != null
        ? SelectedAccount.DisplayName
        : TranslationManager.Translate("main.sidebar.accounts.guest");
    
    public bool IsMojangAccount => SelectedAccount is { Type: EAccountType.MICROSOFT };
    
    private readonly SourceCache<SkinDataModel, string> _skinsCache = new(x => x.Id);
    public ReadOnlyObservableCollection<SkinDataModel> Skins { get; }
    private readonly SourceCache<CapeDataModel, string> _capesCache = new(x => x.Id);
    public ReadOnlyObservableCollection<CapeDataModel> Capes { get; }
    [ObservableProperty] private Bitmap _accountAvatar;
    [ObservableProperty] private Bitmap? _accountSkinPreview;
    [ObservableProperty] private bool _isAccountHasWideModel;
    [ObservableProperty] private bool _isAccountSkinProcessing;
    [ObservableProperty] private CoreConfigModel _coreConfig;
    [ObservableProperty] private AccountSkin _selectedSkin;
    
    #region About Us Properties

    public string Version => App.Version;
    public string Branch => App.Branch;
    public string BuildDate => App.BuildDate;
    [ObservableProperty] private string _license;
    
    #endregion

    /// <summary>
    /// Initializes a new instance of the <see cref="MainViewModel"/> class.
    /// </summary>
    public MainViewModel()
    {
        _currentPageIndex = ESidebarType.Play;
        _currentSettingsTab = ESettingsTab.LAUNCHER;
        _coreConfig = new CoreConfigModel(LauncherHelper.GetLauncherSettings());
        _accountData = new AccountDataModel(LauncherHelper.GetAccountData());

        #region Instances

        var groupDisposer = _groupCache.Connect()
            .ObserveOn(RxApp.MainThreadScheduler)
            .Bind(out var instanceGroups)
            .Subscribe();
        Disposables.Add(groupDisposer);
        InstanceGroups = instanceGroups;

        string uncategorized = TranslationManager.Translate("main.page.play.uncategorized");

        // Then watch for instance changes and update groups manually
        var instanceDisposer = _instanceCache.Connect()
            .Subscribe(changes =>
            {
                _logger.Debug($"Processing {changes.Count} instance changes");
        
                // Get all unique groups
                var allGroups = _instanceCache.Items
                    .Select(x => x.Group ?? uncategorized)
                    .Distinct()
                    .ToList();
        
                _logger.Debug($"Found {allGroups.Count} groups");
        
                // Update group cache
                _groupCache.Edit(groupCache =>
                {
                    // Remove groups that no longer exist
                    var groupsToRemove = groupCache.Keys.Except(allGroups).ToList();
                    foreach (var group in groupsToRemove)
                    {
                        groupCache.Remove(group);
                    }
            
                    // Add or update groups
                    foreach (var groupName in allGroups)
                    {
                        if (!groupCache.Lookup(groupName).HasValue)
                        {
                            groupCache.AddOrUpdate(new InstanceGroup(groupName));
                        }
                
                        var group = groupCache.Lookup(groupName).Value;
                        var instancesInGroup = _instanceCache.Items
                            .Where(x => (x.Group ?? uncategorized) == groupName)
                            .ToList();
                
                        // Update instances in group
                        Dispatcher.UIThread.Post(() =>
                        {
                            group.Instances.Clear();
                            foreach (var instance in instancesInGroup)
                            {
                                group.Instances.Add(instance);
                            }
                        });
                    }
                });
            });
        Disposables.Add(instanceDisposer);

        var instanceCountDisposer = _instanceCache.CountChanged
            .Select(count => count > 0)
            .ObserveOn(RxApp.MainThreadScheduler)
            .BindTo(this, x => x.HasInstances);
        Disposables.Add(instanceCountDisposer);

        var newInstances = LauncherHelper.GetInstances().ConvertAll(x => new InstanceModel(x));
        _instanceCache.Edit(innerCache =>
        {
            innerCache.Clear();
            innerCache.AddOrUpdate(newInstances);
        });
        _logger.Debug("Initialized instance cache");

        #endregion

        #region Patches

        var patchesDisposer = _patchCache.Connect()
            .Bind(out var patches)
            .Subscribe();
        Disposables.Add(patchesDisposer);
        Patches = patches;

        var patchesCountDisposer = _patchCache.CountChanged
            .Select(count => count > 0)
            .ObserveOn(RxApp.MainThreadScheduler)
            .BindTo(this, x => x.HasPatches);
        Disposables.Add(patchesCountDisposer);

        var newPatches = LauncherHelper.GetPatchNotes(_coreConfig.Launcher.CacheDirectoryPath);
        _patchCache.Edit(innerCache =>
        {
            innerCache.Clear();
            innerCache.AddOrUpdate(newPatches);
        });

        #endregion
        
        #region Skins
        var skinsDisposer = _skinsCache.Connect()
            .Bind(out var skins)
            .Subscribe();
        Disposables.Add(skinsDisposer);
        Skins = skins;
        
        var capesDisposer = _capesCache.Connect()
            .Bind(out var capes)
            .Subscribe();
        Disposables.Add(capesDisposer);
        Capes = capes;
        #endregion
        
        Account? selectedAccount = AccountData.Accounts.FirstOrDefault(x => x.Id == AccountData.SelectedAccountId);
        _selectedAccount = selectedAccount;
        OnAccountUpdated();
        
        // Load LICENSE
        var assembly = Assembly.GetExecutingAssembly(); 
        using var stream = assembly.GetManifestResourceStream("Tavstal.KonkordLauncher.Desktop.Assets.LICENSE");
        using var reader = new StreamReader(stream!);
        _license = Regex.Replace(reader.ReadToEnd().Trim(), @" {3,}", " ");

        _isInitialized = true;
        SubscribeToCoreConfigChildren(_coreConfig);
        SubscribeToAccountDataChildren(_accountData);
        GlobalEvents.OnAccountsChanged += OnAccountUpdated;
        GlobalEvents.OnInstancesChanged += HandleInstancesChanged;
    }

    /// <summary>
    /// Releases the resources used by the MainViewModel and unsubscribes from global events.
    /// </summary>
    /// <param name="disposing">
    /// A boolean value indicating whether the method is being called directly or indirectly by a finalizer.
    /// If true, the method has been called directly or indirectly by a user's code. Managed and unmanaged resources can be disposed.
    /// If false, the method has been called by the runtime from inside the finalizer, and only unmanaged resources can be disposed.
    /// </param>
    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        GlobalEvents.OnAccountsChanged -= OnAccountUpdated;
        GlobalEvents.OnInstancesChanged -= HandleInstancesChanged;
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

    #region Sidebar Management

    /// <summary>
    /// Handles the sidebar button click event by changing the current sidebar view.
    /// </summary>
    /// <param name="sidebarType">The type of sidebar to switch to.</param>
    [RelayCommand]
    public async Task HandleSidebarBtn(ESidebarType sidebarType) => await UpdateSidebarButton.Handle(sidebarType);

    #endregion

    #region Instances Management

    /// <summary>
    /// Handles changes to the instances collection by updating the cache with the latest instances data.
    /// Clears the existing cache and adds or updates it with the new instances retrieved from the launcher helper.
    /// </summary>
    private void HandleInstancesChanged()
    {
        _logger.Debug("Instances data changed. Updating instances collection.");
        var newInstances = LauncherHelper.GetInstances().ConvertAll(x => new InstanceModel(x));
        _instanceCache.Edit(innerCache =>
        {
            innerCache.Clear();
            innerCache.AddOrUpdate(newInstances);
        });
    }

    #region Commands

    /// <summary>
    /// Opens the "Create Instance" window to allow the user to add a new Minecraft instance asynchronously.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [RelayCommand]
    private async Task AddInstanceBtnAsync()
    {
        await ShowInstanceCreationDialog.Handle(Unit.Default);
    }

    /// <summary>
    /// Launches the specified Minecraft instance asynchronously.
    /// </summary>
    /// <param name="instance">The instance model representing the Minecraft instance to launch.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [RelayCommand]
    private async Task LaunchInstance(InstanceModel? instance)
    {
        if (instance == null)
            return;
        await instance.LaunchAsync(ShowLogsWindow, CloseLogsWindow, CloseWindowInteraction, ShowAlertDialog);
    }

    /// <summary>
    /// Stops the specified Minecraft instance if it is currently running.
    /// </summary>
    /// <param name="instance">The instance model representing the Minecraft instance to stop.</param>
    [RelayCommand]
    private void StopInstance(InstanceModel? instance)
    {
        if (instance == null)
            return;

        if (!instance.IsGameRunning || instance.GameProcess == null)
        {
            _logger.Warn($"Instance {instance.Name} is not running or has no associated process.");
            return;
        }

        instance.GameProcess.Kill();
    }

    /// <summary>
    /// Opens an edit window for the specified Minecraft instance and updates the instance in the collection if changes are made.
    /// </summary>
    /// <param name="instance">The instance model representing the Minecraft instance to edit.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [RelayCommand]
    private async Task EditInstance(InstanceModel? instance)
    {
        if (instance == null)
            return;
        await ShowInstanceEditDialog.Handle(instance.Id);
    }

    /// <summary>
    /// Displays the logs of the specified Minecraft instance in a separate window asynchronously.
    /// </summary>
    /// <param name="instance">The instance model representing the Minecraft instance whose logs are to be viewed.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [RelayCommand]
    private async Task ViewInstanceLogs(InstanceModel? instance)
    {
        if (instance == null)
            return;

        await ShowLogsWindow.Handle(instance.Id);
    }

    /// <summary>
    /// Renames the specified Minecraft instance asynchronously.
    /// Prompts the user for a new name, validates it, and updates the instance if valid.
    /// </summary>
    /// <param name="instance">The instance model representing the Minecraft instance to rename.</param>
    [RelayCommand]
    private async Task RenameInstance(InstanceModel? instance)
    {
        if (instance == null)
            return;

        var instances = await LauncherHelper.GetInstancesAsync();
        var targetInstance = instances.FirstOrDefault(i => i.Id == instance.Id);
        if (targetInstance == null)
            return;

        int index = instances.IndexOf(targetInstance);
        var result = await ShowTextInputDialog.Handle(TranslationManager.Translate("instance.rename.title"));
        if (string.IsNullOrEmpty(result))
            return;

        if (instances.Any(x => x.Name.Equals(result, StringComparison.OrdinalIgnoreCase)))
        {
            await ShowAlertDialog.Handle(new Alert(TranslationManager.Translate("common.error"),
                TranslationManager.Translate("instance.rename.duplicate"), EAlertType.Error));
            return;
        }

        targetInstance.Name = result;
        instances[index] = targetInstance;
        await JsonHelper.WriteJsonFileAsync(PathHelper.LauncherInstancesPath, instances);
        GlobalEvents.InvokeInstancesChanged();
    }

    /// <summary>
    /// Changes the icon of the specified Minecraft instance asynchronously.
    /// Opens an icon selector dialog, validates the selection, and updates the instance if valid.
    /// </summary>
    /// <param name="instance">The instance model representing the Minecraft instance to update the icon for.</param>
    [RelayCommand]
    private async Task ChangeInstanceIcon(InstanceModel? instance)
    {
        if (instance == null)
            return;

        var instances = await LauncherHelper.GetInstancesAsync();
        var targetInstance = instances.FirstOrDefault(i => i.Id == instance.Id);
        if (targetInstance == null)
            return;

        int index = instances.IndexOf(targetInstance);
        var result = await ShowIconSelectorDialog.Handle(Unit.Default);
        if (string.IsNullOrEmpty(result))
            return;

        targetInstance.IconPath = result;
        instances[index] = targetInstance;
        await JsonHelper.WriteJsonFileAsync(PathHelper.LauncherInstancesPath, instances);
        GlobalEvents.InvokeInstancesChanged();
    }

    /// <summary>
    /// Changes the group of the specified Minecraft instance asynchronously.
    /// Prompts the user for a new group name, validates it, and updates the instance if valid.
    /// </summary>
    /// <param name="instance">The instance model representing the Minecraft instance to update the group for.</param>
    [RelayCommand]
    private async Task ChangeInstanceGroup(InstanceModel? instance)
    {
        if (instance == null)
            return;

        var instances = await LauncherHelper.GetInstancesAsync();
        var targetInstance = instances.FirstOrDefault(i => i.Id == instance.Id);
        if (targetInstance == null)
            return;

        int index = instances.IndexOf(targetInstance);
        var result = await ShowTextInputDialog.Handle(TranslationManager.Translate("instance.change.group.title"));
        if (string.IsNullOrEmpty(result))
            return;

        targetInstance.Group = result;
        instances[index] = targetInstance;
        await JsonHelper.WriteJsonFileAsync(PathHelper.LauncherInstancesPath, instances);
        GlobalEvents.InvokeInstancesChanged();
    }

    /// <summary>
    /// Opens the directory of the specified Minecraft instance in the file explorer.
    /// </summary>
    /// <param name="instance">The instance model representing the Minecraft instance whose directory is to be opened.</param>
    [RelayCommand]
    private void OpenInstanceDir(InstanceModel? instance)
    {
        if (instance == null)
            return;

        if (string.IsNullOrEmpty(instance.GameDirectory))
            return;

        FileSystemHelper.OpenFolderInFileExplorer(instance.GameDirectory);
    }

    /// <summary>
    /// Exports the specified Minecraft instance in the Konkord format asynchronously.
    /// </summary>
    /// <param name="instance">The instance model representing the Minecraft instance to export.</param>
    [RelayCommand]
    private async Task ExportNativeInstance(InstanceModel? instance)
    {
        if (instance == null)
            return;

        var directoryResult = await OpenFolderPicker.Handle(Unit.Default);
        if (string.IsNullOrEmpty(directoryResult))
            return;

        string exportPath = Path.Combine(directoryResult, instance.Name + "-konkord.zip");
        await InstanceHelper.ExportAsync(instance.getInstance(), exportPath, EInstanceProvider.Konkord);
    }

    /// <summary>
    /// Exports the specified Minecraft instance in the Modrinth format asynchronously.
    /// </summary>
    /// <param name="instance">The instance model representing the Minecraft instance to export.</param>
    [RelayCommand]
    private async Task ExportModrinthInstance(InstanceModel? instance)
    {
        if (instance == null)
            return;

        var directoryResult = await OpenFolderPicker.Handle(Unit.Default);
        if (string.IsNullOrEmpty(directoryResult))
            return;

        string exportPath = Path.Combine(directoryResult, instance.Name + "-modrinth.mrpack");

        await InstanceHelper.ExportAsync(instance.getInstance(), exportPath, EInstanceProvider.Modrinth);
    }

    /// <summary>
    /// Exports the specified Minecraft instance in the CurseForge format asynchronously.
    /// </summary>
    /// <param name="instance">The instance model representing the Minecraft instance to export.</param>
    [RelayCommand]
    private async Task ExportCurseForgeInstance(InstanceModel? instance)
    {
        if (instance == null)
            return;

        var directoryResult = await OpenFolderPicker.Handle(Unit.Default);
        if (string.IsNullOrEmpty(directoryResult))
            return;

        string exportPath = Path.Combine(directoryResult, instance.Name + "-curseforge.zip");

        await InstanceHelper.ExportAsync(instance.getInstance(), exportPath, EInstanceProvider.CurseForge);
    }

    /// <summary>
    /// Deletes the specified Minecraft instance asynchronously.
    /// Prompts the user for confirmation before proceeding with the deletion.
    /// If confirmed, removes the instance from the list and deletes its associated directory.
    /// </summary>
    /// <param name="instance">The instance model representing the Minecraft instance to delete.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [RelayCommand]
    private async Task DeleteInstance(InstanceModel? instance)
    {
        if (instance == null)
            return;

        var result = await ShowConfirmDialog.Handle(new Alert(TranslationManager.Translate("instance.delete.title"),
            TranslationManager.Translate("instance.delete.message", instance.Name), EAlertType.Confirm));
        if (!result)
            return;

        var instances = await LauncherHelper.GetInstancesAsync();
        var targetInstance = instances.FirstOrDefault(i => i.Id == instance.Id);
        if (targetInstance == null)
            return;

        if (string.IsNullOrEmpty(targetInstance.GameDirectory))
            return;

        if (Directory.Exists(targetInstance.GameDirectory))
            FileSystemHelper.DeleteDirectory(targetInstance.GameDirectory);
        instances.Remove(targetInstance);
        await JsonHelper.WriteJsonFileAsync(PathHelper.LauncherInstancesPath, instances);
        GlobalEvents.InvokeInstancesChanged();
    }

    #endregion

    #endregion

    #region Account Management

    #region Commands

    /// <summary>
    /// Opens the account management window to add a new account asynchronously.
    /// </summary>
    [RelayCommand]
    private async Task AddAccountBtnAsync()
    {
        await ShowAccountsDialog.Handle(Unit.Default);
    }

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
    [RelayCommand]
    private async Task RefreshAccountBtnAsync(Account account)
    {
        if (!account.CanExpire || string.IsNullOrEmpty(account.GetAccessToken()))
            return;

        if (MicrosoftAuthService.AuthStatus != EAuthStatus.NONE)
            return;

        if (account.AccessTokenExpireDate > DateTime.Now)
            return;

        if (!await MicrosoftAuthService.RefreshLoginAsync(account.GetRefreshToken()))
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

        AccountData accountData = await LauncherHelper.GetAccountDataAsync();
        var index = accountData.Accounts.FindIndex(x => x.Id == account.Id);
        accountData.Accounts[index] = updatedAccount;

        await JsonHelper.WriteJsonFileAsync(PathHelper.LauncherAccountsPath, accountData);
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

    #region Account Operations

    /// <summary>
    /// Updates the account data by fetching the latest data from the launcher helper.
    /// </summary>
    private void OnAccountUpdated()
    {
        Dispatcher.UIThread.Invoke(async () =>
        {
            AccountData = new AccountDataModel(await LauncherHelper.GetAccountDataAsync());
            Account? selectedAccount = AccountData.Accounts.FirstOrDefault(x => x.Id == AccountData.SelectedAccountId);
            
            if (SelectedAccount != selectedAccount)
            {
                List<SkinDataModel> skinCopies = Skins.ToList();
                foreach (SkinDataModel skin in skinCopies)
                    skin.Dispose();
                _skinsCache.Edit(innerCache =>
                {
                    innerCache.Clear();
                });
                List<CapeDataModel> capeCopies = Capes.ToList();
                foreach (CapeDataModel cape in capeCopies)
                    cape.Dispose();
                _capesCache.Edit(innerCache =>
                {
                    innerCache.Clear();
                });
            }
            SelectedAccount = selectedAccount;
            if (SelectedAccount?.MojangProfile != null)
            {
                var settings = await LauncherHelper.GetLauncherSettingsAsync();
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
                        img = new  Bitmap(path);
                    }
                    catch (Exception ex)
                    {
                        _logger.Error("Failed to load skin image: " + ex);
                    }

                    bool isActive = skin.MojangId == activeSkin?.Id && activeSkin != null;
                    _skinsCache.Edit(innerCache =>
                    {
                        innerCache.AddOrUpdate(new  SkinDataModel(skin.Id, skin.Model, img, isActive));
                    });
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
                    noCapeImg = ImageHelper.LoadFromResource(new Uri("avares://Desktop/Assets/Images/placeholders/no_cape.png"));
                }
                catch (Exception ex)
                {
                    _logger.Error("Failed to load cape image: " + ex);
                }
                _capesCache.Edit(innerCache =>
                {
                    innerCache.AddOrUpdate(new CapeDataModel("none", "None", noCapeImg, false));
                });
                foreach (Cape cape in SelectedAccount.MojangProfile.Capes)
                {
                    string path = Path.Combine(capesDir, $"{cape.Id}.png");
                    Bitmap? img = null;
                    try
                    {
                        img = new  Bitmap(path);
                    }
                    catch (Exception ex)
                    {
                        _logger.Error("Failed to load cape image: " + ex);
                    }
                    bool isActive = cape.State.Equals("active", StringComparison.InvariantCultureIgnoreCase);
                    _capesCache.Edit(innerCache =>
                    {
                        innerCache.AddOrUpdate(new CapeDataModel(cape.Id, cape.Alias, img,  isActive));
                    });
                    if (isActive)
                        hadActiveCape = true;
                }

                if (!hadActiveCape)
                    Capes[0].IsSelected = true;
            }
            UpdateSelectedAccountAvatar();
        });
    }
    
    private void UpdateSelectedAccountAvatar()
    {
        AccountAvatar?.Dispose();
        
        string skinsDir = Path.Combine(LauncherHelper.GetLauncherSettings().Launcher.CacheDirectoryPath, "skins");
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
        _logger.Debug("AccountData changed with old and new value. Unsubscribing from old, subscribing to new.");

        if (oldValue != null)
            UnsubscribeFromAccountDataChildren(oldValue);

        SubscribeToAccountDataChildren(newValue);

        if (!_isInitialized)
            return;
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
        if (!_isInitialized)
            return;
        _logger.Debug($"Inner property '{e.PropertyName}' changed on {sender?.GetType().Name}. Saving to file...");
        SaveAccountDataToFile(AccountData);
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

    #endregion

    #region Config Management

    #region Commands

    [RelayCommand]
    public async Task HandleSettingsBtn(ESettingsTab tabType) => await UpdateSettingsTabButton.Handle(tabType);
    
    /// <summary>
    /// Opens a folder picker dialog to select a directory and updates the corresponding configuration path
    /// based on the provided index.
    /// </summary>
    /// <param name="rawIndex">
    /// The index representing the configuration path to update:
    /// <br/>0 - AssetsDirectoryPath,
    /// <br/>1 - CacheDirectoryPath,
    /// <br/>2 - InstancesDirectoryPath,
    /// <br/>3 - IconsDirectoryPath,
    /// <br/>4 - JavaDirectoryPath,
    /// <br/>5 - LibrariesDirectoryPath,
    /// <br/>6 - ManifestsDirectoryPath,
    /// <br/>7 - TranslationsDirectoryPath,
    /// <br/>8 - VersionsDirectoryPath,
    /// <br/>9 - DefaultJavaPath.
    /// </param>
    [RelayCommand]
    public async Task ConfigDirSelectAsync(string rawIndex)
    {
        if (!int.TryParse(rawIndex, out var index))
            return;

        var directoryResult = await OpenFolderPicker.Handle(Unit.Default);
        if (string.IsNullOrEmpty(directoryResult))
            return;
        switch (index)
        {
            case 0:
            {
                CoreConfig.Launcher.AssetsDirectoryPath = directoryResult;
                break;
            }
            case 1:
            {
                CoreConfig.Launcher.CacheDirectoryPath = directoryResult;
                break;
            }
            case 2:
            {
                CoreConfig.Launcher.InstancesDirectoryPath = directoryResult;
                break;
            }
            case 3:
            {
                CoreConfig.Launcher.IconsDirectoryPath = directoryResult;
                break;
            }
            case 4:
            {
                CoreConfig.Launcher.JavaDirectoryPath = directoryResult;
                break;
            }
            case 5:
            {
                CoreConfig.Launcher.LibrariesDirectoryPath = directoryResult;
                break;
            }
            case 6:
            {
                CoreConfig.Launcher.ManifestsDirectoryPath = directoryResult;
                break;
            }
            case 7:
            {
                CoreConfig.Launcher.TranslationsDirectoryPath = directoryResult;
                break;
            }
            case 8:
            {
                CoreConfig.Launcher.VersionsDirectoryPath = directoryResult;
                break;
            }
            case 9:
            {
                CoreConfig.Java.DefaultJavaPath = directoryResult;
                break;
            }
        }
    }

    /// <summary>
    /// Opens a Java selector window to choose a Java version and updates the default Java path
    /// in the configuration with the selected version's path.
    /// </summary>
    [RelayCommand]
    private async Task JavaPathSelectorAsync()
    {
        var javaVersion = await ShowJavaSelectorDialog.Handle(Unit.Default);
        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        if (javaVersion == null)
            return;
        CoreConfig.Java.DefaultJavaPath = javaVersion.Path;
    }

    #endregion

    #region Config Operations

    /// <summary>
    /// Subscribes to property change events for the child configuration objects.
    /// </summary>
    /// <param name="config">The core configuration model to subscribe to.</param>
    private void SubscribeToCoreConfigChildren(CoreConfigModel config)
    {
        config.Launcher.PropertyChanged += OnChildConfigPropertyChanged;
        config.Java.PropertyChanged += OnChildConfigPropertyChanged;
        config.Minecraft.PropertyChanged += OnChildConfigPropertyChanged;
        config.Misc.PropertyChanged += OnChildConfigPropertyChanged;
    }

    /// <summary>
    /// Unsubscribes from property change events for the child configuration objects.
    /// </summary>
    /// <param name="config">The core configuration model to unsubscribe from.</param>
    private void UnsubscribeFromCoreConfigChildren(CoreConfigModel config)
    {
        config.Launcher.PropertyChanged -= OnChildConfigPropertyChanged;
        config.Java.PropertyChanged -= OnChildConfigPropertyChanged;
        config.Minecraft.PropertyChanged -= OnChildConfigPropertyChanged;
        config.Misc.PropertyChanged -= OnChildConfigPropertyChanged;
    }

    /// <summary>
    /// Handles changes to the core configuration model, unsubscribing from the old value and subscribing to the new value.
    /// </summary>
    /// <param name="oldValue">The previous core configuration model.</param>
    /// <param name="newValue">The new core configuration model.</param>
    partial void OnCoreConfigChanged(CoreConfigModel? oldValue, CoreConfigModel newValue)
    {
        _logger.Debug("CoreConfig changed with old and new value. Unsubscribing from old, subscribing to new.");

        if (oldValue != null)
            UnsubscribeFromCoreConfigChildren(oldValue);

        SubscribeToCoreConfigChildren(newValue);

        if (!_isInitialized)
            return;
        SaveCoreConfigToFile(newValue);
    }

    /// <summary>
    /// Handles property change events for child configuration objects and saves the updated configuration to a file.
    /// </summary>
    /// <param name="sender">The object that triggered the property change event.</param>
    /// <param name="e">The event data associated with the property change.</param>
    private void OnChildConfigPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (!_isInitialized)
            return;
        _logger.Debug($"Inner property '{e.PropertyName}' changed on {sender?.GetType().Name}. Saving to file...");
        SaveCoreConfigToFile(CoreConfig);

        // Handle theme change
        if (e.PropertyName == nameof(CoreConfig.Launcher.Theme))
        {
            GlobalEvents.InvokeThemeChanged(CoreConfig.Launcher.Theme);
            return;
        }

        // Handle language change
        if (e.PropertyName == nameof(CoreConfig.Launcher.Language))
        {
            TranslationBindingSource.Instance.RaiseLanguageChanged();
            // ReSharper disable once RedundantJumpStatement, there might be additional logic in the future
            return;
        }
    }

    /// <summary>
    /// Saves the core configuration model to a file, preserving non-observable properties.
    /// </summary>
    /// <param name="newValue">The updated core configuration model to save.</param>
    private void SaveCoreConfigToFile(CoreConfigModel newValue)
    {
        var oldSettings = LauncherHelper.GetLauncherSettings(); // Fetch to preserve non-observable properties

        if (newValue.Java.MinMemory > newValue.Java.MaxMemory)
        {
            _logger.Warn("Min memory cannot be greater than max memory. Adjusting values.");
            newValue.Java.MinMemory = newValue.Java.MaxMemory;
        }

        var settings = new CoreConfig()
        {
            Launcher = new LauncherConfig()
            {
                EnableAutomaticUpdates = newValue.Launcher.EnableAutomaticUpdates,
                UpdateInterval = newValue.Launcher.UpdateInterval,
                NextUpdateCheck = oldSettings.Launcher.NextUpdateCheck, // Preserve
                Language = newValue.Launcher.Language,
                Theme = newValue.Launcher.Theme,
                AssetsDirectoryPath = newValue.Launcher.AssetsDirectoryPath,
                InstancesDirectoryPath = newValue.Launcher.InstancesDirectoryPath,
                CacheDirectoryPath = newValue.Launcher.CacheDirectoryPath,
                IconsDirectoryPath = newValue.Launcher.IconsDirectoryPath,
                LibrariesDirectoryPath = newValue.Launcher.LibrariesDirectoryPath,
                ManifestsDirectoryPath = newValue.Launcher.ManifestsDirectoryPath,
                TranslationsDirectoryPath = newValue.Launcher.TranslationsDirectoryPath,
                VersionsDirectoryPath = newValue.Launcher.VersionsDirectoryPath,
            },
            Java = new JavaConfig()
            {
                MinMemory = newValue.Java.MinMemory,
                MaxMemory = newValue.Java.MaxMemory,
                PermaGen = newValue.Java.PermaGen,
                JavaPath = newValue.Java.DefaultJavaPath,
                JvmArguments = newValue.Java.JvmArguments,
            },
            Minecraft = new MinecraftConfig()
            {
                StartMaximized = newValue.Minecraft.StartMaximized,
                WindowHeight = newValue.Minecraft.WindowHeight,
                WindowWidth = newValue.Minecraft.WindowWidth,
                CloseLauncherOnGameStart = newValue.Minecraft.CloseLauncherOnGameStart,
                CloseLauncherOnGameExit = newValue.Minecraft.CloseLauncherOnGameExit,
            },
            Misc = new MiscConfig()
            {
                PreLaunchCommand = newValue.Misc.PreLaunchCommand,
                WrapperCommand = newValue.Misc.WrapperCommand,
                PostExitCommand = newValue.Misc.PostExitCommand,
                UseCustomGlfw = newValue.Misc.UseCustomGlfw,
                CustomGlfwPath = newValue.Misc.CustomGlfwPath,
                UseCustomOpenAl = newValue.Misc.UseCustomOpenAl,
                CustomOpenAlPath = newValue.Misc.CustomOpenAlPath,
                UseDedicatedGpu = newValue.Misc.UseDedicatedGpu,
                EnableMangoHud = newValue.Misc.EnableMangoHud,
                EnableFeralGameMode = newValue.Misc.EnableFeralGameMode,
            },
            CacheRefreshDate = oldSettings.CacheRefreshDate
        };

        JsonHelper.WriteJsonFile(PathHelper.LauncherConfigPath, settings);
    }

    #endregion

    #endregion
    
    #region Skin Management
    [RelayCommand]
    private async Task SkinUpload()
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

            string? filePath = await OpenImagePicker.Handle(Unit.Default);
            if (filePath == null || !File.Exists(filePath))
                return;

            string skinId = Guid.NewGuid().ToString();
            var settings = await LauncherHelper.GetLauncherSettingsAsync();
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
            
            this.OnAccountUpdated();
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

    [RelayCommand]
    private async Task SkinSelect(SkinDataModel model)
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
            
            var settings = await LauncherHelper.GetLauncherSettingsAsync();
            string skinPath = Path.Combine(settings.Launcher.CacheDirectoryPath, "skins", SelectedAccount.Id, model.Id, "texture.png");
            if (!File.Exists(skinPath))
            {
                _logger.Error("Skin file does not exist: " + skinPath);
                return;
            }
            
            MojangProfile? profile = await MojangSkinService.UploadSkin(SelectedAccount.GetAccessToken(), model.Variant, skinPath);
            if (profile == null)
            {
                // TODO: Translate
                await ShowAlertDialog.Handle(new Alert("Error", "Failed to upload skin. Please try again later.", EAlertType.Error));
                return;
            }

            var newSkin = SelectedAccount.Skins.Find(x => x.Id == model.Id);
            if (SelectedSkin.CapeId != newSkin?.CapeId && newSkin is { CapeId: not null })
            {
                profile = await MojangSkinService.ShowCape(SelectedAccount.GetAccessToken(), newSkin.CapeId);
                if (profile == null)
                {
                    // TODO: Translate
                    await ShowAlertDialog.Handle(new Alert("Error", "Failed to change cape. Please try again later.", EAlertType.Error));
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
    
    [RelayCommand]
    private async Task CapeSelect(CapeDataModel model)
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
                result = await MojangSkinService.HideCape(SelectedAccount.GetAccessToken());
            else
                result = await MojangSkinService.ShowCape(SelectedAccount.GetAccessToken(), model.Id);
            
            if (result == null)
            {
                // TODO: Translate
                await ShowAlertDialog.Handle(new Alert("Error", "Failed to change cape.", EAlertType.Error));
                foreach (CapeDataModel cape in Capes.ToList())
                {
                    cape.IsSelected = cape.Id == SelectedSkin.CapeId && SelectedSkin.CapeId != null;
                    _capesCache.Edit(innerCache =>
                    {
                        innerCache.AddOrUpdate(cape);
                    });
                }
                return;
            }

            foreach (CapeDataModel cape in Capes.ToList())
            {
                cape.IsSelected = cape.Id == SelectedSkin.CapeId && SelectedSkin.CapeId != null;
                _capesCache.Edit(innerCache =>
                {
                    innerCache.AddOrUpdate(cape);
                });
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
            // TODO: Translate
            await ShowAlertDialog.Handle(new Alert("Error", "Unexpected error happened while selecting the cape.", EAlertType.Error));
        }
        finally
        {
            IsAccountSkinProcessing = false;
        }
    }
    
    [RelayCommand]
    private async Task ModelSelect(string model)
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

            MojangProfile? result = await MojangSkinService.ChangeSkin(SelectedAccount.GetAccessToken(), model, skin.Url);
            if (result == null)
            {
                // TODO: Translate
                await ShowAlertDialog.Handle(new Alert("Error", "Failed to change model.", EAlertType.Error));
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
            
            // TODO: Update Preview
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
}