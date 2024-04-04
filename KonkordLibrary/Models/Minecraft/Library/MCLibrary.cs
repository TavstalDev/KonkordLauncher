using System.IO;
using System.Runtime.InteropServices;
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
        [JsonPropertyName("natives")]
        public MCLibraryNatives? Natives { get; set; }

        public MCLibrary() { }

        public MCLibrary(string name, MCLibraryDownloads downloads, List<MCLibraryRule> rules)
        {
            Name = name;
            Downloads = downloads;
            Rules = rules;
        }

        public bool GetRulesResult()
        {
            bool localResult = false;

            if (Rules == null)
                return true;

            if (Rules.Count == 0)
                return true;

            foreach (MCLibraryRule rule in Rules)
            {
                if (rule.OS == null)
                {
                    localResult = rule.Action == "allow";
                    continue;
                }

                if (rule.OS.Name == "windows" && RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    localResult = rule.Action == "allow";
                    continue;
                }

                if (rule.OS.Name == "linux" && RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    localResult = rule.Action == "allow";
                    continue;
                }

                if (rule.OS.Name == "osx" && RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    localResult = rule.Action == "allow";
                    continue;
                }
            }

            return localResult;
        }
    }
}
