using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using Tavstal.KonkordLauncher.Core.Helpers;

namespace Tavstal.KonkordLauncher.Desktop.Models.Instance;

public partial class WorldModel : ObservableObject
{
    [ObservableProperty] private string _name;
    [ObservableProperty] private string _gamemode;
    [ObservableProperty] private string _lastPlayed;
    [ObservableProperty] [NotifyPropertyChangedFor(nameof(FormattedSize))] private long _size;
    [ObservableProperty] private Bitmap? _icon;

    public string FormattedSize => FileSystemHelper.GetFormatedSize(Size);
    
    public WorldModel(string name, string gamemode, string lastPlayed, long size, Bitmap? icon)
    {
        _name = name;
        _gamemode = gamemode;
        _lastPlayed = lastPlayed;
        _size = size;
        _icon = icon;
    }
}