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

    /// <summary>
    /// Writes an object to a JSON file at the specified path.
    /// </summary>
    /// <typeparam name="T">The type of the object to serialize.</typeparam>
    /// <param name="path">The file path to write the JSON content to.</param>
    /// <param name="obj">The object to serialize into JSON.</param>
    /// <returns>True if the operation succeeds, otherwise false.</returns>
    public static bool WriteJsonFile<T>(string path, T obj)
    {
        try
        {
            using var stream = new MemoryStream();
            JsonSerializer.Serialize(stream, obj, options: new JsonSerializerOptions()
            {
                IgnoreReadOnlyFields = true,
                IgnoreReadOnlyProperties = true,
                WriteIndented = true
            });
            stream.Position = 0;
            var reader = new StreamReader(stream);
            string content = reader.ReadToEnd();
            File.WriteAllText(path, content, Encoding.UTF8);
            return true;
        }
        catch (Exception ex)
        {
            _logger.Exc("Error in WriteJsonFile<T>:");
            _logger.Error(ex.ToString());
            return false;
        }
    }

    /// <summary>
    /// Asynchronously writes an object to a JSON file at the specified path.
    /// </summary>
    /// <typeparam name="T">The type of the object to serialize.</typeparam>
    /// <param name="path">The file path to write the JSON content to.</param>
    /// <param name="obj">The object to serialize into JSON.</param>
    /// <returns>True if the operation succeeds, otherwise false.</returns>
    public static async Task<bool> WriteJsonFileAsync<T>(string path, T obj)
    {
        try
        {
            using var stream = new MemoryStream();
            await JsonSerializer.SerializeAsync(stream, obj, options: new JsonSerializerOptions()
            {
                IgnoreReadOnlyFields = true,
                IgnoreReadOnlyProperties = true,
                WriteIndented = true
            });
            stream.Position = 0;
            var reader = new StreamReader(stream);
            string content = await reader.ReadToEndAsync();
            await File.WriteAllTextAsync(path, content, Encoding.UTF8);
            return true;
        }
        catch (Exception ex)
        {
            _logger.Exc("Error in WriteJsonFileAsync<T>:");
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
            _logger.Exc("Error in ReadJsonFile<T>:");
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
    public static async Task<T?> ReadJsonFileAsync<T>(string path)
    {
        try
        {
            await using var stream = File.OpenRead(path);
            var local = await JsonSerializer.DeserializeAsync<T>(stream);
            return local;
        }
        catch (Exception ex)
        {
            _logger.Exc("Error in ReadJsonFileAsync<T>:");
            _logger.Error(ex.ToString());
            return default;
        }
    }
}