using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Tavstal.KonkordLauncher.Desktop.Models.Avalonia;

namespace Tavstal.KonkordLauncher.Desktop.Views.Models.MainView;

public partial class MainViewModel_About : KonkordObservableObject
{
    public string Version { get; } = Program.Version;
    public string Branch { get; } = Program.Branch;
    public string BuildDate { get; } = Program.BuildDate;

    [ObservableProperty]
    public partial string License { get; set; } = string.Empty;

    public async Task InitAsync(CancellationToken cancellationToken = default)
    {
        // Load LICENSE
        var assembly = Assembly.GetExecutingAssembly();
        await using var stream = assembly.GetManifestResourceStream("Tavstal.KonkordLauncher.Desktop.Assets.LICENSE");
        using var reader = new StreamReader(stream!);
        License = Regex.Replace((await reader.ReadToEndAsync(cancellationToken)).Trim(), " {3,}", " ");
    }
}
