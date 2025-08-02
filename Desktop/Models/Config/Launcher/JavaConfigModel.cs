using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using Tavstal.KonkordLauncher.Desktop.Models.Avalonia;

namespace Tavstal.KonkordLauncher.Desktop.Models.Config.Launcher;

public partial class JavaConfigModel : ObservableObject
{
    [ObservableProperty] private uint _minMemory;

    [ObservableProperty] private uint _maxMemory;

    [ObservableProperty] private uint _permaGen;

    [ObservableProperty] private string _defaultJavaPath;

    [ObservableProperty] private string _jvmArguments;

    [ObservableProperty] private ObservableDictionary<int, List<string>> _javaPaths;

    public JavaConfigModel() {}
    
    public JavaConfigModel(uint minMemory, uint maxMemory, uint permaGen, string defaultJavaPath, string jvmArguments, ObservableDictionary<int, List<string>> javaPaths)
    {
        _minMemory = minMemory;
        _maxMemory = maxMemory;
        _permaGen = permaGen;
        _defaultJavaPath = defaultJavaPath;
        _jvmArguments = jvmArguments;
        _javaPaths = javaPaths;
    }
}