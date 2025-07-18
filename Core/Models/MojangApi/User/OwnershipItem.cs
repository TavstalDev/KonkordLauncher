using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace Tavstal.KonkordLauncher.Core.Models.MojangApi.User
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
