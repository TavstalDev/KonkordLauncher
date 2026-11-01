using System.Collections.Generic;
using System.Text.Json.Serialization;
using Tavstal.KonkordLauncher.Desktop.Models.DTOs.Config;

namespace Tavstal.KonkordLauncher.Desktop.Models.DTOs.InstanceConfig;

/// <summary>
///     Holds the settings of an instance.
/// </summary>
public class InstanceConfig
{
    /// <summary>
    ///     Creates a new instance with default values.
    /// </summary>
    public InstanceConfig()
    {
        Java = new JavaConfig();
        Game = new InstanceGameConfig();
        Commands = new InstanceCommandsConfig();
        EnableEnvironment = false;
        Environment = [];
        Misc = new InstanceMiscConfig();
    }

    /// <summary>
    ///     Creates a new instance with the given values.
    /// </summary>
    /// <param name="java">The Java settings.</param>
    /// <param name="game">The game settings.</param>
    /// <param name="commands">The command settings.</param>
    /// <param name="enableEnvironment">Whether environment variables are enabled.</param>
    /// <param name="environment">The environment variables.</param>
    /// <param name="misc">The other settings.</param>
    public InstanceConfig(JavaConfig java, InstanceGameConfig game, InstanceCommandsConfig commands,
        bool enableEnvironment, List<EnvironmentVariable> environment, InstanceMiscConfig misc)
    {
        Java = java;
        Game = game;
        Commands = commands;
        EnableEnvironment = enableEnvironment;
        Environment = environment;
        Misc = misc;
    }

    /// <summary>
    ///     Gets or sets the Java settings.
    /// </summary>
    [JsonPropertyName("java")]
    public JavaConfig Java { get; set; }

    /// <summary>
    ///     Gets or sets the game settings.
    /// </summary>
    [JsonPropertyName("game")]
    public InstanceGameConfig Game { get; set; }

    /// <summary>
    ///     Gets or sets the command settings.
    /// </summary>
    [JsonPropertyName("customCommands")]
    public InstanceCommandsConfig Commands { get; set; }

    /// <summary>
    ///     Gets or sets a value that indicates whether environment variables are enabled.
    /// </summary>
    [JsonPropertyName("enableEnvironment")]
    public bool EnableEnvironment { get; set; }

    /// <summary>
    ///     Gets or sets the environment variables.
    /// </summary>
    [JsonPropertyName("environment")]
    public List<EnvironmentVariable> Environment { get; set; }

    /// <summary>
    ///     Gets or sets the other settings.
    /// </summary>
    [JsonPropertyName("misc")]
    public InstanceMiscConfig Misc { get; set; }
}
