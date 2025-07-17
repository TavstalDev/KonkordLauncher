using System.Text.Json.Serialization;
using Newtonsoft.Json;
using Tavstal.KonkordLauncher.Core.Models.Minecraft.Library;
using Tavstal.KonkordLauncher.Core.Models.Minecraft.Meta;

namespace Tavstal.KonkordLauncher.Core.Models.Minecraft
{
    public class MCVersionMeta
    {
        [JsonPropertyName("arguments"), JsonProperty("arguments")]
        public MCMetaArgument? ArgumentsNew { get; set; }
        [JsonPropertyName("minecraftArguments"), JsonProperty("minecraftArguments")]
        public string? ArgumentsLegacy { get; set; }

        [JsonPropertyName("assetIndex"), JsonProperty("assetIndex")]
        public MCMetaAsset AssetIndex { get; set; }

        [JsonPropertyName("assets"), JsonProperty("assets")]
        public string Assets { get; set; }

        [JsonPropertyName("complianceLevel"), JsonProperty("complianceLevel")]
        public int ComplianceLevel { get; set; }

        [JsonPropertyName("downloads"), JsonProperty("downloads")]
        public MCMetaDownloads Downloads { get; set; }

        [JsonPropertyName("id"), JsonProperty("id")]
        public string Id {  get; set; }

        [JsonPropertyName("javaVersion"), JsonProperty("javaVersion")]
        public JavaVersion JavaVersion { get; set; }
        [JsonPropertyName("libraries"), JsonProperty("libraries")]
        public List<MCLibrary> Libraries { get; set; }
        [JsonPropertyName("logging"), JsonProperty("logging")]
        public MCLogging Logging {  get; set; }
        [JsonPropertyName("mainClass"), JsonProperty("mainClass")]
        public string MainClass { get; set; }
        [JsonPropertyName("type"), JsonProperty("type")]
        public string Type { get; set; }


        public List<string> GetGameArguments()
        {
            if (ArgumentsNew != null)
                return ArgumentsNew.GetGameArgs();
            else if (ArgumentsLegacy != null)
                return ArgumentsLegacy.Split(' ').ToList();
            else
                throw new Exception("Failed to get the game arguments");
        }

        public string GetGameArgumentString()
        {
            if (ArgumentsNew != null)
                return ArgumentsNew.GetGameArgString();
            else if (ArgumentsLegacy != null)
                return ArgumentsLegacy;
            else
                throw new Exception("Failed to get the game arguments");
        }

        public List<string> GetJVMArguments()
        {
            if (ArgumentsNew != null)
                return ArgumentsNew.GetJVMArgs();
            else if (ArgumentsLegacy != null)
                return new List<string>() { 
                    "-Djava.library.path=${natives_directory}",
                    "-Dminecraft.launcher.brand=${launcher_name}",
                    "-Dminecraft.launcher.version=${launcher_version}",
                    "-cp ${classpath}" 
                }; // not provided, adding defaults
            else
                throw new Exception("Failed to get the game arguments");
        }

        public string GetJVMArgumentString()
        {
            if (ArgumentsNew != null)
                return ArgumentsNew.GetJVMArgString();
            else if (ArgumentsLegacy != null)
                return "-Djava.library.path=${natives_directory} -Dminecraft.launcher.brand=${launcher_name} -Dminecraft.launcher.version=${launcher_version} -cp ${classpath}";
            else
                throw new Exception("Failed to get the game arguments");
        }
    }
}
