
using System.Text.Json.Serialization;


namespace Tavstal.KonkordLauncher.Core.Models.ModLoaders.Forge.Legacy;

/// <summary>
/// Represents detailed information about a Forge profile, including its name, target, and associated paths.
/// </summary>
public class ForgeProfileInfo
{
    /// <summary>
    /// Gets or sets the name of the Forge profile.
    /// </summary>
    [JsonPropertyName("profileName")]
    public string ProfileName { get; set; }

    /// <summary>
    /// Gets or sets the target of the Forge profile.
    /// </summary>
    [JsonPropertyName("target")]
    public string Target { get; set; }

    /// <summary>
    /// Gets or sets the path associated with the Forge profile.
    /// </summary>
    [JsonPropertyName("path")]
    public string Path { get; set; }

    /// <summary>
    /// Gets or sets the version of the Forge profile.
    /// </summary>
    [JsonPropertyName("version")]
    public string Version { get; set; }

    /// <summary>
    /// Gets or sets the file path of the Forge profile.
    /// </summary>
    [JsonPropertyName("filePath")]
    public string FilePath { get; set; }

    /// <summary>
    /// Gets or sets the version of Minecraft associated with the Forge profile.
    /// </summary>
    [JsonPropertyName("minecraft")]
    public string Minecraft { get; set; }

    /// <summary>
    /// Gets or sets the mirror list URL for the Forge profile.
    /// </summary>
    [JsonPropertyName("mirrorList")]
    public string MirrorList { get; set; }
}