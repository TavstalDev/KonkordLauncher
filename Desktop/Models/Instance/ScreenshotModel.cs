using System.Threading.Tasks;
using Avalonia.Media.Imaging;

namespace Tavstal.KonkordLauncher.Desktop.Models.Instance;

public class ScreenshotModel
{
    public string Name { get; set; }
    public Task<Bitmap?> Image { get; set; }
    public long Size { get; set; }
}