using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DynamicData;
using DynamicData.Binding;
using Microsoft.Extensions.DependencyInjection;
using Tavstal.BeaconSDK.Enums;
using Tavstal.BeaconSDK.Models.Logging;
using Tavstal.BeaconSDK.Models.ModLoaders;
using Tavstal.BeaconSDK.Models.MojangApi;
using Tavstal.BeaconSDK.Services.Abstractions;
using Tavstal.KonkordLauncher.Desktop.Constants;
using Tavstal.KonkordLauncher.Desktop.Helpers;
using Tavstal.KonkordLauncher.Desktop.Models.Avalonia;
using Tavstal.KonkordLauncher.Desktop.Models.DTOs;
using Tavstal.KonkordLauncher.Desktop.Models.DTOs.Config;
using Tavstal.KonkordLauncher.Desktop.Models.DTOs.InstanceConfig;
using Tavstal.KonkordLauncher.Desktop.Models.Enums;
using Tavstal.KonkordLauncher.Desktop.Services;
using Tavstal.KonkordLauncher.Desktop.Services.Localization;
using Tavstal.KonkordLauncher.Desktop.Services.Presentation.Abstractions;
using Tavstal.KonkordLauncher.Desktop.Services.Storage;

namespace Tavstal.KonkordLauncher.Desktop.Views.Models.CreateInstance;

public partial class CreateInstanceViewModel_Custom : KonkordObservableObject
{
    private readonly IBitmapService _bitmapService = null!;
    private readonly IDialogService _dialogService = null!;
    private readonly IEventService _eventService = null!;
    private readonly ILauncherStore _launcherStore = null!;
    private readonly IBeaconLogger _logger = null!;
    private readonly IManifestService _manifestService = null!;
    private readonly IScreenService _screenService = null!;
    private readonly ITranslationService _translationService = null!;

    public CreateInstanceViewModel_Custom()
    {
        MinecraftVersions =
            new ReadOnlyObservableCollection<MinecraftVersion>([]);
        InstanceIcon = ImageHelper.LoadDesignTime("avares://KonkordLauncher/Assets/Icons/dirt.png");
    }

    public CreateInstanceViewModel_Custom(IScreenService screenService,
        ILauncherStore launcherStore, ITranslationService translationService,
        IManifestService manifestService, IBitmapService bitmapService, IEventService eventService,
        IDialogService dialogService)
    {
        InstanceIcon = new BitmapEntry(null, null);
        MinecraftVersions =
            new ReadOnlyObservableCollection<MinecraftVersion>([]);
        if (Design.IsDesignMode)
        {
            InstanceIcon = ImageHelper.LoadDesignTime("avares://KonkordLauncher/Assets/Icons/dirt.png");
            return;
        }

        _screenService = screenService;
        _logger = Program.Services.GetRequiredService<IBeaconLogger<CreateInstanceViewModel_Custom>>();
        _translationService = translationService;
        _launcherStore = launcherStore;
        _bitmapService = bitmapService;
        _manifestService = manifestService;
        _eventService = eventService;
        _dialogService = dialogService;
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanCreateCustomInstance))]
    public partial string InstanceName { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanCreateCustomInstance))]
    public partial string InstanceGroup { get; set; } = string.Empty;

    [ObservableProperty] public partial string? InstanceIconPath { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanCreateCustomInstance))]
    public partial BitmapEntry InstanceIcon { get; set; }

    /// <summary>
    ///     Gets a value indicating whether a custom instance can be created.
    ///     Returns true if all required fields (instance name, icon, selected Minecraft version,
    ///     and, if applicable, selected mod loader) are set; otherwise, false.
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

    public event EventHandler? OnCloseRequested;

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            Disposables.Dispose();
            _minecraftVersionCache.Clear();
            _minecraftVersionCache.Dispose();
            _modLoaderVersionCache.Clear();
            _modLoaderVersionCache.Dispose();
            InstanceIcon.Dispose(_bitmapService);
        }
        base.Dispose(disposing);
    }

    [RequiresUnreferencedCode(
        "Trimming may break this functionality if not configured to preserve the necessary members.")]
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
                        (x.Type != "release" || showReleases) &&
                        (x.Type != "snapshot" || showSnapshots) &&
                        (x.Type != "old_alpha" || showAlphas) &&
                        (x.Type != "old_beta" || showBetas) &&
                        (x.Type != "experiment" || showExperiments)
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

    public async Task InitAsync(CoreConfig settings, VersionManifest versionManifest,
        CancellationToken cancellationToken = default)
    {
        var fabricManifestCache =
            await _manifestService.GetFabricManifestAsync(settings.Launcher.GetFabricManifestPath(), cancellationToken);
        var forgeManifestCache =
            await _manifestService.GetForgeManifestAsync(settings.Launcher.GetForgeManifestPath(), cancellationToken);
        var neoForgeManifestCache =
            await _manifestService.GetNeoForgeManifestAsync(settings.Launcher.GetNeoForgeManifestPath(),
                cancellationToken);
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

        var icon = await _bitmapService.GetBitmapAsync("avares://KonkordLauncher/Assets/Icons/dirt.png",
            BitmapSizes.InstanceModel);
        Dispatcher.UIThread.Post(() =>
        {
            InstanceIcon = icon;
        });

        await MinecraftVersions
            .ToObservableChangeSet()
            .ToCollection()
            .Where(c => c.Count > 0)
            .Take(1)
            .ToTaskCompat(cancellationToken);

        SelectedMinecraftVersion = MinecraftVersions.FirstOrDefault(v => v.Type == "release");
    }


    #region Vanilla

    private readonly SourceCache<MinecraftVersion, string> _minecraftVersionCache = new(x => x.Id);
    public ReadOnlyObservableCollection<MinecraftVersion> MinecraftVersions { get; private set; }

    [ObservableProperty] public partial string SearchQuery { get; set; } = string.Empty;

    [ObservableProperty] public partial bool ShowReleases { get; set; } = true;

    [ObservableProperty] public partial bool ShowSnapshots { get; set; }

    [ObservableProperty] public partial bool ShowAlphas { get; set; }

    [ObservableProperty] public partial bool ShowBetas { get; set; }

    [ObservableProperty] public partial bool ShowExperiments { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanCreateCustomInstance))]
    public partial MinecraftVersion? SelectedMinecraftVersion { get; set; }

    #endregion

    #region Mod Loader

    [ObservableProperty] public partial string ModLoaderSearchQuery { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanCreateCustomInstance))]
    public partial EMinecraftKind ModLoaderType { get; set; } = EMinecraftKind.VANILLA;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanCreateCustomInstance))]
    public partial IModManifest? SelectedModLoader { get; set; }

    private readonly SourceCache<IModManifest, string> _modLoaderVersionCache =
        new(x => $"{x.LoaderKind}:{x.GameVersion}:{x.Version}");

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

    #region Commands

    [RelayCommand]
    private async Task CustomIconSelectorAsync()
    {
        var result = await _dialogService.ShowIconSelectorAsync();
        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        if (result == null)
            return;
        if (InstanceIcon.Key != null)
            _bitmapService.Release(InstanceIcon.Key!);
        try
        {
            InstanceIcon = await _bitmapService.GetBitmapAsync(result, BitmapSizes.InstanceModel);
        }
        catch
        {
            InstanceIcon = await _bitmapService.GetBitmapAsync("avares://KonkordLauncher/Assets/Icons/dirt.png",
                BitmapSizes.InstanceModel);
        }

        InstanceIconPath = result;
    }

    [RelayCommand]
    private void CustomModLoaderType(EMinecraftKind modLoaderType)
    {
        ModLoaderType = modLoaderType;
    }

    [RelayCommand]
    private async Task CustomCreateAsync()
    {
        if (SelectedMinecraftVersion == null)
            return;

        var settings = await _launcherStore.GetSettingsAsync();
        var instances = await _launcherStore.GetInstancesAsync();
        if (instances.Any(x => x.Name == InstanceName))
        {
            await _dialogService.ShowAlertAsync(
                _translationService.Translate("instance.duplicate.title"),
                _translationService.Translate("instance.duplicate.message"),
                EAlertType.Error);
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
                    WindowHeight = (uint)(0.45 * _screenService.ScreenSize.Height),
                    WindowWidth = (uint)(0.40 * _screenService.ScreenSize.Width),
                    ShowConsoleWhenGameCrashes = true,
                    ShowConsoleWhileGameRunning = false,
                    CloseConsoleOnGameExit = false,
                    EnableFeralGameMode = settings.Misc.EnableFeralGameMode,
                    EnableMangoHud = settings.Misc.EnableMangoHud,
                    UseDedicatedGpu = settings.Misc.UseDedicatedGpu
                },
                Java = new JavaConfig
                {
                    JvmArguments =
                        string.IsNullOrEmpty(settings.Java.JvmArguments)
                            ? Instance.GetDefaultJVMArgs()
                            : settings.Java.JvmArguments,
                    JavaPath = string.Empty,
                    MinMemory = settings.Java.MinMemory,
                    MaxMemory = settings.Java.MaxMemory,
                    PermaGen = settings.Java.PermaGen
                },
                Commands = new InstanceCommandsConfig(),
                EnableEnvironment = false,
                Environment = [],
                Misc = new InstanceMiscConfig()
            }
        });
        await _launcherStore.SaveInstancesAsync(instances);
        _eventService.RaiseInstanceAdded(instances.Last().Id);
        OnCloseRequested?.Invoke(this, EventArgs.Empty);
    }

    [RelayCommand]
    private Task CustomCancelCreate()
    {
        OnCloseRequested?.Invoke(this, EventArgs.Empty);
        return Task.CompletedTask;
    }

    #endregion
}
