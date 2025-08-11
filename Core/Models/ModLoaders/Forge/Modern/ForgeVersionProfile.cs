using System.Text.Json.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Tavstal.KonkordLauncher.Core.Models.MojangApi.Meta;

namespace Tavstal.KonkordLauncher.Core.Models.ModLoaders.Forge.New;

/// <summary>
/// Represents the profile for a specific version of the Forge mod loader.
/// </summary>
public class ForgeVersionProfile
{
    /// <summary>
    /// Gets or sets the specification version of the Forge profile.
    /// </summary>
    [JsonPropertyName("spec"), JsonProperty("spec")]
    public int Spec { get; set; }

    /// <summary>
    /// Gets or sets the profile name of the Forge version.
    /// </summary>
    [JsonPropertyName("profile"), JsonProperty("profile")]
    public string Profile { get; set; }

    /// <summary>
    /// Gets or sets the version of the Forge mod loader.
    /// </summary>
    [JsonPropertyName("version"), JsonProperty("version")]
    public string Version { get; set; }

    /// <summary>
    /// Gets or sets the optional path for the Forge version profile.
    /// </summary>
    [JsonPropertyName("path"), JsonProperty("path")]
    public string? Path { get; set; }

    /// <summary>
    /// Gets or sets the version of Minecraft associated with this Forge profile.
    /// </summary>
    [JsonPropertyName("minecraft"), JsonProperty("minecraft")]
    public string Minecraft { get; set; }

    /// <summary>
    /// Gets or sets the path to the server JAR file for the Forge version.
    /// </summary>
    [JsonPropertyName("serverJarPath"), JsonProperty("serverJarPath")]
    public string ServerJarPath { get; set; }

    /// <summary>
    /// Gets or sets additional data associated with the Forge version profile.
    /// </summary>
    [JsonPropertyName("data"), JsonProperty("data")]
    public JObject? Data { get; set; }

    /// <summary>
    /// Gets or sets the list of processors required by the Forge version.
    /// </summary>
    [JsonPropertyName("processors"), JsonProperty("processors")]
    public JArray Processors { get; set; }

    /// <summary>
    /// Gets or sets the list of libraries required by the Forge version.
    /// </summary>
    [JsonPropertyName("libraries"), JsonProperty("libraries")]
    public List<LibraryMeta> Libraries { get; set; }
}