using Tavstal.KonkordLauncher.Common.Models;
using Tavstal.KonkordLauncher.Common.Models.Package;

namespace Tavstal.KonkordLauncher.Common.Helpers;

/// <summary>
/// High-level helper that provides import and export entry points for instance packages (PrismLauncher, Modrinth, CurseForge).
/// </summary>
public static class InstanceHelper
{
    private static readonly PrismPackageHandler _prismHandler = new();
    private static readonly ModrinthPackageHandler _modrinthHandler = new();
    private static readonly CurseForgePackageHandler _curseForgeHandler = new();
    
    /// <summary>
    /// Imports an instance package from the specified <paramref name="sourcePath"/> according to the selected <paramref name="provider"/>.
    /// </summary>
    /// <param name="sourcePath">Path (or URL, depending on handler support) to the package to import.</param>
    /// <param name="provider">Which provider/format to use when importing (PrismLauncher, Modrinth, CurseForge).</param>
    /// <param name="cancellationToken">Token used to cancel the import operation.</param>
    /// <returns>A task that completes when the import has finished.</returns>
    public static async Task ImportAsync(string sourcePath, EInstanceProvider provider, CancellationToken cancellationToken = default)
    {
        switch (provider)
        {
            case EInstanceProvider.PrismLauncher:
            {
                await _prismHandler.ImportAsync(sourcePath, null, cancellationToken);
                break;
            }
            case EInstanceProvider.Modrinth:
            {
                await _modrinthHandler.ImportAsync(sourcePath, null, cancellationToken);
                break;
            }
            case EInstanceProvider.CurseForge:
            {
                await _curseForgeHandler.ImportAsync(sourcePath, null, cancellationToken);
                break;
            }
        }
    }

    /// <summary>
    /// Exports the provided <paramref name="instance"/> to <paramref name="targetPath"/> using the selected <paramref name="provider"/>'s format.
    /// </summary>
    /// <param name="instance">The instance to export (domain model).</param>
    /// <param name="targetPath">Destination file path for the exported package (e.g. /path/to/out.zip).</param>
    /// <param name="provider">Which provider/format to use for the export (PrismLauncher, Modrinth, CurseForge).</param>
    /// <param name="cancellationToken">Token used to cancel the export operation.</param>
    /// <returns>A task that completes when the export has finished.</returns>
    public static async Task ExportAsync(Instance instance, string targetPath, EInstanceProvider provider, CancellationToken cancellationToken = default)
    {
        switch (provider)
        {
            case EInstanceProvider.PrismLauncher:
            {
                await _prismHandler.ExportAsync(instance, targetPath, null, cancellationToken);
                break;
            }
            case EInstanceProvider.Modrinth:
            {
                await _modrinthHandler.ExportAsync(instance, targetPath, null, cancellationToken);
                break;
            }
            case EInstanceProvider.CurseForge:
            {
                await _curseForgeHandler.ExportAsync(instance, targetPath, null, cancellationToken);
                break;
            }
        }
    }
}