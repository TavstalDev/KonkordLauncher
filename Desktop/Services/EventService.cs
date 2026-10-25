using System.Collections.Generic;
using System.Text;
using System.Threading;
using Tavstal.KonkordLauncher.Desktop.Models.Enums;

namespace Tavstal.KonkordLauncher.Desktop.Services;

/// <inheritdoc/>
public class EventService : IEventService
{
    private readonly Dictionary<string, string> _instanceLogs = new();
    private readonly Lock _instanceLogsLock = new();
    public const int MaxLogSizeBytes = 10 * 1024 * 1024;

    /// <inheritdoc/>
    public event IEventService.ThemeChangedEventHandler? OnThemeChanged;
    /// <inheritdoc/>
    public void RaiseThemeChanged(EThemeType newTheme)
    {
        OnThemeChanged?.Invoke(newTheme);
    }

    /// <inheritdoc/>
    public event IEventService.AccountsChangedEventHandler? OnAccountsChanged;
    /// <inheritdoc/>
    public void RaiseAccountsChanged()
    {
        OnAccountsChanged?.Invoke();
    }

    /// <inheritdoc/>
    public event IEventService.InstanceAddedEventHandler? OnInstanceAdded;
    /// <inheritdoc/>
    public void RaiseInstanceAdded(string instanceId)
    {
        OnInstanceAdded?.Invoke(instanceId);
    }

    /// <inheritdoc/>
    public event IEventService.InstanceUpdatedEventHandler? OnInstanceUpdated;
    /// <inheritdoc/>
    public void RaiseInstanceUpdated(string instanceId)
    {
        OnInstanceUpdated?.Invoke(instanceId);
    }

    /// <inheritdoc/>
    public event IEventService.InstanceRemovedEventHandler? OnInstanceRemoved;
    /// <inheritdoc/>
    public void RaiseInstanceRemoved(string instanceId)
    {
        OnInstanceRemoved?.Invoke(instanceId);
    }

    /// <inheritdoc/>
    public event IEventService.InstanceLoggedEventHandler? OnInstanceLogged;
    /// <inheritdoc/>
    public void RaiseInstanceLogged(string instanceId, string logMessage)
    {
        lock (_instanceLogsLock)
        {
            if (_instanceLogs.TryGetValue(instanceId, out var existing))
            {
                var log = existing + logMessage;
                if (Encoding.UTF8.GetByteCount(log) > MaxLogSizeBytes)
                {
                    int trimIndex = log.IndexOf('\n', log.Length / 2);
                    if (trimIndex > 0)
                        log = log[(trimIndex + 1)..];
                }

                _instanceLogs[instanceId] = log;
            }
            else
            {
                _instanceLogs[instanceId] = logMessage;
            }
        }

        OnInstanceLogged?.Invoke(instanceId, logMessage);
    }

    /// <inheritdoc/>
    public string GetInstanceLogs(string instanceId)
    {
        lock (_instanceLogsLock)
            return _instanceLogs.TryGetValue(instanceId, out var log) ? log : string.Empty;
    }

    /// <inheritdoc/>
    public void CleareInstanceLogs(string instanceId)
    {
        lock (_instanceLogsLock)
        {
            if (_instanceLogs.ContainsKey(instanceId))
                _instanceLogs[instanceId] = string.Empty;
        }
    }
}
