using KonkordLibrary.Enums;
using KonkordLibrary.Helpers;
using KonkordLibrary.Models.Minecraft;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Net.Http;
using System;
using System.Text;
using System.Windows.Controls;
using Newtonsoft.Json;
using KonkordLibrary.Models.GameManager;
using KonkordLibrary.Models;
using System.Diagnostics;
using System.IO.Compression;

namespace KonkordLibrary.Managers
{
    public static class GameManager
    {
        #region Manifests
        #region Vanilla
        // File jsons can be get throught the manifest
        private static readonly string _mcVersionManifestUrl = "https://launchermeta.mojang.com/mc/game/version_manifest.json";
        public static string MCVerisonManifestUrl { get { return _mcVersionManifestUrl; } }
        #endregion

        #region Forge
        private static readonly string _forgeVersionManifestUrl = "https://maven.minecraftforge.net/net/minecraftforge/forge/maven-metadata.xml";
        public static string ForgeVersionManifest { get { return _forgeVersionManifestUrl; } }
        #endregion

        #region Fabric
        private static readonly string _fabricVersionManifestUrl = "https://meta.fabricmc.net/v2/versions";
        public static string FabricVersionManifestUrl { get { return _fabricVersionManifestUrl; } }
        #endregion

        #region Quilt
        private static readonly string _quiltVersionManifestUrl = "https://meta.quiltmc.org/v3/versions";
        public static string QuiltVersionManifestUrl { get { return _quiltVersionManifestUrl; } }
        #endregion
        #endregion

        #region Client Urls
        // Vanilla's are in the manifest

        #region Forge & Neoforge 
        // neoforge is the same as forge (but why, I can't tell)
        private static readonly string _forgeLoaderJarUrl = "https://files.minecraftforge.net/maven/net/minecraftforge/forge/{0}/forge-{0}-universal.jar"; 
        // Version Example: 1.20.4-49.0.38
        public static string ForgeLoaderJarUrl { get { return _forgeLoaderJarUrl; } }
        private static readonly string _forgeInstallerJarUrl = "https://files.minecraftforge.net/maven/net/minecraftforge/forge/{0}/forge-{0}-installer.jar";
        // Version Example: 1.20.4-49.0.38
        public static string ForgeInstallerJarUrl { get { return _forgeInstallerJarUrl; } }
        #endregion

        #region Fabric
        private static readonly string _fabricLoaderJsonUrl = "https://meta.fabricmc.net/v2/versions/loader/{0}/{1}/profile/json";
        // https://meta.fabricmc.net/v2/versions/loader/1.16.5/0.15.6/profile/json
        public static string FabricLoaderJsonUrl { get {  return _fabricLoaderJsonUrl; } }
        private static readonly string _fabricLoaderJarUrl = "https://maven.fabricmc.net/net/fabricmc/fabric-loader/{0}/fabric-loader-{0}.jar"; 
        // Version Example: 0.15.6
        public static string FabricLoaderJarUrl { get { return _fabricLoaderJarUrl; } }
        #endregion

        #region Quilt
        private static readonly string _quiltLoaderJsonUrl = "https://meta.quiltmc.org/v3/versions/loader/{0}/{1}/profile/json";
        // https://meta.quiltmc.org/v3/versions/loader/1.20.4/0.24.0/profile/json
        public static string QuiltLoaderJsonUrl { get { return _quiltLoaderJsonUrl; } }
        private static readonly string _quiltLoaderJarUrl = "https://maven.quiltmc.org/repository/release/org/quiltmc/quilt-loader/{0}/quilt-loader-{0}.jar";
        // Version Example: 0.24.0
        public static string QuiltLoaderJarUrl { get { return _quiltLoaderJarUrl; } }
        #endregion
        #endregion

        #region Version Functions
        public static VersionResponse GetProfileVersionDetails(string versionId, string? vanillaVersionId = null, string? customDirectory = null, string extraNameTag = "")
        {
            VersionResponse response = new VersionResponse();

            response.InstanceVersion = versionId;
            response.VanillaVersion = vanillaVersionId ?? versionId;
            response.VersionDirectory = Path.Combine(IOHelper.VersionsDir, $"{versionId}{extraNameTag}");
            response.VersionJsonPath = Path.Combine(response.VersionDirectory, $"{versionId}{extraNameTag}.json");
            response.VersionJarPath = Path.Combine(response.VersionDirectory, $"{versionId}{extraNameTag}.jar");
            if (string.IsNullOrEmpty(customDirectory))
                response.GameDir = Path.Combine(IOHelper.InstancesDir, $"{versionId}{extraNameTag}");
            else
                response.GameDir = customDirectory;

            return response;
        }

        public static VersionResponse GetProfileVersionDetails(EProfileType type, VersionManifest? manifest = null, Profile? profile = null)
        {
            switch (type)
            {
                case EProfileType.LATEST_RELEASE:
                    {
                        if (manifest == null)
                            throw new ArgumentNullException(nameof(manifest));

                        return GetProfileVersionDetails(manifest.Latest.Release, manifest.Latest.Release, null);
                    }
                case EProfileType.LATEST_SNAPSHOT:
                    {
                        if (manifest == null)
                            throw new ArgumentNullException(nameof(manifest));

                        return GetProfileVersionDetails(manifest.Latest.Snapshot, manifest.Latest.Snapshot, null);
                    }
                case EProfileType.CUSTOM:
                    {
                        if (profile == null)
                            throw new ArgumentNullException(nameof(profile));

                        string nameTag = "";
                        switch (profile.Kind)
                        {
                            case EProfileKind.FORGE:
                                {
                                    nameTag = "-forge";
                                    break;
                                }
                            case EProfileKind.FABRIC:
                                {
                                    nameTag = "-fabric";
                                    break;
                                }
                            case EProfileKind.QUILT:
                                {
                                    nameTag = "-quilt";
                                    break;
                                }
                        }

                        return GetProfileVersionDetails(profile.VersionId, profile.VersionVanillaId, profile.GameDirectory, nameTag);
                    }
                case EProfileType.KONKORD_CREATE:
                case EProfileType.KONKORD_VANILLAPLUS:
                    {
                        // TODO at the end
                        throw new NotImplementedException();
                    }
                default:
                    {
                        throw new NotImplementedException("How did we get here ?");
                    }
            }
        }

        public static async Task<LibraryResponse> DownloadLibraries(LibraryRequest request)
        {
            LibraryResponse libraryResponse = new LibraryResponse();
            switch (request.Kind)
            {
                case EProfileKind.VANILLA:
                    {
                        request.Label.Content = $"Checking asset details...";
                        // Check Cache Directory
                        string librarySizeCacheDir = Path.Combine(IOHelper.CacheDir, "libsizes");
                        if (!Directory.Exists(librarySizeCacheDir))
                            Directory.CreateDirectory(librarySizeCacheDir);
                        // Get Cache File Path
                        string librarySizeCachePath = Path.Combine(librarySizeCacheDir, $"{request.Version}.json");

                        // Check the version json file
                        if (!File.Exists(request.VersionJsonPath))
                        {
                            request.Label.Content = $"Downloading the version json file...";
                            using (var client = new HttpClient())
                            {
                                // Send the request
                                string resultJson = await client.GetStringAsync(request.VersionJsonUrl);

                                // Get jobject
                                JObject localObj = JObject.Parse(resultJson);
                                int localLibrarySize = 0;

                                JToken? librariesJToken = localObj["libraries"];
                                if (librariesJToken == null)
                                {
                                    libraryResponse.Message = "The version json file did not contain the 'libraries' field.";
                                    return libraryResponse;
                                }

                                JArray localjArray = JArray.Parse(librariesJToken.ToString(Formatting.None));
                                // Check the libraries
                                foreach (var jToken in localjArray)
                                {
                                    localLibrarySize += int.Parse(jToken["downloads"]["artifact"]["size"].ToString());
                                }
                                // Save the version cache
                                await File.WriteAllTextAsync(librarySizeCachePath, localLibrarySize.ToString());
                                // Save the version json itself
                                await File.WriteAllTextAsync(request.VersionJsonPath, resultJson);
                            }
                        }

                        // Get the version json object
                        JObject obj = JObject.Parse(await File.ReadAllTextAsync(request.VersionJsonPath));

                        // Get and check jtoken
                        JToken? assetIndexJToken = obj["assetIndex"];
                        if (assetIndexJToken == null)
                        {
                            libraryResponse.Message = "The version json file did not contain the 'assetIndex' field.";
                            return libraryResponse;
                        }

                        // Get and check jtoken
                        JToken? assetIdJToken = assetIndexJToken["id"];
                        if (assetIdJToken == null)
                        {
                            libraryResponse.Message = "The assetIndex jtoken did not contain the 'id' field.";
                            return libraryResponse;
                        }

                        // Get the asset index
                        libraryResponse.AssetIndex = assetIdJToken.ToString();


                        // Get and check jtoken
                        request.Label.Content = $"Checking asset json...";
                        JToken? assetTotalSizeJToken = assetIndexJToken["totalSize"];
                        if (assetTotalSizeJToken == null)
                        {
                            libraryResponse.Message = "The assetIndex jtoken did not contain the 'totalSize' field.";
                            return libraryResponse;
                        }

                        // Get asset total size
                        int totalAssetSize = int.Parse(assetTotalSizeJToken.ToString());
                        // Get Asset Index dir
                        string assetIndexDir = Path.Combine(IOHelper.AssetsDir, "indexes");
                        // Get Asset json
                        string assetIndexJsonPath = Path.Combine(assetIndexDir, $"{libraryResponse.AssetIndex}.json");
                        // Download the asset index json
                        if (!File.Exists(assetIndexJsonPath))
                        {
                            request.Label.Content = $"Downloading assets json...";
                            using (var client = new HttpClient())
                            {
                                byte[] array = await client.GetByteArrayAsync(obj["assetIndex"]["url"].ToString());
                                await File.WriteAllBytesAsync(assetIndexJsonPath, array);
                            }
                        }

                        // Check Asset Objects
                        string assetObjectDir = Path.Combine(IOHelper.AssetsDir, "objects");
                        if (!Directory.Exists(assetObjectDir))
                            Directory.CreateDirectory(assetObjectDir);

                        // Download Assets
                        request.Label.Content = $"Checking assets... 0%";
                        string rawAssetJson = await File.ReadAllTextAsync(assetIndexJsonPath);
                        JToken assetIndexToken = JObject.Parse(rawAssetJson)["objects"];
                        using (var client = new HttpClient())
                        {
                            string hash = string.Empty;
                            string objectDir = string.Empty;
                            string objectPath = string.Empty;
                            int downloadedAssetSize = 0;
                            request.Label.Content = $"Downloading assets... 0%";
                            foreach (JToken token in assetIndexToken.ToList())
                            {
                                hash = token.First["hash"].ToString();
                                objectDir = Path.Combine(assetObjectDir, hash.Substring(0, 2));
                                objectPath = Path.Combine(objectDir, $"{hash}");

                                if (!Directory.Exists(objectDir))
                                    Directory.CreateDirectory(objectDir);

                                if (!File.Exists(objectPath))
                                {
                                    byte[] array = await client.GetByteArrayAsync($"https://resources.download.minecraft.net/{hash.Substring(0, 2)}/{hash}");
                                    await File.WriteAllBytesAsync(objectPath, array);
                                }
                                downloadedAssetSize += int.Parse(token.First["size"].ToString());
                                request.ProgressBar.Value = (double)downloadedAssetSize / (double)totalAssetSize * 100d;
                                request.Label.Content = $"Downloading assets... {request.ProgressBar.Value:0.00}%";
                            }
                        }
                        libraryResponse.ClientDownloadUrl = obj["downloads"]["client"]["url"].ToString();
                        libraryResponse.Libraries = JArray.Parse(obj["libraries"].ToString(Formatting.None));
                        libraryResponse.LibrarySizeCachePath = librarySizeCachePath;
                        libraryResponse.LocalLibrarySize = 0;
                        libraryResponse.IsSuccess = true;
                        break;
                    }
                case EProfileKind.FORGE:
                    {
                        // Download vanilla stuff
                        VersionResponse versionResponse = GetProfileVersionDetails(request.VanillaVersion, request.VanillaVersion);
                        if (!Directory.Exists(versionResponse.VersionDirectory))
                            Directory.CreateDirectory(versionResponse.VersionDirectory);

                        LibraryResponse localResponse = await DownloadLibraries(new LibraryRequest(EProfileKind.VANILLA, request.VanillaVersion, request.VanillaVersion, request.VersionJsonUrl, versionResponse.VersionJsonPath, request.ProgressBar, request.Label));
                        if (!localResponse.IsSuccess)
                        {
                            libraryResponse.IsSuccess = false;
                            libraryResponse.Message = localResponse.Message;
                            return libraryResponse;
                        }
                        libraryResponse.Libraries = localResponse.Libraries;

                        // Check Cache Directory
                        string librarySizeCacheDir = Path.Combine(IOHelper.CacheDir, "libsizes");
                        libraryResponse.LocalLibrarySize = int.Parse(await File.ReadAllTextAsync(localResponse.LibrarySizeCachePath));

                        string forgeVersion = $"{request.VanillaVersion}-{request.Version}";

                        // Download Forge Installer to temp
                        libraryResponse.ClientDownloadUrl = string.Format(ForgeLoaderJarUrl, forgeVersion);

                        
                        string versionJsonUrl = string.Format(ForgeInstallerJarUrl, forgeVersion);
                        request.Label.Content = $"Checking asset details...";
                        
                        // Get Cache File Path
                        string librarySizeCachePath = Path.Combine(librarySizeCacheDir, $"{request.Version}.json");

                        string forgeInstallerFilePath = Path.Combine(IOHelper.TempDir, $"{request.Version}-installer.jar");
                        string forgeInstallerDirPath = Path.Combine(IOHelper.TempDir, $"{request.Version}-installer");
                        string forgeInstallerVersionPath = Path.Combine(forgeInstallerDirPath, $"version.json");

                        // Check the version json file
                        if (!File.Exists(request.VersionJsonPath))
                        {
                            request.Label.Content = $"Downloading the version json file...";
                            using (var client = new HttpClient())
                            {
                                // Send the request to download the installer
                                byte[] installerBytes = await client.GetByteArrayAsync(versionJsonUrl);
                                await File.WriteAllBytesAsync(forgeInstallerFilePath, installerBytes);

                                // Extract installer jar
                                ZipFile.ExtractToDirectory(forgeInstallerFilePath, forgeInstallerDirPath);

                                // Move version.json
                                File.Move(forgeInstallerVersionPath, request.VersionJsonPath);

                                // Delete temps
                                var forgeInstallerDirInfo = new DirectoryInfo(forgeInstallerDirPath);
                                foreach (System.IO.FileInfo file in forgeInstallerDirInfo.GetFiles()) file.Delete();
                                foreach (System.IO.DirectoryInfo subDirectory in forgeInstallerDirInfo.GetDirectories()) subDirectory.Delete(true);
                                Directory.Delete(forgeInstallerDirPath);
                                File.Delete(forgeInstallerFilePath);

                                // Get jobject
                                JObject localObj = JObject.Parse(await File.ReadAllTextAsync(request.VersionJsonPath));
                                int localLibrarySize = 0;

                                JToken? librariesJToken = localObj["libraries"];
                                if (librariesJToken == null)
                                {
                                    libraryResponse.Message = "The version json file did not contain the 'libraries' field.";
                                    return libraryResponse;
                                }

                                JArray localjArray = JArray.Parse(librariesJToken.ToString(Formatting.None));
                                // Check the libraries
                                foreach (var jToken in localjArray)
                                {
                                    localLibrarySize += int.Parse(jToken["downloads"]["artifact"]["size"].ToString());
                                    libraryResponse.Libraries.Add(jToken);
                                }
                                // Save the version cache
                                await JsonHelper.WriteJsonFileAsync(librarySizeCachePath, localLibrarySize);

                                libraryResponse.CustomGameArgs = new List<string>();
                                libraryResponse.CustomJavaArgs = new List<string>();
                                libraryResponse.CustomGameMain = localObj["mainClass"].ToString();
                                foreach (var arg in localObj["arguments"]["game"].ToList())
                                {
                                    libraryResponse.CustomGameArgs.Add(arg.ToString());
                                }
                                foreach (var arg in localObj["arguments"]["jvm"].ToList())
                                {
                                    string rawArg = arg.ToString();
                                    if (rawArg.StartsWith("-p") || rawArg.StartsWith("${library_directory}"))
                                    {
                                        continue;
                                    }
                                    libraryResponse.CustomGameArgs.Add(rawArg);
                                }
                            }
                        }
                        else
                        {
                            JObject localObj = JObject.Parse(await File.ReadAllTextAsync(request.VersionJsonPath));

                            JToken? librariesJToken = localObj["libraries"];
                            if (librariesJToken == null)
                            {
                                libraryResponse.Message = "The version json file did not contain the 'libraries' field.";
                                return libraryResponse;
                            }

                            JArray localjArray = JArray.Parse(librariesJToken.ToString(Formatting.None));
                            // Check the libraries
                            foreach (var jToken in localjArray)
                            { 
                                libraryResponse.Libraries.Add(jToken);
                            }

                            libraryResponse.CustomGameArgs = new List<string>();
                            libraryResponse.CustomJavaArgs = new List<string>();
                            libraryResponse.CustomGameMain = localObj["mainClass"].ToString();
                            foreach (var arg in localObj["arguments"]["game"].ToList())
                            {
                                libraryResponse.CustomGameArgs.Add(arg.ToString());
                            }
                            foreach (var arg in localObj["arguments"]["jvm"].ToList())
                            {
                                string rawArg = arg.ToString();
                                if (rawArg.StartsWith("-p") || rawArg.StartsWith("${library_directory}"))
                                {
                                    continue;
                                }
                                libraryResponse.CustomGameArgs.Add(rawArg);
                            }
                        }

                        // Get the vanilla version json object
                        JObject vanillaVersionJsonObj = JObject.Parse(await File.ReadAllTextAsync(versionResponse.VersionJsonPath));

                        // Get and check jtoken
                        JToken? assetIndexJToken = vanillaVersionJsonObj["assetIndex"];
                        if (assetIndexJToken == null)
                        {
                            libraryResponse.Message = "The version json file did not contain the 'assetIndex' field.";
                            return libraryResponse;
                        }

                        // Get and check jtoken
                        JToken? assetIdJToken = assetIndexJToken["id"];
                        if (assetIdJToken == null)
                        {
                            libraryResponse.Message = "The assetIndex jtoken did not contain the 'id' field.";
                            return libraryResponse;
                        }

                        // Get the asset index
                        libraryResponse.AssetIndex = assetIdJToken.ToString();


                        // Check Asset Objects
                        string assetObjectDir = Path.Combine(IOHelper.AssetsDir, "objects");
                        if (!Directory.Exists(assetObjectDir))
                            Directory.CreateDirectory(assetObjectDir);

                        libraryResponse.LibrarySizeCachePath = librarySizeCachePath;
                        libraryResponse.IsSuccess = true;
                        break;
                    }
                case EProfileKind.FABRIC:
                    {

                        break;
                    }
                case EProfileKind.QUILT:
                    {

                        break;
                    }
            }
            return libraryResponse;
        }
        #endregion

        #region Functions
        /// <summary>
        /// Retrieves the UUID (Universally Unique Identifier) of a player based on the provided username.
        /// </summary>
        /// <param name="username">The username of the player.</param>
        /// <returns>
        /// A <see cref="string"/> representing the UUID of the player.
        /// </returns>
        private static string GetPlayerUUID(string username)
        {
            //new GameProfile(UUID.nameUUIDFromBytes(("OfflinePlayer:" + name).getBytes(Charsets.UTF_8)), name));
            byte[] rawresult = System.Security.Cryptography.MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(username));
            //set the version to 3 -> Name based md5 hash
            rawresult[6] = (byte)(rawresult[6] & 0x0f | 0x30);
            //IETF variant
            rawresult[8] = (byte)(rawresult[8] & 0x3f | 0x80);
            //convert to string and remove any - if any
            string finalresult = BitConverter.ToString(rawresult).Replace("-", "");
            //formatting
            finalresult = finalresult.Insert(8, "-").Insert(13, "-").Insert(18, "-").Insert(23, "-");
            return finalresult;
        }

        /// <summary>
        /// Retrieves the UUID (Universally Unique Identifier) of an offline player based on the provided username.
        /// </summary>
        /// <param name="username">The username of the offline player.</param>
        /// <returns>
        /// A <see cref="string"/> representing the UUID of the offline player.
        /// </returns>
        public static string GetOfflinePlayerUUID(string username)
        {
            return GetPlayerUUID($"OfflinePlayer:{username}");
        }

        /// <summary>
        /// Retrieves the UUID (Universally Unique Identifier) of an online player based on the provided username.
        /// </summary>
        /// <param name="username">The username of the online player.</param>
        /// <returns>
        /// A <see cref="string"/> representing the UUID of the online player.
        /// </returns>
        public static string GetOnlinePlayerUUID(string username)
        {
            return GetPlayerUUID($"{username}");
        }
        #endregion
    }
}
