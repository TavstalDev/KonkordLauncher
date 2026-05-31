namespace Tavstal.KonkordLauncher.Core.Models.Instance;

/// <summary>
/// Represents the details of a specific version of the Minecraft launcher.
/// </summary>
public class VersionDetails
{
    /// <summary>
    /// The Minecraft version identifier (e.g. "1.20.1") used by this instance.
    /// Defaults to an empty string when not set.
    /// </summary>
    public string MinecraftVersion { get; set; } = string.Empty;
    
    /// <summary>
    /// Optional custom version label (for modpacks or user-provided versions).
    /// When present, many operations should prefer the custom version over the vanilla version.
    /// </summary>
    public string? CustomVersion { get; set; }
    
    /// <summary>
    /// Directory containing the vanilla (Mojang-provided) version files for this Minecraft version.
    /// Typically used as the source for vanilla json/jar paths.
    /// </summary>
    public string VanillaVersionDirectory { get; set; } = string.Empty;

    /// <summary>
    /// Optional directory for a custom version (if a modded/custom jar and json are used).
    /// If null, no custom version directory is configured.
    /// </summary>
    public string? CustomVersionDirectory { get; set; }
    
    /// <summary>
    /// Path to the vanilla version JSON (version manifest) file.
    /// This is usually located inside <see cref="VanillaVersionDirectory"/>.
    /// </summary>
    public string VanillaJsonPath { get; set; } = string.Empty;
    
    /// <summary>
    /// Optional path to the custom version JSON file (if using a custom version).
    /// </summary>
    public string? CustomJsonPath { get; set; }

    /// <summary>
    /// Path to the vanilla JAR file for this version. Used when launching the vanilla client.
    /// </summary>
    public string VanillaJarPath { get; set; } = string.Empty;
    
    /// <summary>
    /// Optional path to a custom JAR file (used when a custom/modded jar should be launched).
    /// If present, this will typically be preferred over <see cref="VanillaJarPath"/>.
    /// </summary>
    public string? CustomJarPath { get; set; } = string.Empty;
    
    /// <summary>
    /// The instance's game directory (where runtime files, saves and configuration are stored).
    /// </summary>
    public string GameDir { get; set; } = string.Empty;
    
    /// <summary>
    /// Directory used to store native libraries extracted for the JVM.
    /// </summary>
    public string NativesDir { get; set; } = string.Empty;
}