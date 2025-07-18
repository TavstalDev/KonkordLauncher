using Tavstal.KonkordLauncher.Core.Models.Launcher;

namespace Tavstal.KonkordLauncher.Core.Helpers;

public static class AccountHelper
{
    // TODO: Probably should be moved to Desktop
    
    /// <summary>
    /// Retrieves the launcher settings, if available.
    /// </summary>
    /// <returns>
    /// A <see cref="LauncherSettings"/> object representing the launcher settings, or null if settings are not available.
    /// </returns>
    public static LauncherSettings? GetLauncherSettings()
    {
        return JsonHelper.ReadJsonFile<LauncherSettings?>(_launcherJsonFile);
    }

    /// <summary>
    /// Asynchronously retrieves the launcher settings, if available.
    /// </summary>
    /// <returns>
    /// A <see cref="Task{TResult}"/> representing the asynchronous operation. The task result contains a <see cref="LauncherSettings"/> object representing the launcher settings, or null if settings are not available.
    /// </returns>
    public static async Task<LauncherSettings?> GetLauncherSettingsAsync()
    {
        return await JsonHelper.ReadJsonFileAsync<LauncherSettings?>(_launcherJsonFile);
    }

    /// <summary>
    /// Retrieves the account data, if available.
    /// </summary>
    /// <returns>
    /// An <see cref="AccountData"/> object representing the account data, or null if data is not available.
    /// </returns>
    public static AccountData? GetAccountData()
    {
        return JsonHelper.ReadJsonFile<AccountData?>(_accountsJsonFile);
    }

    /// <summary>
    /// Asynchronously retrieves the account data, if available.
    /// </summary>
    /// <returns>
    /// A <see cref="Task{TResult}"/> representing the asynchronous operation. The task result contains an <see cref="AccountData"/> object representing the account data, or null if data is not available.
    /// </returns>
    public static async Task<AccountData?> GetAccountDataAsync()
    {
        return await JsonHelper.ReadJsonFileAsync<AccountData?>(_accountsJsonFile);
    }
}