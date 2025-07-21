using System.Collections.ObjectModel;
using ReactiveUI;
using Tavstal.KonkordLauncher.Desktop.Enums;
using Tavstal.KonkordLauncher.Desktop.Models;

namespace Tavstal.KonkordLauncher.Desktop.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private ESidebarType _currentPageIndex = ESidebarType.Play;
    public ESidebarType CurrentPageIndex
    {
        get => _currentPageIndex;
        set => this.RaiseAndSetIfChanged(ref _currentPageIndex, value);
    }
    
    public ObservableCollection<PlayCardModel> InstancesOnPlayPage { get; } = [];
}