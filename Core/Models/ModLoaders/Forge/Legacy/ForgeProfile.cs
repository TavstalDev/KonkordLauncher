using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace Tavstal.KonkordLauncher.Core.Models.ModLoaders.Forge.Legacy
{
    public class ForgeProfile
    {
        [JsonPropertyName("install"), JsonProperty("install")]
        public ForgeProfileInfo Install { get; set; }
        [JsonPropertyName("versionInfo"), JsonProperty("versionInfo")]
        public ForgeVersionMeta VersionInfo { get; set; }
        [JsonPropertyName("optionals"), JsonProperty("optionals")]
        public object Optionals { get; set; }
    }
}
