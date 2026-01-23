using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DynamicData;
using DynamicData.Binding;
using ReactiveUI;
using Tavstal.KonkordLauncher.Common.Helpers;
using Tavstal.KonkordLauncher.Common.Models;
using Tavstal.KonkordLauncher.Common.Models.Config;
using Tavstal.KonkordLauncher.Common.Models.InstanceConfig;
using Tavstal.KonkordLauncher.Common.Translation;
using Tavstal.KonkordLauncher.Core.Enums;
using Tavstal.KonkordLauncher.Core.Helpers;
using Tavstal.KonkordLauncher.Core.Models;
using Tavstal.KonkordLauncher.Core.Models.ModLoaders;
using Tavstal.KonkordLauncher.Core.Models.MojangApi;
using Tavstal.KonkordLauncher.Desktop.Helpers;
using Tavstal.KonkordLauncher.Desktop.Models;
using Tavstal.KonkordLauncher.Desktop.Models.Avalonia;
using Tavstal.KonkordLauncher.Desktop.Models.Enums;
using Tavstal.KonkordLauncher.Desktop.Models.Instance;

namespace Tavstal.KonkordLauncher.Desktop.Views.Models;

public partial class CreateInstanceViewModel : KonkordObservableObject
{
    private CoreLogger _logger = CoreLogger.WithModuleType(typeof(CreateInstanceViewModel));
    private ReverseMarkdown.Converter? _converter = new();
    public Interaction<Unit, Unit> CloseWindow { get; }  = new();
    public Interaction<Alert, Unit> ShowAlertDialog { get; } = new();
    public Interaction<Unit, string?> ShowIconSelector { get; } = new();

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
    private readonly SourceCache<MinecraftVersion, string> _minecraftVersionCache = new(x => x.Id);
    public ReadOnlyObservableCollection<MinecraftVersion> MinecraftVersions { get; }
    [ObservableProperty] private string _searchQuery = string.Empty;

    [ObservableProperty] private bool _showReleases = true;

    [ObservableProperty] private bool _showSnapshots;

    [ObservableProperty] private bool _showAlphas;

    [ObservableProperty] private bool _showBetas;

    [ObservableProperty] private bool _showExperiments;
    
    [ObservableProperty] [NotifyPropertyChangedFor(nameof(ModLoaderVersionResult))] [NotifyPropertyChangedFor(nameof(CanCreateCustomInstance))] private MinecraftVersion? _selectedMinecraftVersion;
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
    
    public string? ModpackPreview => SelectedModpack == null ? _converter?.Convert(@"<p>Select a modpack to see its preview.</p>") : _converter?.Convert(SelectedModpack.RawPage);

    #endregion

    #region Import
    [ObservableProperty] [NotifyPropertyChangedFor(nameof(IsSourceFromFile))] private int _selectedImportSourceIndex;
    
    public bool IsSourceFromFile => SelectedImportSourceIndex == 0;
    #endregion

    public CreateInstanceViewModel()
    {
        _instanceIcon = ImageHelper.Load("avares://Desktop/Assets/Icons/dirt.png").Result;
        if (Design.IsDesignMode)
            return;
        
        var search = this.WhenAnyValue(x => x.SearchQuery)
            .Throttle(TimeSpan.FromMilliseconds(150), RxApp.MainThreadScheduler);
        
        var toggles = this.WhenAnyValue(
            x => x.ShowReleases,
            x => x.ShowSnapshots,
            x => x.ShowAlphas,
            x => x.ShowBetas,
            x => x.ShowExperiments
        );
        
        var filter = search.CombineLatest(toggles, (query, t) => (query, t))
            .Select(values =>
            {
                var (query, t) = values;
                var (showReleases, showSnapshots, showAlphas, showBetas, showExperiments) = t;
                
                return (Func<MinecraftVersion, bool>)(x =>
                        (string.IsNullOrWhiteSpace(query) || x.Id.StartsWith(query)) &&
                        (x.Type != "release"    || showReleases) &&
                        (x.Type != "snapshot"   || showSnapshots) &&
                        (x.Type != "old_alpha"  || showAlphas) &&
                        (x.Type != "old_beta"   || showBetas) &&
                        (x.Type != "experiment" || showExperiments)
                    );
            });

        var bindingSubscription =
            _minecraftVersionCache
                .Connect()
                .Filter(filter)
                .Sort(
                    SortExpressionComparer<MinecraftVersion>.Descending(x => x.ReleaseTime)
                )
                .Bind(out var filteredCollection)
                .Subscribe(
                    _ => { },
                    ex => _logger.Error("DynamicData pipeline crashed: " + ex)
                );
        Disposables.Add(bindingSubscription);
        MinecraftVersions = filteredCollection;
        _minecraftVersionCache.Edit(innerCache =>
        {
            var vanillaManifest = ManifestHelper.GetMinecraftManifest();
            if (vanillaManifest == null)
                throw new InvalidOperationException("Minecraft manifest is not available.");
            foreach (var version in vanillaManifest.Versions)
                innerCache.AddOrUpdate(version);
            _selectedMinecraftVersion = innerCache.Items.FirstOrDefault();
        });
    }

    /// <summary>
    /// Releases the resources used by the CreateInstanceViewModel and performs cleanup operations.
    /// </summary>
    /// <param name="disposing">
    /// A boolean value indicating whether the method is being called directly or indirectly by a finalizer.
    /// If true, the method has been called directly or indirectly by a user's code. Managed and unmanaged resources can be disposed.
    /// If false, the method has been called by the runtime from inside the finalizer, and only unmanaged resources can be disposed.
    /// </param>
    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        _minecraftVersionCache.Clear();
        _minecraftVersionCache.Dispose();
        InstanceName = string.Empty;
        InstanceGroup = string.Empty;
        InstanceIcon?.Dispose();
        InstanceIcon = null;
        InstanceIconPath = null;
        SearchQuery = string.Empty;
        SelectedMinecraftVersion = null;
        _converter = null;
        
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
    private async Task CustomIconSelectorAsync()
    {
        var result = await ShowIconSelector.Handle(Unit.Default);
        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        if (result == null)
            return;
        InstanceIcon?.Dispose();
        try
        {
            InstanceIcon = new Bitmap(result);
        }
        catch
        {
            InstanceIcon = ImageHelper.Load("avares://Desktop/Assets/Icons/dirt.png").Result;
        }
        InstanceIconPath = result;
    }
    
    /// <summary>
    /// Sets the mod loader type for the custom instance creation process.
    /// </summary>
    /// <param name="modLoaderType">The type of mod loader to set.</param>
    [RelayCommand]
    private void CustomModLoaderType(EMinecraftKind modLoaderType)
    {
        ModLoaderType = modLoaderType;
    }

    /// <summary>
    /// Creates a new custom instance with the specified settings and adds it to the list of instances.
    /// Displays an error message if an instance with the same name already exists.
    /// </summary>
    [RelayCommand]
    private async Task CustomCreateAsync()
    {
        var settings = await LauncherHelper.GetLauncherSettingsAsync();
        var instances = await LauncherHelper.GetInstancesAsync();
        if (instances.Any(x => x.Name == InstanceName))
        {
            await ShowAlertDialog.Handle(new Alert(TranslationManager.Translate("instance.duplicate.title"),
                TranslationManager.Translate("instance.duplicate.message"),
                EAlertType.Error));
            return;
        }
        
        instances.Add(new Instance
        {
            Name = InstanceName,
            Kind = ModLoaderType,
            Group = null,
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
        GlobalEvents.InvokeInstancesChanged();
        await CloseWindow.Handle(Unit.Default);
    }
    
    /// <summary>
    /// Cancels the custom instance creation process and closes the parent window.
    /// </summary>
    [RelayCommand]
    private async Task CustomCancelCreate() => await CloseWindow.Handle(Unit.Default);

    #endregion

    #endregion
}