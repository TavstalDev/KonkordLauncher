using Tavstal.KonkordLauncher.Core.Enums;
using Tavstal.KonkordLauncher.Core.Helpers;
using Tavstal.KonkordLauncher.Core.Models;
using Tavstal.KonkordLauncher.Core.Models.Installer;
using Tavstal.KonkordLauncher.Core.Models.ModLoaders.Forge;

namespace Tavstal.KonkordLauncher.Core.Installers.Forge;

// 1.1 - 1.5.2
public class ForgeEarlyInstance(
    GameDetails gameDetails,
    PathDetails pathDetails,
    LauncherDetails launcherDetails,
    ClientDetails clientDetails,
    Resolution? resolution = null,
    IProgressReporter? progressReporter = null)
    : ForgeInstanceBase(gameDetails, pathDetails, launcherDetails, clientDetails, resolution, progressReporter)
{
    private readonly CoreLogger _logger = CoreLogger.WithModuleType(typeof(ForgeClassicInstance));

    protected override async Task<ModdedData?> InstallModdedAsync(string tempDir)
    {
        if (!File.Exists(PathDetails.CustomManifestPath))
        {
            _logger.Error("Forge manifest file does not exist. Please ensure the manifest is downloaded.");
            return null;
        }

        VersionDetails forgeVersion = GameHelper.GetVersionDetails(PathDetails.VersionsDir, this.MinecraftVersion.Id,
            EMinecraftKind.FORGE, this.GameDetails.CustomVersion, this.GameDetails.CustomGameDirectory);

        // Create versionDir in the versions folder
        if (!Directory.Exists(forgeVersion.VersionDirectory))
            Directory.CreateDirectory(forgeVersion.VersionDirectory);

        return null;
    }
}