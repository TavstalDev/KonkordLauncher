using System.Collections.Generic;
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
    public ResourceReviewWindow() : this(null!, EResourceType.MOD, []) {}
    
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

    /// <summary>
    /// Sets the progress value for the installation window. If the window is not open, it will be shown.
    /// </summary>
    /// <param name="progress">The progress value to set, typically between 0.0 and 1.0.</param>
    public void ReportProgress(double progress)
    {
        Dispatcher.UIThread.Post(() =>
        {
            if (_installWindow == null)
                OpenReporter();

            _installWindow?.ReportProgress(progress);
        });
    }

    /// <summary>
    /// Sets the status message for the installation window. If the window is not open, it will be shown.
    /// </summary>
    /// <param name="status">The status message to display.</param>
    public void UpdateStatus(string status)
    {
        Dispatcher.UIThread.Post(() =>
        {
            if (_installWindow == null)
                OpenReporter();

            _installWindow?.UpdateStatus(status);
        });
    }

    /// <summary>
    /// Sets a translated status message for the installation window. If the window is not open, it will be shown.
    /// </summary>
    /// <param name="key">The translation key for the status message.</param>
    /// <param name="args">Optional arguments to format the translated message.</param>
    public void UpdateStatusTranslated(string key, params object[]? args)
    {
        Dispatcher.UIThread.Post(() =>
        {
            if (_installWindow == null)
                OpenReporter();

            _installWindow?.UpdateStatusTranslated(key, args);
        });
    }

    /// <summary>
    /// Displays the installation window as a modal dialog. If the window is already open, this method does nothing.
    /// </summary>
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

    /// <summary>
    /// Hides the installation window if it is currently open.
    /// </summary>
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