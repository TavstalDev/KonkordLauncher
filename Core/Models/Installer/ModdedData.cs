using Tavstal.KonkordLauncher.Core.Models.MojangApi.Meta;

namespace Tavstal.KonkordLauncher.Core.Models.Installer;

/// <summary>
/// Represents the modded data required for the installation process.
/// </summary>
public class ModdedData
{
    /// <summary>
    /// Gets or sets the main class of the modded data.
    /// </summary>
    public string? MainClass { get; set; }

    /// <summary>
    /// Gets or sets the list of libraries required for the modded data.
    /// </summary>
    public List<LibraryMeta> Libraries { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ModdedData"/> class.
    /// </summary>
    public ModdedData() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="ModdedData"/> class with specified main class, version details, and libraries.
    /// </summary>
    /// <param name="mainClass">The main class of the modded data.</param>
    /// <param name="libraries">The list of libraries required for the modded data.</param>
    public ModdedData(string? mainClass, List<LibraryMeta> libraries)
    {
        MainClass = mainClass;
        Libraries = libraries;
    }
}