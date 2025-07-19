using System.Diagnostics;
using System.Globalization;
using System.IO.Compression;
using System.Runtime.InteropServices;
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

public class MinecraftFileService
{
    /// <summary>
    /// Downloads the version metadata and JAR file for a specific Minecraft version.
    /// </summary>
    /// <param name="versionData">The details of the version to be downloaded, including paths.</param>
    /// <param name="minecraftVersion">The Minecraft version information, including ID and URL.</param>
    /// <param name="progressReporter">
    /// Optional: An object to report progress and status updates during the download process.
    /// </param>
    /// <returns>
    /// A <see cref="VersionMeta"/> object containing metadata about the downloaded version,
    /// or null if the download fails.
    /// </returns>
    public static async Task<VersionMeta?> DownloadVersionAsync(VersionDetails versionData, MinecraftVersion minecraftVersion, IProgressReporter? progressReporter = null)
    {
        VersionMeta? versionResult;
        // JSON
        if (!File.Exists(versionData.VersionJsonPath))
        {
            progressReporter?.SetProgress(0);
            progressReporter?.SetStatusTranslated($"ui_downloading_version_json", minecraftVersion.Id, 0);

            Progress<double> progress = new Progress<double>();
            progress.ProgressChanged += (sender, e) =>
            {
                progressReporter?.SetStatusTranslated("ui_downloading_version_json", minecraftVersion.Id, e.ToString("0.00"));
            };
            string? jsonResult = await HttpHelper.GetStringAsync(minecraftVersion.Url, progress);
            if (jsonResult == null)
                return null;

            versionResult = JsonConvert.DeserializeObject<VersionMeta>(jsonResult);
            await File.WriteAllTextAsync(versionData.VersionJsonPath, jsonResult);
        }
        else
        {
            progressReporter?.SetStatusTranslated("ui_reading_version_json", minecraftVersion.Id);
            string jsonResult = await File.ReadAllTextAsync(versionData.VersionJsonPath);
            versionResult = JsonConvert.DeserializeObject<VersionMeta>(jsonResult);
        }

        if (versionResult == null)
            return null;

        // JAR
        if (!File.Exists(versionData.VersionJarPath))
        {
            progressReporter?.SetStatusTranslated($"ui_downloading_version_jar", minecraftVersion.Id, 0);

            Progress<double> progress = new Progress<double>();
            progress.ProgressChanged += (_, e) =>
            {
                progressReporter?.SetProgress(e);
                progressReporter?.SetStatusTranslated("ui_downloading_version_jar", minecraftVersion.Id, e.ToString("0.00"));
            };

            byte[]? bytes = await HttpHelper.GetByteArrayAsync(versionResult.Downloads.Client.Url, progress);
            if (bytes == null)
                return null;
            await File.WriteAllBytesAsync(versionData.VersionJarPath, bytes);
        }

        return versionResult;
    }
    
    /// <summary>
    /// Downloads the assets for a specific Minecraft version, including asset index and individual asset files.
    /// </summary>
    /// <param name="versionMeta">The metadata of the Minecraft version, including asset index information.</param>
    /// <param name="progressReporter">
    /// Optional: An object to report progress and status updates during the asset download process.
    /// </param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task DownloadAssetsAsync(VersionMeta versionMeta, IProgressReporter? progressReporter = null)
    {
        // AssetIndex
        progressReporter?.SetProgress(0);
        progressReporter?.SetStatusTranslated("ui_checking_asset_index_json", versionMeta.Index.Id);
        
        string assetIndex = versionMeta.Index.Id;
        string assetPath = Path.Combine(PathHelper.AssetsDir, $"indexes/{assetIndex}.json");
        JToken? assetJToken;
        if (!File.Exists(assetPath))
        {
            progressReporter?.SetStatusTranslated("ui_downloading_asset_index_json", assetIndex, 0);

            Progress<double> progress = new Progress<double>();
            progress.ProgressChanged += (_, e) =>
            {
                progressReporter?.SetProgress(e);
                progressReporter?.SetStatusTranslated("ui_downloading_asset_index_json", assetIndex, e.ToString("0.00"));
            };

            string? resultJson = await HttpHelper.GetStringAsync(versionMeta.Index.Url, progress);
            if (resultJson == null)
                return;

            assetJToken = JObject.Parse(resultJson)["objects"];
            await File.WriteAllTextAsync(assetPath, resultJson);
        }
        else
        {
            progressReporter?.SetStatusTranslated("ui_reading_asset_index_json", assetIndex);
            string resultJson = await File.ReadAllTextAsync(assetPath);
            assetJToken = JObject.Parse(resultJson)["objects"];
        }


        if (assetJToken == null)
            throw new Exception("Asset JToken is null, something went wrong while reading the asset index JSON.");

        // Asset Dir
        string assetObjectDir = Path.Combine(PathHelper.AssetsDir, "objects");
        if (!Directory.Exists(assetObjectDir))
            Directory.CreateDirectory(assetObjectDir);

        // Assets

        int downloadedAssetSize = 0;
        progressReporter?.SetStatusTranslated("ui_checking_assets");
        foreach (JToken token in assetJToken.ToList())
        {
            var rawHash = token.First?["hash"];
            if (rawHash == null)
                continue;
            
            var hash = rawHash.ToString();
            var objectDir = Path.Combine(assetObjectDir, hash.Substring(0, 2));
            var objectPath = Path.Combine(objectDir, $"{hash}");

            if (!Directory.Exists(objectDir))
                Directory.CreateDirectory(objectDir);

            if (!File.Exists(objectPath))
            {
                byte[]? array =
                    await HttpHelper.GetByteArrayAsync(
                        $"{MicrosoftEndpoints.MinecraftResourcesUrl}/{hash.Substring(0, 2)}/{hash}");
                if (array == null)
                    return;

                await File.WriteAllBytesAsync(objectPath, array);
            }

            var sizeToken = token.First?["size"];
            downloadedAssetSize += sizeToken != null ? int.Parse(sizeToken.ToString()) : 0;
            double percent = (double)downloadedAssetSize / (double)versionMeta.Index.TotalSize * 100d;
            progressReporter?.SetStatusTranslated("ui_downloading_assets", percent.ToString("0.00"));
        }
    }

    /// <summary>
    /// Downloads the logging configuration file for a specific Minecraft version and updates its paths.
    /// </summary>
    /// <param name="versionMeta">The metadata of the Minecraft version, including logging information.</param>
    /// <param name="versionDirectory">The directory where the version-specific files are stored.</param>
    /// <param name="gameDir">The directory where the game files are located.</param>
    /// <param name="progressReporter">
    /// Optional: An object to report progress and status updates during the logging configuration download process.
    /// </param>
    /// <returns>
    /// A <see cref="LaunchArg"/> object containing the logging argument for launching the game,
    /// or null if the logging configuration download fails.
    /// </returns>
    public static async Task<LaunchArg?> DownloadLoggingAsync(VersionMeta versionMeta, string versionDirectory, string gameDir, IProgressReporter? progressReporter = null)
    {
        if (versionMeta.LoggingMeta is not { Client: not null })
            return null;
        
        progressReporter?.SetProgress(0);
        progressReporter?.SetStatusTranslated("ui_checking_logging");
        
        string logDirPath = Path.Combine(PathHelper.AssetsDir, "log_configs");
        if (!Directory.Exists(logDirPath))
            Directory.CreateDirectory(logDirPath);
        //string logReadmePath = Path.Combine(logDirPath, "readme.txt");
        /*if (!File.Exists(logReadmePath))
            await File.WriteAllTextAsync(logReadmePath,
                $"The log config files has been moved to {PathHelper.VersionsDir}\\<your_version> so the logs can be made per instance.");*/
        string logFilePath = Path.Combine(versionDirectory, versionMeta.LoggingMeta.Client.File.Id);
        if (!File.Exists(logFilePath))
        {
            progressReporter?.SetStatusTranslated("ui_downloading_logging", 0);

            Progress<double> progress = new Progress<double>();
            progress.ProgressChanged += (_, e) =>
            {
                progressReporter?.SetProgress(e);
                progressReporter?.SetStatusTranslated("ui_downloading_logging", e.ToString("0.00"));
            };

            string? r = await HttpHelper.GetStringAsync(versionMeta.LoggingMeta.Client.File.Url, progress);
            if (r == null)
                return null;

            // FIX LOG LOCATION
            r = r.Replace("fileName=\"logs", $"fileName=\"{gameDir}\\logs")
                .Replace("filePattern=\"logs", $"filePattern=\"{gameDir}\\logs");

            await File.WriteAllTextAsync(logFilePath, r);
        }

        return new LaunchArg(versionMeta.LoggingMeta.Client.Argument.Replace("${path}", logFilePath), 0);
    }

    /// <summary>
    /// Downloads the client mappings file for a specific Minecraft version if it does not already exist.
    /// </summary>
    /// <param name="versionMeta">The metadata of the Minecraft version, including download information for client mappings.</param>
    /// <param name="versionData">The details of the version, including the directory where files are stored.</param>
    /// <param name="progressReporter">
    /// Optional: An object to report progress and status updates during the download process.
    /// </param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task DownloadMappingsAsync(VersionMeta versionMeta, VersionDetails versionData, IProgressReporter? progressReporter = null)
    {
        progressReporter?.SetProgress(0);
        progressReporter?.SetStatusTranslated("ui_checking_client_mappings");
        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        if (versionMeta.Downloads.ClientMappings == null)
            return;

        string clientMappinsPath = Path.Combine(versionData.VersionDirectory, "client.txt");
        if (!File.Exists(clientMappinsPath))
        {
            progressReporter?.SetStatusTranslated("ui_downloading_client_mappings", 0);

            Progress<double> progress = new Progress<double>();
            progress.ProgressChanged += (_, e) =>
            {
                progressReporter?.SetProgress(e);
                progressReporter?.SetStatusTranslated("ui_downloading_client_mappings", e.ToString("0.00"));
            };

            string? r = await HttpHelper.GetStringAsync(versionMeta.Downloads.ClientMappings.Url, progress);
            if (r == null)
                return;

            await File.WriteAllTextAsync(clientMappinsPath, r);
        }
    }

    /// <summary>
    /// Downloads the required libraries for a specific Minecraft version and updates the classpath.
    /// </summary>
    /// <param name="kind">The type of Minecraft (e.g., Java, Bedrock).</param>
    /// <param name="VersionData">The details of the version, including paths and metadata.</param>
    /// <param name="mcLibs">A list of library metadata objects to be downloaded.</param>
    /// <param name="classPath">The classpath string to be updated with downloaded libraries.</param>
    /// <param name="progressReporter">
    /// Optional: An object to report progress and status updates during the library download process.
    /// </param>
    /// <returns>
    /// A tuple containing the updated classpath string and a list of native libraries.
    /// </returns>
    public static async Task<(string, List<LibraryMeta>)> DownloadLibrariesAsync(EMinecraftKind kind, VersionDetails VersionData, List<LibraryMeta> mcLibs, string classPath, IProgressReporter? progressReporter = null)
    {
        progressReporter?.SetProgress(0);
        progressReporter?.SetStatusTranslated("ui_checking_libraries");
        
        List<LibraryMeta> natives = [];
        double libraryOverallSize = 0;
        string libraryCacheDir = Path.Combine(PathHelper.CacheDir, "libsizes");
        if (!Directory.Exists(libraryCacheDir))
            Directory.CreateDirectory(libraryCacheDir);
        string librarySizeCacheFilePath = Path.Combine(libraryCacheDir,
            $"{VersionData.MinecraftVersion}-{kind}-{VersionData.CustomVersion}.json");


        // Calculate the overallSize or read it from cache
        progressReporter?.SetStatusTranslated("ui_calculating_lib_size");
        if (!File.Exists(librarySizeCacheFilePath))
        {
            foreach (var lib in mcLibs)
            {
                // Check the library rule
                if (!lib.GetRulesResult())
                    continue;

                // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
                if (lib.Downloads.Artifact == null)
                    continue;

                libraryOverallSize += lib.Downloads.Artifact.Size;
            }

            await File.WriteAllTextAsync(librarySizeCacheFilePath, libraryOverallSize.ToString(CultureInfo.InvariantCulture));
        }

        // Download the actual libs
        foreach (var lib in mcLibs)
        {
            // Check the library rule
            if (!lib.GetRulesResult())
                continue;

            string libFilePath = string.Empty;
            // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
            if (lib.Downloads.Artifact != null)
            {
                string localPath = lib.Downloads.Artifact.Path.Replace('/', '\\');
                int libDirIndex = localPath.LastIndexOf('\\');
                string libDirPath = Path.Combine(PathHelper.LibrariesDir,
                    localPath.Remove(libDirIndex, localPath.Length - libDirIndex));

                if (!Directory.Exists(libDirPath))
                    Directory.CreateDirectory(libDirPath);

                libFilePath = Path.Combine(PathHelper.LibrariesDir, localPath);
                if (!File.Exists(libFilePath))
                {
                    try
                    {
                        if (!string.IsNullOrEmpty(lib.Downloads.Artifact.Url))
                        {
                            Progress<double> progress = new Progress<double>();
                            progress.ProgressChanged += (_, e) =>
                            {
                                progressReporter?.SetProgress(e);
                                progressReporter?.SetStatusTranslated("ui_library_download", lib.Name, e.ToString("0.00"));
                            };

                            byte[]? bytes = await HttpHelper.GetByteArrayAsync(lib.Downloads.Artifact.Url, progress);
                            if (bytes == null)
                                continue;

                            await File.WriteAllBytesAsync(libFilePath, bytes);
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine($"Failed to get file: {lib.Downloads.Artifact.Url}");
                        Debug.WriteLine($"Error: {e}");
                    }
                }
            }
            else if (lib.Downloads.Classifiers != null)
            {
                string[] rawUrl = lib.Name.Split(':');
                string localPath = Path
                    .Combine(rawUrl[0].Replace('.', '\\'), rawUrl[1], rawUrl[2], $"{rawUrl[1]}-{rawUrl[2]}.jar")
                    .Replace('/', '\\');
                int libDirIndex = localPath.LastIndexOf('\\');
                string libDirPath = Path.Combine(PathHelper.LibrariesDir,
                    localPath.Remove(libDirIndex, localPath.Length - libDirIndex));

                if (!Directory.Exists(libDirPath))
                    Directory.CreateDirectory(libDirPath);

                libFilePath = Path.Combine(PathHelper.LibrariesDir, localPath);
            }

            if (!classPath.Contains(libFilePath))
            {
                classPath += $"{libFilePath};";
                if (lib.Name.StartsWith("org.lwjgl") || lib.Downloads.Classifiers != null)
                    natives.Add(lib);
            }
            else
            {
                if (lib.Name.StartsWith("org.lwjgl") || lib.Downloads.Classifiers != null)
                    natives.Add(lib);
            }
        }

        return (classPath, natives);
    }

    /// <summary>
    /// Downloads and extracts native libraries required for a specific Minecraft version.
    /// </summary>
    /// <param name="nativeLibs">A list of native library metadata objects to be downloaded and extracted.</param>
    /// <param name="nativeDir">The directory where the extracted native files will be stored.</param>
    /// <param name="progressReporter">
    /// Optional: An object to report progress and status updates during the native library download and extraction process.
    /// </param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task DownloadNatives(List<LibraryMeta> nativeLibs, string nativeDir, IProgressReporter? progressReporter = null)
    {
        progressReporter?.SetProgress(0);
        progressReporter?.SetStatusTranslated("ui_checking_natives");
        
        if (!Directory.Exists(nativeDir))
            Directory.CreateDirectory(nativeDir);

        foreach (LibraryMeta lib in nativeLibs)
        {
            Debug.WriteLine("nativeLib: " + lib.Name);
            string libJarFilePath;
            if (lib.Downloads.Classifiers != null)
            {
                string localUrl;
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
                    if (lib.Downloads.Classifiers.WindowsNatives == null)
                        continue;

                    localUrl = lib.Downloads.Classifiers.WindowsNatives.Url;
                    libJarFilePath = lib.Downloads.Classifiers.WindowsNatives.Path;
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
                    if (lib.Downloads.Classifiers.LinuxNatives == null)
                        continue;

                    localUrl = lib.Downloads.Classifiers.LinuxNatives.Url;
                    libJarFilePath = lib.Downloads.Classifiers.LinuxNatives.Path;
                }
                else // OSX
                {
                    // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
                    if (lib.Downloads.Classifiers.OsxNatives == null)
                        continue;

                    localUrl = lib.Downloads.Classifiers.OsxNatives.Url;
                    libJarFilePath = lib.Downloads.Classifiers.OsxNatives.Path;
                }

                // Add dir
                string localFilePath = Path.Combine(PathHelper.LibrariesDir, libJarFilePath);

                if (!File.Exists(localFilePath))
                {
                    string libJarDir = localFilePath.Remove(localFilePath.LastIndexOf('/'),
                        localFilePath.Length - localFilePath.LastIndexOf('/'));
                    if (!Directory.Exists(libJarDir))
                        Directory.CreateDirectory(libJarDir);

                    Progress<double> progress = new Progress<double>();
                    progress.ProgressChanged += (_, e) =>
                    {
                        progressReporter?.SetProgress(e);
                        progressReporter?.SetStatusTranslated("ui_library_download", lib.Name, e.ToString("0.00"));
                    };

                    byte[]? bytes = await HttpHelper.GetByteArrayAsync(localUrl, progress);
                    if (bytes == null)
                        continue;

                    await File.WriteAllBytesAsync(localFilePath, bytes);
                }
            }
            else
                continue;

            string libFilePath = Path.Combine(PathHelper.LibrariesDir, libJarFilePath);
            List<string> dirBaseRaw = libJarFilePath.Split('.').ToList();
            dirBaseRaw.RemoveAt(dirBaseRaw.Count - 1);

            string dirRaw = string.Empty;
            foreach (var ba in dirBaseRaw)
            {
                if (string.IsNullOrEmpty(dirRaw))
                    dirRaw += ba;
                else
                    dirRaw += $".{ba}";
            }
            
            string tempZipDir = Path.Combine(Path.GetTempPath(), dirRaw);
            ZipFile.ExtractToDirectory(libFilePath, tempZipDir, true);

            string[] files = Directory.GetFiles(tempZipDir, "*.dll", searchOption: SearchOption.AllDirectories);
            if (files != null)
            {
                foreach (string file in files)
                {
                    if (Environment.Is64BitOperatingSystem && file.Contains("32"))
                        continue;
                    if (!Environment.Is64BitOperatingSystem && !file.Contains("32"))
                        continue;

                    string fileName = file.Remove(0, file.LastIndexOf('\\') + 1);
                    string filePath = Path.Combine(nativeDir, fileName);
                    if (!File.Exists(filePath))
                        File.Move(file, filePath);
                }
            }

            FileSystemHelper.DeleteDirectory(tempZipDir);
        }
    }
}