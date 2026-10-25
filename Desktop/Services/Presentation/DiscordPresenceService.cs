using System;
using Avalonia.Controls;
using DiscordRPC;
using Microsoft.Extensions.Logging;

namespace Tavstal.KonkordLauncher.Desktop.Services.Presentation;

/// <inheritdoc />
public class DiscordPresenceService : IDiscordPresenceService
{
    private readonly ILogger<DiscordPresenceService> _logger;
    private readonly DiscordRpcClient? _rpcClient;

    public DiscordPresenceService(ILogger<DiscordPresenceService> logger)
    {
        _logger = logger;
        if (Design.IsDesignMode)
        {
            _logger.LogWarning("Design mode is active, skipping RPC setup to prevent issues.");
            return;
        }

        _rpcClient = new DiscordRpcClient("1178002101561995416");
        _rpcClient.Initialize();
        _rpcClient.SetPresence(new RichPresence
        {
            Details = "Starting...",
            Timestamps = Timestamps.Now,
            Assets = new Assets { LargeImageKey = "logo", LargeImageText = "Konkord Launcher" }
        });
    }

    /// <inheritdoc />
    public void Update(string details, Timestamps? timestamps = null)
    {
        try
        {
            if (_rpcClient == null)
            {
                _logger.LogError("Discord RPC client is not initialized or disposed.");
                return;
            }

            if (_rpcClient.IsDisposed)
            {
                _logger.LogError("Discord RPC client has been disposed.");
                return;
            }

            _rpcClient.SetPresence(new RichPresence
            {
                Details = details,
                Timestamps = timestamps ?? _rpcClient.CurrentPresence?.Timestamps ?? Timestamps.Now
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occured while trying to update Discord presence.");
        }
    }

    /// <inheritdoc />
    public void Clear()
    {
        try
        {
            if (_rpcClient == null)
            {
                _logger.LogError("Discord RPC client is not initialized or disposed.");
                return;
            }

            if (_rpcClient.IsDisposed)
            {
                _logger.LogError("Discord RPC client has been disposed.");
                return;
            }

            _rpcClient.SetPresence(null);
            _rpcClient.Invoke();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occured while clearing Discord presence.");
        }
    }
}
