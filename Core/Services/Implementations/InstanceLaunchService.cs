using System.Diagnostics;
using System.Text;
using Microsoft.Extensions.Logging;
using Tavstal.KonkordLauncher.Core.Enums;
using Tavstal.KonkordLauncher.Core.Helpers.Domain;
using Tavstal.KonkordLauncher.Core.Helpers.IO;
using Tavstal.KonkordLauncher.Core.Helpers.Platform;
using Tavstal.KonkordLauncher.Core.Instances;
using Tavstal.KonkordLauncher.Core.Models;
using Tavstal.KonkordLauncher.Core.Models.Logging;
using Tavstal.KonkordLauncher.Core.Services.Abstractions;

namespace Tavstal.KonkordLauncher.Core.Services.Implementations;

/// <inheritdoc/>
public class InstanceLaunchService : IInstanceLaunchService
{
    private readonly ICustomLogger _logger;
    private readonly Dictionary<string, Process> _runningProcesses = new();
    
    /// <summary>
    /// Initializes a new instance of the <see cref="InstanceLaunchService"/> class.
    /// </summary>
    /// <param name="logger">The logger used to record launch lifecycle events, process output, and error diagnostics.</param>
    public InstanceLaunchService(ICustomLogger<InstanceLaunchService> logger)
    {
        _logger = logger;
    }
    
    /// <inheritdoc/>
    public async Task<Process?> LaunchAsync(MinecraftInstance instance, 
        IProgressReporter? progress = null,
        CancellationToken cancellationToken = default)
    {
        var arguments = instance.ArgumentBuilder!.Build();
        string gameArguments = arguments.gameArgs;
        string jvmArguments = arguments.jvmArgs;
        var stopper = new Stopwatch();
        
        // Execute pre-launch command if specified
        if (!string.IsNullOrEmpty(instance.GameDetails.PreLaunchCommand))
        {
            var preLaunchProc = StartCommand(instance.GameDetails.PreLaunchCommand);
            if (preLaunchProc != null)
            {
                _logger.LogDebug("Executing pre-launch command...");
                stopper.Start();
                await preLaunchProc.WaitForExitAsync(cancellationToken);
                stopper.Stop();
                _logger.LogInformation($"Pre-launch command executed in {stopper.ElapsedMilliseconds}ms.");
            }
        }
            
        // Below 1.7 there is no dedicated logs directory
        // so this fixes this issue
        string? customLogPath = null;
        if (!GameHelper.isNewer(instance.GameDetails.MinecraftVersion, "1.7"))
        {
            string logsDir = Path.Combine(instance.VersionData.GameDir, "logs");
            Directory.CreateDirectory(logsDir);
            customLogPath = Path.Combine(logsDir, "latest.log");
        }

        // Launch the Minecraft game process with the constructed arguments

        List<string>? sensitiveDataToReplace = instance.Client is { IsOffline: false, AccessToken: not null } ? [instance.Client.AccessToken] : null;
        
        string finalJavaPath = string.IsNullOrEmpty(instance.GameDetails.JavaPath) ? "java" : instance.GameDetails.JavaPath;
        string args;
        bool useWrapper = false;
        switch (instance.GameDetails.Kind)
        {
            case EMinecraftKind.NEOFORGE:
            case EMinecraftKind.FORGE:
            {
                args = jvmArguments + " " + gameArguments;
                break;
            }
            case EMinecraftKind.VANILLA:
            case EMinecraftKind.FABRIC:
            case EMinecraftKind.QUILT:
            default:
            {
                args = jvmArguments;
                useWrapper = true;
                break;
            }
        }

        ProcessStartInfo psi;
        if (instance.GameDetails.WrapperCommands is { Count: > 0})
        {
            string fileName = instance.GameDetails.WrapperCommands[0];
            psi = new ProcessStartInfo
            {
                FileName = fileName,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false
            };
            bool commandInjected = false;
            foreach (var command in instance.GameDetails.WrapperCommands.Skip(1))
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
        foreach (var kvp in instance.GameDetails.EnvironmentVariables)
            psi.EnvironmentVariables[kvp.Key] = kvp.Value;
        
        // Log the process start details
        _logger.LogDebug("Starting Java process with arguments:");
        _logger.LogDebug("Java: " + finalJavaPath);
        _logger.LogDebug("FileName: " + psi.FileName);
        string argumentsToPrint = string.Join(" ", psi.ArgumentList);
#if DEBUG
        if (sensitiveDataToReplace != null)
        {
            foreach (var sen in sensitiveDataToReplace)
                argumentsToPrint = argumentsToPrint.Replace(sen, "*****");
        }
#endif
        _logger.LogDebug("Arguments: " + argumentsToPrint);

        // Handle existing logs file
        bool shouldHandleLogs = !string.IsNullOrEmpty(customLogPath);
        if (shouldHandleLogs)
        {
            string logsDir = Path.GetDirectoryName(customLogPath)!;
            if (File.Exists(customLogPath))
            {
                var lastWritten = File.GetLastWriteTime(customLogPath);
                string newPath = Path.Combine(logsDir, string.Format(PathHelper.LogsFileFormat, lastWritten));
                string archivePath = newPath + ".gz";
                File.Move(customLogPath, newPath, true);
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
                _logger.LogError($"[JVM Process Error] {e.Data}");
            };
            process.Exited += (_, _) =>
            {
                switch (process.ExitCode)
                {
                    case 0:
                        _logger.LogInformation($"[JVM Process Exit] {process.ExitCode} - Clean exit");
                        break;
                    case 1:
                        _logger.LogError($"[JVM Process Exit] {process.ExitCode} - JVM error (bad arguments, missing class, crash)");
                        break;
                    case -1:
                        _logger.LogWarning($"[JVM Process Exit] {process.ExitCode} - Forced exit / OutOfMemoryError");
                        break;
                    case 130:
                        _logger.LogWarning($"[JVM Process Exit] {process.ExitCode} - Terminated by user (SIGINT/Ctrl+C)");
                        break;
                    case 137:
                        _logger.LogError($"[JVM Process Exit] {process.ExitCode} - Killed by OS (OOM killer or SIGKILL)");
                        break;
                    case 139:
                        _logger.LogError($"[JVM Process Exit] {process.ExitCode} - Segmentation fault (native crash)");
                        break;
                    default:
                        if (process.ExitCode > 0)
                            _logger.LogError($"[JVM Process Exit] {process.ExitCode} - Abnormal exit");
                        else
                            _logger.LogWarning($"[JVM Process Exit] {process.ExitCode} - Unknown exit");
                        break;
                }
                
                if (!string.IsNullOrEmpty(instance.GameDetails.PostExitCommand))
                    StartCommand(instance.GameDetails.PostExitCommand);
            };
            if (shouldHandleLogs)
            {
                var jvmLogger = new CustomLogger("Game", LogLevel.Trace, false, customLogPath);
                bool replaceSD = sensitiveDataToReplace != null;
                process.OutputDataReceived += (_, e) =>
                {
                    if (string.IsNullOrEmpty(e.Data))
                        return;
                    string line = e.Data;
                    if (replaceSD)
                        foreach (string s in sensitiveDataToReplace!)
                            line = line.Replace(s, "*****");
                    
                    jvmLogger.LogInformation(line);
                };
                process.ErrorDataReceived += (_, e) =>
                {
                    if (string.IsNullOrEmpty(e.Data))
                        return;
                    string line = e.Data;
                    if (replaceSD)
                        foreach (string s in sensitiveDataToReplace!)
                            line = line.Replace(s, "*****");
                    jvmLogger.LogError(line);
                };
            }

            if (useWrapper)
            {
                var writer = process.StandardInput;
                string[] gameArgs = gameArguments.Split(' ');
                await writer.WriteLineAsync(gameArgs[0]); // Write the main class
                await writer.WriteLineAsync(
                    Convert.ToBase64String(
                        Encoding.UTF8.GetBytes(gameArgs.Skip(1)
                            .Aggregate((a, b) => a + " " + b)))); // Write the rest of the arguments as a single line
                await writer.FlushAsync(cancellationToken);
                writer.Close();
            }
        }
        
        _runningProcesses.Add(instance.Id, process!);
        return process;
    }

    /// <inheritdoc/>
    public async Task StopAsync(string instanceId, CancellationToken cancellationToken = default)
    {
        if (!_runningProcesses.TryGetValue(instanceId, out var process))
        {
            _logger.LogWarning($"Attempted to stop instance {instanceId} which is not currently running.");
            return;
        }

        if (process.HasExited)
        {
            _logger.LogInformation($"Instance {instanceId} process has already exited.");
            _runningProcesses.Remove(instanceId);
            return;
        }
        
        process.Close();
        await process.WaitForExitAsync(cancellationToken);
        _logger.LogDebug($"Instance {instanceId} stopped.");
        _runningProcesses.Remove(instanceId);
    }

    /// <inheritdoc/>
    public bool IsRunning(string instanceId) => _runningProcesses.TryGetValue(instanceId, out var process) && !process.HasExited;
    
    /// <summary>
    /// Starts a custom shell command as a child process using an OS-specific shell.
    /// </summary>
    /// <param name="command">
    /// The command line to execute. This value is passed to the selected shell (`cmd`, `zsh`, or `sh`)
    /// using that shell's "execute command" argument.
    /// </param>
    /// <param name="environmentVariables">Optional environment variables to inject into the child process before startup.</param>
    /// <returns>
    /// A <see cref="Process"/> instance for the started command if process creation succeeds; otherwise, <see langword="null"/>.
    /// </returns>
    private Process? StartCommand(string command, Dictionary<string, string>? environmentVariables = null)
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
            case EOperatingSystem.WINDOWS:
            {
                psi.FileName = "cmd.exe";
                psi.Arguments = $"/C \"{command}\"";
                break;
            }
            case EOperatingSystem.MACOS:
            {
                psi.FileName = "/bin/zsh";
                psi.Arguments = $"-c \"{command}\"";
                break;
            }
            case EOperatingSystem.UNKNOWN:
            case EOperatingSystem.LINUX:
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
                    _logger.LogDebug($"Custom command: {e.Data}");
            };
            process.ErrorDataReceived += (_, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                    _logger.LogDebug($"Custom command: {e.Data}");
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