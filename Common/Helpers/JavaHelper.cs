using System.Diagnostics;
using Tavstal.KonkordLauncher.Common.Models;
using Tavstal.KonkordLauncher.Core.Enums;
using Tavstal.KonkordLauncher.Core.Helpers;
using Tavstal.KonkordLauncher.Core.Models;

namespace Tavstal.KonkordLauncher.Common.Helpers;

/// <summary>
/// Provides helper methods for working with Java installations and versions.
/// </summary>
public static class JavaHelper
{
    /// <summary>
    /// Logger instance for the JavaHelper module.
    /// </summary>
    private static readonly CoreLogger _logger = CoreLogger.WithModuleType(typeof(JavaHelper));
    // TODO
    private static readonly List<JavaMirror> _javaMirrors = [
        // Windows
        new (7, "", EOperatingSystem.Windows),
        new (7, "", EOperatingSystem.Windows, true),
        new (8, "https://github.com/adoptium/temurin8-binaries/releases/download/jdk8u462-b08/OpenJDK8U-jdk_x64_windows_hotspot_8u462b08.zip", EOperatingSystem.Windows),
        new (8, "", EOperatingSystem.Windows, true),
        new (17, "https://github.com/adoptium/temurin17-binaries/releases/download/jdk-17.0.9%2B9/OpenJDK17U-jdk_x86-32_windows_hotspot_17.0.9_9.zip", EOperatingSystem.Windows),
        new (17, "", EOperatingSystem.Windows, true),
        new (21, "https://github.com/adoptium/temurin21-binaries/releases/download/jdk-21.0.8%2B9/OpenJDK21U-jdk_x64_windows_hotspot_21.0.8_9.zip", EOperatingSystem.Windows),
        new (21, "https://github.com/adoptium/temurin21-binaries/releases/download/jdk-21.0.8%2B9/OpenJDK21U-jdk_aarch64_windows_hotspot_21.0.8_9.zip", EOperatingSystem.Windows, true),
        // Linux
        new (7, "", EOperatingSystem.Linux),
        new (7, "", EOperatingSystem.Linux, true),
        new (8, "https://github.com/adoptium/temurin8-binaries/releases/download/jdk8u462-b08/OpenJDK8U-jdk_x64_linux_hotspot_8u462b08.tar.gz", EOperatingSystem.Linux),
        new (8, "https://github.com/adoptium/temurin8-binaries/releases/download/jdk8u462-b08/OpenJDK8U-jdk_aarch64_linux_hotspot_8u462b08.tar.gz", EOperatingSystem.Linux, true),
        new (17, "https://github.com/adoptium/temurin17-binaries/releases/download/jdk-17.0.9%2B9/OpenJDK17U-jdk_x64_linux_hotspot_17.0.9_9.tar.gz", EOperatingSystem.Linux),
        new (17, "https://github.com/adoptium/temurin17-binaries/releases/download/jdk-17.0.9%2B9/OpenJDK17U-jdk_aarch64_linux_hotspot_17.0.9_9.tar.gz", EOperatingSystem.Linux, true),
        new (21, "https://github.com/adoptium/temurin21-binaries/releases/download/jdk-21.0.8%2B9/OpenJDK21U-jdk_x64_linux_hotspot_21.0.8_9.tar.gz", EOperatingSystem.Linux),
        new (21, "https://github.com/adoptium/temurin21-binaries/releases/download/jdk-21.0.8%2B9/OpenJDK21U-jdk_aarch64_linux_hotspot_21.0.8_9.tar.gz", EOperatingSystem.Linux, true),
        // MacOS
        new (7, "", EOperatingSystem.MacOS),
        new (7, "", EOperatingSystem.MacOS, true),
        new (8, "https://github.com/adoptium/temurin8-binaries/releases/download/jdk8u462-b08/OpenJDK8U-jdk_x64_mac_hotspot_8u462b08.tar.gz", EOperatingSystem.MacOS),
        new (8, "", EOperatingSystem.MacOS, true),
        new (17, "https://github.com/adoptium/temurin17-binaries/releases/download/jdk-17.0.9%2B9/OpenJDK17U-jdk_x64_mac_hotspot_17.0.9_9.tar.gz", EOperatingSystem.MacOS),
        new (17, "https://github.com/adoptium/temurin17-binaries/releases/download/jdk-17.0.9%2B9/OpenJDK17U-jdk_aarch64_mac_hotspot_17.0.9_9.tar.gz", EOperatingSystem.MacOS, true),
        new (21, "https://github.com/adoptium/temurin21-binaries/releases/download/jdk-21.0.8%2B9/OpenJDK21U-jdk_x64_mac_hotspot_21.0.8_9.tar.gz", EOperatingSystem.MacOS),
        new (21, "https://github.com/adoptium/temurin21-binaries/releases/download/jdk-21.0.8%2B9/OpenJDK21U-jdk_aarch64_mac_hotspot_21.0.8_9.tar.gz", EOperatingSystem.MacOS, true),
    ];
    private static List<JavaVersion> _cachedJavaVersions = [];
    private static DateTime _cacheExpiration = DateTime.MinValue;

    private static readonly List<string> WindowsDirectories =
    [
        @"C:\Program Files\Java",
        @"C:\Program Files (x86)\Java",
        @"C:\ProgramData\Oracle\Java",
        @"C:\ProgramData\Microsoft\Windows\Start Menu\Programs\Java"
    ];

    private static readonly List<string> LinuxDirectories =
    [
        "/usr/lib/jvm",
        "/usr/java",
        "/opt/java",
        "/usr/local/java"
    ];

    private static readonly List<string> MacDirectories =
    [
        "/Library/Java/JavaVirtualMachines",
        "/System/Library/Java/JavaVirtualMachines"
    ];

    /// <summary>
    /// Checks if Java is installed on the system by attempting to execute the "java --version" command.
    /// </summary>
    /// <returns>True if Java is installed, otherwise false.</returns>
    public static bool IsJavaInstalled()
    {
        try
        {
            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = "java",
                Arguments = " --version",
                RedirectStandardError = true,
                UseShellExecute = false
            };

            using Process? pr = Process.Start(psi);
            if (pr == null)
            {
                _logger.Error("Failed to start Java process. Is Java installed?");
                return false;
            }
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.Exc("Failed to validate Java:");
            _logger.Error(ex.ToString());
            return false;
        }
    }

    /// <summary>
    /// Retrieves detailed information about a Java installation by executing the specified Java executable.
    /// </summary>
    /// <param name="path">The file path to the Java executable.</param>
    /// <returns>
    /// A <see cref="JavaVersion"/> object containing the Java version details, or null if the details could not be retrieved.
    /// </returns>
    public static JavaVersion? GetJavaVersionDetails(string path)
    {
        try
        {
            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = path,
                Arguments = "-XshowSettings:properties -version",
                RedirectStandardError = true, // Output goes to stderr
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using Process? process = Process.Start(psi);
            if (process == null)
                return null;

            string output = process.StandardError.ReadToEnd();
            process.WaitForExit();

            string? majorVersion = string.Empty;
            string? javaVersion = string.Empty;
            string? architecture = string.Empty;

            foreach (var line in output.Split('\n'))
            {
                if (line.Contains("java.specification.version ="))
                    majorVersion = line.Split('=')[1].Trim();
                
                if (line.Contains("java.version ="))
                    javaVersion = line.Split('=')[1].Trim();
                
                if (line.Contains("os.arch ="))
                    architecture = line.Split('=')[1].Trim();
            }

            if (majorVersion.StartsWith("1."))
            {
                string[] parts = majorVersion.Split('.');
                if (parts.Length > 1)
                {
                    majorVersion = parts[1];
                }
                else
                {
                    _logger.Warn($"Java version format '{majorVersion}' is unexpected, defaulting to 1.");
                    majorVersion = "1"; // Default to 1 if no version is found
                }
            }
            
            return new JavaVersion(int.Parse(majorVersion), javaVersion, architecture, path);
        }
        catch (Exception ex)
        {
            _logger.Exc("Failed to get Java version details:");
            _logger.Error(ex.ToString());
            return null;
        }
    }

    /// <summary>
    /// Searches for Java installations in common paths.
    /// </summary>
    /// <returns>A dictionary where the key is the Java version and the value is a list of installation paths.</returns>
    public static List<JavaVersion> LocateJavaInstallations(bool forceRefresh = false)
    {
        if (forceRefresh)
            _cacheExpiration = DateTime.MinValue;
        
        if (_cachedJavaVersions.Count > 0 && _cacheExpiration > DateTime.Now)
            return _cachedJavaVersions;
        
        List<JavaVersion> javaVersions = [];
        List<string> javaPaths = [];

        switch (OSHelper.GetOperatingSystem())
        {
            case EOperatingSystem.Windows:
            {
                javaPaths = GetWindowsJavaPaths();
                break;
            }
            case EOperatingSystem.MacOS:
            {
                javaPaths = GetMacJavaPaths();
                break;
            }
            case EOperatingSystem.Linux:
            case EOperatingSystem.Unknown:
            {
                javaPaths = GetLinuxJavaPaths();
                break;
            }
        }

        foreach (var path in javaPaths)
        {
            var versionDetails = GetJavaVersionDetails(path);
            if (versionDetails == null)
                continue;

            javaVersions.Add(versionDetails);
        }

        _cachedJavaVersions = javaVersions;
        _cacheExpiration = DateTime.Now.AddMinutes(10); // Cache for 10 minutes
        
        return javaVersions;
    }

    /// <summary>
    /// Retrieves the paths to Java installations on Windows systems by searching common directories.
    /// </summary>
    /// <returns>A list of file paths to Java executables found on Windows.</returns>
    private static List<string> GetWindowsJavaPaths()
    {
        List<string> paths = [];

        foreach (var dirPath in WindowsDirectories)
        {
            if (!Directory.Exists(dirPath))
                continue;

            var subDirs = Directory.GetDirectories(dirPath);
            foreach (var subDir in subDirs)
            {
                string javaPath = Path.Combine(subDir, "bin", "javaw.exe");
                if (!File.Exists(javaPath))
                    continue;

                paths.Add(javaPath);
            }
        }

        return paths;
    }

    /// <summary>
    /// Retrieves the paths to Java installations on Linux systems by searching common directories.
    /// </summary>
    /// <returns>A list of file paths to Java executables found on Linux.</returns>
    private static List<string> GetLinuxJavaPaths()
    {
        List<string> paths = [];

        foreach (var dirPath in LinuxDirectories)
        {
            if (!Directory.Exists(dirPath))
                continue;

            var subDirs = Directory.GetDirectories(dirPath);
            foreach (var subDir in subDirs)
            {
                string javaPath = Path.Combine(subDir, "bin", "java");
                if (!File.Exists(javaPath))
                    continue;

                paths.Add(javaPath);
            }
        }

        return paths;
    }

    /// <summary>
    /// Retrieves the paths to Java installations on macOS systems by searching common directories.
    /// </summary>
    /// <returns>A list of file paths to Java executables found on macOS.</returns>
    private static List<string> GetMacJavaPaths()
    {
        List<string> paths = [];

        foreach (var dirPath in MacDirectories)
        {
            if (!Directory.Exists(dirPath))
                continue;

            var subDirs = Directory.GetDirectories(dirPath);
            foreach (var subDir in subDirs)
            {
                string javaPath = Path.Combine(subDir, "bin", "java");
                if (!File.Exists(javaPath))
                    continue;

                paths.Add(javaPath);
            }
        }

        return paths;
    }
}