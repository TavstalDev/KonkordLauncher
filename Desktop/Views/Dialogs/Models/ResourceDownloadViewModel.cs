using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Modrinth.Models;
using ReactiveUI;
using Tavstal.KonkordLauncher.Common.Models;
using Tavstal.KonkordLauncher.Common.Services.Abstractions;
using Tavstal.KonkordLauncher.Core.Enums;
using Tavstal.KonkordLauncher.Core.Helpers.Serialization;
using Tavstal.KonkordLauncher.Core.Services.Abstractions;
using Tavstal.KonkordLauncher.Desktop.Models.Avalonia;
using Tavstal.KonkordLauncher.Desktop.Models.Instance;
using Version = Modrinth.Models.Version;

namespace Tavstal.KonkordLauncher.Desktop.Views.Dialogs.Models;

public partial class ResourceDownloadViewModel : KonkordObservableObject
{
    private readonly ILauncherStore _launcherStore;
    private readonly IManifestService _manifestService;
    private readonly IMetaCacheService _metaCacheService;
    public readonly Instance Instance;
    public readonly List<InstanceResource> InstanceResources = [];
    public readonly EResourceType ResourceType;
    public bool IsMod { get; }

    [ObservableProperty]
    public partial bool AllowScrollbarRefresh { get; set; } = false;
    [ObservableProperty] 
    public partial ObservableCollection<ResourceBaseModel> Resources  { get; set; } = [];
    
    public ObservableCollection<string> VersionFilterSource { get; } = [];
    [ObservableProperty]
    public partial int VersionFilterIndex { get; set; } = -1;

    [ObservableProperty]
    public partial string SearchQuery { get; set; } = string.Empty;
    [ObservableProperty]
    public partial EMinecraftKind ModLoader { get; set; }

    [ObservableProperty] 
    public partial string MinecraftVersion { get; set; } = string.Empty;
    [ObservableProperty] 
    public partial ObservableCollection<CategoryModel> Categories { get; set; } = [];

    [ObservableProperty]
    public partial EPlatformType SelectedPlatform { get; set; } = EPlatformType.MODRINTH;

    [ObservableProperty] [NotifyPropertyChangedFor(nameof(ModPreview)), NotifyPropertyChangedFor(nameof(IsResourceSelected))]
    public partial ResourceBaseModel? SelectedResource { get; set; }
    [ObservableProperty] public partial int SelectedResourceVersionIndex { get; set; } = -1;

    public bool? IsResourceSelected => SelectedResource != null && ResourcesToDownload.ContainsKey(SelectedResource.Name);
    // TODO: Translate
    public string? ModPreview => SelectedResource == null ? "<p>" + "Select a resource to see its preview." +"</p>" : SelectedResource.RawPage;
    
    public AvaloniaDictionary<string, Version> ResourcesToDownload { get; } = new();
    
    [ObservableProperty]
    public partial bool HasResources { get; set; }
    
    public List<EPlatformType> AvailablePlatforms =>
    [
        EPlatformType.MODRINTH,
        EPlatformType.CURSE_FORGE,
        EPlatformType.TECHNIC,
        EPlatformType.FTB
    ];
    
    #region Interactions
    public Interaction<Unit, Unit> MinimizeWindowInteraction { get; } = new();
    public Interaction<Unit, Unit> MaximizeWindowInteraction { get; } = new();
    public Interaction<Unit, Unit> CloseWindowInteraction { get; } = new();
    #endregion
    
    public ResourceDownloadViewModel(Instance instance, EResourceType resourceType)
    {
        Instance = instance;
        ResourceType = resourceType;
        IsMod = resourceType == EResourceType.MOD;
        ModLoader = Instance.Kind;
        
        if (Design.IsDesignMode)
            return;
        
        var services = Program.ServiceProvider;
        _launcherStore = services.GetRequiredService<ILauncherStore>();
        _manifestService = services.GetRequiredService<IManifestService>();
        _metaCacheService = services.GetRequiredService<IMetaCacheService>();

        if (IsMod)
        {
            Categories!.Add(new CategoryModel
            {
                Name = "adventure",
                TranslationKey = ""
            });
            Categories!.Add(new CategoryModel
            {
                Name = "cursed",
                TranslationKey = ""
            });
            Categories!.Add(new CategoryModel
            {
                Name = "decoration",
                TranslationKey = ""
            });
            Categories!.Add(new CategoryModel
            {
                Name = "economy",
                TranslationKey = ""
            });
            Categories!.Add(new CategoryModel
            {
                Name = "equipment",
                TranslationKey = ""
            });
            Categories!.Add(new CategoryModel
            {
                Name = "food",
                TranslationKey = ""
            });
            Categories!.Add(new CategoryModel
            {
                Name = "game_mechanics",
                TranslationKey = ""
            });
            Categories!.Add(new CategoryModel
            {
                Name = "library",
                TranslationKey = ""
            });
            Categories!.Add(new CategoryModel
            {
                Name = "magic",
                TranslationKey = ""
            });
            Categories!.Add(new CategoryModel
            {
                Name = "management",
                TranslationKey = ""
            });
            Categories!.Add(new CategoryModel
            {
                Name = "minigame",
                TranslationKey = ""
            });
            Categories!.Add(new CategoryModel
            {
                Name = "mobs",
                TranslationKey = ""
            });
            Categories!.Add(new CategoryModel
            {
                Name = "optimization",
                TranslationKey = ""
            });
            Categories!.Add(new CategoryModel
            {
                Name = "social",
                TranslationKey = ""
            });
            Categories!.Add(new CategoryModel
            {
                Name = "storage",
                TranslationKey = ""
            });
            Categories!.Add(new CategoryModel
            {
                Name = "technology",
                TranslationKey = ""
            });
            Categories!.Add(new CategoryModel
            {
                Name = "transportation",
                TranslationKey = ""
            });
            Categories!.Add(new CategoryModel
            {
                Name = "utility",
                TranslationKey = ""
            });
            Categories!.Add(new CategoryModel
            {
                Name = "worldgen",
                TranslationKey = ""
            });
        }
        
        if (Design.IsDesignMode)
            return;

        ResourcesToDownload.CollectionChanged += HandleResourcesToDownload_CollectionChanged;
        
        _ = InitAsync();
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        ResourcesToDownload.CollectionChanged -= HandleResourcesToDownload_CollectionChanged;
        
        var resourcesCopy = Resources;
        Resources.Clear();
        foreach (var resource in resourcesCopy)
            resource.Icon?.Dispose();
        SelectedResource?.Icon?.Dispose();
        SelectedResource = null;
    }

    private async Task InitAsync(CancellationToken  cancellationToken = default)
    {
        await Task.Yield();

        var settings = await Task.Run(() => _launcherStore.GetSettingsAsync(cancellationToken: cancellationToken), cancellationToken);
        var manifestPath = settings.Launcher.GetVanillaManifestPath();
        var versionManifest = await Task.Run(() => _manifestService.GetMinecraftManifestAsync(manifestPath, cancellationToken), cancellationToken);

        if (versionManifest == null)
            throw new Exception("Failed to load Minecraft version manifest.");
        
        int index = 0;
        bool foundInstanceVersion = false;
        foreach (var version in versionManifest.Versions)
        {
            if (version.Type != "release")
                continue;
            VersionFilterSource.Add(version.Id);
            foundInstanceVersion = version.Id == Instance.MinecraftVersion;
            if (foundInstanceVersion)
                continue;
            
            index++;
        }
        VersionFilterIndex = foundInstanceVersion ? index : 0;

        string configPath = Instance.GetResourceConfigPath();
        var instanceResources = await JsonHelper.ReadJsonFileAsync<List<InstanceResource>>(configPath);
        if (instanceResources is { Count: > 0 })
            InstanceResources.AddRange(instanceResources);

        await RefreshResourcesAsync(true, cancellationToken);
        AllowScrollbarRefresh = true;
    }

    public async Task RefreshResourcesAsync(bool resetSearch = false, CancellationToken cancellationToken = default)
    {
        AllowScrollbarRefresh = false;
        string? version = MinecraftVersion;
        
        List<string> categories = [];
        if (IsMod && ModLoader != EMinecraftKind.VANILLA)
            categories.Add(ModLoader.ToString().ToLower());
        foreach (var category in Categories)
        {
            if (!category.IsChecked)
                continue;
            categories.Add(category.Name);
        }
        
        SearchResponse? response = null;
        switch (ResourceType)
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
        
        var versionIds = projects.SelectMany(p => p.Versions).Distinct().ToList();
        var versions = await _metaCacheService.GetVersionsAsync(versionIds, cancellationToken);
        if (IsMod)
        {
            string modLoader = ModLoader switch
            {
                EMinecraftKind.NEOFORGE => "neoforge",
                EMinecraftKind.FORGE => "forge",
                EMinecraftKind.FABRIC => "fabric",
                EMinecraftKind.QUILT => "quilt",
                _ => ""
            };
            versions = versions.Where(v => v.Loaders.Contains(modLoader) && v.GameVersions.Contains(MinecraftVersion)).ToArray();
        }
        else 
            versions = versions.Where(v => v.GameVersions.Contains(MinecraftVersion)).ToArray();
        
        var versionDict = versions.ToDictionary(v => v.Id);
        
        var tasks = projects.Select(project =>
        {
            var projectVersions = project.Versions
                .Where(versionDict.ContainsKey)
                .Select(id => versionDict[id])
                .OrderByDescending(v => v.DatePublished)
                .ToList();
            
            return ResourceBaseModel.FromModrinthProjectAsync(project, projectVersions);
        });
        
        var results = await Task.WhenAll(tasks);

        if (resetSearch)
        {
            var resourcesCopy = Resources;
            Resources.Clear();
            foreach  (var resource in resourcesCopy)
                resource.Icon?.Dispose();
        }

        foreach (var model in results)
        {
            if (InstanceResources.Any(r => r.ProjectId == model.ProjectId))
                model.IsInstalled = true;
            Resources.Add(model);
        }

        AllowScrollbarRefresh = true;
    }
    
    #region Commands

    /// <summary>
    /// Requests the window to minimize by invoking the <see cref="MinimizeWindowInteraction"/> interaction.
    /// </summary>
    [RelayCommand]
    public async Task MinimizeWindow() => await MinimizeWindowInteraction.Handle(Unit.Default);

    /// <summary>
    /// Requests the window to toggle maximize/restore by invoking the <see cref="MaximizeWindowInteraction"/> interaction.
    /// </summary>
    [RelayCommand]
    public async Task MaximizeWindow() => await MaximizeWindowInteraction.Handle(Unit.Default);

    /// <summary>
    /// Requests the window to close by invoking the <see cref="CloseWindowInteraction"/> interaction.
    /// </summary>
    [RelayCommand]
    public async Task CloseWindow() => await CloseWindowInteraction.Handle(Unit.Default);

    [RelayCommand]
    public async Task ToggleResourceSelect()
    {
        if (SelectedResource == null)
            return;
        
        if (SelectedResourceVersionIndex < 0)
            return;
        
        var version = SelectedResource.Versions[SelectedResourceVersionIndex];
        bool shouldAdd = true;
        if (ResourcesToDownload.Remove(SelectedResource.Name, out var existingVersion))
            shouldAdd = existingVersion.Id != version.Id;
        
        HasResources = ResourcesToDownload.Count + (shouldAdd ? 1 : 0) > 0;
        if (!shouldAdd)
            return;

        ResourcesToDownload.Add(SelectedResource.Name, version);
    }

    [RelayCommand]
    public async Task ReviewInstall()
    {
        // TODO: Show a dialog what will be installed, also fetch dependencies
    }
    
    partial void OnSearchQueryChanged(string value)
    {
        if (!AllowScrollbarRefresh)
            return;
        
        Dispatcher.UIThread.Invoke(async () => await RefreshResourcesAsync(true));
    }

    partial void OnSelectedResourceChanged(ResourceBaseModel? value)
    {
        Dispatcher.UIThread.Invoke(async () =>
        {
            await Task.Delay(50); // Minimal delay, otherwise it will always be -1
            SelectedResourceVersionIndex = value == null ? -1 : 0;
        });
    }

    private void HandleResourcesToDownload_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        OnPropertyChanged(nameof(IsResourceSelected));
    }
    
    #endregion
}