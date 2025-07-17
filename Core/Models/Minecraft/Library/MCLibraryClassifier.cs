using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace Tavstal.KonkordLauncher.Core.Models.Minecraft.Library
{
    public class MCLibraryClassifier
    {
        [JsonPropertyName("natives-windows"), JsonProperty("natives-windows")]
        public MCLibraryArtifact WindowsNatives {  get; set; }
        [JsonPropertyName("natives-osx"), JsonProperty("natives-osx")]
        public MCLibraryArtifact OsxNatives { get; set; }
        [JsonPropertyName("natives-linux"), JsonProperty("natives-linux")]
        public MCLibraryArtifact LinuxNatives { get; set; }

        public MCLibraryClassifier() { }

        public MCLibraryClassifier(MCLibraryArtifact windowsNatives, MCLibraryArtifact osxNatives, MCLibraryArtifact linuxNatives)
        {
            WindowsNatives = windowsNatives;
            OsxNatives = osxNatives;
            LinuxNatives = linuxNatives;
        }
    }
}
