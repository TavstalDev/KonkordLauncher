using System.Collections.Concurrent;
using Newtonsoft.Json.Linq;
using Tavstal.KonkordLauncher.Common.Models;
using Tavstal.KonkordLauncher.Common.Models.Config;
using Tavstal.KonkordLauncher.Common.Services.Abstractions;
using Tavstal.KonkordLauncher.Core.Helpers.IO;
using Tavstal.KonkordLauncher.Core.Helpers.Serialization;
using Tavstal.KonkordLauncher.Core.Models.Instance;
using Tavstal.KonkordLauncher.Core.Models.Logging;

namespace Tavstal.KonkordLauncher.Common.Services.Implementations;

/// <inheritdoc/>
public class LauncherStore : ILauncherStore
{
    private readonly ICustomLogger _logger;
    private readonly ConcurrentDictionary<string, (DateTime lastWritten, object data)> _cache = [];
    
    /// <summary>
    /// Initializes a new instance of the <see cref="LauncherStore"/> class.
    /// </summary>
    /// <param name="logger">Logger used to record diagnostics and error messages for store operations.</param>
    public LauncherStore(ICustomLogger<LauncherStore> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc/>
    public CoreConfig? GetSettings()
    {
        if (_cache.TryGetValue(PathHelper.LauncherConfigPath, out var cacheResult))
            return (CoreConfig)cacheResult.data;
        return null;
    }

    /// <inheritdoc/>
    public async Task<CoreConfig> GetSettingsAsync(Resolution? screenResolution = null, CancellationToken cancellationToken = default) => 
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

    /// <inheritdoc/>
    public bool SaveSettings(CoreConfig settings) => 
        Save(PathHelper.LauncherConfigPath, settings);

    /// <inheritdoc/>
    public async Task<bool> SaveSettingsAsync(CoreConfig settings, CancellationToken cancellationToken = default) =>
        await SaveAsync(PathHelper.LauncherConfigPath, settings, cancellationToken);

    /// <inheritdoc/>
    public AccountData? GetAccountData()
    {
        if (_cache.TryGetValue(PathHelper.LauncherAccountsPath, out var cacheResult))
            return (AccountData)cacheResult.data;
        return null;
    }

    /// <inheritdoc/>
    public async Task<AccountData> GetAccountDataAsync(CancellationToken cancellationToken = default)
    {
        var result = await ReadOrRecreateAsync(PathHelper.LauncherAccountsPath, () => new AccountData(), cancellationToken);
        foreach (var account in result.Accounts)
            account.IsSelected = result.SelectedAccountId == account.Id;
        return result;
    }

    /// <inheritdoc/>
    public bool SaveAccountData(AccountData accountData) =>
        Save(PathHelper.LauncherAccountsPath, accountData);

    /// <inheritdoc/>
    public async Task<bool> SaveAccountDataAsync(AccountData accountData, CancellationToken cancellationToken = default) =>
        await SaveAsync(PathHelper.LauncherAccountsPath, accountData, cancellationToken);

    /// <inheritdoc/>
    public List<Instance>? GetInstances()
    {
        if (_cache.TryGetValue(PathHelper.LauncherInstancesPath, out var cacheResult))
            return (List<Instance>)cacheResult.data;
        return null;
    }

    /// <inheritdoc/>
    public async Task<List<Instance>> GetInstancesAsync(CancellationToken cancellationToken = default)=> 
        await ReadOrRecreateAsync<List<Instance>>(PathHelper.LauncherInstancesPath, () => [], cancellationToken);

    /// <inheritdoc/>
    public bool SaveInstances(List<Instance> instances) =>
        Save(PathHelper.LauncherInstancesPath, instances);

    /// <inheritdoc/>
    public async Task<bool> SaveInstancesAsync(List<Instance> instances, CancellationToken cancellationToken = default) =>
        await SaveAsync(PathHelper.LauncherInstancesPath, instances, cancellationToken);

    /// <inheritdoc/>
    public List<InstanceResource>? GetInstanceResources(Instance instance) =>
        ReadOrRecreate<List<InstanceResource>?>(instance.GetResourceConfigPath(), () => []);

    /// <inheritdoc/>
    public async Task<List<InstanceResource>> GetInstanceResourcesAsync(Instance instance, CancellationToken cancellationToken = default) =>
        await ReadOrRecreateAsync<List<InstanceResource>>(instance.GetResourceConfigPath(), () => [], cancellationToken);

    /// <inheritdoc/>
    public bool SaveInstanceResources(Instance instance, List<InstanceResource> resources) =>
        Save(instance.GetResourceConfigPath(), resources);

    /// <inheritdoc/>
    public async Task<bool> SaveInstanceResourcesAsync(Instance instance, List<InstanceResource> resources, CancellationToken cancellationToken = default) =>
        await SaveAsync(instance.GetResourceConfigPath(), resources, cancellationToken);

    /// <inheritdoc/>
    public async Task<List<PatchNote>> GetPatchNotesAsync(string cacheDir, CancellationToken cancellationToken = default)
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
    /// Persist the given value to the specified JSON file path using <c>JsonHelper.WriteJsonFile</c>,
    /// then update the in-memory cache for that path.
    /// </summary>
    /// <typeparam name="T">Type of the value being saved.</typeparam>
    /// <param name="path">Absolute path to the JSON file to write.</param>
    /// <param name="value">The value to serialize and save.</param>
    /// <returns>
    /// <see langword="true"/> if the file was written and the cache updated; otherwise <see langword="false"/>.
    /// In error cases the method logs the exception and returns <see langword="false"/> rather than throwing.
    /// </returns>
    private bool Save<T>(string path, T value)
    {
        try
        {
            JsonHelper.WriteJsonFile(path, value);
            var cacheValue = (DateTime.UtcNow, value);
            _cache.AddOrUpdate(path, cacheValue!, (_, _) => cacheValue!);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, $"Error saving file at {path}:");
            return false;
        }
    }

    /// <summary>
    /// Persist the given value to the specified JSON file path using <c>JsonHelper.WriteJsonFileAsync</c>,
    /// then update the in-memory cache for that path.
    /// </summary>
    /// <typeparam name="T">Type of the value being saved.</typeparam>
    /// <param name="path">Absolute path to the JSON file to write.</param>
    /// <param name="value">The value to serialize and save.</param>
    /// <param name="cancellationToken">Cancellation token observed while performing the write operation.</param>
    /// <returns>
    /// <see langword="true"/> if the file was written and the cache updated; otherwise <see langword="false"/>.
    /// In error cases the method logs the exception and returns <see langword="false"/> rather than throwing.
    /// </returns>
    private async Task<bool> SaveAsync<T>(string path, T value, CancellationToken cancellationToken = default)
    {
        try
        {
            await JsonHelper.WriteJsonFileAsync(path, value, cancellationToken);
            var cacheValue = (DateTime.UtcNow, value);
            _cache.AddOrUpdate(path, cacheValue!, (_, _) => cacheValue!);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, $"Error saving file at {path}:");
            return false;
        }
    }
    
    private T ReadOrRecreate<T>(string path, Func<T> factory)
    {
        try
        {
            var fileInfo = new FileInfo(path);
            DateTime lastWritten = fileInfo.LastWriteTimeUtc;
            if (!fileInfo.Exists)
            {
                T result = factory();
                JsonHelper.WriteJsonFile(path, result);
                return result;
            }
            
            if (_cache.TryGetValue(path, out var cacheResult) && fileInfo.LastWriteTimeUtc <= lastWritten)
                return (T)cacheResult.data;

            var readResult = JsonHelper.ReadJsonFile<T>(path);
            if (readResult == null)
            {
                T result = factory();
                File.Move(path, path + ".bak", true);
                JsonHelper.WriteJsonFile(path, result);
                readResult = result;
                lastWritten = DateTime.UtcNow;
            }

            var cacheValue = (lastWritten, readResult);
            _cache.AddOrUpdate(path, cacheValue!, (_, _) => cacheValue!);
            return readResult;
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, $"Error reading or creating file at {path}:");
            return factory();
        }
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
    private async Task<T> ReadOrRecreateAsync<T>(string path, Func<T> factory, CancellationToken cancellationToken = default)
    {
        try
        {
            var fileInfo = new FileInfo(path);
            DateTime lastWritten = fileInfo.LastWriteTimeUtc;
            if (!fileInfo.Exists)
            {
                T result = factory();
                await JsonHelper.WriteJsonFileAsync(path, result, cancellationToken);
                return result;
            }
            
            if (_cache.TryGetValue(path, out var cacheResult) && fileInfo.LastWriteTimeUtc <= lastWritten)
                return (T)cacheResult.data;

            var readResult = await JsonHelper.ReadJsonFileAsync<T>(path);
            if (readResult == null)
            {
                T result = factory();
                File.Move(path, path + ".bak", true);
                await JsonHelper.WriteJsonFileAsync(path, result, cancellationToken);
                readResult = result;
                lastWritten = DateTime.UtcNow;
            }

            var cacheValue = (lastWritten, readResult);
            _cache.AddOrUpdate(path, cacheValue!, (_, _) => cacheValue!);
            return readResult;
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, $"Error reading or creating file at {path}:");
            return factory();
        }
    }
}