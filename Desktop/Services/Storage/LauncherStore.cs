using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text.Json.Serialization.Metadata;
using System.Threading;
using System.Threading.Tasks;
using Tavstal.BeaconSDK.Helpers.Serialization;
using Tavstal.BeaconSDK.Models.Instance;
using Tavstal.BeaconSDK.Models.Logging;
using Tavstal.BeaconSDK.Services.Abstractions;
using Tavstal.KonkordLauncher.Desktop.Models.DTOs;
using Tavstal.KonkordLauncher.Desktop.Models.DTOs.Config;
using Tavstal.KonkordLauncher.Desktop.Serialization;

namespace Tavstal.KonkordLauncher.Desktop.Services.Storage;

/// <inheritdoc />
public class LauncherStore : ILauncherStore
{
    private readonly string _accountsPath = Path.Combine(Directory.GetCurrentDirectory(), "accounts.json");
    private readonly ConcurrentDictionary<string, (DateTime lastWritten, object data)> _cache = [];
    private readonly string _instancesPath = Path.Combine(Directory.GetCurrentDirectory(), "instances.json");
    private readonly string _launcherConfigPath = Path.Combine(Directory.GetCurrentDirectory(), "config.json");
    private readonly IBeaconLogger _logger;
    private readonly string _tempDir = Path.Combine(Path.GetTempPath(), $"kl-{Guid.NewGuid()}");

    public LauncherStore(IBeaconLogger<LauncherStore> logger)
    {
        _logger = logger;
        Directory.CreateDirectory(_tempDir);
    }

    /// <inheritdoc />
    public string GetTempDir()
    {
        return _tempDir;
    }

    /// <inheritdoc />
    public CoreConfig? GetSettings()
    {
        CoreConfig? result = null;
        if (_cache.TryGetValue(_launcherConfigPath, out var cacheResult))
            result = (CoreConfig)cacheResult.data;
        return result;
    }

    /// <inheritdoc />
    public async Task<CoreConfig> GetSettingsAsync(Resolution? screenResolution = null,
        CancellationToken cancellationToken = default)
    {
        return await ReadOrRecreateAsync(_launcherConfigPath, () =>
        {
            var config = new CoreConfig();
            if (screenResolution != null)
            {
                config.Minecraft.WindowWidth = screenResolution.X;
                config.Minecraft.WindowHeight = screenResolution.Y;
            }

            return config;
        }, CommonJsonContext.Default.CoreConfig, cancellationToken);
    }

    /// <inheritdoc />
    public bool SaveSettings(CoreConfig settings)
    {
        return Save(_launcherConfigPath, settings, CommonJsonContext.Default.CoreConfig);
    }

    /// <inheritdoc />
    public async Task<bool> SaveSettingsAsync(CoreConfig settings, CancellationToken cancellationToken = default)
    {
        return await SaveAsync(_launcherConfigPath, settings, CommonJsonContext.Default.CoreConfig, cancellationToken);
    }

    /// <inheritdoc />
    public AccountData? GetAccountData()
    {
        if (_cache.TryGetValue(_accountsPath, out var cacheResult))
            return (AccountData)cacheResult.data;
        return null;
    }

    /// <inheritdoc />
    public async Task<AccountData> GetAccountDataAsync(CancellationToken cancellationToken = default)
    {
        var result = await ReadOrRecreateAsync(_accountsPath, () => new AccountData(),
            CommonJsonContext.Default.AccountData, cancellationToken);
        return result;
    }

    /// <inheritdoc />
    public bool SaveAccountData(AccountData accountData)
    {
        return Save(_accountsPath, accountData, CommonJsonContext.Default.AccountData);
    }

    /// <inheritdoc />
    public async Task<bool> SaveAccountDataAsync(AccountData accountData, CancellationToken cancellationToken = default)
    {
        return await SaveAsync(_accountsPath, accountData, CommonJsonContext.Default.AccountData, cancellationToken);
    }

    /// <inheritdoc />
    public List<Instance>? GetInstances()
    {
        if (_cache.TryGetValue(_instancesPath, out var cacheResult))
            return (List<Instance>)cacheResult.data;
        return null;
    }

    /// <inheritdoc />
    public async Task<List<Instance>> GetInstancesAsync(CancellationToken cancellationToken = default)
    {
        return await ReadOrRecreateAsync<List<Instance>>(_instancesPath, () => [],
            CommonJsonContext.Default.ListInstance, cancellationToken);
    }

    /// <inheritdoc />
    public bool SaveInstances(List<Instance> instances)
    {
        return Save(_instancesPath, instances, CommonJsonContext.Default.ListInstance);
    }

    /// <inheritdoc />
    public async Task<bool> SaveInstancesAsync(List<Instance> instances, CancellationToken cancellationToken = default)
    {
        return await SaveAsync(_instancesPath, instances, CommonJsonContext.Default.ListInstance, cancellationToken);
    }

    /// <inheritdoc />
    public List<InstanceResource>? GetInstanceResources(Instance instance)
    {
        return ReadOrRecreate<List<InstanceResource>?>(instance.GetResourceConfigPath(), () => [],
            CommonJsonContext.Default.ListInstanceResource);
    }

    /// <inheritdoc />
    public async Task<List<InstanceResource>> GetInstanceResourcesAsync(Instance instance,
        CancellationToken cancellationToken = default)
    {
        return await ReadOrRecreateAsync<List<InstanceResource>>(instance.GetResourceConfigPath(), () => [],
            CommonJsonContext.Default.ListInstanceResource, cancellationToken);
    }

    /// <inheritdoc />
    public bool SaveInstanceResources(Instance instance, List<InstanceResource> resources)
    {
        return Save(instance.GetResourceConfigPath(), resources, CommonJsonContext.Default.ListInstanceResource);
    }

    /// <inheritdoc />
    public async Task<bool> SaveInstanceResourcesAsync(Instance instance, List<InstanceResource> resources,
        CancellationToken cancellationToken = default)
    {
        return await SaveAsync(instance.GetResourceConfigPath(), resources,
            CommonJsonContext.Default.ListInstanceResource, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<List<PatchNote>> GetPatchNotesAsync(string cacheDir,
        CancellationToken cancellationToken = default)
    {
        var githubFilePath = Path.Combine(cacheDir, "github_cache.json");
        if (!File.Exists(githubFilePath))
            return [];

        List<PatchNote>? result =
            await JsonHelper.ReadJsonFileAsync(githubFilePath, CommonJsonContext.Default.ListPatchNote,
                cancellationToken);

        return result ?? [];
    }

    /// <summary>
    ///     Persist the given value to the specified JSON file path using <c>JsonHelper.WriteJsonFile</c>,
    ///     then update the in-memory cache for that path.
    /// </summary>
    /// <typeparam name="T">Type of the value being saved.</typeparam>
    /// <param name="path">Absolute path to the JSON file to write.</param>
    /// <param name="value">The value to serialize and save.</param>
    /// <param name="typeInfo">Type information for the value.</param>
    /// <returns>
    ///     <see langword="true" /> if the file was written and the cache updated; otherwise <see langword="false" />.
    ///     In error cases the method logs the exception and returns <see langword="false" /> rather than throwing.
    /// </returns>
    private bool Save<T>(string path, T value, JsonTypeInfo<T> typeInfo)
    {
        try
        {
            JsonHelper.WriteJsonFile(path, value, typeInfo);
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
    ///     Persist the given value to the specified JSON file path using <c>JsonHelper.WriteJsonFileAsync</c>,
    ///     then update the in-memory cache for that path.
    /// </summary>
    /// <typeparam name="T">Type of the value being saved.</typeparam>
    /// <param name="path">Absolute path to the JSON file to write.</param>
    /// <param name="value">The value to serialize and save.</param>
    /// <param name="typeInfo">Type information for the value.</param>
    /// <param name="cancellationToken">Cancellation token observed while performing the write operation.</param>
    /// <returns>
    ///     <see langword="true" /> if the file was written and the cache updated; otherwise <see langword="false" />.
    ///     In error cases the method logs the exception and returns <see langword="false" /> rather than throwing.
    /// </returns>
    private async Task<bool> SaveAsync<T>(string path, T value, JsonTypeInfo<T> typeInfo,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await JsonHelper.WriteJsonFileAsync(path, value, typeInfo, cancellationToken);
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
    ///     Reads data from a JSON file if it exists; otherwise, recreates the file using a factory method.
    /// </summary>
    /// <typeparam name="T">The type of data to read or write.</typeparam>
    /// <param name="path">The path to the JSON file.</param>
    /// <param name="factory">A delegate that creates new instances of T if the file does not exist.</param>
    /// <param name="typeInfo">JSON serialization information for the type T.</param>
    /// <returns>The data read from the file, or newly created data if the file did not exist.</returns>
    private T ReadOrRecreate<T>(string path, Func<T> factory, JsonTypeInfo<T> typeInfo)
    {
        try
        {
            var fileInfo = new FileInfo(path);
            var lastWritten = fileInfo.LastWriteTimeUtc;
            if (!fileInfo.Exists)
            {
                var result = factory();
                JsonHelper.WriteJsonFile(path, result, typeInfo);
                return result;
            }

            if (_cache.TryGetValue(path, out var cacheResult) && lastWritten <= cacheResult.lastWritten)
                return (T)cacheResult.data;

            var readResult = JsonHelper.ReadJsonFile(path, typeInfo);
            if (readResult == null)
            {
                var result = factory();
                File.Move(path, path + ".bak", true);
                JsonHelper.WriteJsonFile(path, result, typeInfo);
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
    ///     Read the JSON file at <paramref name="path" /> and return the deserialized object if possible; otherwise
    ///     create a fresh object via <paramref name="factory" />, persist it and return that.
    /// </summary>
    /// <typeparam name="T">The type to deserialize from the JSON file.</typeparam>
    /// <param name="path">Absolute path to the JSON file to read or create.</param>
    /// <param name="factory">Factory function called to create a new instance of <typeparamref name="T" />.</param>
    /// <param name="typeInfo">Type information for the value.</param>
    /// <param name="cancellationToken">A token to observe while writing a newly created file to disk.</param>
    /// <returns>A task that resolves to the read or newly created instance of <typeparamref name="T" />.</returns>
    private async Task<T> ReadOrRecreateAsync<T>(string path, Func<T> factory, JsonTypeInfo<T> typeInfo,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var fileInfo = new FileInfo(path);
            var lastWritten = fileInfo.LastWriteTimeUtc;
            if (!fileInfo.Exists)
            {
                var result = factory();
                await JsonHelper.WriteJsonFileAsync(path, result, typeInfo, cancellationToken);
                return result;
            }

            if (_cache.TryGetValue(path, out var cacheResult) && lastWritten <= cacheResult.lastWritten)
                return (T)cacheResult.data;

            var readResult = await JsonHelper.ReadJsonFileAsync(path, typeInfo, cancellationToken);
            if (readResult == null)
            {
                var result = factory();
                File.Move(path, path + ".bak", true);
                await JsonHelper.WriteJsonFileAsync(path, result, typeInfo, cancellationToken);
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
