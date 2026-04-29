using System.Text.Json.Serialization;
using Newtonsoft.Json;
using Tavstal.KonkordLauncher.Core.Enums;

namespace Tavstal.KonkordLauncher.Core.Models.ModLoaders.Forge;

/// <summary>
/// Represents the manifest for a Forge mod loader, containing details about the version
/// and the associated game version.
/// </summary>
public class ForgeManifest : IModManifest
{
    /// <inheritdoc/>
    [JsonProperty("gameVersion"), JsonPropertyName("gameVersion")]
    public string GameVersion { get; set; }

    /// <inheritdoc/>
    [JsonProperty("version"), JsonPropertyName("version")]
    public string Version { get; set; }
    
    /// <inheritdoc/>
    [System.Text.Json.Serialization.JsonIgnore, Newtonsoft.Json.JsonIgnore]
    public EMinecraftKind LoaderKind { get;  } = EMinecraftKind.FORGE;
    
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