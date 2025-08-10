using System.Diagnostics;
using System.IO.Compression;
using System.Text;
using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.Tar;
using Tavstal.KonkordLauncher.Common.Models;
using Tavstal.KonkordLauncher.Common.Models.Java;
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

    private static JavaMirrorConfig? _mirrorConfig;
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
    /// Downloads a specific Java version and extracts it to the target directory.
    /// </summary>
    /// <param name="majorVersion">The major version of Java to download (e.g., 8, 11, 17).</param>
    /// <param name="targetPath">The directory where the downloaded Java version will be extracted.</param>
    /// <param name="progress">
    /// An optional progress reporter to track the download progress as a percentage.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result is a boolean indicating
    /// whether the download and extraction were successful.
    /// </returns>
    public static async Task<bool> DownloadJavaVersionAsync(int majorVersion, string targetPath,
        Progress<double>? progress = null)
    {
        try
        {
            if (_mirrorConfig == null)
            {
                if (!File.Exists(PathHelper.JavaMirrorsPath))
                {
                    _mirrorConfig = new JavaMirrorConfig();
                    await JsonHelper.WriteJsonFileAsync(PathHelper.JavaMirrorsPath, _mirrorConfig);
                }
                else
                {
                    _mirrorConfig = await JsonHelper.ReadJsonFileAsync<JavaMirrorConfig>(PathHelper.JavaMirrorsPath) ??
                                    new JavaMirrorConfig();
                }
            }

            EOperatingSystem operatingSystem = OSHelper.GetOperatingSystem();
            bool isArmBased = OSHelper.IsArmBased();
            var osMirror = operatingSystem switch
            {
                EOperatingSystem.Windows => _mirrorConfig.Windows,
                EOperatingSystem.Linux => _mirrorConfig.Linux,
                EOperatingSystem.MacOS => _mirrorConfig.Mac,
                _ => null
            };
            if (osMirror == null)
                return false;

            var javaMirror = majorVersion switch
            {
                7 => osMirror.Jdk7,
                8 => osMirror.Jdk8,
                17 => osMirror.Jdk17,
                21 => osMirror.Jdk21,
                _ => null
            };

            if (javaMirror == null)
                return false;

            string url = isArmBased ? javaMirror.Arm : javaMirror.X86_64;
            if (string.IsNullOrEmpty(url))
            {
                _logger.Warn(
                    $"No download URL found for Java {majorVersion} on {operatingSystem} OS {(isArmBased ? "arm" : "x64")}.");
                return false;
            }

            string tempDir = Path.Combine(Path.GetTempPath(), "konkordlauncher_java");
            try
            {
                Directory.CreateDirectory(tempDir);
                string extension = url.EndsWith(".zip") ? "zip" : "tar.gz";
                string zipFilePath = Path.Combine(tempDir, $"java_{majorVersion}.{extension}");
                await HttpHelper.DownloadFileAsync(url, zipFilePath, progress);

                if (!File.Exists(zipFilePath))
                {
                    _logger.Error($"Java download failed: {url}");
                    return false;
                }
                
                if (extension == "tar.gz")
                {
                    await using Stream inStream = File.OpenRead(zipFilePath);
                    await using Stream gzipStream = new GZipInputStream(inStream);
                    using TarArchive tarArchive = TarArchive.CreateInputTarArchive(gzipStream, Encoding.UTF8);
                    tarArchive.ExtractContents(targetPath);
                }
                else
                    ZipFile.ExtractToDirectory(zipFilePath, targetPath);
                
                

                // Make java executable
                if (operatingSystem != EOperatingSystem.Windows)
                {
                    
                }
            }
            finally
            {
                FileSystemHelper.DeleteDirectory(tempDir);
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.Exc($"Failed to download Java '{majorVersion}'.");
            _logger.Exc(ex.ToString());
            return false;
        }
    }

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
    /// Locates Java installations on the system, optionally refreshing the cache or searching a specific directory.
    /// </summary>
    /// <param name="instanceJavaDir">
    /// An optional directory path to search for Java installations. If null, default directories are used.
    /// </param>
    /// <param name="forceRefresh">
    /// A boolean indicating whether to force a refresh of the cached Java installations.
    /// </param>
    /// <returns>
    /// A list of <see cref="JavaVersion"/> objects representing the located Java installations.
    /// </returns>
    public static List<JavaVersion> LocateJavaInstallations(string? instanceJavaDir = null, bool forceRefresh = false)
    {
        if (forceRefresh)
        {
            _cachedJavaVersions = [];
            _cacheExpiration = DateTime.MinValue;
        }

        if (_cachedJavaVersions.Count > 0 && _cacheExpiration > DateTime.Now)
            return _cachedJavaVersions;
        
        List<JavaVersion> javaVersions = [];
        List<string> javaPaths = [];

        switch (OSHelper.GetOperatingSystem())
        {
            case EOperatingSystem.Windows:
            {
                javaPaths = GetWindowsJavaPaths(instanceJavaDir);
                break;
            }
            case EOperatingSystem.MacOS:
            {
                javaPaths = GetMacJavaPaths(instanceJavaDir);
                break;
            }
            case EOperatingSystem.Linux:
            case EOperatingSystem.Unknown:
            {
                javaPaths = GetLinuxJavaPaths(instanceJavaDir);
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
    /// Retrieves the paths to Java installations on Windows systems.
    /// </summary>
    /// <param name="instanceJavaDir">
    /// An optional directory path to search for Java installations. If null, default directories are used.
    /// </param>
    /// <returns>
    /// A list of file paths to Java executables found in the specified or default directories.
    /// </returns>
    private static List<string> GetWindowsJavaPaths(string? instanceJavaDir = null)
    {
        List<string> paths = [];
        List<string> localDirs = [];
        if (!string.IsNullOrEmpty(instanceJavaDir))
            localDirs.Add(instanceJavaDir);
        localDirs.AddRange(WindowsDirectories);

        foreach (var dirPath in localDirs)
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
    /// Retrieves the paths to Java installations on Linux systems.
    /// </summary>
    /// <param name="instanceJavaDir">
    /// An optional directory path to search for Java installations. If null, default directories are used.
    /// </param>
    /// <returns>
    /// A list of file paths to Java executables found in the specified or default directories.
    /// </returns>
    private static List<string> GetLinuxJavaPaths(string? instanceJavaDir = null)
    {
        List<string> paths = [];
        List<string> localDirs = [];
        if (!string.IsNullOrEmpty(instanceJavaDir))
            localDirs.Add(instanceJavaDir);
        localDirs.AddRange(LinuxDirectories);

        foreach (var dirPath in localDirs)
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
    /// Retrieves the paths to Java installations on macOS systems.
    /// </summary>
    /// <param name="instanceJavaDir">
    /// An optional directory path to search for Java installations. If null, default directories are used.
    /// </param>
    /// <returns>
    /// A list of file paths to Java executables found in the specified or default directories.
    /// </returns>
    private static List<string> GetMacJavaPaths(string? instanceJavaDir = null)
    {
        List<string> paths = [];
        List<string> localDirs = [];
        if (!string.IsNullOrEmpty(instanceJavaDir))
            localDirs.Add(instanceJavaDir);
        localDirs.AddRange(MacDirectories);

        foreach (var dirPath in localDirs)
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