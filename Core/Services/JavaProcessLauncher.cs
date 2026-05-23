using System.Diagnostics;
using System.Text;
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
    /// <param name="kind">The kind of Minecraft instance being launched.</param>
    /// <param name="logsPath">
    /// Optional path to a log file. If supplied, existing log file at this path will be rotated (moved/archived)
    /// before starting the process and the launched process' stdout/stderr lines will be written to a logger
    /// that targets this path. If null or empty, no log rotation or file logging is performed.
    /// </param>
    /// <param name="wrapperCommands"></param>
    /// <param name="environmentVariables">Optional dictionary of environment variables to set on the started process.</param>
    /// <param name="sensitiveDataToReplace">Optional list of sensitive substrings (e.g. tokens, passwords) that should be masked in logged output.</param>
    /// <returns>A <see cref="Process"/> instance representing the started process, or <c>null</c> if the process could not be started.</returns>
    public static Process? StartJava(string javaPath, string jvmArguments, string gameArguments, EMinecraftKind kind, string? logsPath = null, List<string>? wrapperCommands = null, Dictionary<string, string>? environmentVariables = null, List<string>? sensitiveDataToReplace = null)
    {
        string finalJavaPath = string.IsNullOrEmpty(javaPath) ? "java" : javaPath;
        string args;
        bool useWrapper = false;
        switch (kind)
        {
            case EMinecraftKind.NEOFORGE:
            case EMinecraftKind.FORGE:
            {
                args = jvmArguments + " " + gameArguments;
                break;
            }
            default:
            {
                args = jvmArguments;
                useWrapper = true;
                break;
            }
        }

        ProcessStartInfo psi;
        if (wrapperCommands is { Count: > 0})
        {
            string fileName = wrapperCommands[0];
            psi = new ProcessStartInfo
            {
                FileName = fileName,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false
            };
            bool commandInjected = false;
            foreach (var command in wrapperCommands.Skip(1))
            {
                if (command.Contains("%command%"))
                {
                    psi.ArgumentList.Add(command.Replace("%command%", finalJavaPath));
                    foreach (var arg in SplitArguments(args))
                        psi.ArgumentList.Add(arg);
                    commandInjected = true;
                    continue;
                }
                
                psi.ArgumentList.Add(command);
            }

            if (!commandInjected)
            {
                psi.ArgumentList.Add(finalJavaPath);
                foreach (var arg in SplitArguments(args))
                    psi.ArgumentList.Add(arg);
            }
        }
        else
        {
            psi = new ProcessStartInfo
            {
                FileName = finalJavaPath,
                Arguments = args,
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
        string argumentsToPrint = string.Join(" ", psi.ArgumentList);
#if DEBUG
        if (sensitiveDataToReplace != null)
        {
            foreach (var sen in sensitiveDataToReplace)
                argumentsToPrint = argumentsToPrint.Replace(sen, "*****");
        }
#endif
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
            process.ErrorDataReceived += (_, e) =>
            {
                if (string.IsNullOrEmpty(e.Data))
                    return;
                _logger.Error($"[JVM Process Error] {e.Data}");
            };
            process.Exited += (_, _) =>
            {
                switch (process.ExitCode)
                {
                    case 0:
                        _logger.Info($"[JVM Process Exit] {process.ExitCode} - Clean exit");
                        break;
                    case 1:
                        _logger.Error($"[JVM Process Exit] {process.ExitCode} - JVM error (bad arguments, missing class, crash)");
                        break;
                    case -1:
                        _logger.Warn($"[JVM Process Exit] {process.ExitCode} - Forced exit / OutOfMemoryError");
                        break;
                    case 130:
                        _logger.Warn($"[JVM Process Exit] {process.ExitCode} - Terminated by user (SIGINT/Ctrl+C)");
                        break;
                    case 137:
                        _logger.Error($"[JVM Process Exit] {process.ExitCode} - Killed by OS (OOM killer or SIGKILL)");
                        break;
                    case 139:
                        _logger.Error($"[JVM Process Exit] {process.ExitCode} - Segmentation fault (native crash)");
                        break;
                    default:
                        if (process.ExitCode > 0)
                            _logger.Error($"[JVM Process Exit] {process.ExitCode} - Abnormal exit");
                        else
                            _logger.Warn($"[JVM Process Exit] {process.ExitCode} - Unknown exit");
                        break;
                }
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
            }

            if (useWrapper)
            {
                var writer = process.StandardInput;
                string[] gameArgs = gameArguments.Split(' ');
                writer.WriteLine(gameArgs[0]); // Write the main class
                writer.WriteLine(
                    Convert.ToBase64String(
                        Encoding.UTF8.GetBytes(gameArgs.Skip(1)
                            .Aggregate((a, b) => a + " " + b)))); // Write the rest of the arguments as a single line
                writer.Flush();
                writer.Close();
            }
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
        var psi = new ProcessStartInfo
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
            process.OutputDataReceived += (_, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                    _logger.Debug($"Custom command: {e.Data}");
            };
            process.ErrorDataReceived += (_, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                    _logger.Error($"Custom command: {e.Data}");
            };
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
#endif
        }

        // Start the process and return the Process object
        return process;
    }
    
    /// <summary>
    /// Splits a command-line argument string into individual arguments while respecting quoted strings and escape sequences.
    /// </summary>
    /// /// <param name="args">The raw argument string to split, typically containing space-separated values with optional quotes and escapes.</param>
    /// <returns>An enumerable collection of parsed argument strings, with quotes removed and escape sequences resolved.</returns>
    private static IEnumerable<string> SplitArguments(string args)
    {
        var arguments = new List<string>();
        var currentArg = new StringBuilder();
        bool inQuotes = false;
        bool escapeNext = false;

        foreach (char c in args)
        {
            if (escapeNext)
            {
                currentArg.Append(c);
                escapeNext = false;
                continue;
            }

            if (c == '\\')
            {
                escapeNext = true;
                continue;
            }

            if (c == '"')
            {
                inQuotes = !inQuotes;
                continue;
            }

            if (c == ' ' && !inQuotes)
            {
                if (currentArg.Length > 0)
                {
                    arguments.Add(currentArg.ToString());
                    currentArg.Clear();
                }
                continue;
            }

            currentArg.Append(c);
        }

        if (currentArg.Length > 0)
            arguments.Add(currentArg.ToString());

        return arguments;
    }
}