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
            FileName = string.IsNullOrEmpty(javaPath) ? "java" : javaPath,
            Arguments = arguments,
            UseShellExecute = false,
            RedirectStandardError = true,
            RedirectStandardOutput = true,
        };
        
        // Log the process start details
        _logger.Debug($"Java Path: {javaPath}");
        _logger.Debug("Starting Java process with arguments:");
        _logger.Debug($"\n# START OF ARGUMENTS#\n{arguments.Replace(" ", "\n")}\n# END OF ARGUMENTS#");

        var process = Process.Start(psi);
        if (process != null)
        {
            process.EnableRaisingEvents = true;
#if DEBUG
            process.OutputDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    _logger.Debug($"Java Output: {e.Data}");
                }
            };
            process.ErrorDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    _logger.Error($"Java Output: {e.Data}");
                }
            };
            process.Exited += (sender, e) => { _logger.Info($"Java process exited with code: {process.ExitCode}"); };

            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
#endif
        }

        // Start the process and return the Process object
        return process;
    }
}