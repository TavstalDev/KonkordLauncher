using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace Tavstal.KonkordLauncher.Common.Models.InstanceConfig;

/// <summary>
/// Represents the configuration settings for a game instance, including window properties,
/// console behavior, and performance optimizations.
/// </summary>
public class InstanceGameConfig
{
    /// <summary>
    /// Gets or sets a value indicating whether the game should start in maximized window mode.
    /// </summary>
    [JsonProperty("startMaximized"), JsonPropertyName("startMaximized")]
    public bool StartMaximized { get; set; }

    /// <summary>
    /// Gets or sets the width of the game window in pixels.
    /// </summary>
    [JsonProperty("windowWidth"), JsonPropertyName("windowWidth")]
    public uint WindowWidth { get; set; }

    /// <summary>
    /// Gets or sets the height of the game window in pixels.
    /// </summary>
    [JsonProperty("windowHeight"), JsonPropertyName("windowHeight")]
    public uint WindowHeight { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the console should be shown while the game is running.
    /// </summary>
    [JsonProperty("showConsoleWhileGameRunning"), JsonPropertyName("showConsoleWhileGameRunning")]
    public bool ShowConsoleWhileGameRunning { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the console should close automatically when the game exits.
    /// </summary>
    [JsonProperty("closeConsoleOnGameExit"), JsonPropertyName("closeConsoleOnGameExit")]
    public bool CloseConsoleOnGameExit { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the console should be shown when the game crashes.
    /// </summary>
    [JsonProperty("showConsoleWhenGameCrashes"), JsonPropertyName("showConsoleWhenGameCrashes")]
    public bool ShowConsoleWhenGameCrashes { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether MangoHud (a performance overlay) should be enabled.
    /// </summary>
    [JsonProperty("enableMangoHud"), JsonPropertyName("enableMangoHud")]
    public bool EnableMangoHud { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether Feral GameMode (a performance optimization tool) should be enabled.
    /// </summary>
    [JsonProperty("enableFeralGameMode"), JsonPropertyName("enableFeralGameMode")]
    public bool EnableFeralGameMode { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether a dedicated GPU should be used for the game.
    /// </summary>
    [JsonProperty("useDedicatedGpu"), JsonPropertyName("useDedicatedGpu")]
    public bool UseDedicatedGpu { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="InstanceGameConfig"/> class with default values.
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
    /// Initializes a new instance of the <see cref="InstanceGameConfig"/> class with specified values.
    /// </summary>
    /// <param name="startMaximized">Whether the game should start in maximized window mode.</param>
    /// <param name="windowWidth">The width of the game window in pixels.</param>
    /// <param name="windowHeight">The height of the game window in pixels.</param>
    /// <param name="showConsoleWhileGameRunning">Whether the console should be shown while the game is running.</param>
    /// <param name="closeConsoleOnGameExit">Whether the console should close automatically when the game exits.</param>
    /// <param name="showConsoleWhenGameCrashes">Whether the console should be shown when the game crashes.</param>
    /// <param name="enableMangoHud">Whether MangoHud should be enabled.</param>
    /// <param name="enableFeralGameMode">Whether Feral GameMode should be enabled.</param>
    /// <param name="useDedicatedGpu">Whether a dedicated GPU should be used for the game.</param>
    public InstanceGameConfig(bool startMaximized, uint windowWidth, uint windowHeight, bool showConsoleWhileGameRunning, bool closeConsoleOnGameExit, bool showConsoleWhenGameCrashes, bool enableMangoHud, bool enableFeralGameMode, bool useDedicatedGpu)
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
}