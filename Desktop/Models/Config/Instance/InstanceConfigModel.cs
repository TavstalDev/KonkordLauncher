using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using Tavstal.KonkordLauncher.Common.Models.InstanceConfig;
using Tavstal.KonkordLauncher.Desktop.Models.Avalonia;
using Tavstal.KonkordLauncher.Desktop.Models.Config.Launcher;

namespace Tavstal.KonkordLauncher.Desktop.Models.Config.Instance;

/// <summary>
/// Represents the configuration model for an instance, including Java, game, commands, environment, and miscellaneous settings.
/// </summary>
public partial class InstanceConfigModel : ObservableObject
{
    /// <summary>
    /// Gets or sets the Java configuration for the instance.
    /// </summary>
    [ObservableProperty] private JavaConfigModel _java;

    /// <summary>
    /// Gets or sets the game configuration for the instance.
    /// </summary>
    [ObservableProperty] private InstanceGameConfigModel _game;

    /// <summary>
    /// Gets or sets the commands configuration for the instance.
    /// </summary>
    [ObservableProperty] private InstanceCommandsConfigModel _commands;

    /// <summary>
    /// Gets or sets a value indicating whether the environment variables are enabled.
    /// </summary>
    [ObservableProperty] private bool _enableEnvironment;

    /// <summary>
    /// Gets or sets the environment variables for the instance.
    /// </summary>
    [ObservableProperty] private ObservableDictionary<string, string> _environment;

    /// <summary>
    /// Gets or sets the miscellaneous configuration for the instance.
    /// </summary>
    [ObservableProperty] private InstanceMiscConfigModel _misc;

    /// <summary>
    /// Initializes a new instance of the <see cref="InstanceConfigModel"/> class with default values.
    /// </summary>
    public InstanceConfigModel()
    {
        Java = new JavaConfigModel();
        Game = new InstanceGameConfigModel();
        Commands = new InstanceCommandsConfigModel();
        EnableEnvironment = false;
        Environment = new ObservableDictionary<string, string>();
        Misc = new InstanceMiscConfigModel();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="InstanceConfigModel"/> class with specified values.
    /// </summary>
    /// <param name="java">The Java configuration for the instance.</param>
    /// <param name="game">The game configuration for the instance.</param>
    /// <param name="commands">The commands configuration for the instance.</param>
    /// <param name="enableEnvironment">Whether the environment variables are enabled.</param>
    /// <param name="environment">The environment variables for the instance.</param>
    /// <param name="misc">The miscellaneous configuration for the instance.</param>
    public InstanceConfigModel(JavaConfigModel java, InstanceGameConfigModel game, InstanceCommandsConfigModel commands, bool enableEnvironment, Dictionary<string, string> environment, InstanceMiscConfigModel misc)
    {
        Java = java;
        Game = game;
        Commands = commands;
        EnableEnvironment = enableEnvironment;
        Environment = new ObservableDictionary<string, string>(environment);
        Misc = misc;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="InstanceConfigModel"/> class using an existing configuration object.
    /// </summary>
    /// <param name="config">The existing configuration object to initialize from.</param>
    public InstanceConfigModel(InstanceConfig config)
    {
        Java = new JavaConfigModel
        {
            MinMemory = config.Java.MinMemory,
            MaxMemory = config.Java.MaxMemory,
            PermaGen = config.Java.PermaGen,
            DefaultJavaPath = config.Java.JavaPath,
            JvmArguments = config.Java.JvmArguments,
        };
        _game = new InstanceGameConfigModel
        {
            StartMaximized = config.Game.StartMaximized,
            WindowHeight = config.Game.WindowHeight,
            WindowWidth = config.Game.WindowWidth,
            CloseConsoleOnGameExit = config.Game.CloseConsoleOnGameExit,
            ShowConsoleWhenGameCrashes = config.Game.ShowConsoleWhenGameCrashes,
            ShowConsoleWhileGameRunning = config.Game.ShowConsoleWhileGameRunning,
            EnableFeralGameMode = config.Game.EnableFeralGameMode,
            EnableMangoHud = config.Game.EnableMangoHud,
            UseDedicatedGpu = config.Game.UseDedicatedGpu,
        };
        _commands = new InstanceCommandsConfigModel
        {
            PreLaunchCommand = config.Commands.PreLaunchCommand,
            WrapperCommand = config.Commands.WrapperCommand,
            PostExitCommand = config.Commands.PostExitCommand,
        };
        _enableEnvironment = config.EnableEnvironment;
        _environment = new ObservableDictionary<string, string>(config.Environment);
        _misc = new InstanceMiscConfigModel
        {
            UseCustomGlfw = config.Misc.UseCustomGlfw,
            CustomGlfwPath = config.Misc.CustomGlfwPath,
            UseCustomOpenAL = config.Misc.UseCustomOpenAL,
            CustomOpenALPath = config.Misc.CustomOpenALPath,
            OverrideAccount = config.Misc.OverrideAccount,
            AccountId = config.Misc.AccountId,
            JoinServerOnLaunch = config.Misc.JoinServerOnLaunch,
            ServerAddress = config.Misc.ServerAddress,
        };
    }
}