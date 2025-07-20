using Avalonia.Media.Imaging;
using Tavstal.KonkordLauncher.Desktop.Helpers;

namespace Tavstal.KonkordLauncher.Desktop.Models;

public class PlayCardModel
{
    public string Title { get; set; }
    public Bitmap? IconPath { get; set; } = ImageHelper.Load("avares://Desktop/Assets/Icons/dirt.png").Result;
    public string LaunchText { get; set; } = "Launch";
}