using Tavstal.KonkordLauncher.Core.Models;

namespace Tavstal.KonkordLauncher.Common.Models.Package;

public class ModrinthPackageHandler: IInstancePackageHandler
{
    private readonly CoreLogger _logger = CoreLogger.WithModuleType(typeof(ModrinthPackageHandler));
    /*
     * ARCHIVE LAYOUT:
     *  overrides - containing the game directory 
     *  modrinth.index.json - info about downloaded content from modrinth
     */
    
    public async Task<Instance?> ImportAsync(string sourcePath, IProgress<double>? progress = null, CancellationToken cancellationToken = default)
    {
        try
        {

            return null;
        }
        catch (Exception ex)
        {
            _logger.Error($"Failed to import modrinth package: {ex}");
            return null;
        }
    }

    public async Task<bool> ExportAsync(Instance instance, string targetPath, IProgress<double>? progress = null,
        CancellationToken cancellationToken = default)
    {
        try
        {

            return true;
        }
        catch (Exception ex)
        {
            _logger.Error($"Failed to export modrinth package: {ex}");
            return false;
        }
    }
}