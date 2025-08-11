using System.Text.Json.Serialization;
using Newtonsoft.Json;
using Tavstal.KonkordLauncher.Core.Models.MojangApi.Meta;

namespace Tavstal.KonkordLauncher.Core.Models.ModLoaders.Forge.Legacy;

/// <summary>
/// Represents metadata for a specific version of the Forge mod loader.
/// </summary>
public class ForgeVersionMeta
{
    /// <summary>
    /// Gets or sets the unique identifier of the Forge version.
    /// </summary>
    [JsonPropertyName("id"), JsonProperty("id")]
    public string Id { get; set; }

    /// <summary>
    /// Gets or sets the type of the Forge version (e.g., "release", "snapshot").
    /// </summary>
    [JsonPropertyName("type"), JsonProperty("type")]
    public string Type { get; set; }

    /// <summary>
    /// Gets or sets the Minecraft arguments for the Forge version.
    /// </summary>
    [JsonPropertyName("minecraftArguments"), JsonProperty("minecraftArguments")]
    public string MinecraftArguments { get; set; }

    /// <summary>
    /// Gets or sets the main class to be executed for the Forge version.
    /// </summary>
    [JsonPropertyName("mainClass"), JsonProperty("mainClass")]
    public string MainClass { get; set; }

    /// <summary>
    /// Gets or sets the version of Minecraft that this Forge version inherits from.
    /// </summary>
    [JsonPropertyName("inheritsFrom"), JsonProperty("inheritsFrom")]
    public string InheritsFrom { get; set; }

    /// <summary>
    /// Gets or sets the JAR file name associated with the Forge version.
    /// </summary>
    [JsonPropertyName("jar"), JsonProperty("jar")]
    public string Jar { get; set; }

    /// <summary>
    /// Gets or sets the logging metadata for the Forge version.
    /// </summary>
    [JsonPropertyName("logging"), JsonProperty("logging")]
    public LoggingMeta? Logging { get; set; }

    /// <summary>
    /// Gets or sets the list of libraries required by the Forge version.
    /// </summary>
    [JsonPropertyName("libraries"), JsonProperty("libraries")]
    public List<ForgeLibrary> Libraries { get; set; }
}