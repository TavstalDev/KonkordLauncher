namespace Tavstal.KonkordLauncher.Core.Services.Abstractions;

/// <summary>
/// Defines a contract for services or components that require asynchronous initialization
/// after construction (for example: I/O, network calls, cache warm-up, migrations).
/// </summary>
public interface IAsyncInitializable
{
    /// <summary>
    /// Performs asynchronous initialization work for the implementing component.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the initialization process if needed.</param>
    /// <returns>A task that represents the asynchronous initialization operation.</returns>
    Task InitializeAsync(CancellationToken cancellationToken = default);
}