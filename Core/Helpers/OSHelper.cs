using Tavstal.KonkordLauncher.Core.Enums;

namespace Tavstal.KonkordLauncher.Core.Helpers;

/// <summary>
/// Provides helper methods for operating system-related functionality.
/// </summary>
public static class OSHelper
{
    /// <summary>
    /// Determines the operating system type.
    /// </summary>
    /// <returns>
    /// An <see cref="EOperatingSystem"/> value representing the current operating system.
    /// </returns>
    public static EOperatingSystem GetOperatingSystem()
    {
        var platform = Environment.OSVersion.Platform;
        switch (platform)
        {
            case PlatformID.Win32NT:
            case PlatformID.Win32Windows:
            case PlatformID.Win32S:
            case PlatformID.WinCE:
            {
                return EOperatingSystem.Windows;
            }
            case PlatformID.Unix:
            {
                return EOperatingSystem.Linux;
            }
            case PlatformID.MacOSX:
            {
                return EOperatingSystem.MacOS;
            }
            default:
            {
                return EOperatingSystem.Unknown;
            }
        }
    }
    
    /// <summary>
    /// Retrieves the home directory path for the current user.
    /// </summary>
    /// <param name="os">
    /// Optional parameter specifying the operating system. If null, the current operating system is detected.
    /// </param>
    /// <returns>
    /// A string representing the path to the user's home directory.
    /// </returns>
    public static string GetHomeDirectory(EOperatingSystem? os = null)
    {
        if (os == null)
            os = GetOperatingSystem();

        switch (os)
        {
            case EOperatingSystem.Windows:
            {
                return Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            }
            case EOperatingSystem.Linux:
            case EOperatingSystem.MacOS:
            case EOperatingSystem.Unknown:
            {
                return Environment.GetEnvironmentVariable("HOME") ?? string.Empty;
            }
            default:
                throw new ArgumentOutOfRangeException(nameof(os));
        }
    }
    
    /// <summary>
    /// Retrieves the desktop directory path for the current user.
    /// </summary>
    /// <param name="os">
    /// Optional parameter specifying the operating system. If null, the current operating system is detected.
    /// </param>
    /// <returns>
    /// A string representing the path to the user's desktop directory.
    /// </returns>
    public static string GetDesktopDirectory(EOperatingSystem? os = null)
    {
        if (os == null)
            os = GetOperatingSystem();

        switch (os)
        {
            case EOperatingSystem.Windows:
            {
                return Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            }
            case EOperatingSystem.MacOS:
            {
                return Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            }
            case EOperatingSystem.Linux:
            case EOperatingSystem.Unknown:
            {
                var xdgDesktop = Environment.GetEnvironmentVariable("XDG_DESKTOP_DIR");
                if (!string.IsNullOrEmpty(xdgDesktop))
                    return xdgDesktop;
                
                string userHomeDir = GetHomeDirectory();
                string desktopDir = Path.Combine(userHomeDir, "Desktop"); // Fallback to "Desktop" in home directory
                
                var userDirsFilePath = Path.Combine(userHomeDir, ".config", "user-dirs.dirs");
                if (!File.Exists(userDirsFilePath))
                    return desktopDir; 
                
                string[] fileContent =  File.ReadAllLines(userDirsFilePath);
                foreach (string line in fileContent)
                {
                    if (!line.StartsWith("XDG_DESKTOP_DIR="))
                        continue;
                    
                    desktopDir = line.Split('=')[1].Trim('"');
                    break;
                }
                return  desktopDir;
            }
            default:
                throw new ArgumentOutOfRangeException(nameof(os));
        }
    }
    
    /// <summary>
    /// Retrieves the programs directory path for the current user.
    /// </summary>
    /// <param name="os">
    /// Optional parameter specifying the operating system. If null, the current operating system is detected.
    /// </param>
    /// <returns>
    /// A string representing the path to the user's programs directory.
    /// </returns>
    public static string GetProgramsDirectory(EOperatingSystem? os = null)
    {
        if (os == null)
            os = GetOperatingSystem();

        switch (os)
        {
            case EOperatingSystem.Windows:
            {
                return Environment.GetFolderPath(Environment.SpecialFolder.Programs);
            }
            case EOperatingSystem.MacOS:
            {
                // MacOS: Standard directory for applications.
                return "/Applications";
            }
            case EOperatingSystem.Linux:
            case EOperatingSystem.Unknown:
            {
                // Linux: Standard directory for user-specific applications.
                // ~/.local/share/applications
                string userHomeDir = GetHomeDirectory();
                return Path.Combine(userHomeDir, ".local", "share", "applications"); 
            }
            default:
                throw new ArgumentOutOfRangeException(nameof(os));
        }
    }
}