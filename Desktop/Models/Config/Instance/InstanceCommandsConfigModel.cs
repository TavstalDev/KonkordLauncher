using CommunityToolkit.Mvvm.ComponentModel;

namespace Tavstal.KonkordLauncher.Desktop.Models.Config.Instance;

/// <summary>
///     Holds the pre-launch, wrapper, and post-exit commands for an instance.
/// </summary>
public partial class InstanceCommandsConfigModel : ObservableObject
{
    /// <summary>
    ///     Creates a new instance with empty commands.
    /// </summary>
    public InstanceCommandsConfigModel()
    {
        PreLaunchCommand = string.Empty;
        WrapperCommand = string.Empty;
        PostExitCommand = string.Empty;
    }

    /// <summary>
    ///     Creates a new instance with the given commands.
    /// </summary>
    /// <param name="preLaunchCommand">The command to run before the instance starts.</param>
    /// <param name="wrapperCommand">The command that wraps the game process.</param>
    /// <param name="postExitCommand">The command to run after the instance exits.</param>
    public InstanceCommandsConfigModel(string preLaunchCommand, string wrapperCommand, string postExitCommand)
    {
        PreLaunchCommand = preLaunchCommand;
        WrapperCommand = wrapperCommand;
        PostExitCommand = postExitCommand;
    }

    /// <summary>
    ///     Gets or sets the command to run before the instance starts.
    /// </summary>
    [ObservableProperty]
    public partial string PreLaunchCommand { get; set; }

    /// <summary>
    ///     Gets or sets the command that wraps the game process.
    /// </summary>
    [ObservableProperty]
    public partial string WrapperCommand { get; set; }

    /// <summary>
    ///     Gets or sets the command to run after the instance exits.
    /// </summary>
    [ObservableProperty]
    public partial string PostExitCommand { get; set; }
}
