using System.Text.Json.Serialization;

namespace Tavstal.KonkordLauncher.Desktop.Models.DTOs.InstanceConfig;

/// <summary>
///     Holds the commands that run at different stages of an instance.
/// </summary>
public class InstanceCommandsConfig
{
    /// <summary>
    ///     Creates a new instance with default values.
    /// </summary>
    public InstanceCommandsConfig()
    {
        PreLaunchCommand = string.Empty;
        WrapperCommand = string.Empty;
        PostExitCommand = string.Empty;
    }

    /// <summary>
    ///     Creates a new instance with the given commands.
    /// </summary>
    /// <param name="preLaunchCommand">The command to run before the game starts.</param>
    /// <param name="wrapperCommand">The command that wraps the game process.</param>
    /// <param name="postExitCommand">The command to run after the game exits.</param>
    public InstanceCommandsConfig(string preLaunchCommand, string wrapperCommand, string postExitCommand)
    {
        PreLaunchCommand = preLaunchCommand;
        WrapperCommand = wrapperCommand;
        PostExitCommand = postExitCommand;
    }

    /// <summary>
    ///     Gets or sets the command to run before the game starts.
    /// </summary>
    [JsonPropertyName("preLaunchCommand")]
    public string PreLaunchCommand { get; set; }

    /// <summary>
    ///     Gets or sets the command that wraps the game process.
    /// </summary>
    [JsonPropertyName("wrapperCommand")]
    public string WrapperCommand { get; set; }

    /// <summary>
    ///     Gets or sets the command to run after the game exits.
    /// </summary>
    [JsonPropertyName("postExitCommand")]
    public string PostExitCommand { get; set; }
}
