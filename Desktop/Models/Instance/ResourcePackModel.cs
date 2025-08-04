using System.Threading.Tasks;
using Avalonia.Media.Imaging;

namespace Tavstal.KonkordLauncher.Desktop.Models.Instance;

public class ResourcePackModel
{
    public bool IsEnabled { get; set; }
    public string Name { get; set; }
    public Task<Bitmap?> Icon { get; set; }
    public string Version { get; set; }
    public string LastModified { get; set; }
    public string Provider { get; set; }
    public long Size { get; set; }
}