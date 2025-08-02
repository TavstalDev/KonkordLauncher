using CommunityToolkit.Mvvm.ComponentModel;

namespace Tavstal.KonkordLauncher.Desktop.Models.Config.Launcher;

public partial class MinecraftConfigModel : ObservableObject
{
    [ObservableProperty] private bool _startMaximized;

    [ObservableProperty] private uint _windowWidth;

    [ObservableProperty] private uint _windowHeight;

    [ObservableProperty] private bool _closeLauncherOnGameStart;

    [ObservableProperty] private bool _closeLauncherOnGameExit;

    public MinecraftConfigModel() {}
    
    public MinecraftConfigModel(bool startMaximized, uint windowWidth, uint windowHeight, bool closeLauncherOnGameStart, bool closeLauncherOnGameExit)
    {
        _startMaximized = startMaximized;
        _windowWidth = windowWidth;
        _windowHeight = windowHeight;
        _closeLauncherOnGameStart = closeLauncherOnGameStart;
        _closeLauncherOnGameExit = closeLauncherOnGameExit;
    }
}