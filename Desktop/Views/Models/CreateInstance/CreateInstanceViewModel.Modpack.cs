using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Tavstal.KonkordLauncher.Common.Services.Abstractions;
using Tavstal.KonkordLauncher.Common.Services.Implementations;
using Tavstal.KonkordLauncher.Core.Enums;
using Tavstal.KonkordLauncher.Core.Helpers.IO;
using Tavstal.KonkordLauncher.Core.Models.Logging;
using Tavstal.KonkordLauncher.Core.Models.MojangApi;
using Tavstal.KonkordLauncher.Core.Services.Abstractions;
using Tavstal.KonkordLauncher.Desktop.Models.Avalonia;
using Tavstal.KonkordLauncher.Desktop.Models.Domain;
using Tavstal.KonkordLauncher.Desktop.Models.Enums;
using Tavstal.KonkordLauncher.Desktop.Models.Instance;

namespace Tavstal.KonkordLauncher.Desktop.Views.Models.CreateInstance;

public partial class CreateInstanceViewModel_Modpack : KonkordObservableObject
{
    private readonly ICustomLogger _logger;
    private readonly IHttpService _httpService;
    private readonly ITranslationService _translationService;
    private readonly IBitmapService _bitmapService;
    private readonly IMetaCacheService _metaCacheService;
    private readonly ILauncherStore _launcherStore;
    private readonly ModrinthPackageService _modrinthPackageService;
    private readonly CreateInstanceViewModel _parent;

    [ObservableProperty]
    public partial bool AllowScrollbarRefresh { get; set; } = false;

    [ObservableProperty]
    public partial ObservableCollection<ModPackModel> Modpacks { get; set; } = new();
    public ObservableCollection<string> VersionFilterSource { get; } = new();
    [ObservableProperty]
    public partial int VersionFilterIndex { get; set; } = -1;

    [ObservableProperty] private string _searchQuery = string.Empty;

    [ObservableProperty]
    public partial EMinecraftKind ModLoader { get; set; } = EMinecraftKind.VANILLA;

    [ObservableProperty] private string? _minecraftVersion;

    [ObservableProperty]
    public partial bool CategoryAdventure { get; set; }

    [ObservableProperty]
    public partial bool CategoryChallenging { get; set; }

    [ObservableProperty]
    public partial bool CategoryCombat { get; set; }

    [ObservableProperty]
    public partial bool CategoryKitchenSink { get; set; }

    [ObservableProperty]
    public partial bool CategoryLightweight { get; set; }

    [ObservableProperty]
    public partial bool CategoryMagic { get; set; }

    [ObservableProperty]
    public partial bool CategoryMultiplayer { get; set; }

    [ObservableProperty]
    public partial bool CategoryOptimization { get; set; }

    [ObservableProperty]
    public partial bool CategoryQuests { get; set; }

    [ObservableProperty]
    public partial bool CategoryTechnology { get; set; }

    [ObservableProperty] private EPlatformType _selectedPlatform = EPlatformType.MODRINTH;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ModpackPreview))]
    public partial ModPackModel? SelectedModpack { get; set; }

    [ObservableProperty]
    public partial int SelectedModpackVersionIndex { get; set; }

    [ObservableProperty]
    public partial string InstanceName { get; set; }

    [ObservableProperty]
    public partial bool CanCreateInstance { get; set; } = false;

    public List<EPlatformType> AvailablePlatforms =>
    [
        EPlatformType.MODRINTH,
        EPlatformType.CURSE_FORGE,
        EPlatformType.TECHNIC,
        EPlatformType.FTB
    ];
    
    public string? ModpackPreview => SelectedModpack == null ? "<p>" + _translationService.Translate("instance.create.modpack.preview.select") +"</p>" : SelectedModpack.RawPage;
    
    public CreateInstanceViewModel_Modpack(CreateInstanceViewModel parent)
    {
        _parent = parent;
        if (Design.IsDesignMode)
            return;
        
        var services = Program.ServiceProvider;
        _logger = services.GetRequiredService<ICustomLogger<CreateInstanceViewModel_Modpack>>();
        _httpService = services.GetRequiredService<IHttpService>();
        _translationService = services.GetRequiredService<ITranslationService>();
        _bitmapService = services.GetRequiredService<IBitmapService>();
        _metaCacheService = services.GetRequiredService<IMetaCacheService>();
        _launcherStore = services.GetRequiredService<ILauncherStore>();
        _modrinthPackageService = services.GetRequiredService<ModrinthPackageService>();
    }
    
    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        var modpacksCopy = Modpacks;
        Modpacks.Clear();
        foreach (var modpack in modpacksCopy)
            modpack.Icon?.Dispose(_bitmapService);
        SelectedModpack?.Icon?.Dispose(_bitmapService);
        SelectedModpack = null;
    }
    
    public async Task InitAsync(VersionManifest versionManifest, CancellationToken cancellationToken = default)
    {
        VersionFilterSource.Add("Any");
        VersionFilterIndex = 0;
        foreach (var version in versionManifest.Versions)
        {
            if (version.Type != "release")
                continue;
            VersionFilterSource.Add(version.Id);
        }
        
        var response = await _metaCacheService.SearchModpacksAsync(cancellationToken: cancellationToken);
        if (response == null)
            throw new Exception("Modrinth search failed.");

        var projectIds = response.Hits.Select(h => h.ProjectId).ToList();
        var projects = await _metaCacheService.GetProjectsAsync(projectIds, cancellationToken);
        
        var versionIds = projects.SelectMany(p => p.Versions).Distinct().ToList();
        var versions = await _metaCacheService.GetVersionsAsync(versionIds, cancellationToken);
        
        var versionDict = versions.ToDictionary(v => v.Id);
        
        var tasks = projects.Select(project =>
        {
            var projectVersions = project.Versions
                .Where(versionDict.ContainsKey)
                .Select(id => versionDict[id])
                .OrderByDescending(v => v.DatePublished)
                .ToList();
            
            return ModPackModel.FromModrinthProjectAsync(project, projectVersions);
        });

        var results = await Task.WhenAll(tasks);

        foreach (var model in results)
            Modpacks.Add(model);

        AllowScrollbarRefresh = true;
    }
    
    #region Commands

    [RelayCommand]
    private async Task CreateInstance(CancellationToken cancellationToken = default)
    {
        var modpack = SelectedModpack;
        if (modpack == null || SelectedModpackVersionIndex == -1)
            return;

        var selectedVersion = modpack.Versions[SelectedModpackVersionIndex];
        var instances = await _launcherStore.GetInstancesAsync(cancellationToken);

        if (instances.Any(x => x.Name == InstanceName))
        {
            await _parent.ShowAlertDialogInteraction.Handle(new Alert(
                _translationService.Translate("instance.create.modpack.error.instance_exists.title"),
                _translationService.Translate("instance.create.modpack.error.instance_exists.message"),
                EAlertType.Error
            ));
            return;
        }
        
        var file = selectedVersion.Files.FirstOrDefault(x => x.Primary);
        if (file == null)        {
            _logger.LogError("Selected modpack version does not have a primary file.");
            await _parent.ShowAlertDialogInteraction.Handle(new Alert(
                _translationService.Translate("instance.create.modpack.error.no_primary_file.title"),
                _translationService.Translate("instance.create.modpack.error.no_primary_file.message"),
                EAlertType.Error
            ));
            return;
        }
        
        string tempDir = Path.Combine(PathHelper.TempDir, "modpacks");
        Directory.CreateDirectory(tempDir);
        try
        {
            string tempPath = Path.Combine(tempDir, file.FileName);
            _parent.OpenReporter();
            var prog = new Progress<double>(p =>
            {
                _parent?.ReportProgress(p);
                _parent?.UpdateStatusTranslated("instance.download.file", file.FileName, p.ToString("0.00"));
            });
            
            await _httpService.DownloadFileAsync(file.Url, tempPath, prog, cancellationToken);
            
            _parent.CloseReporter();

            if (await _modrinthPackageService.ImportAsync(tempPath, App.ScreenResolution, InstanceName, null, modpack.IconUrl, _parent, cancellationToken) != null)
            {
                _parent.CloseReporter();
                instances = await _launcherStore.GetInstancesAsync(cancellationToken);
                GlobalEvents.InvokeInstanceAdded(instances.Last().Id);
                await _parent.CloseWindowInteraction.Handle(Unit.Default);
            }
            else
            {
                _logger.LogWarning("Failed to import instance from modpack file.");
                await _parent.ShowAlertDialogInteraction.Handle(new Alert(
                    _translationService.Translate("instance.create.modpack.error.import_failed.title"),
                    _translationService.Translate("instance.create.modpack.error.import_failed.message"),
                    EAlertType.Error
                ));
            }
        }
        finally
        {
            FileSystemHelper.DeleteDirectory(tempDir);
        }
    }

    partial void OnSearchQueryChanged(string value)
    {
        if (!AllowScrollbarRefresh)
            return;
        
        Dispatcher.UIThread.Invoke(async () => await RefreshModpacksAsync(true));
    }

    partial void OnSelectedModpackChanged(ModPackModel? value)
    {
        CanCreateInstance = value != null && !string.IsNullOrEmpty(InstanceName);
        Dispatcher.UIThread.Invoke(async () =>
        {
            await Task.Delay(50); // Minimal delay, otherwise it will always be -1
            SelectedModpackVersionIndex = value == null ? -1 : 0;
        });
    }

    partial void OnInstanceNameChanged(string? value)
    {
        CanCreateInstance = value != null && !string.IsNullOrEmpty(InstanceName);
    }
    
    #endregion
    
    public async Task RefreshModpacksAsync(bool resetSearch = false, CancellationToken cancellationToken = default)
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
        
        var response = await _metaCacheService.SearchModpacksAsync(SearchQuery, version, categories, resetSearch ? 0 : Modpacks.Count, cancellationToken);
        if (response == null)
            throw new Exception("Modrinth search failed.");

        var projectIds = response.Hits.Select(h => h.ProjectId).ToList();
        var projects = await _metaCacheService.GetProjectsAsync(projectIds, cancellationToken);
        
        var versionIds = projects.SelectMany(p => p.Versions).Distinct().ToList();
        var versions = await _metaCacheService.GetVersionsAsync(versionIds, cancellationToken);
        
        var versionDict = versions.ToDictionary(v => v.Id);
        
        var tasks = projects.Select(project =>
        {
            var projectVersions = project.Versions
                .Where(versionDict.ContainsKey)
                .Select(id => versionDict[id])
                .OrderByDescending(v => v.DatePublished)
                .ToList();
            
            return ModPackModel.FromModrinthProjectAsync(project, projectVersions);
        });
        
        var results = await Task.WhenAll(tasks);

        if (resetSearch)
        {
            var modpacksCopy = Modpacks;
            Modpacks.Clear();
            foreach  (var modpack in modpacksCopy)
                modpack.Icon?.Dispose(_bitmapService);
        }
        foreach (var model in results)
            Modpacks.Add(model);
        
        AllowScrollbarRefresh = true;
    }
}