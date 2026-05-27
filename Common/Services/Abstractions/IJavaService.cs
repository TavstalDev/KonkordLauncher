using Tavstal.KonkordLauncher.Common.Models.Java;

namespace Tavstal.KonkordLauncher.Common.Services.Abstractions;

/// <summary>
/// Provides Java runtime discovery, version detection, and download operations.
/// </summary>
public interface IJavaService
{
    /// <summary>
    /// Downloads a specific Java major version and extracts it to the specified target directory.
    /// </summary>
    /// <param name="majorVersion">The major version of Java to download (e.g., 8, 11, 17, 21).</param>
    /// <param name="targetPath">The directory where the downloaded and extracted Java installation will be placed.</param>
    /// <param name="progress">An optional progress reporter that receives download progress as a percentage (0–100).</param>
    /// <param name="cancellationToken">A cancellation token observed during the download and extraction operations.</param>
    /// <returns>
    /// A task that resolves to <see langword="true"/> if the download and extraction completed successfully;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    Task<bool> DownloadJavaVersionAsync(int majorVersion, string targetPath,
        Progress<double>? progress = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks whether Java is installed and accessible on the system.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token used to cancel the installation check operation.</param>
    /// <returns>
    /// A task that resolves to <see langword="true"/> if Java is installed and can be executed;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    Task<bool> IsJavaInstalledAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Retrieves detailed version and architecture information for a Java installation.
    /// </summary>
    /// <param name="path">The file path to the Java executable (e.g., "java" on Linux/macOS or "javaw.exe" on Windows).</param>
    /// <param name="cancellationToken">A cancellation token used to cancel the version retrieval operation.</param>
    /// <returns>
    /// A <see cref="JavaVersion"/> object containing the major version, full version string, and
    /// architecture; or <see langword="null"/> if the Java executable cannot be queried or the version
    /// information cannot be parsed.
    /// </returns>
    Task<JavaVersion?> GetJavaVersionDetailsAsync(string path, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Locates all Java installations on the system, optionally searching a specific directory or refreshing the cache.
    /// </summary>
    /// <param name="instanceJavaDir">
    /// An optional directory path to search for Java installations. If provided, this directory is searched first.
    /// If <see langword="null"/>, only default system directories are searched.
    /// </param>
    /// <param name="forceRefresh">A boolean indicating whether to bypass the cache and perform a fresh search.</param>
    /// <param name="cancellationToken">A cancellation token used to cancel the Java discovery operation.</param>
    /// <returns>A list of <see cref="JavaVersion"/> objects representing all discovered Java installations.</returns>
    Task<List<JavaVersion>> LocateJavaInstallationsAsync(string? instanceJavaDir = null, bool forceRefresh = false, 
        CancellationToken cancellationToken = default);
}