using Newtonsoft.Json.Linq;
using Tavstal.KonkordLauncher.Common.Models;
using Tavstal.KonkordLauncher.Common.Models.Config;
using Tavstal.KonkordLauncher.Core.Helpers;

namespace Tavstal.KonkordLauncher.Common.Helpers;

/// <summary>
/// Provides helper methods for managing launcher settings, accounts, and instances.
/// </summary>
public static class LauncherHelper
{
    /// <summary>
    /// Retrieves the launcher settings from the configuration file.
    /// If the file does not exist or is invalid, a new configuration is created and saved.
    /// </summary>
    /// <returns>The launcher settings as a <see cref="CoreConfig"/> object.</returns>
    public static CoreConfig GetLauncherSettings()
    {
        if (!File.Exists(PathHelper.LauncherConfigPath))
        {
            CoreConfig result = new CoreConfig();
            JsonHelper.WriteJsonFile(PathHelper.LauncherConfigPath, result);
            return result;
        }

        var readResult = JsonHelper.ReadJsonFile<CoreConfig>(PathHelper.LauncherConfigPath);
        if (readResult == null)
        {
            CoreConfig result = new CoreConfig();
            File.Move(PathHelper.LauncherConfigPath, PathHelper.LauncherConfigPath + ".bak", true);
            JsonHelper.WriteJsonFile(PathHelper.LauncherConfigPath, result);
            return result;
        }

        return readResult;
    }

    /// <summary>
    /// Asynchronously retrieves the launcher settings from the configuration file.
    /// If the file does not exist or is invalid, a new configuration is created and saved.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The task result contains the launcher settings as a <see cref="CoreConfig"/> object.</returns>
    public static async Task<CoreConfig> GetLauncherSettingsAsync()
    {
        if (!File.Exists(PathHelper.LauncherConfigPath))
        {
            CoreConfig result = new CoreConfig();
            await JsonHelper.WriteJsonFileAsync(PathHelper.LauncherConfigPath, result);
            return result;
        }

        var readResult = await JsonHelper.ReadJsonFileAsync<CoreConfig>(PathHelper.LauncherConfigPath);
        if (readResult == null)
        {
            CoreConfig result = new CoreConfig();
            File.Move(PathHelper.LauncherConfigPath, PathHelper.LauncherConfigPath + ".bak", true);
            await JsonHelper.WriteJsonFileAsync(PathHelper.LauncherConfigPath, result);
            return result;
        }

        return readResult;
    }

    /// <summary>
    /// Retrieves the account data from the configuration file.
    /// If the file does not exist or is invalid, a new account data configuration is created and saved.
    /// </summary>
    /// <returns>The account data as an <see cref="AccountData"/> object.</returns>
    public static AccountData GetAccountData()
    {
        if (!File.Exists(PathHelper.LauncherAccountsPath))
        {
            AccountData result = new AccountData();
            JsonHelper.WriteJsonFile(PathHelper.LauncherAccountsPath, result);

            foreach (var account in result.Accounts)
                account.IsSelected = result.SelectedAccountId == account.Id;
            return result;
        }

        var readResult = JsonHelper.ReadJsonFile<AccountData>(PathHelper.LauncherAccountsPath);
        if (readResult == null)
        {
            AccountData result = new AccountData();
            JsonHelper.WriteJsonFile(PathHelper.LauncherAccountsPath, result);
            foreach (var account in result.Accounts)
                account.IsSelected = result.SelectedAccountId == account.Id;
            return result;
        }
        foreach (var account in readResult.Accounts)
            account.IsSelected = readResult.SelectedAccountId == account.Id;
        return readResult;
    }

    /// <summary>
    /// Asynchronously retrieves the account data from the configuration file.
    /// If the file does not exist or is invalid, a new account data configuration is created and saved.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The task result contains the account data as an <see cref="AccountData"/> object.</returns>
    public static async Task<AccountData> GetAccountDataAsync()
    {
        if (!File.Exists(PathHelper.LauncherAccountsPath))
        {
            AccountData result = new AccountData();
            await JsonHelper.WriteJsonFileAsync(PathHelper.LauncherAccountsPath, result);
            foreach (var account in result.Accounts)
                account.IsSelected = result.SelectedAccountId == account.Id;
            return result;
        }

        var readResult = await JsonHelper.ReadJsonFileAsync<AccountData>(PathHelper.LauncherAccountsPath);
        if (readResult == null)
        {
            AccountData result = new AccountData();
            File.Move(PathHelper.LauncherAccountsPath, PathHelper.LauncherAccountsPath + ".bak", true);
            await JsonHelper.WriteJsonFileAsync(PathHelper.LauncherAccountsPath, result);
            foreach (var account in result.Accounts)
                account.IsSelected = result.SelectedAccountId == account.Id;
            return result;
        }
        
        foreach (var account in readResult.Accounts)
            account.IsSelected = readResult.SelectedAccountId == account.Id;

        return readResult;
    }

    /// <summary>
    /// Retrieves the list of instances.
    /// This method is not yet implemented.
    /// </summary>
    /// <returns>A list of <see cref="Instance"/> objects.</returns>
    public static List<Instance> GetInstances()
    {
        if (!File.Exists(PathHelper.LauncherInstancesPath))
        {
            List<Instance> result = [];
            JsonHelper.WriteJsonFile(PathHelper.LauncherInstancesPath, result);
            return result;
        }

        var readResult = JsonHelper.ReadJsonFile<List<Instance>>(PathHelper.LauncherInstancesPath);
        if (readResult == null)
        {
            List<Instance> result = [];
            File.Move(PathHelper.LauncherInstancesPath, PathHelper.LauncherInstancesPath + ".bak", true);
           JsonHelper.WriteJsonFile(PathHelper.LauncherInstancesPath, result);
            return result;
        }
        
        return readResult;
    }

    /// <summary>
    /// Asynchronously retrieves the list of instances.
    /// This method is not yet implemented.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The task result contains a list of <see cref="Instance"/> objects.</returns>
    public static async Task<List<Instance>> GetInstancesAsync()
    {
        if (!File.Exists(PathHelper.LauncherInstancesPath))
        {
            List<Instance> result = [];
            await JsonHelper.WriteJsonFileAsync(PathHelper.LauncherInstancesPath, result);
            return result;
        }

        var readResult = await JsonHelper.ReadJsonFileAsync<List<Instance>>(PathHelper.LauncherInstancesPath);
        if (readResult == null)
        {
            List<Instance> result = [];
            File.Move(PathHelper.LauncherInstancesPath, PathHelper.LauncherInstancesPath + ".bak", true);
            await JsonHelper.WriteJsonFileAsync(PathHelper.LauncherInstancesPath, result);
            return result;
        }
        
        return readResult;
    }
    
    /// <summary>
    /// Retrieves a list of patch notes from a cached GitHub JSON file.
    /// If the file does not exist, an empty list is returned.
    /// </summary>
    /// <param name="cacheDir">The directory where the GitHub cache file is located.</param>
    /// <returns>A list of <see cref="PatchNote"/> objects containing the patch notes.</returns>
    public static List<PatchNote> GetPatchNotes(string cacheDir)
    {
        string githubFilePath = Path.Combine(cacheDir, "github_cache.json");
        if (!File.Exists(githubFilePath))
            return [];

        List<PatchNote> result = [];
        string rawJson = File.ReadAllText(githubFilePath);
        JArray jArray = JArray.Parse(rawJson);
        foreach (var patchNote in jArray)
        {
            string tagName = patchNote["tag_name"]?.ToString() ?? "Unknown Version";
            string body = patchNote["body"]?.ToString() ?? "No description available.";
            string url = patchNote["html_url"]?.ToString() ?? "";
            result.Add(new PatchNote(tagName, body, url));
        }
        
        return result;
    }
}