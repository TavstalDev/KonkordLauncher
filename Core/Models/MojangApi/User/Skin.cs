
using Newtonsoft.Json;

namespace Tavstal.KonkordLauncher.Core.Models.MojangApi.User;

/// <summary>
/// Represents a Minecraft skin entry from the Mojang API, including its identifier,
/// state, texture URL, variant, and optional alias.
/// </summary>
public class Skin
{
    /// <summary>
    /// Gets or sets the unique identifier of the skin.
    /// </summary>
    [JsonProperty("id")]
    public string Id { get; set; }

    /// <summary>
    /// Gets or sets the current state of the skin, such as active or inactive.
    /// </summary>
    [JsonProperty("state")]
    public string State { get; set; }

    /// <summary>
    /// Gets or sets the URL where the skin texture can be downloaded.
    /// </summary>
    [JsonProperty("url")]
    public string Url { get; set; }

    /// <summary>
    /// Gets or sets the skin variant, such as classic or slim.
    /// </summary>
    [JsonProperty("variant")]
    public string Variant { get; set; }

    /// <summary>
    /// Gets or sets the optional alias associated with the skin.
    /// </summary>
    [JsonProperty("alias")]
    public string? Alias { get; set; }

    /// <summary>
    /// Initializes a new, empty instance of the <see cref="Skin"/> class.
    /// </summary>
    public Skin() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="Skin"/> class with the specified values.
    /// </summary>
    /// <param name="id">The skin identifier.</param>
    /// <param name="state">The state of the skin.</param>
    /// <param name="url">The URL of the skin texture.</param>
    /// <param name="variant">The skin variant.</param>
    /// <param name="alias">The optional alias of the skin.</param>
    public Skin(string id, string state, string url, string variant, string? alias)
    {
        Id = id;
        State = state;
        Url = url;
        Variant = variant;
        Alias = alias;
    }
}