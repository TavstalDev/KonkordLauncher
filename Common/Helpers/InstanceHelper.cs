using Tavstal.KonkordLauncher.Common.Models;
using Tavstal.KonkordLauncher.Common.Models.Package;
using Tavstal.KonkordLauncher.Core.Models;
using Tavstal.KonkordLauncher.Core.Models.Instance;

namespace Tavstal.KonkordLauncher.Common.Helpers;

/// <summary>
/// High-level helper that provides import and export entry points for instance packages (Modrinth, CurseForge).
/// </summary>
public static class InstanceHelper
{
    private static readonly ModrinthPackageHandler _modrinthHandler = new();
    private static readonly CurseForgePackageHandler _curseForgeHandler = new();
    
    /// <summary>
    /// Imports an instance package from the specified <paramref name="sourcePath"/> according to the selected <paramref name="provider"/>.
    /// </summary>
    /// <param name="sourcePath">Path (or URL, depending on handler support) to the package to import.</param>
    /// <param name="provider">Which provider/format to use when importing (Modrinth, CurseForge).</param>
    /// <param name="resolution">The resolution to set for the imported instance's game config.</param>
    /// <param name="customName">Optional custom name for the imported instance.</param>
    /// <param name="customGroup">Optional custom group for the imported instance.</param>
    /// <param name="progressReporter">Optional progress reporter to receive progress updates during the import process.</param>
    /// <param name="cancellationToken">Token used to cancel the import operation.</param>
    /// <returns>A task that completes when the import has finished.</returns>
    public static async Task<Instance?> ImportAsync(string sourcePath, EInstanceProvider provider, Resolution resolution, string? customName = null, string? customGroup = null, IProgressReporter? progressReporter = null, CancellationToken cancellationToken = default)
    {
        switch (provider)
        {
            case EInstanceProvider.Modrinth:
            {
                return await _modrinthHandler.ImportAsync(sourcePath, resolution, customName, customGroup, progressReporter, cancellationToken);
            }
            case EInstanceProvider.CurseForge:
            {
                return await _curseForgeHandler.ImportAsync(sourcePath, resolution, customName, customGroup, progressReporter, cancellationToken);
            }
        }

        return null;
    }

    /// <summary>
    /// Exports the provided <paramref name="instance"/> to <paramref name="targetPath"/> using the selected <paramref name="provider"/>'s format.
    /// </summary>
    /// <param name="instance">The instance to export (domain model).</param>
    /// <param name="targetPath">Destination file path for the exported package (e.g. /path/to/out.zip).</param>
    /// <param name="provider">Which provider/format to use for the export (Modrinth, CurseForge).</param>
    /// <param name="exportVersion">Version to set in the exported package's metadata (if supported by provider).</param>
    /// <param name="summary">Summary/description to set in the exported package's metadata (if supported by provider).</param>
    /// <param name="cancellationToken">Token used to cancel the export operation.</param>
    /// <returns>A task that completes when the export has finished.</returns>
    public static async Task ExportAsync(Instance instance, string targetPath, EInstanceProvider provider, string exportVersion = "1.0.0", string summary = "", CancellationToken cancellationToken = default)
    {
        switch (provider)
        {
            case EInstanceProvider.Modrinth:
            {
                await _modrinthHandler.ExportAsync(instance, targetPath, exportVersion, summary, null, cancellationToken);
                break;
            }
            case EInstanceProvider.CurseForge:
            {
                await _curseForgeHandler.ExportAsync(instance, targetPath, exportVersion, summary, null, cancellationToken);
                break;
            }
        }
    }
}