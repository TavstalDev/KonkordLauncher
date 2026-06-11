using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Microsoft.Extensions.Logging;

using Tavstal.KonkordLauncher.Core.Helpers.IO;
using Tavstal.KonkordLauncher.Core.Models.Logging;

namespace Tavstal.KonkordLauncher.Core.Helpers.Serialization;

/// <summary>
/// Provides helper methods for reading and writing JSON files synchronously and asynchronously.
/// </summary>
public static class JsonHelper
{
    private static readonly ICustomLogger _logger = new CustomLogger(nameof(JsonHelper), LogLevel.Error);
    private static readonly ConcurrentDictionary<string, SemaphoreSlim> _fileLocks =
        new(StringComparer.OrdinalIgnoreCase);
    private const int _maxRetries = 5;
    private static readonly TimeSpan _retryDelay = TimeSpan.FromMilliseconds(1000);
    
    /// <summary>
    /// Writes an object to a file as JSON using an atomic write pattern with retries.
    /// </summary>
    /// <typeparam name="T">Type of the object to serialize to JSON.</typeparam>
    /// <param name="path">Destination file path. If necessary the parent directory will be created.</param>
    /// <param name="obj">Object to serialize and write.</param>
    /// <param name="typeInfo">Type information of the object.</param>
    /// <returns><c>true</c> if the file was written successfully; otherwise <c>false</c>.</returns>
    public static bool WriteJsonFile<T>(string path, T obj, JsonTypeInfo<T> typeInfo)
    {
        var fileLock = GetFileLock(path);
        bool lockTaken = false;
        
        try
        {
            lockTaken = fileLock.Wait(_retryDelay);
            if (!lockTaken)
            {
                _logger.LogError($"Failed to acquire file lock for sync write to {path} within {_retryDelay}.");
                return false;
            }
            CreateDirectory(path);
            
            for (int i = 0; i < _maxRetries; i++)
            {
                string tempPath = GetTempPath(path);
                try
                {
                    using (var fileStream = new FileStream(tempPath, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: 65536, useAsync: true))
                    { 
                        JsonSerializer.Serialize(fileStream, obj, typeInfo);
                    }
                    File.Move(tempPath, path, true);
                    return true;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Failed to delete file {path}:");
                    FileSystemHelper.DeleteFile(tempPath);
                }
            }
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, $"Error in WriteJsonFile<T> {path}:");
            return false;
        }
        finally
        {
            if (lockTaken)
            {
                try
                {
                    fileLock.Release();
                }
                catch (SemaphoreFullException ex)
                {
                    _logger.LogError(ex, $"Attempted to release semaphore for {path} but it was already at max count.");
                }
            }
        }
    }
    
    /// <summary>
    /// Asynchronously writes an object as formatted JSON to <paramref name="path"/> using an atomic write strategy
    /// and a per-path semaphore to prevent concurrent writers to the same file.
    /// </summary>
    /// <typeparam name="T">Type of the object to serialize to JSON.</typeparam>
    /// <param name="path">Destination file path.</param>
    /// <param name="obj">The object to serialize and write.</param>
    /// <param name="typeInfo">The type information used for deserialization.</param>
    /// <param name="cancellationToken">Token used to request cancellation of the operation.</param>
    /// <returns>
    /// A task that resolves to <c>true</c> when the file was written successfully; <c>false</c> otherwise.
    /// </returns>
    public static async Task<bool> WriteJsonFileAsync<T>(string path, T obj, JsonTypeInfo<T> typeInfo, CancellationToken cancellationToken = default)
    {
        var fileLock = GetFileLock(path);
        bool lockTaken = false;

        try
        {
            // Attempt to acquire the per-path semaphore with the specified timeout.
            lockTaken = await fileLock.WaitAsync(_retryDelay, cancellationToken);
            if (!lockTaken)
            {
                _logger.LogError($"Failed to acquire file lock for {path} within {_retryDelay}.");
                return false;
            }

            CreateDirectory(path);

            for (int i = 0; i < _maxRetries; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                string tempPath = GetTempPath(path);
                try
                {
                    await using (var fileStream = new FileStream(tempPath, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: 65536, useAsync: true))
                    {
                        await JsonSerializer.SerializeAsync(fileStream, obj, typeInfo, cancellationToken);
                    }

                    File.Move(tempPath, path, overwrite: true);
                    return true;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Failed to delete file {path}:");
                    FileSystemHelper.DeleteFile(tempPath);
                    if (_maxRetries - 1 > i)
                        await Task.Delay(_retryDelay, cancellationToken);
                }
            }

            _logger.LogError($"Failed to acquire file lock for {path} after {_maxRetries} attempts.");
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, $"Error in WriteJsonFileAsync<T> {path}:");
            return false;
        }
        finally
        {
            if (lockTaken)
            {
                try
                {
                    fileLock.Release();
                }
                catch (SemaphoreFullException ex)
                {
                    _logger.LogError(ex, $"Attempted to release semaphore for {path} but it was already at max count.");
                }
            }
        }
    }
    
    /// <summary>
    /// Reads and deserializes a JSON file into an object of type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type of the object to deserialize.</typeparam>
    /// <param name="path">The file path to read the JSON content from.</param>
    /// <param name="typeInfo">The type information used for deserialization.</param>
    /// <returns>The deserialized object, or default if the file does not exist or an error occurs during deserialization.</returns>
    public static T? ReadJsonFile<T>(string path, JsonTypeInfo<T> typeInfo)
    {
        var fileLock = GetFileLock(path);
        bool lockTaken = false;
        
        try
        {
            // Attempt to acquire the per-path semaphore with the specified timeout.
            lockTaken = fileLock.Wait(_retryDelay);
            if (!lockTaken)
            {
                _logger.LogError($"Failed to acquire file lock for {path} within {_retryDelay}.");
                return default;
            }
            
            if (!File.Exists(path))
                return default;
            
            using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using var streamReader = new StreamReader(stream, Encoding.UTF8);
            return JsonSerializer.Deserialize(stream, typeInfo);
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, $"Error in ReadJsonFileAsync<T> {path}:");
            return default;
        }
        finally
        {
            if (lockTaken)
            {
                try
                {
                    fileLock.Release();
                }
                catch (SemaphoreFullException ex)
                {
                    _logger.LogError(ex, $"Attempted to release semaphore for {path} but it was already at max count.");
                }
            }
        }
    }

    /// <summary>
    /// Asynchronously reads and deserializes a JSON file into an object of type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type of the object to deserialize.</typeparam>
    /// <param name="path">The file path to read the JSON content from.</param>
    /// <param name="typeInfo">The type information used for deserialization.</param>
    /// <param name="cancellationToken">The cancellation token to use for asynchronous operations"></param>
    /// <returns>The deserialized object, or default if an error occurs.</returns>
    public static async Task<T?> ReadJsonFileAsync<T>(string path, JsonTypeInfo<T> typeInfo, CancellationToken cancellationToken = default)
    {
        var fileLock = GetFileLock(path);
        bool lockTaken = false;
        
        try
        {
            // Attempt to acquire the per-path semaphore with the specified timeout.
            lockTaken = await fileLock.WaitAsync(_retryDelay, cancellationToken);
            if (!lockTaken)
            {
                _logger.LogError($"Failed to acquire file lock for {path} within {_retryDelay}.");
                return default;
            }
            
            if (!File.Exists(path))
                return default;
            
            await using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using var streamReader = new StreamReader(stream, Encoding.UTF8);
            return JsonSerializer.Deserialize(stream, typeInfo);
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, $"Error in ReadJsonFileAsync<T> {path}:");
            return default;
        }
        finally
        {
            if (lockTaken)
            {
                try
                {
                    fileLock.Release();
                }
                catch (SemaphoreFullException ex)
                {
                    _logger.LogError(ex, $"Attempted to release semaphore for {path} but it was already at max count.");
                }
            }
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