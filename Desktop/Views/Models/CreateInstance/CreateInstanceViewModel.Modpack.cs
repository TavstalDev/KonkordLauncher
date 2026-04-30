using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using Tavstal.KonkordLauncher.Common.Helpers;
using Tavstal.KonkordLauncher.Common.Translation;
using Tavstal.KonkordLauncher.Core.Enums;
using Tavstal.KonkordLauncher.Core.Models;
using Tavstal.KonkordLauncher.Core.Models.MojangApi;
using Tavstal.KonkordLauncher.Desktop.Models.Avalonia;
using Tavstal.KonkordLauncher.Desktop.Models.Instance;

namespace Tavstal.KonkordLauncher.Desktop.Views.Models.CreateInstance;

public partial class CreateInstanceViewModel_Modpack : KonkordObservableObject
{
    private readonly CoreLogger _logger = CoreLogger.WithModuleType(typeof(CreateInstanceViewModel_Modpack));
    private readonly CreateInstanceViewModel _parent;
    
    [ObservableProperty] private bool _allowScrollbarRefresh = false;
    [ObservableProperty] private ObservableCollection<ModPackModel> _modpacks = new();
    public ObservableCollection<string> VersionFilterSource { get; } = new();
    [ObservableProperty] private int _versionFilterIndex = -1;
    
    [ObservableProperty] private string _searchQuery = string.Empty;

    [ObservableProperty] private EMinecraftKind _modLoader = EMinecraftKind.VANILLA;
    
    [ObservableProperty] private string? _minecraftVersion;

    [ObservableProperty] private bool _categoryAdventure;
    [ObservableProperty] private bool _categoryChallenging;
    [ObservableProperty] private bool _categoryCombat;
    [ObservableProperty] private bool _categoryKitchenSink;
    [ObservableProperty] private bool _categoryLightweight;
    [ObservableProperty] private bool _categoryMagic;
    [ObservableProperty] private bool _categoryMultiplayer;
    [ObservableProperty] private bool _categoryOptimization;
    [ObservableProperty] private bool _categoryQuests;
    [ObservableProperty] private bool _categoryTechnology;
    
    [ObservableProperty] private EPlatformType _selectedPlatform = EPlatformType.Modrinth;
    
    [ObservableProperty] [NotifyPropertyChangedFor(nameof(ModpackPreview))] private ModPackModel? _selectedModpack;
    
    public List<EPlatformType> AvailablePlatforms =>
    [
        EPlatformType.Modrinth,
        EPlatformType.CurseForge,
        EPlatformType.Technic,
        EPlatformType.FTB
    ];
    
    public string? ModpackPreview => SelectedModpack == null ? "<p>" + TranslationManager.Translate("instance.create.modpack.preview.select") +"</p>" : SelectedModpack.RawPage;
    
    public CreateInstanceViewModel_Modpack(CreateInstanceViewModel parent)
    {
        _parent = parent;
    }
    
    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        var modpacksCopy = Modpacks;
        Modpacks.Clear();
        foreach (var modpack in modpacksCopy)
            modpack.Icon?.Dispose();
        SelectedModpack?.Icon?.Dispose();
        SelectedModpack = null;
    }
    
    public void SetupPipeline()
    {
        
    }
    
    public async Task InitAsync(VersionManifest versionManifest, CancellationToken cancellationToken = default)
    {
        foreach (var version in versionManifest.Versions)
        {
            if (version.Type != "release")
                continue;
            VersionFilterSource.Add(version.Id);
        }

        var response = await ModrinthHelper.SearchModpacksAsync(token: cancellationToken);
        if (response == null)
            throw new Exception("Modrinth search failed.");

        var tasks = response.Hits.Select(ModPackModel.FromModrinthProjectAsync);

        var results = await Task.WhenAll(tasks);

        foreach (var model in results)
            Modpacks.Add(model);

        AllowScrollbarRefresh = true;
        
        Dispatcher.UIThread.Post(() =>
        {
            VersionFilterSource.Add("Any");
            VersionFilterIndex = 0;
        });
    }
    
    #region Commands

    partial void OnSearchQueryChanged(string value)
    {
        if (!AllowScrollbarRefresh)
            return;
        
        Dispatcher.UIThread.Invoke(async () => await RefreshModpacksAsync(true));
    }

    partial void OnSelectedModpackChanged(ModPackModel? value)
    {
        
    }

    #endregion
    
    public async Task RefreshModpacksAsync(bool resetSearch = false)
    {
        AllowScrollbarRefresh = false;
        string? version = MinecraftVersion is null or "Any" ? null : MinecraftVersion;

        List<string> categories = [];
        if (ModLoader != EMinecraftKind.VANILLA)
            categories.Add(ModLoader.ToString().ToLower());
        if (CategoryAdventure)
            categories.Add("adventure");
        if (CategoryChallenging)
            categories.Add("challenging");
        if (CategoryCombat)
            categories.Add("combat");
        if (CategoryKitchenSink)
            categories.Add("kitchen_sink");
        if (CategoryLightweight)
            categories.Add("lightweight");
        if (CategoryMagic)
            categories.Add("magic");
        if (CategoryMultiplayer)
            categories.Add("multiplayer");
        if (CategoryOptimization)
            categories.Add("optimization");
        if (CategoryQuests)
            categories.Add("quests");
        if (CategoryTechnology)
            categories.Add("technology");
        
        var response = await ModrinthHelper.SearchModpacksAsync(SearchQuery, version, categories, Modpacks.Count);
        if (response == null)
            throw new Exception("Modrinth search failed.");

        var tasks = response.Hits.Select(ModPackModel.FromModrinthProjectAsync);
        
        var results = await Task.WhenAll(tasks);
        
        if (resetSearch)
            Modpacks.Clear();
        foreach (var model in results)
            Modpacks.Add(model);
        
        AllowScrollbarRefresh = true;
    }
}