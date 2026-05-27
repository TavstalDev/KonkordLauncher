using Tavstal.KonkordLauncher.Core.Models;

namespace Tavstal.KonkordLauncher.Common.Services.Abstractions;

/// <summary>
/// Provides validation operations for launcher state and required data files.
/// </summary>
public interface IValidationService
{
    /// <summary>
    /// Validates the launcher directory structure and required files.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token observed during validation.</param>
    /// <returns>A task that resolves to <see langword="true"/> if the launcher directory is valid; otherwise, <see langword="false"/>.</returns>
    Task<bool> ValidateLauncherDirectoryAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates the stored launcher account data.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token observed during validation.</param>
    /// <returns>A task that resolves to <see langword="true"/> if the account data is valid; otherwise, <see langword="false"/>.</returns>
    Task<bool> ValidateAccounts(CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates the cached or downloaded manifests required by the launcher.
    /// </summary>
    /// <param name="progressReporter">Optional progress reporter used to report validation progress.</param>
    /// <param name="cancellationToken">Cancellation token observed during validation.</param>
    /// <returns>A task that resolves to <see langword="true"/> if all manifests are valid; otherwise, <see langword="false"/>.</returns>
    Task<bool> ValidateManifests(IProgressReporter? progressReporter = null,
        CancellationToken cancellationToken = default);
}