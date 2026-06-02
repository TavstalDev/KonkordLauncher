using Tavstal.KonkordLauncher.Core.Services.Abstractions;

namespace Tavstal.KonkordLauncher.Core.Services;

/// <summary>
/// Extension methods to run asynchronous initialization for all registered
/// <see cref="IAsyncInitializable"/> implementations resolved from an <see cref="IServiceProvider"/>.
/// </summary>
public static class AsyncInitializerExtensions
{
    /// <summary>
    /// Discovers and runs <see cref="IAsyncInitializable.InitializeAsync(CancellationToken)"/>
    /// on every <see cref="IAsyncInitializable"/> registered in the provided <see cref="IServiceProvider"/>.
    /// </summary>
    /// <param name="provider">The <see cref="IServiceProvider"/> used to resolve registered <see cref="IAsyncInitializable"/> instances.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> that is observed while running initializers.</param>
    /// <returns>A <see cref="Task"/> that completes when all discovered initializers have completed.</returns>
    /// <exception cref="OperationCanceledException">
    /// Thrown when <paramref name="cancellationToken"/> is cancelled during execution.
    /// </exception>
    public static async Task InitializeAllAsync(this IServiceProvider provider, CancellationToken cancellationToken = default)
    {
        var initializables = provider.GetService(typeof(IEnumerable<IAsyncInitializable>))
            as IEnumerable<IAsyncInitializable> ?? [];
        
        foreach (var init in initializables)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await init.InitializeAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}