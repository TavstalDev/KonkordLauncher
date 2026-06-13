
using System.Text.Json;
using System.Text.Json.Serialization;
using Tavstal.KonkordLauncher.Core.Models.MojangApi.Meta;

namespace Tavstal.KonkordLauncher.Core.Models.ModLoaders.Forge.Modern;

/// <summary>
/// Represents the profile for a specific version of the Forge mod loader.
/// </summary>
public class ForgeVersionProfile
{
    /// <summary>
    /// Gets or sets the specification version of the Forge profile.
    /// </summary>
    [JsonPropertyName("spec")]
    public int Spec { get; set; }

    /// <summary>
    /// Gets or sets the profile name of the Forge version.
    /// </summary>
    [JsonPropertyName("profile")]
    public string Profile { get; set; }

    /// <summary>
    /// Gets or sets the version of the Forge mod loader.
    /// </summary>
    [JsonPropertyName("version")]
    public string Version { get; set; }

    /// <summary>
    /// Gets or sets the optional path for the Forge version profile.
    /// </summary>
    [JsonPropertyName("path")]
    public string? Path { get; set; }

    /// <summary>
    /// Gets or sets the version of Minecraft associated with this Forge profile.
    /// </summary>
    [JsonPropertyName("minecraft")]
    public string Minecraft { get; set; }

    /// <summary>
    /// Gets or sets the path to the server JAR file for the Forge version.
    /// </summary>
    [JsonPropertyName("serverJarPath")]
    public string ServerJarPath { get; set; }

    /// <summary>
    /// Gets or sets additional data associated with the Forge version profile.
    /// </summary>
    [JsonPropertyName("data")]
    public JsonElement? Data { get; set; }

    /// <summary>
    /// Gets or sets the list of processors required by the Forge version.
    /// </summary>
    [JsonPropertyName("processors")]
    public JsonElement Processors { get; set; }

    /// <summary>
    /// Gets or sets the list of libraries required by the Forge version.
    /// </summary>
    [JsonPropertyName("libraries")]
    public List<LibraryMeta> Libraries { get; set; }
}