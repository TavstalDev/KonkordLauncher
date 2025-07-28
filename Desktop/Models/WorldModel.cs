using System.Threading.Tasks;
using Avalonia.Media.Imaging;

namespace Tavstal.KonkordLauncher.Desktop.Models;

public class WorldModel
{
    public Task<Bitmap?> Icon { get; set; }
    public string Name { get; set; }
    public string Gamemode { get; set; }
    public string LastPlayed { get; set; }
    public long Size { get; set; }
}