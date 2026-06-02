using Tavstal.KonkordLauncher.Core.Models.ModLoaders;
using Tavstal.KonkordLauncher.Core.Models.MojangApi;

namespace Tavstal.KonkordLauncher.Core.Services.Abstractions;

/// <summary>
/// Provides access to mod loader and Minecraft version manifests with caching and path-based invalidation.
/// </summary>
public interface IManifestService
{
    /// <summary>
    /// Returns the currently-cached Minecraft version manifest, or <see langword="null"/>
    /// if no manifest is cached.
    /// </summary>
    /// <returns>The cached <see cref="VersionManifest"/> or <see langword="null"/> if none is cached.</returns>
    VersionManifest? GetMinecraftManifest();
    
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
    /// Returns the cached Fabric mod loader manifest entries, or <see langword="null"/>
    /// if no Fabric manifest is cached.
    /// </summary>
    /// <returns>Cached list of <see cref="IModManifest"/> entries for Fabric, or <see langword="null"/>.</returns>
    List<IModManifest>? GetFabricManifest();

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
    /// Returns the cached Quilt mod loader manifest entries, or <see langword="null"/>
    /// if no Quilt manifest is cached.
    /// </summary>
    /// <returns>Cached list of <see cref="IModManifest"/> entries for Quilt, or <see langword="null"/>.</returns>
    List<IModManifest>? GetQuiltManifest();
    
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
    /// Returns the cached Forge mod loader manifest entries, or <see langword="null"/>
    /// if no Forge manifest is cached.
    /// </summary>
    /// <returns>Cached list of <see cref="IModManifest"/> entries for Forge, or <see langword="null"/>.</returns>
    List<IModManifest>? GetForgeManifest();
    
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
    /// Returns the cached NeoForge mod loader manifest entries, or <see langword="null"/>
    /// if no NeoForge manifest is cached.
    /// </summary>
    /// <returns>Cached list of <see cref="IModManifest"/> entries for NeoForge, or <see langword="null"/>.</returns>
    List<IModManifest>? GetNeoForgeManifest();
    
    /// <summary>
    /// Gets the NeoForge mod loader manifest from the specified path, retrieving from cache if valid.
    /// </summary>
    /// <param name="manifestPath">The path to the NeoForge manifest file.</param>
    /// <returns>
    /// A task that resolves to a list of <see cref="IModManifest"/> entries if successful; otherwise, <see langword="null"/>.
    /// </returns>
    Task<List<IModManifest>?> GetNeoForgeManifestAsync(string manifestPath);
}