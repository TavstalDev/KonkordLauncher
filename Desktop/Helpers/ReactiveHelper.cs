using System;
using System.Threading;
using System.Threading.Tasks;

namespace Tavstal.KonkordLauncher.Desktop.Helpers;

public static class ReactiveHelper
{
    public static IObservable<T> ObserveOnMainThread<T>(this IObservable<T> source) where T : notnull
    {
        return ReactiveUI.Primitives.LinqExtensions.ObserveOn(source, ReactiveUI.RxSchedulers.MainThreadScheduler);
    }

    public static Task<T> ToTaskCompat<T>(this IObservable<T> source, CancellationToken cancellationToken)
    {
        return ReactiveUI.Primitives.LinqExtensions.ToTask(source, cancellationToken);
    }
}
