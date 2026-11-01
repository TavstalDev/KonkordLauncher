using System.Collections.Generic;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Tavstal.KonkordLauncher.Desktop.Models.Config.Launcher;
using Tavstal.KonkordLauncher.Desktop.Models.DTOs.InstanceConfig;

namespace Tavstal.KonkordLauncher.Desktop.Models.Config.Instance;

/// <summary>
///     Holds all configuration settings for an instance.
/// </summary>
public partial class InstanceConfigModel : ObservableObject
{
    /// <summary>
    ///     Creates a new instance with default settings.
    /// </summary>
    public InstanceConfigModel()
    {
        Java = new JavaConfigModel();
        Game = new InstanceGameConfigModel();
        Commands = new InstanceCommandsConfigModel();
        EnableEnvironment = false;
        Environment = [];
        Misc = new InstanceMiscConfigModel();
    }

    /// <summary>
    ///     Creates a new instance with the given settings.
    /// </summary>
    /// <param name="java">The Java settings.</param>
    /// <param name="game">The game settings.</param>
    /// <param name="commands">The command settings.</param>
    /// <param name="enableEnvironment">Whether environment variables are enabled.</param>
    /// <param name="environment">The environment variables.</param>
    /// <param name="misc">The other settings.</param>
    public InstanceConfigModel(JavaConfigModel java, InstanceGameConfigModel game, InstanceCommandsConfigModel commands,
        bool enableEnvironment, List<EnvironmentVariable> environment, InstanceMiscConfigModel misc)
    {
        Java = java;
        Game = game;
        Commands = commands;
        EnableEnvironment = enableEnvironment;
        Environment = new ObservableCollection<EnvironmentVariable>(environment);
        Misc = misc;
    }

    /// <summary>
    ///     Creates a new instance from an existing configuration.
    /// </summary>
    /// <param name="config">The configuration to copy.</param>
    public InstanceConfigModel(InstanceConfig config)
    {
        Java = new JavaConfigModel
        {
            MinMemory = config.Java.MinMemory,
            MaxMemory = config.Java.MaxMemory,
            PermaGen = config.Java.PermaGen,
            DefaultJavaPath = config.Java.JavaPath,
            JvmArguments = config.Java.JvmArguments
        };
        Game = new InstanceGameConfigModel
        {
            StartMaximized = config.Game.StartMaximized,
            WindowHeight = config.Game.WindowHeight,
            WindowWidth = config.Game.WindowWidth,
            CloseConsoleOnGameExit = config.Game.CloseConsoleOnGameExit,
            ShowConsoleWhenGameCrashes = config.Game.ShowConsoleWhenGameCrashes,
            ShowConsoleWhileGameRunning = config.Game.ShowConsoleWhileGameRunning,
            EnableFeralGameMode = config.Game.EnableFeralGameMode,
            EnableMangoHud = config.Game.EnableMangoHud,
            UseDedicatedGpu = config.Game.UseDedicatedGpu
        };
        Commands = new InstanceCommandsConfigModel
        {
            PreLaunchCommand = config.Commands.PreLaunchCommand,
            WrapperCommand = config.Commands.WrapperCommand,
            PostExitCommand = config.Commands.PostExitCommand
        };
        EnableEnvironment = config.EnableEnvironment;
        Environment = new ObservableCollection<EnvironmentVariable>(config.Environment);
        Misc = new InstanceMiscConfigModel
        {
            UseCustomGlfw = config.Misc.UseCustomGlfw,
            CustomGlfwPath = config.Misc.CustomGlfwPath,
            UseCustomOpenAL = config.Misc.UseCustomOpenAL,
            CustomOpenALPath = config.Misc.CustomOpenALPath,
            OverrideAccount = config.Misc.OverrideAccount,
            AccountId = config.Misc.AccountId,
            JoinServerOnLaunch = config.Misc.JoinServerOnLaunch,
            ServerAddress = config.Misc.ServerAddress
        };
    }

    /// <summary>
    ///     Gets or sets the Java settings.
    /// </summary>
    [ObservableProperty]
    public partial JavaConfigModel Java { get; set; }

    /// <summary>
    ///     Gets or sets the game settings.
    /// </summary>
    [ObservableProperty]
    public partial InstanceGameConfigModel Game { get; set; }

    /// <summary>
    ///     Gets or sets the command settings.
    /// </summary>
    [ObservableProperty]
    public partial InstanceCommandsConfigModel Commands { get; set; }

    /// <summary>
    ///     Gets or sets a value that indicates whether environment variables are enabled.
    /// </summary>
    [ObservableProperty]
    public partial bool EnableEnvironment { get; set; }

    /// <summary>
    ///     Gets or sets the environment variables.
    /// </summary>
    public ObservableCollection<EnvironmentVariable> Environment { get; set; }

    /// <summary>
    ///     Gets or sets the other settings.
    /// </summary>
    [ObservableProperty]
    public partial InstanceMiscConfigModel Misc { get; set; }
}
