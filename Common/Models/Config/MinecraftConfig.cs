
using System.Text.Json.Serialization;


namespace Tavstal.KonkordLauncher.Common.Models.Config;

/// <summary>
/// Represents the configuration settings for Minecraft, including window properties
/// and launcher behavior during game start and exit.
/// </summary>
public class MinecraftConfig
{
    /// <summary>
    /// Gets or sets a value indicating whether the game should start maximized.
    /// </summary>
    [JsonPropertyName("startMaximized")]
    public bool StartMaximized { get; set; }
    
    /// <summary>
    /// Gets or sets the width of the game window in pixels.
    /// </summary>
    [JsonPropertyName("windowWidth")]
    public uint WindowWidth { get; set; }
    
    /// <summary>
    /// Gets or sets the height of the game window in pixels.
    /// </summary>
    [JsonPropertyName("windowHeight")]
    public uint WindowHeight { get; set; }
    
    /// <summary>
    /// Gets or sets a value indicating whether the launcher should close when the game starts.
    /// </summary>
    [JsonPropertyName("closeLauncherOnGameStart")]
    public bool CloseLauncherOnGameStart { get; set; }
    
    /// <summary>
    /// Gets or sets a value indicating whether the launcher should close when the game exits.
    /// </summary>
    [JsonPropertyName("closeLauncherOnGameExit")]
    public bool CloseLauncherOnGameExit { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="MinecraftConfig"/> class with default values.
    /// </summary>
    public MinecraftConfig()
    {
        StartMaximized = false;
        WindowWidth = 1280;
        WindowHeight = 720;
        CloseLauncherOnGameStart = false;
        CloseLauncherOnGameExit = false;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MinecraftConfig"/> class with specified values.
    /// </summary>
    /// <param name="startMaximized">Whether the game should start maximized.</param>
    /// <param name="windowWidth">The width of the game window in pixels.</param>
    /// <param name="windowHeight">The height of the game window in pixels.</param>
    /// <param name="closeLauncherOnGameStart">Whether the launcher should close when the game starts.</param>
    /// <param name="closeLauncherOnGameExit">Whether the launcher should close when the game exits.</param>
    public MinecraftConfig(bool startMaximized, uint windowWidth, uint windowHeight, bool closeLauncherOnGameStart, bool closeLauncherOnGameExit)
    {
        StartMaximized = startMaximized;
        WindowWidth = windowWidth;
        WindowHeight = windowHeight;
        CloseLauncherOnGameStart = closeLauncherOnGameStart;
        CloseLauncherOnGameExit = closeLauncherOnGameExit;
    }
}