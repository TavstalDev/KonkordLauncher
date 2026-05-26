using Newtonsoft.Json.Linq;
using Tavstal.KonkordLauncher.Common.Models;
using Tavstal.KonkordLauncher.Common.Models.Config;
using Tavstal.KonkordLauncher.Core.Helpers.IO;
using Tavstal.KonkordLauncher.Core.Helpers.Serialization;
using Tavstal.KonkordLauncher.Core.Models;
using Tavstal.KonkordLauncher.Core.Models.Instance;

namespace Tavstal.KonkordLauncher.Common.Helpers;

/// <summary>
/// Helper methods for reading and creating launcher-related data files (settings, accounts, instances, patch notes).
/// </summary>
[Obsolete("This class is deprecated and will be removed in a future release. Please use the corresponding methods in LauncherStore instead.")]
public static class LauncherHelper
{
    private static readonly CoreLogger _logger = new(typeof(LauncherHelper));
    
    /// <summary>
    /// Load the launcher's core configuration from disk, creating it with reasonable defaults if missing.
    /// </summary>
    /// <param name="screenResolution">Optional initial screen resolution to apply to the created default configuration.</param>
    /// <param name="cancellationToken">A token to observe while writing a newly created configuration file.</param>
    /// <returns>A task that resolves to the loaded <see cref="CoreConfig"/>.</returns>
    public static async Task<CoreConfig> GetLauncherSettingsAsync(Resolution? screenResolution = null, CancellationToken cancellationToken = default) =>
        await ReadOrRecreateAsync(PathHelper.LauncherConfigPath, () =>
        {
            CoreConfig config = new CoreConfig();
            if (screenResolution != null)
            {
                config.Minecraft.WindowWidth = screenResolution.X;
                config.Minecraft.WindowHeight = screenResolution.Y;
            }
            return config;
        }, cancellationToken);
    
    /// <summary>
    /// Load the stored account data from disk, creating an empty
    /// <see cref="AccountData"/> instance if none exists or if the file cannot be read.
    /// </summary>
    /// <param name="cancellationToken">A token to observe while writing a newly created accounts file.</param>
    /// <returns>A task that resolves to the loaded <see cref="AccountData"/>.</returns>
    public static async Task<AccountData> GetAccountDataAsync(CancellationToken cancellationToken = default)
    {
        var result = await ReadOrRecreateAsync(PathHelper.LauncherAccountsPath, () => new AccountData(), cancellationToken);
        foreach (var account in result.Accounts)
            account.IsSelected = result.SelectedAccountId == account.Id;
        return result;
    }
    
    /// <summary>
    /// Load the list of launcher instances from disk, creating an empty list if none exists.
    /// </summary>
    /// <param name="cancellationToken">A token to observe while writing a newly created instances file.</param>
    /// <returns>A task that resolves to a list of <see cref="Instance"/>.</returns>
    public static async Task<List<Instance>> GetInstancesAsync(CancellationToken cancellationToken = default) => 
        await ReadOrRecreateAsync<List<Instance>>(PathHelper.LauncherInstancesPath, () => [], cancellationToken);
    
    /// <summary>
    /// Read cached GitHub release notes saved in the given cache directory and convert them to <see cref="PatchNote"/>.
    /// </summary>
    /// <param name="cacheDir">Directory that contains the GitHub cache file named <c>github_cache.json</c>.</param>
    /// <param name="cancellationToken">A token to observe while reading the cache file from disk.</param>
    /// <returns>
    /// A task that resolves to a list of <see cref="PatchNote"/> instances parsed from the cache file.
    /// If the cache file does not exist, an empty list is returned.
    /// </returns>
    public static async Task<List<PatchNote>> GetPatchNotesAsync(string cacheDir, CancellationToken cancellationToken = default)
    {
        string githubFilePath = Path.Combine(cacheDir, "github_cache.json");
        if (!File.Exists(githubFilePath))
            return [];

        List<PatchNote> result = [];
        string rawJson = await File.ReadAllTextAsync(githubFilePath, cancellationToken);
        JArray jArray = JArray.Parse(rawJson);
        // ReSharper disable once LoopCanBeConvertedToQuery
        foreach (var patchNote in jArray)
        {
            string tagName = patchNote["tag_name"]?.ToString() ?? "Unknown Version";
            string body = patchNote["body"]?.ToString() ?? "No description available.";
            string url = patchNote["html_url"]?.ToString() ?? "";
            result.Add(new PatchNote(tagName, body, url));
        }
        
        return result;
    }

    /// <summary>
    /// Read the JSON file at <paramref name="path"/> and return the deserialized object if possible; otherwise
    /// create a fresh object via <paramref name="factory"/>, persist it and return that.
    /// </summary>
    /// <typeparam name="T">The type to deserialize from the JSON file.</typeparam>
    /// <param name="path">Absolute path to the JSON file to read or create.</param>
    /// <param name="factory">Factory function called to create a new instance of <typeparamref name="T"/>.</param>
    /// <param name="cancellationToken">A token to observe while writing a newly created file to disk.</param>
    /// <returns>A task that resolves to the read or newly created instance of <typeparamref name="T"/>.</returns>
    private static async Task<T> ReadOrRecreateAsync<T>(string path, Func<T> factory, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!File.Exists(path))
            {
                T result = factory();
                await JsonHelper.WriteJsonFileAsync(path, result, cancellationToken);
                return result;
            }

            var readResult = await JsonHelper.ReadJsonFileAsync<T>(path);
            if (readResult == null)
            {
                T result = factory();
                File.Move(path, path + ".bak", true);
                await JsonHelper.WriteJsonFileAsync(path, result, cancellationToken);
                return result;
            }

            return readResult;
        }
        catch (Exception ex)
        {
            _logger.Error($"Error reading or creating file at {path}: {ex}");
            return factory();
        }
    }
}