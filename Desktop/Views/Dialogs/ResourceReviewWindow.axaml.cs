using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reactive;
using System.Threading.Tasks;
using Avalonia.Controls;
using ReactiveUI;
using ReactiveUI.Primitives;
using Tavstal.BeaconSDK.Models;
using Tavstal.KonkordLauncher.Desktop.Models;
using Tavstal.KonkordLauncher.Desktop.Models.Avalonia;
using Tavstal.KonkordLauncher.Desktop.Models.DTOs;
using Tavstal.KonkordLauncher.Desktop.Models.Enums;
using Tavstal.KonkordLauncher.Desktop.Models.Instance;
using Tavstal.KonkordLauncher.Desktop.Views.Dialogs.Models;

namespace Tavstal.KonkordLauncher.Desktop.Views.Dialogs;

public partial class ResourceReviewWindow : KonkordWindow<ResourceReviewViewModel>, IKonkordProgress
{
    [RequiresUnreferencedCode( "Trimming may break this functionality if not configured to preserve the necessary members.")]
    public ResourceReviewWindow() : this(null!, EResourceType.MOD, []) {}

    [RequiresUnreferencedCode( "Trimming may break this functionality if not configured to preserve the necessary members.")]
    public ResourceReviewWindow(Instance instance, EResourceType resourceType, List<ResourceDownloadModel> resources)
    {
        InitializeComponent();

        DataContext = new ResourceReviewViewModel(instance, resourceType, resources, this);

        if (Design.IsDesignMode)
            return;

        this.WhenActivated(disposables =>
        {
            DataContext.CloseWindowInteraction.RegisterHandler(action =>
            {
                Close(action.Input);
                action.SetOutput(Unit.Default);
                return Task.CompletedTask;
            }).DisposeWith(disposables);
            DataContext.ShowAlertInteraction.RegisterHandler(async action =>
            {
                AlertWindow alertWindow = new(action.Input.Title, action.Input.Message, action.Input.Type);
                await alertWindow.ShowDialog<bool>(this);
                action.SetOutput(Unit.Default);
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

    public void Report(double progress, string key, params object[]? args)
    {
        EnsureWindowExists();
        _installWindow?.Report(progress, key, args);
    }

    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
    private void EnsureWindowExists()
    {
        _installWindow ??= new ProgressWindow();
        _installWindow.Show();
    }

    #endregion
}
