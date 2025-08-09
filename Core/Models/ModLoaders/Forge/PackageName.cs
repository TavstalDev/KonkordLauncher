namespace Tavstal.KonkordLauncher.Core.Models.ModLoaders.Forge;

/// <summary>
/// Represents a package name used in the Forge mod loader.
/// Provides methods to parse and manipulate package data.
/// </summary>
/// <remarks>
/// Source: https://github.com/CmlLib/CmlLib.Core.Installer.Forge
/// </remarks>
public class PackageName
{
    /// <summary>
    /// Stores the components of the package name.
    /// </summary>
    private readonly string[] names;

    /// <summary>
    /// Gets the component of the package name at the specified index.
    /// </summary>
    /// <param name="index">The index of the component to retrieve.</param>
    /// <returns>The component at the specified index.</returns>
    public string this[int index] => names[index];

    /// <summary>
    /// Gets the package identifier (first component of the package name).
    /// </summary>
    public string Package => names[0];

    /// <summary>
    /// Gets the name of the package (second component of the package name).
    /// </summary>
    public string Name => names[1];

    /// <summary>
    /// Gets the version of the package (third component of the package name).
    /// </summary>
    public string Version => names[2];

    /// <summary>
    /// Parses a package name string into a <see cref="PackageName"/> instance.
    /// </summary>
    /// <param name="name">The package name string to parse.</param>
    /// <returns>A <see cref="PackageName"/> instance representing the parsed package name.</returns>
    /// <exception cref="ArgumentNullException">Thrown if the provided name is null.</exception>
    /// <exception cref="ArgumentException">Thrown if the provided name is invalid.</exception>
    public static PackageName Parse(string name)
    {
        if (name == null)
        {
            throw new ArgumentNullException("name");
        }

        string[] array = name.Split(':');
        if (array.Length < 3)
        {
            throw new ArgumentException("invalid name");
        }

        return new PackageName(array);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PackageName"/> class with the specified components.
    /// </summary>
    /// <param name="names">The components of the package name.</param>
    private PackageName(string[] names)
    {
        this.names = names;
    }

    /// <summary>
    /// Gets the file path for the package with an optional native identifier and default extension.
    /// </summary>
    /// <returns>The file path for the package.</returns>
    public string GetPath()
    {
        return GetPath("");
    }

    /// <summary>
    /// Gets the file path for the package with the specified native identifier and default extension.
    /// </summary>
    /// <param name="nativeId">The native identifier to include in the file path.</param>
    /// <returns>The file path for the package.</returns>
    public string GetPath(string? nativeId)
    {
        return GetPath(nativeId, "jar");
    }

    /// <summary>
    /// Gets the file path for the package with the specified native identifier and file extension.
    /// </summary>
    /// <param name="nativeId">The native identifier to include in the file path.</param>
    /// <param name="extension">The file extension to use.</param>
    /// <returns>The file path for the package.</returns>
    public string GetPath(string? nativeId, string extension)
    {
        string text = string.Join("-", names, 1, names.Length - 1);
        if (!string.IsNullOrEmpty(nativeId))
        {
            text = text + "-" + nativeId;
        }

        text = text + "." + extension;
        return Path.Combine(GetDirectory(), text).Replace("@jar", ""); // NeoForge fix
    }

    /// <summary>
    /// Gets the directory path for the package.
    /// </summary>
    /// <returns>The directory path for the package.</returns>
    public string GetDirectory()
    {
        return Path.Combine(Package.Replace(".", "/"), Name, Version);
    }

    /// <summary>
    /// Gets the class path for the package.
    /// </summary>
    /// <returns>The class path for the package.</returns>
    public string GetClassPath()
    {
        return Package + "." + Name;
    }
}