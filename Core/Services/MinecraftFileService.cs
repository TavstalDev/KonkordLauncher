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
        IProgressReporter? progressReporter, Func<string, T?> deserialize)
    {
        if (File.Exists(filePath))
        {
            progressReporter?.SetStatusTranslated($"instance.reading.{statusKey}", Path.GetFileName(filePath));
            string jsonResult = await File.ReadAllTextAsync(filePath);
            return deserialize(jsonResult);
        }

        progressReporter?.SetProgress(0);
        Progress<double> progress = new Progress<double>();
        progress.ProgressChanged += (_, e) =>
        {
            progressReporter?.SetProgress(e);
            progressReporter?.SetStatusTranslated($"instance.downloading.{statusKey}", Path.GetFileName(filePath),
                e.ToString("0.00"));
        };

        string? result = await HttpHelper.GetStringAsync(url, progress);
        if (result == null)
        {
            return default;
        }

        T? deserializedResult = deserialize(result);
        if (deserializedResult != null)
        {
            await File.WriteAllTextAsync(filePath, result);
        }

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
        IProgressReporter? progressReporter)
    {
        if (File.Exists(filePath))
        {
            progressReporter?.SetStatusTranslated($"instance.reading.{statusKey}", Path.GetFileName(filePath));
            return;
        }

        progressReporter?.SetProgress(0);
        Progress<double> progress = new Progress<double>();
        progress.ProgressChanged += (_, e) =>
        {
            progressReporter?.SetProgress(e);
            progressReporter?.SetStatusTranslated($"instance.downloading.{statusKey}", Path.GetFileName(filePath),
                e.ToString("0.00"));
        };

        await HttpHelper.DownloadFileAsync(url, filePath, progress);
    }

    /// <summary>
    /// Downloads the version metadata and client JAR file for a specific Minecraft version.
    /// </summary>
    /// <param name="versionData">The details of the version to be downloaded.</param>
    /// <param name="minecraftVersion">The Minecraft version metadata.</param>
    /// <param name="progressReporter">An optional progress reporter for tracking download progress.</param>
    /// <returns>The deserialized version metadata or null if the operation fails.</returns>
    public static async Task<VersionMeta?> DownloadVersionAsync(VersionDetails versionData,
        MinecraftVersion minecraftVersion, IProgressReporter? progressReporter = null)
    {
        // JSON
        var versionResult = await DownloadAndSaveFileAsync(
            versionData.VersionJsonPath,
            minecraftVersion.Url,
            "version_json",
            progressReporter,
            JsonConvert.DeserializeObject<VersionMeta>);

        if (versionResult == null) return null;

        // JAR
        await DownloadAndSaveBinaryFileAsync(
            versionData.VersionJarPath,
            versionResult.Downloads.Client.Url,
            "version_jar",
            progressReporter);

        return versionResult;
    }
    
    public static async Task DownloadAssetsAsync(VersionMeta versionMeta, string assetsDir, string gameDir,
        IProgressReporter? progressReporter = null)
    {
        // AssetIndex
        string assetIndexId = versionMeta.Index.Id;
        string assetPath = Path.Combine(assetsDir, $"indexes/{assetIndexId}.json");

        string? resultJson = await DownloadAndSaveFileAsync(
            assetPath,
            versionMeta.Index.Url,
            "asset_index_json",
            progressReporter,
            (json) => json); // Deserialize to string, then parse JObject

        if (resultJson == null) return;

        bool isLegacy = resultJson.Contains("READ_ME_I_AM_VERY_IMPORTANT");
        
        var assetJToken = JObject.Parse(resultJson)["objects"];
        if (assetJToken == null)
            throw new Exception("Asset JToken is null, something went wrong while reading the asset index JSON.");

        // Assets
        int downloadedAssetSize = 0;
        progressReporter?.SetStatusTranslated("instance.reading.assets");
        
        if (isLegacy)
        {
            string resourcesDir = Path.Combine(gameDir, "resources");
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

                if (!File.Exists(objectPath))
                {
                     await HttpHelper.DownloadFileAsync(
                        $"{MicrosoftEndpoints.MinecraftResourcesUrl}/{hash.Substring(0, 2)}/{hash}", objectPath, null);
                }
                
                var sizeToken = token.First?["size"];
                downloadedAssetSize += sizeToken != null ? int.Parse(sizeToken.ToString()) : 0;
                double percent = (double)downloadedAssetSize / (double)versionMeta.Index.TotalSize * 100d;
                progressReporter?.SetStatusTranslated("instance.downloading.assets", percent.ToString("0.00"));
            }
            
            return;
        }
        
        // Asset Dir
        string assetObjectDir = Path.Combine(assetsDir, "objects");
        Directory.CreateDirectory(assetObjectDir);
        
        foreach (JToken token in assetJToken.ToList())
        {
            var rawHash = token.First?["hash"];
            if (rawHash == null) continue;

            var hash = rawHash.ToString();
            var objectDir = Path.Combine(assetObjectDir, hash.Substring(0, 2));
            var objectPath = Path.Combine(objectDir, $"{hash}");

            Directory.CreateDirectory(objectDir);

            if (!File.Exists(objectPath))
            {
                await HttpHelper.DownloadFileAsync(
                    $"{MicrosoftEndpoints.MinecraftResourcesUrl}/{hash.Substring(0, 2)}/{hash}", objectPath, null);
            }

            var sizeToken = token.First?["size"];
            downloadedAssetSize += sizeToken != null ? int.Parse(sizeToken.ToString()) : 0;
            double percent = (double)downloadedAssetSize / (double)versionMeta.Index.TotalSize * 100d;
            progressReporter?.SetStatusTranslated("instance.downloading.assets", percent.ToString("0.00"));
        }
    }

    /// <summary>
    /// Downloads and modifies the logging configuration for a specific Minecraft version.
    /// </summary>
    /// <param name="versionMeta">The metadata of the Minecraft version.</param>
    /// <param name="versionDirectory">The directory where the version files are stored.</param>
    /// <param name="gameDir">The directory where the game files are stored.</param>
    /// <param name="assetsDir">The directory where the assets are stored.</param>
    /// <param name="progressReporter">An optional progress reporter for tracking download progress.</param>
    /// <returns>A launch argument for the logging configuration or null if the operation fails.</returns>
    public static async Task<LaunchArg?> DownloadLoggingAsync(VersionMeta versionMeta, string versionDirectory,
        string gameDir, string assetsDir, IProgressReporter? progressReporter = null)
    {
        if (versionMeta.LoggingMeta is not { Client: not null }) return null;

        string logDirPath = Path.Combine(assetsDir, "log_configs");
        Directory.CreateDirectory(logDirPath);

        string logFilePath = Path.Combine(versionDirectory, versionMeta.LoggingMeta.Client.File.Id);

        string? logContent = await DownloadAndSaveFileAsync(
            logFilePath,
            versionMeta.LoggingMeta.Client.File.Url,
            "logging",
            progressReporter,
            (json) => json); // Deserialize to string

        if (logContent == null) return null;

        // FIX LOG LOCATION
        string modifiedContent = logContent
            .Replace("fileName=\"logs", $"fileName=\"{gameDir}/logs")
            .Replace("filePattern=\"logs", $"filePattern=\"{gameDir}/logs");

        await File.WriteAllTextAsync(logFilePath, modifiedContent);

        return new LaunchArg(versionMeta.LoggingMeta.Client.Argument.Replace("${path}", logFilePath), 0);
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
        if (versionMeta.Downloads.ClientMappings == null) return;

        string clientMappinsPath = Path.Combine(versionData.VersionDirectory, "client.txt");

        await DownloadAndSaveFileAsync(
            clientMappinsPath,
            versionMeta.Downloads.ClientMappings.Url,
            "client_mappings",
            progressReporter,
            (json) => json); // Deserialize to string
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
    public static async Task<string> DownloadLibrariesAsync(
        EMinecraftKind kind, VersionDetails versionData, List<LibraryMeta> mcLibs,
        string classPath, string cacheDir, string libsDir, IProgressReporter? progressReporter = null)
    {
        progressReporter?.SetProgress(0);
        progressReporter?.SetStatusTranslated("instance.reading.libraries");
        
        string libraryCacheDir = Path.Combine(cacheDir, "libsizes");
        Directory.CreateDirectory(libraryCacheDir);

        string librarySizeCacheFilePath = Path.Combine(libraryCacheDir,
            $"{versionData.MinecraftVersion}-{kind}-{versionData.CustomVersion}.json");

        // Calculate or read library size
        if (!File.Exists(librarySizeCacheFilePath))
        {
            double libraryOverallSize = mcLibs
                .Where(lib => lib.GetRulesResult() && lib.Downloads.Artifact != null)
                .Sum(lib => lib.Downloads.Artifact?.Size ?? 0);

            await File.WriteAllTextAsync(librarySizeCacheFilePath,
                libraryOverallSize.ToString(CultureInfo.InvariantCulture));
        }

        // Download libraries
        foreach (var lib in mcLibs.Where(lib => lib.GetRulesResult()))
        {
            if (lib.Downloads.Artifact != null)
            {
                var libFilePath = await DownloadLibraryArtifactAsync(lib, libsDir, progressReporter);
                if (!string.IsNullOrEmpty(libFilePath) && !classPath.Contains(libFilePath))
                    classPath += $"{libFilePath}${{classpath_separator}}";
            }
            
            if (lib.Downloads.Classifiers != null)
            {
                var classifier = lib.Downloads.Classifiers.GetOsNative();
                var libJarFilePath = Path.Combine(libsDir, classifier.Path);
                await DownloadNativeFileAsync(classifier.Url, libJarFilePath, lib.Name, versionData.NativesDir, progressReporter);
                if (!string.IsNullOrEmpty(libJarFilePath) && !classPath.Contains(libJarFilePath))
                    classPath += $"{libJarFilePath}${{classpath_separator}}";
            }
        }

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
        LibraryMeta lib, string libsDir, IProgressReporter? progressReporter)
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
                progressReporter?.SetProgress(e);
                progressReporter?.SetStatusTranslated("instance.downloading.libraries", lib.Name, e.ToString("0.00"));
            };

            await HttpHelper.DownloadFileAsync(lib.Downloads.Artifact.Url, libFilePath, progress);
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
        string url, string filePath, string libName, string nativeDir, IProgressReporter? progressReporter)
    {
        string libDir = Path.GetDirectoryName(filePath)!;
        Directory.CreateDirectory(libDir);

        Progress<double> progress = new Progress<double>();
        progress.ProgressChanged += (_, e) =>
        {
            progressReporter?.SetProgress(e);
            progressReporter?.SetStatusTranslated("instance.downloading.natives", libName, e.ToString("0.00"));
        };

        await HttpHelper.DownloadFileAsync(url, filePath, progress);
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