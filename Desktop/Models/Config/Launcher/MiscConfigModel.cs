using CommunityToolkit.Mvvm.ComponentModel;

namespace Tavstal.KonkordLauncher.Desktop.Models.Config.Launcher;

public partial class MiscConfigModel: ObservableObject
{
    [ObservableProperty] private string _preLaunchCommand;

    [ObservableProperty] private string _wrapperCommand;

    [ObservableProperty] private string _postExitCommand;

    [ObservableProperty] private bool _useCustomGlfw;

    [ObservableProperty] private string _customGlfwPath;

    [ObservableProperty] private bool _useCustomOpenAl;

    [ObservableProperty] private string _customOpenAlPath;

    [ObservableProperty] private bool _enableFeralGameMode;

    [ObservableProperty] private bool _enableMangoHud;

    [ObservableProperty] private bool _useDedicatedGpu;
    
    public MiscConfigModel() { }

    public MiscConfigModel(string preLaunchCommand, string wrapperCommand, string postExitCommand, bool useCustomGlfw, string customGlfwPath, bool useCustomOpenAl, string customOpenAlPath, bool enableFeralGameMode, bool enableMangoHud, bool useDedicatedGpu)
    {
        _preLaunchCommand = preLaunchCommand;
        _wrapperCommand = wrapperCommand;
        _postExitCommand = postExitCommand;
        _useCustomGlfw = useCustomGlfw;
        _customGlfwPath = customGlfwPath;
        _useCustomOpenAl = useCustomOpenAl;
        _customOpenAlPath = customOpenAlPath;
        _enableFeralGameMode = enableFeralGameMode;
        _enableMangoHud = enableMangoHud;
        _useDedicatedGpu = useDedicatedGpu;
    }
}