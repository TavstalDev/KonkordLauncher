using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using Tavstal.KonkordLauncher.Core.Models.Logging;
using Tavstal.KonkordLauncher.Desktop.Models.Avalonia;

namespace Tavstal.KonkordLauncher.Desktop.Views.Models.MainView;

/// <summary>
/// ViewModel for the "About" section of the main view.
/// </summary>
public partial class MainViewModel_About : KonkordObservableObject
{
    private readonly MainViewModel _parent;
        
    public string Version { get; } =  App.Version;
    public string Branch { get; } = App.Branch;
    public string BuildDate { get; } = App.BuildDate;

    [ObservableProperty]
    public partial string License { get; set; }

    /// <summary>
    /// Creates a new instance of <see cref="MainViewModel_About"/>.
    /// </summary>
    /// <param name="parent">Parent <see cref="MainViewModel"/> instance that owns this sub-view-model.</param>
    public MainViewModel_About(MainViewModel parent)
    {
        _parent = parent;
    }
    
    /// <summary>
    /// Asynchronously initializes the About view-model.
    /// Currently, this loads the LICENSE text from an embedded resource and normalizes whitespace for display.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> that may be used to cancel the asynchronous operation.
    ///</param>
    /// <returns>A <see cref="Task"/> that completes when initialization has finished.</returns>
    public async Task InitAsync(CancellationToken cancellationToken = default)
    {
        // Load LICENSE
        var assembly = Assembly.GetExecutingAssembly();
        await using var stream = assembly.GetManifestResourceStream("Tavstal.KonkordLauncher.Desktop.Assets.LICENSE");
        using var reader = new StreamReader(stream!);
        License = Regex.Replace((await reader.ReadToEndAsync(cancellationToken)).Trim(), @" {3,}", " ");
    }
}