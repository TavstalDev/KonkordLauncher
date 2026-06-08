using System.Diagnostics.CodeAnalysis;
using System.Reactive;
using System.Reactive.Disposables.Fluent;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using ReactiveUI;
using Tavstal.KonkordLauncher.Desktop.Models.Avalonia;
using Tavstal.KonkordLauncher.Desktop.Views.Dialogs.Models;

namespace Tavstal.KonkordLauncher.Desktop.Views.Dialogs;

/// <summary>
/// Window that displays logs for a specific Minecraft instance.
/// Registers reactive handlers for close, clipboard copy, and auto-scroll interactions.
/// </summary>
public partial class InstanceLogsWindow : KonkordWindow<InstanceLogsViewModel>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="InstanceLogsWindow"/> class for design mode.
    /// </summary>
    public InstanceLogsWindow()
    {
        InitializeComponent();
        DataContext = new InstanceLogsViewModel(string.Empty);
    }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="InstanceLogsWindow"/> class with the specified instance ID.
    /// </summary>
    /// <param name="instanceId">The ID of the instance whose logs will be displayed.</param>
    [RequiresUnreferencedCode( "Trimming may break this functionality if not configured to preserve the necessary members.")]
    public InstanceLogsWindow(string instanceId)
    {
        InitializeComponent();
        DataContext = new InstanceLogsViewModel(instanceId);
        
        if (Design.IsDesignMode)
            return;
        
        this.WhenActivated(disposables =>
        {
            DataContext.CloseWindowInteraction.RegisterHandler(action =>
            {
                Close();
                action.SetOutput(Unit.Default);
                return Task.CompletedTask;
            }).DisposeWith(disposables);
            DataContext.SetClipboardText.RegisterHandler(async action =>
            {
                await SetClipboardTextAsync(action.Input);
                action.SetOutput(Unit.Default);
            }).DisposeWith(disposables);
            DataContext.LogsScrollToEnd.RegisterHandler(action =>
            {
                LogsScrollViewer.Offset =  new Vector(0, LogsScrollViewer.Extent.Height);
                action.SetOutput(Unit.Default);
                return Task.CompletedTask;
            });
        });
    }
}