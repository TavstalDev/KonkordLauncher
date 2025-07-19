namespace Tavstal.KonkordLauncher.Core.Models.Endpoints;

/// <summary>
/// Provides constants for Forge-related endpoints used in the application.
/// </summary>
public static class ForgeEndpoints
{
    /// <summary>
    /// The URL for retrieving the Forge version manifest.
    /// </summary>
    public const string VersionManifest = "https://maven.minecraftforge.net/net/minecraftforge/forge/maven-metadata.xml";

    /// <summary>
    /// The URL template for downloading the Forge universal JAR file for a specific version.
    /// </summary>
    /// <remarks>
    /// Replace `{0}` with the Forge version (e.g., "1.20.4-49.0.38").
    /// Example: https://files.minecraftforge.net/maven/net/minecraftforge/forge/1.20.4-49.0.38/forge-1.20.4-49.0.38-universal.jar
    /// </remarks>
    public const string LoaderUniversalJarUrl = "https://files.minecraftforge.net/maven/net/minecraftforge/forge/{0}/forge-{0}-universal.jar";

    /// <summary>
    /// The URL template for downloading the Forge installer JAR file for a specific version.
    /// </summary>
    /// <remarks>
    /// Replace `{0}` with the Forge version (e.g., "1.20.4-49.0.38").
    /// Example: https://files.minecraftforge.net/maven/net/minecraftforge/forge/1.20.4-49.0.38/forge-1.20.4-49.0.38-installer.jar
    /// </remarks>
    public const string InstallerJarUrl = "https://files.minecraftforge.net/maven/net/minecraftforge/forge/{0}/forge-{0}-installer.jar";
}