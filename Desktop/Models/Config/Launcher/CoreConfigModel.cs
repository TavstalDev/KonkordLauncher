using CommunityToolkit.Mvvm.ComponentModel;
using Tavstal.KonkordLauncher.Common.Models.Config;

namespace Tavstal.KonkordLauncher.Desktop.Models.Config.Launcher;

public partial class CoreConfigModel : ObservableObject
{
    [ObservableProperty] private LauncherConfigModel _launcher;

    [ObservableProperty] private JavaConfigModel _java;

    [ObservableProperty] private MinecraftConfigModel _minecraft;

    [ObservableProperty] private MiscConfigModel _misc;

    public CoreConfigModel() {}
    
    public CoreConfigModel(LauncherConfigModel launcher, JavaConfigModel java, MinecraftConfigModel minecraft, MiscConfigModel misc)
    {
        _launcher = launcher;
        _java = java;
        _minecraft = minecraft;
        _misc = misc;
    }

    public CoreConfigModel(CoreConfig config)
    {
        _launcher = new LauncherConfigModel() {
            EnableAutomaticUpdates = config.Launcher.EnableAutomaticUpdates,
            UpdateInterval = config.Launcher.UpdateInterval,
            Language = config.Launcher.Language,
            Theme = config.Launcher.Theme,
            AssetsDirectoryPath = config.Launcher.AssetsDirectoryPath,
            CacheDirectoryPath = config.Launcher.CacheDirectoryPath,
            IconsDirectoryPath = config.Launcher.IconsDirectoryPath,
            InstancesDirectoryPath = config.Launcher.InstancesDirectoryPath,
            LibrariesDirectoryPath = config.Launcher.LibrariesDirectoryPath,
            ManifestsDirectoryPath = config.Launcher.ManifestsDirectoryPath,
            TranslationsDirectoryPath = config.Launcher.TranslationsDirectoryPath,
            VersionsDirectoryPath = config.Launcher.VersionsDirectoryPath
        };
        _java = new JavaConfigModel() {
            MinMemory = config.Java.MinMemory,
            MaxMemory = config.Java.MaxMemory,
            PermaGen = config.Java.PermaGen,
            DefaultJavaPath = config.Java.DefaultJavaPath,
            JvmArguments = config.Java.JvmArguments,
            JavaPaths = []
        };
        _minecraft = new MinecraftConfigModel()
        {
            StartMaximized = config.Minecraft.StartMaximized,
            WindowWidth = config.Minecraft.WindowWidth,
            WindowHeight = config.Minecraft.WindowHeight,
            CloseLauncherOnGameStart = config.Minecraft.CloseLauncherOnGameStart,
            CloseLauncherOnGameExit = config.Minecraft.CloseLauncherOnGameExit
        };
        _misc = new MiscConfigModel()
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