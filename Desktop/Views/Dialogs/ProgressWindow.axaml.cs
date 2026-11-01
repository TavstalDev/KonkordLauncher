using System.Diagnostics.CodeAnalysis;
using System.Reactive;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Threading;
using ReactiveUI;
using ReactiveUI.Primitives;
using Tavstal.BeaconSDK.Models;
using Tavstal.KonkordLauncher.Desktop.Models;
using Tavstal.KonkordLauncher.Desktop.Models.Avalonia;
using Tavstal.KonkordLauncher.Desktop.Services.Localization;
using Tavstal.KonkordLauncher.Desktop.Services.Presentation.Abstractions;
using Tavstal.KonkordLauncher.Desktop.Translation;
using Tavstal.KonkordLauncher.Desktop.Views.Dialogs.Models;

namespace Tavstal.KonkordLauncher.Desktop.Views.Dialogs;

/// <summary>
///     A dialog window that shows progress and status text during long tasks.
/// </summary>
public partial class ProgressWindow : KonkordWindow<ProgressViewModel>, IKonkordProgress
{
    private readonly ITranslationService _translationService = null!;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ProgressWindow" /> class for the designer.
    /// </summary>
    public ProgressWindow()
    {
        InitializeComponent();
        if (Design.IsDesignMode)
            DataContext = new ProgressViewModel();
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="ProgressWindow" /> class with the given services.
    /// </summary>
    /// <param name="translationService">The service used to translate text.</param>
    /// <param name="bitmapService">The service used to load bitmaps.</param>
    /// <param name="cancellationTokenSource">The source used to cancel the running task,
    ///     or <see langword="null" /> when the task cannot be cancelled.</param>
    [RequiresUnreferencedCode(
        "Trimming may break this functionality if not configured to preserve the necessary members.")]
    public ProgressWindow(ITranslationService translationService, IBitmapService bitmapService,
        CancellationTokenSource? cancellationTokenSource) : base(bitmapService)
    {
        InitializeComponent();

        if (Design.IsDesignMode)
            return;

        _translationService = translationService;
        DataContext = new ProgressViewModel(cancellationTokenSource);

        this.WhenActivated(disposables =>
        {
            DataContext.CloseWindowInteraction.RegisterHandler(action =>
            {
                action.SetOutput(Unit.Default);
                Close(action.Input);
                return Task.CompletedTask;
            }).DisposeWith(disposables);
        });
    }

    #region IProgressReporter Implementation

    /// <summary>
    ///     Updates the progress bar with the given value.
    /// </summary>
    /// <param name="progress">The progress value as a percentage.</param>
    public void Report(double progress)
    {
        Dispatcher.UIThread.Post(() =>
        {
            if (DataContext == null)
                return;
            DataContext.ProgressValue = progress;
        });
    }

    /// <summary>
    ///     Updates the progress bar for the current file.
    /// </summary>
    /// <param name="progress">The progress value as a percentage.</param>
    /// <param name="fileName">The name of the file being handled.</param>
    /// <param name="downloading">true when the file is downloaded; false when it is read.</param>
    public void Report(double progress, string fileName, bool downloading = true)
    {
        Dispatcher.UIThread.Post(() =>
        {
            if (DataContext == null)
                return;
            DataContext.ProgressValue = progress;
            // TODO
        });
    }

    /// <summary>
    ///     Updates the progress bar and shows the status text for the given kind of work.
    /// </summary>
    /// <param name="kind">The kind of work that is running.</param>
    /// <param name="progress">The progress value as a percentage.</param>
    public void Report(EProgressKind kind, double progress)
    {
        Dispatcher.UIThread.Post(() =>
        {
            if (DataContext == null)
                return;
            DataContext.ProgressValue = progress;
            DataContext.ProgressText =
                _translationService.Translate(TranslationHelper.GetProgressKey(kind), progress.ToString("0.00"));
        });
    }

    /// <summary>
    ///     Shows the translated text for the given key.
    /// </summary>
    /// <param name="key">The translation key.</param>
    public void Report(string key)
    {
        Dispatcher.UIThread.Post(() =>
        {
            if (DataContext == null)
                return;
            DataContext.ProgressText = _translationService.Translate(key);
        });
    }

    /// <summary>
    ///     Updates the progress bar and shows a translated text for the given key.
    /// </summary>
    /// <param name="progress">The progress value as a percentage.</param>
    /// <param name="key">The translation key.</param>
    /// <param name="args">The values used in the translated text.</param>
    public void Report(double progress, string key, params object[]? args)
    {
        Dispatcher.UIThread.Post(() =>
        {
            if (DataContext == null)
                return;
            DataContext.ProgressValue = progress;
            DataContext.ProgressText = _translationService.Translate(key, args);
        });
    }

    /// <summary>
    ///     Sets the number of tasks that are expected.
    /// </summary>
    /// <param name="count">The number of tasks, or <see langword="null" /> to clear the value.</param>
    public void ReportExpectedTask(int? count)
    {
        Dispatcher.UIThread.Post(() =>
        {
            if (DataContext == null)
                return;
            if (count == null)
                DataContext.CompletedTasks = null;
            DataContext.TotalTasks = count;
        });
    }

    /// <summary>
    ///     Marks one task as completed.
    /// </summary>
    public void ReportTaskCompleted()
    {
        Dispatcher.UIThread.Post(() =>
        {
            if (DataContext == null)
                return;
            DataContext.CompletedTasks ??= 1;
            DataContext.CompletedTasks++;
        });
    }

    #endregion
}
