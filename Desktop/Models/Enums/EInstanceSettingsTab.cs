namespace Tavstal.KonkordLauncher.Desktop.Models.Enums;

/// <summary>
/// Tabs available in the "Instance Settings" section of the UI.
/// </summary>
public enum EInstanceSettingsTab
{
    /// <summary>
    /// Java-related settings (memory, JVM args, Java path, permaGen, etc.).
    /// </summary>
    JAVA = 0,
    
    /// <summary>
    /// Game-specific settings (window size, start maximized, close behavior, game options).
    /// </summary>
    GAME = 1,
    
    /// <summary>
    /// Custom command settings (pre-launch, wrapper, post-exit commands).
    /// </summary>
    CUSTOM_COMMAND = 2,
    
    /// <summary>
    /// Environment variables and related configuration for the instance.
    /// </summary>
    ENVIRONMENT = 3,
    
    /// <summary>
    /// Miscellaneous settings (native libraries, performance toggles, preferences).
    /// </summary>
    MISC = 4
}