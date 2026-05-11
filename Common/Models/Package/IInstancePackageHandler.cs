using Tavstal.KonkordLauncher.Core.Models;
using Tavstal.KonkordLauncher.Core.Models.Instance;

namespace Tavstal.KonkordLauncher.Common.Models.Package;

/// <summary>
/// Handles importing and exporting instance packages.
/// </summary>
public interface IInstancePackageHandler
{
    /// <summary>
    /// Import an instance package from the given source path (file or URL).
    /// Returns the imported Instance on success, or null on failure.
    /// </summary>
    Task<Instance?> ImportAsync(string sourcePath, Resolution resolution, string? customName = null, string? customGroup = null, IProgressReporter? progress = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Export the supplied Instance to a target path (e.g. .zip, .mrpack).
    /// Returns true on success; false on failure.
    /// </summary>
    Task<bool> ExportAsync(Instance instance, string targetPath, string exportVersion = "1.0.0", string summary = "", IProgressReporter? progress = null, CancellationToken cancellationToken = default);
}