namespace Tavstal.KonkordLauncher.Desktop.Models.Enums;

/// <summary>
/// Tabs available in the "Edit Instance" UI.
/// </summary>
public enum EEditInstanceTab
{
    /// <summary>
    /// The "Logs" tab: shows the latest log output and a list of log files for the instance.
    /// </summary>
    LOGS = 0,
    
    /// <summary>
    /// The "Mods" tab: lists installed mods and allows enabling/disabling, adding or removing mods.
    /// </summary>
    MODS = 1,
    
    /// <summary>
    /// The "Resource Packs" tab: lists available resource packs (textures, lang files, etc.) for the instance.
    /// </summary>
    RESOURCE_PACKS = 2,
    
    /// <summary>
    /// The "Shader Packs" tab: lists shader packs available to the instance and provides download/install controls.
    /// </summary>
    SHADER_PACKS = 3,
    
    /// <summary>
    /// The "Worlds" tab: shows saved worlds for the instance and provides actions such as open, delete or export.
    /// </summary>
    WORLDS = 4,
    
    /// <summary>
    /// The "Servers" tab: lists configured multiplayer servers for quick join or management.
    /// </summary>
    SERVERS = 5,
    
    /// <summary>
    /// The "Screenshots" tab: displays screenshots saved by the instance with options to view or remove.
    /// </summary>
    SCREENSHOTS = 6,
    
    /// <summary>
    /// The "Settings" tab: instance-specific settings such as JVM args, memory, window options, and launch options.
    /// </summary>
    SETTINGS = 7
}