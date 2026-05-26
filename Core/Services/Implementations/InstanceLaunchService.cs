using System.Diagnostics;
using System.Text;
using Microsoft.Extensions.Logging;
using Tavstal.KonkordLauncher.Core.Enums;
using Tavstal.KonkordLauncher.Core.Helpers.IO;
using Tavstal.KonkordLauncher.Core.Instances;
using Tavstal.KonkordLauncher.Core.Models;
using Tavstal.KonkordLauncher.Core.Services.Abstractions;

namespace Tavstal.KonkordLauncher.Core.Services.Implementations;

public class InstanceLaunchService : IInstanceLaunchService
{
    private readonly ILogger _logger;
    private readonly Dictionary<string, Process> _runningProcesses = new();
    
    public InstanceLaunchService(ILogger<InstanceLaunchService> logger)
    {
        _logger = logger;
    }
    
    public async Task<Process?> LaunchAsync(MinecraftInstance instance, 
        string gameArguments, string jvmArguments, string? customLogPath = null,
        List<string>? sensitiveDataToReplace = null,
        IProgressReporter? progress = null,
        CancellationToken cancellationToken = default)
    {
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
            };
            if (shouldHandleLogs)
            {
                CoreLogger jvmLogger = new CoreLogger("Game", false, customLogPath);
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

    public async Task StopAsync(string instanceId, CancellationToken cancellationToken = default)
    {
        if (!_runningProcesses.TryGetValue(instanceId, out var process))
        {
            _logger.LogWarning("Attempted to stop instance {InstanceId} which is not currently running.", instanceId);
            return;
        }

        if (process.HasExited)
        {
            _logger.LogInformation("Instance {InstanceId} process has already exited.", instanceId);
            _runningProcesses.Remove(instanceId);
            return;
        }
        
        process.Close();
        await process.WaitForExitAsync(cancellationToken);
        _logger.LogDebug("Instance {InstanceId} stopped.", instanceId);
        _runningProcesses.Remove(instanceId);
    }

    public bool IsRunning(string instanceId) => _runningProcesses.TryGetValue(instanceId, out var process) && !process.HasExited;
    
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