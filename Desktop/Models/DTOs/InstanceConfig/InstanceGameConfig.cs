using System.Text.Json.Serialization;

namespace Tavstal.KonkordLauncher.Desktop.Models.DTOs.InstanceConfig;

/// <summary>
///     Holds the game settings of an instance.
/// </summary>
public class InstanceGameConfig
{
    /// <summary>
    ///     Creates a new instance with default values.
    /// </summary>
    public InstanceGameConfig()
    {
        StartMaximized = false;
        WindowWidth = 1280;
        WindowHeight = 720;
        ShowConsoleWhileGameRunning = false;
        CloseConsoleOnGameExit = false;
        ShowConsoleWhenGameCrashes = true;
        EnableMangoHud = false;
        EnableFeralGameMode = false;
        UseDedicatedGpu = false;
    }

    /// <summary>
    ///     Creates a new instance with the given values.
    /// </summary>
    /// <param name="startMaximized">Whether the game starts maximized.</param>
    /// <param name="windowWidth">The width of the game window in pixels.</param>
    /// <param name="windowHeight">The height of the game window in pixels.</param>
    /// <param name="showConsoleWhileGameRunning">Whether the console is shown while the game runs.</param>
    /// <param name="closeConsoleOnGameExit">Whether the console closes when the game exits.</param>
    /// <param name="showConsoleWhenGameCrashes">Whether the console is shown when the game crashes.</param>
    /// <param name="enableMangoHud">Whether MangoHud is enabled.</param>
    /// <param name="enableFeralGameMode">Whether Feral GameMode is enabled.</param>
    /// <param name="useDedicatedGpu">Whether the game uses the dedicated GPU.</param>
    public InstanceGameConfig(bool startMaximized, uint windowWidth, uint windowHeight,
        bool showConsoleWhileGameRunning, bool closeConsoleOnGameExit, bool showConsoleWhenGameCrashes,
        bool enableMangoHud, bool enableFeralGameMode, bool useDedicatedGpu)
    {
        StartMaximized = startMaximized;
        WindowWidth = windowWidth;
        WindowHeight = windowHeight;
        ShowConsoleWhileGameRunning = showConsoleWhileGameRunning;
        CloseConsoleOnGameExit = closeConsoleOnGameExit;
        ShowConsoleWhenGameCrashes = showConsoleWhenGameCrashes;
        EnableMangoHud = enableMangoHud;
        EnableFeralGameMode = enableFeralGameMode;
        UseDedicatedGpu = useDedicatedGpu;
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
    ///     Gets or sets a value that indicates whether the console is shown while the game runs.
    /// </summary>
    [JsonPropertyName("showConsoleWhileGameRunning")]
    public bool ShowConsoleWhileGameRunning { get; set; }

    /// <summary>
    ///     Gets or sets a value that indicates whether the console closes when the game exits.
    /// </summary>
    [JsonPropertyName("closeConsoleOnGameExit")]
    public bool CloseConsoleOnGameExit { get; set; }

    /// <summary>
    ///     Gets or sets a value that indicates whether the console is shown when the game crashes.
    /// </summary>
    [JsonPropertyName("showConsoleWhenGameCrashes")]
    public bool ShowConsoleWhenGameCrashes { get; set; }

    /// <summary>
    ///     Gets or sets a value that indicates whether MangoHud is enabled.
    /// </summary>
    [JsonPropertyName("enableMangoHud")]
    public bool EnableMangoHud { get; set; }

    /// <summary>
    ///     Gets or sets a value that indicates whether Feral GameMode is enabled.
    /// </summary>
    [JsonPropertyName("enableFeralGameMode")]
    public bool EnableFeralGameMode { get; set; }

    /// <summary>
    ///     Gets or sets a value that indicates whether the game uses the dedicated GPU.
    /// </summary>
    [JsonPropertyName("useDedicatedGpu")]
    public bool UseDedicatedGpu { get; set; }
}
