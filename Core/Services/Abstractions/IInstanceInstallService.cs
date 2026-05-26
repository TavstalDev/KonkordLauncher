using Tavstal.KonkordLauncher.Core.Instances;
using Tavstal.KonkordLauncher.Core.Models;

namespace Tavstal.KonkordLauncher.Core.Services.Abstractions;

/// <summary>
/// Provides installation lifecycle operations for Minecraft instances.
/// </summary>
public interface IInstanceInstallService
{
    /// <summary>
    /// Installs all required runtime files and dependencies for the specified instance.
    /// </summary>
    /// <param name="instance">The Minecraft instance to install.</param>
    /// <param name="progress">Optional progress reporter for installation status updates.</param>
    /// <param name="cancellationToken">Cancellation token observed during installation.</param>
    /// <returns>
    /// A task that resolves to <see langword="true"/> if installation completed successfully; otherwise, <see langword="false"/>.
    /// </returns>
    Task<bool> InstallAsync(
        MinecraftInstance instance,
        IProgressReporter? progress = null,
        CancellationToken cancellationToken = default);
}