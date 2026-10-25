using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Reactive;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Modrinth.Models;
using Tavstal.BeaconSDK.Enums;
using Tavstal.BeaconSDK.Helpers.Serialization;
using Tavstal.BeaconSDK.Models.Logging;
using Tavstal.BeaconSDK.Services.Abstractions;
using Tavstal.KonkordLauncher.Desktop.Models.Avalonia;
using Tavstal.KonkordLauncher.Desktop.Models.DTOs;
using Tavstal.KonkordLauncher.Desktop.Models.Enums;
using Tavstal.KonkordLauncher.Desktop.Models.Instance;
using Tavstal.KonkordLauncher.Desktop.Serialization;
using Tavstal.KonkordLauncher.Desktop.Services.Content;
using Tavstal.KonkordLauncher.Desktop.Services.Localization;
using Tavstal.KonkordLauncher.Desktop.Services.Presentation;
using Tavstal.KonkordLauncher.Desktop.Services.Storage;
using Version = Modrinth.Models.Version;

namespace Tavstal.KonkordLauncher.Desktop.Views.Dialogs.Models;

/// <summary>
///     ViewModel for handling the resource download process.
/// </summary>
public partial class ResourceDownloadViewModel : KonkordObservableObject
{
    private readonly IBitmapService _bitmapService = null!;
    private readonly Instance? _instance;
    private readonly List<InstanceResource> _instanceResources = [];
    private readonly ILauncherStore _launcherStore = null!;
    private readonly IBeaconLogger _logger = null!;
    private readonly IManifestService _manifestService = null!;
    private readonly IMetaCacheService _metaCacheService = null!;
    private readonly EResourceType _resourceType;
    private readonly ITranslationService _translationService = null!;
    private readonly IWindowFactory _windowFactory;
    private Window _parent = null!;
    private CancellationTokenSource? _refreshCancellationTokenSource;
    private long _refreshGeneration;

    /// <summary>
    ///     Initializes a new instance of the ResourceDownloadViewModel class.
    /// </summary>
    /// <param name="parent">The parent window associated with this view model.</param>
    /// <param name="instance">The instance associated with this view model, or null if it's not an instance.</param>
    /// <param name="resourceType">Type of resource being downloaded, e.g., mod or resource pack.</param>
    public ResourceDownloadViewModel()
    {
        _instance = null;
        _resourceType = EResourceType.MOD;
        _windowFactory = null!;
        IsMod = false;
        ModLoader = EMinecraftKind.FABRIC;
        ModLoaderIndex = (int)ModLoader;
    }

    public ResourceDownloadViewModel(Instance? instance, EResourceType resourceType,
        IWindowFactory windowFactory, IBeaconLogger<ResourceDownloadViewModel> logger,
        ITranslationService translationService, ILauncherStore launcherStore,
        IManifestService manifestService, IMetaCacheService metaCacheService, IBitmapService bitmapService)
    {
        _instance = instance;
        _resourceType = resourceType;
        _windowFactory = windowFactory;
        _logger = logger;
        _translationService = translationService;
        _launcherStore = launcherStore;
        _manifestService = manifestService;
        _metaCacheService = metaCacheService;
        _bitmapService = bitmapService;
        IsMod = resourceType == EResourceType.MOD;
        ModLoader = _instance?.Kind ?? EMinecraftKind.FABRIC;
        ModLoaderIndex = (int)ModLoader;

        if (Design.IsDesignMode || instance == null)
            return;


        if (IsMod)
        {
            Categories!.Add(new CategoryModel { Name = "adventure", TranslationKey = "modrinth.category.adventure" });
            Categories.Add(new CategoryModel { Name = "cursed", TranslationKey = "modrinth.category.cursed" });
            Categories.Add(new CategoryModel { Name = "decoration", TranslationKey = "modrinth.category.decoration" });
            Categories.Add(new CategoryModel { Name = "economy", TranslationKey = "modrinth.category.economy" });
            Categories.Add(new CategoryModel { Name = "equipment", TranslationKey = "modrinth.category.equipment" });
            Categories.Add(new CategoryModel { Name = "food", TranslationKey = "modrinth.category.food" });
            Categories.Add(new CategoryModel
            {
                Name = "game_mechanics", TranslationKey = "modrinth.category.game_mechanics"
            });
            Categories.Add(new CategoryModel { Name = "library", TranslationKey = "modrinth.category.library" });
            Categories.Add(new CategoryModel { Name = "magic", TranslationKey = "modrinth.category.magic" });
            Categories.Add(new CategoryModel { Name = "management", TranslationKey = "modrinth.category.management" });
            Categories.Add(new CategoryModel { Name = "minigame", TranslationKey = "modrinth.category.minigame" });
            Categories.Add(new CategoryModel { Name = "mobs", TranslationKey = "modrinth.category.mobs" });
            Categories.Add(new CategoryModel
            {
                Name = "optimization", TranslationKey = "modrinth.category.optimization"
            });
            Categories.Add(new CategoryModel { Name = "social", TranslationKey = "modrinth.category.social" });
            Categories.Add(new CategoryModel { Name = "storage", TranslationKey = "modrinth.category.storage" });
            Categories.Add(new CategoryModel { Name = "technology", TranslationKey = "modrinth.category.technology" });
            Categories.Add(new CategoryModel
            {
                Name = "transportation", TranslationKey = "modrinth.category.transportation"
            });
            Categories.Add(new CategoryModel { Name = "utility", TranslationKey = "modrinth.category.utility" });
            Categories.Add(new CategoryModel { Name = "worldgen", TranslationKey = "modrinth.category.worldgen" });
        }

        if (Design.IsDesignMode)
            return;

        ResourcesToDownload.CollectionChanged += HandleResourcesToDownload_CollectionChanged;
        _ = InitAsync().ContinueWith(t =>
        {
            if (t.IsFaulted)
                _logger.LogError(t.Exception, "Failed to initialize ResourceDownloadViewModel:");
        }, TaskScheduler.Default);
    }

    public bool IsMod { get; }

    public bool? IsResourceSelected =>
        SelectedResource != null && ResourcesToDownload.ContainsKey(SelectedResource.Name);

    public string ModPreview => SelectedResource != null
        ? SelectedResource.RawPage
        : Design.IsDesignMode
            ? "<p>Select a resource to see its preview.</p>"
            : "<p>" + _translationService.Translate("instance.resource.download.preview") + "</p>";

    public AvaloniaDictionary<string, (Version version, string? iconUrl)> ResourcesToDownload { get; } = new();

    public List<EPlatformType> AvailablePlatforms =>
    [
        EPlatformType.MODRINTH,
        EPlatformType.CURSE_FORGE,
        EPlatformType.TECHNIC,
        EPlatformType.FTB
    ];

    /// <summary>
    ///     Interaction for closing the window.
    /// </summary>
    public Interaction<bool, Unit> CloseWindowInteraction { get; } = new();

    /// <summary>
    ///     Disposes of the resources associated with this view model.
    /// </summary>
    /// <param name="disposing">True if disposing resources, false otherwise.</param>
    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        ResourcesToDownload.CollectionChanged -= HandleResourcesToDownload_CollectionChanged;

        var resourcesCopy = Resources;
        Resources.Clear();
        foreach (var resource in resourcesCopy)
            resource.Icon.Dispose(_bitmapService);
        SelectedResource?.Icon.Dispose(_bitmapService);
        SelectedResource = null;
    }

    /// <summary>
    ///     Initializes the resource browser by loading the Minecraft version manifest, setting up the version filter,
    ///     ⬖ Getting started                ✕
    ///     reading installed instance resources, and performing the initial resource refresh.
    /// </summary>
    /// OpenCode includes free models
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    private async Task InitAsync(CancellationToken cancellationToken = default)
    {
        await Task.Yield();

        var settings = await Task.Run(() => _launcherStore.GetSettingsAsync(cancellationToken: cancellationToken),
            cancellationToken);
        var manifestPath = settings.Launcher.GetVanillaManifestPath();
        var versionManifest =
            await Task.Run(() => _manifestService.GetMinecraftManifestAsync(manifestPath, cancellationToken),
                cancellationToken);

        if (versionManifest == null)
            throw new Exception("Failed to load Minecraft version manifest.");

        var index = 0;
        var foundInstanceVersion = false;
        foreach (var version in versionManifest.Versions)
        {
            if (version.Type != "release")
                continue;
            VersionFilterSource.Add(version.Id);
            foundInstanceVersion = version.Id == _instance!.MinecraftVersion;
            if (foundInstanceVersion)
                continue;

            index++;
        }

        VersionFilterIndex = foundInstanceVersion ? index : 0;

        var configPath = _instance!.GetResourceConfigPath();
        var instanceResources = await JsonHelper.ReadJsonFileAsync<List<InstanceResource>>(configPath,
            CommonJsonContext.Default.ListInstanceResource, cancellationToken);
        if (instanceResources is { Count: > 0 })
            _instanceResources.AddRange(instanceResources);

        await RefreshResourcesAsync(true);
        AllowScrollbarRefresh = true;
    }

    /// <summary>
    ///     Refreshes the resource list by querying the Modrinth API with the current search filters (query, version,
    ///     categories).
    ///     When <paramref name="resetSearch" /> is true, existing resources are cleared and the offset starts from 0.
    /// </summary>
    /// <param name="resetSearch">If true, clears the current resource list and resets the search offset.</param>
    public async Task RefreshResourcesAsync(bool resetSearch = false)
    {
        if (_refreshCancellationTokenSource != null)
            await _refreshCancellationTokenSource.CancelAsync();

        _refreshCancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = _refreshCancellationTokenSource.Token;

        try
        {
            var refreshGeneration = Interlocked.Increment(ref _refreshGeneration);
            AllowScrollbarRefresh = false;
            var version = MinecraftVersion;

            List<string> categories = [];
            if (IsMod && ModLoader != EMinecraftKind.VANILLA)
                categories.Add(ModLoader.ToString().ToLower());
            foreach (var category in Categories)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (!category.IsChecked)
                    continue;
                categories.Add(category.Name);
            }

            SearchResponse? response = null;
            switch (_resourceType)
            {
                case EResourceType.RESOURCE_PACK:
                {
                    response = await _metaCacheService.SearchResourcePacksAsync(SearchQuery, version, categories,
                        resetSearch ? 0 : Resources.Count, cancellationToken);
                    break;
                }
                case EResourceType.MOD:
                {
                    response = await _metaCacheService.SearchModsAsync(SearchQuery, version, categories,
                        resetSearch ? 0 : Resources.Count, cancellationToken);
                    break;
                }
                case EResourceType.SHADER_PACK:
                {
                    response = await _metaCacheService.SearchShaderPacksAsync(SearchQuery, version, categories,
                        resetSearch ? 0 : Resources.Count, cancellationToken);
                    break;
                }
            }

            if (response == null)
                throw new Exception("Modrinth search failed.");

            var projectIds = response.Hits.Select(h => h.ProjectId).ToList();
            var projects = await _metaCacheService.GetProjectsAsync(projectIds, cancellationToken);
            if (refreshGeneration != _refreshGeneration)
                return;

            cancellationToken.ThrowIfCancellationRequested();
            var projectOrder = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            for (var i = 0; i < response.Hits.Length; i++)
            {
                var projectId = response.Hits[i].ProjectId;
                if (string.IsNullOrWhiteSpace(projectId))
                    continue;
                projectOrder.TryAdd(projectId, i);
            }

            projects = projects
                .OrderBy(project => projectOrder.GetValueOrDefault(project.Id, int.MaxValue))
                .ToArray();

            cancellationToken.ThrowIfCancellationRequested();
            var versionIds = projects.SelectMany(p => p.Versions).Distinct().ToList();
            var versions = await _metaCacheService.GetVersionsAsync(versionIds, cancellationToken);
            if (refreshGeneration != _refreshGeneration)
                return;

            cancellationToken.ThrowIfCancellationRequested();
            if (IsMod)
            {
                var modLoader = ModLoader switch
                {
                    EMinecraftKind.NEOFORGE => "neoforge",
                    EMinecraftKind.FORGE => "forge",
                    EMinecraftKind.FABRIC => "fabric",
                    EMinecraftKind.QUILT => "quilt",
                    _ => ""
                };
                versions = versions
                    .Where(v => v.Loaders.Contains(modLoader) && v.GameVersions.Contains(MinecraftVersion)).ToArray();
            }
            else
            {
                versions = versions.Where(v => v.GameVersions.Contains(MinecraftVersion)).ToArray();
            }

            var versionDict = versions.ToDictionary(v => v.Id);

            cancellationToken.ThrowIfCancellationRequested();
            var tasks = projects.Select(project =>
            {
                var projectVersions = project.Versions
                    .Where(versionDict.ContainsKey)
                    .Select(id => versionDict[id])
                    .OrderByDescending(v => v.DatePublished)
                    .ToList();

                return ResourceBaseModel.FromModrinthProjectAsync(project, projectVersions, _metaCacheService);
            });

            cancellationToken.ThrowIfCancellationRequested();
            var results = await Task.WhenAll(tasks);
            if (refreshGeneration != _refreshGeneration)
                return;

            if (resetSearch)
            {
                var resourcesCopy = Resources;
                Resources.Clear();
                foreach (var resource in resourcesCopy)
                    resource.Icon.Dispose(_bitmapService);
            }

            foreach (var model in results)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (_instanceResources.Any(r => r.ProjectId == model.ProjectId))
                    model.IsInstalled = true;
                if (ResourcesToDownload.ContainsKey(model.Name))
                    model.IsSelected = true;
                Resources.Add(model);
            }
        }
        catch (OperationCanceledException)
        {
            /* Ignored */
            _logger.LogDebug("Resource refresh operation was canceled.");
        }
        finally
        {
            AllowScrollbarRefresh = true;
        }
    }

    public void SetParent(Window parent)
    {
        _parent = parent;
    }

    #region Observable Properties

    [ObservableProperty] public partial bool AllowScrollbarRefresh { get; set; } = false;

    [ObservableProperty] public partial ObservableCollection<ResourceBaseModel> Resources { get; set; } = [];

    public ObservableCollection<string> VersionFilterSource { get; } = [];

    [ObservableProperty] public partial int VersionFilterIndex { get; set; } = -1;

    [ObservableProperty] public partial string SearchQuery { get; set; } = string.Empty;

    [ObservableProperty] public partial EMinecraftKind ModLoader { get; set; }

    [ObservableProperty] public partial int ModLoaderIndex { get; set; } = 0;

    [ObservableProperty] public partial string MinecraftVersion { get; set; } = string.Empty;

    [ObservableProperty] public partial ObservableCollection<CategoryModel> Categories { get; set; } = [];

    [ObservableProperty] public partial EPlatformType SelectedPlatform { get; set; } = EPlatformType.MODRINTH;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ModPreview))]
    [NotifyPropertyChangedFor(nameof(IsResourceSelected))]
    public partial ResourceBaseModel? SelectedResource { get; set; }

    [ObservableProperty] public partial int SelectedResourceVersionIndex { get; set; } = -1;

    [ObservableProperty] public partial bool HasResources { get; set; }

    #endregion

    #region Commands

    /// <summary>
    ///     Requests the window to close by invoking the <see cref="CloseWindowInteraction" /> interaction.
    /// </summary>
    [RelayCommand]
    public async Task CloseWindow()
    {
        await CloseWindowInteraction.Handle(false);
    }

    /// <summary>
    ///     Toggles the selection state of the currently selected resource for download.
    /// </summary>
    [RelayCommand]
    public Task ToggleResourceSelect()
    {
        try
        {
            if (SelectedResource == null)
                return Task.CompletedTask;

            if (SelectedResourceVersionIndex < 0)
                return Task.CompletedTask;

            var version = SelectedResource.Versions[SelectedResourceVersionIndex];
            var shouldAdd = true;
            if (ResourcesToDownload.Remove(SelectedResource.Name, out var existingVersion))
            {
                shouldAdd = existingVersion.version.Id != version.Id;
                SelectedResource.IsSelected = false;
            }

            HasResources = ResourcesToDownload.Count + (shouldAdd ? 1 : 0) > 0;
            if (!shouldAdd)
                return Task.CompletedTask;

            ResourcesToDownload.Add(SelectedResource.Name, (version, SelectedResource.IconUrl));
            SelectedResource.IsSelected = true;
            return Task.CompletedTask;
        }
        catch (Exception exception)
        {
            return Task.FromException(exception);
        }
    }

    /// <summary>
    ///     Opens the resource review window for the selected resources and their dependencies.
    ///     Collects download models for each selected resource, resolves missing dependencies,
    ///     and passes them to <see cref="ResourceReviewWindow" /> for user confirmation.
    /// </summary>
    [RelayCommand]
    public async Task ReviewInstall()
    {
        if (ResourcesToDownload.Count == 0)
            return;

        List<ResourceDownloadModel> resources = [];
        Dictionary<string, string> dependencies = new();
        foreach (var resource in ResourcesToDownload)
        {
            var version = resource.Value;
            var file = version.version.Files.FirstOrDefault();
            if (file == null)
            {
                _logger.LogWarning($"Resource {resource.Key} has no files, skipping.");
                continue;
            }

            var deps = version.version.Dependencies;
            if (deps != null)
                foreach (var resourceDependency in deps)
                {
                    if (resourceDependency.ProjectId == null || resourceDependency.VersionId == null ||
                        _instanceResources.Any(x => x.ProjectId == resourceDependency.ProjectId)
                        || resources.Any(x => x.ProjectId == resourceDependency.ProjectId))
                        continue;

                    dependencies[resourceDependency.ProjectId] = resourceDependency.VersionId;
                }

            resources.Add(new ResourceDownloadModel
            {
                ProjectId = version.version.ProjectId,
                Name = resource.Key,
                Version = version.version.VersionNumber,
                VersionId = version.version.Id,
                Url = file.Url,
                IconUrl = resource.Value.iconUrl,
                Sha1 = file.Hashes.Sha1,
                Sha512 = file.Hashes.Sha512,
                FileName = file.FileName,
                FileSize = file.Size,
                Platform = SelectedPlatform,
                ShouldDownload = true
            });
        }

        var dependencyVersions = await _metaCacheService.GetVersionsAsync(dependencies.Values.ToList());
        foreach (var dependencyVersion in dependencyVersions)
        {
            var file = dependencyVersion.Files.FirstOrDefault();
            if (file == null)
            {
                _logger.LogWarning($"Version {dependencyVersion.Id} has no files, skipping.");
                continue;
            }

            resources.Add(new ResourceDownloadModel
            {
                ProjectId = dependencyVersion.ProjectId,
                Name = dependencyVersion.Name,
                Version = dependencyVersion.VersionNumber,
                VersionId = dependencyVersion.Id,
                Url = file.Url,
                Sha1 = file.Hashes.Sha1,
                Sha512 = file.Hashes.Sha512,
                FileName = file.FileName,
                FileSize = file.Size,
                Platform = SelectedPlatform,
                ShouldDownload = true
            });
        }

        var reviewWindow = _windowFactory.CreateResourceReviewWindow(_instance!, _resourceType, resources);
        var result = await reviewWindow.ShowDialog<bool>(_parent);
        if (result)
            await CloseWindowInteraction.Handle(true);
    }

    /// <summary>
    ///     Triggers a resource refresh with reset when the search query changes.
    /// </summary>
    // ReSharper disable once UnusedParameterInPartialMethod
    partial void OnSearchQueryChanged(string value)
    {
        if (!AllowScrollbarRefresh)
            return;

        Dispatcher.UIThread.Invoke(async () => await RefreshResourcesAsync(true));
    }

    /// <summary>
    ///     Resets the selected resource version index after a brief delay when the selection changes.
    /// </summary>
    partial void OnSelectedResourceChanged(ResourceBaseModel? value)
    {
        Dispatcher.UIThread.Invoke(async () =>
        {
            await Task.Delay(50); // Minimal delay, otherwise it will always be -1
            SelectedResourceVersionIndex = value == null ? -1 : 0;
        });
    }

    /// <summary>
    ///     Notifies the UI when <see cref="IsResourceSelected" /> has changed due to collection modifications.
    /// </summary>
    private void HandleResourcesToDownload_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        OnPropertyChanged(nameof(IsResourceSelected));
    }

    #endregion
}
