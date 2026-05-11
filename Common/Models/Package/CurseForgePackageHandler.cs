using Tavstal.KonkordLauncher.Core.Models;
using Tavstal.KonkordLauncher.Core.Models.Instance;

namespace Tavstal.KonkordLauncher.Common.Models.Package;

public class CurseForgePackageHandler: IInstancePackageHandler
{
    private readonly CoreLogger _logger = CoreLogger.WithModuleType(typeof(CurseForgePackageHandler));
    /*
     * ARCHIVE LAYOUT:
     * overrides - containing the game directory
     * manifest.json - info about downloaded content from curseforge
     * modlist.html - list of mods in a raw html list
     */
    
    public async Task<Instance?> ImportAsync(string sourcePath, Resolution resolution, string? customName = null, string? customGroup = null, IProgressReporter? progress = null, CancellationToken cancellationToken = default)
    {
        try
        {

            return null;
        }
        catch (Exception ex)
        {
            _logger.Error($"Failed to import curse forge package: {ex}");
            return null;
        }
    }

    public async Task<bool> ExportAsync(Instance instance, string targetPath, string exportVersion = "1.0.0", string summary = "", IProgressReporter? progress = null,
        CancellationToken cancellationToken = default)
    {
        try
        {

            return true;
        }
        catch (Exception ex)
        {
            _logger.Error($"Failed to export curse forge package: {ex}");
            return false;
        }
    }
}