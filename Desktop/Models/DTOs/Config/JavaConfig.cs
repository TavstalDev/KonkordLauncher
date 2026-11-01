using System.Text.Json.Serialization;

namespace Tavstal.KonkordLauncher.Desktop.Models.DTOs.Config;

/// <summary>
///     Holds the Java settings, such as memory, path, and JVM arguments.
/// </summary>
public class JavaConfig
{
    /// <summary>
    ///     Creates a new instance with default values.
    /// </summary>
    public JavaConfig()
    {
        MinMemory = 1024;
        MaxMemory = 4096;
        PermaGen = 128;
        JavaPath = string.Empty;
        JvmArguments = string.Empty;
    }

    /// <summary>
    ///     Creates a new instance with the given values.
    /// </summary>
    /// <param name="minMemory">The minimum memory in megabytes.</param>
    /// <param name="maxMemory">The maximum memory in megabytes.</param>
    /// <param name="permaGen">The permanent generation memory size in megabytes.</param>
    /// <param name="defaultJavaPath">The path to the Java executable.</param>
    /// <param name="jvmArguments">The extra JVM arguments.</param>
    public JavaConfig(uint minMemory, uint maxMemory, uint permaGen, string defaultJavaPath, string jvmArguments)
    {
        MinMemory = minMemory;
        MaxMemory = maxMemory;
        PermaGen = permaGen;
        JavaPath = defaultJavaPath;
        JvmArguments = jvmArguments;
    }

    /// <summary>
    ///     Gets or sets the minimum memory in megabytes.
    /// </summary>
    [JsonPropertyName("minMemory")]
    public uint MinMemory { get; set; }

    /// <summary>
    ///     Gets or sets the maximum memory in megabytes.
    /// </summary>
    [JsonPropertyName("maxMemory")]
    public uint MaxMemory { get; set; }

    /// <summary>
    ///     Gets or sets the permanent generation memory size in megabytes.
    /// </summary>
    [JsonPropertyName("permaGen")]
    public uint PermaGen { get; set; }

    /// <summary>
    ///     Gets or sets the path to the Java executable.
    /// </summary>
    [JsonPropertyName("javaPath")]
    public string JavaPath { get; set; }

    /// <summary>
    ///     Gets or sets the extra JVM arguments.
    /// </summary>
    [JsonPropertyName("jvmArguments")]
    public string JvmArguments { get; set; }
}
