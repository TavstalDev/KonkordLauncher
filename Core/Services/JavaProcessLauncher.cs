using System.Diagnostics;
using Tavstal.KonkordLauncher.Core.Enums;
using Tavstal.KonkordLauncher.Core.Helpers;
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
    /// Starts a Java process with the specified parameters and logs its output.
    /// </summary>
    /// <param name="javaPath">The path to the Java executable. If null or empty, "java" is used by default.</param>
    /// <param name="arguments">The arguments to pass to the Java process.</param>
    /// <param name="logFilePath">The path to the log file where the process output will be redirected.</param>
    /// <param name="wrapperCommand">
    /// An optional wrapper command to execute the Java process. If it contains "%command%", 
    /// it will be replaced with the Java executable path and arguments.
    /// </param>
    /// <param name="environmentVariables">
    /// An optional dictionary of environment variables to set for the process.
    /// </param>
    /// <returns>
    /// A <see cref="Process"/> object representing the started Java process, or null if the process could not be started.
    /// </returns>
    public static Process? StartJava(string javaPath, string arguments, string? logFilePath = null, string? wrapperCommand = null, Dictionary<string, string>? environmentVariables = null)
    {
        string finalJavaPath = string.IsNullOrEmpty(javaPath) ? "java" : javaPath;
    
        // Construct the full command string
        string fullCommand;
        if (!string.IsNullOrEmpty(wrapperCommand))
        {
            if (wrapperCommand.Contains("%command%"))
                fullCommand = wrapperCommand.Replace("%command%", finalJavaPath) + " " + arguments;
            else
                fullCommand = wrapperCommand + (wrapperCommand.EndsWith(" ") ? "" : " ") + finalJavaPath + " " + arguments;
        }
        else
            fullCommand = finalJavaPath + " " + arguments;
        
        fullCommand = fullCommand.Replace("\"", "\\\"");
        
        // Configure the process start information
        var psi = new ProcessStartInfo()
        {
#if DEBUG
            UseShellExecute = false,
            RedirectStandardError = true,
            RedirectStandardOutput = true,
#else
            UseShellExecute = true,
#endif
        };
        // Add environment variables if provided
        if (environmentVariables != null)
        {
            foreach (var kvp in environmentVariables)
                psi.EnvironmentVariables[kvp.Key] = kvp.Value;
        }
        
        if (!string.IsNullOrEmpty(logFilePath) && File.Exists(logFilePath))
            File.Delete(logFilePath);
        
        switch (OSHelper.GetOperatingSystem())
        {
            case EOperatingSystem.Windows:
            {
                psi.FileName = "cmd.exe";
                psi.Arguments = string.IsNullOrEmpty(logFilePath) ? 
                    $@"/C ""{fullCommand}""" 
                    : 
                    $@"/C ""{fullCommand} >> ""{logFilePath}"" 2>&1""";
                break;
            }
            case EOperatingSystem.MacOS:
            {
                psi.FileName = "/bin/zsh";
                psi.Arguments = string.IsNullOrEmpty(logFilePath) ? 
                    $@"-c ""{fullCommand}""" 
                    : 
                    $@"-c ""{fullCommand} >> ""{logFilePath}"" 2>&1""";
                break;
            }
            case EOperatingSystem.Unknown:
            case EOperatingSystem.Linux:
            {
                psi.FileName = "/bin/sh";
                psi.Arguments = string.IsNullOrEmpty(logFilePath) ? 
                    $@"-c ""{fullCommand}""" 
                    : 
                    $@"-c ""{fullCommand} >> '{logFilePath}' 2>&1""";
                break;
            }
        }
        
        // Log the process start details
        _logger.Debug($"Java Path: {javaPath}");
        _logger.Debug("Starting Java process with arguments:");
        _logger.Debug("FileName: " + psi.FileName);
        _logger.Debug("Arguments: " + psi.Arguments);
        //_logger.Debug("Log File Path: " + (string.IsNullOrEmpty(logFilePath) ? "No log file specified" : logFilePath));
        _logger.Debug($"\n# START OF JAVA ARGUMENTS#\n{arguments.Replace(" ", "\n")}\n# END OF JAVA ARGUMENTS#");
        

        var process = Process.Start(psi);
        if (process != null)
        {
            process.EnableRaisingEvents = true;
            process.Exited += (_, _) =>
            {
                _logger.Debug($"Java process exited with code: {process.ExitCode}");
            };
        }

        // Start the process and return the Process object
        return process;
    }
    
    /// <summary>
    /// Starts a process to execute a custom command with optional environment variables.
    /// </summary>
    /// <param name="command">The command to execute.</param>
    /// <param name="environmentVariables">
    /// An optional dictionary of environment variables to set for the process.
    /// </param>
    /// <returns>
    /// A <see cref="Process"/> object representing the started process, or null if the process could not be started.
    /// </returns>
    public static Process? StartCommand(string command, Dictionary<string, string>? environmentVariables = null)
    {
        // Configure the process start information
        var psi = new ProcessStartInfo()
        {
            UseShellExecute = false,
            RedirectStandardError = true,
            RedirectStandardOutput = true,
        };
        // Add environment variables if provided
        if (environmentVariables != null)
        {
            foreach (var kvp in environmentVariables)
                psi.EnvironmentVariables[kvp.Key] = kvp.Value;
        }

        switch (OSHelper.GetOperatingSystem())
        {
            case EOperatingSystem.Windows:
            {
                psi.FileName = "cmd.exe";
                psi.Arguments = $"/C \"{command}\"";
                break;
            }
            case EOperatingSystem.MacOS:
            {
                psi.FileName = "/bin/zsh";
                psi.Arguments = $"-c \"{command}\"";
                break;
            }
            case EOperatingSystem.Unknown:
            case EOperatingSystem.Linux:
            {
                psi.FileName = "/bin/sh";
                psi.Arguments = $"-c \"{command}\"";
                break;
            }
        }
        
        var process = Process.Start(psi);
        if (process != null)
        {
            process.EnableRaisingEvents = true;
#if DEBUG
            process.OutputDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    _logger.Debug($"Custom command: {e.Data}");
                }
            };
            process.ErrorDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    _logger.Error($"Custom command: {e.Data}");
                }
            };
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
#endif
        }

        // Start the process and return the Process object
        return process;
    }
}