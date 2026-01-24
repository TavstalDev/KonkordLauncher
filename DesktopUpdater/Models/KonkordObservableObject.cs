using System;
using System.Reactive.Disposables;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Tavstal.KonkordLauncher.DesktopUpdater.Models;

public abstract class KonkordObservableObject : ObservableObject, IDisposable
{
    private bool _isDisposed;

    // A CompositeDisposable to store all IDisposable resources (e.g., event subscriptions).
    protected CompositeDisposable Disposables { get; } = new CompositeDisposable();
    
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
    
    protected virtual void Dispose(bool disposing)
    {
        if (_isDisposed)
            return;

        if (disposing)
        {
            // Dispose of all managed resources (like subscriptions).
            Disposables.Dispose();
        }

        _isDisposed = true;
    }

}