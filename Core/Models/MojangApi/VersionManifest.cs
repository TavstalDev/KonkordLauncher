
using System.Text.Json.Serialization;


namespace Tavstal.KonkordLauncher.Core.Models.MojangApi;

/// <summary>
/// Represents the Mojang version manifest, including the latest release and snapshot versions,
/// as well as the full list of available Minecraft versions.
/// </summary>
public class VersionManifest
{
    /// <summary>
    /// Gets or sets the latest release and snapshot version information.
    /// </summary>
    [JsonPropertyName("latest")]
    public VersionManifestLatest Latest {  get; set; }

    /// <summary>
    /// Gets or sets the list of available Minecraft versions.
    /// </summary>
    [JsonPropertyName("versions")]
    public List<MinecraftVersion> Versions { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="VersionManifest"/> class
    /// with empty default values.
    /// </summary>
    public VersionManifest()
    {
        Latest = new VersionManifestLatest();
        Versions = [];
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="VersionManifest"/> class
    /// with the specified latest version information and version list.
    /// </summary>
    /// <param name="latest">The latest release and snapshot version information.</param>
    /// <param name="versions">The collection of available Minecraft versions.</param>
    public VersionManifest(VersionManifestLatest latest, List<MinecraftVersion> versions)
    {
        Latest = latest;
        Versions = versions;
    }
}