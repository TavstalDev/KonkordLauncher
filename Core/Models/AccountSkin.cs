using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace Tavstal.KonkordLauncher.Core.Models;

/// <summary>
/// Represents an account skin with associated properties such as ID, model, cape ID, and Mojang ID.
/// </summary>
public class AccountSkin
{
    /// <summary>
    /// Gets or sets the unique identifier for the account skin.
    /// </summary>
    [JsonPropertyName("id"), JsonProperty("id")]
    public string Id { get; set; }
    
    /// <summary>
    /// Gets or sets the model type of the account skin.
    /// </summary>
    [JsonPropertyName("model"), JsonProperty("model")]
    public string Model { get; set; }
    
    /// <summary>
    /// Gets or sets the optional cape identifier for the account skin.
    /// </summary>
    [JsonPropertyName("capeId"), JsonProperty("capeId")]
    public string? CapeId { get; set; }
    
    /// <summary>
    /// Gets or sets the optional Mojang identifier for the account skin.
    /// </summary>
    [JsonPropertyName("mojangId"), JsonProperty("mojangId")]
    public string? MojangId { get; set; }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="AccountSkin"/> class.
    /// </summary>
    public AccountSkin() {}
    
    /// <summary>
    /// Initializes a new instance of the <see cref="AccountSkin"/> class with specified properties.
    /// </summary>
    /// <param name="id">The unique identifier for the account skin.</param>
    /// <param name="model">The model type of the account skin.</param>
    /// <param name="capeId">The optional cape identifier for the account skin.</param>
    /// <param name="mojangId">The optional Mojang identifier for the account skin.</param>
    public AccountSkin(string id, string model, string? capeId = null, string? mojangId = null)
    {
        Id = id;
        Model = model;
        CapeId = capeId;
        MojangId = mojangId;
    }
}
