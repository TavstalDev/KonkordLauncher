using System.Text;

namespace KonkordLibrary.Managers
{
    public static class GameManager
    {
        #region Manifests
        // Vanilla
        private static readonly string _mcVersionManifestUrl = "https://launchermeta.mojang.com/mc/game/version_manifest.json";
        public static string MCVerisonManifestUrl {  get { return _mcVersionManifestUrl; } }

        // Forge
        private static readonly string _forgeVersionManifestUrl = "https://maven.minecraftforge.net/net/minecraftforge/forge/maven-metadata.xml";
        public static string ForgeVersionManifest { get { return _forgeVersionManifestUrl; } }

        // Fabric
        private static readonly string _fabricVersionManifestUrl = "https://meta.fabricmc.net/v2/versions/game";
        public static string FabricVersionManifestUrl { get { return _fabricVersionManifestUrl; } }

        // Quilt
        private static readonly string _quiltVersionManifestUrl = "https://meta.quiltmc.org/v3/versions";
        public static string QuiltVersionManifestUrl { get { return _quiltVersionManifestUrl; } }
        #endregion

        #region Client Urls
        // Vanilla's are in the manifest
        // Neoforge is the same as forge (but why, I can't tell)
        private static readonly string _forgeDownloadUrl = "https://files.minecraftforge.net/maven/net/minecraftforge/forge/{0}/forge-{0}-universal.jar";
        public static string ForgeDownloadUrl { get { return _forgeDownloadUrl; } }

        // Fabric
        private static readonly string _fabricDownloadUrl = "https://maven.fabricmc.net/net/fabricmc/fabric-loader/{0}/fabric-loader-{0}.jar";
        public static string FabricDownloadUrl { get { return _fabricDownloadUrl; } }

        // Quilt
        private static readonly string _quiltDownloadUrl = "https://maven.quiltmc.org/repository/release/org/quiltmc/quilt-loader/{0}/quilt-loader-{0}.jar";
        public static string QuiltDownloadUrl { get { return _quiltDownloadUrl; } }
        #endregion

        #region Version Functions
        // TODO: implement multiple functions from LaunchWindow - LaunchPlay_Click to make more maintanable code. 
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
