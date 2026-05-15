using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Tavstal.KonkordLauncher.Common.Helpers;
using Tavstal.KonkordLauncher.Common.Models;
using Tavstal.KonkordLauncher.Common.Translation;
using Tavstal.KonkordLauncher.Core.Enums;
using Tavstal.KonkordLauncher.Core.Helpers.IO;
using Tavstal.KonkordLauncher.Core.Helpers.Network;
using Tavstal.KonkordLauncher.Core.Models;
using Tavstal.KonkordLauncher.Core.Models.MojangApi;
using Tavstal.KonkordLauncher.Desktop.Models.Avalonia;
using Tavstal.KonkordLauncher.Desktop.Models.Domain;
using Tavstal.KonkordLauncher.Desktop.Models.Enums;
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
    [ObservableProperty] private int _selectedModpackVersionIndex;
    [ObservableProperty] private string _instanceName;
    [ObservableProperty] private bool _canCreateInstance = false;
    
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
        VersionFilterSource.Add("Any");
        VersionFilterIndex = 0;
        foreach (var version in versionManifest.Versions)
        {
            if (version.Type != "release")
                continue;
            VersionFilterSource.Add(version.Id);
        }

        var response = await MetaCacheHelper.SearchModpacksAsync(cancellationToken: cancellationToken);
        if (response == null)
            throw new Exception("Modrinth search failed.");

        var projectIds = response.Hits.Select(h => h.ProjectId).ToList();
        var projects = await MetaCacheHelper.GetProjectsAsync(projectIds, cancellationToken);
        
        var versionIds = projects.SelectMany(p => p.Versions).Distinct().ToList();
        var versions = await MetaCacheHelper.GetVersionsAsync(versionIds, cancellationToken);
        
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
        var instances = await LauncherHelper.GetInstancesAsync(cancellationToken);

        if (instances.Any(x => x.Name == InstanceName))
        {
            await _parent.ShowAlertDialogInteraction.Handle(new Alert(
                TranslationManager.Translate("instance.create.modpack.error.instance_exists.title"),
                TranslationManager.Translate("instance.create.modpack.error.instance_exists.message"),
                EAlertType.Error
            ));
            return;
        }
        
        var file = selectedVersion.Files.FirstOrDefault(x => x.Primary);
        if (file == null)        {
            _logger.Error("Selected modpack version does not have a primary file.");
            await _parent.ShowAlertDialogInteraction.Handle(new Alert(
                TranslationManager.Translate("instance.create.modpack.error.no_primary_file.title"),
                TranslationManager.Translate("instance.create.modpack.error.no_primary_file.message"),
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
            
            await HttpHelper.DownloadFileAsync(file.Url, tempPath, prog, cancellationToken);
            
            _parent.CloseReporter();
            if (await InstanceHelper.ImportAsync(tempPath, EInstanceProvider.Modrinth, App.ScreenResolution, InstanceName, null, _parent, cancellationToken) != null)
            {
                _parent.CloseReporter();
                instances = await LauncherHelper.GetInstancesAsync(cancellationToken);
                GlobalEvents.InvokeInstanceAdded(instances.Last().Id);
                await _parent.CloseWindowInteraction.Handle(Unit.Default);
            }
            else
            {
                _logger.Warn("Failed to import instance from modpack file.");
                await _parent.ShowAlertDialogInteraction.Handle(new Alert(
                    TranslationManager.Translate("instance.create.modpack.error.import_failed.title"),
                    TranslationManager.Translate("instance.create.modpack.error.import_failed.message"),
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
        
        var response = await MetaCacheHelper.SearchModpacksAsync(SearchQuery, version, categories, resetSearch ? 0 : Modpacks.Count, cancellationToken);
        if (response == null)
            throw new Exception("Modrinth search failed.");

        var projectIds = response.Hits.Select(h => h.ProjectId).ToList();
        var projects = await MetaCacheHelper.GetProjectsAsync(projectIds, cancellationToken);
        
        var versionIds = projects.SelectMany(p => p.Versions).Distinct().ToList();
        var versions = await MetaCacheHelper.GetVersionsAsync(versionIds, cancellationToken);
        
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
                modpack.Icon?.Dispose();
        }
        foreach (var model in results)
            Modpacks.Add(model);
        
        AllowScrollbarRefresh = true;
    }
}