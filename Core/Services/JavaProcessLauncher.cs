using System.Diagnostics;
using Tavstal.KonkordLauncher.Core.Models;

namespace Tavstal.KonkordLauncher.Core.Services;

/// <summary>
/// Provides functionality to launch Java processes with specified arguments.
/// </summary>
public static class JavaProcessLauncher
{
    // Logger instance for the JavaProcessLauncher module
    private static readonly CoreLogger _logger = CoreLogger.WithModuleType(typeof(JavaProcessLauncher));

    /// <summary>
    /// Starts a Java process with the given executable path and arguments.
    /// </summary>
    /// <param name="javaPath">The full path to the Java executable.</param>
    /// <param name="arguments">The command-line arguments to pass to the Java process.</param>
    /// <returns>
    /// A <see cref="Process"/> object representing the started Java process, or null if the process could not be started.
    /// </returns>
    public static Process? StartJava(string javaPath, string arguments)
    {
        // Configure the process start information
        var psi = new ProcessStartInfo()
        {
            FileName = javaPath,
            Arguments = arguments,
            UseShellExecute = false,
            RedirectStandardError = true,
        };

        // Log the process start details
        _logger.Debug("Starting Java process with arguments:");
        _logger.Debug(arguments.Replace(' ', '\n'));

        // Start the process and return the Process object
        return Process.Start(psi);
    }
}