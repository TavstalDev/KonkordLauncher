using Tavstal.KonkordLauncher.Core.Enums;
using Tavstal.KonkordLauncher.Core.Models;
using Tavstal.KonkordLauncher.Core.Models.Installer;
using Tavstal.KonkordLauncher.Core.Models.Instance;
using Tavstal.KonkordLauncher.Core.Models.MojangApi;
using Tavstal.KonkordLauncher.Core.Models.MojangApi.Meta;

namespace Tavstal.KonkordLauncher.Core.Services.Abstractions;

/// <summary>
/// Handles downloading, preparing, and resolving Minecraft runtime libraries and related assets.
/// </summary>
public interface ILibraryDownloadService
{
    /// <summary>
    /// Downloads and resolves the version metadata required for the specified Minecraft version.
    /// </summary>
    /// <param name="versionData">The version details describing the Minecraft version to download.</param>
    /// <param name="minecraftVersion">The Minecraft version being prepared.</param>
    /// <param name="progressReporter">Optional progress reporter for download status updates.</param>
    /// <param name="cancellationToken">Cancellation token observed during the operation.</param>
    /// <returns>
    /// A task that resolves to the downloaded <see cref="VersionMeta"/> if successful; otherwise, <see langword="null"/>.
    /// </returns>
    Task<VersionMeta?> DownloadVersionAsync(VersionDetails versionData,
        MinecraftVersion minecraftVersion, IProgressReporter? progressReporter = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Downloads the assets required by the specified version into the target assets directory.
    /// </summary>
    /// <param name="versionMeta">The version metadata containing asset information.</param>
    /// <param name="assetsDir">The directory where assets should be stored.</param>
    /// <param name="gameDir">The target game directory used for relative asset placement.</param>
    /// <param name="progressReporter">Optional progress reporter for download status updates.</param>
    /// <param name="cancellationToken">Cancellation token observed during the operation.</param>
    /// <returns>A task that completes when all assets have been downloaded.</returns>
    Task DownloadAssetsAsync(VersionMeta versionMeta, string assetsDir, string gameDir,
        IProgressReporter? progressReporter = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Downloads the logging configuration or logging-related artifact for the specified version.
    /// </summary>
    /// <param name="versionMeta">The version metadata containing logging configuration information.</param>
    /// <param name="versionDirectory">The directory for the target version installation.</param>
    /// <param name="gameDir">The target game directory used for runtime setup.</param>
    /// <param name="progressReporter">Optional progress reporter for download status updates.</param>
    /// <param name="cancellationToken">Cancellation token observed during the operation.</param>
    /// <returns>
    /// A task that resolves to the extracted <see cref="LaunchArg"/> if available; otherwise, <see langword="null"/>.
    /// </returns>
    Task<LaunchArg?> DownloadLoggingAsync(VersionMeta versionMeta, string versionDirectory,
        string gameDir, IProgressReporter? progressReporter = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Downloads and prepares mapping files required for the specified Minecraft version.
    /// </summary>
    /// <param name="versionMeta">The version metadata containing mapping information.</param>
    /// <param name="versionData">The version details describing the Minecraft version to prepare.</param>
    /// <param name="progressReporter">Optional progress reporter for download status updates.</param>
    /// <param name="cancellationToken">Cancellation token observed during the operation.</param>
    /// <returns>A task that completes when the mappings have been downloaded.</returns>
    Task DownloadMappingsAsync(VersionMeta versionMeta, VersionDetails versionData,
        IProgressReporter? progressReporter = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Extracts the bundled launch wrapper into the specified libraries directory.
    /// </summary>
    /// <param name="libsDir">The libraries directory where the launch wrapper should be extracted.</param>
    /// <param name="cancellationToken">Cancellation token observed during the operation.</param>
    /// <returns>
    /// A task that resolves to the path of the extracted launch wrapper if successful; otherwise, <see langword="null"/>.
    /// </returns>
    Task<string?> ExtractLaunchWrapperAsync(string libsDir, CancellationToken cancellationToken = default);

    /// <summary>
    /// Downloads all libraries required to build the runtime classpath for a Minecraft instance.
    /// </summary>
    /// <param name="kind">The Minecraft loader kind being prepared.</param>
    /// <param name="versionData">The version details describing the Minecraft version to prepare.</param>
    /// <param name="mcLibs">The list of Minecraft libraries to process.</param>
    /// <param name="classPath">The current classpath entries to extend.</param>
    /// <param name="cacheDir">The cache directory used for library metadata and size tracking.</param>
    /// <param name="libsDir">The target libraries directory.</param>
    /// <param name="progressReporter">Optional progress reporter for download status updates.</param>
    /// <param name="cancellationToken">Cancellation token observed during the operation.</param>
    /// <returns>
    /// A task that resolves to the updated list of classpath entries after all libraries have been downloaded.
    /// </returns>
    Task<List<string>> DownloadLibrariesAsync(
        EMinecraftKind kind, VersionDetails versionData, List<LibraryMeta> mcLibs,
        List<string> classPath, string cacheDir, string libsDir, IProgressReporter? progressReporter = null,
        CancellationToken cancellationToken = default);
}