using System.Reactive;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using ReactiveUI;
using Tavstal.KonkordLauncher.Core.Models;
using Tavstal.KonkordLauncher.Desktop.Models.Avalonia;
using Tavstal.KonkordLauncher.Desktop.Views.Dialogs.Models;

namespace Tavstal.KonkordLauncher.Desktop.Views.Dialogs;

public partial class InstanceLogsWindow : KonkordWindow<InstanceLogsViewModel>
{
    private readonly CoreLogger _logger = CoreLogger.WithModuleType(typeof(InstanceLogsWindow));
    
    public InstanceLogsWindow()
    {
        InitializeComponent();
        DataContext = new InstanceLogsViewModel(string.Empty);
    }
    
    public InstanceLogsWindow(string instanceId)
    {
        InitializeComponent();
        
#if DEBUG
        // Attaches Avalonia Dev Tools for debugging purposes.
        this.AttachDevTools();
#endif
        
        DataContext = new InstanceLogsViewModel(instanceId);
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
    
    private void DragStart_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        // Start moving the window when left mouse button is pressed
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            BeginMoveDrag(e);
        }
    }

    /// <summary>
    /// Copies the provided text to the system clipboard.
    /// </summary>
    /// <param name="text">The text to copy to the clipboard.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task SetClipboardTextAsync(string text)
    {
        var topLevel = GetTopLevel(this);
        if (topLevel?.Clipboard == null)
            return;

        await topLevel.Clipboard.SetTextAsync(text);
    }
}