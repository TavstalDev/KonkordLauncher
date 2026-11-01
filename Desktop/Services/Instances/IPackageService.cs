using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Tavstal.BeaconSDK.Models.Instance;
using Tavstal.KonkordLauncher.Desktop.Models;
using Tavstal.KonkordLauncher.Desktop.Models.DTOs;
using Tavstal.KonkordLauncher.Desktop.Models.DTOs.Package;

namespace Tavstal.KonkordLauncher.Desktop.Services.Instances;

/// <summary>
///     Imports and exports launcher instances as package files.
/// </summary>
public interface IPackageService
{
    /// <summary>
    ///     Imports a package file and creates a launcher instance from it.
    /// </summary>
    /// <param name="sourcePath">The path to the package file, such as a <c>.mrpack</c> file.</param>
    /// <param name="resolution">The screen resolution used for the new instance window.</param>
    /// <param name="customName">
    ///     An optional name for the instance. If <see langword="null" />, the name is read from the
    ///     package.
    /// </param>
    /// <param name="customGroup">An optional group for the instance.</param>
    /// <param name="customIconUrl">An optional URL for a custom instance icon.</param>
    /// <param name="progress">An optional reporter for import progress.</param>
    /// <param name="cancellationToken">A token that cancels the import.</param>
    /// <returns>The new <see cref="Instance" />, or <see langword="null" /> if the import failed.</returns>
    Task<Instance?> ImportAsync(string sourcePath, Resolution resolution, string? customName = null,
        string? customGroup = null, string? customIconUrl = null, IKonkordProgress? progress = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Exports an instance to a package file.
    /// </summary>
    /// <param name="instance">The instance to export.</param>
    /// <param name="fileNodes">The file tree of the instance game folder to add as overrides.</param>
    /// <param name="targetPath">The path of the package file to create.</param>
    /// <param name="exportVersion">The version to write in the package metadata.</param>
    /// <param name="summary">A short description of the package.</param>
    /// <param name="progress">An optional reporter for export progress.</param>
    /// <param name="cancellationToken">A token that cancels the export.</param>
    /// <returns><see langword="true" /> if the export succeeded; otherwise <see langword="false" />.</returns>
    Task<bool> ExportAsync(Instance instance, List<FileNode> fileNodes, string targetPath,
        string exportVersion = "1.0.0", string summary = "", IKonkordProgress? progress = null,
        CancellationToken cancellationToken = default);
}
