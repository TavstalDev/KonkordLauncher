using CommunityToolkit.Mvvm.ComponentModel;

namespace Tavstal.KonkordLauncher.Desktop.Models.Config.Instance;

/// <summary>
/// Represents the configuration model for instance commands, including pre-launch, wrapper, and post-exit commands.
/// </summary>
public partial class InstanceCommandsConfigModel : ObservableObject
{
    /// <summary>
    /// Gets or sets the command to be executed before launching the instance.
    /// </summary>
    [ObservableProperty] private string _preLaunchCommand;

    /// <summary>
    /// Gets or sets the wrapper command to be executed during the instance runtime.
    /// </summary>
    [ObservableProperty] private string _wrapperCommand;

    /// <summary>
    /// Gets or sets the command to be executed after the instance exits.
    /// </summary>
    [ObservableProperty] private string _postExitCommand;

    /// <summary>
    /// Initializes a new instance of the <see cref="InstanceCommandsConfigModel"/> class with default values.
    /// </summary>
    public InstanceCommandsConfigModel()
    {
        PreLaunchCommand = string.Empty;
        WrapperCommand = string.Empty;
        PostExitCommand = string.Empty;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="InstanceCommandsConfigModel"/> class with specified commands.
    /// </summary>
    /// <param name="preLaunchCommand">The command to execute before launching the instance.</param>
    /// <param name="wrapperCommand">The wrapper command to execute during the instance runtime.</param>
    /// <param name="postExitCommand">The command to execute after the instance exits.</param>
    public InstanceCommandsConfigModel(string preLaunchCommand, string wrapperCommand, string postExitCommand)
    {
        PreLaunchCommand = preLaunchCommand;
        WrapperCommand = wrapperCommand;
        PostExitCommand = postExitCommand;
    }
}