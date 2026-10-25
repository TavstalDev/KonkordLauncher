using System;
using System.Diagnostics.CodeAnalysis;
using System.Reactive;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Tavstal.BeaconSDK.Models;
using Tavstal.BeaconSDK.Services.Abstractions;
using Tavstal.KonkordLauncher.Desktop.Models;
using Tavstal.KonkordLauncher.Desktop.Models.Avalonia;
using Tavstal.KonkordLauncher.Desktop.Models.Domain;
using Tavstal.KonkordLauncher.Desktop.Models.Enums;
using Tavstal.KonkordLauncher.Desktop.Services;
using Tavstal.KonkordLauncher.Desktop.Services.Content;
using Tavstal.KonkordLauncher.Desktop.Services.Instances;
using Tavstal.KonkordLauncher.Desktop.Services.Localization;
using Tavstal.KonkordLauncher.Desktop.Services.Presentation;
using Tavstal.KonkordLauncher.Desktop.Services.Storage;
using Tavstal.KonkordLauncher.Desktop.Views.Dialogs;
using Tavstal.KonkordLauncher.Desktop.Views.Models.CreateInstance;

namespace Tavstal.KonkordLauncher.Desktop.Views.Models;

public partial class CreateInstanceViewModel : KonkordObservableObject, IKonkordProgress
{
    private readonly ILauncherStore _launcherStore = null!;
    private readonly IManifestService _manifestService = null!;
    private readonly IWindowFactory _windowFactory = null!;
    public CreateInstanceViewModel_Custom Custom { get;  }
    public CreateInstanceViewModel_Modpack Modpack { get;  }
    public CreateInstanceViewModel_Import Import { get;  }

    [ObservableProperty]
    public partial ECreateInstanceTab SelectedTab { get; set; } = ECreateInstanceTab.CUSTOM;

    #region Interactions


    public Interaction<Unit, Unit> CloseWindowInteraction { get; } = new();
    public Interaction<ECreateInstanceTab, Unit> SwitchTabInteraction { get; } = new();
    public Interaction<int, Unit> SwitchImportTabInteraction { get; } = new();
    public Interaction<Alert, Unit> ShowAlertDialogInteraction { get; } = new();
    public Interaction<Unit, string?> ShowIconSelectorInteraction { get; } = new();
    public Interaction<Unit, string?> ShowFileSelectorInteraction { get; } = new();
    #endregion

    public CreateInstanceViewModel()
    {
        Custom = new CreateInstanceViewModel_Custom();
        Modpack = new CreateInstanceViewModel_Modpack();
        Import = new CreateInstanceViewModel_Import();
    }

    public CreateInstanceViewModel(IScreenService screenService, ILauncherStore launcherStore,
        ITranslationService translationService, IManifestService manifestService, IHttpService httpService,
        IMetaCacheService metaCacheService, IBitmapService bitmapService, IEventService eventService,
        ModrinthPackageService modrinthPackageService, IWindowFactory windowFactory)
    {
        _launcherStore = launcherStore;
        _manifestService = manifestService;
        _windowFactory = windowFactory;

        Custom = new CreateInstanceViewModel_Custom(this, screenService, launcherStore, translationService,
            manifestService, bitmapService, eventService);
        Modpack = new CreateInstanceViewModel_Modpack(this, screenService, launcherStore, translationService,
            httpService, metaCacheService, bitmapService, eventService, modrinthPackageService);
        Import = new CreateInstanceViewModel_Import(this, screenService, launcherStore, translationService,
            httpService, eventService, modrinthPackageService);

        if (Design.IsDesignMode)
            return;

        SetupPipeline();
        _ = InitAsync();
    }

    /// <summary>
    /// Releases the resources used by the CreateInstanceViewModel and performs cleanup operations.
    /// </summary>
    /// <param name="disposing">A boolean value indicating whether the method is being called directly or indirectly by a finalizer.</param>
    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        Custom.Dispose();
        Modpack.Dispose();
        Import.Dispose();
    }

    /// <summary>
    /// Asynchronously initializes the CreateInstanceViewModel and its subcomponents.
    /// </summary>
    /// <param name="cancellationToken">Optional <see cref="CancellationToken"/> that can be used by callers to cancel the initialization flow.</param>
    /// <returns>A <see cref="Task"/> that completes when initialization has finished for the view-model and all child modules.</returns>
    private async Task InitAsync(CancellationToken cancellationToken = default)
    {
        await Task.Yield();

        var settings = await Task.Run(() => _launcherStore.GetSettingsAsync(cancellationToken: cancellationToken), cancellationToken);
        var manifestPath = settings.Launcher.GetVanillaManifestPath();
        var versionManifest = await Task.Run(() => _manifestService.GetMinecraftManifestAsync(manifestPath, cancellationToken), cancellationToken);

        if (versionManifest == null)
            throw new Exception("Failed to load Minecraft version manifest.");

        await Custom.InitAsync(settings, versionManifest, cancellationToken);
        await Modpack.InitAsync(versionManifest, cancellationToken);
    }

    /// <summary>
    /// Sets up reactive/filtering pipelines for all child sub-view-models.
    /// </summary>
    private void SetupPipeline()
    {
        Custom.SetupPipeline();
    }

    /// <summary>
    /// Requests the window to close by invoking the <see cref="CloseWindowInteraction"/> interaction.
    /// </summary>
    [RelayCommand]
    public async Task CloseWindow()
    {
        await CloseWindowInteraction.Handle(Unit.Default);
    }

    /// <summary>
    /// Requests the parent view to change the selected create-instance tab.
    /// </summary>
    /// <param name="tab">The tab to switch to (one of <see cref="ECreateInstanceTab"/>).</param>
    [RelayCommand]
    private async Task HandleTabBtn(ECreateInstanceTab tab)
    {
        await SwitchTabInteraction.Handle(tab);
    }

    #region Progress Reporter
    private ProgressWindow? _installWindow;

    public void Report(double progress)
    {
        EnsureWindowExists();
        _installWindow?.Report(progress);
    }

    public void Report(double progress, string fileName, bool downloading = true)
    {
        EnsureWindowExists();
        _installWindow?.Report(progress, fileName, downloading);
    }

    public void Report(EProgressKind kind, double progress)
    {
        EnsureWindowExists();
        _installWindow?.Report(kind, progress);
    }

    public void ReportExpectedTask(int? count)
    {
        EnsureWindowExists();
        _installWindow?.ReportExpectedTask(count);
    }

    public void ReportTaskCompleted()
    {
        EnsureWindowExists();
        _installWindow?.ReportTaskCompleted();
    }

    public void Report(string key)
    {
        EnsureWindowExists();
        _installWindow?.Report(key);
    }

    public void Report(double progress, string key, params object[]? args)
    {
        EnsureWindowExists();
        _installWindow?.Report(progress, key, args);
    }

    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
    private void EnsureWindowExists()
    {
        _installWindow ??= _windowFactory.CreateProgressWindow();
        _installWindow.Show();
    }

    #endregion
}
