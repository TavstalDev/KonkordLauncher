using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace Tavstal.KonkordLauncher.Common.Models.Config;

/// <summary>
/// Represents miscellaneous configuration settings for the launcher, including custom commands,
/// library paths, and additional runtime options.
/// </summary>
public class MiscConfig
{
    /// <summary>
    /// Gets or sets the command to execute before launching the application.
    /// </summary>
    [JsonProperty("preLaunchCommand"), JsonPropertyName("preLaunchCommand")]
    public string PreLaunchCommand { get; set; }

    /// <summary>
    /// Gets or sets the wrapper command to execute the application.
    /// </summary>
    [JsonProperty("wrapperCommand"), JsonPropertyName("wrapperCommand")]
    public string WrapperCommand { get; set; }

    /// <summary>
    /// Gets or sets the command to execute after the application exits.
    /// </summary>
    [JsonProperty("postExitCommand"), JsonPropertyName("postExitCommand")]
    public string PostExitCommand { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether a custom GLFW library should be used.
    /// </summary>
    [JsonProperty("useCustomGlfw"), JsonPropertyName("useCustomGlfw")]
    public bool UseCustomGlfw { get; set; }

    /// <summary>
    /// Gets or sets the file system path to the custom GLFW library.
    /// </summary>
    [JsonProperty("customGlfwPath"), JsonPropertyName("customGlfwPath")]
    public string CustomGlfwPath { get; set; }
    
    /// <summary>
    /// Gets or sets a value indicating whether a custom OpenAL library should be used.
    /// </summary>
    [JsonProperty("useCustomOpenAl"), JsonPropertyName("useCustomOpenAl")]
    public bool UseCustomOpenAl { get; set; }

    /// <summary>
    /// Gets or sets the file system path to the custom OpenAL library.
    /// </summary>
    [JsonProperty("customOpenAlPath"), JsonPropertyName("customOpenAlPath")]
    public string CustomOpenAlPath { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether Feral GameMode should be enabled.
    /// </summary>
    [JsonProperty("enableFeralGameMode"), JsonPropertyName("enableFeralGameMode")]
    public bool EnableFeralGameMode { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether MangoHud should be enabled.
    /// </summary>
    [JsonProperty("enableMangoHud"), JsonPropertyName("enableMangoHud")]
    public bool EnableMangoHud { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether a dedicated GPU should be used.
    /// </summary>
    [JsonProperty("useDedicatedGpu"), JsonPropertyName("useDedicatedGpu")]
    public bool UseDedicatedGpu { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="MiscConfig"/> class with default values.
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
    /// Initializes a new instance of the <see cref="MiscConfig"/> class with specified values.
    /// </summary>
    /// <param name="preLaunchCommand">The command to execute before launching the application.</param>
    /// <param name="wrapperCommand">The wrapper command to execute the application.</param>
    /// <param name="postExitCommand">The command to execute after the application exits.</param>
    /// <param name="useCustomGlfw">Whether a custom GLFW library should be used.</param>
    /// <param name="customGlfwPath">The file system path to the custom GLFW library.</param>
    /// <param name="useCustomOpenAl">Whether a custom OpenAL library should be used.</param>
    /// <param name="customOpenAlPath">The file system path to the custom OpenAL library.</param>
    /// <param name="enableFeralGameMode">Whether Feral GameMode should be enabled.</param>
    /// <param name="enableMangoHud">Whether MangoHud should be enabled.</param>
    /// <param name="useDedicatedGpu">Whether a dedicated GPU should be used.</param>
    public MiscConfig(string preLaunchCommand, string wrapperCommand, string postExitCommand, bool useCustomGlfw, string customGlfwPath, bool useCustomOpenAl, string customOpenAlPath, bool enableFeralGameMode, bool enableMangoHud, bool useDedicatedGpu)
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
}