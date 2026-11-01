using System.Text.Json.Serialization;

namespace Tavstal.KonkordLauncher.Desktop.Models.DTOs.Config;

/// <summary>
///     Holds the Minecraft settings, such as the window size and launcher behavior.
/// </summary>
public class MinecraftConfig
{
    /// <summary>
    ///     Creates a new instance with default values.
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
    ///     Creates a new instance with the given values.
    /// </summary>
    /// <param name="startMaximized">Whether the game starts maximized.</param>
    /// <param name="windowWidth">The width of the game window in pixels.</param>
    /// <param name="windowHeight">The height of the game window in pixels.</param>
    /// <param name="closeLauncherOnGameStart">Whether the launcher closes when the game starts.</param>
    /// <param name="closeLauncherOnGameExit">Whether the launcher closes when the game exits.</param>
    public MinecraftConfig(bool startMaximized, uint windowWidth, uint windowHeight, bool closeLauncherOnGameStart,
        bool closeLauncherOnGameExit)
    {
        StartMaximized = startMaximized;
        WindowWidth = windowWidth;
        WindowHeight = windowHeight;
        CloseLauncherOnGameStart = closeLauncherOnGameStart;
        CloseLauncherOnGameExit = closeLauncherOnGameExit;
    }

    /// <summary>
    ///     Gets or sets a value that indicates whether the game starts maximized.
    /// </summary>
    [JsonPropertyName("startMaximized")]
    public bool StartMaximized { get; set; }

    /// <summary>
    ///     Gets or sets the width of the game window in pixels.
    /// </summary>
    [JsonPropertyName("windowWidth")]
    public uint WindowWidth { get; set; }

    /// <summary>
    ///     Gets or sets the height of the game window in pixels.
    /// </summary>
    [JsonPropertyName("windowHeight")]
    public uint WindowHeight { get; set; }

    /// <summary>
    ///     Gets or sets a value that indicates whether the launcher closes when the game starts.
    /// </summary>
    [JsonPropertyName("closeLauncherOnGameStart")]
    public bool CloseLauncherOnGameStart { get; set; }

    /// <summary>
    ///     Gets or sets a value that indicates whether the launcher closes when the game exits.
    /// </summary>
    [JsonPropertyName("closeLauncherOnGameExit")]
    public bool CloseLauncherOnGameExit { get; set; }
}
