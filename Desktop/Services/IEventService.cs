using Tavstal.KonkordLauncher.Desktop.Models.Enums;

namespace Tavstal.KonkordLauncher.Desktop.Services;

/// <summary>
///     Shares launcher events, such as theme changes and instance updates.
/// </summary>
public interface IEventService
{
    #region Theme Change Event

    /// <summary>
    ///     Handles a theme change.
    /// </summary>
    /// <param name="newTheme">The new theme.</param>
    public delegate void ThemeChangedEventHandler(EThemeType newTheme);

    /// <summary>
    ///     Raised after the theme changes.
    /// </summary>
    public event ThemeChangedEventHandler? OnThemeChanged;

    /// <summary>
    ///     Raises the <see cref="OnThemeChanged" /> event.
    /// </summary>
    /// <param name="newTheme">The new theme.</param>
    void RaiseThemeChanged(EThemeType newTheme);

    #endregion

    #region Accounts Changed Event

    /// <summary>
    ///     Handles a change in the account list.
    /// </summary>
    public delegate void AccountsChangedEventHandler();

    /// <summary>
    ///     Raised after the account list changes.
    /// </summary>
    public event AccountsChangedEventHandler? OnAccountsChanged;

    /// <summary>
    ///     Raises the <see cref="OnAccountsChanged" /> event.
    /// </summary>
    void RaiseAccountsChanged();

    #endregion

    #region Instance Added Event

    /// <summary>
    ///     Handles an added instance.
    /// </summary>
    /// <param name="instanceId">The ID of the instance.</param>
    public delegate void InstanceAddedEventHandler(string instanceId);

    /// <summary>
    ///     Raised after an instance is added.
    /// </summary>
    public event InstanceAddedEventHandler? OnInstanceAdded;

    /// <summary>
    ///     Raises the <see cref="OnInstanceAdded" /> event.
    /// </summary>
    /// <param name="instanceId">The ID of the instance.</param>
    void RaiseInstanceAdded(string instanceId);

    #endregion

    #region Instance Updated Event

    /// <summary>
    ///     Handles an updated instance.
    /// </summary>
    /// <param name="instanceId">The ID of the instance.</param>
    public delegate void InstanceUpdatedEventHandler(string instanceId);

    /// <summary>
    ///     Raised after an instance is updated.
    /// </summary>
    public event InstanceUpdatedEventHandler? OnInstanceUpdated;

    /// <summary>
    ///     Raises the <see cref="OnInstanceUpdated" /> event.
    /// </summary>
    /// <param name="instanceId">The ID of the instance.</param>
    void RaiseInstanceUpdated(string instanceId);

    #endregion

    #region Instance Removed Event

    /// <summary>
    ///     Handles a removed instance.
    /// </summary>
    /// <param name="instanceId">The ID of the instance.</param>
    public delegate void InstanceRemovedEventHandler(string instanceId);

    /// <summary>
    ///     Raised after an instance is removed.
    /// </summary>
    public event InstanceRemovedEventHandler? OnInstanceRemoved;

    /// <summary>
    ///     Raises the <see cref="OnInstanceRemoved" /> event.
    /// </summary>
    /// <param name="instanceId">The ID of the instance.</param>
    void RaiseInstanceRemoved(string instanceId);

    #endregion

    #region Instance Logged Event

    /// <summary>
    ///     Handles a new log message from an instance.
    /// </summary>
    /// <param name="instanceId">The ID of the instance.</param>
    /// <param name="logMessage">The log message.</param>
    public delegate void InstanceLoggedEventHandler(string instanceId, string logMessage);

    /// <summary>
    ///     Raised after an instance writes a log message.
    /// </summary>
    public event InstanceLoggedEventHandler? OnInstanceLogged;

    /// <summary>
    ///     Stores the log message and raises the <see cref="OnInstanceLogged" /> event.
    /// </summary>
    /// <param name="instanceId">The ID of the instance.</param>
    /// <param name="logMessage">The log message to add.</param>
    void RaiseInstanceLogged(string instanceId, string logMessage);

    /// <summary>
    ///     Gets the stored log text for an instance.
    /// </summary>
    /// <param name="instanceId">The ID of the instance.</param>
    /// <returns>The log text, or an empty string if no log exists.</returns>
    string GetInstanceLogs(string instanceId);

    /// <summary>
    ///     Clears the stored log text for an instance.
    /// </summary>
    /// <param name="instanceId">The ID of the instance.</param>
    void CleareInstanceLogs(string instanceId);

    #endregion
}
