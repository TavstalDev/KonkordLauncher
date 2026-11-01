using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reactive;
using System.Threading.Tasks;
using Avalonia.Controls;
using ReactiveUI;
using ReactiveUI.Primitives;
using Tavstal.BeaconSDK.Models;
using Tavstal.BeaconSDK.Services.Abstractions;
using Tavstal.KonkordLauncher.Desktop.Models;
using Tavstal.KonkordLauncher.Desktop.Models.Avalonia;
using Tavstal.KonkordLauncher.Desktop.Models.DTOs;
using Tavstal.KonkordLauncher.Desktop.Models.Enums;
using Tavstal.KonkordLauncher.Desktop.Models.Instance;
using Tavstal.KonkordLauncher.Desktop.Services.Content;
using Tavstal.KonkordLauncher.Desktop.Services.Localization;
using Tavstal.KonkordLauncher.Desktop.Services.Presentation.Abstractions;
using Tavstal.KonkordLauncher.Desktop.Services.Storage;
using Tavstal.KonkordLauncher.Desktop.Views.Dialogs.Models;

namespace Tavstal.KonkordLauncher.Desktop.Views.Dialogs;

/// <summary>
///     A dialog window where the user reviews the resources found for an instance.
///     It also shows the install progress in a separate window.
/// </summary>
public partial class ResourceReviewWindow : KonkordWindow<ResourceReviewViewModel>, IKonkordProgress
{
    private readonly IWindowFactory _windowFactory = null!;
    private ProgressWindow? _installWindow;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ResourceReviewWindow" /> class for the designer.
    /// </summary>
    public ResourceReviewWindow()
    {
        InitializeComponent();
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="ResourceReviewWindow" /> class with the given resources.
    /// </summary>
    /// <param name="instance">The instance that receives the resources.</param>
    /// <param name="resourceType">The type of resource to review.</param>
    /// <param name="resources">The resources to review.</param>
    /// <param name="httpService">The service used to download files.</param>
    /// <param name="translationService">The service used to translate text.</param>
    /// <param name="launcherStore">The store that holds launcher data.</param>
    /// <param name="metaCacheService">The cache that holds resource metadata.</param>
    /// <param name="windowFactory">The factory that creates other windows.</param>
    /// <param name="dialogService">The service used to open dialogs.</param>
    /// <param name="bitmapService">The service used to load bitmaps.</param>
    [RequiresUnreferencedCode(
        "Trimming may break this functionality if not configured to preserve the necessary members.")]
    public ResourceReviewWindow(Instance instance, EResourceType resourceType,
        List<ResourceDownloadModel> resources, IHttpService httpService, ITranslationService translationService,
        ILauncherStore launcherStore, IMetaCacheService metaCacheService, IWindowFactory windowFactory,
        IDialogService dialogService, IBitmapService bitmapService) : base(bitmapService)
    {
        InitializeComponent();

        if (Design.IsDesignMode)
            return;

        _windowFactory = windowFactory;
        DataContext = new ResourceReviewViewModel(instance, resourceType, resources, this,
            httpService, translationService, launcherStore, metaCacheService, dialogService);

        this.WhenActivated(disposables =>
        {
            DataContext.CloseWindowInteraction.RegisterHandler(action =>
            {
                Close(action.Input);
                action.SetOutput(Unit.Default);
                return Task.CompletedTask;
            }).DisposeWith(disposables);
        });
    }

    /// <summary>
    ///     Closes the progress window when this window closes.
    /// </summary>
    /// <param name="e">The event data.</param>
    protected override void OnClosing(WindowClosingEventArgs e)
    {
        if (_installWindow != null)
        {
            _installWindow.Close();
            _installWindow = null;
        }
    }

    #region Progress Reporter

    /// <summary>
    ///     Reports the overall progress.
    /// </summary>
    /// <param name="progress">The progress value as a percentage.</param>
    public void Report(double progress)
    {
        EnsureWindowExists();
        _installWindow?.Report(progress);
    }

    /// <summary>
    ///     Reports the progress of the current file.
    /// </summary>
    /// <param name="progress">The progress value as a percentage.</param>
    /// <param name="fileName">The name of the file being handled.</param>
    /// <param name="downloading">true when the file is downloaded; false when it is read.</param>
    public void Report(double progress, string fileName, bool downloading = true)
    {
        EnsureWindowExists();
        _installWindow?.Report(progress, fileName, downloading);
    }

    /// <summary>
    ///     Reports the progress for the given kind of work.
    /// </summary>
    /// <param name="kind">The kind of work that is running.</param>
    /// <param name="progress">The progress value as a percentage.</param>
    public void Report(EProgressKind kind, double progress)
    {
        EnsureWindowExists();
        _installWindow?.Report(kind, progress);
    }

    /// <summary>
    ///     Sets the number of tasks that are expected.
    /// </summary>
    /// <param name="count">The number of tasks, or <see langword="null" /> to clear the value.</param>
    public void ReportExpectedTask(int? count)
    {
        EnsureWindowExists();
        _installWindow?.ReportExpectedTask(count);
    }

    /// <summary>
    ///     Marks one task as completed.
    /// </summary>
    public void ReportTaskCompleted()
    {
        EnsureWindowExists();
        _installWindow?.ReportTaskCompleted();
    }

    /// <summary>
    ///     Shows the translated text for the given key.
    /// </summary>
    /// <param name="key">The translation key.</param>
    public void Report(string key)
    {
        EnsureWindowExists();
        _installWindow?.Report(key);
    }

    /// <summary>
    ///     Reports progress and shows a translated text for the given key.
    /// </summary>
    /// <param name="progress">The progress value as a percentage.</param>
    /// <param name="key">The translation key.</param>
    /// <param name="args">The values used in the translated text.</param>
    public void Report(double progress, string key, params object[]? args)
    {
        EnsureWindowExists();
        _installWindow?.Report(progress, key, args);
    }

    /// <summary>
    ///     Opens the progress window if it is not open yet.
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

    private void EnsureWindowExists()
    {
        if (_installWindow != null)
            return;

        _installWindow = _windowFactory.CreateProgressWindow();
        _installWindow.Show();
    }

    #endregion
}
