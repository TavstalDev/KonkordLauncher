using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace KonkordLibrary.Models.Minecraft.Library
{
    public class MCLibraryNatives
    {
        [JsonProperty("windows")]
        public string Windows {  get; set; }
        [JsonProperty("osx")]
        public string Osx { get; set; }
        [JsonProperty("linux")]
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
