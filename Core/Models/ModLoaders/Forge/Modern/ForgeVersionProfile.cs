
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
    [JsonProperty("spec")]
    public int Spec { get; set; }

    /// <summary>
    /// Gets or sets the profile name of the Forge version.
    /// </summary>
    [JsonProperty("profile")]
    public string Profile { get; set; }

    /// <summary>
    /// Gets or sets the version of the Forge mod loader.
    /// </summary>
    [JsonProperty("version")]
    public string Version { get; set; }

    /// <summary>
    /// Gets or sets the optional path for the Forge version profile.
    /// </summary>
    [JsonProperty("path")]
    public string? Path { get; set; }

    /// <summary>
    /// Gets or sets the version of Minecraft associated with this Forge profile.
    /// </summary>
    [JsonProperty("minecraft")]
    public string Minecraft { get; set; }

    /// <summary>
    /// Gets or sets the path to the server JAR file for the Forge version.
    /// </summary>
    [JsonProperty("serverJarPath")]
    public string ServerJarPath { get; set; }

    /// <summary>
    /// Gets or sets additional data associated with the Forge version profile.
    /// </summary>
    [JsonProperty("data")]
    public JObject? Data { get; set; }

    /// <summary>
    /// Gets or sets the list of processors required by the Forge version.
    /// </summary>
    [JsonProperty("processors")]
    public JArray Processors { get; set; }

    /// <summary>
    /// Gets or sets the list of libraries required by the Forge version.
    /// </summary>
    [JsonProperty("libraries")]
    public List<LibraryMeta> Libraries { get; set; }
}