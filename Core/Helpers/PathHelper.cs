namespace Tavstal.KonkordLauncher.Core.Helpers;

/// <summary>
/// Provides helper methods and properties for managing application paths.
/// </summary>
public static class PathHelper
{
    /// <summary>
    /// Gets the application directory path.
    /// In debug mode, it appends "LauncherDebug" to the current directory.
    /// </summary>
    public static string ApplicationDir
    {
        get
        {
#if DEBUG
            return Path.Combine(Directory.GetCurrentDirectory(), "LauncherDebug");
#else
            return Directory.GetCurrentDirectory();
#endif
        }
    }

    /// <summary>
    /// Gets the path to the launcher configuration file.
    /// </summary>
    public static readonly string LauncherConfigPath = Path.Combine(ApplicationDir, "config.json");

    /// <summary>
    /// Gets the path to the launcher accounts file.
    /// </summary>
    public static readonly string LauncherAccountsPath = Path.Combine(ApplicationDir, "accounts.json");
    
    /// <summary>
    /// Gets the path to the launcher instances file.
    /// </summary>
    public static readonly string LauncherInstancesPath = Path.Combine(ApplicationDir, "instances.json");
}