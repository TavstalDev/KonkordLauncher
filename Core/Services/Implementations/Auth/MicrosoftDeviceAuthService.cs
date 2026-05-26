using Tavstal.KonkordLauncher.Core.Enums;
using Tavstal.KonkordLauncher.Core.Services.Abstractions.Auth;

namespace Tavstal.KonkordLauncher.Core.Services.Implementations.Auth;

/// <inheritdoc/>
public class MicrosoftDeviceAuthService : IMicrosoftDeviceAuthService
{
    private readonly IMicrosoftAuthService _microsoftAuthService;
    private bool _isListening;
    private DateTime _nextCheckTime = DateTime.MinValue;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="MicrosoftDeviceAuthService"/> class.
    /// </summary>
    /// <param name="microsoftAuthService">The Microsoft authentication service to use for checking device codes.</param>
    public MicrosoftDeviceAuthService(IMicrosoftAuthService microsoftAuthService)
    {
        _microsoftAuthService = microsoftAuthService;
    }
    
    /// <inheritdoc/>
    public async Task StartListeningAsync(string deviceCode, int interval, CancellationToken cancellationToken = default)
    {
        // Prevents starting the listener if it is already active.
        if (_isListening)
            return;
        
        _isListening = true;

        // Continuously checks the device authentication status while the listener is active.
        while (_isListening)
        {
            // Stops listening if the authentication status is no longer pending or none.
            if (!(_microsoftAuthService.AuthStatus == EAuthStatus.PENDING || _microsoftAuthService.AuthStatus == EAuthStatus.NONE))
            {
                await StopListeningAsync(cancellationToken);
                break;
            }

            // Skips the check if the next check time has not been reached.
            if (_nextCheckTime > DateTime.Now)
                continue;

            // Checks the device code and updates the next check time.
            await _microsoftAuthService.CheckDeviceCodeAsync(deviceCode, cancellationToken);
            _nextCheckTime = DateTime.Now.AddSeconds(interval);
        }
    }

    /// <inheritdoc/>
    public Task StopListeningAsync(CancellationToken cancellationToken = default)
    {
        _isListening = false;
        return Task.CompletedTask;
    }
}