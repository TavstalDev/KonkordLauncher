using CommunityToolkit.Mvvm.ComponentModel;
using Tavstal.KonkordLauncher.Common.Models.Java;

namespace Tavstal.KonkordLauncher.Desktop.Models.Domain;

/// <summary>
/// Represents a model for a Java version, including its major version, full version string, 
/// architecture, and installation path.
/// </summary>
public partial class JavaVersionModel : ObservableObject
{
    /// <summary>
    /// The major version of the Java installation.
    /// </summary>
    [ObservableProperty]
    public partial int Major { get; set; }

    /// <summary>
    /// The full version string of the Java installation.
    /// </summary>
    [ObservableProperty]
    public partial string Version { get; set; }

    /// <summary>
    /// The architecture of the Java installation (e.g., x86, x64).
    /// </summary>
    [ObservableProperty]
    public partial string Architecture { get; set; }

    /// <summary>
    /// The file system path to the Java installation.
    /// </summary>
    [ObservableProperty]
    public partial string Path { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="JavaVersionModel"/> class.
    /// </summary>
    public JavaVersionModel() {}

    /// <summary>
    /// Initializes a new instance of the <see cref="JavaVersionModel"/> class with the specified properties.
    /// </summary>
    /// <param name="major">The major version of the Java installation.</param>
    /// <param name="version">The full version string of the Java installation.</param>
    /// <param name="architecture">The architecture of the Java installation.</param>
    /// <param name="path">The file system path to the Java installation.</param>
    public JavaVersionModel(int major, string version, string architecture, string path)
    {
        Major = major;
        Version = version;
        Architecture = architecture;
        Path = path;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="JavaVersionModel"/> class from an existing <see cref="JavaVersion"/> object.
    /// </summary>
    /// <param name="javaVersion">The <see cref="JavaVersion"/> object to copy properties from.</param>
    public JavaVersionModel(JavaVersion javaVersion)
    {
        Major = javaVersion.Major;
        Version = javaVersion.Version;
        Architecture = javaVersion.Architecture;
        Path = javaVersion.Path;
    }
}