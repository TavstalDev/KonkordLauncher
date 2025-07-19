namespace Tavstal.KonkordLauncher.Core.Models.Endpoints;

/// <summary>
/// Provides constants for Quilt-related endpoints used in the application.
/// </summary>
public static class QuiltEndpoints
{
    /// <summary>
    /// The URL for retrieving the Quilt version manifest.
    /// </summary>
    public const string VersionManifestUrl = "https://meta.quiltmc.org/v3/versions";

    /// <summary>
    /// The URL template for retrieving the Quilt loader JSON profile for a specific Minecraft version and loader version.
    /// </summary>
    /// <remarks>
    /// Replace `{0}` with the Minecraft version and `{1}` with the Quilt loader version.
    /// Example: https://meta.quiltmc.org/v3/versions/loader/1.20.4/0.24.0/profile/json
    /// </remarks>
    public const string LoaderJsonUrl = "https://meta.quiltmc.org/v3/versions/loader/{0}/{1}/profile/json";

    /// <summary>
    /// The URL template for downloading the Quilt loader JAR file for a specific version.
    /// </summary>
    /// <remarks>
    /// Replace `{0}` with the Quilt loader version.
    /// Example: https://maven.quiltmc.org/repository/release/org/quiltmc/quilt-loader/0.24.0/quilt-loader-0.24.0.jar
    /// </remarks>
    public const string LoaderJarUrl = "https://maven.quiltmc.org/repository/release/org/quiltmc/quilt-loader/{0}/quilt-loader-{0}.jar";
}