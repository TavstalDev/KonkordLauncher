namespace Tavstal.KonkordLauncher.Core.Models.Endpoints;

public static class NeoForgeEndpoints
{
    /// <summary>
    /// The URL for retrieving the Forge version manifest.
    /// </summary>
    // https://maven.neoforged.net/api/maven/versions/releases/net%2Fneoforged%2Fneoforge
    public const string VersionManifest = "https://maven.neoforged.net/net/neoforged/neoforge/maven-metadata.xml";

    /// <summary>
    /// The URL template for downloading the Forge universal JAR file for a specific version.
    /// </summary>
    /// <remarks>
    /// Replace `{0}` with the Forge version (e.g., "21.8.25").
    /// Example: https://maven.neoforged.net/releases/net/neoforged/neoforge/21.8.25/neoforge-21.8.2-universal.jar
    /// </remarks>
    public const string LoaderUniversalJarUrl = "https://maven.neoforged.net/releases/net/neoforged/neoforge/{0}/neoforge-{0}-universal.jar";

    /// <summary>
    /// The URL template for downloading the Forge installer JAR file for a specific version.
    /// </summary>
    /// <remarks>
    /// Replace `{0}` with the NeoForge version (e.g., "21.8.25").
    /// Example: https://maven.neoforged.net/releases/net/neoforged/neoforge/21.8.25/neoforge-21.8.25-installer.jar
    /// </remarks>
    public const string InstallerJarUrl = "https://maven.neoforged.net/releases/net/neoforged/neoforge/{0}/neoforge-{0}-installer.jar";
}