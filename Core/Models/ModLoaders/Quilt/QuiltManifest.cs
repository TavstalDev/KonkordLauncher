
using Newtonsoft.Json;
using Tavstal.KonkordLauncher.Core.Enums;

namespace Tavstal.KonkordLauncher.Core.Models.ModLoaders.Quilt;

/// <summary>
/// Represents the manifest for a Quilt mod loader, containing details about the game version
/// and the mod loader version.
/// </summary>
public class QuiltManifest : IModManifest
{
    /// <inheritdoc/>
    [JsonProperty("gameVersion")]
    public string GameVersion { get; set; }

    /// <inheritdoc/>
    [JsonProperty("version")]
    public string Version { get; set; }
    
    /// <inheritdoc/>
    [JsonIgnore]
    public EMinecraftKind LoaderKind { get;  } = EMinecraftKind.QUILT;

    /// <summary>
    /// Initializes a new instance of the <see cref="QuiltManifest"/> class with default values.
    /// </summary>
    public QuiltManifest() {}

    /// <summary>
    /// Initializes a new instance of the <see cref="QuiltManifest"/> class with a specified version.
    /// </summary>
    /// <param name="version">The version of the Quilt mod loader.</param>
    public QuiltManifest(string version)
    {
        GameVersion = string.Empty;
        Version = version;
    }
}