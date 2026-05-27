namespace Tavstal.KonkordLauncher.Core.Services.Abstractions.Auth;

/// <summary>
/// Provides an abstraction for device code polling services used in Microsoft OAuth2 device flow authentication.
/// </summary>
public interface IMicrosoftDeviceAuthService
{
    /// <summary>
    /// Starts polling the Microsoft device token endpoint to check authentication status.
    /// </summary>
    /// <param name="deviceCode">The device code returned from <see cref="IMicrosoftAuthService.CreateDeviceCodeAsync"/>.
    /// This code uniquely identifies this authentication session.</param>
    /// <param name="interval">The interval in seconds between polling requests. Typically, 5-10 seconds.
    /// Microsoft may provide a recommended polling interval in the device code response.</param>
    /// <param name="cancellationToken">Token to cancel the polling operation if needed.</param>
    /// <returns>A task representing the asynchronous polling operation.</returns>
    Task StartListeningAsync(string deviceCode, int interval, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Stops the device code polling loop.
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the stopping operation if needed.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task StopListeningAsync(CancellationToken cancellationToken = default);
}