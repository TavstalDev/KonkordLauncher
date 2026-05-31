using System.Diagnostics;
using System.Runtime.InteropServices;
using Hardware.Info;
using Microsoft.Extensions.Logging;
using Tavstal.KonkordLauncher.Core.Enums;
using Tavstal.KonkordLauncher.Core.Models.Logging;

namespace Tavstal.KonkordLauncher.Core.Helpers.Platform;

/// <summary>
/// Provides helper methods for operating system-related functionality.
/// </summary>
public static class OSHelper
{
    private static readonly ICustomLogger _logger = new CustomLogger(nameof(OSHelper), LogLevel.Error);
    private static readonly HardwareInfo _hardwareInfo = new();
    private const int Windows11MajorVersion = 10;
    private const int Windows11MinimumBuild = 22000;
    private static readonly string[] NvidiaKeywords = ["nvidia", "geforce", "quadro", "gtx", "rtx", "mx", "tesla", "h100"];
    private static readonly string[] AmdKeywords = ["amd", "radeon", "vega", "rx", "r9", "r7", "r5"];
    private static readonly string[] IntelKeywords = ["intel", "arc", "battlemage"];
    private static readonly string[] AppleKeywords = ["apple", "m1", "m2", "m3"];
    
    /// <summary>
    /// Determines the operating system type.
    /// </summary>
    /// <returns>
    /// An <see cref="EOperatingSystem"/> value representing the current operating system.
    /// </returns>
    public static EOperatingSystem GetOperatingSystem()
    {
        var platform = Environment.OSVersion.Platform;
        return platform switch
        {
            PlatformID.Win32NT or PlatformID.Win32Windows or PlatformID.Win32S or PlatformID.WinCE => EOperatingSystem
                .WINDOWS,
            PlatformID.Unix => EOperatingSystem.LINUX,
            PlatformID.MacOSX => EOperatingSystem.MACOS,
            _ => EOperatingSystem.UNKNOWN
        };
    }
    
    /// <summary>
    /// Determines if the operating system is Windows 11.
    /// </summary>
    /// <returns>
    /// A boolean value indicating whether the operating system is Windows 11.
    /// </returns>
    public static bool IsWindows11()
    {
        if (GetOperatingSystem() != EOperatingSystem.WINDOWS)
            return false;

        Version osVersion = Environment.OSVersion.Version;
        return osVersion.Major > Windows11MajorVersion || osVersion is { Major: Windows11MajorVersion, Build: >= Windows11MinimumBuild };
    }

    /// <summary>
    /// Determines if the operating system is ARM-based.
    /// </summary>
    /// <returns>
    /// A boolean value indicating whether the operating system architecture is ARM
    /// </returns>
    public static bool IsArmBased()
    {
        Architecture osArchitecture = RuntimeInformation.OSArchitecture;
        return osArchitecture == Architecture.Arm || osArchitecture == Architecture.Arm64;
    }

    /// <summary>
    /// Checks if the operating system is 64-bit.
    /// </summary>
    /// <returns>
    /// A boolean value indicating whether the operating system is 64-bit.
    /// </returns>
    public static bool Is64BitOperatingSystem() => Environment.Is64BitOperatingSystem;
    
    /// <summary>
    /// Retrieves the type and description of the dedicated GPU available on the system.
    /// </summary>
    /// <returns>
    /// A tuple containing:
    /// <br/>- A string representing the GPU type ("nvidia", "amd", "intel", or "apple") if detected.
    /// <br/>- A string representing the GPU description.
    /// Returns <c>null</c> if no dedicated GPU is detected.
    /// </returns>
    public static (string, string)? GetDedicatedGpuType()
    {
        _hardwareInfo.RefreshVideoControllerList();
        foreach (var gpu in _hardwareInfo.VideoControllerList)
        {
            var lowerName = gpu.Description.ToLowerInvariant();
            if (NvidiaKeywords.Any(lowerName.Contains))
                return ("nvidia", gpu.Description);
            
            if (AmdKeywords.Any(lowerName.Contains))
                return ("amd", gpu.Description);
            
            if (IntelKeywords.Any(lowerName.Contains))
                return ("intel", gpu.Description);
            
            if (AppleKeywords.Any(lowerName.Contains))
                return ("apple", gpu.Description);
        }
        
        return null;
    }

    /// <summary>
    /// Retrieves the total amount of physical RAM available on the system in bytes.
    /// </summary>
    /// <returns>
    /// A <see cref="ulong"/> value representing the total physical memory in bytes.
    /// </returns>
    public static ulong GetRamInBytes()
    {
        _hardwareInfo.RefreshMemoryStatus();
        return _hardwareInfo.MemoryStatus.TotalPhysical;
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
        os ??= GetOperatingSystem();

        switch (os)
        {
            case EOperatingSystem.WINDOWS:
            {
                return Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            }
            case EOperatingSystem.LINUX:
            case EOperatingSystem.MACOS:
            case EOperatingSystem.UNKNOWN:
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
        os ??= GetOperatingSystem();

        switch (os)
        {
            case EOperatingSystem.WINDOWS:
            {
                return Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            }
            case EOperatingSystem.MACOS:
            {
                return Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            }
            case EOperatingSystem.LINUX:
            case EOperatingSystem.UNKNOWN:
            {
                var xdgDesktop = Environment.GetEnvironmentVariable("XDG_DESKTOP_DIR");
                if (!string.IsNullOrEmpty(xdgDesktop))
                    return xdgDesktop;
                
                string userHomeDir = GetHomeDirectory();
                string desktopDir = Path.Combine(userHomeDir, "Desktop"); // Fallback to "Desktop" in home directory
                if (Directory.Exists(desktopDir))
                    return desktopDir;
                
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
        os ??= GetOperatingSystem();

        switch (os)
        {
            case EOperatingSystem.WINDOWS:
            {
                return Environment.GetFolderPath(Environment.SpecialFolder.Programs);
            }
            case EOperatingSystem.MACOS:
            {
                // MacOS: Standard directory for applications.
                return "/Applications";
            }
            case EOperatingSystem.LINUX:
            case EOperatingSystem.UNKNOWN:
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
    
    /// <summary>
    /// Opens the specified URL in the default web browser based on the operating system.
    /// </summary>
    /// <param name="url">The URL to be opened.</param>
    public static bool OpenUrl(string url)
    {
        try
        {
            ProcessStartInfo startInfo;
            switch (GetOperatingSystem())
            {
                case EOperatingSystem.WINDOWS:
                {
                    startInfo = new ProcessStartInfo(url)
                    {
                        UseShellExecute = true
                    };
                    break;
                }
                case EOperatingSystem.MACOS:
                {
                    startInfo = new ProcessStartInfo("open", url)
                    {
                        UseShellExecute = false
                    };
                    break;
                }
                case EOperatingSystem.LINUX:
                {
                    startInfo = new ProcessStartInfo("xdg-open", url)
                    {
                        UseShellExecute = false // xdg-open is the executable
                    };
                    break;
                }
                case EOperatingSystem.UNKNOWN:
                default:
                {
                    _logger.LogWarning("Unsupported operating system for opening URLs.");
                    return false;
                }
            }
        
            // Start the process
            Process.Start(startInfo);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Failed to open the website after installation:");
            return false;
        }
    }
}