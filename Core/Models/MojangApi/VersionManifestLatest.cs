
using System.Text.Json.Serialization;


namespace Tavstal.KonkordLauncher.Core.Models.MojangApi;

/// <summary>
/// Represents the latest release and snapshot versions from the Mojang version manifest.
/// </summary>
public class VersionManifestLatest
{
    /// <summary>
    /// Gets or sets the latest stable release version.
    /// </summary>
    [JsonPropertyName("release")]
    public string Release { get; set; }

    /// <summary>
    /// Gets or sets the latest snapshot version.
    /// </summary>
    [JsonPropertyName("snapshot")]
    public string Snapshot { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="VersionManifestLatest"/> class
    /// with empty release and snapshot values.
    /// </summary>
    public VersionManifestLatest()
    {
        Release = string.Empty;
        Snapshot = string.Empty;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="VersionManifestLatest"/> class
    /// with the specified release and snapshot versions.
    /// </summary>
    /// <param name="release">The latest stable release version.</param>
    /// <param name="snapshot">The latest snapshot version.</param>
    public VersionManifestLatest(string release, string snapshot)
    {
        Release = release;
        Snapshot = snapshot;
    }
}