using CommunityToolkit.Mvvm.ComponentModel;

namespace Tavstal.KonkordLauncher.Desktop.Models.Config.Launcher;

/// <summary>
/// Represents the configuration model for Minecraft settings in the launcher.
/// </summary>
public partial class MinecraftConfigModel : ObservableObject
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
    /// Gets or sets a value indicating whether the launcher should close when the game starts.
    /// </summary>
    [ObservableProperty]
    public partial bool CloseLauncherOnGameStart { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the launcher should close when the game exits.
    /// </summary>
    [ObservableProperty]
    public partial bool CloseLauncherOnGameExit { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="MinecraftConfigModel"/> class with default values.
    /// </summary>
    public MinecraftConfigModel() {}

    /// <summary>
    /// Initializes a new instance of the <see cref="MinecraftConfigModel"/> class with specified values.
    /// </summary>
    /// <param name="startMaximized">Whether the game should start maximized.</param>
    /// <param name="windowWidth">The width of the game window.</param>
    /// <param name="windowHeight">The height of the game window.</param>
    /// <param name="closeLauncherOnGameStart">Whether the launcher should close when the game starts.</param>
    /// <param name="closeLauncherOnGameExit">Whether the launcher should close when the game exits.</param>
    public MinecraftConfigModel(bool startMaximized, uint windowWidth, uint windowHeight, bool closeLauncherOnGameStart, bool closeLauncherOnGameExit)
    {
        StartMaximized = startMaximized;
        WindowWidth = windowWidth;
        WindowHeight = windowHeight;
        CloseLauncherOnGameStart = closeLauncherOnGameStart;
        CloseLauncherOnGameExit = closeLauncherOnGameExit;
    }
}