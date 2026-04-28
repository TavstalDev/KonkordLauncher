namespace Tavstal.KonkordLauncher.Desktop.Models.Enums;

/// <summary>
/// Represents the top-level settings tabs in the application's Settings UI.
/// </summary>
public enum ESettingsTab
{
    /// <summary>
    /// Launcher-specific settings (auto-update, language, theme, directories, etc.).
    /// </summary>
    LAUNCHER = 0,
    
    /// <summary>
    /// Minecraft-specific settings (window behavior, close-on-launch/exit, instance defaults).
    /// </summary>
    MINECRAFT = 1,
    
    /// <summary>
    /// Global Java settings (default Java path, memory settings, JVM arguments).
    /// </summary>
    JAVA = 2,
    
    /// <summary>
    /// Miscellaneous settings (custom commands, native libraries, performance toggles, preferences).
    /// </summary>
    MISC = 3
}