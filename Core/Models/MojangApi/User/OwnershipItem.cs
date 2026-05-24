
using Newtonsoft.Json;

namespace Tavstal.KonkordLauncher.Core.Models.MojangApi.User;

public class OwnershipItem
{
    [JsonProperty("name")]
    public string Name { get; set; }
    [JsonProperty("signature")]
    public string Signature { get; set; }

    public OwnershipItem() { }

    public OwnershipItem(string name, string signature) 
    { 
        Name = name;
        Signature = signature;
    }
}