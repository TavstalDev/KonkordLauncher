using Tavstal.KonkordLauncher.Desktop.Models.Enums;

namespace Tavstal.KonkordLauncher.Desktop.Services.Abstractions;

public interface IEventService
{
    #region Theme Change Event

    public delegate void ThemeChangedEventHandler(EThemeType newTheme);
    
    public event ThemeChangedEventHandler? OnThemeChanged;

    void RaiseThemeChanged(EThemeType newTheme);
    
    #endregion

    #region Accounts Changed Event

    public delegate void AccountsChangedEventHandler();
    
    public event AccountsChangedEventHandler? OnAccountsChanged;

    void RaiseAccountsChanged();
    #endregion

    #region Instance Added Event
    
    public delegate void InstanceAddedEventHandler(string instanceId);
    
    public event InstanceAddedEventHandler? OnInstanceAdded;
    
    void RaiseInstanceAdded(string instanceId);


    #endregion

    #region Instance Updated Event
    
    public delegate void InstanceUpdatedEventHandler(string instanceId);
    
    public event InstanceUpdatedEventHandler? OnInstanceUpdated;
    
    void RaiseInstanceUpdated(string instanceId);

    #endregion
    
    #region Instance Removed Event
    
    public delegate void InstanceRemovedEventHandler(string instanceId);
    
    public event InstanceRemovedEventHandler? OnInstanceRemoved;

    void RaiseInstanceRemoved(string instanceId);
    
    #endregion

    #region Instance Logged Event
    
    public delegate void InstanceLoggedEventHandler(string instanceId, string logMessage);
    
    public event InstanceLoggedEventHandler? OnInstanceLogged;

    void RaiseInstanceLogged(string instanceId, string logMessage);
    
    #endregion
}
