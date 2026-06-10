using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reactive;
using System.Reactive.Disposables.Fluent;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Threading;
using ReactiveUI;
using Tavstal.KonkordLauncher.Common.Models;
using Tavstal.KonkordLauncher.Core.Models;
using Tavstal.KonkordLauncher.Desktop.Models.Avalonia;
using Tavstal.KonkordLauncher.Desktop.Models.Instance;
using Tavstal.KonkordLauncher.Desktop.Views.Dialogs.Models;

namespace Tavstal.KonkordLauncher.Desktop.Views.Dialogs;

public partial class ResourceReviewWindow : KonkordWindow<ResourceReviewViewModel>, IProgressReporter
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

    #region Progress Reporter
    private ProgressWindow? _installWindow;

    /// <inheritdoc/>
    public void ReportProgress(double progress)
    {
        Dispatcher.UIThread.Post(() =>
        {
            if (_installWindow == null)
                OpenReporter();

            _installWindow?.ReportProgress(progress);
        });
    }

    /// <inheritdoc/>
    public void SetTargetTasks(int? count)
    {
        Dispatcher.UIThread.Post(() =>
        {
            if (_installWindow == null)
                OpenReporter();

            _installWindow?.SetTargetTasks(count);
        });
    }

    /// <inheritdoc/>
    public void CompleteTask()
    {
        Dispatcher.UIThread.Post(() =>
        {
            if (_installWindow == null)
                OpenReporter();

            _installWindow?.CompleteTask();
        });
    }

    /// <inheritdoc/>
    public void SetTargetBytes(long? bytes)
    {
        Dispatcher.UIThread.Post(() =>
        {
            if (_installWindow == null)
                OpenReporter();

            _installWindow?.SetTargetBytes(bytes);
        });
    }

    /// <inheritdoc/>
    public void CompleteBytes(long bytes)
    {
        Dispatcher.UIThread.Post(() =>
        {
            if (_installWindow == null)
                OpenReporter();

            _installWindow?.CompleteBytes(bytes);
        });
    }

    /// <inheritdoc/>
    public void UpdateStatus(string status)
    {
        Dispatcher.UIThread.Post(() =>
        {
            if (_installWindow == null)
                OpenReporter();

            _installWindow?.UpdateStatus(status);
        });
    }

    /// <inheritdoc/>
    public void UpdateStatusTranslated(string key, params object[]? args)
    {
        Dispatcher.UIThread.Post(() =>
        {
            if (_installWindow == null)
                OpenReporter();

            _installWindow?.UpdateStatusTranslated(key, args);
        });
    }

    /// <inheritdoc/>
    public void OpenReporter()
    {
        Dispatcher.UIThread.Post(() =>
        {
            if (_installWindow != null)
                return;

            _installWindow = new ProgressWindow();
            _installWindow.Show();
        });
    }

    /// <inheritdoc/>
    public void CloseReporter()
    {
        Dispatcher.UIThread.Post(() =>
        {
            if (_installWindow == null)
                return;

            _installWindow.Close();
            _installWindow = null;
        });
    }

    #endregion
}