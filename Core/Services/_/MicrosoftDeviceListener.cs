using Tavstal.KonkordLauncher.Core.Enums;
using Tavstal.KonkordLauncher.Core.Models;

namespace Tavstal.KonkordLauncher.Core.Services;

/// <summary>
/// Provides functionality to listen for Microsoft device authentication status updates.
/// </summary>
[Obsolete("This class is deprecated. Please use MicrosoftDeviceAuthService instead.")]
public static class MicrosoftDeviceListener
{
    // Indicates whether the listener is currently active.
    private static bool _isListening;

    // Logger instance for logging messages related to the listener.
    // ReSharper disable once UnusedMember.Local
    private static readonly CoreLogger _logger = new(typeof(MicrosoftDeviceListener));

    // Optional progress reporter for reporting progress updates.
    // ReSharper disable once NotAccessedField.Local
    private static IProgressReporter? _progressReporter;

    // Tracks the next time the device code should be checked.
    private static DateTime _nextCheckTime = DateTime.MinValue;

    /// <summary>
    /// Starts listening for device authentication status updates.
    /// </summary>
    /// <param name="deviceCode">The device code to check for authentication status.</param>
    /// <param name="interval">The interval, in seconds, between device code checks.</param>
    /// <param name="progressReporter">Optional progress reporter for reporting progress updates.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task StartListening(string deviceCode, int interval, IProgressReporter? progressReporter = null, CancellationToken cancellationToken = default)
    {
        // Prevents starting the listener if it is already active.
        if (_isListening)
            return;

        _progressReporter = progressReporter;
        _isListening = true;

        // Continuously checks the device authentication status while the listener is active.
        while (_isListening)
        {
            // Stops listening if the authentication status is no longer pending or none.
            if (!(MicrosoftAuthService.AuthStatus == EAuthStatus.PENDING || MicrosoftAuthService.AuthStatus == EAuthStatus.NONE))
            {
                StopListening();
                break;
            }

            // Skips the check if the next check time has not been reached.
            if (_nextCheckTime > DateTime.Now)
                continue;

            // Checks the device code and updates the next check time.
            await MicrosoftAuthService.CheckDeviceCodeAsync(deviceCode, cancellationToken);
            _nextCheckTime = DateTime.Now.AddSeconds(interval);
        }
    }

    /// <summary>
    /// Stops the device authentication listener.
    /// </summary>
    public static void StopListening()
    {
        _isListening = false;
    }
}