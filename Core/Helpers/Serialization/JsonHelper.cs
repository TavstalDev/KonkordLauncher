using System.Collections.Concurrent;
using System.Text;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Tavstal.KonkordLauncher.Core.Helpers.IO;
using Tavstal.KonkordLauncher.Core.Models;
using Tavstal.KonkordLauncher.Core.Models.Logging;

namespace Tavstal.KonkordLauncher.Core.Helpers.Serialization;

/// <summary>
/// Provides helper methods for reading and writing JSON files synchronously and asynchronously.
/// </summary>
public static class JsonHelper
{
    private static readonly ICustomLogger _logger = new CustomLogger(nameof(JsonHelper), LogLevel.Error);
    private static readonly JsonSerializerSettings _jsonSerializerSettings = new()
    {
        Formatting = Formatting.Indented,
        ContractResolver = new IgnoreReadOnlyContractResolver()
    };
    private static readonly ConcurrentDictionary<string, SemaphoreSlim> _fileLocks =
        new(StringComparer.OrdinalIgnoreCase);
    private const int _maxRetries = 5;
    private static readonly TimeSpan _retryDelay = TimeSpan.FromMilliseconds(75);
    
    /// <summary>
    /// Writes an object to a file as JSON using an atomic write pattern with retries.
    /// </summary>
    /// <typeparam name="T">Type of the object to serialize to JSON.</typeparam>
    /// <param name="path">Destination file path. If necessary the parent directory will be created.</param>
    /// <param name="obj">Object to serialize and write.</param>
    /// <returns><c>true</c> if the file was written successfully; otherwise <c>false</c>.</returns>
    public static bool WriteJsonFile<T>(string path, T obj)
    {
        try
        {
            string content = JsonConvert.SerializeObject(obj, _jsonSerializerSettings);
            CreateDirectory(path);
            
            for (int i = 0; i < _maxRetries; i++)
            {
                string tempPath = GetTempPath(path);
                try
                {
                    File.WriteAllText(tempPath, content, Encoding.UTF8);

                    if (!FileSystemHelper.DeleteFile(path))
                    {
                        _logger.LogError($"Failed to delete file {path}.");
                        continue;
                    }
                    File.Move(tempPath, path, true);
                    return true;
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Failed to delete file {path}:\n {ex}");
                    FileSystemHelper.DeleteFile(tempPath);
                }
            }
            
            _logger.LogError($"Failed to acquire file lock for {path} after {_maxRetries} attempts.");
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogCritical($"Error in WriteJsonFile<T> {path}:", ex);
            return false;
        }
    }
    
    /// <summary>
    /// Asynchronously writes an object as formatted JSON to <paramref name="path"/> using an atomic write strategy
    /// and a per-path semaphore to prevent concurrent writers to the same file.
    /// </summary>
    /// <typeparam name="T">Type of the object to serialize to JSON.</typeparam>
    /// <param name="path">Destination file path.</param>
    /// <param name="obj">The object to serialize and write.</param>
    /// <param name="cancellationToken">Token used to request cancellation of the operation.</param>
    /// <returns>
    /// A task that resolves to <c>true</c> when the file was written successfully; <c>false</c> otherwise.
    /// </returns>
    public static async Task<bool> WriteJsonFileAsync<T>(string path, T obj, CancellationToken cancellationToken = default)
    {
        var fileLock = GetFileLock(path);
        
        try
        {
            string content = JsonConvert.SerializeObject(obj, _jsonSerializerSettings);
            CreateDirectory(path);
            
            await fileLock.WaitAsync(_retryDelay, cancellationToken);
            try
            {
                for (int i = 0; i < _maxRetries; i++)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    string tempPath = GetTempPath(path);
                    try
                    {
                        await File.WriteAllTextAsync(tempPath, content, Encoding.UTF8, cancellationToken);

                        if (!FileSystemHelper.DeleteFile(path))
                        {
                            _logger.LogError($"Failed to delete file {path}.");
                            continue;
                        }
                        File.Move(tempPath, path, true);
                        return true;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Failed to delete file {path}:\n {ex}");
                        FileSystemHelper.DeleteFile(tempPath);
                        if (_maxRetries - 1 > i)
                            await Task.Delay(_retryDelay, cancellationToken);
                    }
                }
            }
            finally
            {
                fileLock.Release();
            }

            _logger.LogError($"Failed to acquire file lock for {path} after {_maxRetries} attempts.");
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogCritical($"Error in WriteJsonFileAsync<T> {path}:", ex);
            return false;
        }
    }

    /// <summary>
    /// Asynchronously reads and deserializes a JSON file into an object of type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type of the object to deserialize.</typeparam>
    /// <param name="path">The file path to read the JSON content from.</param>
    /// <returns>The deserialized object, or default if an error occurs.</returns>
    public static async Task<T?> ReadJsonFileAsync<T>(string path)
    {
        try
        {
            await using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using var streamReader = new StreamReader(stream, Encoding.UTF8);
            await using var jsonReader = new JsonTextReader(streamReader);
            var serializer = JsonSerializer.Create(_jsonSerializerSettings);
            return serializer.Deserialize<T>(jsonReader);
        }
        catch (Exception ex)
        {
            _logger.LogCritical($"Error in ReadJsonFileAsync<T> {path}:", ex);
            return default;
        }
    }
    
    /// <summary>
    /// Returns a semaphore used to synchronize file access for the provided path.
    /// </summary>
    /// <param name="path">A file path (can be relative or absolute) for which a lock is required.</param>
    /// <returns>
    /// A <see cref="SemaphoreSlim"/> instance that callers can await on to mutually-exclude access to the specified path.
    /// </returns>
    private static SemaphoreSlim GetFileLock(string path)
    {
        string fullPath = Path.GetFullPath(path);
        return _fileLocks.GetOrAdd(fullPath, _ => new SemaphoreSlim(1, 1));
    }

    /// <summary>
    /// Ensures that the directory portion of the supplied path exists by creating it if necessary.
    /// </summary>
    /// <param name="path">A file path (or directory path). If a file path is supplied, its parent directory will be ensured.</param>
    private static void CreateDirectory(string path)
    {
        var dir = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            Directory.CreateDirectory(dir);
    }

    /// <summary>
    /// Produces a temporary path that is suitable for writing an atomic temporary file next to the target path.
    /// </summary>
    /// <param name="path">The target file path for which a temporary sibling path should be generated.</param>
    /// <returns>A new, likely-unused file path in the same directory as <paramref name="path"/> (or based on <paramref name="path"/> if no directory is present).</returns>
    private static string GetTempPath(string path)
    {
        string? dir = Path.GetDirectoryName(path);
        return Path.Combine(string.IsNullOrEmpty(dir) ? path : dir, Guid.NewGuid().ToString());
    }
}