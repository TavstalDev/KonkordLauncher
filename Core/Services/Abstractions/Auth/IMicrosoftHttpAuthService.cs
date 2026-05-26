using Tavstal.KonkordLauncher.Core.Models;

namespace Tavstal.KonkordLauncher.Core.Services.Abstractions.Auth;

/// <summary>
/// Provides an abstraction for HTTP listening services used in Microsoft OAuth2 authentication callbacks.
/// </summary>
public interface IMicrosoftHttpAuthService
{
    /// <summary>
    /// Starts the HTTP listener to receive Microsoft OAuth2 authorization callbacks.
    /// </summary>
    /// <param name="progressReporter">Optional reporter for tracking listener startup progress and status updates.</param>
    /// <param name="cancellationToken">Token to cancel the listening operation if needed.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task StartListeningAsync(IProgressReporter? progressReporter = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Stops the HTTP listener from receiving further requests.
    /// </summary>
    /// <param name="cancelled">Indicates whether the listener was stopped due to user cancellation (true) 
    /// or normal completion (false). Defaults to true.</param>
    /// <param name="cancellationToken">Token to cancel the stopping operation if needed.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task StopListeningAsync(bool cancelled = true, CancellationToken cancellationToken = default);
}