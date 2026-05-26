using Tavstal.KonkordLauncher.Core.Models.ModLoaders;
using Tavstal.KonkordLauncher.Core.Models.MojangApi;

namespace Tavstal.KonkordLauncher.Core.Services.Abstractions;

/// <summary>
/// Provides access to mod loader and Minecraft version manifests with caching and path-based invalidation.
/// </summary>
public interface IManifestService
{
    /// <summary>
    /// Gets the Minecraft version manifest from the specified path, retrieving from cache if valid.
    /// </summary>
    /// <param name="manifestPath">
    /// The path to the Minecraft version manifest file (typically the Mojang manifest or a cached copy).
    /// </param>
    /// <param name="cancellationToken">Cancellation token observed during the operation.</param>
    /// <returns>
    /// A task that resolves to the <see cref="VersionManifest"/> if successful; otherwise, <see langword="null"/>.
    /// </returns>
    Task<VersionManifest?>
        GetMinecraftManifestAsync(string manifestPath, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the Fabric mod loader manifest from the specified path, retrieving from cache if valid.
    /// </summary>
    /// <param name="manifestPath">The path to the Fabric manifest file.</param>
    /// <param name="cancellationToken">Cancellation token observed during the operation.</param>
    /// <returns>
    /// A task that resolves to a list of <see cref="IModManifest"/> entries if successful; otherwise, <see langword="null"/>.
    /// </returns>
    Task<List<IModManifest>?>
        GetFabricManifestAsync(string manifestPath, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the Quilt mod loader manifest from the specified path, retrieving from cache if valid.
    /// </summary>
    /// <param name="manifestPath">The path to the Quilt manifest file.</param>
    /// <param name="cancellationToken">Cancellation token observed during the operation.</param>
    /// <returns>
    /// A task that resolves to a list of <see cref="IModManifest"/> entries if successful; otherwise, <see langword="null"/>.
    /// </returns>
    Task<List<IModManifest>?> GetQuiltManifestAsync(string manifestPath, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the Forge mod loader manifest from the specified path, retrieving from cache if valid.
    /// </summary>
    /// <param name="manifestPath">The path to the Forge manifest file.</param>
    /// <param name="cancellationToken">Cancellation token observed during the operation.</param>
    /// <returns>
    /// A task that resolves to a list of <see cref="IModManifest"/> entries if successful; otherwise, <see langword="null"/>.
    /// </returns>
    Task<List<IModManifest>?> GetForgeManifestAsync(string manifestPath, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the NeoForge mod loader manifest from the specified path, retrieving from cache if valid.
    /// </summary>
    /// <param name="manifestPath">The path to the NeoForge manifest file.</param>
    /// <returns>
    /// A task that resolves to a list of <see cref="IModManifest"/> entries if successful; otherwise, <see langword="null"/>.
    /// </returns>
    Task<List<IModManifest>?> GetNeoForgeManifestAsync(string manifestPath);
}