using CommunityToolkit.Mvvm.ComponentModel;
using Tavstal.BeaconSDK.Models.Java;

namespace Tavstal.KonkordLauncher.Desktop.Models.Domain;

/// <summary>
///     A Java version with its major version, full version text, architecture, and path.
/// </summary>
public partial class JavaVersionModel : ObservableObject
{
    /// <summary>
    ///     Creates an empty Java version model.
    /// </summary>
    public JavaVersionModel() { }

    /// <summary>
    ///     Creates a Java version model with the given values.
    /// </summary>
    /// <param name="major">The major version.</param>
    /// <param name="version">The full version text.</param>
    /// <param name="architecture">The architecture.</param>
    /// <param name="path">The install path.</param>
    public JavaVersionModel(int major, string version, string architecture, string path)
    {
        Major = major;
        Version = version;
        Architecture = architecture;
        Path = path;
    }

    /// <summary>
    ///     Creates a Java version model from an existing <see cref="JavaVersion" />.
    /// </summary>
    /// <param name="javaVersion">The Java version to copy.</param>
    public JavaVersionModel(JavaVersion javaVersion)
    {
        Major = javaVersion.Major;
        Version = javaVersion.Version;
        Architecture = javaVersion.Architecture;
        Path = javaVersion.Path;
    }

    /// <summary>
    ///     Gets or sets the major version.
    /// </summary>
    [ObservableProperty]
    public partial int Major { get; set; }

    /// <summary>
    ///     Gets or sets the full version text.
    /// </summary>
    [ObservableProperty]
    public partial string Version { get; set; }

    /// <summary>
    ///     Gets or sets the architecture, such as x86 or x64.
    /// </summary>
    [ObservableProperty]
    public partial string Architecture { get; set; }

    /// <summary>
    ///     Gets or sets the Java install path.
    /// </summary>
    [ObservableProperty]
    public partial string Path { get; set; }
}
