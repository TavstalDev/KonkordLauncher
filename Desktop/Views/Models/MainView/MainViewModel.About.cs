using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Tavstal.KonkordLauncher.Desktop.Models.Avalonia;

namespace Tavstal.KonkordLauncher.Desktop.Views.Models.MainView;

/// <summary>
///     The view model for the "About" page. It shows version and license information.
/// </summary>
public partial class MainViewModel_About : KonkordObservableObject
{
    /// <summary>
    ///     Gets the launcher version.
    /// </summary>
    public string Version { get; } = Program.Version;

    /// <summary>
    ///     Gets the name of the source branch that the build came from.
    /// </summary>
    public string Branch { get; } = Program.Branch;

    /// <summary>
    ///     Gets the date when the build was created.
    /// </summary>
    public string BuildDate { get; } = Program.BuildDate;

    /// <summary>
    ///     Gets or sets the license text.
    /// </summary>
    [ObservableProperty] public partial string License { get; set; } = string.Empty;

    /// <summary>
    ///     Loads the license text from the application resources.
    /// </summary>
    /// <param name="cancellationToken">A token that can cancel the operation.</param>
    /// <returns>A task that represents the operation.</returns>
    public async Task InitAsync(CancellationToken cancellationToken = default)
    {
        // Load LICENSE
        var assembly = Assembly.GetExecutingAssembly();
        await using var stream = assembly.GetManifestResourceStream("Tavstal.KonkordLauncher.Desktop.Assets.LICENSE");
        using var reader = new StreamReader(stream!);
        License = Regex.Replace((await reader.ReadToEndAsync(cancellationToken)).Trim(), " {3,}", " ");
    }
}
