using System;
using System.Threading;
using System.Threading.Tasks;
using ReactiveUI;
using ReactiveUI.Primitives;

namespace Tavstal.KonkordLauncher.Desktop.Helpers;

/// <summary>
///     Helpers for working with observable streams.
/// </summary>
public static class ReactiveHelper
{
    /// <summary>
    ///     Sends notifications from the source on the main UI thread.
    /// </summary>
    /// <typeparam name="T">The type of the items in the stream.</typeparam>
    /// <param name="source">The source stream.</param>
    /// <returns>A stream that sends notifications on the main thread.</returns>
    public static IObservable<T> ObserveOnMainThread<T>(this IObservable<T> source) where T : notnull
    {
        return source.ObserveOn(RxSchedulers.MainThreadScheduler);
    }

    /// <summary>
    ///     Converts a stream into a task that ends with the first item.
    /// </summary>
    /// <typeparam name="T">The type of the items in the stream.</typeparam>
    /// <param name="source">The source stream.</param>
    /// <param name="cancellationToken">A token that can cancel the task.</param>
    /// <returns>A task with the first item from the stream.</returns>
    public static Task<T> ToTaskCompat<T>(this IObservable<T> source, CancellationToken cancellationToken)
    {
        return source.ToTask(cancellationToken);
    }
}
