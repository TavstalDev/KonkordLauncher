using Tavstal.KonkordLauncher.Core.Instances.Forge;
using Tavstal.KonkordLauncher.Core.Models;
using Tavstal.KonkordLauncher.Core.Models.Installer;
using Tavstal.KonkordLauncher.Core.Models.Instance;
using Tavstal.KonkordLauncher.Core.Models.MojangApi;

namespace Tavstal.KonkordLauncher.Core.Instances;

/// <summary>
/// Factory and helpers for creating Forge-specific <see cref="MinecraftInstance"/> implementations.
/// </summary>
public static class ForgeInstance
{
    /// <summary>
    /// Create a configured <see cref="MinecraftInstance"/> for the given Forge build.
    /// </summary>
    /// <param name="id">Unique instance identifier.</param>
    /// <param name="gameVersion">The Minecraft version to use for this instance.</param>
    /// <param name="gameDetails">Gameplay and version information such as the Minecraft version and custom version.</param>
    /// <param name="pathDetails">Paths used by the instance (libraries, versions, assets, etc.).</param>
    /// <param name="launcherDetails">Information about the launcher (branding, version, etc.).</param>
    /// <param name="clientDetails">Client-specific details (user agent, client settings).</param>
    /// <param name="resolution">Optional screen resolution to use when launching the game.</param>
    /// <param name="progressReporter">Optional progress reporter used during instance creation/install operations.</param>
    /// <returns>
    /// A concrete <see cref="MinecraftInstance"/> appropriate for the combination of Minecraft and Forge versions.
    /// The returned instance will be one of:
    /// <br/>- <see cref="ForgeEarlyInstance"/> for very old (pre-1.3/1.5 era) builds,
    /// <br/>- <see cref="ForgeClassicInstance"/> for older classic builds (1.5 - 1.9 era),
    /// <br/>- <see cref="ForgeLegacyInstance"/> for legacy 1.6 - 1.12 era builds,
    /// <br/>- <see cref="ForgeModernInstance"/> for modern Forge versions (1.12.2+).
    /// </returns>
    public static MinecraftInstance GetForgeInstance(
        string id,
        MinecraftVersion gameVersion,
        GameDetails gameDetails,
        PathDetails pathDetails,
        LauncherDetails launcherDetails,
        ClientDetails clientDetails,
        Resolution? resolution = null,
        IProgressReporter? progressReporter = null)
    {
        Version minecraftVersion = new Version(gameDetails.MinecraftVersion);
        
        string mcVer = gameDetails.MinecraftVersion;
        string forgeVer = gameDetails.CustomVersion!;
        return (minecraftVersion.Major, minecraftVersion.Minor) switch
        {
            // Early 1.1 - 1.2.5
            // Only 1.2.5 is supported
            (1, <= 2) => new ForgeEarlyInstance(GetLegacyName(mcVer, forgeVer), "client", id, gameVersion, gameDetails, pathDetails, launcherDetails, clientDetails, resolution,
                progressReporter),
            // Early 1.3.2 - 1.5.1
            // Classic 1.5.2
            (1, <= 5) => mcVer switch
            {
                "1.5.2" => new ForgeClassicInstance(GetLegacyName(mcVer, forgeVer), "minecraftforge-universal-${version}.jar", id, gameVersion, gameDetails, pathDetails, launcherDetails, clientDetails, resolution,
                    progressReporter),
                
                _ => new ForgeEarlyInstance(GetLegacyName(mcVer, forgeVer), "universal", id, gameVersion, gameDetails, pathDetails, launcherDetails, clientDetails, resolution,
                    progressReporter)
            },
            // Classic 1.6 - 1.7.2
            // Legacy 1.7.10-pre4 & 1.7.10
            (1, <= 7) => mcVer switch
            {
                "1.7.10" => new ForgeLegacyInstance($"1.7.10-{forgeVer}-1.7.10", id, gameVersion, gameDetails, pathDetails, launcherDetails, clientDetails, resolution,
                    progressReporter),
                
                "1.7.10-pre4" => new ForgeLegacyInstance($"1.7.10_pre4-{forgeVer}-prerelease", id, gameVersion, gameDetails, pathDetails, launcherDetails, clientDetails, resolution,
                    progressReporter),
                
                "1.7.2" => new ForgeClassicInstance($"1.7.2-{forgeVer}-mc172", "forge-${version}-universal.jar", id, gameVersion, gameDetails, pathDetails, launcherDetails, clientDetails, resolution,
                    progressReporter),
                
                _ => new ForgeClassicInstance(GetLegacyName(mcVer, forgeVer), "minecraftforge-universal-${version}.jar", id, gameVersion, gameDetails, pathDetails, launcherDetails, clientDetails, resolution,
                    progressReporter),
            },
            // Legacy 1.8+ - 1.12.x
            (1, <= 12) => (mcVer, forgeVer) switch
            {
                ("1.8", _) => new ForgeLegacyInstance(GetLegacyName(mcVer, forgeVer), id, gameVersion,gameDetails, pathDetails, launcherDetails, clientDetails, resolution,
                        progressReporter),
                ("1.8.8", _) => new ForgeLegacyInstance(GetLegacyName(mcVer, forgeVer), id, gameVersion, gameDetails, pathDetails, launcherDetails, clientDetails, resolution,
                        progressReporter),
                ("1.8.9", _) => new ForgeLegacyInstance(GetLegacyNameWithMc(mcVer, forgeVer), id, gameVersion, gameDetails, pathDetails, launcherDetails, clientDetails, resolution,
                    progressReporter),

                ("1.9", "12.16.1.1938") => new ForgeLegacyInstance(GetLegacyNameWithZero(mcVer,  forgeVer), id, gameVersion, gameDetails, pathDetails, launcherDetails, clientDetails, resolution,
                    progressReporter),
                
                ("1.9", _) => new ForgeLegacyInstance(GetLegacyName(mcVer,  forgeVer), id, gameVersion, gameDetails, pathDetails, launcherDetails, clientDetails, resolution,
                        progressReporter),
                
                ("1.9.4", _) => new ForgeLegacyInstance(GetLegacyNameWithMc(mcVer,  forgeVer), id, gameVersion, gameDetails, pathDetails, launcherDetails, clientDetails, resolution,
                    progressReporter),

                ("1.10", _) => new ForgeLegacyInstance(GetLegacyNameWithZero(mcVer,  forgeVer), id, gameVersion, gameDetails, pathDetails, launcherDetails, clientDetails, resolution,
                    progressReporter),
                
                ("1.10.2", _) or ("1.11", _) or ("1.11.2", _) or ("1.12", _) or
                ("1.12.1", _) => new ForgeLegacyInstance(GetLegacyName(mcVer,  forgeVer), id, gameVersion, gameDetails, pathDetails, launcherDetails, clientDetails, resolution,
                            progressReporter),
                ("1.12.2", _) => new ForgeModernInstance(id, gameVersion, gameDetails, pathDetails, launcherDetails, clientDetails, resolution,
                    progressReporter),

                _ => new ForgeLegacyInstance(GetLegacyName(mcVer,  forgeVer), id, gameVersion, gameDetails, pathDetails, launcherDetails, clientDetails, resolution,
                    progressReporter)  
            },
            // Modern 1.13+
            _ => new ForgeModernInstance(id, gameVersion, gameDetails, pathDetails, launcherDetails, clientDetails, resolution,
                progressReporter)
        };
    }
    
    /// <summary>
    /// Returns a list of legacy Forge support libraries for very old Minecraft versions (used to fix compatibility issues).
    /// </summary>
    /// <param name="minecraftVersion">The Minecraft version string to match against (e.g. "1.5.2").</param>
    /// <param name="path">
    /// Optional base path prefix for the returned library entries. Defaults to the embedded asset path
    /// <c>"Tavstal.KonkordLauncher.Core.Assets.Fmllib."</c>. Each returned entry is the concatenation of this
    /// prefix and the actual library filename.
    /// </param>
    /// <returns>
    /// A list of string library identifiers (path + filename) appropriate for the given Minecraft version.
    /// </returns>
    public static List<string> GetLegacyLibraries(string minecraftVersion, string path = "Tavstal.KonkordLauncher.Core.Assets.Fmllib.")
    {
        Version mcVersion = new Version(minecraftVersion);
        if (mcVersion.Major != 1)
            return [];
        
        return (mcVersion.Minor, mcVersion.Build) switch
        {
            (3, _) =>
            [
                $"{path}argo-2.25.jar",
                $"{path}asm-all-4.0.jar",
                $"{path}guava-12.0.1.jar"
            ],
            (4, _) =>
            [
                $"{path}argo-2.25.jar",
                $"{path}asm-all-4.0.jar",
                $"{path}guava-12.0.1.jar",
                $"{path}bcprov-jdk15on-147.jar"
            ],
            (5, 1) =>
            [
                $"{path}argo-small-3.2.jar",
                $"{path}asm-all-4.1.jar",
                $"{path}bcprov-jdk15on-148.jar",
                $"{path}deobfuscation_data_1.5.1.zip",
                $"{path}guava-14.0-rc3.jar",
                $"{path}scala-library.jar"
            ],
            (5, 2) =>
            [
                $"{path}argo-small-3.2.jar",
                $"{path}asm-all-4.1.jar",
                $"{path}bcprov-jdk15on-148.jar",
                $"{path}deobfuscation_data_1.5.2.zip",
                $"{path}guava-14.0-rc3.jar",
                $"{path}scala-library.jar"
            ],
            (5, _) =>
            [
                $"{path}argo-small-3.2.jar",
                $"{path}asm-all-4.1.jar",
                $"{path}bcprov-jdk15on-148.jar",
                $"{path}deobfuscation_data_1.5.zip",
                $"{path}guava-14.0-rc3.jar",
                $"{path}scala-library.jar"
            ],
            _ => []
        };
    }
    
    /// <summary>
    /// Generates the standard legacy Forge installer name.
    /// Format: "mc_version-forgemc_version-forge_version"
    /// Example: "1.8-forge1.8-11.14.4.1563"
    /// </summary>
    private static string GetLegacyName(string mcVersion, string forgeVersion)
    {
        return $"{mcVersion}-{forgeVersion}";
    }

    /// <summary>
    /// Generates a specific legacy Forge installer name including the full Minecraft version twice.
    /// Format: "mc_version-forgemc_version-forge_version-mc_version"
    /// Example: "1.8.9-forge1.8.9-11.15.1.1904-1.8.9"
    /// </summary>
    private static string GetLegacyNameWithMc(string mcVersion, string forgeVersion)
    {
        return $"{mcVersion}-{forgeVersion}-{mcVersion}";
    }

    /// <summary>
    /// Generates a specific legacy Forge installer name with a ".0" suffix.
    /// Format: "mc_version-forgemc_version-forge_version-mc_version.0"
    /// Example: "1.10-forge1.10-12.18.0.2000-1.10.0"
    /// </summary>
    private static string GetLegacyNameWithZero(string mcVersion, string forgeVersion)
    {
        return $"{mcVersion}-{forgeVersion}-{mcVersion}.0";
    }
}