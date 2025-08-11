using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace Tavstal.KonkordLauncher.Core.Models.ModLoaders.Forge.Legacy;

/// <summary>
/// Represents a Forge profile containing installation details, version metadata, and optional settings.
/// </summary>
public class ForgeProfile
{
    /// <summary>
    /// Gets or sets the installation information for the Forge profile.
    /// </summary>
    [JsonPropertyName("install"), JsonProperty("install")]
    public ForgeProfileInfo Install { get; set; }

    /// <summary>
    /// Gets or sets the version metadata for the Forge profile.
    /// </summary>
    [JsonPropertyName("versionInfo"), JsonProperty("versionInfo")]
    public ForgeVersionMeta VersionInfo { get; set; }

    /// <summary>
    /// Gets or sets the optional settings for the Forge profile.
    /// </summary>
    [JsonPropertyName("optionals"), JsonProperty("optionals")]
    public object Optionals { get; set; }
}