using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace KonkordLibrary.Models.Minecraft.Meta
{
    public class MCLoggingClient
    {
        [JsonPropertyName("argument")]
        public string Argument { get; set; }
        [JsonPropertyName("file")]
        public MCLoggingFile File { get; set; }
        [JsonPropertyName("type")]
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
