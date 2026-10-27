using System;
using System.Threading;
using System.Threading.Tasks;
using ReactiveUI;
using ReactiveUI.Primitives;

namespace Tavstal.KonkordLauncher.Desktop.Helpers;

public static class ReactiveHelper
{
    public static IObservable<T> ObserveOnMainThread<T>(this IObservable<T> source) where T : notnull
    {
        return source.ObserveOn(RxSchedulers.MainThreadScheduler);
    }

    public static Task<T> ToTaskCompat<T>(this IObservable<T> source, CancellationToken cancellationToken)
    {
        return source.ToTask(cancellationToken);
    }
}
