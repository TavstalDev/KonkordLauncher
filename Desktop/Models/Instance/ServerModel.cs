using System.Threading.Tasks;
using Avalonia.Media.Imaging;

namespace Tavstal.KonkordLauncher.Desktop.Models.Instance;

public class ServerModel
{
    public string Name { get; set; }
    public string Address { get; set; }
    public Task<Bitmap?> Icon { get; set; }
}