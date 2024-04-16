using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace Tavstal.KonkordLibrary.Models.Minecraft.API
{
    public class OwnershipItem
    {
        [JsonProperty("name"), JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonProperty("signature"), JsonPropertyName("signature")]
        public string Signature { get; set; }

        public OwnershipItem() { }

        public OwnershipItem(string name, string signature) 
        { 
            Name = name;
            Signature = signature;
        }
    }
}
