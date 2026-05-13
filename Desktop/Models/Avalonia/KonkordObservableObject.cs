using System;
using System.Reactive.Disposables;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Tavstal.KonkordLauncher.Desktop.Models.Avalonia;

/// <summary>
/// Base class for observable view-models that need deterministic cleanup.
/// </summary>
public abstract class KonkordObservableObject : ObservableObject, IDisposable
{
    private bool _isDisposed;

    /// <summary>
    /// Collection of disposable resources owned by this object.
    /// </summary>
    protected CompositeDisposable Disposables { get; } = new();
    
    /// <summary>
    /// Disposes the object and suppresses finalization.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
    
    /// <summary>
    /// Releases managed resources owned by this object.
    /// Derived classes may override this method to clean up additional resources,
    /// but should call the base implementation.
    /// </summary>
    /// <param name="disposing">
    /// True when called from <see cref="Dispose()"/>; false if invoked from a finalizer path.
    /// </param>
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