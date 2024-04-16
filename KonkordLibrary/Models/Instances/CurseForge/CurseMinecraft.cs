using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace Tavstal.KonkordLibrary.Models.Instances.CurseForge
{
    public class CurseMinecraft
    {
        [JsonProperty("version"), JsonPropertyName("version")]
        public string Version { get; set; }
        [JsonProperty("modLoaders"), JsonPropertyName("modLoaders")]
        public List<CurseModLoader> ModLoaders { get; set; }

        public CurseMinecraft() { }

        public CurseMinecraft(string version, List<CurseModLoader> modLoaders)
        {
            Version = version;
            ModLoaders = modLoaders;
        }
    }
}
