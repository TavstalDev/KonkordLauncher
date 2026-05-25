using CommunityToolkit.Mvvm.ComponentModel;

namespace Tavstal.KonkordLauncher.Desktop.Models.Config.Instance;

/// <summary>
/// Represents the configuration model for game-specific settings of a Minecraft instance.
/// </summary>
public partial class InstanceGameConfigModel : ObservableObject
{
    /// <summary>
    /// Gets or sets a value indicating whether the game should start maximized.
    /// </summary>
    [ObservableProperty]
    public partial bool StartMaximized { get; set; }

    /// <summary>
    /// Gets or sets the width of the game window.
    /// </summary>
    [ObservableProperty]
    public partial uint WindowWidth { get; set; }

    /// <summary>
    /// Gets or sets the height of the game window.
    /// </summary>
    [ObservableProperty]
    public partial uint WindowHeight { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the console should be shown while the game is running.
    /// </summary>
    [ObservableProperty]
    public partial bool ShowConsoleWhileGameRunning { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the console should close automatically when the game exits.
    /// </summary>
    [ObservableProperty]
    public partial bool CloseConsoleOnGameExit { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the console should be shown when the game crashes.
    /// </summary>
    [ObservableProperty]
    public partial bool ShowConsoleWhenGameCrashes { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether MangoHud should be enabled.
    /// </summary>
    [ObservableProperty]
    public partial bool EnableMangoHud { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether Feral GameMode should be enabled.
    /// </summary>
    [ObservableProperty]
    public partial bool EnableFeralGameMode { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether a dedicated GPU should be used.
    /// </summary>
    [ObservableProperty]
    public partial bool UseDedicatedGpu { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="InstanceGameConfigModel"/> class with default values.
    /// </summary>
    public InstanceGameConfigModel()
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
    /// Initializes a new instance of the <see cref="InstanceGameConfigModel"/> class with specified values.
    /// </summary>
    /// <param name="startMaximized">Whether the game should start maximized.</param>
    /// <param name="windowWidth">The width of the game window.</param>
    /// <param name="windowHeight">The height of the game window.</param>
    /// <param name="showConsoleWhileGameRunning">Whether the console should be shown while the game is running.</param>
    /// <param name="closeConsoleOnGameExit">Whether the console should close automatically when the game exits.</param>
    /// <param name="showConsoleWhenGameCrashes">Whether the console should be shown when the game crashes.</param>
    /// <param name="enableMangoHud">Whether MangoHud should be enabled.</param>
    /// <param name="enableFeralGameMode">Whether Feral GameMode should be enabled.</param>
    /// <param name="useDedicatedGpu">Whether a dedicated GPU should be used.</param>
    public InstanceGameConfigModel(bool startMaximized, uint windowWidth, uint windowHeight, bool showConsoleWhileGameRunning, bool closeConsoleOnGameExit, bool showConsoleWhenGameCrashes, bool enableMangoHud, bool enableFeralGameMode, bool useDedicatedGpu)
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