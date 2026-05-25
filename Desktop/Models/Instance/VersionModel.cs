using CommunityToolkit.Mvvm.ComponentModel;

namespace Tavstal.KonkordLauncher.Desktop.Models.Instance;

/// <summary>
/// Represents a version model containing version details such as version number, release date, and type.
/// </summary>
public partial class VersionModel : ObservableObject
{
    /// <summary>
    /// The version number of the instance.
    /// </summary>
    [ObservableProperty]
    public partial string Version { get; set; }

    /// <summary>
    /// The release date of the version.
    /// </summary>
    [ObservableProperty]
    public partial string ReleaseDate { get; set; }

    /// <summary>
    /// The type of the version (e.g., stable, beta, etc.).
    /// </summary>
    [ObservableProperty]
    public partial string Type { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="VersionModel"/> class.
    /// </summary>
    /// <param name="version">The version number of the instance.</param>
    /// <param name="releaseDate">The release date of the version.</param>
    /// <param name="type">The type of the version.</param>
    public VersionModel(string version, string releaseDate, string type)
    {
        Version = version;
        ReleaseDate = releaseDate;
        Type = type;
    }
}