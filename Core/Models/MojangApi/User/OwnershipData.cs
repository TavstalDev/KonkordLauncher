
using Newtonsoft.Json;

namespace Tavstal.KonkordLauncher.Core.Models.MojangApi.User;

public class OwnershipData
{
    [JsonProperty("keyId")]
    public string KeyId { get; set; }
    [JsonProperty("signature")]
    public string Signature { get; set; }
    [JsonProperty("items")]
    public List<OwnershipItem> Items {  get; set; }

    public OwnershipData() { }

    public OwnershipData(string keyId, string signature, List<OwnershipItem> items)
    {
        KeyId = keyId;
        Signature = signature;
        Items = items;
    }
}