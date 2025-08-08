using Tavstal.KonkordLauncher.Core.Installers.Forge;
using Tavstal.KonkordLauncher.Core.Models;
using Tavstal.KonkordLauncher.Core.Models.Installer;

namespace Tavstal.KonkordLauncher.Core.Installers;

public static class ForgeInstance
{
    public static MinecraftInstance GetForgeInstance(
        GameDetails gameDetails,
        PathDetails pathDetails,
        LauncherDetails launcherDetails,
        ClientDetails clientDetails,
        Resolution? resolution = null,
        IProgressReporter? progressReporter = null)
    {
        Version minecraftVersion = new Version(gameDetails.MinecraftVersion);

        return (minecraftVersion.Major, minecraftVersion.Minor) switch
        {
            // Early 1.1 - 1.5.1
            (1, <= 4) => new ForgeEarlyInstance(gameDetails, pathDetails, launcherDetails, clientDetails, resolution,
                progressReporter),
            // Early 1.5 & 1.5.1
            // Classic 1.5.2
            (1, 5) => gameDetails.MinecraftVersion switch
            {
                "1.5.2" => new ForgeClassicInstance(gameDetails, pathDetails, launcherDetails, clientDetails, resolution,
                    progressReporter),
                
                _ => new ForgeEarlyInstance(gameDetails, pathDetails, launcherDetails, clientDetails, resolution,
                    progressReporter)
            },
            // Classic 1.6 - 1.7.2
            // Legacy 1.7.10-pre4 & 1.7.10
            (1, <= 7) => gameDetails.MinecraftVersion switch
            {
                "1.7.10" => new ForgeLegacyInstance(gameDetails, pathDetails, launcherDetails, clientDetails, resolution,
                    progressReporter),
                
                "1.7.10-pre4" => new ForgeLegacyInstance(gameDetails, pathDetails, launcherDetails, clientDetails, resolution,
                    progressReporter),
                
                _ => new ForgeClassicInstance(gameDetails, pathDetails, launcherDetails, clientDetails, resolution,
                    progressReporter),
            },
            // Legacy 1.8+ - 1.12.x
            (1, <= 12) => new ForgeLegacyInstance(gameDetails, pathDetails, launcherDetails, clientDetails, resolution,
                progressReporter),
            // Modern 1.13+
            _ => new ForgeModernInstance(gameDetails, pathDetails, launcherDetails, clientDetails, resolution,
                progressReporter)
        };
    }
    
    /// <summary>
    /// Generates the standard legacy Forge installer name.
    /// Format: "mc_version-forgemc_version-forge_version"
    /// Example: "1.8-forge1.8-11.14.4.1563"
    /// </summary>
    private static string GeLegacyName(string mcVersion, string forgeVersion)
    {
        return $"{mcVersion}-forge{mcVersion}-{forgeVersion}";
    }

    /// <summary>
    /// Generates a specific legacy Forge installer name including the full Minecraft version twice.
    /// Format: "mc_version-forgemc_version-forge_version-mc_version"
    /// Example: "1.8.9-forge1.8.9-11.15.1.1904-1.8.9"
    /// </summary>
    private static string GetLegacyNameWithMc(string mcVersion, string forgeVersion)
    {
        return $"{mcVersion}-forge{mcVersion}-{forgeVersion}-{mcVersion}";
    }

    /// <summary>
    /// Generates a specific legacy Forge installer name with a ".0" suffix.
    /// Format: "mc_version-forgemc_version-forge_version-mc_version.0"
    /// Example: "1.10-forge1.10-12.18.0.2000-1.10.0"
    /// </summary>
    private static string GetLegacyNameWithZero(string mcVersion, string forgeVersion)
    {
        return $"{mcVersion}-forge{mcVersion}-{forgeVersion}-{mcVersion}.0";
    }
}