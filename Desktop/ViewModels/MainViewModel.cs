using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using ReactiveUI;
using Tavstal.KonkordLauncher.Common.Helpers;
using Tavstal.KonkordLauncher.Desktop.Models;
using Tavstal.KonkordLauncher.Desktop.Models.Config.Launcher;
using Tavstal.KonkordLauncher.Desktop.Models.Enums;

namespace Tavstal.KonkordLauncher.Desktop.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private bool _isInitialized;
    
    [ObservableProperty]
    private ESidebarType _currentPageIndex;

    public ObservableCollection<PlayCardModel> Instances { get; } = [];
    
    public ObservableCollection<NewsCardModel> News { get; } = [];
    
    public ObservableCollection<AccountCardModel> Accounts { get; } = [];
    
    [ObservableProperty]
    private CoreConfigModel _coreConfig;

    public MainViewModel()
    {
        _currentPageIndex = ESidebarType.Play;
        // TODO: Load instances
        // TODO: Fetch news
        // TODO: Load accounts

        _coreConfig = new CoreConfigModel(LauncherHelper.GetLauncherSettings());
        _isInitialized = true;
    }

    partial void OnCoreConfigChanged(CoreConfigModel value)
    {
        if (!_isInitialized)
            return;
        
        // TODO: Save the core config to the settings file
    }
}