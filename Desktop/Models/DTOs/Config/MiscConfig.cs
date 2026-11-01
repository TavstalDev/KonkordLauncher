using System.Text.Json.Serialization;

namespace Tavstal.KonkordLauncher.Desktop.Models.DTOs.Config;

/// <summary>
///     Holds other launcher settings, such as custom commands and library paths.
/// </summary>
public class MiscConfig
{
    /// <summary>
    ///     Creates a new instance with default values.
    /// </summary>
    public MiscConfig()
    {
        PreLaunchCommand = string.Empty;
        WrapperCommand = string.Empty;
        PostExitCommand = string.Empty;
        UseCustomGlfw = false;
        CustomGlfwPath = string.Empty;
        UseCustomOpenAl = false;
        CustomOpenAlPath = string.Empty;
        EnableFeralGameMode = false;
        EnableMangoHud = false;
        UseDedicatedGpu = false;
    }

    /// <summary>
    ///     Creates a new instance with the given values.
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
    public MiscConfig(string preLaunchCommand, string wrapperCommand, string postExitCommand, bool useCustomGlfw,
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
    [JsonPropertyName("preLaunchCommand")]
    public string PreLaunchCommand { get; set; }

    /// <summary>
    ///     Gets or sets the command that wraps the game process.
    /// </summary>
    [JsonPropertyName("wrapperCommand")]
    public string WrapperCommand { get; set; }

    /// <summary>
    ///     Gets or sets the command to run after the launcher exits.
    /// </summary>
    [JsonPropertyName("postExitCommand")]
    public string PostExitCommand { get; set; }

    /// <summary>
    ///     Gets or sets a value that indicates whether a custom GLFW library is used.
    /// </summary>
    [JsonPropertyName("useCustomGlfw")]
    public bool UseCustomGlfw { get; set; }

    /// <summary>
    ///     Gets or sets the path to the custom GLFW library.
    /// </summary>
    [JsonPropertyName("customGlfwPath")]
    public string CustomGlfwPath { get; set; }

    /// <summary>
    ///     Gets or sets a value that indicates whether a custom OpenAL library is used.
    /// </summary>
    [JsonPropertyName("useCustomOpenAl")]
    public bool UseCustomOpenAl { get; set; }

    /// <summary>
    ///     Gets or sets the path to the custom OpenAL library.
    /// </summary>
    [JsonPropertyName("customOpenAlPath")]
    public string CustomOpenAlPath { get; set; }

    /// <summary>
    ///     Gets or sets a value that indicates whether Feral GameMode is enabled.
    /// </summary>
    [JsonPropertyName("enableFeralGameMode")]
    public bool EnableFeralGameMode { get; set; }

    /// <summary>
    ///     Gets or sets a value that indicates whether MangoHud is enabled.
    /// </summary>
    [JsonPropertyName("enableMangoHud")]
    public bool EnableMangoHud { get; set; }

    /// <summary>
    ///     Gets or sets a value that indicates whether the game uses the dedicated GPU.
    /// </summary>
    [JsonPropertyName("useDedicatedGpu")]
    public bool UseDedicatedGpu { get; set; }
}
