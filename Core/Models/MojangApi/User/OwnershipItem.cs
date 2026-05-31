
using Newtonsoft.Json;

namespace Tavstal.KonkordLauncher.Core.Models.MojangApi.User;

/// <summary>
/// Represents an ownership record returned by the Mojang API, typically identifying
/// a purchased product and its associated signature.
/// </summary>
public class OwnershipItem
{
    /// <summary>
    /// Gets or sets the name of the owned product or entitlement.
    /// </summary>
    [JsonProperty("name")]
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the signature associated with this ownership record.
    /// </summary>
    [JsonProperty("signature")]
    public string Signature { get; set; }

    /// <summary>
    /// Initializes a new, empty instance of the <see cref="OwnershipItem"/> class.
    /// </summary>
    public OwnershipItem() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="OwnershipItem"/> class with the specified values.
    /// </summary>
    /// <param name="name">The ownership item name.</param>
    /// <param name="signature">The signature associated with the ownership item.</param>
    public OwnershipItem(string name, string signature) 
    { 
        Name = name;
        Signature = signature;
    }
}