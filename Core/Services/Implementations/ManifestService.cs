using System.Text.Json;
using Tavstal.KonkordLauncher.Core.Helpers.Serialization;
using Tavstal.KonkordLauncher.Core.Models.Json;
using Tavstal.KonkordLauncher.Core.Models.Logging;
using Tavstal.KonkordLauncher.Core.Models.ModLoaders;
using Tavstal.KonkordLauncher.Core.Models.ModLoaders.Fabric;
using Tavstal.KonkordLauncher.Core.Models.ModLoaders.Forge;
using Tavstal.KonkordLauncher.Core.Models.ModLoaders.NeoForge;
using Tavstal.KonkordLauncher.Core.Models.MojangApi;
using Tavstal.KonkordLauncher.Core.Services.Abstractions;

namespace Tavstal.KonkordLauncher.Core.Services.Implementations;

/// <inheritdoc cref="IManifestService" />
public class ManifestService : IManifestService
{
    private readonly ICustomLogger _logger;
    private VersionManifest? _versionManifest;
    private List<IModManifest>? _fabricManifest;
    private List<IModManifest>? _quiltManifest;
    private List<IModManifest>? _forgeManifest;
    private List<IModManifest>? _neoForgeManifest;

    /// <summary>
    /// Initializes a new instance of the <see cref="ManifestService"/> class.
    /// </summary>
    /// <param name="logger">Logger instance used to record diagnostic and error messages during manifest operations.</param>
    public ManifestService(ICustomLogger<ManifestService> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc/>
    public VersionManifest? GetMinecraftManifest() => _versionManifest;

    /// <inheritdoc/>
    public async Task<VersionManifest?> GetMinecraftManifestAsync(string manifestPath, CancellationToken cancellationToken = default)
    {
        if (_versionManifest != null)
            return _versionManifest;

        _versionManifest = await JsonHelper.ReadJsonFileAsync<VersionManifest>(manifestPath, CoreJsonContext.Default.VersionManifest, cancellationToken);
        return _versionManifest;
    }

    /// <inheritdoc/>
    public List<IModManifest>? GetFabricManifest() => _fabricManifest;

    /// <inheritdoc/>
    public async Task<List<IModManifest>?> GetFabricManifestAsync(string manifestPath, CancellationToken cancellationToken = default)
    {
        if (_fabricManifest != null)
            return _fabricManifest;

        var rawManifest = await File.ReadAllTextAsync(manifestPath, cancellationToken);
        JsonElement jObject = JsonElement.Parse(rawManifest);
        if (!jObject.TryGetProperty("loader", out var mappings) || mappings.GetArrayLength() == 0)
            throw new InvalidOperationException("Fabric manifest loader not found in the JSON.");
        _fabricManifest = [];
        foreach (var mapping in mappings.EnumerateArray())
        {
            if (!mapping.TryGetProperty("version", out var version))
                continue;
            
            _fabricManifest.Add(new FabricManifest(version.ToString()));
        }

        return _fabricManifest;
    }

    /// <inheritdoc/>
    public List<IModManifest>? GetQuiltManifest() => _quiltManifest;

    /// <inheritdoc/>
    public async Task<List<IModManifest>?> GetQuiltManifestAsync(string manifestPath, CancellationToken cancellationToken = default)
    {
        if (_quiltManifest != null)
            return _quiltManifest;

        var rawManifest = await File.ReadAllTextAsync(manifestPath, cancellationToken);
        JsonElement jObject = JsonElement.Parse(rawManifest);
        if (!jObject.TryGetProperty("loader", out var mappings) || mappings.GetArrayLength() == 0)
            throw new InvalidOperationException("Quilt manifest loader not found in the JSON.");
        _quiltManifest = [];
        foreach (var mapping in  mappings.EnumerateArray())
        {
            if (!mapping.TryGetProperty("version", out var version))
                continue;
            _quiltManifest.Add(new FabricManifest(version.ToString()));
        }

        return _quiltManifest;
    }

    /// <inheritdoc/>
    public List<IModManifest>? GetForgeManifest() => _forgeManifest;

    /// <inheritdoc/>
    public async Task<List<IModManifest>?> GetForgeManifestAsync(string manifestPath, CancellationToken cancellationToken = default)
    {
        if (_forgeManifest != null)
            return _forgeManifest;

        _forgeManifest = [];
        var localManifests = await JsonHelper.ReadJsonFileAsync<List<ForgeManifest>>(manifestPath, CoreJsonContext.Default.ListForgeManifest);
        if (localManifests == null)
            throw new  InvalidOperationException("Forge manifest loader not found in the JSON.");
        
        foreach (var manifest in localManifests)
            _forgeManifest.Add(manifest);
        
        return _forgeManifest;
    }

    /// <inheritdoc/>
    public List<IModManifest>? GetNeoForgeManifest() => _neoForgeManifest;

    /// <inheritdoc/>
    public async Task<List<IModManifest>?> GetNeoForgeManifestAsync(string manifestPath)
    {
        if (_neoForgeManifest != null)
            return _neoForgeManifest;

        _neoForgeManifest = [];
        var localManifests = await JsonHelper.ReadJsonFileAsync<List<NeoForgeManifest>>(manifestPath, CoreJsonContext.Default.ListNeoForgeManifest);
        if (localManifests == null)
            throw new  InvalidOperationException("Neo forge manifest loader not found in the JSON.");
        
        foreach (var manifest in localManifests)
            _neoForgeManifest.Add(manifest);
        
        return _neoForgeManifest;
    }
}