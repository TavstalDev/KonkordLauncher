using CommunityToolkit.Mvvm.ComponentModel;

namespace Tavstal.KonkordLauncher.Desktop.Models.Config.Launcher;

/// <summary>
///     Holds the Minecraft settings for the launcher.
/// </summary>
public partial class MinecraftConfigModel : ObservableObject
{
    /// <summary>
    ///     Creates a new instance with default settings.
    /// </summary>
    public MinecraftConfigModel() { }

    /// <summary>
    ///     Creates a new instance with the given settings.
    /// </summary>
    /// <param name="startMaximized">Whether the game starts maximized.</param>
    /// <param name="windowWidth">The width of the game window.</param>
    /// <param name="windowHeight">The height of the game window.</param>
    /// <param name="closeLauncherOnGameStart">Whether the launcher closes when the game starts.</param>
    /// <param name="closeLauncherOnGameExit">Whether the launcher closes when the game exits.</param>
    public MinecraftConfigModel(bool startMaximized, uint windowWidth, uint windowHeight, bool closeLauncherOnGameStart,
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
    ///     Gets or sets a value that indicates whether the launcher closes when the game starts.
    /// </summary>
    [ObservableProperty]
    public partial bool CloseLauncherOnGameStart { get; set; }

    /// <summary>
    ///     Gets or sets a value that indicates whether the launcher closes when the game exits.
    /// </summary>
    [ObservableProperty]
    public partial bool CloseLauncherOnGameExit { get; set; }
}
