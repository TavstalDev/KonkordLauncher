using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using Tavstal.KonkordLauncher.Desktop.Models.DTOs;
using Tavstal.KonkordLauncher.Desktop.Models.DTOs.Config;

namespace Tavstal.KonkordLauncher.Desktop.Models.Config.Launcher;

/// <summary>
///     Holds the main settings for the launcher.
/// </summary>
public partial class CoreConfigModel : ObservableObject
{
    /// <summary>
    ///     Creates a new instance with default settings.
    /// </summary>
    public CoreConfigModel() { }

    /// <summary>
    ///     Creates a new instance with the given settings.
    /// </summary>
    /// <param name="launcher">The launcher settings.</param>
    /// <param name="java">The Java settings.</param>
    /// <param name="minecraft">The Minecraft settings.</param>
    /// <param name="misc">The other settings.</param>
    public CoreConfigModel(LauncherConfigModel launcher, JavaConfigModel java, MinecraftConfigModel minecraft,
        MiscConfigModel misc)
    {
        Launcher = launcher;
        Java = java;
        Minecraft = minecraft;
        Misc = misc;
    }

    /// <summary>
    ///     Creates a new instance from an existing configuration.
    /// </summary>
    /// <param name="config">The configuration to copy.</param>
    public CoreConfigModel(CoreConfig config)
    {
        Launcher = new LauncherConfigModel
        {
            EnableAutomaticUpdates = config.Launcher.EnableAutomaticUpdates,
            UpdateInterval = config.Launcher.UpdateInterval,
            Language = config.Launcher.Language,
            Theme = config.Launcher.Theme,
            AssetsDirectoryPath = config.Launcher.AssetsDirectoryPath,
            CacheDirectoryPath = config.Launcher.CacheDirectoryPath,
            IconsDirectoryPath = config.Launcher.IconsDirectoryPath,
            InstancesDirectoryPath = config.Launcher.InstancesDirectoryPath,
            JavaDirectoryPath = config.Launcher.JavaDirectoryPath,
            LibrariesDirectoryPath = config.Launcher.LibrariesDirectoryPath,
            ManifestsDirectoryPath = config.Launcher.ManifestsDirectoryPath,
            TranslationsDirectoryPath = config.Launcher.TranslationsDirectoryPath,
            VersionsDirectoryPath = config.Launcher.VersionsDirectoryPath
        };
        Java = new JavaConfigModel
        {
            MinMemory = config.Java.MinMemory,
            MaxMemory = config.Java.MaxMemory,
            PermaGen = config.Java.PermaGen,
            DefaultJavaPath = config.Java.JavaPath,
            JvmArguments = config.Java.JvmArguments
        };
        Minecraft = new MinecraftConfigModel
        {
            StartMaximized = config.Minecraft.StartMaximized,
            WindowWidth = config.Minecraft.WindowWidth,
            WindowHeight = config.Minecraft.WindowHeight,
            CloseLauncherOnGameStart = config.Minecraft.CloseLauncherOnGameStart,
            CloseLauncherOnGameExit = config.Minecraft.CloseLauncherOnGameExit
        };
        Misc = new MiscConfigModel
        {
            PreLaunchCommand = config.Misc.PreLaunchCommand,
            WrapperCommand = config.Misc.WrapperCommand,
            PostExitCommand = config.Misc.PostExitCommand,
            UseCustomGlfw = config.Misc.UseCustomGlfw,
            CustomGlfwPath = config.Misc.CustomGlfwPath,
            UseCustomOpenAl = config.Misc.UseCustomOpenAl,
            CustomOpenAlPath = config.Misc.CustomOpenAlPath,
            EnableFeralGameMode = config.Misc.EnableFeralGameMode,
            EnableMangoHud = config.Misc.EnableMangoHud,
            UseDedicatedGpu = config.Misc.UseDedicatedGpu
        };
    }

    /// <summary>
    ///     Gets or sets the launcher settings.
    /// </summary>
    [ObservableProperty]
    public partial LauncherConfigModel Launcher { get; set; }

    /// <summary>
    ///     Gets or sets the Java settings.
    /// </summary>
    [ObservableProperty]
    public partial JavaConfigModel Java { get; set; }

    /// <summary>
    ///     Gets or sets the Minecraft settings.
    /// </summary>
    [ObservableProperty]
    public partial MinecraftConfigModel Minecraft { get; set; }

    /// <summary>
    ///     Gets or sets the other settings.
    /// </summary>
    [ObservableProperty]
    public partial MiscConfigModel Misc { get; set; }

    /// <summary>
    ///     Gets the languages that the launcher supports.
    /// </summary>
    public List<Language> AvailableLanguages =>
    [
        new() { Name = "English", TwoLetterCode = "en", IsDefault = true }
    ];
}
