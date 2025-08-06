using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace Tavstal.KonkordLauncher.Common.Models.Config;

/// <summary>
/// Represents the core configuration for the launcher, including launcher settings, Java settings,
/// Minecraft settings, and miscellaneous options.
/// </summary>
public class CoreConfig
{
    /// <summary>
    /// Gets or sets the configuration for the launcher.
    /// </summary>
    [JsonProperty("launcher"), JsonPropertyName("launcher")]
    public LauncherConfig Launcher { get; set; }
    
    /// <summary>
    /// Gets or sets the Java configuration for the launcher.
    /// </summary>
    [JsonProperty("java"), JsonPropertyName("java")]
    public JavaConfig Java { get; set; }
    
    /// <summary>
    /// Gets or sets the Minecraft configuration for the launcher.
    /// </summary>
    [JsonProperty("minecraft"), JsonPropertyName("minecraft")]
    public MinecraftConfig Minecraft { get; set; }
    
    /// <summary>
    /// Gets or sets the miscellaneous configuration for the launcher.
    /// </summary>
    [JsonProperty("misc"), JsonPropertyName("misc")]
    public MiscConfig Misc { get; set; }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="CoreConfig"/> class with default values.
    /// </summary>
    public CoreConfig()
    {
        Launcher = new LauncherConfig();
        Java = new JavaConfig();
        Minecraft = new MinecraftConfig();
        Misc = new MiscConfig();
    }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="CoreConfig"/> class with specified values.
    /// </summary>
    /// <param name="launcher">The configuration for the launcher.</param>
    /// <param name="java">The Java configuration for the launcher.</param>
    /// <param name="minecraft">The Minecraft configuration for the launcher.</param>
    /// <param name="misc">The miscellaneous configuration for the launcher.</param>
    public CoreConfig(LauncherConfig launcher, JavaConfig java, MinecraftConfig minecraft, MiscConfig misc)
    {
        Launcher = launcher;
        Java = java;
        Minecraft = minecraft;
        Misc = misc;
    }
}