using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace Tavstal.KonkordLauncher.Core.Models.MojangApi.Meta;

/// <summary>
/// Represents metadata for a specific version of Minecraft, including arguments, assets, libraries, and other details.
/// </summary>
public class VersionMeta
{
    /// <summary>
    /// Gets or sets the new format of game arguments.
    /// </summary>
    [JsonPropertyName("arguments"), JsonProperty("arguments")]
    public ArgumentMeta? ArgumentsNew { get; set; }

    /// <summary>
    /// Gets or sets the legacy format of game arguments as a single string.
    /// </summary>
    [JsonPropertyName("minecraftArguments"), JsonProperty("minecraftArguments")]
    public string? ArgumentsLegacy { get; set; }

    /// <summary>
    /// Gets or sets the metadata for the asset index.
    /// </summary>
    [JsonPropertyName("assetIndex"), JsonProperty("assetIndex")]
    public AssetMeta Index { get; set; }

    /// <summary>
    /// Gets or sets the name of the assets version.
    /// </summary>
    [JsonPropertyName("assets"), JsonProperty("assets")]
    public string Assets { get; set; }

    /// <summary>
    /// Gets or sets the compliance level of the version.
    /// </summary>
    [JsonPropertyName("complianceLevel"), JsonProperty("complianceLevel")]
    public int ComplianceLevel { get; set; }

    /// <summary>
    /// Gets or sets the metadata for downloads associated with this version.
    /// </summary>
    [JsonPropertyName("downloads"), JsonProperty("downloads")]
    public DownloadsMeta Downloads { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier for this version.
    /// </summary>
    [JsonPropertyName("id"), JsonProperty("id")]
    public string Id { get; set; }

    /// <summary>
    /// Gets or sets the metadata for the required Java version.
    /// </summary>
    [JsonPropertyName("javaVersion"), JsonProperty("javaVersion")]
    public JavaVersionMeta JavaVersionMeta { get; set; }

    /// <summary>
    /// Gets or sets the list of libraries required for this version.
    /// </summary>
    [JsonPropertyName("libraries"), JsonProperty("libraries")]
    public List<LibraryMeta> Libraries { get; set; }

    /// <summary>
    /// Gets or sets the metadata for logging configuration.
    /// </summary>
    [JsonPropertyName("logging"), JsonProperty("logging")]
    public LoggingMeta LoggingMeta { get; set; }

    /// <summary>
    /// Gets or sets the main class to be executed for this version.
    /// </summary>
    [JsonPropertyName("mainClass"), JsonProperty("mainClass")]
    public string MainClass { get; set; }

    /// <summary>
    /// Gets or sets the type of this version (e.g., release, snapshot).
    /// </summary>
    [JsonPropertyName("type"), JsonProperty("type")]
    public string Type { get; set; }


    /// <summary>
    /// Retrieves the game arguments as a list of strings.
    /// </summary>
    /// <returns>A list of game arguments.</returns>
    /// <exception cref="Exception">Thrown if no arguments are available.</exception>
    public List<string> GetGameArguments()
    {
        if (ArgumentsNew != null)
            return ArgumentsNew.GetGameArgs();
        if (ArgumentsLegacy != null)
            return ArgumentsLegacy.Split(' ').ToList();
        throw new Exception("Failed to get the game arguments");
    }

    /// <summary>
    /// Retrieves the game arguments as a single string.
    /// </summary>
    /// <returns>A string containing the game arguments.</returns>
    /// <exception cref="Exception">Thrown if no arguments are available.</exception>
    public string GetGameArgumentString()
    {
        if (ArgumentsNew != null)
            return ArgumentsNew.GetGameArgString();
        if (ArgumentsLegacy != null)
            return ArgumentsLegacy;
        throw new Exception("Failed to get the game arguments");
    }

    /// <summary>
    /// Retrieves the JVM arguments as a list of strings.
    /// </summary>
    /// <returns>A list of JVM arguments.</returns>
    /// <exception cref="Exception">Thrown if no arguments are available.</exception>
    public List<string> GetJvmArguments()
    {
        if (ArgumentsNew != null)
            return ArgumentsNew.GetJvmArgs();
        if (ArgumentsLegacy != null)
            return new List<string>()
            {
                "-Djava.library.path=${natives_directory}",
                "-Dminecraft.launcher.brand=${launcher_name}",
                "-Dminecraft.launcher.version=${launcher_version}",
                "-cp ${classpath}"
            }; // not provided, adding defaults
        throw new Exception("Failed to get the game arguments");
    }

    /// <summary>
    /// Retrieves the JVM arguments as a single string.
    /// </summary>
    /// <returns>A string containing the JVM arguments.</returns>
    /// <exception cref="Exception">Thrown if no arguments are available.</exception>
    public string GetJvmArgumentString()
    {
        if (ArgumentsNew != null)
            return ArgumentsNew.GetJvmArgString();
        if (ArgumentsLegacy != null)
            return
                "-Djava.library.path=${natives_directory} -Dminecraft.launcher.brand=${launcher_name} -Dminecraft.launcher.version=${launcher_version} -cp ${classpath}";
        throw new Exception("Failed to get the game arguments");
    }
}