using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DynamicData;
using DynamicData.Binding;
using Tavstal.BeaconSDK.Enums;
using Tavstal.BeaconSDK.Models.Logging;
using Tavstal.BeaconSDK.Models.ModLoaders;
using Tavstal.BeaconSDK.Models.MojangApi;
using Tavstal.BeaconSDK.Services.Abstractions;
using Tavstal.KonkordLauncher.Desktop.Helpers;
using Tavstal.KonkordLauncher.Desktop.Models.Avalonia;
using Tavstal.KonkordLauncher.Desktop.Models.DTOs;
using Tavstal.KonkordLauncher.Desktop.Services;
using Tavstal.KonkordLauncher.Desktop.Services.Storage;

namespace Tavstal.KonkordLauncher.Desktop.Views.Dialogs.Models;

/// <summary>
///     The view model for choosing an instance version and mod loader.
/// </summary>
public partial class InstanceVersionSelectorViewModel : KonkordObservableObject
{
    private readonly IEventService _eventService = null!;
    private readonly Instance _instance;
    private readonly ILauncherStore _launcherStore = null!;
    private readonly IBeaconLogger _logger = null!;
    private readonly IManifestService _manifestService = null!;

    /// <summary>
    ///     Initializes a new instance for the designer.
    /// </summary>
    public InstanceVersionSelectorViewModel()
    {
        _instance = null!;
        Kind = EMinecraftKind.VANILLA;
        MinecraftVersions =
            new ReadOnlyObservableCollection<MinecraftVersion>([]);
    }

    /// <summary>
    ///     Initializes a new instance for the given instance.
    /// </summary>
    /// <param name="instance">The instance being edited.</param>
    /// <param name="logger">The logger used by the view model.</param>
    /// <param name="launcherStore">The store that reads and saves launcher data.</param>
    /// <param name="manifestService">The service that provides version manifests.</param>
    /// <param name="eventService">The service that raises instance events.</param>
    public InstanceVersionSelectorViewModel(Instance instance,
        IBeaconLogger<InstanceVersionSelectorViewModel> logger, ILauncherStore launcherStore,
        IManifestService manifestService, IEventService eventService)
    {
        _instance = instance;
        Kind = instance?.Kind ?? EMinecraftKind.VANILLA;

        if (Design.IsDesignMode)
            return;

        _logger = logger;
        _launcherStore = launcherStore;
        _manifestService = manifestService;
        _eventService = eventService;

        SetupPipeline();
        _ = InitAsync().ContinueWith(t =>
        {
            if (t.IsFaulted)
                _logger.LogError(t.Exception, "Failed to initialize InstanceVersionSelectorViewModel:");
        }, TaskScheduler.Default);
    }

    /// <summary>
    ///     Gets the mod loader kind of the instance.
    /// </summary>
    public EMinecraftKind Kind { get; }

    /// <summary>
    ///     Gets a value that indicates whether the instance uses a mod loader.
    /// </summary>
    public bool IsModded => Kind != EMinecraftKind.VANILLA;

    /// <summary>
    ///     Gets a value that indicates whether the selected changes can be saved.
    /// </summary>
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

    /// <summary>
    ///     Gets the interaction that closes the dialog.
    /// </summary>
    public Interaction<bool, Unit> CloseWindowInteraction { get; } = new();

    /// <summary>
    ///     Releases the resources used by this view model.
    /// </summary>
    /// <param name="disposing">True to release managed resources.</param>
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

    /// <summary>
    ///     Sets up the filters for the version and mod loader lists.
    /// </summary>
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

        var filter = search.CombineLatest(toggles,
                (query, t) => new
                {
                    Query = query,
                    ShowReleases = t.Property1,
                    ShowSnapshots = t.Property2,
                    ShowAlphas = t.Property3,
                    ShowBetas = t.Property4,
                    ShowExperiments = t.Property5
                })
            .Select(values =>
            {
                return (Func<MinecraftVersion, bool>)(x =>
                        (string.IsNullOrWhiteSpace(values.Query) || x.Id.StartsWith(values.Query)) &&
                        (x.Type != "release" || values.ShowReleases) &&
                        (x.Type != "snapshot" || values.ShowSnapshots) &&
                        (x.Type != "old_alpha" || values.ShowAlphas) &&
                        (x.Type != "old_beta" || values.ShowBetas) &&
                        (x.Type != "experiment" || values.ShowExperiments)
                    );
            });

        var bindingSubscription =
            _minecraftVersionCache
                .Connect()
                .Filter(filter)
                .ObserveOnMainThread()
                .SortAndBind(out var filteredCollection,
                    SortExpressionComparer<MinecraftVersion>.Descending(x => x.ReleaseTime))
                .Subscribe(
                    _ => { },
                    ex => _logger.LogError(ex, "Version pipeline crashed:")
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

                    // Filter by game version
                    if (!manifest.EqualsGameVersion(selectedVersion))
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
            .ObserveOnMainThread()
            .SortAndBind(out var filteredModLoaders, ModVersionComparer)
            .Subscribe(
                _ => { },
                ex => _logger.LogError(ex, "ModLoader pipeline crashed:")
            );

        Disposables.Add(modLoaderSubscription);
        ModLoaderVersionResult = filteredModLoaders;

        #endregion
    }

    /// <summary>
    ///     Loads the available Minecraft versions and mod loader versions.
    /// </summary>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>A task that completes when the lists are loaded.</returns>
    public async Task InitAsync(CancellationToken cancellationToken = default)
    {
        var settings = _launcherStore.GetSettings() ?? throw new InvalidOperationException("Settings cannot be null");
        var versionManifest = _manifestService.GetMinecraftManifest() ??
                              throw new InvalidOperationException("Failed to fetch Minecraft version manifest");
        var fabricManifestCache =
            await _manifestService.GetFabricManifestAsync(settings.Launcher.GetFabricManifestPath(), cancellationToken);
        var forgeManifestCache =
            await _manifestService.GetForgeManifestAsync(settings.Launcher.GetForgeManifestPath(), cancellationToken);
        var neoForgeManifestCache =
            await _manifestService.GetNeoForgeManifestAsync(settings.Launcher.GetNeoForgeManifestPath());
        var quiltManifestCache =
            await _manifestService.GetQuiltManifestAsync(settings.Launcher.GetQuiltManifestPath(), cancellationToken);

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
            .ToTaskCompat(cancellationToken);

        SelectedMinecraftVersion = MinecraftVersions
                                       .FirstOrDefault(v => v.Id == _instance.MinecraftVersion)
                                   ?? MinecraftVersions.FirstOrDefault(v => v.Type == "release");

        await ModLoaderVersionResult
            .ToObservableChangeSet()
            .ToCollection()
            .Where(c => c.Count > 0 || Kind == EMinecraftKind.VANILLA)
            .Take(1)
            .ToTaskCompat(cancellationToken);

        SelectedModLoader = ModLoaderVersionResult
            .FirstOrDefault(m => m.Version == _instance.CustomVersion && m.LoaderKind == Kind);
    }

    /// <summary>
    ///     Closes the dialog.
    /// </summary>
    [RelayCommand]
    private async Task Close()
    {
        await CloseWindowInteraction.Handle(false);
    }

    /// <summary>
    ///     Saves the selected instance settings.
    /// </summary>
    [RelayCommand]
    private async Task Save()
    {
        if (!CanSaveChanges)
            return;

        if ((_instance.MinecraftVersion == SelectedMinecraftVersion!.Id && Kind == EMinecraftKind.VANILLA)
            || (Kind != EMinecraftKind.VANILLA && _instance.CustomVersion == SelectedModLoader?.Version))
        {
            await CloseWindowInteraction.Handle(true);
            return;
        }

        var instances = await _launcherStore.GetInstancesAsync();
        var instanceIndex = instances.FindIndex(x => x.Id == _instance.Id);
        if (instanceIndex == -1)
            return;

        _instance.MinecraftVersion = SelectedMinecraftVersion!.Id;
        if (Kind != EMinecraftKind.VANILLA)
            _instance.CustomVersion = SelectedModLoader!.Version;
        instances[instanceIndex] = _instance;
        await _launcherStore.SaveInstancesAsync(instances);
        _eventService.RaiseInstanceUpdated(_instance.Id);
        await CloseWindowInteraction.Handle(true);
    }

    #region Vanilla

    private readonly SourceCache<MinecraftVersion, string> _minecraftVersionCache = new(x => x.Id);

    /// <summary>
    ///     Gets the Minecraft versions that match the current filters.
    /// </summary>
    public ReadOnlyObservableCollection<MinecraftVersion> MinecraftVersions { get; private set; } = null!;

    /// <summary>
    ///     Gets or sets the text used to search Minecraft versions.
    /// </summary>
    [ObservableProperty] public partial string SearchQuery { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets a value that indicates whether release versions are shown.
    /// </summary>
    [ObservableProperty] public partial bool ShowReleases { get; set; } = true;

    /// <summary>
    ///     Gets or sets a value that indicates whether snapshot versions are shown.
    /// </summary>
    [ObservableProperty] public partial bool ShowSnapshots { get; set; }

    /// <summary>
    ///     Gets or sets a value that indicates whether old alpha versions are shown.
    /// </summary>
    [ObservableProperty] public partial bool ShowAlphas { get; set; }

    /// <summary>
    ///     Gets or sets a value that indicates whether old beta versions are shown.
    /// </summary>
    [ObservableProperty] public partial bool ShowBetas { get; set; }

    /// <summary>
    ///     Gets or sets a value that indicates whether experiment versions are shown.
    /// </summary>
    [ObservableProperty] public partial bool ShowExperiments { get; set; }

    /// <summary>
    ///     Gets or sets the selected Minecraft version.
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanSaveChanges))]
    public partial MinecraftVersion? SelectedMinecraftVersion { get; set; }

    #endregion

    #region Mod Loader

    /// <summary>
    ///     Gets or sets the text used to search mod loader versions.
    /// </summary>
    [ObservableProperty] public partial string ModLoaderSearchQuery { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the selected mod loader version.
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanSaveChanges))]
    public partial IModManifest? SelectedModLoader { get; set; }

    private readonly SourceCache<IModManifest, string> _modLoaderVersionCache =
        new(x => $"{x.LoaderKind}:{x.GameVersion}:{x.Version}");

    /// <summary>
    ///     Gets the mod loader versions that match the current filters.
    /// </summary>
    public ReadOnlyObservableCollection<IModManifest> ModLoaderVersionResult { get; private set; } = new([]);

    private static readonly IComparer<IModManifest> ModVersionComparer =
        Comparer<IModManifest>.Create((x, y) =>
        {
            // Descending sort: compare y to x instead of x to y
            return Parse(y.Version).CompareTo(Parse(x.Version));

            Version Parse(string v)
            {
                if (string.IsNullOrEmpty(v)) return new Version(0, 0, 0);
                // Split once for both '+' and '-' metadata
                var clean = v.Split(['+', '-'], StringSplitOptions.RemoveEmptyEntries)[0];
                return Version.TryParse(clean, out var parsed) ? parsed : new Version(0, 0, 0);
            }
        });

    #endregion
}
