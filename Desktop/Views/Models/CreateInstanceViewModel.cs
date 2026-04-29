using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
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
using Tavstal.KonkordLauncher.Core.Helpers.Domain;
using Tavstal.KonkordLauncher.Core.Helpers.IO;
using Tavstal.KonkordLauncher.Core.Helpers.Serialization;
using Tavstal.KonkordLauncher.Core.Models;
using Tavstal.KonkordLauncher.Core.Models.ModLoaders;
using Tavstal.KonkordLauncher.Core.Models.MojangApi;
using Tavstal.KonkordLauncher.Desktop.Helpers;
using Tavstal.KonkordLauncher.Desktop.Models.Avalonia;
using Tavstal.KonkordLauncher.Desktop.Models.Domain;
using Tavstal.KonkordLauncher.Desktop.Models.Enums;
using Tavstal.KonkordLauncher.Desktop.Models.Instance;

namespace Tavstal.KonkordLauncher.Desktop.Views.Models;

public partial class CreateInstanceViewModel : KonkordObservableObject
{
    private readonly CoreLogger _logger = CoreLogger.WithModuleType(typeof(CreateInstanceViewModel));

    [ObservableProperty] private ECreateInstanceTab _selectedTab = ECreateInstanceTab.MODPACK;
    
    #region Interactions
    public Interaction<Unit, Unit> MinimizeWindowInteraction { get; } = new();
    public Interaction<Unit, Unit> MaximizeWindowInteraction { get; } = new();
    public Interaction<Unit, Unit> CloseWindowInteraction { get; } = new();
    public Interaction<ECreateInstanceTab, Unit> UpdateSelectedTabButton { get; } = new();
    public Interaction<int, Unit> UpdateSelectedImportTypeButton { get; } = new();
    public Interaction<Alert, Unit> ShowAlertDialog { get; } = new();
    public Interaction<Unit, string?> ShowIconSelector { get; } = new();
    public Interaction<Unit, string?> ShowFileSelector { get; } = new();
    #endregion

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
    public ReadOnlyObservableCollection<MinecraftVersion> MinecraftVersions { get; private set; }
    [ObservableProperty] private string _searchQuery = string.Empty;

    [ObservableProperty] private bool _showReleases = true;

    [ObservableProperty] private bool _showSnapshots;

    [ObservableProperty] private bool _showAlphas;

    [ObservableProperty] private bool _showBetas;

    [ObservableProperty] private bool _showExperiments;
    
    [ObservableProperty] [NotifyPropertyChangedFor(nameof(CanCreateCustomInstance))] private MinecraftVersion? _selectedMinecraftVersion;
    #endregion
    #region  Mod Loader
    [ObservableProperty] private string _modLoaderSearchQuery = string.Empty;
    [ObservableProperty] [NotifyPropertyChangedFor(nameof(CanCreateCustomInstance))] private EMinecraftKind _modLoaderType = EMinecraftKind.VANILLA;
    [ObservableProperty] [NotifyPropertyChangedFor(nameof(CanCreateCustomInstance))] private IModManifest? _selectedModLoader;

    private readonly SourceCache<IModManifest, string> _modLoaderVersionCache = new(x => x.Version);
    public ReadOnlyObservableCollection<IModManifest> ModLoaderVersionResult { get; private set; } = new([]);
    
    private static readonly IComparer<IModManifest> ModVersionComparer = 
        Comparer<IModManifest>.Create((x, y) => 
        {
            // Descending sort: compare y to x instead of x to y
            return Parse(y.Version).CompareTo(Parse(x.Version));

            Version Parse(string v) {
                if (string.IsNullOrEmpty(v)) return new Version(0, 0, 0);
                // Split once for both '+' and '-' metadata
                var clean = v.Split(['+', '-'], StringSplitOptions.RemoveEmptyEntries)[0];
                return Version.TryParse(clean, out var parsed) ? parsed : new Version(0, 0, 0);
            }
        });
    
    #endregion
    #endregion
    
    #region Modpack
    
    [ObservableProperty] private bool _modpackAllowScrollbarRefresh = false;
    [ObservableProperty] private ObservableCollection<ModPackModel> _modpacks = new();
    public ObservableCollection<string> ModpackVersionFilterSource { get; } = new();
    [ObservableProperty] private int _modpackVersionFilterIndex = -1;
    
    [ObservableProperty] private string _modpackSearchQuery = string.Empty;

    [ObservableProperty] private EMinecraftKind _modpackModLoader = EMinecraftKind.VANILLA;
    
    [ObservableProperty] private string? _modpackMinecraftVersion;

    [ObservableProperty] private bool _modpackCategoryAdventure;
    [ObservableProperty] private bool _modpackCategoryChallenging;
    [ObservableProperty] private bool _modpackCategoryCombat;
    [ObservableProperty] private bool _modpackCategoryKitchenSink;
    [ObservableProperty] private bool _modpackCategoryLightweight;
    [ObservableProperty] private bool _modpackCategoryMagic;
    [ObservableProperty] private bool _modpackCategoryMultiplayer;
    [ObservableProperty] private bool _modpackCategoryOptimization;
    [ObservableProperty] private bool _modpackCategoryQuests;
    [ObservableProperty] private bool _modpackCategoryTechnology;
    
    [ObservableProperty] private EPlatformType _selectedModpackPlatform = EPlatformType.Modrinth;
    
    [ObservableProperty] [NotifyPropertyChangedFor(nameof(ModpackPreview))] private ModPackModel? _selectedModpack;
    
    public List<EPlatformType> AvailableModpackPlatforms =>
    [
        EPlatformType.Modrinth,
        EPlatformType.CurseForge,
        EPlatformType.Technic,
        EPlatformType.FTB
    ];
    
    public string? ModpackPreview => SelectedModpack == null ? "<p>" + TranslationManager.Translate("instance.create.modpack.preview.select") +"</p>" : SelectedModpack.RawPage;

    #endregion

    #region Import
    [ObservableProperty] [NotifyPropertyChangedFor(nameof(IsSourceFromFile))] private int _selectedImportSourceIndex = 0;
    
    public bool IsSourceFromFile => SelectedImportSourceIndex == 0;

    [ObservableProperty] private string? _importPath;

    [ObservableProperty] private bool _hasImportPath;

    [ObservableProperty] private string _importPreviewName = "---";
    
    [ObservableProperty] private string _importPreviewVersion = "---";
    
    [ObservableProperty] private string _importPreviewModLoader = "---";
    #endregion

    public CreateInstanceViewModel()
    {
        if (Design.IsDesignMode)
        {
            _instanceIcon = ImageHelper.Load("avares://Desktop/Assets/Icons/dirt.png").Result;
            return;
        }
        
        var sw = System.Diagnostics.Stopwatch.StartNew();
        _minecraftVersionCache = new SourceCache<MinecraftVersion, string>(x => x.Id);

        SetupPipeline();
        _ = InitAsync();
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
        _modLoaderVersionCache.Clear();
        _modLoaderVersionCache.Dispose();
        InstanceName = string.Empty;
        InstanceGroup = string.Empty;
        InstanceIcon?.Dispose();
        InstanceIcon = null;
        InstanceIconPath = null;
        SearchQuery = string.Empty;
        SelectedMinecraftVersion = null;
        
        ModLoaderSearchQuery = string.Empty;
        SelectedModLoader = null;
    }

    private async Task InitAsync()
    {
        await Task.Yield();
        
        var settings = await Task.Run(() => LauncherHelper.GetLauncherSettingsAsync());
        var manifestPath = settings.Launcher.GetVanillaManifestPath();
        var versionManifest = await Task.Run(() => ManifestHelper.GetMinecraftManifestAsync(manifestPath));

        if (versionManifest == null)
            throw new Exception("Failed to load Minecraft version manifest.");
        
        List<IModManifest>? fabricManifestCache= await ManifestHelper.GetFabricManifestAsync(settings.Launcher.GetFabricManifestPath());
        List<IModManifest>? forgeManifestCache= await ManifestHelper.GetForgeManifestAsync(settings.Launcher.GetForgeManifestPath());
        List<IModManifest>? neoForgeManifestCache= await ManifestHelper.GetNeoForgeManifestAsync(settings.Launcher.GetNeoForgeManifestPath());
        List<IModManifest>? quiltManifestCache = await ManifestHelper.GetQuiltManifestAsync(settings.Launcher.GetQuiltManifestPath());
        
        _minecraftVersionCache.Edit(innerCache =>
        {
            innerCache.Clear();
            innerCache.AddOrUpdate(versionManifest.Versions);
        });
        
        // Populate ModLoader cache
        _modLoaderVersionCache.Edit(innerCache =>
        {
            innerCache.Clear();
            if (neoForgeManifestCache != null)
                innerCache.AddOrUpdate(neoForgeManifestCache);
            if (forgeManifestCache != null)
                innerCache.AddOrUpdate(forgeManifestCache);
            if (fabricManifestCache != null)
                innerCache.AddOrUpdate(fabricManifestCache);
            if (quiltManifestCache != null)
                innerCache.AddOrUpdate(quiltManifestCache);
        });
        
        var icon = await Task.Run(() => ImageHelper.Load("avares://Desktop/Assets/Icons/dirt.png"));
        Dispatcher.UIThread.Post(() =>
        {
            InstanceIcon = icon;
            SelectedMinecraftVersion = MinecraftVersions.FirstOrDefault();
            ModpackVersionFilterSource.Add("Any");
            ModpackVersionFilterIndex = 0;
        });

        /*foreach (var version in versionManifest.Versions)
        {
            if (version.Type != "release")
                continue;
            ModpackVersionFilterSource.Add(version.Id);
        }

        var response = await ModrinthHelper.SearchModpacksAsync();
        if (response == null)
            throw new Exception("Modrinth search failed.");

        var tasks = response.Hits.Select(ModPackModel.FromModrinthProjectAsync);
        
        var results = await Task.WhenAll(tasks);
        
        foreach (var model in results)
            Modpacks.Add(model);

        ModpackAllowScrollbarRefresh = true;*/
    }

    private void SetupPipeline()
    {
        var search = this.WhenAnyValue(x => x.SearchQuery)
            .Throttle(TimeSpan.FromMilliseconds(150), TaskPoolScheduler.Default);
        
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
                .ObserveOn(RxSchedulers.MainThreadScheduler)
                .SortAndBind(out var filteredCollection, SortExpressionComparer<MinecraftVersion>.Descending(x => x.ReleaseTime))
                .Subscribe(
                    _ => { },
                    ex => _logger.Error($"Version pipeline crashed: {ex}")
                );

        Disposables.Add(bindingSubscription);
        MinecraftVersions = filteredCollection;
        
        // Setup ModLoader version filtering pipeline
        var modLoaderFilter = this.WhenAnyValue(
                x => x.ModLoaderType,
                x => x.SelectedMinecraftVersion,
                x => x.ModLoaderSearchQuery
            )
            .Throttle(TimeSpan.FromMilliseconds(100), TaskPoolScheduler.Default)
            .Select(_ =>
            {
                if (SelectedMinecraftVersion == null)
                    return (Func<IModManifest, bool>)(_ => false);

                var selectedVersion = SelectedMinecraftVersion.Id;
                var searchQuery = ModLoaderSearchQuery;
                var modLoaderType = ModLoaderType;

                return (Func<IModManifest, bool>)(manifest =>
                {
                    // Return empty if no mod loader is selected or the mod loader type does not match
                    if (modLoaderType == EMinecraftKind.VANILLA || modLoaderType != manifest.LoaderKind)
                        return false;

                    // Filter by mod loader type
                    switch (modLoaderType)
                    {
                        case EMinecraftKind.NEOFORGE:
                        case EMinecraftKind.FORGE:
                            if (manifest.GameVersion != selectedVersion)
                                return false;
                            break;
                        case EMinecraftKind.FABRIC:
                        case EMinecraftKind.QUILT:
                            break;
                        default:
                            return false;
                    }

                    // Filter by search query
                    if (!string.IsNullOrEmpty(searchQuery) && !manifest.Version.StartsWith(searchQuery))
                        return false;

                    return true;
                });
            });
        
        var modLoaderSubscription = _modLoaderVersionCache
            .Connect()
            .Filter(modLoaderFilter)
            .ObserveOn(RxSchedulers.MainThreadScheduler)
            .SortAndBind(out var filteredModLoaders, ModVersionComparer)
            .Subscribe(
                _ => { },
                ex => _logger.Error($"ModLoader pipeline crashed: {ex}")
            );

        Disposables.Add(modLoaderSubscription);
        ModLoaderVersionResult = filteredModLoaders;
    }
    
    public async Task RefreshModpacksAsync(bool resetSearch = false)
    {
        ModpackAllowScrollbarRefresh = false;
        string? version = ModpackMinecraftVersion is null or "Any" ? null : ModpackMinecraftVersion;

        List<string> categories = [];
        if (ModpackModLoader != EMinecraftKind.VANILLA)
            categories.Add(ModpackModLoader.ToString().ToLower());
        if (ModpackCategoryAdventure)
            categories.Add("adventure");
        if (ModpackCategoryChallenging)
            categories.Add("challenging");
        if (ModpackCategoryCombat)
            categories.Add("combat");
        if (ModpackCategoryKitchenSink)
            categories.Add("kitchen_sink");
        if (ModpackCategoryLightweight)
            categories.Add("lightweight");
        if (ModpackCategoryMagic)
            categories.Add("magic");
        if (ModpackCategoryMultiplayer)
            categories.Add("multiplayer");
        if (ModpackCategoryOptimization)
            categories.Add("optimization");
        if (ModpackCategoryQuests)
            categories.Add("quests");
        if (ModpackCategoryTechnology)
            categories.Add("technology");
        
        var response = await ModrinthHelper.SearchModpacksAsync(ModpackSearchQuery, version, categories, Modpacks.Count);
        if (response == null)
            throw new Exception("Modrinth search failed.");

        var tasks = response.Hits.Select(ModPackModel.FromModrinthProjectAsync);
        
        var results = await Task.WhenAll(tasks);
        
        if (resetSearch)
            Modpacks.Clear();
        foreach (var model in results)
            Modpacks.Add(model);
        
        ModpackAllowScrollbarRefresh = true;
    }
    
    #region Commands

    #region Window
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

    #region Common

    [RelayCommand]
    private async Task HandleTabBtn(ECreateInstanceTab tab) => await UpdateSelectedTabButton.Handle(tab);

    #endregion
    
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
        if (SelectedMinecraftVersion == null)
            return;
        
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
            MinecraftVersion = SelectedMinecraftVersion?.Id!,
            CustomVersion = SelectedModLoader?.Version ?? string.Empty,
            IconPath = InstanceIconPath ?? string.Empty,
            GameDirectory = Path.Combine(settings.Launcher.InstancesDirectoryPath, InstanceName),
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
        await CloseWindowInteraction.Handle(Unit.Default);
    }
    
    /// <summary>
    /// Cancels the custom instance creation process and closes the parent window.
    /// </summary>
    [RelayCommand]
    private async Task CustomCancelCreate() => await CloseWindowInteraction.Handle(Unit.Default);

    #endregion

    #region Modpack

    partial void OnModpackSearchQueryChanged(string value)
    {
        if (!ModpackAllowScrollbarRefresh)
            return;
        
        Dispatcher.UIThread.Invoke(async () => await RefreshModpacksAsync(true));
    }

    partial void OnSelectedModpackChanged(ModPackModel? value)
    {
        
    }

    #endregion
    
    #region Import

    partial void OnImportPathChanged(string? value)
    {
        if (IsSourceFromFile)
            return;

        if (Uri.TryCreate(value, UriKind.Absolute, out _))
        {
            HasImportPath = true;
            if (ImportUrlTextChangedCommand.CanExecute(value))
                ImportUrlTextChangedCommand.Execute(value);
        }
        else
        {
            HasImportPath = false;
        }
    }
    
    [RelayCommand]
    private async Task ChangeImportType(int index) => await UpdateSelectedImportTypeButton.Handle(index);

    [RelayCommand]
    private async Task SelectFileToImport()
    {
        string? path = await ShowFileSelector.Handle(Unit.Default);
        if (string.IsNullOrEmpty(path))
        {
            HasImportPath = false;
            return;
        }

        if (!File.Exists(path))
        {
            HasImportPath = false;
            return;
        }

        HasImportPath = true;
        ImportPath = path;
        await FetchImportPreviewFromFile();
    }
    
    [RelayCommand]
    private async Task ImportUrlTextChanged(string path) => await FetchImportPreviewFromUrl();

    [RelayCommand]
    private async Task CreateInstanceFromImport()
    {
        // TODO
    }
    #endregion
    
    #endregion

    private async Task FetchImportPreviewFromFile()
    {
        // TODO
    }
    
    private async Task FetchImportPreviewFromUrl()
    {
        // TODO
    }
}