using System;
using System.Diagnostics.CodeAnalysis;
using System.Reactive;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Tavstal.BeaconSDK.Models;
using Tavstal.BeaconSDK.Services.Abstractions;
using Tavstal.KonkordLauncher.Desktop.Models;
using Tavstal.KonkordLauncher.Desktop.Models.Avalonia;
using Tavstal.KonkordLauncher.Desktop.Models.Enums;
using Tavstal.KonkordLauncher.Desktop.Services;
using Tavstal.KonkordLauncher.Desktop.Services.Content;
using Tavstal.KonkordLauncher.Desktop.Services.Instances;
using Tavstal.KonkordLauncher.Desktop.Services.Localization;
using Tavstal.KonkordLauncher.Desktop.Services.Presentation.Abstractions;
using Tavstal.KonkordLauncher.Desktop.Services.Storage;
using Tavstal.KonkordLauncher.Desktop.Views.Dialogs;
using Tavstal.KonkordLauncher.Desktop.Views.Models.CreateInstance;

namespace Tavstal.KonkordLauncher.Desktop.Views.Models;

/// <summary>
///     A view model for the Create Instance window. It holds the Custom, Modpack and Import tabs and shows progress
///     while an instance is installed.
/// </summary>
public partial class CreateInstanceViewModel : KonkordObservableObject, IKonkordProgress
{
    private readonly ILauncherStore _launcherStore = null!;
    private readonly IManifestService _manifestService = null!;
    private readonly IWindowFactory _windowFactory = null!;

    /// <summary>
    ///     Creates an empty view model for the XAML designer.
    /// </summary>
    public CreateInstanceViewModel()
    {
        Custom = new CreateInstanceViewModel_Custom();
        Modpack = new CreateInstanceViewModel_Modpack();
        Import = new CreateInstanceViewModel_Import();
    }

    /// <summary>
    ///     Creates the view model and its tabs.
    /// </summary>
    /// <param name="screenService">The service that provides the screen size.</param>
    /// <param name="launcherStore">The store for launcher data.</param>
    /// <param name="translationService">The service that translates text.</param>
    /// <param name="manifestService">The service that loads version manifests.</param>
    /// <param name="httpService">The service that downloads files.</param>
    /// <param name="metaCacheService">The service that reads cached modpack data.</param>
    /// <param name="bitmapService">The service that loads and releases images.</param>
    /// <param name="eventService">The service that raises and listens to events.</param>
    /// <param name="modrinthPackageService">The service that imports modpacks and instances.</param>
    /// <param name="windowFactory">The factory that creates windows.</param>
    /// <param name="dialogService">The service that shows dialogs.</param>
    /// <param name="filePickerService">The service that opens file pickers.</param>
    public CreateInstanceViewModel(IScreenService screenService, ILauncherStore launcherStore,
        ITranslationService translationService, IManifestService manifestService, IHttpService httpService,
        IMetaCacheService metaCacheService, IBitmapService bitmapService, IEventService eventService,
        ModrinthPackageService modrinthPackageService, IWindowFactory windowFactory,
        IDialogService dialogService, IFilePickerService filePickerService)
    {
        _launcherStore = launcherStore;
        _manifestService = manifestService;
        _windowFactory = windowFactory;

        Custom = new CreateInstanceViewModel_Custom(screenService, launcherStore, translationService,
            manifestService, bitmapService, eventService, dialogService);
        Custom.OnCloseRequested += OnCloseRequested;
        Modpack = new CreateInstanceViewModel_Modpack(this, screenService, launcherStore, translationService,
            httpService, metaCacheService, bitmapService, eventService, modrinthPackageService, dialogService);
        Modpack.OnCloseRequested += OnCloseRequested;
        Import = new CreateInstanceViewModel_Import(this, screenService, launcherStore, translationService,
            httpService, eventService, modrinthPackageService, dialogService, filePickerService);
        Import.OnCloseRequested += OnCloseRequested;

        if (Design.IsDesignMode)
            return;

        Custom.SetupPipeline();
        _ = InitAsync();
    }

    /// <summary>
    ///     Gets the view model of the Custom tab.
    /// </summary>
    public CreateInstanceViewModel_Custom Custom { get; }

    /// <summary>
    ///     Gets the view model of the Modpack tab.
    /// </summary>
    public CreateInstanceViewModel_Modpack Modpack { get; }

    /// <summary>
    ///     Gets the view model of the Import tab.
    /// </summary>
    public CreateInstanceViewModel_Import Import { get; }

    /// <summary>
    ///     Gets or sets the selected tab.
    /// </summary>
    [ObservableProperty] public partial ECreateInstanceTab SelectedTab { get; set; } = ECreateInstanceTab.CUSTOM;

    /// <inheritdoc />
    protected override void Dispose(bool disposing)
    {
        if (!disposing)
            return;
        base.Dispose(disposing);
        Custom.Dispose();
        Custom.OnCloseRequested -= OnCloseRequested;
        Modpack.Dispose();
        Modpack.OnCloseRequested -= OnCloseRequested;
        Import.Dispose();
        Import.OnCloseRequested -= OnCloseRequested;
    }

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

        await Custom.InitAsync(settings, versionManifest, cancellationToken);
        await Modpack.InitAsync(versionManifest, cancellationToken);
    }

    private void OnCloseRequested(object? sender, EventArgs args)
    {
        Dispatcher.UIThread.Invoke(async () => await CloseWindowInteraction.Handle(Unit.Default));
    }

    [RelayCommand]
    private async Task CloseWindow()
    {
        await CloseWindowInteraction.Handle(Unit.Default);
    }

    [RelayCommand]
    private async Task HandleTabBtn(ECreateInstanceTab tab)
    {
        await SwitchTabInteraction.Handle(tab);
    }

    #region Interactions

    /// <summary>
    ///     Gets the interaction that asks the window to close.
    /// </summary>
    public Interaction<Unit, Unit> CloseWindowInteraction { get; } = new();

    /// <summary>
    ///     Gets the interaction that switches to another tab.
    /// </summary>
    public Interaction<ECreateInstanceTab, Unit> SwitchTabInteraction { get; } = new();

    #endregion

    #region Progress Reporter

    private ProgressWindow? _installWindow;

    /// <summary>
    ///     Opens the progress window, or shows it again if it already exists.
    /// </summary>
    public void OpenReporter()
    {
        EnsureWindowExists();
    }

    /// <summary>
    ///     Closes the progress window.
    /// </summary>
    public void CloseReporter()
    {
        _installWindow?.Close();
        _installWindow = null;
    }

    /// <summary>
    ///     Shows the current progress.
    /// </summary>
    /// <param name="progress">The progress from 0 to 1.</param>
    public void Report(double progress)
    {
        EnsureWindowExists();
        _installWindow?.Report(progress);
    }

    /// <summary>
    ///     Shows the progress of a file.
    /// </summary>
    /// <param name="progress">The progress from 0 to 1.</param>
    /// <param name="fileName">The name of the file.</param>
    /// <param name="downloading">A value that indicates whether the file is being downloaded.</param>
    public void Report(double progress, string fileName, bool downloading = true)
    {
        EnsureWindowExists();
        _installWindow?.Report(progress, fileName, downloading);
    }

    /// <summary>
    ///     Shows the progress of a given kind.
    /// </summary>
    /// <param name="kind">The kind of progress to show.</param>
    /// <param name="progress">The progress from 0 to 1.</param>
    public void Report(EProgressKind kind, double progress)
    {
        EnsureWindowExists();
        _installWindow?.Report(kind, progress);
    }

    /// <summary>
    ///     Sets how many tasks are expected.
    /// </summary>
    /// <param name="count">The number of tasks, or <see langword="null" /> to clear it.</param>
    public void ReportExpectedTask(int? count)
    {
        EnsureWindowExists();
        _installWindow?.ReportExpectedTask(count);
    }

    /// <summary>
    ///     Marks one expected task as completed.
    /// </summary>
    public void ReportTaskCompleted()
    {
        EnsureWindowExists();
        _installWindow?.ReportTaskCompleted();
    }

    /// <summary>
    ///     Shows a translated message.
    /// </summary>
    /// <param name="key">The translation key of the message.</param>
    public void Report(string key)
    {
        EnsureWindowExists();
        _installWindow?.Report(key);
    }

    /// <summary>
    ///     Shows the progress and a translated message.
    /// </summary>
    /// <param name="progress">The progress from 0 to 1.</param>
    /// <param name="key">The translation key of the message.</param>
    /// <param name="args">The values used to format the message.</param>
    public void Report(double progress, string key, params object[]? args)
    {
        EnsureWindowExists();
        _installWindow?.Report(progress, key, args);
    }

    [UnconditionalSuppressMessage("Trimming",
        "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code",
        Justification = "<Pending>")]
    private void EnsureWindowExists()
    {
        _installWindow ??= _windowFactory.CreateProgressWindow();
        _installWindow.Show();
    }

    #endregion
}
