using Tavstal.KonkordLauncher.Core.Enums;

namespace Tavstal.KonkordLauncher.Core.Models.Installer;

/// <summary>
/// Represents the details of a Minecraft game configuration, including Java path, memory allocation,
/// JVM arguments, Minecraft version, game kind, and optional custom settings.
/// </summary>
public class GameDetails
{
    /// <summary>
    /// Gets or sets the path to the Java executable.
    /// </summary>
    public string JavaPath { get; set; }
    
    /// <summary>
    /// Gets or sets the minimum memory allocation for the game in megabytes.
    /// </summary>
    public uint MinMemory { get; set; }
    
    /// <summary>
    /// Gets or sets the maximum memory allocation for the game in megabytes.
    /// </summary>
    public uint MaxMemory { get; set; }
    
    /// <summary>
    /// Gets or sets the JVM arguments to be used when launching the game.
    /// </summary>
    public string JvmArgs { get; set; }
    
    /// <summary>
    /// Gets or sets the version of Minecraft to be launched.
    /// </summary>
    public string MinecraftVersion { get; set; }
    
    /// <summary>
    /// Gets or sets the kind of Minecraft (e.g., Vanilla, Modded).
    /// </summary>
    public EMinecraftKind Kind { get; set; }
    
    /// <summary>
    /// Gets or sets the custom version of Minecraft, if specified.
    /// </summary>
    public string? CustomVersion { get; set; }
    
    /// <summary>
    /// Gets or sets the custom game directory, if specified.
    /// </summary>
    public string? CustomGameDirectory { get; set; }
    
    /// <summary>
    /// Gets or sets the command to execute before launching the game.
    /// </summary>
    public string PreLaunchCommand { get; set; }

    /// <summary>
    /// Gets or sets the command wrapper to use when launching the game.
    /// </summary>
    public string WrapperCommand { get; set; }

    /// <summary>
    /// Gets or sets the command to execute after the game exits.
    /// </summary>
    public string PostExitCommand { get; set; }

    /// <summary>
    /// Gets or sets the server address to join automatically when launching the game, if specified.
    /// </summary>
    public string? ServerAddressToJoin { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="GameDetails"/> class with the specified parameters.
    /// </summary>
    /// <param name="javaPath">The path to the Java executable.</param>
    /// <param name="minMemory">The minimum memory allocation for the game in megabytes.</param>
    /// <param name="maxMemory">The maximum memory allocation for the game in megabytes.</param>
    /// <param name="jvmArgs">The JVM arguments to be used when launching the game.</param>
    /// <param name="minecraftVersion">The version of Minecraft to be launched.</param>
    /// <param name="kind">The kind of Minecraft (e.g., Vanilla, Modded).</param>
    /// <param name="customVersion">The custom version of Minecraft, if specified.</param>
    /// <param name="customGameDirectory">The custom game directory, if specified.</param>
    /// <param name="preLaunchCommand">The command to execute before launching the game.</param>
    /// <param name="wrapperCommand">The command wrapper to use when launching the game.</param>
    /// <param name="postExitCommand">The command to execute after the game exits.</param>
    /// <param name="serverAddressToJoin">The server address to join automatically when launching the game, if specified.</param>
    public GameDetails(string javaPath, uint minMemory, uint maxMemory, string jvmArgs, string minecraftVersion, EMinecraftKind kind, string? customVersion, string? customGameDirectory, string preLaunchCommand, string wrapperCommand, string postExitCommand, string? serverAddressToJoin)
    {
        JavaPath = javaPath;
        MinMemory = minMemory;
        MaxMemory = maxMemory;
        JvmArgs = jvmArgs;
        MinecraftVersion = minecraftVersion;
        Kind = kind;
        CustomVersion = customVersion;
        CustomGameDirectory = customGameDirectory;
        PreLaunchCommand = preLaunchCommand;
        WrapperCommand = wrapperCommand;
        PostExitCommand = postExitCommand;
        ServerAddressToJoin = serverAddressToJoin;
    }
}