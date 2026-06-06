
using System.Text.Json.Serialization;

using Tavstal.KonkordLauncher.Core.Enums;

namespace Tavstal.KonkordLauncher.Core.Models.ModLoaders.Fabric;

/// <summary>
/// Represents the manifest for a Fabric mod loader, containing details about the game version
/// and the mod loader version.
/// </summary>
public class FabricManifest : IModManifest
{
    /// <inheritdoc/>
    [JsonPropertyName("gameVersion")]
    public string GameVersion { get; set; }

    /// <inheritdoc/>
    [JsonPropertyName("version")]
    public string Version { get; set; }
    
    /// <inheritdoc/>
    [JsonIgnore]
    public EMinecraftKind LoaderKind { get;  } = EMinecraftKind.FABRIC;

    /// <summary>
    /// Initializes a new instance of the <see cref="FabricManifest"/> class with default values.
    /// </summary>
    public FabricManifest() {}

    /// <summary>
    /// Initializes a new instance of the <see cref="FabricManifest"/> class with a specified version.
    /// </summary>
    /// <param name="version">The version of the Fabric mod loader.</param>
    public FabricManifest(string version)
    {
        GameVersion = string.Empty;
        Version = version;
    }
}