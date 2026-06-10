using System.Diagnostics.CodeAnalysis;
using System.Reactive;
using System.Reactive.Disposables.Fluent;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;
using Tavstal.KonkordLauncher.Common.Services.Abstractions;
using Tavstal.KonkordLauncher.Core.Models;
using Tavstal.KonkordLauncher.Desktop.Models.Avalonia;
using Tavstal.KonkordLauncher.Desktop.Views.Dialogs.Models;

namespace Tavstal.KonkordLauncher.Desktop.Views.Dialogs;

/// <summary>
/// Represents the installation window in the application, which implements the <see cref="IProgressReporter"/> interface
/// to report progress and status updates during installation.
/// </summary>
public partial class ProgressWindow : KonkordWindow<ProgressViewModel>, IProgressReporter
{
    private readonly ITranslationService _translationService = null!;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="ProgressWindow"/> class.
    /// </summary>
    [RequiresUnreferencedCode( "Trimming may break this functionality if not configured to preserve the necessary members.")]
    public ProgressWindow() : this(null) { }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="ProgressWindow"/> class.
    /// </summary>
    /// <param name="cancellationTokenSource">An optional cancellation token source to allow cancellation of the installation process.</param>
    [RequiresUnreferencedCode( "Trimming may break this functionality if not configured to preserve the necessary members.")]
    public ProgressWindow(CancellationTokenSource? cancellationTokenSource)
    {
        InitializeComponent();
        DataContext = new ProgressViewModel(cancellationTokenSource);
        
        if (Design.IsDesignMode)
            return;
        
        var services = Program.ServiceProvider;
        _translationService = services.GetRequiredService<ITranslationService>();
        
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

    #region  IProgressReporter Implementation
    
    /// <inheritdoc/>
    public void ReportProgress(double progress)
    {
        Dispatcher.UIThread.Post(() =>
        {
            if (DataContext == null)
                return;
            DataContext.ProgressValue = progress;
        });
    }

    /// <inheritdoc/>
    public void SetTargetTasks(int? count)
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

    /// <inheritdoc/>
    public void CompleteTask()
    {
        Dispatcher.UIThread.Post(() =>
        {
            if (DataContext == null)
                return;
            if (DataContext.CompletedTasks == null)
                DataContext.CompletedTasks = 1;
            DataContext.CompletedTasks++;
        });
    }

    /// <inheritdoc/>
    public void SetTargetBytes(long? bytes)
    {
        
        Dispatcher.UIThread.Post(() =>
        {
            if (DataContext == null)
                return;
            if (bytes == null)
                DataContext.TotalBytes = null;
            DataContext.TotalBytes = bytes;
        });
    }

    /// <inheritdoc/>
    public void CompleteBytes(long bytes)
    {
        
        Dispatcher.UIThread.Post(() =>
        {
            if (DataContext == null)
                return;
            if (DataContext.CompletedBytes == null)
                DataContext.CompletedBytes = 1;
            DataContext.CompletedBytes++;
        });
    }

    /// <inheritdoc/>
    public void UpdateStatus(string status)
    {
        Dispatcher.UIThread.Post(() =>
        {
            if (DataContext == null)
                return;
            DataContext.ProgressText = status;
        });
    }

    /// <inheritdoc/>
    public void UpdateStatusTranslated(string key, params object[]? args)
    {
        Dispatcher.UIThread.Post(() =>
        {
            if (DataContext == null)
                return;
            DataContext.ProgressText = _translationService.Translate(key, args);
        });
    }

    /// <inheritdoc/>
    public void OpenReporter() => Show(); 
    
    /// <inheritdoc/>
    public void CloseReporter() => Close(!DataContext?.IsCancellable);
    #endregion
}