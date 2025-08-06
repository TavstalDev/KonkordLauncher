using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace Tavstal.KonkordLauncher.Core.Models.ModLoaders.Forge;

/// <summary>
/// Represents the manifest for a Forge mod loader, containing details about the version
/// and the associated game version.
/// </summary>
public class ForgeManifest : IModManifest
{
    /// <summary>
    /// Gets or sets the version of the Forge mod loader.
    /// </summary>
    [JsonProperty("version"), JsonPropertyName("version")]
    public string Version { get; set; }

    /// <summary>
    /// Gets or sets the game version associated with this Forge manifest.
    /// </summary>
    [JsonProperty("gameVersion"), JsonPropertyName("gameVersion")]
    public string GameVersion { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ForgeManifest"/> class with default values.
    /// </summary>
    public ForgeManifest() {}

    /// <summary>
    /// Initializes a new instance of the <see cref="ForgeManifest"/> class with specified values.
    /// </summary>
    /// <param name="version">The version of the Forge mod loader.</param>
    /// <param name="gameVersion">The game version associated with this Forge manifest.</param>
    public ForgeManifest(string version, string gameVersion)
    {
        Version = version;
        GameVersion = gameVersion;
    }
}