using System;
using Avalonia.Controls;
using Avalonia.ReactiveUI;

namespace Tavstal.KonkordLauncher.Desktop.Models;

public abstract class KonkordWindow<TViewModel> : ReactiveWindow<TViewModel> where TViewModel : class
{
    public new TViewModel? DataContext
    {
        get => (TViewModel?)base.DataContext;
        set => base.DataContext = value;
    }
    
    protected override void OnClosing(WindowClosingEventArgs e)
    {
        (DataContext as IDisposable)?.Dispose();
        base.OnClosing(e);
    }
}