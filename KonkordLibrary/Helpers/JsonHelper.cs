using System.Text;
using System.Text.Json;
using System.IO;

namespace KonkordLibrary.Helpers
{
    public static class JsonHelper
    {
        /// <summary>
        /// Writes the provided object to a JSON file.
        /// </summary>
        /// <typeparam name="T">The type of the object to write.</typeparam>
        /// <param name="path">The path to the JSON file.</param>
        /// <param name="obj">The object to write to the JSON file.</param>
        /// <returns>
        /// A <see cref="bool"/> value indicating whether the writing operation was successful.
        /// </returns>
        public static bool WriteJsonFile<T>(string path, T obj)
        {
            try
            {
                using (var stream = new MemoryStream())
                {
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
                }
                return true;
            }
            catch (Exception ex)
            {
                NotificationHelper.SendErrorMsg(ex.ToString(), "Error in WriteJsonFile<T>");
                return false;
            }
        }

        /// <summary>
        /// Asynchronously writes the provided object to a JSON file.
        /// </summary>
        /// <typeparam name="T">The type of the object to write.</typeparam>
        /// <param name="path">The path to the JSON file.</param>
        /// <param name="obj">The object to write to the JSON file.</param>
        /// <returns>
        /// A <see cref="Task{TResult}"/> representing the asynchronous operation. The task result contains a <see cref="bool"/> value indicating whether the writing operation was successful.
        /// </returns>
        public static async Task<bool> WriteJsonFileAsync<T>(string path, T obj)
        {
            try
            {
                using (var stream = new MemoryStream())
                {
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
                }
                return true;
            }
            catch (Exception ex)
            {
                NotificationHelper.SendErrorMsg(ex.ToString(), "Error in WriteJsonFileAsync<T>");
                return false;
            }
        }

        /// <summary>
        /// Reads the content of a JSON file and deserializes it to the specified type.
        /// </summary>
        /// <typeparam name="T">The type to deserialize the JSON content to.</typeparam>
        /// <param name="path">The path to the JSON file.</param>
        /// <returns>
        /// The deserialized object of type <typeparamref name="T"/>, or null if the file does not exist or deserialization fails.
        /// </returns>
        public static T? ReadJsonFile<T>(string path)
        {
            try
            {
                T? local = default;
                using (var stream = File.OpenRead(path))
                {
                    local = JsonSerializer.Deserialize<T>(stream);
                }
                return local;
            }
            catch (Exception ex)
            {
                NotificationHelper.SendErrorMsg(ex.ToString(), "Error in ReadJsonFile<T>");
                return default;
            }
        }

        /// <summary>
        /// Asynchronously reads the content of a JSON file and deserializes it to the specified type.
        /// </summary>
        /// <typeparam name="T">The type to deserialize the JSON content to.</typeparam>
        /// <param name="path">The path to the JSON file.</param>
        /// <returns>
        /// A <see cref="Task{TResult}"/> representing the asynchronous operation. The task result contains the deserialized object of type <typeparamref name="T"/>, or null if the file does not exist or deserialization fails.
        /// </returns>
        public static async Task<T?> ReadJsonFileAsync<T>(string path)
        {
            try
            {
                T? local = default;
                using (var stream = File.OpenRead(path))
                {
                    local = await JsonSerializer.DeserializeAsync<T>(stream);
                }
                return local;
            }
            catch (Exception ex)
            {
                NotificationHelper.SendErrorMsg(ex.ToString(), "Error in ReadJsonFileAsync<T>");
                return default;
            }
        }
    }
}
