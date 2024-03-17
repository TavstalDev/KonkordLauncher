namespace KonkordLauncher.API.Managers
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

    }
}
