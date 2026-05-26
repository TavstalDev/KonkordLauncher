using System.Collections.Concurrent;
using System.IO.Compression;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Tavstal.KonkordLauncher.Core.Enums;
using Tavstal.KonkordLauncher.Core.Helpers.Domain;
using Tavstal.KonkordLauncher.Core.Helpers.IO;
using Tavstal.KonkordLauncher.Core.Helpers.Platform;
using Tavstal.KonkordLauncher.Core.Models;
using Tavstal.KonkordLauncher.Core.Models.Endpoints;
using Tavstal.KonkordLauncher.Core.Models.Installer;
using Tavstal.KonkordLauncher.Core.Models.Instance;
using Tavstal.KonkordLauncher.Core.Models.MojangApi;
using Tavstal.KonkordLauncher.Core.Models.MojangApi.Meta;
using Tavstal.KonkordLauncher.Core.Services.Abstractions;

namespace Tavstal.KonkordLauncher.Core.Services.Implementations;

/// <inheritdoc/>
public class LibraryDownloadService : ILibraryDownloadService
{
    private readonly ILogger _logger;
    private readonly IHttpService _httpService;
    private const int MaxParallelDownloads = 16;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="LibraryDownloadService"/> class.
    /// </summary>
    /// <param name="logger">Logger used to record diagnostics and errors during library download operations.</param>
    /// <param name="httpService">HTTP service used to download files and fetch remote content.</param>
    public LibraryDownloadService(ILogger<LibraryDownloadService> logger, IHttpService httpService)
    {
        _logger = logger;
        _httpService = httpService;
    }
    
    /// <inheritdoc/>
    public async Task<VersionMeta?> DownloadVersionAsync(VersionDetails versionData, MinecraftVersion minecraftVersion,
        IProgressReporter? progressReporter = null, CancellationToken cancellationToken = default)
    {
        // JSON
        var versionResult = await DownloadAndSaveFileAsync(
            versionData.VanillaJsonPath,
            minecraftVersion.Url,
            "version_json",
            progressReporter,
            JsonConvert.DeserializeObject<VersionMeta>, cancellationToken);

        if (versionResult == null) return null;

        // JAR
        await DownloadAndSaveBinaryFileAsync(
            versionData.VanillaJarPath,
            versionResult.Downloads.Client.Url,
            "version_jar",
            progressReporter, cancellationToken);

        // Create default JavaVersionMeta if null
        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        // ReSharper disable once ConvertIfStatementToNullCoalescingAssignment
        if (versionResult.JavaVersionMeta == null)
            versionResult.JavaVersionMeta = new JavaVersionMeta
            {
                MajorVersion = 8
            };

        return versionResult;
    }

    /// <inheritdoc/>
    public async Task DownloadAssetsAsync(VersionMeta versionMeta, string assetsDir, string gameDir,
        IProgressReporter? progressReporter = null, CancellationToken cancellationToken = default)
    {
        // AssetIndex
        string assetIndexId = versionMeta.Index.Id;
        string assetPath = Path.Combine(assetsDir, $"indexes/{assetIndexId}.json");

        string? resultJson = await DownloadAndSaveFileAsync(
            assetPath,
            versionMeta.Index.Url,
            "asset_index_json",
            progressReporter,
            json => json, cancellationToken); // Deserialize to string, then parse JObject

        if (resultJson == null) return;

        string assetsType = versionMeta.Assets;
        
        var assetJToken = JObject.Parse(resultJson)["objects"];
        if (assetJToken == null)
            throw new Exception("Asset JToken is null, something went wrong while reading the asset index JSON.");

        // Assets
        progressReporter?.UpdateStatusTranslated("instance.reading.assets");

        var semaphore = new SemaphoreSlim(MaxParallelDownloads);
        long downloadedBytes = 0;
        var tasks = new List<Task>();
        
        switch (assetsType)
        {
            // Olds Assets
            case "pre-1.6":
            {
                string resourcesDir = Path.Combine(gameDir, "resources");
                Directory.CreateDirectory(resourcesDir);
                
                // For some reason pre-1.6 still wants to use icons from legacy folder
                // So we fix this by copying the icon files to the legacy folder
                string legacyDir= Path.Combine(assetsDir, "virtual", "legacy");
                Directory.CreateDirectory(legacyDir);
            
                foreach (JProperty token in assetJToken.Children<JProperty>().ToList())
                {
                    var rawHash = token.First?["hash"];
                    if (rawHash == null) continue;

                    string rawFilePath = token.Name;
                    var hash = rawHash.ToString();

                    var fileName = Path.GetFileName(rawFilePath);
                    var fileDirectory = Path.GetDirectoryName(rawFilePath);
                    string? objectDir = null;
                    if (!string.IsNullOrEmpty(fileDirectory))
                    {
                        objectDir = Path.Combine(resourcesDir, fileDirectory);
                        Directory.CreateDirectory(objectDir);
                    }
                    var objectPath = Path.Combine(objectDir ?? resourcesDir, fileName);
                    if (File.Exists(objectPath))
                        continue;
                    
                    await semaphore.WaitAsync(cancellationToken);
                    var t = Task.Run(async () =>
                    {
                        try
                        {
                            await _httpService.DownloadFileAsync(
                                $"{MicrosoftEndpoints.MinecraftResourcesUrl}/{hash[..2]}/{hash}",
                                objectPath,
                                null, cancellationToken);
                            
                            if (fileName.Contains("icon") || (objectDir != null && objectDir.Contains("icon")))
                            {
                                if (!string.IsNullOrEmpty(fileDirectory))
                                {
                                    objectDir = Path.Combine(legacyDir, fileDirectory);
                                    Directory.CreateDirectory(objectDir);
                                }
                                var legacyObjectPath = Path.Combine(objectDir ?? legacyDir, fileName);
                                if (!File.Exists(legacyObjectPath) && File.Exists(objectPath)) // Double check to be sure
                                    File.Copy(objectPath, legacyObjectPath);
                            }

                            var sizeToken = token.First?["size"];
                            var size = sizeToken != null ? int.Parse(sizeToken.ToString()) : 0;
                            Interlocked.Add(ref downloadedBytes, size);

                            double percent = downloadedBytes / (double)versionMeta.Index.TotalSize * 100d;
                            progressReporter?.ReportProgress(percent);
                            progressReporter?.UpdateStatusTranslated("instance.downloading.assets", percent.ToString("0.00"));
                        }
                        finally
                        {
                            semaphore.Release();
                        }
                    }, cancellationToken);
                    tasks.Add(t);
                }
                
                break;
            }
            // Legacy Assets
            case "legacy":
            {
                string resourcesDir= Path.Combine(assetsDir, "virtual", "legacy");
                Directory.CreateDirectory(resourcesDir);
                
                foreach (JProperty token in assetJToken.Children<JProperty>().ToList())
                {
                    var rawHash = token.First?["hash"];
                    if (rawHash == null) continue;

                    string rawFilePath = token.Name;
                    var hash = rawHash.ToString();

                    var fileName = Path.GetFileName(rawFilePath);
                    var fileDirectory = Path.GetDirectoryName(rawFilePath);
                    string? objectDir = null;
                    if (!string.IsNullOrEmpty(fileDirectory))
                    {
                        objectDir = Path.Combine(resourcesDir, fileDirectory);
                        Directory.CreateDirectory(objectDir);
                    }
                    var objectPath = Path.Combine(objectDir ?? resourcesDir, fileName);
                    if (File.Exists(objectPath))
                        continue;
                    
                    await semaphore.WaitAsync(cancellationToken);
                    var t = Task.Run(async () =>
                    {
                        try
                        {
                            await _httpService.DownloadFileAsync(
                                $"{MicrosoftEndpoints.MinecraftResourcesUrl}/{hash[..2]}/{hash}",
                                objectPath,
                                null, cancellationToken);

                            var sizeToken = token.First?["size"];
                            var size = sizeToken != null ? int.Parse(sizeToken.ToString()) : 0;
                            Interlocked.Add(ref downloadedBytes, size);

                            double percent = downloadedBytes / (double)versionMeta.Index.TotalSize * 100d;
                            progressReporter?.ReportProgress(percent);
                            progressReporter?.UpdateStatusTranslated("instance.downloading.assets", percent.ToString("0.00"));
                        }
                        finally
                        {
                            semaphore.Release();
                        }
                    }, cancellationToken);
                    tasks.Add(t);
                }
                break;
            }
            // Modern Assets
            default:
            {
                // Asset Dir
                string assetObjectDir = Path.Combine(assetsDir, "objects");
                Directory.CreateDirectory(assetObjectDir);
        
                foreach (JToken token in assetJToken.ToList())
                {
                    var rawHash = token.First?["hash"];
                    if (rawHash == null) continue;

                    var hash = rawHash.ToString();
                    var objectDir = Path.Combine(assetObjectDir, hash[..2]);
                    var objectPath = Path.Combine(objectDir, $"{hash}");

                    Directory.CreateDirectory(objectDir);

                    if (File.Exists(objectPath))
                        continue;
                    
                    await semaphore.WaitAsync(cancellationToken);
                    var t = Task.Run(async () =>
                    {
                        try
                        {
                            await _httpService.DownloadFileAsync(
                                $"{MicrosoftEndpoints.MinecraftResourcesUrl}/{hash[..2]}/{hash}",
                                objectPath,
                                null, cancellationToken);

                            var sizeToken = token.First?["size"];
                            var size = sizeToken != null ? int.Parse(sizeToken.ToString()) : 0;
                            Interlocked.Add(ref downloadedBytes, size);

                            double percent = downloadedBytes / (double)versionMeta.Index.TotalSize * 100d;
                            progressReporter?.ReportProgress(percent);
                            progressReporter?.UpdateStatusTranslated("instance.downloading.assets", percent.ToString("0.00"));
                        }
                        finally
                        {
                            semaphore.Release();
                        }
                    }, cancellationToken);
                    tasks.Add(t);
                }
                break;
            }
        }
        await Task.WhenAll(tasks);
    }

    /// <inheritdoc/>
    public async Task<LaunchArg?> DownloadLoggingAsync(VersionMeta versionMeta, string versionDirectory, string gameDir,
        IProgressReporter? progressReporter = null, CancellationToken cancellationToken = default)
    {
        // ReSharper disable once ArrangeNullCheckingPattern
        if (versionMeta.LoggingMeta is not { }) return null;
        
        string logFilePath = Path.Combine(versionDirectory, versionMeta.LoggingMeta.Client.File.Id);

        string? logContent = await DownloadAndSaveFileAsync(
            logFilePath,
            versionMeta.LoggingMeta.Client.File.Url,
            "logging",
            progressReporter,
            json => json, cancellationToken);

        if (logContent == null) return null;

        // FIX LOG LOCATION
        string modifiedContent = logContent
            .Replace("fileName=\"logs", $"fileName=\"{gameDir}/logs")
            .Replace("filePattern=\"logs", $"filePattern=\"{gameDir}/logs");

        await File.WriteAllTextAsync(logFilePath, modifiedContent, cancellationToken);

        return new LaunchArg(versionMeta.LoggingMeta.Client.Argument.Replace("${path}", logFilePath), 2);
    }

    /// <inheritdoc/>
    public async Task DownloadMappingsAsync(VersionMeta versionMeta, VersionDetails versionData,
        IProgressReporter? progressReporter = null, CancellationToken cancellationToken = default)
    {
        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        if (versionMeta.Downloads.ClientMappings == null) return;

        string clientMappinsPath = Path.Combine(versionData.VanillaVersionDirectory, "client.txt");

        await DownloadAndSaveFileAsync(
            clientMappinsPath,
            versionMeta.Downloads.ClientMappings.Url,
            "client_mappings",
            progressReporter,
            json => json, cancellationToken); // Deserialize to string
    }

    /// <inheritdoc/>
    public async Task<string?> ExtractLaunchWrapperAsync(string libsDir, CancellationToken cancellationToken = default)
    {
        string targetDir = Path.Combine(libsDir, "io", "github", "tavstaldev", "launchWrapper");
        Directory.CreateDirectory(targetDir);
        
        const string targetAssetName = "launchWrapper-1.0.jar";
        string targetFile = Path.Combine(targetDir, targetAssetName);
        var assembly = Assembly.GetExecutingAssembly();
        var stream = assembly.GetManifestResourceStream("Tavstal.KonkordLauncher.Core.Assets.launchWrapper-1.0.jar");
        if (stream == null)
            throw new Exception("Failed to find embedded resource: Tavstal.KonkordLauncher.Core.Assets.launchWrapper-1.0.jar");
        
        if (File.Exists(targetFile))
        {
            string? existingHash = await FileSystemHelper.GetFileHashAsync(targetFile);
            string? expectedHash = await FileSystemHelper.GetFileHashAsync(stream);
            if (existingHash == expectedHash && expectedHash != null)
                return targetFile;
            FileSystemHelper.DeleteFile(targetFile);
        }
        
        await using var fileStream = new FileStream(targetFile, FileMode.Create, FileAccess.Write);
        await stream.CopyToAsync(fileStream, cancellationToken);
        return targetFile;
    }

    /// <inheritdoc/>
    public async Task<List<string>> DownloadLibrariesAsync(EMinecraftKind kind, VersionDetails versionData, List<LibraryMeta> mcLibs, List<string> classPath,
        string cacheDir, string libsDir, IProgressReporter? progressReporter = null,
        CancellationToken cancellationToken = default)
    {
        progressReporter?.ReportProgress(0);
        progressReporter?.UpdateStatusTranslated("instance.reading.libraries");

        var safeClassPath = new ConcurrentBag<string>(classPath);
        string jsonKey = $"{versionData.MinecraftVersion}-{kind}-{versionData.CustomVersion}";
        string librarySizeCacheFilePath = Path.Combine(cacheDir, "libsizes.json");
        JObject cacheObject;
        if (!File.Exists(librarySizeCacheFilePath)) // Create empty cache file if it does not exist
        {
            cacheObject = new  JObject();
            await File.WriteAllTextAsync(librarySizeCacheFilePath, "{}", cancellationToken);
        }
        else
        {
            string json = await File.ReadAllTextAsync(librarySizeCacheFilePath, cancellationToken);
            cacheObject = JObject.Parse(json);
        }

        // Calculate or read library size
        long overallLibrarySize;
        if (cacheObject.TryGetValue(jsonKey, out var cacheValue))
        {
            overallLibrarySize = cacheValue.Value<long>();
        }
        else
        {
            overallLibrarySize = mcLibs
                .Where(lib => lib.GetRulesResult() && lib.Downloads.Artifact != null)
                .Sum(lib => lib.Downloads.Artifact?.Size ?? 0);

            cacheObject[jsonKey] = overallLibrarySize;
            await File.WriteAllTextAsync(librarySizeCacheFilePath, cacheObject.ToString(), cancellationToken);
        }
        
        var semaphore = new SemaphoreSlim(MaxParallelDownloads);
        long downloadedBytes = 0;
        var tasks = new List<Task>();
        
        // Download libraries
        // Before downloading, we must get rid of duplicates
        // Fixes fabric 0.17.x libraries issue
        var libraryMetas = mcLibs.Where(lib => lib.GetRulesResult()).ToArray();
        foreach (var lib in libraryMetas)
        {
            var libParts = lib.Name.Split(':').ToList();
            var libVersion = libParts[2];
            libParts.RemoveAt(2);
            var libName = string.Join(":", libParts);
            var hasNewerVersion = libraryMetas.Any(otherLib =>
            {
                var otherParts = otherLib.Name.Split(':').ToList();
                if (otherParts.Count < 3) return false;

                var otherVersion = otherParts[2];
                otherParts.RemoveAt(2);
                var otherName = string.Join(":", otherParts);
                return otherName == libName && VersionHelper.isNewer(otherVersion, libVersion);
            });
            if (hasNewerVersion)
                continue;
            
            await semaphore.WaitAsync(cancellationToken);
            var t = Task.Run(async () =>
            {
                try
                {
                    if (lib.Downloads.Artifact != null)
                    {
                        var libFilePath = await DownloadLibraryArtifactAsync(lib, libsDir, progressReporter, cancellationToken);
                        Interlocked.Add(ref downloadedBytes, lib.Downloads.Artifact.Size);
                        
                        if (!string.IsNullOrEmpty(libFilePath) && !safeClassPath.Contains(libFilePath))
                            safeClassPath.Add(libFilePath);
                    }
            
                    if (lib.Downloads.Classifiers != null)
                    {
                        var classifier = lib.Downloads.Classifiers.GetOsNative();
                        var libJarFilePath = Path.Combine(libsDir, classifier.Path);
                        await DownloadNativeFileAsync(classifier.Url, libJarFilePath, lib.Name, versionData.NativesDir, progressReporter, cancellationToken);
                        Interlocked.Add(ref downloadedBytes, classifier.Size);
                        
                        if (!string.IsNullOrEmpty(libJarFilePath) && !safeClassPath.Contains(libJarFilePath))
                            safeClassPath.Add(libJarFilePath);
                    }
                }
                finally
                {
                    progressReporter?.ReportProgress(downloadedBytes / (double)overallLibrarySize * 100d);
                    semaphore.Release();
                }
            }, cancellationToken);
            tasks.Add(t);
        }
        await Task.WhenAll(tasks);
        return safeClassPath.ToList();
    }

    /// <summary>
    /// Downloads a library artifact to the local libraries directory if it is not already present.
    /// </summary>
    /// <param name="lib">The library metadata describing the artifact to download.</param>
    /// <param name="libsDir">The root libraries directory where the artifact should be stored.</param>
    /// <param name="progressReporter">Optional progress reporter used to report download status.</param>
    /// <param name="cancellationToken">Cancellation token observed during the download operation.</param>
    /// <returns>
    /// A task that resolves to the local file path of the downloaded artifact, or an empty string
    /// if the library does not contain an artifact.
    /// </returns>
    private async Task<string> DownloadLibraryArtifactAsync(LibraryMeta lib, string libsDir, IProgressReporter? progressReporter,
        CancellationToken cancellationToken = default)
    {
        if (lib.Downloads.Artifact == null)
            return string.Empty;
        
        string localPath = lib.Downloads.Artifact.Path;
        string libDirPath = Path.Combine(libsDir, Path.GetDirectoryName(localPath)!);
        Directory.CreateDirectory(libDirPath);

        string libFilePath = Path.Combine(libsDir, localPath);
        if (!File.Exists(libFilePath) && !string.IsNullOrEmpty(lib.Downloads.Artifact.Url))
        {
            Progress<double> progress = new Progress<double>();
            progress.ProgressChanged += (_, e) =>
            {
                progressReporter?.ReportProgress(e);
                progressReporter?.UpdateStatusTranslated("instance.downloading.libraries", lib.Name, e.ToString("0.00"));
            };

            await _httpService.DownloadFileAsync(lib.Downloads.Artifact.Url, libFilePath, progress, cancellationToken);
        }

        return libFilePath;
    }

    /// <summary>
    /// Downloads a native library file and extracts its native contents into the specified native directory.
    /// </summary>
    /// <param name="url">The URL of the native archive to download.</param>
    /// <param name="filePath">The local path where the downloaded native archive should be stored.</param>
    /// <param name="libName">The library name used for progress reporting.</param>
    /// <param name="nativeDir">The directory where extracted native files should be placed.</param>
    /// <param name="progressReporter">Optional progress reporter used to report download status.</param>
    /// <param name="cancellationToken">Cancellation token observed during the download operation.</param>
    /// <returns>A task that completes when the native file has been downloaded and extracted.</returns>
    private async Task DownloadNativeFileAsync(string url, string filePath, string libName, string nativeDir,
        IProgressReporter? progressReporter, CancellationToken cancellationToken = default)
    {
        string libDir = Path.GetDirectoryName(filePath)!;
        Directory.CreateDirectory(libDir);

        if (File.Exists(filePath))
        {
            ExtractNativeFiles(filePath,nativeDir);
            return;
        }
        
        Progress<double> progress = new Progress<double>();
        progress.ProgressChanged += (_, e) =>
        {
            progressReporter?.ReportProgress(e);
            progressReporter?.UpdateStatusTranslated("instance.downloading.natives", libName, e.ToString("0.00"));
        };

        await _httpService.DownloadFileAsync(url, filePath, progress, cancellationToken);
        ExtractNativeFiles(filePath,nativeDir);
    }
    
    /// <summary>
    /// Downloads a file from a URL, deserializes its content, and saves it locally if it doesn't already exist.
    /// </summary>
    /// <typeparam name="T">The type to which the file content will be deserialized.</typeparam>
    /// <param name="filePath">The local file path where the file will be saved.</param>
    /// <param name="url">The URL from which the file will be downloaded.</param>
    /// <param name="statusKey">A key used for progress reporting and status messages.</param>
    /// <param name="progressReporter">An optional progress reporter for tracking download progress.</param>
    /// <param name="deserialize">A function to deserialize the file content into the specified type.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>The deserialized object of type <typeparamref name="T"/> or null if the operation fails.</returns>
    private async Task<T?> DownloadAndSaveFileAsync<T>(string filePath, string url, string statusKey,
        IProgressReporter? progressReporter, Func<string, T?> deserialize, CancellationToken cancellationToken = default)
    {
        if (File.Exists(filePath))
        {
            progressReporter?.UpdateStatusTranslated($"instance.reading.{statusKey}", Path.GetFileName(filePath));
            string jsonResult = await File.ReadAllTextAsync(filePath, cancellationToken);
            return deserialize(jsonResult);
        }

        progressReporter?.ReportProgress(0);
        Progress<double> progress = new Progress<double>();
        progress.ProgressChanged += (_, e) =>
        {
            progressReporter?.ReportProgress(e);
            progressReporter?.UpdateStatusTranslated($"instance.downloading.{statusKey}", Path.GetFileName(filePath),
                e.ToString("0.00"));
        };

        string? result = await _httpService.GetStringAsync(url, progress, cancellationToken);
        if (result == null)
            return default;

        T? deserializedResult = deserialize(result);
        if (deserializedResult != null)
            await File.WriteAllTextAsync(filePath, result, cancellationToken);

        return deserializedResult;
    }

    /// <summary>
    /// Downloads a binary file from a URL and saves it locally if it doesn't already exist.
    /// </summary>
    /// <param name="filePath">The local file path where the file will be saved.</param>
    /// <param name="url">The URL from which the file will be downloaded.</param>
    /// <param name="statusKey">A key used for progress reporting and status messages.</param>
    /// <param name="progressReporter">An optional progress reporter for tracking download progress.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A byte array containing the file content or null if the operation fails.</returns>
    private async Task DownloadAndSaveBinaryFileAsync(string filePath, string url, string statusKey,
        IProgressReporter? progressReporter, CancellationToken cancellationToken = default)
    {
        if (File.Exists(filePath))
        {
            progressReporter?.UpdateStatusTranslated($"instance.reading.{statusKey}", Path.GetFileName(filePath));
            return;
        }

        progressReporter?.ReportProgress(0);
        Progress<double> progress = new Progress<double>();
        progress.ProgressChanged += (_, e) =>
        {
            progressReporter?.ReportProgress(e);
            progressReporter?.UpdateStatusTranslated($"instance.downloading.{statusKey}", Path.GetFileName(filePath),
                e.ToString("0.00"));
        };

        await _httpService.DownloadFileAsync(url, filePath, progress, cancellationToken);
    }
    
    /// <summary>
    /// Extracts native library files from a compressed archive and moves them to the specified directory.
    /// </summary>
    /// <param name="libFilePath">The file path of the compressed native library archive.</param>
    /// <param name="nativeDir">The directory where the extracted native files will be stored.</param>
    private void ExtractNativeFiles(string libFilePath, string nativeDir)
    {
        string tempDir = Path.Combine(nativeDir, Path.GetRandomFileName());
        try
        {
            ZipFile.ExtractToDirectory(libFilePath, tempDir, true);

            string searchPattern = "*.so";
            if (OSHelper.GetOperatingSystem() == EOperatingSystem.Windows)
                searchPattern = "*.dll";

            foreach (var file in Directory.GetFiles(tempDir, searchPattern, SearchOption.AllDirectories))
            {
                if ((Environment.Is64BitOperatingSystem && file.Contains("32")) ||
                    (!Environment.Is64BitOperatingSystem && !file.Contains("32")))
                    continue;

                string destFile = Path.Combine(nativeDir, Path.GetFileName(file));
                if (!File.Exists(destFile))
                    File.Move(file, destFile, true);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Failed to extract native files from {libFilePath}: {ex}");
        }
        finally
        {
            FileSystemHelper.DeleteDirectory(tempDir);
        }
    }
}