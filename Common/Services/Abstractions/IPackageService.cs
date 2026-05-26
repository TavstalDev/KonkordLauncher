using Tavstal.KonkordLauncher.Common.Models;
using Tavstal.KonkordLauncher.Common.Models.Package;
using Tavstal.KonkordLauncher.Core.Models;
using Tavstal.KonkordLauncher.Core.Models.Instance;

namespace Tavstal.KonkordLauncher.Common.Services.Abstractions;

/// <summary>
/// Defines operations for importing and exporting launcher instances as packages.
/// </summary>
public interface IPackageService
{
    /// <summary>
    /// Imports a package archive and converts it into a launcher instance.
    /// </summary>
    /// <param name="sourcePath">The path to the package archive file (e.g., <c>.mrpack</c>).</param>
    /// <param name="resolution">The screen resolution used to initialize the instance's window size.</param>
    /// <param name="customName">Optional custom name for the instance; if <see langword="null"/>, the name is read from the package metadata.</param>
    /// <param name="customGroup">Optional custom group assignment for the instance.</param>
    /// <param name="customIconUrl">Optional URL to download a custom icon for the instance.</param>
    /// <param name="progress">Optional progress reporter to track import operations (file downloads, extraction, etc.).</param>
    /// <param name="cancellationToken">Cancellation token observed during import operations.</param>
    /// <returns>
    /// A task that resolves to the imported <see cref="Instance"/> if successful; otherwise, <see langword="null"/>.
    /// </returns>
    Task<Instance?> ImportAsync(string sourcePath, Resolution resolution, string? customName = null, string? customGroup = null, string? customIconUrl = null, IProgressReporter? progress = null, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Exports a launcher instance into a package archive format.
    /// </summary>
    /// <param name="instance">The instance to export.</param>
    /// <param name="fileNodes">The file tree structure representing the instance's game directory to include as package overrides.</param>
    /// <param name="targetPath">The destination path for the exported package archive.</param>
    /// <param name="exportVersion">The version identifier to write in the package metadata. Defaults to <c>"1.0.0"</c>.</param>
    /// <param name="summary">A human-readable summary or description of the package. Defaults to an empty string.</param>
    /// <param name="progress">Optional progress reporter to track export operations (file copying, archiving, etc.).</param>
    /// <param name="cancellationToken">Cancellation token observed during export operations.</param>
    /// <returns>
    /// A task that resolves to <see langword="true"/> if the package was exported successfully; otherwise, <see langword="false"/>.
    /// </returns>
    Task<bool> ExportAsync(Instance instance, List<FileNode> fileNodes, string targetPath, string exportVersion = "1.0.0", string summary = "", IProgressReporter? progress = null, CancellationToken cancellationToken = default);
}