
using System.Text.Json.Serialization;


namespace Tavstal.KonkordLauncher.Core.Models.MojangApi.User;

/// <summary>
/// Represents a Mojang profile for an authenticated user, including the user's UUID,
/// display name, available skins, and capes.
/// </summary>
public class MojangProfile
{
    /// <summary>
    /// Gets or sets the unique identifier of the Mojang profile.
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; }

    /// <summary>
    /// Gets or sets the player's display name.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the list of skins associated with this profile.
    /// </summary>
    [JsonPropertyName("skins")]
    public List<Skin> Skins {  get; set; }

    /// <summary>
    /// Gets or sets the list of capes associated with this profile.
    /// </summary>
    [JsonPropertyName("capes")]
    public List<Cape> Capes { get; set; }

    /// <summary>
    /// Initializes a new, empty instance of the <see cref="MojangProfile"/> class.
    /// </summary>
    public MojangProfile() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="MojangProfile"/> class with the specified values.
    /// </summary>
    /// <param name="id">The Mojang profile identifier.</param>
    /// <param name="name">The player's display name.</param>
    /// <param name="skins">The list of skins associated with the profile.</param>
    /// <param name="capes">The list of capes associated with the profile.</param>
    public MojangProfile(string id, string name, List<Skin> skins, List<Cape> capes)
    {
        Id = id;
        Name = name;
        Skins = skins;
        Capes = capes;
    }
}