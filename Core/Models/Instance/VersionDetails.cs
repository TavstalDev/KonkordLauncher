namespace Tavstal.KonkordLauncher.Core.Models.Instance;

/// <summary>
/// Represents the details of a specific version of the Minecraft launcher.
/// </summary>
public class VersionDetails
{
    public string MinecraftVersion { get; set; } = string.Empty;
    
    public string? CustomVersion { get; set; }
    
    public string VanillaVersionDirectory { get; set; } = string.Empty;

    public string? CustomVersionDirectory { get; set; }
    
    public string VanillaJsonPath { get; set; } = string.Empty;
    
    public string? CustomJsonPath { get; set; }

    public string VanillaJarPath { get; set; } = string.Empty;
    
    public string? CustomJarPath { get; set; } = string.Empty;
    
    public string GameDir { get; set; } = string.Empty;
    
    public string NativesDir { get; set; } = string.Empty;
}