using System.Diagnostics;
using Tavstal.KonkordLauncher.Core.Instances;
using Tavstal.KonkordLauncher.Core.Models;

namespace Tavstal.KonkordLauncher.Core.Services.Abstractions;


/// <summary>
/// Provides process lifecycle operations for launching and managing Minecraft instances.
/// </summary>
public interface IInstanceLaunchService
{
    /// <summary>
    /// Launches the specified Minecraft instance.
    /// </summary>
    /// <param name="instance">The instance to launch.</param>
    /// <param name="gameArgs">The fully resolved game arguments string to use for the launch.</param>
    /// <param name="jvmArgs">The fully resolved JVM arguments string to use for the launch.</param>
    /// <param name="customLogPath">Optional custom log file path to redirect the instance's standard output and error streams.</param>
    /// <param name="sensitiveDataToReplace">Optional list of sensitive data strings to redact from logs and process information.</param>
    /// <param name="progress">Optional progress reporter for launch-stage updates.</param>
    /// <param name="cancellationToken">Cancellation token observed during launch preparation and startup.</param>
    /// <returns>
    /// A task that resolves to the created <see cref="Process"/> when launch succeeds; otherwise, <see langword="null"/>.
    /// </returns>  
    Task<Process?> LaunchAsync(
        MinecraftInstance instance,
        string gameArgs,
        string jvmArgs,
        string? customLogPath = null,
        List<string>? sensitiveDataToReplace = null,
        IProgressReporter? progress = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Stops a running instance process by its instance identifier.
    /// </summary>
    /// <param name="instanceId">The unique identifier of the instance to stop.</param>
    /// <param name="cancellationToken">Cancellation token observed during shutdown.</param>
    /// <returns>A task that completes when the stop operation has finished.</returns>
    Task StopAsync(string instanceId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Determines whether the specified instance is currently running.
    /// </summary>
    /// <param name="instanceId">The unique identifier of the instance.</param>
    /// <returns><see langword="true"/> if the instance is currently tracked as running; otherwise, <see langword="false"/>.</returns>
    bool IsRunning(string instanceId);
}