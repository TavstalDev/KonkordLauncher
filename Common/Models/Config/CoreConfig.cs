
using System.Text.Json.Serialization;


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
    [JsonPropertyName("launcher")]
    public LauncherConfig Launcher { get; set; }
    
    /// <summary>
    /// Gets or sets the Java configuration for the launcher.
    /// </summary>
    [JsonPropertyName("java")]
    public JavaConfig Java { get; set; }
    
    /// <summary>
    /// Gets or sets the Minecraft configuration for the launcher.
    /// </summary>
    [JsonPropertyName("minecraft")]
    public MinecraftConfig Minecraft { get; set; }
    
    /// <summary>
    /// Gets or sets the miscellaneous configuration for the launcher.
    /// </summary>
    [JsonPropertyName("misc")]
    public MiscConfig Misc { get; set; }
    
    [JsonPropertyName("cacheRefreshDate")]
    public DateTime CacheRefreshDate { get; set; }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="CoreConfig"/> class with default values.
    /// </summary>
    public CoreConfig()
    {
        Launcher = new LauncherConfig();
        Java = new JavaConfig();
        Minecraft = new MinecraftConfig();
        Misc = new MiscConfig();
        CacheRefreshDate = DateTime.Now;
    }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="CoreConfig"/> class with the specified configurations.
    /// </summary>
    /// <param name="launcher">The launcher configuration.</param>
    /// <param name="java">The Java configuration.</param>
    /// <param name="minecraft">The Minecraft configuration.</param>
    /// <param name="misc">The miscellaneous configuration.</param>
    /// <param name="cacheRefreshDate">The date when the cache was last refreshed.</param>
    public CoreConfig(LauncherConfig launcher, JavaConfig java, MinecraftConfig minecraft, MiscConfig misc, DateTime cacheRefreshDate)
    {
        Launcher = launcher;
        Java = java;
        Minecraft = minecraft;
        Misc = misc;
        CacheRefreshDate = cacheRefreshDate;
    }
}