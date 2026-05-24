
using Newtonsoft.Json;
using Tavstal.KonkordLauncher.Core.Models.MojangApi.Meta;

namespace Tavstal.KonkordLauncher.Core.Models.ModLoaders.Forge.Modern;

/// <summary>
/// Represents metadata for a specific version of the Forge mod loader.
/// </summary>
public class ForgeVersionMeta
{
    /// <summary>
    /// Gets or sets the arguments metadata for the Forge version.
    /// </summary>
    [JsonProperty("arguments")]
    public ArgumentMeta Arguments { get; set; }

    /// <summary>
    /// Gets or sets the Minecraft arguments for the Forge version.
    /// </summary>
    [JsonProperty("minecraftArguments")]
    public string? MinecraftArguments { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier of the Forge version.
    /// </summary>
    [JsonProperty("id")]
    public string Id { get; set; }

    /// <summary>
    /// Gets or sets the version of Minecraft that this Forge version inherits from.
    /// </summary>
    [JsonProperty("inheritsFrom")]
    public string InheritsFrom { get; set; }

    /// <summary>
    /// Gets or sets the list of libraries required by the Forge version.
    /// </summary>
    [JsonProperty("libraries")]
    public List<LibraryMeta> Libraries { get; set; }

    /// <summary>
    /// Gets or sets the logging metadata for the Forge version.
    /// </summary>
    [JsonProperty("logging")]
    public LoggingMeta LoggingMeta { get; set; }

    /// <summary>
    /// Gets or sets the main class to be executed for the Forge version.
    /// </summary>
    [JsonProperty("mainClass")]
    public string MainClass { get; set; }

    /// <summary>
    /// Gets or sets the type of the Forge version (e.g., "release", "snapshot").
    /// </summary>
    [JsonProperty("type")]
    public string Type { get; set; }
}