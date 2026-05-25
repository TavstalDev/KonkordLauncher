using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using Tavstal.KonkordLauncher.Common.Models;
using Tavstal.KonkordLauncher.Common.Models.Config;
using Tavstal.KonkordLauncher.Common.Translation;

namespace Tavstal.KonkordLauncher.Desktop.Models.Config.Launcher;

/// <summary>
/// Represents the core configuration model for the launcher, including launcher, Java, Minecraft, and miscellaneous settings.
/// </summary>
public partial class CoreConfigModel : ObservableObject
{
    /// <summary>
    /// Gets or sets the launcher configuration.
    /// </summary>
    [ObservableProperty]
    public partial LauncherConfigModel Launcher { get; set; }

    /// <summary>
    /// Gets or sets the Java configuration.
    /// </summary>
    [ObservableProperty]
    public partial JavaConfigModel Java { get; set; }

    /// <summary>
    /// Gets or sets the Minecraft configuration.
    /// </summary>
    [ObservableProperty]
    public partial MinecraftConfigModel Minecraft { get; set; }

    /// <summary>
    /// Gets or sets the miscellaneous configuration.
    /// </summary>
    [ObservableProperty]
    public partial MiscConfigModel Misc { get; set; }

    /// <summary>
    /// Gets the list of available languages for the launcher.
    /// </summary>
    public List<Language> AvailableLanguages => LanguagePackProvider.LanguagePacks;

    /// <summary>
    /// Initializes a new instance of the <see cref="CoreConfigModel"/> class with default values.
    /// </summary>
    public CoreConfigModel() {}

    /// <summary>
    /// Initializes a new instance of the <see cref="CoreConfigModel"/> class with specified configurations.
    /// </summary>
    /// <param name="launcher">The launcher configuration.</param>
    /// <param name="java">The Java configuration.</param>
    /// <param name="minecraft">The Minecraft configuration.</param>
    /// <param name="misc">The miscellaneous configuration.</param>
    public CoreConfigModel(LauncherConfigModel launcher, JavaConfigModel java, MinecraftConfigModel minecraft, MiscConfigModel misc)
    {
        Launcher = launcher;
        Java = java;
        Minecraft = minecraft;
        Misc = misc;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CoreConfigModel"/> class using a core configuration object.
    /// </summary>
    /// <param name="config">The core configuration object to initialize from.</param>
    public CoreConfigModel(CoreConfig config)
    {
        Launcher = new LauncherConfigModel {
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
        Java = new JavaConfigModel {
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
}