using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace Tavstal.KonkordLauncher.Core.Models.Instances.CurseForge
{
    public class CurseModLoader
    {
        [JsonProperty("id"), JsonPropertyName("id")]
        public string Id { get; set; }
        [JsonProperty("primary"), JsonPropertyName("primary")]
        public bool IsPrimary { get; set; }

        public CurseModLoader() { }

        public CurseModLoader(string id, bool isPrimary)
        {
            Id = id;
            IsPrimary = isPrimary;
        }
    }
}
