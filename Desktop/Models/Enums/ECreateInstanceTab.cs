namespace Tavstal.KonkordLauncher.Desktop.Models.Enums;

/// <summary>
/// Tabs available in the "Create Instance" UI.
/// </summary>
public enum ECreateInstanceTab
{
    /// <summary>
    /// The "Custom" tab for creating a blank/custom instance where the user picks versions, loaders and settings.
    /// </summary>
    CUSTOM = 0,
    
    /// <summary>
    /// The "Modpack" tab for selecting and installing pre-built modpacks.
    /// </summary>
    MODPACK = 1,
    
    /// <summary>
    /// The "Import" tab for importing an instance from a file or URL.
    /// </summary>
    IMPORT = 2
}