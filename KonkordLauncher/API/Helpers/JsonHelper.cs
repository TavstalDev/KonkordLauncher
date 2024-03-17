using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Transactions;

namespace KonkordLauncher.API.Helpers
{
    public static class JsonHelper
    {
        public static async Task<bool> WriteJsonFile<T>(string path, T obj)
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
                NotificationHelper.SendError(ex.ToString(), "Error in WriteJsonFile<T>");
                return false;
            }
        }

        public static async Task<T?> ReadJsonFile<T>(string path)
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
                NotificationHelper.SendError(ex.ToString(), "Error in ReadJsonFile<T>");
                return default;
            }
        }
    }
}
