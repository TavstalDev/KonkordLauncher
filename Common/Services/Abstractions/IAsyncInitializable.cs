namespace Tavstal.KonkordLauncher.Common.Services.Abstractions;

/// <summary>
/// Defines a contract for services that require asynchronous initialization
/// before they can be used safely.
/// </summary>
public interface IAsyncInitializable
{
    /// <summary>
    /// Performs asynchronous initialization logic for the implementing service.
    /// </summary>
    /// <param name="cancellationToken">A token that can be used by the caller to cancel initialization.</param>
    /// <returns>A task that completes when initialization has finished.</returns>
    Task InitializeAsync(CancellationToken cancellationToken = default);
}