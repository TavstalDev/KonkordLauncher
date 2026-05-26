using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Tavstal.KonkordLauncher.Core.Enums;
using Tavstal.KonkordLauncher.Core.Helpers.Domain;
using Tavstal.KonkordLauncher.Core.Helpers.IO;
using Tavstal.KonkordLauncher.Core.Helpers.Platform;
using Tavstal.KonkordLauncher.Core.Instances;
using Tavstal.KonkordLauncher.Core.Models;
using Tavstal.KonkordLauncher.Core.Models.Installer;
using Tavstal.KonkordLauncher.Core.Models.Instance;
using Tavstal.KonkordLauncher.Core.Models.MojangApi;
using Tavstal.KonkordLauncher.Core.Models.MojangApi.Meta;
using Tavstal.KonkordLauncher.Core.Services.Abstractions;

namespace Tavstal.KonkordLauncher.Core.Services.Implementations;

public class InstanceInstallService : IInstanceInstallService
{
    private readonly ILogger _logger;
    private readonly ILibraryDownloadService _libraryDownloadService;
    
    public InstanceInstallService(ILogger<InstanceInstallService> logger, ILibraryDownloadService libraryDownloadService)
    {
        _logger = logger;
        _libraryDownloadService = libraryDownloadService;
    }
    
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
            await DownloadCoreFilesAsync(cancellationToken);
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
            var moddedData = await instance.InstallModdedAsync(tempDir, cancellationToken);
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
            await DownloadDependenciesAsync(instance.VersionData, libraries, cancellationToken);
            endTime = DateTime.Now;
            _logger.LogInformation($"Dependencies downloaded in {(endTime - startTime).TotalMilliseconds}ms.");

            // Fix for Forge: 1.17.x-1.20.3 unable to launch issue
            if (instance.GameDetails.Kind != EMinecraftKind.FORGE ||
                VersionHelper.isNewer(instance.VersionData.MinecraftVersion, "1.20.3") ||
                !VersionHelper.isNewer(instance.VersionData.MinecraftVersion, "1.16.5"))
                instance.ArgumentBuilder.AddClass(moddedData != null ? instance.VersionData.CustomJarPath! : instance.VersionData.VanillaJarPath);

            var arguments = instance.ArgumentBuilder.Build();
            await Task.Delay(250, cancellationToken); // Ensure the progress reporter has time to update before launching
            //_progressReporter?.CloseReporter();
            
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

            // Execute pre-launch command if specified
            if (!string.IsNullOrEmpty(instance.GameDetails.PreLaunchCommand))
            {
               var preLaunchProc = StartCommand(instance.GameDetails.PreLaunchCommand);
               if (preLaunchProc != null)
               {
                   _logger.LogDebug("Executing pre-launch command...");
                   startTime = DateTime.Now;
                   await preLaunchProc.WaitForExitAsync(cancellationToken);
                   endTime = DateTime.Now;
                   _logger.LogInformation($"Pre-launch command executed in {(endTime - startTime).TotalMilliseconds}ms.");
               }
            }
            
            return true;
        }
        finally
        {
            FileSystemHelper.DeleteDirectory(tempDir);
        }
    }

    public async Task<bool> IsInstalledAsync(MinecraftInstance instance, CancellationToken cancellationToken = default)
    {
       
    }

    public async Task RepairAsync(MinecraftInstance instance, IProgressReporter? progress = null,
        CancellationToken cancellationToken = default)
    {
       
    }
    
    private Process? StartCommand(string command, Dictionary<string, string>? environmentVariables = null)
    {
        // Configure the process start information
        var psi = new ProcessStartInfo
        {
            UseShellExecute = false,
            RedirectStandardError = true,
            RedirectStandardOutput = true,
        };
        // Add environment variables if provided
        if (environmentVariables != null)
        {
            foreach (var kvp in environmentVariables)
                psi.EnvironmentVariables[kvp.Key] = kvp.Value;
        }

        switch (OSHelper.GetOperatingSystem())
        {
            case EOperatingSystem.Windows:
            {
                psi.FileName = "cmd.exe";
                psi.Arguments = $"/C \"{command}\"";
                break;
            }
            case EOperatingSystem.MacOS:
            {
                psi.FileName = "/bin/zsh";
                psi.Arguments = $"-c \"{command}\"";
                break;
            }
            case EOperatingSystem.Unknown:
            case EOperatingSystem.Linux:
            {
                psi.FileName = "/bin/sh";
                psi.Arguments = $"-c \"{command}\"";
                break;
            }
        }
        
        var process = Process.Start(psi);
        if (process != null)
        {
            process.EnableRaisingEvents = true;
#if DEBUG
            process.OutputDataReceived += (_, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                    _logger.LogDebug($"Custom command: {e.Data}");
            };
            process.ErrorDataReceived += (_, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                    _logger.LogDebug($"Custom command: {e.Data}");
            };
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
#endif
        }

        // Start the process and return the Process object
        return process;
    }
    
    private async Task DownloadCoreFilesAsync(CancellationToken cancellationToken = default)
    {
        var localVersionMeta = await MinecraftFileService.DownloadVersionAsync(VersionData, MinecraftVersion, _progressReporter, cancellationToken);
        MinecraftVersionMeta = localVersionMeta ?? throw new InvalidOperationException("Failed to download the version meta data. Please check your internet connection and try again.");

        // Change the required Java version if necessary
        if (GameDetails.Kind == EMinecraftKind.FORGE)
        {
            Version forgeMinecraftVersion = new Version(GameDetails.MinecraftVersion);
            // Set the required Java version to 7 for Forge versions 1.7.2 and below
            if (forgeMinecraftVersion.Major == 1 &&
                (forgeMinecraftVersion.Minor < 7 || forgeMinecraftVersion is { Minor: 7, Build: < 10 }))
                MinecraftVersionMeta.JavaVersionMeta.MajorVersion = 7;
        }
        
        if (GameDetails.JavaPath == "LAUNCH_ME_FIRST" || string.IsNullOrEmpty(GameDetails.JavaPath))
            // ReSharper disable once ConditionalAccessQualifierIsNonNullableAccordingToAPIContract - It can be null if the event has no subscribers
            OnSetupDefaultJava?.Invoke(MinecraftVersionMeta);
        
        await MinecraftFileService.DownloadMappingsAsync(MinecraftVersionMeta, VersionData, _progressReporter);
        await MinecraftFileService.DownloadAssetsAsync(MinecraftVersionMeta, PathDetails.AssetsDir, VersionData.GameDir, _progressReporter, cancellationToken);
    }
    
    private async Task DownloadDependenciesAsync(VersionDetails versionDetails, List<LibraryMeta> libraries, CancellationToken cancellationToken = default)
    {
        if (ArgumentBuilder == null)
            throw new InvalidOperationException($"{nameof(ArgumentBuilder)} cannot be null.");
        
        var loggingArg = await MinecraftFileService.DownloadLoggingAsync(MinecraftVersionMeta, VersionData.CustomVersionDirectory ?? VersionData.VanillaVersionDirectory, versionDetails.GameDir, _progressReporter, cancellationToken);
        if (loggingArg != null)
            ArgumentBuilder.AddJvmArgumentBeforeClassPath(loggingArg);

        var classPath = await MinecraftFileService.DownloadLibrariesAsync(GameDetails.Kind, VersionData, libraries, ArgumentBuilder.ClassPath, PathDetails.CacheDir, PathDetails.LibrariesDir, _progressReporter, cancellationToken);
        foreach (var cp in classPath)
            ArgumentBuilder.AddClass(cp);

        if (GameDetails.Kind is not (EMinecraftKind.FORGE or EMinecraftKind.NEOFORGE))
        {
            string? result =
                await MinecraftFileService.ExtractLaunchWrapperAsync(PathDetails.LibrariesDir, cancellationToken);
            if (!string.IsNullOrEmpty(result))
                ArgumentBuilder.AddClass(result);
            ArgumentBuilder.AddJvmArgument(new LaunchArg("io.github.tavstaldev.launchWrapper.Launch", 2));
        }
    }
}