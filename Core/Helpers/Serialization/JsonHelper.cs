using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;
using Tavstal.KonkordLauncher.Core.Helpers.IO;
using Tavstal.KonkordLauncher.Core.Models;

namespace Tavstal.KonkordLauncher.Core.Helpers.Serialization;

/// <summary>
/// Provides helper methods for reading and writing JSON files synchronously and asynchronously.
/// </summary>
public static class JsonHelper
{
    private static readonly CoreLogger _logger = CoreLogger.WithModuleType(typeof(JsonHelper));
    private static readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        IgnoreReadOnlyFields = true,
        IgnoreReadOnlyProperties = true,
        WriteIndented = true
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
            string content = JsonSerializer.Serialize(obj, _jsonSerializerOptions);
            CreateDirectory(path);
            
            for (int i = 0; i < _maxRetries; i++)
            {
                string tempPath = GetTempPath(path);
                try
                {
                    File.WriteAllText(tempPath, content, Encoding.UTF8);

                    if (!FileSystemHelper.DeleteFile(path))
                    {
                        _logger.Error($"Failed to delete file {path}.");
                        continue;
                    }
                    File.Move(tempPath, path, true);
                    return true;
                }
                catch (Exception ex)
                {
                    _logger.Error($"Failed to delete file {path}:\n {ex.Message}");
                    FileSystemHelper.DeleteFile(tempPath);
                }
            }
            
            _logger.Error($"Failed to acquire file lock for {path} after {_maxRetries} attempts.");
            return false;
        }
        catch (Exception ex)
        {
            _logger.Exc($"Error in WriteJsonFile<T> {path}:");
            _logger.Error(ex.ToString());
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
            string content = JsonSerializer.Serialize(obj, _jsonSerializerOptions);
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
                            _logger.Error($"Failed to delete file {path}.");
                            continue;
                        }
                        File.Move(tempPath, path, true);
                        return true;
                    }
                    catch (Exception ex)
                    {
                        _logger.Error($"Failed to delete file {path}:\n {ex.Message}");
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

            _logger.Error($"Failed to acquire file lock for {path} after {_maxRetries} attempts.");
            return false;
        }
        catch (Exception ex)
        {
            _logger.Exc($"Error in WriteJsonFileAsync<T> {path}:");
            _logger.Error(ex.ToString());
            return false;
        }
    }

    /// <summary>
    /// Reads and deserializes a JSON file into an object of type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type of the object to deserialize.</typeparam>
    /// <param name="path">The file path to read the JSON content from.</param>
    /// <returns>The deserialized object, or default if an error occurs.</returns>
    [Obsolete]
    public static T? ReadJsonFile<T>(string path)
    {
        try
        {
            byte[] buffer;
            using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                buffer = new byte[stream.Length];
                stream.ReadExactly(buffer, 0, (int)stream.Length);
            }
            using var ms = new MemoryStream(buffer);
            var local = JsonSerializer.Deserialize<T>(ms);
            return local;
        }
        catch (Exception ex)
        {
            _logger.Exc($"Error in ReadJsonFile<T> {path}:");
            _logger.Error(ex.ToString());
            return default;
        }
    }

    /// <summary>
    /// Asynchronously reads and deserializes a JSON file into an object of type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type of the object to deserialize.</typeparam>
    /// <param name="path">The file path to read the JSON content from.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>The deserialized object, or default if an error occurs.</returns>
    public static async Task<T?> ReadJsonFileAsync<T>(string path, CancellationToken cancellationToken = default)
    {
        try
        {
            byte[] buffer;
            await using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                buffer = new byte[stream.Length];
                await stream.ReadExactlyAsync(buffer, 0, (int)stream.Length, cancellationToken);
            }
            using var ms = new MemoryStream(buffer);
            var local = await JsonSerializer.DeserializeAsync<T>(ms, cancellationToken: cancellationToken);
            return local;
        }
        catch (Exception ex)
        {
            _logger.Exc($"Error in ReadJsonFileAsync<T> {path}:");
            _logger.Error(ex.ToString());
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