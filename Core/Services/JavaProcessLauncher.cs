using System.Diagnostics;
using Tavstal.KonkordLauncher.Core.Enums;
using Tavstal.KonkordLauncher.Core.Helpers.IO;
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
    
    /// <summary>
    /// Starts a Java process using the specified java executable and arguments.
    /// </summary>
    /// <param name="javaPath">Path to the java executable to use. If null or empty, the system "java" command will be used.</param>
    /// <param name="jvmArguments">JVM arguments to pass to the Java executable (e.g. "-Xmx2G -Xms1G"). This string should contain any JVM flags required.</param>
    /// <param name="gameArguments">Game (application) arguments to pass after JVM arguments (e.g. "--username user --version 1.16.5").</param>
    /// <param name="logsPath">
    /// Optional path to a log file. If supplied, existing log file at this path will be rotated (moved/archived)
    /// before starting the process and the launched process' stdout/stderr lines will be written to a logger
    /// that targets this path. If null or empty, no log rotation or file logging is performed.
    /// </param>
    /// <param name="wrapperCommand">
    /// Optional wrapper command to run the Java command through. If provided, the constructed java command
    /// string (<c>javaPath + " " + jvmArguments + " " + gameArguments</c>) will be injected into the wrapper:
    /// - If the wrapper contains the literal <c>"%command%"</c> substring it will be replaced with the constructed command.
    /// </param>
    /// <param name="environmentVariables">Optional dictionary of environment variables to set on the started process.</param>
    /// <param name="sensitiveDataToReplace">Optional list of sensitive substrings (e.g. tokens, passwords) that should be masked in logged output.</param>
    /// <returns>A <see cref="Process"/> instance representing the started process, or <c>null</c> if the process could not be started.</returns>
    public static Process? StartJava(string javaPath, string jvmArguments, string gameArguments, string? logsPath = null, string? wrapperCommand = null, Dictionary<string, string>? environmentVariables = null, List<string>? sensitiveDataToReplace = null)
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
        string argumentsToPrint = psi.Arguments;
        if (sensitiveDataToReplace != null)
        {
            foreach (var sen in sensitiveDataToReplace)
                argumentsToPrint = argumentsToPrint.Replace(sen, "*****");
        }
        _logger.Debug("Arguments: " + argumentsToPrint);

        // Handle existing logs file
        bool shouldHandleLogs = !string.IsNullOrEmpty(logsPath);
        if (shouldHandleLogs)
        {
            string logsDir = Path.GetDirectoryName(logsPath)!;
            if (File.Exists(logsPath))
            {
                var lastWritten = File.GetLastWriteTime(logsPath);
                string newPath = Path.Combine(logsDir, string.Format(PathHelper.LogsFileFormat, lastWritten));
                string archivePath = newPath + ".gz";
                File.Move(logsPath, newPath, true);
                FileSystemHelper.CompressFile(newPath, archivePath);
            }
        }
        
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
            if (shouldHandleLogs)
            {
                CoreLogger jvmLogger = new CoreLogger("Game", false, logsPath);
                bool replaceSD = sensitiveDataToReplace != null;
                process.OutputDataReceived += (_, e) =>
                {
                    if (string.IsNullOrEmpty(e.Data))
                        return;
                    string line = e.Data;
                    if (replaceSD)
                        foreach (string s in sensitiveDataToReplace!)
                            line = line.Replace(s, "*****");
                    
                    jvmLogger.Info(line);
                };
                process.ErrorDataReceived += (_, e) =>
                {
                    if (string.IsNullOrEmpty(e.Data))
                        return;
                    string line = e.Data;
                    if (replaceSD)
                        foreach (string s in sensitiveDataToReplace!)
                            line = line.Replace(s, "*****");
                    jvmLogger.Error(line);
                };
                process.Exited += (_, e_) =>
                {
                    jvmLogger.Debug("JVM exited with code: " + process.ExitCode);
                };
            }
            
            // TODO: Implement launch wrapper
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
}