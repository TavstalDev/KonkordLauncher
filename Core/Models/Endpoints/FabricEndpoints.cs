namespace Tavstal.KonkordLauncher.Core.Models.Endpoints;

/// <summary>
/// Provides constants for Fabric-related endpoints used in the application.
/// </summary>
public static class FabricEndpoints
{
    /// <summary>
    /// The URL for retrieving the Fabric version manifest.
    /// </summary>
    public const string VersionManifestUrl = "https://meta.fabricmc.net/v2/versions";

    /// <summary>
    /// The URL template for retrieving the Fabric loader JSON profile for a specific Minecraft version and loader version.
    /// </summary>
    /// <remarks>
    /// Replace `{0}` with the Minecraft version and `{1}` with the Fabric loader version.
    /// Example: https://meta.fabricmc.net/v2/versions/loader/1.16.5/0.15.6/profile/json
    /// </remarks>
    public const string LoaderJsonUrl = "https://meta.fabricmc.net/v2/versions/loader/{0}/{1}/profile/json";

    /// <summary>
    /// The URL template for downloading the Fabric loader JAR file for a specific version.
    /// </summary>
    /// <remarks>
    /// Replace `{0}` with the Fabric loader version.
    /// Example: https://maven.fabricmc.net/net/fabricmc/fabric-loader/0.15.6/fabric-loader-0.15.6.jar
    /// </remarks>
    public const string LoaderJarUrl = "https://maven.fabricmc.net/net/fabricmc/fabric-loader/{0}/fabric-loader-{0}.jar";
}