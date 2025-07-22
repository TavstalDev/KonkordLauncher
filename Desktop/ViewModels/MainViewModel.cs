using System.Collections.ObjectModel;
using ReactiveUI;
using Tavstal.KonkordLauncher.Desktop.Enums;
using Tavstal.KonkordLauncher.Desktop.Models;

namespace Tavstal.KonkordLauncher.Desktop.ViewModels;

public class MainViewModel : ViewModelBase
{
    private ESidebarType _currentPageIndex = ESidebarType.Play;
    public ESidebarType CurrentPageIndex
    {
        get => _currentPageIndex;
        set => this.RaiseAndSetIfChanged(ref _currentPageIndex, value);
    }
    
    public ObservableCollection<PlayCardModel> Instances { get; } = [];
    
    public ObservableCollection<NewsCardModel> News { get; } = [];
    
    public ObservableCollection<AccountCardModel> Accounts { get; } = [];
}