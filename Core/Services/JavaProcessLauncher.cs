using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using Tavstal.KonkordLauncher.Core.Enums;
using Tavstal.KonkordLauncher.Core.Helpers.Platform;
using Tavstal.KonkordLauncher.Core.Models;

namespace Tavstal.KonkordLauncher.Core.Services;

/// <summary>
/// Provides functionality to launch Java processes with specified arguments.
/// </summary>
public static class JavaProcessLauncher
{
    // Logger instance for the JavaProcessLauncher module
    private static readonly CoreLogger _logger = CoreLogger.WithModuleType(typeof(JavaProcessLauncher));
    
    public static Process? StartJava(string javaPath, string jvmArguments, string gameArguments, string? logsPath = null, string? wrapperCommand = null, Dictionary<string, string>? environmentVariables = null)
    {
        string finalJavaPath = string.IsNullOrEmpty(javaPath) ? "java" : javaPath;

        ProcessStartInfo psi;
        if (!string.IsNullOrEmpty(wrapperCommand))
        {
            string cmdstr = finalJavaPath + " " + jvmArguments + " " + gameArguments;
            if (wrapperCommand.Contains("%command%"))
                wrapperCommand = wrapperCommand.Replace("%command%", cmdstr);
            else
                wrapperCommand += cmdstr;
            
            string[] cmd = wrapperCommand.Split(' ');
            psi = new ProcessStartInfo
            {
                FileName = cmd[0],
                Arguments = string.Join(' ', cmd.Skip(1)),
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false
            };
        }
        else
        {
            psi = new ProcessStartInfo
            {
                FileName = finalJavaPath,
                Arguments = jvmArguments + " " + gameArguments,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false
            };
        }
        
        // Add environment variables if provided
        if (environmentVariables != null)
        {
            foreach (var kvp in environmentVariables)
                psi.EnvironmentVariables[kvp.Key] = kvp.Value;
        }
        
        // Log the process start details
        _logger.Debug("Starting Java process with arguments:");
        _logger.Debug("Java: " + finalJavaPath);
        _logger.Debug("FileName: " + psi.FileName);
        _logger.Debug("Arguments: " + psi.Arguments);
        

        var process = Process.Start(psi);
        if (process != null)
        {
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            process.EnableRaisingEvents = true;
            process.Exited += (_, _) =>
            {
                _logger.Debug($"Java process exited with code: {process.ExitCode}");
            };
            /*process.OutputDataReceived += (sender, e) => Console.WriteLine("[JAVA-OUT] " + e.Data);
            process.ErrorDataReceived += (sender, e) => Console.WriteLine("[JAVA-ERR] " + e.Data);
            var writer = process.StandardInput;
            string[] gameArgs = gameArguments.Split(' ');
            writer.WriteLine(gameArgs[0]); // Write the main class or jar file first
            _logger.Info("Main class: " + gameArgs[0]);
            writer.WriteLine(Convert.ToBase64String(Encoding.UTF8.GetBytes(gameArgs.Skip(1).Aggregate((a, b) => a + " " + b)))); // Write the rest of the arguments as a single line
            writer.Flush();
            writer.Close();*/
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
    
    private static string QuoteForProcess(string s)
    {
        if (string.IsNullOrEmpty(s))
            return "\"\"";
        if (s.Contains(" ") || s.Contains("\""))
            return $"\"{s.Replace("\"", "\\\"")}\"";
        return s;
    }
}