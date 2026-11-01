using CommunityToolkit.Mvvm.ComponentModel;

namespace Tavstal.KonkordLauncher.Desktop.Models.Config.Instance;

/// <summary>
///     Holds the game settings for an instance.
/// </summary>
public partial class InstanceGameConfigModel : ObservableObject
{
    /// <summary>
    ///     Creates a new instance with default settings.
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
    ///     Creates a new instance with the given settings.
    /// </summary>
    /// <param name="startMaximized">Whether the game starts maximized.</param>
    /// <param name="windowWidth">The width of the game window.</param>
    /// <param name="windowHeight">The height of the game window.</param>
    /// <param name="showConsoleWhileGameRunning">Whether the console is shown while the game runs.</param>
    /// <param name="closeConsoleOnGameExit">Whether the console closes when the game exits.</param>
    /// <param name="showConsoleWhenGameCrashes">Whether the console is shown when the game crashes.</param>
    /// <param name="enableMangoHud">Whether MangoHud is enabled.</param>
    /// <param name="enableFeralGameMode">Whether Feral GameMode is enabled.</param>
    /// <param name="useDedicatedGpu">Whether the game uses the dedicated GPU.</param>
    public InstanceGameConfigModel(bool startMaximized, uint windowWidth, uint windowHeight,
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
    [ObservableProperty]
    public partial bool StartMaximized { get; set; }

    /// <summary>
    ///     Gets or sets the width of the game window.
    /// </summary>
    [ObservableProperty]
    public partial uint WindowWidth { get; set; }

    /// <summary>
    ///     Gets or sets the height of the game window.
    /// </summary>
    [ObservableProperty]
    public partial uint WindowHeight { get; set; }

    /// <summary>
    ///     Gets or sets a value that indicates whether the console is shown while the game runs.
    /// </summary>
    [ObservableProperty]
    public partial bool ShowConsoleWhileGameRunning { get; set; }

    /// <summary>
    ///     Gets or sets a value that indicates whether the console closes when the game exits.
    /// </summary>
    [ObservableProperty]
    public partial bool CloseConsoleOnGameExit { get; set; }

    /// <summary>
    ///     Gets or sets a value that indicates whether the console is shown when the game crashes.
    /// </summary>
    [ObservableProperty]
    public partial bool ShowConsoleWhenGameCrashes { get; set; }

    /// <summary>
    ///     Gets or sets a value that indicates whether MangoHud is enabled.
    /// </summary>
    [ObservableProperty]
    public partial bool EnableMangoHud { get; set; }

    /// <summary>
    ///     Gets or sets a value that indicates whether Feral GameMode is enabled.
    /// </summary>
    [ObservableProperty]
    public partial bool EnableFeralGameMode { get; set; }

    /// <summary>
    ///     Gets or sets a value that indicates whether the game uses the dedicated GPU.
    /// </summary>
    [ObservableProperty]
    public partial bool UseDedicatedGpu { get; set; }
}
