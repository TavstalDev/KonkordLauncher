using System.IO;
using System.Security.Policy;
using System.Text.Json.Serialization;

namespace KonkordLibrary.Models.Minecraft.Library
{
    public class MCLibrary
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("downloads")]
        public MCLibraryDownloads Downloads { get; set; }
        [JsonPropertyName("rules")]
        public List<MCLibraryRule> Rules { get; set; }

        public MCLibrary() { }

        public MCLibrary(string name, MCLibraryDownloads downloads, List<MCLibraryRule> rules)
        {
            Name = name;
            Downloads = downloads;
            Rules = rules;
        }
    }
}
