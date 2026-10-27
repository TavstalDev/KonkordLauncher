using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Tavstal.BeaconSDK.Enums;
using Tavstal.BeaconSDK.Helpers.IO;
using Tavstal.BeaconSDK.Models.Instance;
using Tavstal.BeaconSDK.Models.Logging;
using Tavstal.BeaconSDK.Models.MojangApi;
using Tavstal.BeaconSDK.Services.Abstractions;
using Tavstal.KonkordLauncher.Desktop.Models;
using Tavstal.KonkordLauncher.Desktop.Models.Avalonia;
using Tavstal.KonkordLauncher.Desktop.Models.Enums;
using Tavstal.KonkordLauncher.Desktop.Models.Instance;
using Tavstal.KonkordLauncher.Desktop.Services;
using Tavstal.KonkordLauncher.Desktop.Services.Content;
using Tavstal.KonkordLauncher.Desktop.Services.Instances;
using Tavstal.KonkordLauncher.Desktop.Services.Localization;
using Tavstal.KonkordLauncher.Desktop.Services.Presentation.Abstractions;
using Tavstal.KonkordLauncher.Desktop.Services.Storage;

namespace Tavstal.KonkordLauncher.Desktop.Views.Models.CreateInstance;

public partial class CreateInstanceViewModel_Modpack : KonkordObservableObject
{
    private readonly IBitmapService _bitmapService = null!;
    private readonly IDialogService _dialogService = null!;
    private readonly IEventService _eventService = null!;
    private readonly IHttpService _httpService = null!;
    private readonly ILauncherStore _launcherStore = null!;
    private readonly IBeaconLogger _logger = null!;
    private readonly IMetaCacheService _metaCacheService = null!;
    private readonly ModrinthPackageService _modrinthPackageService = null!;
    private readonly IKonkordProgress _progress = null!;
    private readonly IScreenService _screenService = null!;
    private readonly ITranslationService _translationService = null!;

    public CreateInstanceViewModel_Modpack()
    {
        _screenService = null!;
    }

    public CreateInstanceViewModel_Modpack(IKonkordProgress progress,
        IScreenService screenService, ILauncherStore launcherStore,
        ITranslationService translationService, IHttpService httpService, IMetaCacheService metaCacheService,
        IBitmapService bitmapService, IEventService eventService, ModrinthPackageService modrinthPackageService,
        IDialogService dialogService)
    {
        if (Design.IsDesignMode)
            return;

        _progress = progress;
        _screenService = screenService;
        _logger = Program.Services.GetRequiredService<IBeaconLogger<CreateInstanceViewModel_Modpack>>();
        _httpService = httpService;
        _translationService = translationService;
        _bitmapService = bitmapService;
        _metaCacheService = metaCacheService;
        _launcherStore = launcherStore;
        _eventService = eventService;
        _modrinthPackageService = modrinthPackageService;
        _dialogService = dialogService;
    }

    [ObservableProperty] public partial string? MinecraftVersion { get; set; }

    [ObservableProperty] public partial string SearchQuery { get; set; } = string.Empty;

    [ObservableProperty] public partial EPlatformType SelectedPlatform { get; set; } = EPlatformType.MODRINTH;

    [ObservableProperty] public partial bool AllowScrollbarRefresh { get; set; } = false;

    [ObservableProperty] public partial ObservableCollection<ModPackModel> Modpacks { get; set; } = new();

    public ObservableCollection<string> VersionFilterSource { get; } = new();

    [ObservableProperty] public partial int VersionFilterIndex { get; set; } = -1;

    [ObservableProperty] public partial EMinecraftKind ModLoader { get; set; } = EMinecraftKind.VANILLA;

    [ObservableProperty] public partial bool CategoryAdventure { get; set; }

    [ObservableProperty] public partial bool CategoryChallenging { get; set; }

    [ObservableProperty] public partial bool CategoryCombat { get; set; }

    [ObservableProperty] public partial bool CategoryKitchenSink { get; set; }

    [ObservableProperty] public partial bool CategoryLightweight { get; set; }

    [ObservableProperty] public partial bool CategoryMagic { get; set; }

    [ObservableProperty] public partial bool CategoryMultiplayer { get; set; }

    [ObservableProperty] public partial bool CategoryOptimization { get; set; }

    [ObservableProperty] public partial bool CategoryQuests { get; set; }

    [ObservableProperty] public partial bool CategoryTechnology { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ModpackPreview))]
    public partial ModPackModel? SelectedModpack { get; set; }

    [ObservableProperty] public partial int SelectedModpackVersionIndex { get; set; }

    [ObservableProperty] public partial string InstanceName { get; set; } = string.Empty;

    [ObservableProperty] public partial bool CanCreateInstance { get; set; } = false;

    public List<EPlatformType> AvailablePlatforms =>
    [
        EPlatformType.MODRINTH,
        EPlatformType.CURSE_FORGE,
        EPlatformType.TECHNIC,
        EPlatformType.FTB
    ];

    public string ModpackPreview => SelectedModpack != null
        ? SelectedModpack.RawPage
        : Design.IsDesignMode
            ? "<p>Select a modpack</p>"
            : "<p>" + _translationService.Translate("instance.create.modpack.preview.select") + "</p>";

    public event EventHandler? OnCloseRequested;

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        var modpacksCopy = Modpacks;
        Modpacks.Clear();
        foreach (var modpack in modpacksCopy)
            modpack.Icon.Dispose(_bitmapService);
        SelectedModpack?.Icon.Dispose(_bitmapService);
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

            return ModPackModel.FromModrinthProjectAsync(project, projectVersions, _metaCacheService);
        });

        var results = await Task.WhenAll(tasks);

        foreach (var model in results)
            Modpacks.Add(model);

        AllowScrollbarRefresh = true;
    }

    public async Task RefreshModpacksAsync(bool resetSearch = false, CancellationToken cancellationToken = default)
    {
        AllowScrollbarRefresh = false;
        var version = MinecraftVersion is null or "Any" ? null : MinecraftVersion;

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

        var response = await _metaCacheService.SearchModpacksAsync(SearchQuery, version, categories,
            resetSearch ? 0 : Modpacks.Count, cancellationToken);
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

            return ModPackModel.FromModrinthProjectAsync(project, projectVersions, _metaCacheService);
        });

        var results = await Task.WhenAll(tasks);

        if (resetSearch)
        {
            var modpacksCopy = Modpacks;
            Modpacks.Clear();
            foreach (var modpack in modpacksCopy)
                modpack.Icon.Dispose(_bitmapService);
        }

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
            await _dialogService.ShowAlertAsync(
                _translationService.Translate("instance.create.modpack.error.instance_exists.title"),
                _translationService.Translate("instance.create.modpack.error.instance_exists.message"),
                EAlertType.Error
            );
            return;
        }

        var file = selectedVersion.Files.FirstOrDefault(x => x.Primary);
        if (file == null)
        {
            _logger.LogError("Selected modpack version does not have a primary file.");
            await _dialogService.ShowAlertAsync(
                _translationService.Translate("instance.create.modpack.error.no_primary_file.title"),
                _translationService.Translate("instance.create.modpack.error.no_primary_file.message"),
                EAlertType.Error
            );
            return;
        }

        var tempDir = Path.Combine(_launcherStore.GetTempDir(), "modpacks");
        Directory.CreateDirectory(tempDir);
        try
        {
            var tempPath = Path.Combine(tempDir, file.FileName);
            _progress.OpenReporter();
            var prog = new Progress<double>(p =>
            {
                _progress.Report(p, "instance.download.file", file.FileName, p.ToString("0.00"));
            });

            await _httpService.DownloadFileAsync(file.Url, tempPath, file.Hashes.Sha1, prog, cancellationToken);

            _progress.CloseReporter();

            if (await _modrinthPackageService.ImportAsync(tempPath,
                    new Resolution
                    {
                        X = (uint)_screenService.ScreenSize.Width, Y = (uint)_screenService.ScreenSize.Height
                    },
                    InstanceName, null, modpack.IconUrl, _progress, cancellationToken) != null)
            {
                _progress.CloseReporter();
                instances = await _launcherStore.GetInstancesAsync(cancellationToken);
                _eventService.RaiseInstanceAdded(instances.Last().Id);
                OnCloseRequested?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                _logger.LogWarning("Failed to import instance from modpack file.");
                await _dialogService.ShowAlertAsync(
                    _translationService.Translate("instance.create.modpack.error.import_failed.title"),
                    _translationService.Translate("instance.create.modpack.error.import_failed.message"),
                    EAlertType.Error
                );
            }
        }
        finally
        {
            FileSystemHelper.DeleteDirectory(tempDir);
        }
    }

    // ReSharper disable once UnusedParameterInPartialMethod
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
}
