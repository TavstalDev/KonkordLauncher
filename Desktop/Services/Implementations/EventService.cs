using Tavstal.KonkordLauncher.Desktop.Models.Enums;
using Tavstal.KonkordLauncher.Desktop.Services.Abstractions;

namespace Tavstal.KonkordLauncher.Desktop.Services.Implementations;

public class EventService : IEventService
{
    public event IEventService.ThemeChangedEventHandler? OnThemeChanged;
    public void RaiseThemeChanged(EThemeType newTheme)
    {
        OnThemeChanged?.Invoke(newTheme);
    }

    public event IEventService.AccountsChangedEventHandler? OnAccountsChanged;
    public void RaiseAccountsChanged()
    {
        OnAccountsChanged?.Invoke();
    }

    public event IEventService.InstanceAddedEventHandler? OnInstanceAdded;
    public void RaiseInstanceAdded(string instanceId)
    {
        OnInstanceAdded?.Invoke(instanceId);
    }

    public event IEventService.InstanceUpdatedEventHandler? OnInstanceUpdated;
    public void RaiseInstanceUpdated(string instanceId)
    {
        OnInstanceUpdated?.Invoke(instanceId);
    }

    public event IEventService.InstanceRemovedEventHandler? OnInstanceRemoved;
    public void RaiseInstanceRemoved(string instanceId)
    {
        OnInstanceRemoved?.Invoke(instanceId);
    }

    public event IEventService.InstanceLoggedEventHandler? OnInstanceLogged;
    public void RaiseInstanceLogged(string instanceId, string logMessage)
    {
        OnInstanceLogged?.Invoke(instanceId, logMessage);
    }
}
