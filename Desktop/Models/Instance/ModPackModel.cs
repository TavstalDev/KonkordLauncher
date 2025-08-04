using System.Collections.ObjectModel;
using Avalonia.Media.Imaging;

namespace Tavstal.KonkordLauncher.Desktop.Models.Instance;

public class ModPackModel
{
    public string Name { get; set; }
    public string Description { get; set; }
    public Bitmap? Icon { get; set; }
    public string RawPage { get; set; }
    public ObservableCollection<string> Versions { get; set; }
    public ObservableCollection<string> Tags { get; set; }
}