using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.ReactiveUI;

namespace Tavstal.KonkordLauncher.Desktop.Models.Avalonia;

public abstract class KonkordWindow<TViewModel> : ReactiveWindow<TViewModel> where TViewModel : class
{
    public new TViewModel? DataContext
    {
        get => (TViewModel?)base.DataContext;
        init => base.DataContext = value;
    }
    
    protected override void OnClosing(WindowClosingEventArgs e)
    {
        (DataContext as IDisposable)?.Dispose();
        base.OnClosing(e);
    }
    
    protected void DragStart_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            BeginMoveDrag(e);
    }
    
    protected void ResizeHandle_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (!CanResize)
            return;
    
        if (!e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            return;
    
        if (sender is not Control { Tag: string tag })
            return;
    
        if (!Enum.TryParse(tag, ignoreCase: true, out WindowEdge edge))
            return;
    
        BeginResizeDrag(edge, e);
        e.Handled = true; // prevent bubbling to DragStart_PointerPressed
    }
    
    
    protected async Task SetClipboardTextAsync(string text)
    {
        if (string.IsNullOrEmpty(text))
            return;
        
        var topLevel = GetTopLevel(this);
        if (topLevel?.Clipboard == null)
            return;

        await topLevel.Clipboard.SetTextAsync(text);
    }
}