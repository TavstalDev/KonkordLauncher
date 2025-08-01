using System.Diagnostics;
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

    private const string _java8 = "1.8";
    private const string _java11 = "11";
    private const string _java17 = "17";
    private const string _java21 = "21";
    
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

            /*
            string output = pr.StandardError.ReadToEnd();
            string javaVersion = output.Split(' ')[2].Replace("\"", "");
            string rawMajorVersion = javaVersion.Split(".")[0];
            int major = int.Parse(rawMajorVersion);*/
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
    /// Validates if a specific Java version is installed.
    /// </summary>
    /// <param name="major">The major version of Java to validate.</param>
    /// <returns>Throws a NotImplementedException as the method is not yet implemented.</returns>
    public static bool ValidateJavaVersion(int major)
    {
        throw new NotImplementedException("Java version not implemented");
    }

    /// <summary>
    /// Searches for Java installations in common paths.
    /// </summary>
    /// <returns>A dictionary where the key is the Java version and the value is a list of installation paths.</returns>
    public static Dictionary<int, List<string>> LocateJavaInstallations()
    {
        throw new NotImplementedException("Java installations not implemented");
    }
}