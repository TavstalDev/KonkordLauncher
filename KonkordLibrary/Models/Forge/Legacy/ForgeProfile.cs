using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace Tavstal.KonkordLibrary.Models.Forge.Legacy
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
