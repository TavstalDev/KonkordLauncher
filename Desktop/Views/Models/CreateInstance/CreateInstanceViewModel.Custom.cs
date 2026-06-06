using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DynamicData;
using DynamicData.Binding;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;
using Tavstal.KonkordLauncher.Common.Models;
using Tavstal.KonkordLauncher.Common.Models.Config;
using Tavstal.KonkordLauncher.Common.Models.InstanceConfig;
using Tavstal.KonkordLauncher.Common.Services.Abstractions;
using Tavstal.KonkordLauncher.Core.Enums;
using Tavstal.KonkordLauncher.Core.Models.Logging;
using Tavstal.KonkordLauncher.Core.Models.ModLoaders;
using Tavstal.KonkordLauncher.Core.Models.MojangApi;
using Tavstal.KonkordLauncher.Core.Services.Abstractions;
using Tavstal.KonkordLauncher.Desktop.Helpers;
using Tavstal.KonkordLauncher.Desktop.Models.Avalonia;
using Tavstal.KonkordLauncher.Desktop.Models.Domain;
using Tavstal.KonkordLauncher.Desktop.Models.Enums;

namespace Tavstal.KonkordLauncher.Desktop.Views.Models.CreateInstance;

/// <summary>
/// Partial view-model that encapsulates the "Custom" tab logic for creating an instance in the UI.
/// </summary>
public partial class CreateInstanceViewModel_Custom : KonkordObservableObject
{
    private readonly ICustomLogger _logger;
    private readonly ITranslationService _translationService;
    private readonly ILauncherStore _launcherStore;
    private readonly IBitmapService _bitmapService;
    private readonly IManifestService _manifestService;
    private readonly CreateInstanceViewModel _parent;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanCreateCustomInstance))]
    public partial string InstanceName { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanCreateCustomInstance))]
    public partial string InstanceGroup { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string? InstanceIconPath { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanCreateCustomInstance))]
    public partial BitmapEntry InstanceIcon { get; set; }

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
    [ObservableProperty]
    public partial string SearchQuery { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool ShowReleases { get; set; } = true;

    [ObservableProperty]
    public partial bool ShowSnapshots { get; set; }

    [ObservableProperty]
    public partial bool ShowAlphas { get; set; }

    [ObservableProperty]
    public partial bool ShowBetas { get; set; }

    [ObservableProperty]
    public partial bool ShowExperiments { get; set; }

    [ObservableProperty] [NotifyPropertyChangedFor(nameof(CanCreateCustomInstance))] private MinecraftVersion? _selectedMinecraftVersion;
    #endregion
    #region  Mod Loader
    [ObservableProperty]
    public partial string ModLoaderSearchQuery { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanCreateCustomInstance))]
    public partial EMinecraftKind ModLoaderType { get; set; } = EMinecraftKind.VANILLA;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanCreateCustomInstance))]
    public partial IModManifest? SelectedModLoader { get; set; }

    private readonly SourceCache<IModManifest, string> _modLoaderVersionCache = new(x => $"{x.LoaderKind}:{x.GameVersion}:{x.Version}");
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
    
    /// <summary>
    /// Initializes a new instance of <see cref="CreateInstanceViewModel_Custom"/> and captures a reference to its parent view-model.
    /// </summary>
    /// <param name="parent">The parent <see cref="CreateInstanceViewModel"/> instance that owns or composes this custom sub-view-model.</param>
    public CreateInstanceViewModel_Custom(CreateInstanceViewModel parent)
    {
        _parent = parent;
        InstanceIcon = new BitmapEntry(null, null);
        if (Design.IsDesignMode)
        {
            InstanceIcon = ImageHelper.LoadDesignTime("avares://KonkordLauncher/Assets/Icons/dirt.png");
            return;
        }
        
        var services = Program.ServiceProvider;
        _logger = services.GetRequiredService<ICustomLogger<CreateInstanceViewModel_Custom>>();
        _translationService = services.GetRequiredService<ITranslationService>();
        _launcherStore = services.GetRequiredService<ILauncherStore>();
        _bitmapService = services.GetRequiredService<IBitmapService>();
        _manifestService = services.GetRequiredService<IManifestService>();
    }

    /// <summary>
    /// Releases resources used by this view-model and resets transient state.
    /// </summary>
    /// <param name="disposing">True when called from user code / IDisposable.Dispose; false when called from a finalizer.</param>
    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        _minecraftVersionCache.Clear();
        _minecraftVersionCache.Dispose();
        _modLoaderVersionCache.Clear();
        _modLoaderVersionCache.Dispose();
        InstanceName = string.Empty;
        InstanceGroup = string.Empty;
        InstanceIcon.Dispose(_bitmapService);
        InstanceIconPath = null;
        SearchQuery = string.Empty;
        SelectedMinecraftVersion = null;
        
        ModLoaderSearchQuery = string.Empty;
        SelectedModLoader = null;
    }

    /// <summary>
    /// Sets up the reactive DynamicData pipelines used to provide filtered, sorted, and UI-bound collections
    /// for the UI.
    /// </summary>
    [RequiresUnreferencedCode( "Trimming may break this functionality if not configured to preserve the necessary members.")]
    public void SetupPipeline()
    {
        #region Minecraft Version
        
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
                    ex => _logger.LogError(ex, $"Version pipeline crashed:")
                );

        Disposables.Add(bindingSubscription);
        MinecraftVersions = filteredCollection;
        
        #endregion

        #region Mod Loader
        
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
                    if ((modLoaderType == EMinecraftKind.NEOFORGE || modLoaderType == EMinecraftKind.QUILT) &&
                        manifest.GameVersion != selectedVersion)
                        return false;

                    // Filter by search query
                    if (string.IsNullOrEmpty(searchQuery))
                        return true;
                    
                    return manifest.Version.StartsWith(searchQuery);
                });
            });
        
        var modLoaderSubscription = _modLoaderVersionCache
            .Connect()
            .Filter(modLoaderFilter)
            .ObserveOn(RxSchedulers.MainThreadScheduler)
            .SortAndBind(out var filteredModLoaders, ModVersionComparer)
            .Subscribe(
                _ => { },
                ex => _logger.LogError(ex, $"ModLoader pipeline crashed:")
            );

        Disposables.Add(modLoaderSubscription);
        ModLoaderVersionResult = filteredModLoaders;
        
        #endregion
    }

    /// <summary>
    /// Initializes the view-model's version and modloader data caches and sets default UI state (icon and selected version).
    /// </summary>
    /// <param name="settings">The application's <see cref="CoreConfig"/> containing launcher paths (used to locate manifests).</param>
    /// <param name="versionManifest">The already-loaded <see cref="VersionManifest"/> (vanilla Minecraft versions) to populate the vanilla versions cache.</param>
    /// <param name="cancellationToken">Optional <see cref="CancellationToken"/> used to cancel asynchronous IO operations started by this method.</param>
    /// <returns>A <see cref="Task"/> that completes when initialization has finished.</returns>
    public async Task InitAsync(CoreConfig settings, VersionManifest versionManifest,  CancellationToken cancellationToken = default)
    {
        List<IModManifest>? fabricManifestCache = await _manifestService.GetFabricManifestAsync(settings.Launcher.GetFabricManifestPath(), cancellationToken);
        List<IModManifest>? forgeManifestCache = await _manifestService.GetForgeManifestAsync(settings.Launcher.GetForgeManifestPath(), cancellationToken);
        List<IModManifest>? neoForgeManifestCache = await _manifestService.GetNeoForgeManifestAsync(settings.Launcher.GetNeoForgeManifestPath());
        List<IModManifest>? quiltManifestCache = await _manifestService.GetQuiltManifestAsync(settings.Launcher.GetQuiltManifestPath(), cancellationToken);
        
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
        
        var icon = await _bitmapService.GetBitmapAsync("avares://KonkordLauncher/Assets/Icons/dirt.png");
        Dispatcher.UIThread.Post(() =>
        {
            InstanceIcon = icon;
        });
        
        await MinecraftVersions
            .ToObservableChangeSet()
            .ToCollection()
            .Where(c => c.Count > 0)
            .Take(1)
            .ToTask(cancellationToken);

        SelectedMinecraftVersion = MinecraftVersions.FirstOrDefault(v => v.Type == "release");
    }
    
    #region Commands
    /// <summary>
    /// Opens a window to select a custom icon for the instance. 
    /// If an icon is selected, it updates the instance's icon and its path.
    /// </summary>
    [RelayCommand]
    private async Task CustomIconSelectorAsync()
    {
        var result = await _parent.ShowIconSelectorInteraction.Handle(Unit.Default);
        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        if (result == null)
            return;
        //InstanceIcon?.Dispose();
        try
        {
            InstanceIcon = await _bitmapService.GetBitmapAsync(result);
        }
        catch
        {
            InstanceIcon = await _bitmapService.GetBitmapAsync("avares://KonkordLauncher/Assets/Icons/dirt.png");
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
        
        var settings = await _launcherStore.GetSettingsAsync();
        var instances = await _launcherStore.GetInstancesAsync();
        if (instances.Any(x => x.Name == InstanceName))
        {
            await _parent.ShowAlertDialogInteraction.Handle(new Alert(_translationService.Translate("instance.duplicate.title"),
                _translationService.Translate("instance.duplicate.message"),
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
        await _launcherStore.SaveInstancesAsync(instances);
        GlobalEvents.InvokeInstanceAdded(instances.Last().Id);
        await _parent.CloseWindowInteraction.Handle(Unit.Default);
    }
    
    /// <summary>
    /// Cancels the custom instance creation process and closes the parent window.
    /// </summary>
    [RelayCommand]
    private async Task CustomCancelCreate() => await _parent.CloseWindowInteraction.Handle(Unit.Default);

    #endregion
}