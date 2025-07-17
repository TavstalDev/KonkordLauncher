using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace Tavstal.KonkordLauncher.Core.Models.Minecraft.API
{
    public class OwnershipData
    {
        [JsonProperty("keyId"), JsonPropertyName("keyId")]
        public string KeyId { get; set; }
        [JsonProperty("signature"), JsonPropertyName("signature")]
        public string Signature { get; set; }
        [JsonProperty("items"), JsonPropertyName("items")]
        public List<OwnershipItem> Items {  get; set; }

        public OwnershipData() { }

        public OwnershipData(string keyId, string signature, List<OwnershipItem> items)
        {
            KeyId = keyId;
            Signature = signature;
            Items = items;
        }
    }
}
