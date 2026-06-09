using Tavstal.KonkordLauncher.Core.Enums;
using Tavstal.KonkordLauncher.Core.Helpers.Domain;
using Tavstal.KonkordLauncher.Core.Helpers.IO;
using Tavstal.KonkordLauncher.Core.Instances;
using Tavstal.KonkordLauncher.Core.Models;
using Tavstal.KonkordLauncher.Core.Models.Installer;
using Tavstal.KonkordLauncher.Core.Models.Instance;
using Tavstal.KonkordLauncher.Core.Models.Logging;
using Tavstal.KonkordLauncher.Core.Models.MojangApi.Meta;
using Tavstal.KonkordLauncher.Core.Services.Abstractions;

namespace Tavstal.KonkordLauncher.Core.Services.Implementations;

/// <inheritdoc/>
public class InstanceInstallService : IInstanceInstallService
{
    private readonly ICustomLogger _logger;
    private readonly IHttpService _httpService;
    private readonly ILibraryDownloadService _libraryDownloadService;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="InstanceInstallService"/> class with the
    /// dependencies required to install and prepare a Minecraft instance.
    /// </summary>
    /// <param name="logger">Logger used to record installation progress and diagnostic information.</param>
    /// <param name="httpService">HTTP service used for downloading or retrieving remote installation data.</param>
    /// <param name="libraryDownloadService">Service responsible for downloading Minecraft libraries, mappings, assets, and related files.</param>
    public InstanceInstallService(ICustomLogger<InstanceInstallService> logger, IHttpService httpService, ILibraryDownloadService libraryDownloadService)
    {
        _logger = logger;
        _httpService = httpService;
        _libraryDownloadService = libraryDownloadService;
    }
    
    /// <inheritdoc/>
    public async Task<bool> InstallAsync(MinecraftInstance instance, IProgressReporter? progress = null,
        CancellationToken cancellationToken = default)
    {
       string tempDir = Path.Combine(PathHelper.TempDir, Path.GetRandomFileName());
        Directory.CreateDirectory(tempDir);
        Directory.CreateDirectory(instance.VersionData.VanillaVersionDirectory);

        try
        {
            _logger.LogDebug("Downloading core files...");
            DateTime startTime = DateTime.Now;
            await DownloadCoreFilesAsync(instance, progress, cancellationToken);
            DateTime endTime = DateTime.Now;
            _logger.LogInformation($"Core files downloaded in {(endTime - startTime).TotalMilliseconds}ms.");

            
            instance.ArgumentBuilder = new ArgumentBuilder(
                version: instance.MinecraftVersion.Id,
                versionName: instance.GetVersionName(null),
                nativesDir: instance.VersionData.NativesDir,
                gameDir: instance.VersionData.GameDir,
                assetIndexId: instance.MinecraftVersionMeta.Index.Id,
                versionMeta: instance.MinecraftVersionMeta,
                launcherDetails: instance.LauncherDetails,
                gameDetails: instance.GameDetails,
                clientDetails: instance.Client,
                pathDetails: instance.PathDetails,
                resolution: instance.Resolution);

            if (instance.MinecraftVersionMeta.JavaVersionMeta.MajorVersion >= 9)
            {
                instance.ArgumentBuilder.UseClasspathFile = true;
                instance.ArgumentBuilder.ClasspathFilePath = Path.Combine(instance.VersionData.GameDir, "classpath.txt");
            }

            _logger.LogDebug("Installing modded data if applicable...");
            startTime = DateTime.Now;
            var moddedData = await instance.InstallModdedAsync(tempDir, _httpService, cancellationToken);
            endTime = DateTime.Now;
            _logger.LogInformation($"Modded data installation completed in {(endTime - startTime).TotalMilliseconds}ms.");
            string mainClass = moddedData?.MainClass ?? instance.MinecraftVersionMeta.MainClass;

            // Force update main class
            instance.ArgumentBuilder.AddGameArgument(new LaunchArg(mainClass + " ", 101));
            instance.ArgumentBuilder.AddPlaceholder("${version_name}", instance.GetVersionName(instance.VersionData.CustomVersion));
            
            Directory.CreateDirectory(instance.VersionData.GameDir);

            _logger.LogDebug("Downloading dependencies...");
            var libraries = instance.GetCombinedLibraries(moddedData);
            startTime = DateTime.Now;
            await DownloadDependenciesAsync(instance, libraries, progress, cancellationToken);
            endTime = DateTime.Now;
            _logger.LogInformation($"Dependencies downloaded in {(endTime - startTime).TotalMilliseconds}ms.");

            // Fix for Forge: 1.17.x-1.20.3 unable to launch issue
            if (instance.GameDetails.Kind != EMinecraftKind.FORGE ||
               GameHelper.isNewer(instance.VersionData.MinecraftVersion, "1.20.3") ||
                !GameHelper.isNewer(instance.VersionData.MinecraftVersion, "1.16.5"))
                instance.ArgumentBuilder.AddClass(moddedData != null ? instance.VersionData.CustomJarPath! : instance.VersionData.VanillaJarPath);
            
            await Task.Delay(250, cancellationToken); // Ensure the progress reporter has time to update before launching
            progress?.CloseReporter();
            
            // Copy custom natives if specified
            _logger.LogDebug("Copying custom native files if specified...");
            startTime = DateTime.Now;
            foreach (string nativePath in instance.PathDetails.CustomNativeFiles)
            {
                if (!File.Exists(nativePath))
                    continue;
                string destPath = Path.Combine(instance.VersionData.NativesDir, Path.GetFileName(nativePath));
                File.Copy(nativePath, destPath, true);
            }
            endTime = DateTime.Now;
            _logger.LogInformation($"Custom native files copied in {(endTime - startTime).TotalMilliseconds}ms.");
            return true;
        }
        finally
        {
            FileSystemHelper.DeleteDirectory(tempDir);
        }
    }
    
    /// <summary>
    /// Downloads the core files required for an instance, including version metadata, mappings,
    /// and game assets. Also adjusts Java requirements for older Forge versions when needed.
    /// </summary>
    /// <param name="instance">The Minecraft instance being prepared for installation.</param>
    /// <param name="progressReporter">Optional progress reporter used to report download progress.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <exception cref="InvalidOperationException">Thrown when the version metadata cannot be downloaded successfully.</exception>
    private async Task DownloadCoreFilesAsync(MinecraftInstance instance,
        IProgressReporter? progressReporter = null,
        CancellationToken cancellationToken = default)
    {
        var localVersionMeta = await _libraryDownloadService.DownloadVersionAsync(instance.VersionData, instance.MinecraftVersion, progressReporter, cancellationToken);
        instance.MinecraftVersionMeta = localVersionMeta ?? throw new InvalidOperationException("Failed to download the version meta data. Please check your internet connection and try again.");

        // Change the required Java version if necessary
        if (instance.GameDetails.Kind == EMinecraftKind.FORGE)
        {
            Version forgeMinecraftVersion = new Version(instance.GameDetails.MinecraftVersion);
            // Set the required Java version to 7 for Forge versions 1.7.2 and below
            if (forgeMinecraftVersion.Major == 1 &&
                (forgeMinecraftVersion.Minor < 7 || forgeMinecraftVersion is { Minor: 7, Build: < 10 }))
                instance.MinecraftVersionMeta.JavaVersionMeta.MajorVersion = 7;
        }
        
        await _libraryDownloadService.DownloadMappingsAsync(instance.MinecraftVersionMeta, instance.VersionData, progressReporter, cancellationToken);
        await _libraryDownloadService.DownloadAssetsAsync(instance.MinecraftVersionMeta, instance.PathDetails.AssetsDir, instance.VersionData.GameDir, progressReporter, cancellationToken);
    }
    
    /// <summary>
    /// Downloads and prepares all runtime dependencies required by the instance, including
    /// logging configuration, libraries, and the LaunchWrapper for non-Forge/non-NeoForge setups.
    /// </summary>
    /// <param name="instance">The Minecraft instance being installed.</param>
    /// <param name="libraries">The complete list of libraries that must be downloaded.</param>
    /// <param name="progressReporter">Optional progress reporter used to report download progress.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <exception cref="InvalidOperationException">Thrown when the argument builder has not been initialized.</exception>
    private async Task DownloadDependenciesAsync(MinecraftInstance instance,
        List<LibraryMeta> libraries,
        IProgressReporter? progressReporter = null,
        CancellationToken cancellationToken = default)
    {
        if (instance.ArgumentBuilder == null)
            throw new InvalidOperationException($"{nameof(ArgumentBuilder)} cannot be null.");
        
        var loggingArg = await _libraryDownloadService.DownloadLoggingAsync(instance.MinecraftVersionMeta, instance.VersionData.CustomVersionDirectory ?? instance.VersionData.VanillaVersionDirectory, instance.VersionData.GameDir, progressReporter, cancellationToken);
        if (loggingArg != null)
            instance.ArgumentBuilder.AddJvmArgumentBeforeClassPath(loggingArg);

        var classPath = await _libraryDownloadService.DownloadLibrariesAsync(instance.GameDetails.Kind, instance.VersionData, libraries, 
            instance.ArgumentBuilder.ClassPath, instance.PathDetails.CacheDir, instance.PathDetails.LibrariesDir, progressReporter, cancellationToken);
        foreach (var cp in classPath)
            instance.ArgumentBuilder.AddClass(cp);

        if (instance.GameDetails.Kind is not (EMinecraftKind.FORGE or EMinecraftKind.NEOFORGE))
        {
            string? result =
                await _libraryDownloadService.ExtractLaunchWrapperAsync(instance.PathDetails.LibrariesDir, cancellationToken);
            if (!string.IsNullOrEmpty(result))
                instance.ArgumentBuilder.AddClass(result);
            instance.ArgumentBuilder.AddJvmArgument(new LaunchArg("io.github.tavstaldev.launchWrapper.Launch", 2));
        }
    }
}