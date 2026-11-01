using CommunityToolkit.Mvvm.ComponentModel;

namespace Tavstal.KonkordLauncher.Desktop.Models.Config.Launcher;

/// <summary>
///     Holds other launcher settings, such as commands and library paths.
/// </summary>
public partial class MiscConfigModel : ObservableObject
{
    /// <summary>
    ///     Creates a new instance with default settings.
    /// </summary>
    public MiscConfigModel() { }

    /// <summary>
    ///     Creates a new instance with the given settings.
    /// </summary>
    /// <param name="preLaunchCommand">The command to run before the launcher starts.</param>
    /// <param name="wrapperCommand">The command that wraps the game process.</param>
    /// <param name="postExitCommand">The command to run after the launcher exits.</param>
    /// <param name="useCustomGlfw">Whether a custom GLFW library is used.</param>
    /// <param name="customGlfwPath">The path to the custom GLFW library.</param>
    /// <param name="useCustomOpenAl">Whether a custom OpenAL library is used.</param>
    /// <param name="customOpenAlPath">The path to the custom OpenAL library.</param>
    /// <param name="enableFeralGameMode">Whether Feral GameMode is enabled.</param>
    /// <param name="enableMangoHud">Whether MangoHud is enabled.</param>
    /// <param name="useDedicatedGpu">Whether the game uses the dedicated GPU.</param>
    public MiscConfigModel(string preLaunchCommand, string wrapperCommand, string postExitCommand, bool useCustomGlfw,
        string customGlfwPath, bool useCustomOpenAl, string customOpenAlPath, bool enableFeralGameMode,
        bool enableMangoHud, bool useDedicatedGpu)
    {
        PreLaunchCommand = preLaunchCommand;
        WrapperCommand = wrapperCommand;
        PostExitCommand = postExitCommand;
        UseCustomGlfw = useCustomGlfw;
        CustomGlfwPath = customGlfwPath;
        UseCustomOpenAl = useCustomOpenAl;
        CustomOpenAlPath = customOpenAlPath;
        EnableFeralGameMode = enableFeralGameMode;
        EnableMangoHud = enableMangoHud;
        UseDedicatedGpu = useDedicatedGpu;
    }

    /// <summary>
    ///     Gets or sets the command to run before the launcher starts.
    /// </summary>
    [ObservableProperty]
    public partial string PreLaunchCommand { get; set; }

    /// <summary>
    ///     Gets or sets the command that wraps the game process.
    /// </summary>
    [ObservableProperty]
    public partial string WrapperCommand { get; set; }

    /// <summary>
    ///     Gets or sets the command to run after the launcher exits.
    /// </summary>
    [ObservableProperty]
    public partial string PostExitCommand { get; set; }

    /// <summary>
    ///     Gets or sets a value that indicates whether a custom GLFW library is used.
    /// </summary>
    [ObservableProperty]
    public partial bool UseCustomGlfw { get; set; }

    /// <summary>
    ///     Gets or sets the path to the custom GLFW library.
    /// </summary>
    [ObservableProperty]
    public partial string CustomGlfwPath { get; set; }

    /// <summary>
    ///     Gets or sets a value that indicates whether a custom OpenAL library is used.
    /// </summary>
    [ObservableProperty]
    public partial bool UseCustomOpenAl { get; set; }

    /// <summary>
    ///     Gets or sets the path to the custom OpenAL library.
    /// </summary>
    [ObservableProperty]
    public partial string CustomOpenAlPath { get; set; }

    /// <summary>
    ///     Gets or sets a value that indicates whether Feral GameMode is enabled.
    /// </summary>
    [ObservableProperty]
    public partial bool EnableFeralGameMode { get; set; }

    /// <summary>
    ///     Gets or sets a value that indicates whether MangoHud is enabled.
    /// </summary>
    [ObservableProperty]
    public partial bool EnableMangoHud { get; set; }

    /// <summary>
    ///     Gets or sets a value that indicates whether the game uses the dedicated GPU.
    /// </summary>
    [ObservableProperty]
    public partial bool UseDedicatedGpu { get; set; }
}
