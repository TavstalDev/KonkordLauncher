using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Tavstal.KonkordLauncher.Common.Helpers;
using Tavstal.KonkordLauncher.Common.Models;
using Tavstal.KonkordLauncher.Common.Models.Config;
using Tavstal.KonkordLauncher.Common.Models.InstanceConfig;
using Tavstal.KonkordLauncher.Common.Translation;
using Tavstal.KonkordLauncher.Core.Enums;
using Tavstal.KonkordLauncher.Core.Helpers;
using Tavstal.KonkordLauncher.Core.Models.ModLoaders;
using Tavstal.KonkordLauncher.Core.Models.MojangApi;
using Tavstal.KonkordLauncher.Desktop.Helpers;
using Tavstal.KonkordLauncher.Desktop.Models;
using Tavstal.KonkordLauncher.Desktop.Models.Enums;
using Tavstal.KonkordLauncher.Desktop.Models.Instance;
using Tavstal.KonkordLauncher.Desktop.Views.Dialogs;

namespace Tavstal.KonkordLauncher.Desktop.Views.Models;

public partial class CreateInstanceViewModel : KonkordObservableObject
{
    private CreateInstanceWindow? _parentWindow;
    private VersionManifest? _vanillaManifest;
    private readonly ReverseMarkdown.Converter _converter = new();

    #region Custom
    
    [ObservableProperty] [NotifyPropertyChangedFor(nameof(CanCreateCustomInstance))] private string _instanceName = string.Empty;
    [ObservableProperty] [NotifyPropertyChangedFor(nameof(CanCreateCustomInstance))] private string _instanceGroup = string.Empty;
    [ObservableProperty]  private string? _instanceIconPath;
    [ObservableProperty] [NotifyPropertyChangedFor(nameof(CanCreateCustomInstance))] private Bitmap? _instanceIcon;

    /// <summary>
    /// Gets a value indicating whether a custom instance can be created.
    /// Returns true if all required fields (instance name, icon, selected Minecraft version,
    /// and, if applicable, selected mod loader) are set; otherwise, false.
    /// </summary>
    public bool CanCreateCustomInstance
    {
        get
        {
            if (string.IsNullOrEmpty(InstanceName))
                return false;
            
            if (InstanceIcon == null)
                return false;
            
            // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
            if (SelectedMinecraftVersion == null)
                return false;
            
            if (ModLoaderType != EMinecraftKind.VANILLA && SelectedModLoader == null)
                return false;
            
            return true;
        }
    }
    
    #region Vanilla
    [ObservableProperty] [NotifyPropertyChangedFor(nameof(VersionResult))] private string _searchQuery = string.Empty;

    [ObservableProperty] [NotifyPropertyChangedFor(nameof(VersionResult))] private bool _showReleases = true;

    [ObservableProperty] [NotifyPropertyChangedFor(nameof(VersionResult))] private bool _showSnapshots;

    [ObservableProperty] [NotifyPropertyChangedFor(nameof(VersionResult))] private bool _showAlphas;

    [ObservableProperty] [NotifyPropertyChangedFor(nameof(VersionResult))] private bool _showBetas;

    [ObservableProperty] [NotifyPropertyChangedFor(nameof(VersionResult))] private bool _showExperiments;
    
    [ObservableProperty] [NotifyPropertyChangedFor(nameof(CanCreateCustomInstance))] [NotifyPropertyChangedFor(nameof(ModLoaderVersionResult))] private MinecraftVersion? _selectedMinecraftVersion;

    /// <summary>
    /// Gets a filtered list of available Minecraft versions based on the current search query and selected version types.
    /// Filters include release, snapshot, old alpha, old beta, and experiment versions,
    /// depending on the corresponding boolean properties.
    /// </summary>
    public ObservableCollection<MinecraftVersion> VersionResult
    {
        get
        {
            if (_vanillaManifest == null)
                return [];
            
            var versions = _vanillaManifest.Versions.FindAll(x =>
                (string.IsNullOrEmpty(SearchQuery) || x.Id.StartsWith(SearchQuery)) &&
                (x.Type != "release" || ShowReleases) &&
                (x.Type != "snapshot" || ShowSnapshots) &&
                (x.Type != "old_alpha" || ShowAlphas) &&
                (x.Type != "old_beta" || ShowBetas) &&
                (x.Type != "experiment" || ShowExperiments));

            return new ObservableCollection<MinecraftVersion>(versions.Count == 0 ? [] : versions);
        }
    }
    #endregion
    #region  Mod Loader
    [ObservableProperty] [NotifyPropertyChangedFor(nameof(ModLoaderVersionResult))] private string _modLoaderSearchQuery = string.Empty;
    [ObservableProperty] [NotifyPropertyChangedFor(nameof(ModLoaderVersionResult))] [NotifyPropertyChangedFor(nameof(CanCreateCustomInstance))] private EMinecraftKind _modLoaderType = EMinecraftKind.VANILLA;
    [ObservableProperty] [NotifyPropertyChangedFor(nameof(CanCreateCustomInstance))] private IModManifest? _selectedModLoader;
    
    /// <summary>
    /// Gets a filtered list of available mod loader versions based on the selected mod loader type,
    /// the currently selected Minecraft version, and the mod loader search query.
    /// For each mod loader type (NeoForge, Forge, Fabric, Quilt), retrieves the corresponding manifest,
    /// filters by the selected Minecraft version and search query, and returns the matching results.
    /// Returns an empty list for Vanilla or if no matching versions are found.
    /// </summary>
    public List<IModManifest> ModLoaderVersionResult
    {
        get
        {
            if (SelectedMinecraftVersion == null)
                return [];
            
            List<IModManifest> result = [];
            switch (ModLoaderType)
            {
                case EMinecraftKind.VANILLA:
                    break;
                case EMinecraftKind.NEOFORGE:
                {
                    var data = ManifestHelper.GetNeoForgeManifest();
                    if (data == null)
                        return [];
                    foreach (var version in data)
                    {
                        if (version.GameVersion != SelectedMinecraftVersion.Id)
                            continue;
                        
                        if (!string.IsNullOrEmpty(ModLoaderSearchQuery) || !version.Version.StartsWith(ModLoaderSearchQuery))
                            continue;
                        
                        result.Add(version);
                    }
                    break;
                }
                case EMinecraftKind.FORGE:
                {
                    var data = ManifestHelper.GetForgeManifest();
                    if (data == null)
                        return [];
                    foreach (var version in data)
                    {
                        if (version.GameVersion != SelectedMinecraftVersion.Id)
                            continue;
                        
                        if (!string.IsNullOrEmpty(ModLoaderSearchQuery) || !version.Version.StartsWith(ModLoaderSearchQuery))
                            continue;
                        
                        result.Add(version);
                    }
                    break;
                }
                case EMinecraftKind.FABRIC:
                {
                    result = ManifestHelper.GetFabricManifest()!;
                    if (result == null)
                        return [];
                    
                    if (!string.IsNullOrEmpty(ModLoaderSearchQuery))
                        result = result.FindAll(x => x.Version.StartsWith(ModLoaderSearchQuery));
                    break;
                }
                case EMinecraftKind.QUILT:
                {
                    result = ManifestHelper.GetQuiltManifest()!;
                    if (result == null)
                        return [];
                    
                    if (!string.IsNullOrEmpty(ModLoaderSearchQuery))
                        result = result.FindAll(x => x.Version.StartsWith(ModLoaderSearchQuery));
                    break;
                }
            }
            
            return result;
        }
    }
    #endregion
    #endregion
    
    #region Modpack

    [ObservableProperty] private ObservableCollection<ModPackModel> _modpacks;
    
    [ObservableProperty] private EPlatformType _selectedModpackPlatform = EPlatformType.Modrinth;
    
    [ObservableProperty] [NotifyPropertyChangedFor(nameof(ModpackPreview))] private ModPackModel? _selectedModpack;
    
    public List<EPlatformType> AvailableModpackPlatforms =>
    [
        EPlatformType.Modrinth,
        EPlatformType.CurseForge,
        EPlatformType.Technic,
        EPlatformType.FTB
    ];
    
    public string ModpackPreview => SelectedModpack == null ? _converter.Convert(@"<p>Select a modpack to see its preview.</p>") : _converter.Convert(SelectedModpack.RawPage);

    #endregion

    #region Import
    [ObservableProperty] [NotifyPropertyChangedFor(nameof(IsSourceFromFile))] private int _selectedImportSourceIndex = 0;
    
    public bool IsSourceFromFile => SelectedImportSourceIndex == 0;
    #endregion

    public CreateInstanceViewModel(CreateInstanceWindow parentWindow)
    {
        _parentWindow = parentWindow;
        _instanceIcon = ImageHelper.Load("avares://Desktop/Assets/Icons/dirt.png").Result;
        if (Design.IsDesignMode)
            return;
        
        _vanillaManifest = ManifestHelper.GetMinecraftManifest()!;
        _selectedMinecraftVersion = VersionResult.First();
    }

    public override void FreeMemory()
    {
        InstanceName = string.Empty;
        InstanceGroup = string.Empty;
        InstanceIcon?.Dispose();
        InstanceIcon = null;
        InstanceIconPath = null;
        SearchQuery = string.Empty;
        SelectedMinecraftVersion = null;
        _vanillaManifest = null;
        
        ModLoaderSearchQuery = string.Empty;
        SelectedModLoader = null;
        ModLoaderVersionResult.Clear();
    }
    
    #region Commands

    #region Custom
    /// <summary>
    /// Opens a window to select a custom icon for the instance. 
    /// If an icon is selected, it updates the instance's icon and its path.
    /// </summary>
    [RelayCommand]
    public async Task CustomIconSelectorAsync()
    {
        if (_parentWindow == null)
            return;
        
        IconSelectorWindow window = new();
        var result = await window.ShowDialog<IconDataModel>(_parentWindow);
        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        if (result == null)
            return;
        InstanceIcon?.Dispose();
        InstanceIcon = result.Image;
        InstanceIconPath = result.Path;
    }
    
    /// <summary>
    /// Sets the mod loader type for the custom instance creation process.
    /// </summary>
    /// <param name="modLoaderType">The type of mod loader to set.</param>
    [RelayCommand]
    public void CustomModLoaderType(EMinecraftKind modLoaderType)
    {
        ModLoaderType = modLoaderType;
    }

    /// <summary>
    /// Creates a new custom instance with the specified settings and adds it to the list of instances.
    /// Displays an error message if an instance with the same name already exists.
    /// </summary>
    [RelayCommand]
    public async Task CustomCreateAsync()
    {
        if (_parentWindow == null)
            return;
        
        var settings = await LauncherHelper.GetLauncherSettingsAsync();
        var instances = await LauncherHelper.GetInstancesAsync();
        if (instances.Any(x => x.Name == InstanceName))
        {
            AlertWindow alertWindow = new(TranslationManager.Translate("instance.duplicate.title"),
                TranslationManager.Translate("instance.duplicate.message"),
                EAlertType.Error);
            await alertWindow.ShowDialog(_parentWindow);
            return;
        }
        
        instances.Add(new Instance
        {
            Name = InstanceName,
            Kind = ModLoaderType,
            Group = "none",
            MinecraftVersion = SelectedMinecraftVersion?.Id,
            CustomVersion = SelectedModLoader?.Version ?? string.Empty,
            IconPath = InstanceIconPath ?? string.Empty,
            GameDirectory = System.IO.Path.Combine(settings.Launcher.InstancesDirectoryPath, InstanceName),
            Config = new InstanceConfig
            {
                Game = new InstanceGameConfig
                {
                    StartMaximized = settings.Minecraft.StartMaximized,
                    WindowHeight = (uint)(0.45 * App.ScreenSize.Height),
                    WindowWidth = (uint)(0.40 * App.ScreenSize.Width),
                    ShowConsoleWhenGameCrashes = true,
                    ShowConsoleWhileGameRunning = false,
                    CloseConsoleOnGameExit = false,
                    EnableFeralGameMode = settings.Misc.EnableFeralGameMode,
                    EnableMangoHud = settings.Misc.EnableMangoHud,
                    UseDedicatedGpu = settings.Misc.UseDedicatedGpu 
                },
                Java = new JavaConfig
                {
                    JvmArguments = string.IsNullOrEmpty(settings.Java.JvmArguments) ? Instance.GetDefaultJVMArgs() : settings.Java.JvmArguments,
                    JavaPath = "LAUNCH_ME_FIRST",
                    MinMemory = settings.Java.MinMemory,
                    MaxMemory = settings.Java.MaxMemory,
                    PermaGen = settings.Java.PermaGen,
                },
                Commands = new InstanceCommandsConfig(),
                EnableEnvironment = false,
                Environment = [],
                Misc =new InstanceMiscConfig()
            }
        });
        await JsonHelper.WriteJsonFileAsync(PathHelper.LauncherInstancesPath, instances);
        App.InvokeInstancesChanged();
        _parentWindow?.Close();
    }
    
    /// <summary>
    /// Cancels the custom instance creation process and closes the parent window.
    /// </summary>
    [RelayCommand]
    public void CustomCancelCreate() => _parentWindow?.Close();

    #endregion

    #endregion
}