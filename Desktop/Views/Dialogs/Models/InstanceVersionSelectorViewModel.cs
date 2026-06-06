using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DynamicData;
using DynamicData.Binding;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;
using Tavstal.KonkordLauncher.Common.Models;
using Tavstal.KonkordLauncher.Common.Services.Abstractions;
using Tavstal.KonkordLauncher.Core.Enums;
using Tavstal.KonkordLauncher.Core.Models.Logging;
using Tavstal.KonkordLauncher.Core.Models.ModLoaders;
using Tavstal.KonkordLauncher.Core.Models.MojangApi;
using Tavstal.KonkordLauncher.Core.Services.Abstractions;
using Tavstal.KonkordLauncher.Desktop.Models.Avalonia;

namespace Tavstal.KonkordLauncher.Desktop.Views.Dialogs.Models;

public partial class InstanceVersionSelectorViewModel : KonkordObservableObject
{
    private readonly Instance _instance;
    private readonly ICustomLogger _logger;
    private readonly ILauncherStore _launcherStore;
    private readonly IManifestService _manifestService;
    public EMinecraftKind Kind { get; }
    public bool IsModded => Kind != EMinecraftKind.VANILLA;
    
    public bool CanSaveChanges
    {
        get
        {
            // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
            if (SelectedMinecraftVersion == null)
                return false;
            
            if (Kind != EMinecraftKind.VANILLA && SelectedModLoader == null)
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

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanSaveChanges))]
    public partial MinecraftVersion? SelectedMinecraftVersion { get; set; }
    
    #endregion
    #region  Mod Loader
    [ObservableProperty]
    public partial string ModLoaderSearchQuery { get; set; } = string.Empty;
    
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanSaveChanges))]
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
    
    public Interaction<bool, Unit> CloseWindowInteraction { get; } = new();

    public InstanceVersionSelectorViewModel(Instance instance)
    {
        _instance = instance;
        Kind = instance?.Kind ?? EMinecraftKind.VANILLA;
        if (Design.IsDesignMode)
            return;

        var services = Program.ServiceProvider;
        _logger = services.GetRequiredService<ICustomLogger<InstanceVersionSelectorViewModel>>();
        _launcherStore = services.GetRequiredService<ILauncherStore>();
        _manifestService = services.GetRequiredService<IManifestService>();
        
        SetupPipeline();
        _ = InitAsync().ContinueWith(t =>
        {
            if (t.IsFaulted)
                _logger.LogError(t.Exception, "Failed to initialize InstanceVersionSelectorViewModel:");
        }, TaskScheduler.Default);
    }
    
    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        _minecraftVersionCache.Clear();
        _minecraftVersionCache.Dispose();
        _modLoaderVersionCache.Clear();
        _modLoaderVersionCache.Dispose();
        SearchQuery = string.Empty;
        SelectedMinecraftVersion = null;
        
        ModLoaderSearchQuery = string.Empty;
        SelectedModLoader = null;
    }
    
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
                x => x.Kind,
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
                var modLoaderType = Kind;
                
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
    
    public async Task InitAsync(CancellationToken cancellationToken = default)
    {
        var settings = _launcherStore.GetSettings() ?? throw new InvalidOperationException("Settings cannot be null");
        var versionManifest = _manifestService.GetMinecraftManifest() ?? throw new InvalidOperationException("Failed to fetch Minecraft version manifest");
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
        
        await MinecraftVersions
            .ToObservableChangeSet()
            .ToCollection()
            .Where(c => c.Count > 0)
            .Take(1)
            .ToTask(cancellationToken);

        SelectedMinecraftVersion = MinecraftVersions
                                       .FirstOrDefault(v => v.Id == _instance?.MinecraftVersion)
                                   ?? MinecraftVersions.FirstOrDefault(v => v.Type == "release");
        
        await ModLoaderVersionResult
            .ToObservableChangeSet()
            .ToCollection()
            .Where(c => c.Count > 0 || Kind == EMinecraftKind.VANILLA)
            .Take(1)
            .ToTask(cancellationToken);
        
        SelectedModLoader = ModLoaderVersionResult
                                .FirstOrDefault(m => m.Version == _instance?.CustomVersion && m.LoaderKind == Kind);

    }
    
    [RelayCommand]
    private async Task Close() => await CloseWindowInteraction.Handle(false);

    [RelayCommand]
    private async Task Save()
    {
        if (!CanSaveChanges)
            return;

        var instances = await _launcherStore.GetInstancesAsync();
        int instanceIndex = instances.FindIndex(x => x.Id == _instance.Id);
        if (instanceIndex == -1)
            return;
        
        _instance.MinecraftVersion = SelectedMinecraftVersion!.Id;
        if (Kind !=  EMinecraftKind.VANILLA)
            _instance.CustomVersion = SelectedModLoader!.Version;
        instances[instanceIndex] = _instance;
        await _launcherStore.SaveInstancesAsync(instances);
        GlobalEvents.InvokeInstanceUpdated(_instance.Id);
        await CloseWindowInteraction.Handle(true);
    }
}