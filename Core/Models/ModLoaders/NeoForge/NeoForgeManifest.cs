
using System.Text.Json.Serialization;

using Tavstal.KonkordLauncher.Core.Enums;

namespace Tavstal.KonkordLauncher.Core.Models.ModLoaders.NeoForge;

/// <summary>
/// Represents the manifest for a NeoForge mod loader, containing details about the version
/// and the associated game version.
/// </summary>
public class NeoForgeManifest : IModManifest
{
    /// <inheritdoc/>
    [JsonPropertyName("gameVersion")]
    public string GameVersion { get; set; }

    /// <inheritdoc/>
    [JsonPropertyName("version")]
    public string Version { get; set; }
    
    /// <inheritdoc/>
    [JsonIgnore]
    public EMinecraftKind LoaderKind { get; } = EMinecraftKind.NEOFORGE;

    /// <inheritdoc/>
    public bool EqualsGameVersion(string gameVersion)
    {
        // Check if minecraft is modern
        if (gameVersion.StartsWith("1."))
            return gameVersion == GameVersion;
        
        if (gameVersion.Split('.').Length == 2)
            return Version.StartsWith(gameVersion + ".0");
        return Version.StartsWith(gameVersion);
    }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="NeoForgeManifest"/> class with default values.
    /// </summary>
    public NeoForgeManifest() {}

    /// <summary>
    /// Initializes a new instance of the <see cref="NeoForgeManifest"/> class with specified values.
    /// </summary>
    /// <param name="version">The version of the NeoForge mod loader.</param>
    /// <param name="gameVersion">The game version associated with this NeoForge manifest.</param>
    public NeoForgeManifest(string version, string gameVersion)
    {
        Version = version;
        GameVersion = gameVersion;
    }
}