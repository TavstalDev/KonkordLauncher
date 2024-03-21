using System.Text;
using System.Text.Json;
using System.IO;

namespace KonkordLibrary.Helpers
{
    public static class JsonHelper
    {
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
                NotificationHelper.SendError(ex.ToString(), "Error in WriteJsonFile<T>");
                return false;
            }
        }

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
                NotificationHelper.SendError(ex.ToString(), "Error in WriteJsonFileAsync<T>");
                return false;
            }
        }

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
                NotificationHelper.SendError(ex.ToString(), "Error in ReadJsonFile<T>");
                return default;
            }
        }

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
                NotificationHelper.SendError(ex.ToString(), "Error in ReadJsonFileAsync<T>");
                return default;
            }
        }
    }
}
