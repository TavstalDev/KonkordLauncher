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

public partial class ResourceReviewWindow : KonkordWindow<ResourceReviewViewModel>, IKonkordProgress
{
    private readonly IWindowFactory _windowFactory = null!;
    private ProgressWindow? _installWindow;

    public ResourceReviewWindow()
    {
        InitializeComponent();
    }

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

    protected override void OnClosing(WindowClosingEventArgs e)
    {
        if (_installWindow != null)
        {
            _installWindow.Close();
            _installWindow = null;
        }
    }

    #region Progress Reporter

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

    public void OpenReporter()
    {
        EnsureWindowExists();
    }

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
