using System.Globalization;
using System.IO.Compression;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Tavstal.KonkordLauncher.Core.Enums;
using Tavstal.KonkordLauncher.Core.Helpers;
using Tavstal.KonkordLauncher.Core.Models;
using Tavstal.KonkordLauncher.Core.Models.Endpoints;
using Tavstal.KonkordLauncher.Core.Models.Installer;
using Tavstal.KonkordLauncher.Core.Models.MojangApi;
using Tavstal.KonkordLauncher.Core.Models.MojangApi.Meta;

namespace Tavstal.KonkordLauncher.Core.Services;

public static class MinecraftFileService
{
    private static readonly int MaxParallelDownloads = 16;
    
    /// <summary>
    /// Downloads a file from a URL, deserializes its content, and saves it locally if it doesn't already exist.
    /// </summary>
    /// <typeparam name="T">The type to which the file content will be deserialized.</typeparam>
    /// <param name="filePath">The local file path where the file will be saved.</param>
    /// <param name="url">The URL from which the file will be downloaded.</param>
    /// <param name="statusKey">A key used for progress reporting and status messages.</param>
    /// <param name="progressReporter">An optional progress reporter for tracking download progress.</param>
    /// <param name="deserialize">A function to deserialize the file content into the specified type.</param>
    /// <returns>The deserialized object of type <typeparamref name="T"/> or null if the operation fails.</returns>
    private static async Task<T?> DownloadAndSaveFileAsync<T>(string filePath, string url, string statusKey,
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

        string? result = await HttpHelper.GetStringAsync(url, progress, cancellationToken);
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
    /// <returns>A byte array containing the file content or null if the operation fails.</returns>
    private static async Task DownloadAndSaveBinaryFileAsync(string filePath, string url, string statusKey,
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

        await HttpHelper.DownloadFileAsync(url, filePath, progress, cancellationToken);
    }

    /// <summary>
    /// Downloads the version metadata and client JAR file for a specific Minecraft version.
    /// </summary>
    /// <param name="versionData">The details of the version to be downloaded.</param>
    /// <param name="minecraftVersion">The Minecraft version metadata.</param>
    /// <param name="progressReporter">An optional progress reporter for tracking download progress.</param>
    /// <returns>The deserialized version metadata or null if the operation fails.</returns>
    public static async Task<VersionMeta?> DownloadVersionAsync(VersionDetails versionData,
        MinecraftVersion minecraftVersion, IProgressReporter? progressReporter = null, CancellationToken cancellationToken = default)
    {
        // JSON
        var versionResult = await DownloadAndSaveFileAsync(
            versionData.VersionJsonPath,
            minecraftVersion.Url,
            "version_json",
            progressReporter,
            JsonConvert.DeserializeObject<VersionMeta>, cancellationToken);

        if (versionResult == null) return null;

        // JAR
        await DownloadAndSaveBinaryFileAsync(
            versionData.VersionJarPath,
            versionResult.Downloads.Client.Url,
            "version_jar",
            progressReporter, cancellationToken);

        // Create default JavaVersionMeta if null
        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        if (versionResult.JavaVersionMeta == null)
            versionResult.JavaVersionMeta = new JavaVersionMeta()
            {
                MajorVersion = 8
            };

        return versionResult;
    }
    
    /// <summary>
    /// Downloads and processes the assets for a specific Minecraft version.
    /// </summary>
    /// <param name="versionMeta">The metadata of the Minecraft version.</param>
    /// <param name="assetsDir">The directory where the assets will be stored.</param>
    /// <param name="gameDir">The directory where the game files are stored.</param>
    /// <param name="progressReporter">An optional progress reporter for tracking download progress.</param>
    public static async Task DownloadAssetsAsync(VersionMeta versionMeta, string assetsDir, string gameDir,
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
                            await HttpHelper.DownloadFileAsync(
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
                            await HttpHelper.DownloadFileAsync(
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
                            await HttpHelper.DownloadFileAsync(
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

    /// <summary>
    /// Downloads and modifies the logging configuration for a specific Minecraft version.
    /// </summary>
    /// <param name="versionMeta">The metadata of the Minecraft version.</param>
    /// <param name="versionDirectory">The directory where the version files are stored.</param>
    /// <param name="gameDir">The directory where the game files are stored.</param>
    /// <param name="progressReporter">An optional progress reporter for tracking download progress.</param>
    /// <returns>A launch argument for the logging configuration or null if the operation fails.</returns>
    public static async Task<LaunchArg?> DownloadLoggingAsync(VersionMeta versionMeta, string versionDirectory,
        string gameDir, IProgressReporter? progressReporter = null, CancellationToken cancellationToken = default)
    {
        if (versionMeta.LoggingMeta is not { Client: not null }) return null;
        
        string logFilePath = Path.Combine(versionDirectory, versionMeta.LoggingMeta.Client.File.Id);

        string? logContent = await DownloadAndSaveFileAsync(
            logFilePath,
            versionMeta.LoggingMeta.Client.File.Url,
            "logging",
            progressReporter,
            json => json, cancellationToken); // Deserialize to string

        if (logContent == null) return null;

        // FIX LOG LOCATION
        string modifiedContent = logContent
            .Replace("fileName=\"logs", $"fileName=\"{gameDir}/logs")
            .Replace("filePattern=\"logs", $"filePattern=\"{gameDir}/logs");

        await File.WriteAllTextAsync(logFilePath, modifiedContent, cancellationToken);

        return new LaunchArg(versionMeta.LoggingMeta.Client.Argument.Replace("${path}", logFilePath), 2);
    }

    /// <summary>
    /// Downloads and saves the client mappings for a specific Minecraft version.
    /// </summary>
    /// <param name="versionMeta">The metadata of the Minecraft version.</param>
    /// <param name="versionData">The details of the version to be downloaded.</param>
    /// <param name="progressReporter">An optional progress reporter for tracking download progress.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task DownloadMappingsAsync(VersionMeta versionMeta, VersionDetails versionData,
        IProgressReporter? progressReporter = null)
    {
        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        if (versionMeta.Downloads.ClientMappings == null) return;

        string clientMappinsPath = Path.Combine(versionData.VersionDirectory, "client.txt");

        await DownloadAndSaveFileAsync(
            clientMappinsPath,
            versionMeta.Downloads.ClientMappings.Url,
            "client_mappings",
            progressReporter,
            json => json); // Deserialize to string
    }

    /// <summary>
    /// Downloads and processes the libraries required for a specific Minecraft version.
    /// </summary>
    /// <param name="kind">The type of Minecraft (e.g., Java, Bedrock).</param>
    /// <param name="versionData">The details of the Minecraft version.</param>
    /// <param name="mcLibs">The list of libraries to be downloaded.</param>
    /// <param name="classPath">The classpath string to be updated with downloaded libraries.</param>
    /// <param name="cacheDir">The directory where cached files are stored.</param>
    /// <param name="libsDir">The directory where libraries will be downloaded.</param>
    /// <param name="progressReporter">An optional progress reporter for tracking download progress.</param>
    /// <returns>A tuple containing the updated classpath and a list of native libraries.</returns>
    public static async Task<List<string>> DownloadLibrariesAsync(
        EMinecraftKind kind, VersionDetails versionData, List<LibraryMeta> mcLibs,
        List<string> classPath, string cacheDir, string libsDir, IProgressReporter? progressReporter = null, CancellationToken cancellationToken = default)
    {
        progressReporter?.ReportProgress(0);
        progressReporter?.UpdateStatusTranslated("instance.reading.libraries");
        
        string libraryCacheDir = Path.Combine(cacheDir, "libsizes");
        Directory.CreateDirectory(libraryCacheDir);

        string librarySizeCacheFilePath = Path.Combine(libraryCacheDir,
            $"{versionData.MinecraftVersion}-{kind}-{versionData.CustomVersion}.json");

        // Calculate or read library size
        long overallLibrarySize;
        if (!File.Exists(librarySizeCacheFilePath))
        {
            overallLibrarySize = mcLibs
                .Where(lib => lib.GetRulesResult() && lib.Downloads.Artifact != null)
                .Sum(lib => lib.Downloads.Artifact?.Size ?? 0);

            await File.WriteAllTextAsync(librarySizeCacheFilePath,
                overallLibrarySize.ToString(CultureInfo.InvariantCulture), cancellationToken);
        }
        else
            overallLibrarySize = long.Parse(await File.ReadAllTextAsync(librarySizeCacheFilePath, cancellationToken), CultureInfo.InvariantCulture);
        
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
                        
                        if (!string.IsNullOrEmpty(libFilePath) && !classPath.Contains(libFilePath))
                            classPath.Add(libFilePath);
                    }
            
                    if (lib.Downloads.Classifiers != null)
                    {
                        var classifier = lib.Downloads.Classifiers.GetOsNative();
                        var libJarFilePath = Path.Combine(libsDir, classifier.Path);
                        await DownloadNativeFileAsync(classifier.Url, libJarFilePath, lib.Name, versionData.NativesDir, progressReporter, cancellationToken);
                        Interlocked.Add(ref downloadedBytes, classifier.Size);
                        
                        if (!string.IsNullOrEmpty(libJarFilePath) && !classPath.Contains(libJarFilePath))
                            classPath.Add(libJarFilePath);
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
        return classPath;
    }

    /// <summary>
    /// Downloads a library artifact and saves it locally.
    /// </summary>
    /// <param name="lib">The metadata of the library to be downloaded.</param>
    /// <param name="libsDir">The directory where the library will be saved.</param>
    /// <param name="progressReporter">An optional progress reporter for tracking download progress.</param>
    /// <returns>The file path of the downloaded library.</returns>
    private static async Task<string> DownloadLibraryArtifactAsync(
        LibraryMeta lib, string libsDir, IProgressReporter? progressReporter, CancellationToken cancellationToken = default)
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

            await HttpHelper.DownloadFileAsync(lib.Downloads.Artifact.Url, libFilePath, progress, cancellationToken);
        }

        return libFilePath;
    }
    
    /// <summary>
    /// Downloads a native library file from the specified URL, saves it to the given file path, 
    /// and extracts its contents to the specified native directory.
    /// </summary>
    /// <param name="url">The URL of the native library file to download.</param>
    /// <param name="filePath">The local file path where the downloaded file will be saved.</param>
    /// <param name="libName">The name of the library being downloaded, used for progress reporting.</param>
    /// <param name="nativeDir">The directory where the extracted native files will be stored.</param>
    /// <param name="progressReporter">An optional progress reporter for tracking download progress.</param>
    private static async Task DownloadNativeFileAsync(
        string url, string filePath, string libName, string nativeDir, IProgressReporter? progressReporter, CancellationToken cancellationToken = default)
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

        await HttpHelper.DownloadFileAsync(url, filePath, progress, cancellationToken);
        ExtractNativeFiles(filePath,nativeDir);
    }
    
    /// <summary>
    /// Extracts native library files from a compressed archive and moves them to the specified directory.
    /// </summary>
    /// <param name="libFilePath">The file path of the compressed native library archive.</param>
    /// <param name="nativeDir">The directory where the extracted native files will be stored.</param>
    private static void ExtractNativeFiles(string libFilePath, string nativeDir)
    {
        string tempDir = Path.Combine(nativeDir, Path.GetRandomFileName());
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
                File.Move(file, destFile);
        }

        Directory.Delete(tempDir, true);
    }
}