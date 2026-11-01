using System;
using System.Text.Json.Serialization;

namespace Tavstal.KonkordLauncher.Desktop.Models.DTOs.Config;

/// <summary>
///     Holds the main launcher settings.
/// </summary>
public class CoreConfig
{
    /// <summary>
    ///     Creates a new instance with default values.
    /// </summary>
    public CoreConfig()
    {
        Launcher = new LauncherConfig();
        Java = new JavaConfig();
        Minecraft = new MinecraftConfig();
        Misc = new MiscConfig();
        CacheRefreshDate = DateTime.UtcNow;
    }

    /// <summary>
    ///     Creates a new instance with the given settings.
    /// </summary>
    /// <param name="launcher">The launcher settings.</param>
    /// <param name="java">The Java settings.</param>
    /// <param name="minecraft">The Minecraft settings.</param>
    /// <param name="misc">The other settings.</param>
    /// <param name="cacheRefreshDate">The date when the cache was last refreshed.</param>
    public CoreConfig(LauncherConfig launcher, JavaConfig java, MinecraftConfig minecraft, MiscConfig misc,
        DateTime cacheRefreshDate)
    {
        Launcher = launcher;
        Java = java;
        Minecraft = minecraft;
        Misc = misc;
        CacheRefreshDate = cacheRefreshDate;
    }

    /// <summary>
    ///     Gets or sets the launcher settings.
    /// </summary>
    [JsonPropertyName("launcher")]
    public LauncherConfig Launcher { get; set; }

    /// <summary>
    ///     Gets or sets the Java settings.
    /// </summary>
    [JsonPropertyName("java")]
    public JavaConfig Java { get; set; }

    /// <summary>
    ///     Gets or sets the Minecraft settings.
    /// </summary>
    [JsonPropertyName("minecraft")]
    public MinecraftConfig Minecraft { get; set; }

    /// <summary>
    ///     Gets or sets the other settings.
    /// </summary>
    [JsonPropertyName("misc")]
    public MiscConfig Misc { get; set; }

    /// <summary>
    ///     Gets or sets the date when the cache was last refreshed.
    /// </summary>
    [JsonPropertyName("cacheRefreshDate")] public DateTime CacheRefreshDate { get; set; }
}
