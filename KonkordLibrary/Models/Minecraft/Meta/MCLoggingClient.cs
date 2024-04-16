using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace Tavstal.KonkordLibrary.Models.Minecraft.Meta
{
    public class MCLoggingClient
    {
        [JsonPropertyName("argument"), JsonProperty("argument")]
        public string Argument { get; set; }
        [JsonPropertyName("file"), JsonProperty("file")]
        public MCLoggingFile File { get; set; }
        [JsonPropertyName("type"), JsonProperty("type")]
        public string Type { get; set; }

        public MCLoggingClient() { }
        public MCLoggingClient(string argument, MCLoggingFile file, string type)
        {
            Argument = argument;
            File = file;
            Type = type;
        }
    }
}
