using Newtonsoft.Json.Linq;
using Tavstal.KonkordLauncher.Core.Models.ModLoaders;
using Tavstal.KonkordLauncher.Core.Models.ModLoaders.Fabric;
using Tavstal.KonkordLauncher.Core.Models.ModLoaders.Forge;
using Tavstal.KonkordLauncher.Core.Models.MojangApi;

namespace Tavstal.KonkordLauncher.Core.Helpers;

/// <summary>
/// Provides helper methods for managing and retrieving mod loader manifests.
/// </summary>
public static class ManifestHelper
{
    /// <summary>
    /// Stores the Minecraft version manifest.
    /// </summary>
    private static VersionManifest? _minecraftManifest;

    /// <summary>
    /// Retrieves the cached Minecraft version manifest.
    /// </summary>
    /// <returns>The cached <see cref="VersionManifest"/> or null if not loaded.</returns>
    public static VersionManifest? GetMinecraftManifest() => _minecraftManifest;

    /// <summary>
    /// Asynchronously loads the Minecraft version manifest from the specified path.
    /// </summary>
    /// <param name="manifestPath">The file path to the Minecraft manifest.</param>
    /// <returns>The loaded <see cref="VersionManifest"/> or null if loading fails.</returns>
    public static async Task<VersionManifest?> GetMinecraftManifestAsync(string manifestPath, CancellationToken cancellationToken = default)
    {
        if (_minecraftManifest != null)
            return _minecraftManifest;

        _minecraftManifest = await JsonHelper.ReadJsonFileAsync<VersionManifest>(manifestPath, cancellationToken);
        return _minecraftManifest;
    }

    /// <summary>
    /// Stores the Fabric mod loader manifests.
    /// </summary>
    private static List<IModManifest>? _fabricManifest;

    /// <summary>
    /// Retrieves the cached Fabric mod loader manifests.
    /// </summary>
    /// <returns>A list of <see cref="IModManifest"/> or null if not loaded.</returns>
    public static List<IModManifest>? GetFabricManifest() => _fabricManifest;

    /// <summary>
    /// Asynchronously loads the Fabric mod loader manifests from the specified path.
    /// </summary>
    /// <param name="manifestPath">The file path to the Fabric manifest.</param>
    /// <returns>A list of <see cref="IModManifest"/> or null if loading fails.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the loader section is missing in the JSON.</exception>
    public static async Task<List<IModManifest>?> GetFabricManifestAsync(string manifestPath, CancellationToken cancellationToken = default)
    {
        if (_fabricManifest != null)
            return _fabricManifest;

        var rawManifest = await File.ReadAllTextAsync(manifestPath, cancellationToken);
        JObject jObject = JObject.Parse(rawManifest);
        var mappings = jObject["loader"] as JArray;
        if (mappings == null)
        {
            throw new InvalidOperationException("Fabric manifest loader not found in the JSON.");
        }
        _fabricManifest = [];
        foreach (var mapping in mappings)
        {
            _fabricManifest.Add(new FabricManifest(mapping.Value<string>("version")!));
        }

        return _fabricManifest;
    }

    /// <summary>
    /// Stores the Quilt mod loader manifests.
    /// </summary>
    private static List<IModManifest>? _quiltManifest;

    /// <summary>
    /// Retrieves the cached Quilt mod loader manifests.
    /// </summary>
    /// <returns>A list of <see cref="IModManifest"/> or null if not loaded.</returns>
    public static List<IModManifest>? GetQuiltManifest() => _quiltManifest;

    /// <summary>
    /// Asynchronously loads the Quilt mod loader manifests from the specified path.
    /// </summary>
    /// <param name="manifestPath">The file path to the Quilt manifest.</param>
    /// <returns>A list of <see cref="IModManifest"/> or null if loading fails.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the loader section is missing in the JSON.</exception>
    public static async Task<List<IModManifest>?> GetQuiltManifestAsync(string manifestPath, CancellationToken cancellationToken = default)
    {
        if (_quiltManifest != null)
            return _quiltManifest;

        var rawManifest = await File.ReadAllTextAsync(manifestPath, cancellationToken);
        JObject jObject = JObject.Parse(rawManifest);
        var mappings = jObject["loader"] as JArray;
        if (mappings == null)
        {
            throw new InvalidOperationException("Quilt manifest loader not found in the JSON.");
        }
        _quiltManifest = [];
        foreach (var mapping in mappings)
        {
            _quiltManifest.Add(new FabricManifest(mapping.Value<string>("version")!));
        }

        return _quiltManifest;
    }

    /// <summary>
    /// Stores the Forge mod loader manifests.
    /// </summary>
    private static List<ForgeManifest>? _forgeManifest;

    /// <summary>
    /// Retrieves the cached Forge mod loader manifests.
    /// </summary>
    /// <returns>A list of <see cref="ForgeManifest"/> or null if not loaded.</returns>
    public static List<ForgeManifest>? GetForgeManifest() => _forgeManifest;

    /// <summary>
    /// Asynchronously loads the Forge mod loader manifests from the specified path.
    /// </summary>
    /// <param name="manifestPath">The file path to the Forge manifest.</param>
    /// <returns>A list of <see cref="ForgeManifest"/> or null if loading fails.</returns>
    public static async Task<List<ForgeManifest>?> GetForgeManifestAsync(string manifestPath, CancellationToken cancellationToken = default)
    {
        if (_forgeManifest != null)
            return _forgeManifest;

        _forgeManifest = await JsonHelper.ReadJsonFileAsync<List<ForgeManifest>>(manifestPath, cancellationToken);
        return _forgeManifest;
    }

    /// <summary>
    /// Stores the NeoForge mod loader manifests.
    /// </summary>
    private static List<ForgeManifest>? _neoForgeManifest;

    /// <summary>
    /// Retrieves the cached NeoForge mod loader manifests.
    /// </summary>
    /// <returns>A list of <see cref="ForgeManifest"/> or null if not loaded.</returns>
    public static List<ForgeManifest>? GetNeoForgeManifest() => _neoForgeManifest;

    /// <summary>
    /// Asynchronously loads the NeoForge mod loader manifests from the specified path.
    /// </summary>
    /// <param name="manifestPath">The file path to the NeoForge manifest.</param>
    /// <returns>A list of <see cref="ForgeManifest"/> or null if loading fails.</returns>
    public static async Task<List<ForgeManifest>?> GetNeoForgeManifestAsync(string manifestPath, CancellationToken cancellationToken = default)
    {
        if (_neoForgeManifest != null)
            return _neoForgeManifest;

        _neoForgeManifest = await JsonHelper.ReadJsonFileAsync<List<ForgeManifest>>(manifestPath, cancellationToken);
        return _neoForgeManifest;
    }
}