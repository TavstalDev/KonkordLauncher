using System;
using Avalonia.Controls;

namespace Tavstal.KonkordLauncher.Desktop.Models;

/// <summary>
/// Represents an abstract base class for a Konkord window, inheriting from Avalonia's Window class.
/// Provides a method to free memory resources.
/// </summary>
public abstract class KonkordWindow : Window
{
    /// <summary>
    /// Overrides the OnClosing event to free memory resources before the window is closed.
    /// </summary>
    /// <param name="e">The event arguments for the window closing event.</param>
    protected override void OnClosing(WindowClosingEventArgs e)
    {
        if (this.DataContext is KonkordObservableObject konkordObservableObject)
            konkordObservableObject.FreeMemory();
        FreeMemory();
        this.DataContext = null;
        this.Content = null;
        GC.Collect();
        GC.WaitForPendingFinalizers();
        base.OnClosing(e);
    }

    /// <summary>
    /// Abstract method to free memory resources associated with the window.
    /// Must be implemented by derived classes.
    /// </summary>
    protected abstract void FreeMemory();
}