using System.Diagnostics;
using Tavstal.KonkordLauncher.Core.Enums;
using Tavstal.KonkordLauncher.Core.Helpers.Domain;
using Tavstal.KonkordLauncher.Core.Helpers.IO;
using Tavstal.KonkordLauncher.Core.Helpers.Platform;
using Tavstal.KonkordLauncher.Core.Models;
using Tavstal.KonkordLauncher.Core.Models.Installer;
using Tavstal.KonkordLauncher.Core.Models.Instance;
using Tavstal.KonkordLauncher.Core.Models.MojangApi;
using Tavstal.KonkordLauncher.Core.Models.MojangApi.Meta;
using Tavstal.KonkordLauncher.Core.Services;

namespace Tavstal.KonkordLauncher.Core.Instances;

public class MinecraftInstance
{
    private readonly CoreLogger _logger = CoreLogger.WithModuleType(typeof(MinecraftInstance));

    public string Id { get; }
    public LauncherDetails LauncherDetails { get; }
    public ClientDetails Client { get; }
    public GameDetails GameDetails { get; }
    public PathDetails PathDetails { get; }
    public Resolution? Resolution { get; }
    public VersionDetails VersionData { get; }
    public VersionManifest VersionManifest { get; }
    public MinecraftVersion MinecraftVersion { get; }
    public ArgumentBuilder? ArgumentBuilder { get; set; }
    protected IProgressReporter? _progressReporter { get; }

    public VersionMeta MinecraftVersionMeta { get; set; }
    
    public MinecraftInstance(string id, GameDetails gameDetails, PathDetails pathDetails, LauncherDetails launcherDetails,
        ClientDetails clientDetails, Resolution? resolution = null, IProgressReporter? progressReporter = null)
    {
        Id = id;
        _progressReporter = progressReporter;
        GameDetails = gameDetails;
        PathDetails = pathDetails;
        Resolution = resolution;
        LauncherDetails = launcherDetails;
        Client = clientDetails;

        VersionManifest = ManifestHelper.GetMinecraftManifest()
                          ?? throw new InvalidOperationException(
                              "Failed to read the local vanilla manifest. Please ensure that the file exists and is valid.");

        MinecraftVersion = VersionManifest.Versions.FirstOrDefault(x => x.Id == GameDetails.MinecraftVersion)
                           ?? throw new InvalidOperationException(
                               $"The specified Minecraft version does not exist in the manifest: {GameDetails.MinecraftVersion}");


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
    
    public async Task<Process?> StartAsync(CancellationToken cancellationToken = default)
    {
        return null;
    }
    
    public List<LibraryMeta> GetCombinedLibraries(ModdedData? moddedData)
    {
        var libraries = new List<LibraryMeta>(MinecraftVersionMeta.Libraries);
        if (moddedData?.Libraries.Count > 0)
            libraries.InsertRange(0, moddedData.Libraries);
        return libraries;
    }
    
    
    public virtual Task<ModdedData?> InstallModdedAsync(string tempDir, CancellationToken cancellationToken = default)
    {
        // Vanilla installer, do nothing
        return Task.FromResult<ModdedData?>(null);
    }
    
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
    
    #region  Events

    /// <summary>
    /// Delegate for handling the setup of the default Java path based on the provided version metadata.
    /// </summary>
    /// <param name="versionMeta">The metadata of the Minecraft version used to determine the default Java path.</param>
    public delegate void SetupDefaultJavaEventHandler(VersionMeta versionMeta);

    /// <summary>
    /// Event triggered when the default Java path needs to be set up.
    /// Subscribers can handle this event to configure the Java path based on the provided version metadata.
    /// </summary>
    public event SetupDefaultJavaEventHandler? OnSetupDefaultJava;

    public void InvokeSetupDefaultJava(VersionMeta versionMeta) => OnSetupDefaultJava?.Invoke(versionMeta);
    
    /// <summary>
    /// Updates the Java path used by the game and logs the change.
    /// </summary>
    /// <param name="javaPath">The new Java path to be used by the game.</param>
    public void UpdateJavaPath(string javaPath)
    {
        GameDetails.JavaPath = javaPath;
        _logger.Debug($"Java path updated to: {javaPath}");
    }
    #endregion
}