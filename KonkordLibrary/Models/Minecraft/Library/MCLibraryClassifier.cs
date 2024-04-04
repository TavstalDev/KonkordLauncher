using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace KonkordLibrary.Models.Minecraft.Library
{
    public class MCLibraryClassifier
    {
        [JsonProperty("natives-windows")]
        public MCLibraryArtifact WindowsNatives {  get; set; }
        [JsonProperty("natives-osx")]
        public MCLibraryArtifact OsxNatives { get; set; }
        [JsonProperty("natives-linux")]
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
