using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace KonkordLibrary.Models.Minecraft.Library
{
    public class MCLibraryNatives
    {
        [JsonPropertyName("windows"), JsonProperty("windows")]
        public string Windows {  get; set; }
        [JsonPropertyName("osx"), JsonProperty("osx")]
        public string Osx { get; set; }
        [JsonPropertyName("linux"), JsonProperty("linux")]
        public string Linux { get; set; }

        public MCLibraryNatives() { }

        public MCLibraryNatives(string windows, string osx, string linux)
        {
            Windows = windows;
            Osx = osx;
            Linux = linux;
        }
    }
}
