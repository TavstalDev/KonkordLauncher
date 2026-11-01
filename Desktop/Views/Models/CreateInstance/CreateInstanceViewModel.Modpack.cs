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
using DynamicData;
using Microsoft.Extensions.DependencyInjection;
using Tavstal.BeaconSDK.Enums;
using Tavstal.BeaconSDK.Helpers.IO;
using Tavstal.BeaconSDK.Models.Instance;
using Tavstal.BeaconSDK.Models.Logging;
using Tavstal.BeaconSDK.Models.MojangApi;
using Tavstal.BeaconSDK.Services.Abstractions;
using Tavstal.KonkordLauncher.Desktop.Models;
using Tavstal.KonkordLauncher.Desktop.Models.Avalonia;
using Tavstal.KonkordLauncher.Desktop.Models.DTOs.Modpack;
using Tavstal.KonkordLauncher.Desktop.Models.Enums;
using Tavstal.KonkordLauncher.Desktop.Models.Instance;
using Tavstal.KonkordLauncher.Desktop.Services;
using Tavstal.KonkordLauncher.Desktop.Services.Content;
using Tavstal.KonkordLauncher.Desktop.Services.Instances;
using Tavstal.KonkordLauncher.Desktop.Services.Localization;
using Tavstal.KonkordLauncher.Desktop.Services.Presentation.Abstractions;
using Tavstal.KonkordLauncher.Desktop.Services.Storage;
// ReSharper disable UnusedParameterInPartialMethod

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
    private CancellationTokenSource? _refreshCts;

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

        Categories.Add(new CategoryModel { Name = "adventure", TranslationKey = "instance.create.category.adventure" });
        Categories.Add(new CategoryModel { Name = "challenging", TranslationKey = "instance.create.category.challenging" });
        Categories.Add(new CategoryModel { Name = "combat", TranslationKey = "instance.create.category.combat" });
        Categories.Add(new CategoryModel { Name = "kitchen_sink", TranslationKey = "instance.create.category.kitchenSink" });
        Categories.Add(new CategoryModel { Name = "lightweight", TranslationKey = "instance.create.category.lightweight" });
        Categories.Add(new CategoryModel { Name = "magic", TranslationKey = "instance.create.category.magic" });
        Categories.Add(new CategoryModel { Name = "multiplayer", TranslationKey = "instance.create.category.multiplayer" });
        Categories.Add(new CategoryModel { Name = "optimization", TranslationKey = "instance.create.category.optimization" });
        Categories.Add(new CategoryModel { Name = "quests", TranslationKey = "instance.create.category.quests" });
        Categories.Add(new CategoryModel { Name = "technology", TranslationKey = "instance.create.category.technology" });
    }

    [ObservableProperty]
    public partial string? MinecraftVersion { get; set; }

    [ObservableProperty]
    public partial string SearchQuery { get; set; } = string.Empty;

    [ObservableProperty]
    public partial EPlatformType SelectedPlatform { get; set; } = EPlatformType.MODRINTH;

    [ObservableProperty]
    public partial bool AllowScrollbarRefresh { get; set; } = false;

    [ObservableProperty]
    public partial ObservableCollection<ModPackPreview> Modpacks { get; set; } = [];

    public ObservableCollection<string> VersionFilterSource { get; } = [];

    [ObservableProperty]
    public partial EMinecraftKind ModLoader { get; set; } = EMinecraftKind.VANILLA;

    [ObservableProperty]
    public partial ObservableCollection<CategoryModel> Categories { get; set; } = [];

    [ObservableProperty]
    public partial ModPackPreview? SelectedModpack { get; set; }

    [ObservableProperty]
    public partial string? SelectedModpackMarkdownBody { get; set; }

    [ObservableProperty]
    public partial ObservableCollection<ModPackVersion> SelectedModpackVersions { get; set; } = [];

    [ObservableProperty]
    public partial ModPackVersion? SelectedModpackSelectedVersion { get; set; }

    [ObservableProperty]
    public partial string InstanceName { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool CanCreateInstance { get; set; } = false;

    public List<EPlatformType> AvailablePlatforms =>
    [
        EPlatformType.MODRINTH,
        EPlatformType.CURSE_FORGE,
        EPlatformType.TECHNIC,
        EPlatformType.FTB
    ];

    public event EventHandler? OnCloseRequested;

    protected override void Dispose(bool disposing)
    {
        if (!disposing)
            return;
        base.Dispose(disposing);
        var modpacksCopy = Modpacks.ToList();
        Modpacks.Clear();
        foreach (var modpack in modpacksCopy)
            modpack.Icon.Dispose(_bitmapService);
        modpacksCopy.Clear();
        SelectedModpack?.Icon.Dispose(_bitmapService);
        SelectedModpack = null;
    }

    public async Task InitAsync(VersionManifest versionManifest, CancellationToken cancellationToken = default)
    {
        VersionFilterSource.Add("Any");
        MinecraftVersion = "Any";

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
        var tasks = projects.Select(project => ModPackPreview.FromModrinthProjectAsync(project, _metaCacheService, _bitmapService));
        var results = await Task.WhenAll(tasks);

        Modpacks.AddRange(results);
        AllowScrollbarRefresh = true;
    }

    public async Task RefreshModpacksAsync(bool resetSearch = false)
    {
        AllowScrollbarRefresh = false;
        bool cancelled = false;
        try
        {
            if (_refreshCts != null)
            {
                await _refreshCts.CancelAsync();
                _refreshCts.Dispose();
            }

            _refreshCts = new CancellationTokenSource();
            var cancellationToken = _refreshCts.Token;

            var version = MinecraftVersion is null or "Any" ? null : MinecraftVersion;

            List<string> categories = [];
            if (ModLoader != EMinecraftKind.VANILLA)
                categories.Add(ModLoader.ToString().ToLower());

            foreach (var cat in Categories)
            {
                if (!cat.IsChecked)
                    continue;
                categories.Add(cat.Name);
            }

            cancellationToken.ThrowIfCancellationRequested();
            var response = await _metaCacheService.SearchModpacksAsync(SearchQuery, version, categories,
                resetSearch ? 0 : Modpacks.Count, cancellationToken);
            if (response == null)
                throw new Exception("Modrinth search failed.");

            cancellationToken.ThrowIfCancellationRequested();
            var projectIds = response.Hits.Select(h => h.ProjectId).ToList();
            cancellationToken.ThrowIfCancellationRequested();
            var projects = await _metaCacheService.GetProjectsAsync(projectIds, cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();
            var tasks = projects.Select(project =>
                ModPackPreview.FromModrinthProjectAsync(project, _metaCacheService, _bitmapService));
            cancellationToken.ThrowIfCancellationRequested();
            var results = await Task.WhenAll(tasks);

            if (resetSearch)
            {
                foreach (var modpack in Modpacks)
                    modpack.Dispose(_bitmapService);
                Modpacks.Clear();
            }
            cancellationToken.ThrowIfCancellationRequested();
            Modpacks.AddRange(results);
        }
        catch (OperationCanceledException)
        {
            cancelled = true;
        }
        finally
        {
            if (!cancelled)
                AllowScrollbarRefresh = true;
        }
    }

    #region Commands

    [RelayCommand]
    private async Task CreateInstance(CancellationToken cancellationToken = default)
    {
        var modpack = SelectedModpack;
        if (modpack == null || SelectedModpackSelectedVersion == null)
            return;

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

        var version = await _metaCacheService.GetVersionAsync(SelectedModpackSelectedVersion.Id, cancellationToken);
        if (version == null)
        {
            _logger.LogError("Failed to get selected modpack version from the remote server.");
            await _dialogService.ShowAlertAsync(
                _translationService.Translate("instance.create.modpack.error.no_primary_file.title"),
                _translationService.Translate("instance.create.modpack.error.no_primary_file.message"),
                EAlertType.Error
            );
            return;
        }

        var file = version.Files.FirstOrDefault(x => x.Primary);
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
                    InstanceName, null, modpack.Icon.Key, _progress, cancellationToken) != null)
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

    partial void OnSearchQueryChanged(string value)
    {
        if (!AllowScrollbarRefresh)
            return;

        Dispatcher.UIThread.Invoke(async () => await RefreshModpacksAsync(true));
    }

    partial void OnMinecraftVersionChanged(string? oldValue, string? newValue)
    {
        if (!AllowScrollbarRefresh)
            return;

        SelectedModpackSelectedVersion = null;
        SelectedModpackVersions.Clear();
        SelectedModpackMarkdownBody = null;
        SelectedModpack = null;

        Dispatcher.UIThread.Invoke(async () => await RefreshModpacksAsync(true));
    }

    partial void OnSelectedModpackChanged(ModPackPreview? value)
    {
        CanCreateInstance = value != null && !string.IsNullOrEmpty(InstanceName);
        Dispatcher.UIThread.Invoke(async () =>
        {
            SelectedModpackSelectedVersion = null;
            SelectedModpackVersions.Clear();
            SelectedModpackMarkdownBody = null;
            if (value == null)
                return;

            var project = await _metaCacheService.GetProjectAsync(value.ProjectId);
            if (project == null)
                return;

            SelectedModpackMarkdownBody = project.Body;

            var projectVersions = await _metaCacheService.GetVersionsAsync([.. project.Versions]);
            if (projectVersions.Length == 0)
                return;

            if (MinecraftVersion != "Any")
                projectVersions = [.. projectVersions.Where(x => x.GameVersions.Contains(MinecraftVersion))];

            projectVersions = [.. projectVersions.OrderByDescending(x => x.DatePublished)];

            ModPackVersion? selectedVersion = null;
            foreach (var ver in projectVersions)
            {
                var model = new ModPackVersion { Id = ver.Id, Name = ver.Name };
                selectedVersion ??= model;
                SelectedModpackVersions.Add(model);
            }

            SelectedModpackSelectedVersion = selectedVersion;
        });
    }

    partial void OnInstanceNameChanged(string? value)
    {
        CanCreateInstance = !string.IsNullOrEmpty(value) && !string.IsNullOrEmpty(InstanceName);
    }

    #endregion
}
