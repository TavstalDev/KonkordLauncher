
using Newtonsoft.Json;

namespace Tavstal.KonkordLauncher.Core.Models.MojangApi.User;

/// <summary>
/// Represents Mojang ownership data, including the public key identifier, signature,
/// and the collection of owned entitlement items.
/// </summary>
public class OwnershipData
{
    /// <summary>
    /// Gets or sets the key identifier associated with this ownership payload.
    /// </summary>
    [JsonProperty("keyId")]
    public string KeyId { get; set; }

    /// <summary>
    /// Gets or sets the signature that validates the ownership data.
    /// </summary>
    [JsonProperty("signature")]
    public string Signature { get; set; }

    /// <summary>
    /// Gets or sets the collection of ownership items contained in this payload.
    /// </summary>
    [JsonProperty("items")]
    public List<OwnershipItem> Items {  get; set; }

    /// <summary>
    /// Initializes a new, empty instance of the <see cref="OwnershipData"/> class.
    /// </summary>
    public OwnershipData() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="OwnershipData"/> class with the specified values.
    /// </summary>
    /// <param name="keyId">The key identifier for the ownership payload.</param>
    /// <param name="signature">The signature associated with the payload.</param>
    /// <param name="items">The ownership items included in the payload.</param>
    public OwnershipData(string keyId, string signature, List<OwnershipItem> items)
    {
        KeyId = keyId;
        Signature = signature;
        Items = items;
    }
}