using System;
using System.Diagnostics;
using System.Runtime;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using ReactiveUI.Avalonia;
using Tavstal.BeaconSDK.Enums;
using Tavstal.BeaconSDK.Helpers.Platform;
using Tavstal.KonkordLauncher.Desktop.Helpers;
using Tavstal.KonkordLauncher.Desktop.Services.Presentation.Abstractions;

namespace Tavstal.KonkordLauncher.Desktop.Models.Avalonia;

/// <summary>
///     Base window class used across the Konkord desktop UI that extends ReactiveUI's
///     <see cref="ReactiveWindow{TViewModel}" />.
///     Provides common window helpers (drag/resize, clipboard helpers, file/folder pickers) and ensures the view-model
///     is disposed when the window closes.
/// </summary>
/// <typeparam name="TViewModel">Type of the view model assigned to the window.</typeparam>
public abstract class KonkordWindow<TViewModel> : ReactiveWindow<TViewModel> where TViewModel : class
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="KonkordWindow{TViewModel}" /> class and subscribes to the
    ///     <see cref="Window.Closed" /> event.
    /// </summary>
    protected KonkordWindow()
    {
        Icon = new WindowIcon("avares://KonkordLauncher/Assets/Images/logo.png");
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="KonkordWindow{TViewModel}" /> class and subscribes to the
    ///     <see cref="Window.Closed" /> event.
    /// </summary>
    protected KonkordWindow(IBitmapService bitmapService)
    {
        Icon = new WindowIcon(bitmapService.GetBitmap("avares://KonkordLauncher/Assets/Images/logo.png", 64).Value!);
        Closed += OnClosed;
    }

    /// <summary>
    ///     Strongly-typed DataContext for this window. The setter is protected to allow derived
    ///     windows to assign a view-model during construction while still exposing the typed getter.
    /// </summary>
    public new TViewModel? DataContext
    {
        get => (TViewModel?)base.DataContext;
        protected set => base.DataContext = value;
    }

    /// <summary>
    ///     Called when the window is closing. If the assigned <see cref="DataContext" /> implements <see cref="IDisposable" />
    ///     ,
    ///     it will be disposed here to release resources held by the view-model.
    /// </summary>
    /// <param name="e">Window closing event arguments.</param>
    protected override void OnClosing(WindowClosingEventArgs e)
    {
        (DataContext as IDisposable)?.Dispose();
        base.OnClosing(e);
    }

    /// <summary>
    ///     Handles the window closed event. Unsubscribes from the event, clears the data context,
    ///     forces garbage collection with large object heap compaction, and attempts to trim the
    ///     working set (platform-specific).
    /// </summary>
    /// <param name="sender">The event source.</param>
    /// <param name="e">Event arguments.</param>
    private void OnClosed(object? sender, EventArgs e)
    {
        Closed -= OnClosed;
        DataContext = null;
        Dispatcher.UIThread.Post(() =>
        {
            GC.Collect(2, GCCollectionMode.Forced, true, true);
            GC.WaitForPendingFinalizers();
            GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
            GC.Collect(2, GCCollectionMode.Forced, true, true);

            try
            {
                switch (OSHelper.GetOperatingSystem())
                {
                    case EOperatingSystem.WINDOWS:
                    {
                        MemoryHelper.EmptyWorkingSet(Process.GetCurrentProcess().Handle);
                        break;
                    }
                    case EOperatingSystem.LINUX:
                    {
                        MemoryHelper.malloc_trim(0);
                        break;
                    }
                }
            }
            catch
            {
                /* Suppress */
            }
        }, DispatcherPriority.Background);
    }

    /// <summary>
    ///     Handler to start moving the window when the left mouse button is pressed on a draggable surface.
    ///     Intended to be wired to pointer pressed events on title bars or custom drag handles.
    /// </summary>
    /// <param name="sender">Event source (unused).</param>
    /// <param name="e">Pointer event args.</param>
    protected void DragStart_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            BeginMoveDrag(e);
    }
}
