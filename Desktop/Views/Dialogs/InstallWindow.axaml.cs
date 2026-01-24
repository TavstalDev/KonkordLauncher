using System.Reactive;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using ReactiveUI;
using Tavstal.KonkordLauncher.Common.Translation;
using Tavstal.KonkordLauncher.Core.Models;
using Tavstal.KonkordLauncher.Desktop.Models.Avalonia;
using InstallViewModel = Tavstal.KonkordLauncher.Desktop.Views.Dialogs.Models.InstallViewModel;

namespace Tavstal.KonkordLauncher.Desktop.Views.Dialogs;

/// <summary>
/// Represents the installation window in the application, which implements the <see cref="IProgressReporter"/> interface
/// to report progress and status updates during installation.
/// </summary>
public partial class InstallWindow : KonkordWindow<InstallViewModel>, IProgressReporter
{
    /// <summary>
    /// Initializes a new instance of the <see cref="InstallWindow"/> class.
    /// </summary>
    public InstallWindow()
    {
        InitializeComponent();

#if DEBUG
        // Attaches Avalonia Dev Tools for debugging purposes in debug mode.
        this.AttachDevTools();
#endif

        // Sets the data context of the window to an instance of the InstallViewModel.
        DataContext = new InstallViewModel();
        this.WhenActivated(disposables =>
        {
            DataContext.MinimizeWindowInteraction.RegisterHandler(action =>
            {
                WindowState = WindowState.Minimized;
                action.SetOutput(Unit.Default);
                return Task.CompletedTask;
            }).DisposeWith(disposables);
            DataContext.MaximizeWindowInteraction.RegisterHandler(action =>
            {
                WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
                action.SetOutput(Unit.Default);
                return Task.CompletedTask;
            }).DisposeWith(disposables);
            DataContext.CloseWindowInteraction.RegisterHandler(action =>
            {
                Close();
                action.SetOutput(Unit.Default);
                return Task.CompletedTask;
            }).DisposeWith(disposables);
        });
    }
    
    protected override void OnClosing(WindowClosingEventArgs e)
    {
        base.OnClosing(e);
        // Ensure the progress bar is not indeterminate when closing
        // it may use more resources than necessary otherwise
        ProgressBar.IsIndeterminate = false;
    }
    
    private void DragStart_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        // Start moving the window when left mouse button is pressed
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            BeginMoveDrag(e);
        }
    }

    #region  IProgressReporter Implementation
    /// <summary>
    /// Updates the progress value in the associated view model.
    /// </summary>
    /// <param name="progress">The progress value to set, typically a percentage (0-100).</param>
    public void SetProgress(double progress)
    {
        Dispatcher.UIThread.Invoke(() =>
        {
            if (DataContext == null)
                return;
            DataContext.ProgressValue = progress;
        });
    }

    /// <summary>
    /// Updates the status text in the associated view model.
    /// </summary>
    /// <param name="status">The status message to display.</param>
    public void SetStatus(string status)
    {
        Dispatcher.UIThread.Invoke(() =>
        {
            if (DataContext == null)
                return;
            DataContext.ProgressText = status;
        });
    }

    /// <summary>
    /// Updates the status text in the associated view model using a translated string.
    /// </summary>
    /// <param name="statusKey">The translation key for the status message.</param>
    /// <param name="args">Optional arguments to format the translated message.</param>
    public void SetStatusTranslated(string statusKey, params object[]? args)
    {
        Dispatcher.UIThread.Invoke(() =>
        {
            if (DataContext == null)
                return;
            DataContext.ProgressText = TranslationManager.Translate(statusKey, args);
        });
    }
    #endregion
}