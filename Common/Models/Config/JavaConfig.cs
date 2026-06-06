
using System.Text.Json.Serialization;


namespace Tavstal.KonkordLauncher.Common.Models.Config;

/// <summary>
/// Represents the configuration for Java settings, including memory allocation, Java path, and JVM arguments.
/// </summary>
public class JavaConfig
{
    /// <summary>
    /// Gets or sets the minimum memory allocation for the Java process, in megabytes.
    /// </summary>
    [JsonPropertyName("minMemory")]
    public uint MinMemory { get; set; }

    /// <summary>
    /// Gets or sets the maximum memory allocation for the Java process, in megabytes.
    /// </summary>
    [JsonPropertyName("maxMemory")]
    public uint MaxMemory { get; set; }

    /// <summary>
    /// Gets or sets the size of the permanent generation (PermGen) memory, in megabytes.
    /// </summary>
    [JsonPropertyName("permaGen")]
    public uint PermaGen { get; set; }

    /// <summary>
    /// Gets or sets the file system path to the Java executable.
    /// </summary>
    [JsonPropertyName("javaPath")]
    public string JavaPath { get; set; }

    /// <summary>
    /// Gets or sets the additional JVM arguments to be passed to the Java process.
    /// </summary>
    [JsonPropertyName("jvmArguments")]
    public string JvmArguments { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="JavaConfig"/> class with default values.
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
    /// Initializes a new instance of the <see cref="JavaConfig"/> class with specified values.
    /// </summary>
    /// <param name="minMemory">The minimum memory allocation for the Java process, in megabytes.</param>
    /// <param name="maxMemory">The maximum memory allocation for the Java process, in megabytes.</param>
    /// <param name="permaGen">The size of the permanent generation (PermGen) memory, in megabytes.</param>
    /// <param name="defaultJavaPath">The file system path to the Java executable.</param>
    /// <param name="jvmArguments">The additional JVM arguments to be passed to the Java process.</param>
    public JavaConfig(uint minMemory, uint maxMemory, uint permaGen, string defaultJavaPath, string jvmArguments)
    {
        MinMemory = minMemory;
        MaxMemory = maxMemory;
        PermaGen = permaGen;
        JavaPath = defaultJavaPath;
        JvmArguments = jvmArguments;
    }
}