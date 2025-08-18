using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DynamicData;
using NbtLib;
using ReactiveUI;
using Tavstal.KonkordLauncher.Common.Helpers;
using Tavstal.KonkordLauncher.Common.Models;
using Tavstal.KonkordLauncher.Common.Models.Config;
using Tavstal.KonkordLauncher.Common.Models.InstanceConfig;
using Tavstal.KonkordLauncher.Core.Enums;
using Tavstal.KonkordLauncher.Core.Helpers;
using Tavstal.KonkordLauncher.Core.Models;
using Tavstal.KonkordLauncher.Core.Models.MojangApi;
using Tavstal.KonkordLauncher.Desktop.Helpers;
using Tavstal.KonkordLauncher.Desktop.Models;
using Tavstal.KonkordLauncher.Desktop.Models.Avalonia;
using Tavstal.KonkordLauncher.Desktop.Models.Config.Instance;
using Tavstal.KonkordLauncher.Desktop.Models.Instance;

namespace Tavstal.KonkordLauncher.Desktop.Views.Models;

public partial class EditInstanceViewModel : KonkordObservableObject
{
    private readonly bool _isInitialized;
    private readonly string _instanceId;
    private readonly CoreLogger _logger = CoreLogger.WithModuleType(typeof(EditInstanceViewModel));
    private bool _isClosing;

    public bool IsLinux => OSHelper.GetOperatingSystem() == EOperatingSystem.Linux;
    public List<Account> Accounts { get; set; }

    #region Interactions

    public Interaction<Unit, Unit> CloseWindow { get; } = new();
    public Interaction<Alert, Unit> ShowAlertDialog { get; } = new();
    public Interaction<string, Unit> SetClipboardText { get; } = new();
    public Interaction<ScreenshotModel, Unit> SetClipboardImage { get; } = new();
    public Interaction<Unit, Unit> BeginWorldRename { get; } = new();
    public Interaction<Unit, Unit> BeginScreenshotRename { get; } = new();
    public Interaction<Unit, Unit> LogsScrollToEnd { get; } = new();

    #endregion

    #region Observable Properties

    [ObservableProperty] private string _instanceName;
    [ObservableProperty] private string? _gameDirectory;
    [ObservableProperty] private bool _isVanilla;
    [ObservableProperty] private string _logs;
    [ObservableProperty] private string _serverName;
    [ObservableProperty] private string _serverIp;

    private readonly SourceCache<ModModel, Guid> _modsCache = new(x => x.Id);
    public ReadOnlyObservableCollection<ModModel> FilteredMods { get; }
    [ObservableProperty] private ModModel? _selectedMod;
    [ObservableProperty] private string? _modSearchQuery = string.Empty;

    private readonly SourceCache<ResourcePackModel, Guid> _resourcePackCache = new(x => x.Id);
    public ReadOnlyObservableCollection<ResourcePackModel> FilteredResourcePacks { get; }
    [ObservableProperty] private ResourcePackModel? _selectedResourcePack;
    [ObservableProperty] private string? _resourcePackSearchQuery = string.Empty;

    private readonly SourceCache<ShaderPackModel, Guid> _shaderPackCache = new(x => x.Id);
    public ReadOnlyObservableCollection<ShaderPackModel> FilteredShaderPacks { get; }
    [ObservableProperty] private ShaderPackModel? _selectedShaderPack;
    [ObservableProperty] private string? _shaderPackSearchQuery = string.Empty;

    public ObservableCollection<WorldModel> Worlds { get; set; } = [];
    [ObservableProperty] private WorldModel? _selectedWorld;

    public ObservableCollection<ServerModel> Servers { get; set; } = [];
    [ObservableProperty] private ServerModel? _selectedServer;

    public ObservableCollection<ScreenshotModel> Screenshots { get; set; } = [];
    [ObservableProperty] private ScreenshotModel? _selectedScreenshot;

    [ObservableProperty] [NotifyPropertyChangedFor(nameof(CanRemoveEnvironmentVariable))]
    private InstanceConfigModel _instanceConfig;

    [ObservableProperty] [NotifyPropertyChangedFor(nameof(CanRemoveEnvironmentVariable))]
    private int? _selectedEnvironmentVariableIndex;

    [ObservableProperty] private int? _overridenAccountIndex = 0;

    public bool CanRemoveEnvironmentVariable =>
        SelectedEnvironmentVariableIndex is >= 0 && InstanceConfig.EnableEnvironment;

    #endregion

    public EditInstanceViewModel(string instanceId)
    {
        if (Design.IsDesignMode)
        {
            _instanceConfig = new InstanceConfigModel();
            return;
        }

        _instanceId = instanceId;
        var instances = LauncherHelper.GetInstances();
        var currentInstance = instances.FirstOrDefault(x => x.Id == _instanceId);
        if (currentInstance == null)
        {
            _logger.Error($"Instance with ID '{_instanceId}' not found.");
            throw new KeyNotFoundException($"Instance with ID '{_instanceId}' not found.");
        }

        _instanceName = currentInstance.Name;
        _isVanilla = currentInstance.Kind == EMinecraftKind.VANILLA;
        _gameDirectory = currentInstance.GameDirectory;
        _instanceConfig = new InstanceConfigModel(currentInstance.Config);
        _isInitialized = true;
        Accounts = LauncherHelper.GetAccountData().Accounts;
        SubscribeToConfigChildren(_instanceConfig);
        if (!string.IsNullOrEmpty(_instanceConfig.Misc.AccountId))
            OverridenAccountIndex = Accounts.FindIndex(x => x.Id == _instanceConfig.Misc.AccountId);

        // Logging setup
        GlobalEvents.OnInstanceLogged += OnInstanceLogged;
        Logs = GlobalEvents.GetInstanceLogs(_instanceId);
        if (!string.IsNullOrEmpty(Logs))
            Dispatcher.UIThread.Invoke(async () => await LogsScrollToEnd.Handle(Unit.Default));

        #region Mods
        if (!_isVanilla)
        {
            var mods = this.WhenAnyValue(x => x.ModSearchQuery)
                .Select(query =>
                {
                    if (string.IsNullOrWhiteSpace(query))
                        return (Func<ModModel, bool>)(_ => true); // No filter
                    return (Func<ModModel, bool>)(mod =>
                        mod.Name.Contains(query, StringComparison.OrdinalIgnoreCase));
                });

            var modSubscription = _modsCache.Connect()
                .Filter(mods)
                .Bind(out var filteredMods)
                .Subscribe();

            Disposables.Add(modSubscription);
            FilteredMods = filteredMods;

            RefreshMods();
        }
        #endregion
        
        #region Resource Packs

        // Set up a reactive filter for the ResourcePackSearchQuery property.
        // The filter updates dynamically based on the search query, matching resource packs whose names contain the query string (case-insensitive).
        var resourcePack = this.WhenAnyValue(x => x.ResourcePackSearchQuery)
            .Select(query =>
            {
                if (string.IsNullOrWhiteSpace(query))
                    return (Func<ResourcePackModel, bool>)(_ => true); // No filter
                return (Func<ResourcePackModel, bool>)(pack =>
                    pack.Name.Contains(query, StringComparison.OrdinalIgnoreCase));
            });

        // Connect the resource pack cache to the reactive filter.
        // Apply the filter and bind the resulting filtered collection to the FilteredResourcePacks property.
        // Subscribe to changes in the cache to keep the filtered collection up-to-date.
        var resourcePackSubscription = _resourcePackCache.Connect()
            .Filter(resourcePack)
            .Bind(out var filteredResourcePacks)
            .Subscribe();

        Disposables.Add(resourcePackSubscription);

        FilteredResourcePacks = filteredResourcePacks;
        RefreshResourcePacks();

        #endregion

        #region Shader Packs

        if (!_isVanilla)
        {
            var shaderPacks = this.WhenAnyValue(x => x.ShaderPackSearchQuery)
                .Select(query =>
                {
                    if (string.IsNullOrWhiteSpace(query))
                        return (Func<ShaderPackModel, bool>)(_ => true); // No filter
                    return (Func<ShaderPackModel, bool>)(pack =>
                        pack.Name.Contains(query, StringComparison.OrdinalIgnoreCase));
                });

            var shaderPackSubscription = _shaderPackCache.Connect()
                .Filter(shaderPacks)
                .Bind(out var filteredShaderPacks)
                .Subscribe();

            Disposables.Add(shaderPackSubscription);
            FilteredShaderPacks = filteredShaderPacks;

            RefreshShaderPacks();
        }
        #endregion
        
        RefreshWorlds();
        RefreshServers();
        RefreshScreenshots();
    }

    /// <summary>
    /// Handles log messages for a specific instance by updating the Logs property
    /// and triggering the LogsScrollToEnd interaction to scroll to the end of the logs.
    /// </summary>
    /// <param name="instanceId">The ID of the instance that generated the log message.</param>
    /// <param name="logMessage">The log message to be handled.</param>
    private void OnInstanceLogged(string instanceId, string logMessage)
    {
        if (instanceId != _instanceId)
            return;

        Logs += logMessage;
        Dispatcher.UIThread.Invoke(async () => await LogsScrollToEnd.Handle(Unit.Default));
    }

    /// <summary>
    /// Releases the resources used by the EditInstanceViewModel and performs cleanup operations.
    /// </summary>
    /// <param name="disposing">
    /// A boolean value indicating whether the method is being called directly or indirectly by a finalizer.
    /// If true, the method has been called directly or indirectly by a user's code. Managed and unmanaged resources can be disposed.
    /// If false, the method has been called by the runtime from inside the finalizer, and only unmanaged resources can be disposed.
    /// </param>
    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        _logger.Debug("Freeing memory in EditInstanceViewModel...");
        _isClosing = true;
        //GlobalEvents.OnInstanceLogged -= OnInstanceLogged;
        Worlds.CollectionChanged -= WorldsOnCollectionChanged;
        Servers.CollectionChanged -= ServersOnCollectionChanged;
        // Dispose of all image resources before clearing the collections
        foreach (var resourcePack in _resourcePackCache.Items)
            resourcePack.Icon?.Dispose();
        foreach (var world in Worlds)
            world.Icon?.Dispose();
        foreach (var server in Servers)
            server.Image?.Dispose();
        foreach (var screenshot in Screenshots)
            screenshot.Image?.Dispose();
        Accounts.Clear();
        _modsCache.Clear();
        _modsCache.Dispose();
        _resourcePackCache.Clear();
        _resourcePackCache.Dispose();
        _shaderPackCache.Clear();
        _shaderPackCache.Dispose();
        Worlds.Clear();
        Servers.Clear();
        Screenshots.Clear();
        UnsubscribeFromConfigChildren(InstanceConfig);

        InstanceConfig = new InstanceConfigModel();
        InstanceName = string.Empty;
        GameDirectory = null;
        Logs = string.Empty;

        SelectedEnvironmentVariableIndex = null;
        SelectedMod = null;
        SelectedResourcePack?.Icon?.Dispose();
        SelectedResourcePack = null;
        SelectedShaderPack = null;
        SelectedWorld?.Icon?.Dispose();
        SelectedWorld = null;
        SelectedServer?.Image?.Dispose();
        SelectedServer = null;
        SelectedScreenshot?.Image?.Dispose();
        SelectedScreenshot = null;
    }

    #region Logs

    /// <summary>
    /// Scrolls the logs to the end by triggering the LogsScrollToEnd interaction.
    /// </summary>
    [RelayCommand]
    private async Task ScrollLogsToEnd() => await LogsScrollToEnd.Handle(Unit.Default);

    /// <summary>
    /// Copies the current logs to the system clipboard by triggering the SetClipboardText interaction.
    /// </summary>
    [RelayCommand]
    private async Task CopyLogs() => await SetClipboardText.Handle(Logs);

    /// <summary>
    /// Clears the logs for the current instance and updates the global log storage.
    /// </summary>
    [RelayCommand]
    private void ClearLogs()
    {
        Logs = string.Empty;
        GlobalEvents.CleareInstanceLogs(_instanceId);
    }

    #endregion

    #region Mods

    #region Commands
    /// <summary>
    /// Toggles the enabled state of a mod and saves the updated state.
    /// </summary>
    /// <param name="mod">The mod to toggle.</param>
    [RelayCommand]
    private void ModToggleCommand(ModModel mod)
    {
        mod.IsEnabled = !mod.IsEnabled;
        SaveMods();
    }

    [RelayCommand]
    private void ModCheckUpdateCommand(ModModel mod)
    {
        // TODO: Implement mod update check logic
    }

    [RelayCommand]
    private void ModChangeVersionCommand(ModModel mod)
    {
        // TODO: Implement mod version change logic
    }

    /// <summary>
    /// Removes the specified mod file from the file system and refreshes the mod list.
    /// </summary>
    /// <param name="mod">The mod to remove.</param>
    [RelayCommand]
    private void ModRemoveCommand(ModModel mod)
    {
        if (!File.Exists(mod.Path))
            return;

        File.Delete(mod.Path);
        RefreshMods();
    }

    [RelayCommand]
    private void ModDownloadCommand()
    {
        // TODO: Implement mod download logic
    }

    /// <summary>
    /// Opens the directory containing the mods in the file explorer.
    /// </summary>
    [RelayCommand]
    private void ModOpenDirectoryCommand()
    {
        if (GameDirectory == null)
            return;
        
        string modsDir = Path.Combine(GameDirectory, "mods");
        if (!Directory.Exists(modsDir))
            return;

        FileSystemHelper.OpenFolderInFileExplorer(modsDir);
    }
    #endregion
    
    /// <summary>
    /// Refreshes the list of mods by scanning the game directory for mod files.
    /// Updates the `_modsCache` with metadata such as name, size, and enabled status for each mod.
    /// </summary>
    public void RefreshMods()
    {
        if (GameDirectory == null)
            return;

        string modsDir = Path.Combine(GameDirectory, "mods");
        if (!Directory.Exists(modsDir))
            return;

        _modsCache.Edit(innerCache =>
        {
            innerCache.Clear();
            var mods = Directory.GetFiles(modsDir, "*")
                .Where(x => x.EndsWith(".jar") || x.EndsWith(".jar.dis"));
            foreach (var mod in mods)
            {
                // Make sure the name does not include the extension
                var modName = Path.GetFileNameWithoutExtension(mod.Replace(".jar.dis", ".jar"));
                var size = File.ReadAllBytes(mod).LongLength;
                
                // TODO: Handle icon based on provider
                var icon = ImageHelper.LoadFromResource(new Uri("avares://Desktop/Assets/Images/default_world.png"));
                
                // TODO: Handle provider & version
                var newMod = new ModModel(mod.EndsWith(".jar"), modName, mod, icon, "unknown", "unknown", size);
                innerCache.AddOrUpdate(newMod);
            }
        });
    }
    
    /// <summary>
    /// Saves the current state of mods by renaming their file extensions
    /// based on their enabled or disabled status. Enabled mods have the
    /// `.jar` extension, while disabled ones have `.jar.dis`.
    /// </summary>
    public void SaveMods()
    {
        _logger.Debug("Saving mods...");
        if (GameDirectory == null)
            return;

        string modsDir = Path.Combine(GameDirectory, "mods");
        if (!Directory.Exists(modsDir))
            return;

        foreach (var mod in _modsCache.Items)
        {
            string? newPath = null;
            if (mod.IsEnabled && mod.Path.EndsWith(".jar.dis"))
                newPath = mod.Path.Replace(".dis", "");
            else if (!mod.IsEnabled && mod.Path.EndsWith(".jar"))
                newPath = mod.Path.Replace(".jar", ".jar.dis");

            if (newPath == null)
                continue;

            if (File.Exists(newPath))
            {
                _logger.Warn("Skipping save... Mod file already exists: " + newPath);
                continue;
            }

            File.Move(mod.Path, newPath);
            mod.Path = newPath;
        }
    }

    #endregion

    #region Resource Packs

    #region Commands

    /// <summary>
    /// Toggles the enabled state of a resource pack and saves the updated state.
    /// </summary>
    /// <param name="resourcePack">The resource pack to toggle.</param>
    [RelayCommand]
    private void ResourcePackToggleCommand(ResourcePackModel resourcePack)
    {
        resourcePack.IsEnabled = !resourcePack.IsEnabled;
        SaveResourcePacks();
    }

    /// <summary>
    /// Removes a resource pack file from the file system and refreshes the resource pack list.
    /// </summary>
    /// <param name="resourcePack">The resource pack to remove.</param>
    [RelayCommand]
    private void ResourcePackRemoveCommand(ResourcePackModel resourcePack)
    {
        if (!File.Exists(resourcePack.Path))
            return;

        File.Delete(resourcePack.Path);
        RefreshResourcePacks();
    }

    [RelayCommand]
    private void ResourcePackDownloadCommand(ResourcePackModel resourcePack)
    {
        // TODO: Implement resource pack download logic
    }

    /// <summary>
    /// Opens the directory containing the resource packs in the file explorer.
    /// </summary>
    [RelayCommand]
    private void ResourcePackOpenDirectoryCommand()
    {
        if (GameDirectory == null)
            return;
        
        string resourcePacksDir = Path.Combine(GameDirectory, "resourcepacks");
        if (!Directory.Exists(resourcePacksDir))
            return;

        FileSystemHelper.OpenFolderInFileExplorer(resourcePacksDir);
    }

    #endregion

    /// <summary>
    /// Saves the current state of resource packs by renaming their file extensions
    /// based on their enabled or disabled status. Enabled resource packs have the
    /// `.zip` extension, while disabled ones have `.zip.dis`.
    /// </summary>
    public void SaveResourcePacks()
    {
        _logger.Debug("Saving resource packs...");
        if (GameDirectory == null)
            return;

        string resourcePacksDir = Path.Combine(GameDirectory, "resourcepacks");
        if (!Directory.Exists(resourcePacksDir))
            return;

        foreach (var resourcePack in _resourcePackCache.Items)
        {
            string? newPath = null;
            if (resourcePack.IsEnabled && resourcePack.Path.EndsWith(".zip.dis"))
                newPath = resourcePack.Path.Replace(".dis", "");
            else if (!resourcePack.IsEnabled && resourcePack.Path.EndsWith(".zip"))
                newPath = resourcePack.Path.Replace(".zip", ".zip.dis");

            if (newPath == null)
                continue;

            if (File.Exists(newPath))
            {
                _logger.Warn("Skipping save... Resource pack file already exists: " + newPath);
                continue;
            }

            File.Move(resourcePack.Path, newPath);
            resourcePack.Path = newPath;
        }
    }

    /// <summary>
    /// Refreshes the list of resource packs by scanning the game directory for resource pack files.
    /// Updates the `ResourcePacks` collection with metadata such as name, size, and icon for each resource pack.
    /// </summary>
    public void RefreshResourcePacks()
    {
        if (GameDirectory == null)
            return;

        string resourcePacksDir = Path.Combine(GameDirectory, "resourcepacks");
        if (!Directory.Exists(resourcePacksDir))
            return;

        _resourcePackCache.Edit(innerCache =>
        {
            foreach (var resourcePack in innerCache.Items)
            {
                // Dispose of the image to free memory
                resourcePack.Icon?.Dispose();
            }

            innerCache.Clear();
            var resources = Directory.GetFiles(resourcePacksDir, "*")
                .Where(x => x.EndsWith(".zip") || x.EndsWith(".zip.dis"));
            foreach (var resource in resources)
            {
                // Make sure the name does not include the extension
                var resourceName = Path.GetFileNameWithoutExtension(resource.Replace(".zip.dis", ".zip"));
                var size = File.ReadAllBytes(resource).LongLength;
                using var zipFile = ZipFile.OpenRead(resource);
                Bitmap? icon = null;

                var iconEntry = zipFile.Entries.FirstOrDefault(x =>
                    x.FullName.EndsWith("pack.png", StringComparison.OrdinalIgnoreCase));
                if (iconEntry != null)
                {
                    using var iconStream = iconEntry.Open();
                    using var memoryStream = new MemoryStream();
                    iconStream.CopyTo(memoryStream);
                    iconStream.Close();
                    icon = ImageHelper.Base64ToBitmap(Convert.ToBase64String(memoryStream.ToArray()));
                }

                icon ??= ImageHelper.LoadFromResource(new Uri("avares://Desktop/Assets/Images/default_world.png"));

                // TODO: Handle provider
                var newResourcePack = new ResourcePackModel(resource.EndsWith(".zip"), resourceName, resource, icon,
                    "unknown", size);
                innerCache.AddOrUpdate(newResourcePack);
            }
        });
    }

    #endregion

    #region Shaders

    #region Commands

    /// <summary>
    /// Toggles the enabled state of a shader pack and saves the updated state.
    /// </summary>
    /// <param name="shader">The shader pack to toggle.</param>
    [RelayCommand]
    private void ShaderToggleCommand(ShaderPackModel shader)
    {
        shader.IsEnabled = !shader.IsEnabled;
        SaveShaderPacks();
    }
    
    /// <summary>
    /// Removes a shader pack file from the file system and refreshes the shader pack list.
    /// </summary>
    /// <param name="shader">The shader pack to remove.</param>
    [RelayCommand]
    private void ShaderRemoveCommand(ShaderPackModel shader)
    {
        if (!File.Exists(shader.Path))
            return;

        File.Delete(shader.Path);
        RefreshShaderPacks();
    }

    [RelayCommand]
    private void ShaderDownloadCommand()
    {
        // TODO: Implement shader download logic
    }

    /// <summary>
    /// Opens the directory containing the shader packs in the file explorer.
    /// </summary>
    [RelayCommand]
    private void ShaderOpenDirectoryCommand()
    {
        if (GameDirectory == null)
            return;
        
        string shaderPacksDir = Path.Combine(GameDirectory, "shaderpacks");
        if (!Directory.Exists(shaderPacksDir))
            return;

        FileSystemHelper.OpenFolderInFileExplorer(shaderPacksDir);
    }

    #endregion
    
    /// <summary>
    /// Saves the current state of shader packs by renaming their file extensions
    /// based on their enabled or disabled status. Enabled shader packs have the
    /// `.zip` extension, while disabled ones have `.zip.dis`.
    /// </summary>
    public void SaveShaderPacks()
    {
        _logger.Debug("Saving shader packs...");
        if (GameDirectory == null)
            return;

        string shaderPacksDir = Path.Combine(GameDirectory, "shaderpacks");
        if (!Directory.Exists(shaderPacksDir))
            return;

        foreach (var shaderPack in _shaderPackCache.Items)
        {
            string? newPath = null;
            if (shaderPack.IsEnabled && shaderPack.Path.EndsWith(".zip.dis"))
                newPath = shaderPack.Path.Replace(".dis", "");
            else if (!shaderPack.IsEnabled && shaderPack.Path.EndsWith(".zip"))
                newPath = shaderPack.Path.Replace(".zip", ".zip.dis");

            if (newPath == null)
                continue;

            if (File.Exists(newPath))
            {
                _logger.Warn("Skipping save... Shader pack file already exists: " + newPath);
                continue;
            }

            File.Move(shaderPack.Path, newPath);
            shaderPack.Path = newPath;
        }
    }
    
    /// <summary>
    /// Refreshes the list of shader packs by scanning the game directory for shader pack files.
    /// Updates the `_shaderPackCache` with metadata such as name, size, and enabled status for each shader pack.
    /// </summary>
    public void RefreshShaderPacks()
    {
        if (GameDirectory == null)
            return;

        string shaderPacksDir = Path.Combine(GameDirectory, "shaderpacks");
        if (!Directory.Exists(shaderPacksDir))
            return;

        _shaderPackCache.Edit(innerCache =>
        {
            innerCache.Clear();
            var packs = Directory.GetFiles(shaderPacksDir, "*")
                .Where(x => x.EndsWith(".zip") || x.EndsWith(".zip.dis"));
            foreach (var pack in packs)
            {
                // Make sure the name does not include the extension
                var packName = Path.GetFileNameWithoutExtension(pack.Replace(".zip.dis", ".zip"));
                var size = File.ReadAllBytes(pack).LongLength;
                
                // TODO: Handle provider
                var newPack = new ShaderPackModel(pack.EndsWith(".zip"), packName, pack, "unknown", size);
                innerCache.AddOrUpdate(newPack);
            }
        });
    }

    #endregion
    
    #region Worlds

    #region Commands

    /// <summary>
    /// Duplicates the specified Minecraft world by creating a copy of its directory
    /// and refreshing the list of worlds.
    /// </summary>
    /// <param name="world">The world to duplicate.</param>
    [RelayCommand]
    private void WorldsDuplicateCommand(WorldModel world)
    {
        DuplicateWorld(world);
        RefreshWorlds();
    }

    /// <summary>
    /// Initiates the renaming process for the specified Minecraft world by enabling
    /// edit mode in the worlds table.
    /// </summary>
    /// <param name="world">The world to rename.</param>
    [RelayCommand]
    private async Task WorldsRenameCommand(WorldModel world) => await BeginWorldRename.Handle(Unit.Default);

    /// <summary>
    /// Deletes the specified Minecraft world by removing its directory from the file system
    /// and refreshing the list of worlds.
    /// </summary>
    /// <param name="world">The world to delete.</param>
    [RelayCommand]
    private void WorldsDeleteCommand(WorldModel world)
    {
        FileSystemHelper.DeleteDirectory(world.Path);
        RefreshWorlds();
    }

    /// <summary>
    /// Copies the seed of the specified Minecraft world to the system clipboard.
    /// </summary>
    /// <param name="world">The world whose seed is to be copied.</param>
    [RelayCommand]
    private async Task WorldsCopySeed(WorldModel world) => await SetClipboardText.Handle(world.Seed.ToString());

    /// <summary>
    /// Opens the directory of the specified Minecraft world in the file explorer.
    /// </summary>
    /// <param name="world">The world whose directory is to be opened.</param>
    [RelayCommand]
    private void WorldsOpenDirectoryCommand(WorldModel world)
    {
        if (!Directory.Exists(world.Path))
            return;
    
        FileSystemHelper.OpenFolderInFileExplorer(world.Path);
    }
    
    #endregion
    
    /// <summary>
    /// Duplicates a Minecraft world by creating a copy of its directory and updating its metadata.
    /// Generates a unique name for the duplicated world and updates the "LevelName" tag in the `level.dat` file.
    /// </summary>
    /// <param name="world">The world to duplicate.</param>
    public void DuplicateWorld(WorldModel world)
    {
        _logger.Debug("Saving worlds...");
        if (GameDirectory == null)
            return;
        
        string worldsDir = Path.Combine(GameDirectory, "saves");
        if (!Directory.Exists(worldsDir))
            return;
        
        if (!Directory.Exists(world.Path))
            return;
            
        string? newPath = null;
        string? newName = null;
        for (int i = 0; i < 100; i++)
        {
            // Generate a new path with a unique name
            newName = $"{world.Name}({i})";
            newPath = Path.Combine(worldsDir, newName);
            if (!Directory.Exists(newPath))
                break;
        }
        if (newPath == null)
        {
            _logger.Error("Failed to generate a unique path for the duplicated world.");
            return;
        }

        try
        {
            FileSystemHelper.MoveDirectory(world.Path, newPath, true, false);
            
            string levelDatPath = Path.Combine(newPath, "level.dat");
            using var inputStream = File.OpenRead(levelDatPath);
            using var gzip = new GZipStream(inputStream, CompressionMode.Decompress);
            using var mem = new MemoryStream();
            gzip.CopyTo(mem);
            mem.Seek(0, SeekOrigin.Begin);
            var worldData = NbtConvert.ParseNbtStream(mem);
            inputStream.Close(); // Close the input stream to avoid file lock issues
            if (worldData == null)
            {
                _logger.Error("Failed to parse level.dat for world: " + world.Name);
                return;
            }

            // TryGet should not be used here, since it can't convert the INbtTag to NbtCompoundTag
            // ReSharper disable once CanSimplifyDictionaryLookupWithTryGetValue
            if (!worldData.ContainsKey("Data"))
            {
                _logger.Error("No 'Data' tag found in level.dat for world: " + world.Name);
                return;
            }

            var dataTag = worldData["Data"] as NbtCompoundTag;
            if (dataTag == null)
            {
                _logger.Error("Data tag is not a compound tag in level.dat for world: " + world.Name);
                return;
            }

            // TryGet should not be used here, as we want to ensure the tag exists
            // and it will be remowed and replaced with the new name.
            // ReSharper disable once CanSimplifyDictionaryRemovingWithSingleCall
            if (!dataTag.ContainsKey("LevelName"))
            {
                _logger.Error("No 'LevelName' tag found in level.dat for world: " + world.Name);
                return;
            }

            dataTag.Remove("LevelName"); // dataTag[] uses .Add, so the old one should be removed
            dataTag["LevelName"] = new NbtStringTag(newName);

            using var outputStream = new NbtWriter().CreateNbtStream(worldData);
            using var fileOutputStream = File.Create(levelDatPath);
            outputStream.Seek(0, SeekOrigin.Begin);
            outputStream.CopyTo(fileOutputStream);
            fileOutputStream.Close();
        }
        catch (Exception ex)
        {
            _logger.Exc("Failed to duplicate world.");
            _logger.Error(ex);
        }
    }
    
    /// <summary>
    /// Saves the current list of Minecraft worlds by updating their metadata and renaming their directories
    /// if necessary. This method ensures that the world's name in the `level.dat` file matches the directory name.
    /// </summary>
    public void SaveWorlds()
    {
        _logger.Debug("Saving worlds...");
        if (GameDirectory == null)
            return;
        
        try
        {
            string worldsDir = Path.Combine(GameDirectory, "saves");
            if (!Directory.Exists(worldsDir))
                return;
            
            foreach (var world in Worlds)
            {
                if (!Directory.Exists(world.Path))
                    continue;
                
                string? oldName = Path.GetDirectoryName(world.Path);
                // This should be changed if other properties will be changed in the future
                if (oldName == world.Name)
                    continue;
                
                string newPath = Path.Combine(worldsDir, world.Name);
                if (Directory.Exists(newPath))
                {
                    _logger.Warn("World with the same name already exists, skipping save.");
                    continue;
                }
                
                string levelDatPath = Path.Combine(world.Path, "level.dat");
                using var inputStream = File.OpenRead(levelDatPath);
                using var gzip = new GZipStream(inputStream, CompressionMode.Decompress);
                using var mem = new MemoryStream();
                gzip.CopyTo(mem);
                mem.Seek(0, SeekOrigin.Begin);
                var worldData = NbtConvert.ParseNbtStream(mem);
                inputStream.Close(); // Close the input stream to avoid file lock issues
                if (worldData == null)
                {
                    _logger.Error("Failed to parse level.dat for world: " + world.Name);
                    continue;
                }
                
                // TryGet should not be used here, since it can't convert the INbtTag to NbtCompoundTag
                // ReSharper disable once CanSimplifyDictionaryLookupWithTryGetValue
                if (!worldData.ContainsKey("Data"))
                {
                    _logger.Error("No 'Data' tag found in level.dat for world: " + world.Name);
                    continue;
                }
                
                var dataTag = worldData["Data"] as NbtCompoundTag;
                if (dataTag == null)
                {
                    _logger.Error("Data tag is not a compound tag in level.dat for world: " + world.Name);
                    continue;
                }
                
                // TryGet should not be used here, as we want to ensure the tag exists
                // and it will be remowed and replaced with the new name.
                // ReSharper disable once CanSimplifyDictionaryRemovingWithSingleCall
                if (!dataTag.ContainsKey("LevelName"))
                {
                    _logger.Error("No 'LevelName' tag found in level.dat for world: " + world.Name);
                    continue;
                }
                
                dataTag.Remove("LevelName"); // dataTag[] uses .Add, so the old one should be removed
                dataTag["LevelName"] = new NbtStringTag(world.Name);
                
                using var outputStream = new NbtWriter().CreateNbtStream(worldData);
                using var fileOutputStream = File.Create(levelDatPath);
                outputStream.Seek(0, SeekOrigin.Begin);
                outputStream.CopyTo(fileOutputStream);
                fileOutputStream.Close();
                FileSystemHelper.MoveDirectory(world.Path, newPath, true);
                world.Path = newPath;
            }
        }
        catch (Exception ex)
        {
            _logger.Exc("Failed to save worlds.");
            _logger.Error(ex);
        }
    }
    
    /// <summary>
    /// Handles changes to the `Worlds` collection by saving the updated list of worlds.
    /// This method is triggered whenever the `Worlds` collection is modified.
    /// </summary>
    /// <param name="sender">The source of the event, typically the `Worlds` collection.</param>
    /// <param name="e">The event data containing details about the collection change.</param>
    public void WorldsOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) => SaveWorlds();
    
    /// <summary>
    /// Refreshes the list of Minecraft worlds by scanning the game directory for saved worlds.
    /// Updates the Worlds collection with metadata such as name, game mode, last played timestamp, 
    /// seed, size, and icon for each world.
    /// </summary>
    public void RefreshWorlds()
    {
        if (GameDirectory == null)
            return;
        
        string worldsDir = Path.Combine(GameDirectory, "saves");
        if (!Directory.Exists(worldsDir))
            return;

        Worlds.CollectionChanged -= WorldsOnCollectionChanged;
        
        foreach (var world in Worlds)
        {
            // Dispose of the image to free memory
            world.Icon?.Dispose();
        }
        Worlds.Clear();
        var worldDirs = Directory.GetDirectories(worldsDir);
        foreach (var worldDir in worldDirs)
        {
            var worldName = "unknown";
            string levelDatPath = Path.Combine(worldDir, "level.dat");
            string gamemode = "unknown";
            long lastPlayed = 0;
            long seed = 0;
            Bitmap? icon = null;

            if (File.Exists(levelDatPath))
            {
                using var inputStream = File.OpenRead(levelDatPath);
                using var gzip = new GZipStream(inputStream, CompressionMode.Decompress);
                using var mem = new MemoryStream();
                gzip.CopyTo(mem);
                mem.Seek(0, SeekOrigin.Begin);
                var data = NbtConvert.DeserializeObject<Level>(mem);
                inputStream.Close(); // Close the input stream to avoid file lock issues

                if (data != null)
                {
                    lastPlayed = data.Data.LastPlayed;

                    if (!string.IsNullOrEmpty(data.Data.LevelName))
                        worldName = data.Data.LevelName;

                    if (data.Data.RandomSeed != null)
                        seed = data.Data.RandomSeed.Value;
                    
                    // GameMode (int) — try "GameType" first, then "GameMode"
                    if (data.Data.GameMode != null)
                        gamemode = (data.Data.GameMode) switch
                        {
                            0 => "Survival",
                            1 => "Creative",
                            2 => "Adventure",
                            3 => "Spectator",
                            _ => "Unknown"
                        };
                    else if (data.Data.GameType != null)
                        gamemode = (data.Data.GameType) switch
                        {
                            0 => "Survival",
                            1 => "Creative",
                            2 => "Adventure",
                            3 => "Spectator",
                            _ => "Unknown"
                        };
                }
            }
            
            var files = Directory.EnumerateFiles(worldDir, "*", SearchOption.AllDirectories);
            long size = files.Sum(file => new FileInfo(file).Length);
            if (File.Exists(Path.Combine(worldDir, "icon.png")))
            {
                try
                {
                    icon = new Bitmap(Path.Combine(worldDir, "icon.png"));
                }
                catch (Exception ex)
                {
                    _logger.Error($"Failed to load world icon for {worldName}: {ex.Message}");
                }
            }
            icon ??= ImageHelper.LoadFromResource(new Uri("avares://Desktop/Assets/Images/default_world.png"));
            
            Worlds.Add(new WorldModel(worldName, worldDir, gamemode,  seed, lastPlayed, size, icon));
        }
        Worlds.CollectionChanged += WorldsOnCollectionChanged;
    }

    #endregion
    
    #region  Servers

    #region Commands

    /// <summary>
    /// Joins the specified Minecraft server by launching the instance with the server's IP address
    /// and then closes the parent window.
    /// </summary>
    /// <param name="server">The server to join.</param>
    [RelayCommand]
    private async Task ServersJoinCommand(ServerModel server)
    {
        // TOOO: Implement server joining logic
        //await _instance.LaunchAsync(_parentWindow, server.Ip);
        await CloseWindow.Handle(Unit.Default);
    }

    /// <summary>
    /// Adds a new server to the list of servers if both the server name and IP address are provided.
    /// </summary>
    [RelayCommand]
    private void ServerAddCommand()
    {
        if (string.IsNullOrEmpty(ServerName) || string.IsNullOrEmpty(ServerIp))
            return;

        Servers.Add(new ServerModel(ServerName, ServerIp, 0, 0, null));
    }

    /// <summary>
    /// Removes the specified server from the list of servers if it exists in the collection.
    /// </summary>
    /// <param name="server">The server to remove.</param>
    [RelayCommand]
    private void ServersRemoveCommand(ServerModel server)
    {
        if (Servers.Contains(server))
            Servers.Remove(server);
    }
    
    #endregion
    
    /// <summary>
    /// Saves the current list of Minecraft servers to the `servers.dat` file
    /// in the game directory. The method serializes the server data into NBT format
    /// and writes it to the file.
    /// </summary>
    public void SaveServers()
    {
        _logger.Debug("Saving servers to servers.dat file...");
        if (GameDirectory == null)
            return;
        try
        {
            string filePath = Path.Combine(GameDirectory, "servers.dat");
            
            var root = new NbtCompoundTag();
            var serversList = new NbtListTag(NbtTagType.Compound);

            foreach (var s in Servers)
            {
                var serverTag = new NbtCompoundTag
                {
                    { "name", new NbtStringTag(s.Name) },
                    { "ip", new NbtStringTag(s.Ip) },
                    { "acceptTextures", new NbtIntTag(s.AcceptTextures) },
                };
                
                if (s.HideAddress.HasValue)
                    serverTag.Add("hideAddress", new NbtIntTag(s.HideAddress.Value));
                
                if (!string.IsNullOrEmpty(s.Icon))
                    serverTag.Add("icon", new NbtStringTag(s.Icon));

                serversList.Add(serverTag);
            }

            root.Add("servers", serversList);
            
            using var outputStream = new NbtWriter().CreateUncompressedNbtStream(root, "");
            using var fileStream = File.Create(filePath);
            outputStream.Seek(0, SeekOrigin.Begin);
            outputStream.CopyTo(fileStream);
        }
        catch (Exception ex)
        {
            _logger.Exc("Failed to save servers to servers.dat file.");
            _logger.Error(ex);
        }
    }

    /// <summary>
    /// Handles changes to the `Servers` collection by saving the updated list
    /// of servers to the `servers.dat` file.
    /// </summary>
    /// <param name="sender">The source of the event, typically the `Servers` collection.</param>
    /// <param name="e">The event data containing details about the collection change.</param>
    private void ServersOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) => SaveServers();
    
    /// <summary>
    /// Refreshes the list of Minecraft servers by reading the `servers.dat` file
    /// from the game directory and updating the Servers collection with the data.
    /// </summary>
    public void RefreshServers()
    {
        if (GameDirectory == null)
            return;
    
        // Construct the file path for the servers.dat file
        string filePath = Path.Combine(GameDirectory, "servers.dat");
        if (!File.Exists(filePath))
            return;

        // Open the servers.dat file and deserialize its content
        using var inputStream = File.OpenRead(filePath);
        var serversDat = NbtConvert.DeserializeObject<ServersDat>(inputStream);
        if (serversDat == null)
            return;
    
        Servers.CollectionChanged -= ServersOnCollectionChanged;
        
        // Clear the existing Servers collection and populate it with new data
        foreach (var server in Servers)
        {
            // Dispose of the image to free memory
            server.Image?.Dispose();
        }
        Servers.Clear();
        foreach (var server in serversDat.Servers)
            Servers.Add(new ServerModel(server.Name, server.Ip, server.AcceptTextures, server.HideAddress, server.Icon));
        
        Servers.CollectionChanged += ServersOnCollectionChanged;
    }

    #endregion

    #region Screenshots

    #region Commands

    /// <summary>
    /// Copies the specified screenshot to the system clipboard.
    /// </summary>
    /// <param name="screenshot">The screenshot to copy to the clipboard.</param>
    [RelayCommand]
    private async Task ScreenshotsCopyCommand(ScreenshotModel screenshot) => await SetClipboardImage.Handle(screenshot);

    /// <summary>
    /// Deletes the specified screenshot file from the file system and refreshes the screenshot list.
    /// </summary>
    /// <param name="screenshot">The screenshot to delete.</param>
    [RelayCommand]
    private void ScreenshotsDeleteCommand(ScreenshotModel screenshot)
    {
        if (!File.Exists(screenshot.Path))
            return;

        File.Delete(screenshot.Path);
        RefreshScreenshots();
    }

    /// <summary>
    /// Initiates the renaming process for the specified screenshot by enabling edit mode in the screenshots table.
    /// </summary>
    /// <param name="screenshot">The screenshot to rename.</param>
    [RelayCommand]
    private async Task ScreenshotsRenameCommand(ScreenshotModel screenshot) => await BeginScreenshotRename.Handle(Unit.Default);

    /// <summary>
    /// Opens the directory containing the screenshots in the file explorer.
    /// </summary>
    /// <param name="screenshot">The screenshot whose directory to open.</param>
    [RelayCommand]
    private void ScreenshotsOpenDirectoryCommand(ScreenshotModel screenshot)
    {
        if (string.IsNullOrEmpty(GameDirectory))
            return;
    
        string screenshotDir = Path.Combine(GameDirectory, "screenshots");
        if (!Directory.Exists(screenshotDir))
            return;

        FileSystemHelper.OpenFolderInFileExplorer(screenshotDir);
    }

    #endregion
    
    /// <summary>
    /// Refreshes the list of screenshots by scanning the game directory for PNG files
    /// and updating the Screenshots collection with their metadata and image data.
    /// </summary>
    public void RefreshScreenshots()
    {
        if (GameDirectory == null)
            return;

        string screenshotDir = Path.Combine(GameDirectory, "screenshots");
        if (!Directory.Exists(screenshotDir))
            return;
        
        foreach (var screenshot in Screenshots)
        {
            // Dispose of the image to free memory
            screenshot.Image?.Dispose();
        }
        Screenshots.Clear();
        var screenshots = Directory.GetFiles(screenshotDir, "*.png");
        foreach (var screenshot in screenshots)
        {
            var bytes = File.ReadAllBytes(screenshot);
            var newScreenshot = new ScreenshotModel(screenshot, new Bitmap(screenshot), bytes.LongLength);
            Screenshots.Add(newScreenshot);
        }
    }
    #endregion

    #region Settings

    /// <summary>
    /// Subscribes to the PropertyChanged event of the child configuration models
    /// to monitor changes in their properties.
    /// </summary>
    /// <param name="config">The instance configuration model to subscribe to.</param>
    private void SubscribeToConfigChildren(InstanceConfigModel config)
    {
        config.Game.PropertyChanged += OnChildConfigPropertyChanged;
        config.Java.PropertyChanged += OnChildConfigPropertyChanged;
        config.Commands.PropertyChanged += OnChildConfigPropertyChanged;
        config.Environment.CollectionChanged += OnChildConfigCollectionChanged;
        config.Misc.PropertyChanged += OnChildConfigPropertyChanged;
    }

    /// <summary>
    /// Unsubscribes from the PropertyChanged event of the child configuration models
    /// to stop monitoring changes in their properties.
    /// </summary>
    /// <param name="config">The instance configuration model to unsubscribe from.</param>
    private void UnsubscribeFromConfigChildren(InstanceConfigModel config)
    {
        config.Game.PropertyChanged -= OnChildConfigPropertyChanged;
        config.Java.PropertyChanged -= OnChildConfigPropertyChanged;
        config.Commands.PropertyChanged -= OnChildConfigPropertyChanged;
        config.Environment.CollectionChanged -= OnChildConfigCollectionChanged;
        config.Misc.PropertyChanged -= OnChildConfigPropertyChanged;
    }

    /// <summary>
    /// Handles changes to the InstanceConfigModel by unsubscribing from the old configuration
    /// and subscribing to the new configuration. Saves the new configuration to a file if initialized.
    /// </summary>
    /// <param name="oldValue">The previous instance configuration model.</param>
    /// <param name="newValue">The new instance configuration model.</param>
    partial void OnInstanceConfigChanged(InstanceConfigModel? oldValue, InstanceConfigModel newValue)
    {
        if (_isClosing)
            return;
        
        _logger.Debug("InstanceConfig changed with old and new value. Unsubscribing from old, subscribing to new.");
        
        if (oldValue != null)
            UnsubscribeFromConfigChildren(oldValue);

        SubscribeToConfigChildren(newValue);

        if (!_isInitialized)
            return;
        SaveCoreConfigToFile(newValue);
    }

    /// <summary>
    /// Handles the PropertyChanged event for child configuration models.
    /// Logs the change and saves the updated configuration to a file if initialized.
    /// </summary>
    /// <param name="sender">The object that raised the event.</param>
    /// <param name="e">The event data containing the name of the changed property.</param>
    private void OnChildConfigPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (!_isInitialized || _isClosing)
            return;
        _logger.Debug($"Inner property '{e.PropertyName}' changed on {sender?.GetType().Name}. Saving to file...");
        SaveCoreConfigToFile(InstanceConfig);
    }
    
    /// <summary>
    /// Handles changes to a collection within the instance configuration model.
    /// Logs the change and saves the updated configuration to a file if the view model is initialized.
    /// </summary>
    /// <param name="sender">The source of the event, typically the collection that changed.</param>
    /// <param name="e">The event data containing details about the collection change.</param>
    private void OnChildConfigCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (!_isInitialized || _isClosing)
            return;
        _logger.Debug($"Inner collection changed on {sender?.GetType().Name}. Saving to file...");
        SaveCoreConfigToFile(InstanceConfig);
    }

    /// <summary>
    /// Saves the updated instance configuration to a file. Ensures that the minimum memory
    /// does not exceed the maximum memory in the Java configuration.
    /// </summary>
    /// <param name="newValue">The updated instance configuration model to save.</param>
    private void SaveCoreConfigToFile(InstanceConfigModel newValue)
    {
        if (_isClosing)
            return;
        if (newValue.Java.MinMemory > newValue.Java.MaxMemory)
            newValue.Java.MinMemory = newValue.Java.MaxMemory;

        var instances = LauncherHelper.GetInstances();
        int index = 0;
        Instance? instanceToSave = null;
        foreach (var instance in instances)
        {
            if (instance.Id == _instanceId)
            {
                instanceToSave = instance;
                break;
            }

            index++;
        }

        if (instanceToSave == null)
            return;
        
        var environmentVariables = newValue.Environment
            .Select(x => new EnvironmentVariable(x.Key, x.Value))
            .ToList();

        instanceToSave.Config = new InstanceConfig()
        {
            Java = new JavaConfig()
            {
                MinMemory = newValue.Java.MinMemory,
                MaxMemory = newValue.Java.MaxMemory,
                PermaGen = newValue.Java.PermaGen,
                JavaPath = newValue.Java.DefaultJavaPath,
                JvmArguments = newValue.Java.JvmArguments
            },
            Game = new InstanceGameConfig()
            {
                StartMaximized = newValue.Game.StartMaximized,
                WindowHeight = newValue.Game.WindowHeight,
                WindowWidth = newValue.Game.WindowWidth,
                ShowConsoleWhileGameRunning = newValue.Game.ShowConsoleWhileGameRunning,
                ShowConsoleWhenGameCrashes = newValue.Game.ShowConsoleWhenGameCrashes,
                CloseConsoleOnGameExit = newValue.Game.CloseConsoleOnGameExit,
                EnableFeralGameMode = newValue.Game.EnableFeralGameMode,
                EnableMangoHud = newValue.Game.EnableMangoHud,
                UseDedicatedGpu = newValue.Game.UseDedicatedGpu,
            },
            Commands = new InstanceCommandsConfig()
            {
                PreLaunchCommand = newValue.Commands.PreLaunchCommand,
                WrapperCommand = newValue.Commands.WrapperCommand,
                PostExitCommand = newValue.Commands.PostExitCommand,
            },
            EnableEnvironment = newValue.EnableEnvironment,
            Environment = environmentVariables,
            Misc = new InstanceMiscConfig()
            {
                UseCustomGlfw = newValue.Misc.UseCustomGlfw,
                CustomGlfwPath = newValue.Misc.CustomGlfwPath,
                UseCustomOpenAL = newValue.Misc.UseCustomOpenAL,
                CustomOpenALPath = newValue.Misc.CustomOpenALPath,
                AccountId = newValue.Misc.AccountId,
                OverrideAccount = newValue.Misc.OverrideAccount,
                JoinServerOnLaunch = newValue.Misc.JoinServerOnLaunch,
                ServerAddress = newValue.Misc.ServerAddress,
            }
        };
        instances[index] = instanceToSave;

        JsonHelper.WriteJsonFile(PathHelper.LauncherInstancesPath, instances);
        GlobalEvents.InvokeInstancesChanged();
    }

    #endregion
}