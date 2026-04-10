using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;
using Tavstal.KonkordLauncher.Core.Models;

namespace Tavstal.KonkordLauncher.Core.Helpers;

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
            using var stream = File.OpenRead(path);
            var local = JsonSerializer.Deserialize<T>(stream);
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
    /// <returns>The deserialized object, or default if an error occurs.</returns>
    public static async Task<T?> ReadJsonFileAsync<T>(string path, CancellationToken cancellationToken = default)
    {
        try
        {
            await using var stream = File.OpenRead(path);
            var local = await JsonSerializer.DeserializeAsync<T>(stream, cancellationToken: cancellationToken);
            return local;
        }
        catch (Exception ex)
        {
            _logger.Exc($"Error in ReadJsonFileAsync<T> {path}:");
            _logger.Error(ex.ToString());
            return default;
        }
    }
    
    private static SemaphoreSlim GetFileLock(string path)
    {
        string fullPath = Path.GetFullPath(path);
        return _fileLocks.GetOrAdd(fullPath, _ => new SemaphoreSlim(1, 1));
    }

    private static void CreateDirectory(string path)
    {
        var dir = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            Directory.CreateDirectory(dir);
    }

    private static string GetTempPath(string path)
    {
        string? dir = Path.GetDirectoryName(path);
        return Path.Combine(string.IsNullOrEmpty(dir) ? path : dir, Guid.NewGuid().ToString());
    }
}