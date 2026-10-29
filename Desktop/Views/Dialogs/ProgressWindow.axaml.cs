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
///     Represents the installation window in the application, which implements the <see cref="IProgressReporter" />
///     interface
///     to report progress and status updates during installation.
/// </summary>
public partial class ProgressWindow : KonkordWindow<ProgressViewModel>, IKonkordProgress
{
    private readonly ITranslationService _translationService = null!;

    public ProgressWindow()
    {
        InitializeComponent();
        if (Design.IsDesignMode)
            DataContext = new ProgressViewModel();
    }

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

    public void Report(double progress)
    {
        Dispatcher.UIThread.Post(() =>
        {
            if (DataContext == null)
                return;
            DataContext.ProgressValue = progress;
        });
    }

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

    public void Report(string key)
    {
        Dispatcher.UIThread.Post(() =>
        {
            if (DataContext == null)
                return;
            DataContext.ProgressText = _translationService.Translate(key);
        });
    }

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
