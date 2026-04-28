using Tavstal.KonkordLauncher.Core.Models;

namespace Tavstal.KonkordLauncher.Common.Models.Package;

public class PrismPackageHandler : IInstancePackageHandler
{
    private readonly CoreLogger _logger = CoreLogger.WithModuleType(typeof(PrismPackageHandler));
    /*
     * ARCHIVE LAYOUT:
     *  .minecraft - storing the whole minecraft directory
     *  mrpack
     * - modrinth.index.json - file infos about downloaded content from modrinth
     * - overrides.txt - path overrides to files
     * instance.cfg - contains instance settings
     * mmc-pack.json - contains some component mappings
     */
    
    
    public async Task<Instance?> ImportAsync(string sourcePath, IProgress<double>? progress = null, CancellationToken cancellationToken = default)
    {
        try
        {

            return null;
        }
        catch (Exception ex)
        {
            _logger.Error($"Failed to import prism package: {ex}");
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
            _logger.Error($"Failed to export prism package: {ex}");
            return false;
        }
    }
}