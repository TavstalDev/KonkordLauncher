using Tavstal.KonkordLauncher.Core.Enums;
using Tavstal.KonkordLauncher.Core.Models;
using Tavstal.KonkordLauncher.Core.Models.Installer;
using Tavstal.KonkordLauncher.Core.Models.Instance;
using Tavstal.KonkordLauncher.Core.Models.Logging;
using Tavstal.KonkordLauncher.Core.Models.MojangApi;
using Tavstal.KonkordLauncher.Core.Models.MojangApi.Meta;
using Tavstal.KonkordLauncher.Core.Services.Abstractions;

namespace Tavstal.KonkordLauncher.Core.Instances;


/// <summary>
/// Represents a configurable Minecraft instance used by the launcher. This class encapsulates
/// paths, version metadata and runtime configuration for an instance and provides common
/// installation and argument-building helpers used by derived instance types (vanilla, Fabric/Quilt, Forge, etc.).
/// </summary>
public class MinecraftInstance
{
    protected readonly ICustomLogger _logger;
    public string Id { get; }
    public LauncherDetails LauncherDetails { get; }
    public ClientDetails Client { get; }
    public GameDetails GameDetails { get; }
    public PathDetails PathDetails { get; }
    public Resolution? Resolution { get; }
    public VersionDetails VersionData { get; }
    public MinecraftVersion MinecraftVersion { get; }
    public ArgumentBuilder? ArgumentBuilder { get; set; }
    protected IProgressReporter? _progressReporter { get; }
    public VersionMeta MinecraftVersionMeta { get; set; }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="MinecraftInstance"/> class and prepares
    /// the <see cref="VersionData"/> by computing file-system locations for vanilla and custom versions.
    /// </summary>
    /// <param name="id">Unique instance identifier.</param>
    /// <param name="gameVersion">Selected vanilla <see cref="MinecraftVersion"/> metadata.</param>
    /// <param name="gameDetails">Game runtime details like version strings and custom paths.</param>
    /// <param name="pathDetails">Filesystem root paths used by the instance (libraries, assets, versions, etc.).</param>
    /// <param name="launcherDetails">Launcher-global details and settings.</param>
    /// <param name="clientDetails">Client-specific information passed to the instance.</param>
    /// <param name="logger">Logger used to write diagnostic and progress messages.</param>
    /// <param name="resolution">Optional default resolution for the instance window.</param>
    /// <param name="progressReporter">Optional progress reporter for long running tasks.</param>
    public MinecraftInstance(string id, MinecraftVersion gameVersion, GameDetails gameDetails, PathDetails pathDetails, LauncherDetails launcherDetails,
        ClientDetails clientDetails, ICustomLogger logger, Resolution? resolution = null, IProgressReporter? progressReporter = null)
    {
        Id = id;
        MinecraftVersion = gameVersion;
        GameDetails = gameDetails;
        PathDetails = pathDetails;
        LauncherDetails = launcherDetails;
        Client = clientDetails;
        Resolution = resolution;
        _logger = logger;
        _progressReporter = progressReporter;

        string vanillaVersionsRoot = Path.Combine(PathDetails.VersionsDir, "vanilla");
        Directory.CreateDirectory(vanillaVersionsRoot);
        string vanillaVersionDir = Path.Combine(vanillaVersionsRoot, GameDetails.MinecraftVersion);
        
        VersionData = new VersionDetails
        {
            MinecraftVersion = GameDetails.MinecraftVersion,
            CustomVersion = GameDetails.CustomVersion,
            VanillaVersionDirectory = vanillaVersionDir,
            VanillaJarPath = Path.Combine(vanillaVersionDir, $"{GameDetails.MinecraftVersion}.jar"),
            VanillaJsonPath = Path.Combine(vanillaVersionDir, $"{GameDetails.MinecraftVersion}.json"),
        };

        bool hasCustomGameDir = string.IsNullOrEmpty(GameDetails.CustomGameDirectory);
        if (GameDetails.Kind != EMinecraftKind.VANILLA)
        {
            string customVersionRoot = Path.Combine(PathDetails.VersionsDir, GameDetails.Kind.ToString().ToLower());
            Directory.CreateDirectory(customVersionRoot);
            string customVersionName = $"{GameDetails.MinecraftVersion}-{GameDetails.CustomVersion}";
            string customVersionDir = Path.Combine(customVersionRoot, customVersionName);
            bool isFabric = GameDetails.Kind is EMinecraftKind.FABRIC or EMinecraftKind.QUILT;
                
            VersionData.CustomVersionDirectory = customVersionDir;
            VersionData.CustomJarPath = isFabric ? VersionData.VanillaJarPath : Path.Combine(customVersionDir, $"{customVersionName}.jar");
            VersionData.CustomJsonPath = Path.Combine(customVersionDir, $"{customVersionName}.json");
            VersionData.GameDir = hasCustomGameDir ? Path.Combine(customVersionDir, "game") : GameDetails.CustomGameDirectory!;
            VersionData.NativesDir = hasCustomGameDir ? Path.Combine(customVersionDir, "natives") : Path.Combine(VersionData.GameDir, "natives");
        }
        else
        {
            VersionData.GameDir = hasCustomGameDir ? Path.Combine(vanillaVersionDir, "game") : GameDetails.CustomGameDirectory!;
            VersionData.NativesDir = hasCustomGameDir ? Path.Combine(vanillaVersionDir, "natives") : Path.Combine(VersionData.GameDir, "natives");
        }
    }
    
    /// <summary>
    /// Builds the combined list of libraries required to launch the instance by merging
    /// the vanilla <see cref="MinecraftVersionMeta.Libraries"/> with any modded libraries provided by <paramref name="moddedData"/>.
    /// </summary>
    /// <param name="moddedData">Optional modded data returned by <see cref="InstallModdedAsync"/> (may be null for vanilla).</param>
    /// <returns>
    /// A list where modded libraries (if present) are placed before the vanilla libraries, preserving order.
    /// </returns>
    public List<LibraryMeta> GetCombinedLibraries(ModdedData? moddedData)
    {
        var libraries = new List<LibraryMeta>(MinecraftVersionMeta.Libraries);
        if (moddedData?.Libraries.Count > 0)
            libraries.InsertRange(0, moddedData.Libraries);
        return libraries;
    }
    
    /// <summary>
    /// Performs modloader-specific installation steps (if applicable) and returns any modded launch metadata.
    /// </summary>
    /// <param name="tempDir">Temporary directory that may be used for downloads and extraction.</param>
    /// <param name="httpService">HTTP service used for remote downloads.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>
    /// A <see cref="ModdedData"/> instance containing a custom main class and additional libraries when a modloader is used,
    /// or <c>null</c> for vanilla instances. Derived classes override this method to implement installation logic.
    /// </returns>
    public virtual Task<ModdedData?> InstallModdedAsync(string tempDir, IHttpService httpService, CancellationToken cancellationToken = default)
    {
        // Vanilla installer, do nothing
        return Task.FromResult<ModdedData?>(null);
    }
    
    /// <summary>
    /// Computes a version name string used in version directories and classpath selection.
    /// </summary>
    /// <param name="modVersion">Optional modloader version (e.g. Fabric loader, Forge, NeoForge).</param>
    /// <returns>A formatted version name appropriate for the configured <see cref="GameDetails.Kind"/>.</returns>
    public string GetVersionName(string? modVersion)
    {
        return GameDetails.Kind switch
        {
            EMinecraftKind.VANILLA => VersionData.MinecraftVersion,
            EMinecraftKind.FABRIC => $"fabric-loader-{modVersion}-{VersionData.MinecraftVersion}",
            EMinecraftKind.QUILT => $"quilt-loader-{modVersion}-{VersionData.MinecraftVersion}",
            EMinecraftKind.FORGE => $"{VersionData.MinecraftVersion}-forge-{modVersion}",
            EMinecraftKind.NEOFORGE => $"{VersionData.MinecraftVersion}-neoforge-{modVersion}",
            _ => VersionData.MinecraftVersion
        };
    }
}