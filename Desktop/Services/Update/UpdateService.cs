using System;
using System.Threading;
using System.Threading.Tasks;
using Tavstal.BeaconSDK.Models.Logging;
using Tavstal.BeaconSDK.Services.Abstractions;
using Velopack;
using GithubSource = Velopack.Sources.GithubSource;

namespace Tavstal.KonkordLauncher.Desktop.Services.Update;

/// <inheritdoc />
public class UpdateService : IUpdateService
{
    private readonly IBeaconLogger<UpdateService> _logger;
    private readonly UpdateManager _updateManager;
    private UpdateInfo? _updateInfo;

    public UpdateService(IBeaconLogger<UpdateService> logger)
    {
        _logger = logger;
        _updateManager =
            new UpdateManager(new GithubSource("https://github.com/TavstalDev/KonkordLauncher", null, false));
    }

    /// <inheritdoc />
    public async Task<bool> CheckAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            if (!_updateManager.IsInstalled)
            {
                _logger.LogInformation("Running in portalbe/dev mode, skipping check for updates.");
                return false;
            }

            _updateInfo = await _updateManager.CheckForUpdatesAsync();
            return _updateInfo != null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while checking for updates.");
            return false;
        }
    }

    /// <inheritdoc />
    public async Task<bool> ApplyAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            if (!_updateManager.IsInstalled)
            {
                _logger.LogInformation("Running in portalbe/dev mode, skipping update applying.");
                return true;
            }

            if (_updateInfo == null)
            {
                _logger.LogError($"{nameof(CheckAsync)} must be called before applying updates.");
                return false;
            }

            await _updateManager.DownloadUpdatesAsync(_updateInfo, cancelToken: cancellationToken);
            _updateManager.ApplyUpdatesAndRestart(_updateInfo);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occured while applying updates.");
            return false;
        }
    }
}
