using CommunityToolkit.Mvvm.ComponentModel;

namespace Tavstal.KonkordLauncher.Desktop.Models.Config.Launcher;

/// <summary>
/// Represents the miscellaneous configuration model for the launcher, 
/// including pre-launch, wrapper, and post-exit commands, as well as 
/// various customization and performance options.
/// </summary>
public partial class MiscConfigModel: ObservableObject
{
    /// <summary>
    /// Gets or sets the command to execute before launching the application.
    /// </summary>
    [ObservableProperty] private string _preLaunchCommand;

    /// <summary>
    /// Gets or sets the wrapper command to execute during the application's runtime.
    /// </summary>
    [ObservableProperty] private string _wrapperCommand;

    /// <summary>
    /// Gets or sets the command to execute after the application exits.
    /// </summary>
    [ObservableProperty] private string _postExitCommand;

    /// <summary>
    /// Gets or sets a value indicating whether a custom GLFW library should be used.
    /// </summary>
    [ObservableProperty] private bool _useCustomGlfw;

    /// <summary>
    /// Gets or sets the file path to the custom GLFW library.
    /// </summary>
    [ObservableProperty] private string _customGlfwPath;

    /// <summary>
    /// Gets or sets a value indicating whether a custom OpenAL library should be used.
    /// </summary>
    [ObservableProperty] private bool _useCustomOpenAl;

    /// <summary>
    /// Gets or sets the file path to the custom OpenAL library.
    /// </summary>
    [ObservableProperty] private string _customOpenAlPath;

    /// <summary>
    /// Gets or sets a value indicating whether Feral GameMode should be enabled.
    /// </summary>
    [ObservableProperty] private bool _enableFeralGameMode;

    /// <summary>
    /// Gets or sets a value indicating whether MangoHud should be enabled.
    /// </summary>
    [ObservableProperty] private bool _enableMangoHud;

    /// <summary>
    /// Gets or sets a value indicating whether a dedicated GPU should be used.
    /// </summary>
    [ObservableProperty] private bool _useDedicatedGpu;

    /// <summary>
    /// Initializes a new instance of the <see cref="MiscConfigModel"/> class with default values.
    /// </summary>
    public MiscConfigModel() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="MiscConfigModel"/> class with specified values.
    /// </summary>
    /// <param name="preLaunchCommand">The command to execute before launching the application.</param>
    /// <param name="wrapperCommand">The wrapper command to execute during the application's runtime.</param>
    /// <param name="postExitCommand">The command to execute after the application exits.</param>
    /// <param name="useCustomGlfw">Whether a custom GLFW library should be used.</param>
    /// <param name="customGlfwPath">The file path to the custom GLFW library.</param>
    /// <param name="useCustomOpenAl">Whether a custom OpenAL library should be used.</param>
    /// <param name="customOpenAlPath">The file path to the custom OpenAL library.</param>
    /// <param name="enableFeralGameMode">Whether Feral GameMode should be enabled.</param>
    /// <param name="enableMangoHud">Whether MangoHud should be enabled.</param>
    /// <param name="useDedicatedGpu">Whether a dedicated GPU should be used.</param>
    public MiscConfigModel(string preLaunchCommand, string wrapperCommand, string postExitCommand, bool useCustomGlfw, string customGlfwPath, bool useCustomOpenAl, string customOpenAlPath, bool enableFeralGameMode, bool enableMangoHud, bool useDedicatedGpu)
    {
        _preLaunchCommand = preLaunchCommand;
        _wrapperCommand = wrapperCommand;
        _postExitCommand = postExitCommand;
        _useCustomGlfw = useCustomGlfw;
        _customGlfwPath = customGlfwPath;
        _useCustomOpenAl = useCustomOpenAl;
        _customOpenAlPath = customOpenAlPath;
        _enableFeralGameMode = enableFeralGameMode;
        _enableMangoHud = enableMangoHud;
        _useDedicatedGpu = useDedicatedGpu;
    }
}