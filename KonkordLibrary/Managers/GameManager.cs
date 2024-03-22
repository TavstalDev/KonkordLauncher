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
        // - json
        private static readonly string _forgeVersionManifestUrl = "https://maven.minecraftforge.net/net/minecraftforge/forge/maven-metadata.xml";
        public static string ForgeVersionManifest { get { return _forgeVersionManifestUrl; } }
        #endregion

        #region Fabric
        // https://maven.fabricmc.net/net/fabricmc/fabric-loader/0.15.6/fabric-loader-0.15.6.json
        // https://meta.fabricmc.net/v2/versions/loader/1.16.5/0.15.6/profile/json - json 
        private static readonly string _fabricVersionManifestUrl = "https://meta.fabricmc.net/v2/versions";
        public static string FabricVersionManifestUrl { get { return _fabricVersionManifestUrl; } }
        #endregion

        #region Quilt
        // https://meta.quiltmc.org/v3/versions/loader/1.20.4/0.24.0/profile/json - json
        private static readonly string _quiltVersionManifestUrl = "https://meta.quiltmc.org/v3/versions";
        public static string QuiltVersionManifestUrl { get { return _quiltVersionManifestUrl; } }
        #endregion
        #endregion

        #region Client Urls
        // Vanilla's are in the manifest

        #region Forge & Neoforge - is the same as forge (but why, I can't tell)
        private static readonly string _forgeDownloadUrl = "https://files.minecraftforge.net/maven/net/minecraftforge/forge/{0}/forge-{0}-universal.jar"; 
        // Version Example: 1.20.4-49.0.38
        public static string ForgeDownloadUrl { get { return _forgeDownloadUrl; } }
        #endregion

        // Fabric 
        private static readonly string _fabricDownloadUrl = "https://maven.fabricmc.net/net/fabricmc/fabric-loader/{0}/fabric-loader-{0}.jar"; 
        // Version Example: 0.15.6
        public static string FabricDownloadUrl { get { return _fabricDownloadUrl; } }

        // Quilt
        private static readonly string _quiltDownloadUrl = "https://maven.quiltmc.org/repository/release/org/quiltmc/quilt-loader/{0}/quilt-loader-{0}.jar";
        // Version Example: 0.24.0
        public static string QuiltDownloadUrl { get { return _quiltDownloadUrl; } }
        #endregion

        #region Version Functions
        public static VersionResponse GetProfileVersionDetails(string versionId, string? vanillaVersionId = null, string? customDirectory = null)
        {
            VersionResponse response = new VersionResponse();

            response.InstanceVersion = versionId;
            response.VanillaVersion = vanillaVersionId ?? versionId;
            response.VersionDirectory = Path.Combine(IOHelper.VersionsDir, versionId);
            response.VersionJsonPath = Path.Combine(response.VersionDirectory, $"{versionId}.json");
            response.VersionJarPath = Path.Combine(response.VersionDirectory, $"{versionId}.jar");
            if (string.IsNullOrEmpty(customDirectory))
                response.GameDir = Path.Combine(IOHelper.InstancesDir, versionId);
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

                        return GetProfileVersionDetails(profile.VersionId, profile.VersionVanillaId, profile.GameDirectory);
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
                                await JsonHelper.WriteJsonFileAsync(librarySizeCachePath, localLibrarySize);
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
                        libraryResponse.IsSuccess = true;
                        break;
                    }
                case EProfileKind.FORGE:
                    {
                        // Download vanilla stuff
                        VersionResponse versionResponse = GetProfileVersionDetails(request.VanillaVersion, request.VanillaVersion);
                        LibraryResponse localResponse = await DownloadLibraries(new LibraryRequest(EProfileKind.VANILLA, request.VanillaVersion, request.VanillaVersion, request.VersionJsonUrl, versionResponse.VersionJsonPath, request.ProgressBar, request.Label));
                        if (!localResponse.IsSuccess)
                        {
                           libraryResponse.IsSuccess = false;
                            libraryResponse.Message = localResponse.Message;
                            return libraryResponse;
                        }

                        string versionJsonUrl = string.Format(ForgeDownloadUrl, request.Version);
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
                                string resultJson = await client.GetStringAsync(versionJsonUrl);

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
                                await JsonHelper.WriteJsonFileAsync(librarySizeCachePath, localLibrarySize);
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
