using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Tavstal.KonkordLauncher.Core.Helpers.Serialization;
using Tavstal.KonkordLauncher.Core.Models.ModLoaders;
using Tavstal.KonkordLauncher.Core.Models.ModLoaders.Fabric;
using Tavstal.KonkordLauncher.Core.Models.ModLoaders.Forge;
using Tavstal.KonkordLauncher.Core.Models.ModLoaders.NeoForge;
using Tavstal.KonkordLauncher.Core.Models.ModLoaders.Quilt;
using Tavstal.KonkordLauncher.Core.Models.MojangApi;
using Tavstal.KonkordLauncher.Core.Services.Abstractions;

namespace Tavstal.KonkordLauncher.Core.Services.Implementations;

/// <inheritdoc/>
public class ManifestService : IManifestService
{
    private readonly ILogger _logger;
    private VersionManifest? _versionManifest;
    private List<IModManifest>? _fabricManifest;
    private List<IModManifest>? _quiltManifest;
    private List<IModManifest>? _forgeManifest;
    private List<IModManifest>? _neoForgeManifest;

    /// <summary>
    /// Initializes a new instance of the <see cref="ManifestService"/> class.
    /// </summary>
    /// <param name="logger">Logger instance used to record diagnostic and error messages during manifest operations.</param>
    public ManifestService(ILogger<ManifestService> logger)
    {
        _logger = logger;
    }
    
    /// <inheritdoc/>
    public async Task<VersionManifest?> GetMinecraftManifestAsync(string manifestPath, CancellationToken cancellationToken = default)
    {
        if (_versionManifest != null)
            return _versionManifest;

        _versionManifest = await JsonHelper.ReadJsonFileAsync<VersionManifest>(manifestPath);
        return _versionManifest;
    }

    /// <inheritdoc/>
    public async Task<List<IModManifest>?> GetFabricManifestAsync(string manifestPath, CancellationToken cancellationToken = default)
    {
        if (_fabricManifest != null)
            return _fabricManifest;

        var rawManifest = await File.ReadAllTextAsync(manifestPath, cancellationToken);
        JObject jObject = JObject.Parse(rawManifest);
        if (jObject["loader"] is not JArray mappings)
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
    
    /// <inheritdoc/>
    public async Task<List<IModManifest>?> GetQuiltManifestAsync(string manifestPath, CancellationToken cancellationToken = default)
    {
        if (_quiltManifest != null)
            return _quiltManifest;

        var rawManifest = await File.ReadAllTextAsync(manifestPath, cancellationToken);
        JObject jObject = JObject.Parse(rawManifest);
        if (jObject["loader"] is not JArray mappings)
        {
            throw new InvalidOperationException("Quilt manifest loader not found in the JSON.");
        }
        _quiltManifest = [];
        foreach (var mapping in mappings)
        {
            _quiltManifest.Add(new QuiltManifest(mapping.Value<string>("version")!));
        }

        return _quiltManifest;
    }

    /// <inheritdoc/>
    public async Task<List<IModManifest>?> GetForgeManifestAsync(string manifestPath, CancellationToken cancellationToken = default)
    {
        if (_forgeManifest != null)
            return _forgeManifest;

        _forgeManifest = [];
        var localManifests = await JsonHelper.ReadJsonFileAsync<List<ForgeManifest>>(manifestPath);
        if (localManifests == null)
            throw new  InvalidOperationException("Forge manifest loader not found in the JSON.");
        
        foreach (var manifest in localManifests)
            _forgeManifest.Add(manifest);
        
        return _forgeManifest;
    }

    /// <inheritdoc/>
    public async Task<List<IModManifest>?> GetNeoForgeManifestAsync(string manifestPath)
    {
        if (_neoForgeManifest != null)
            return _neoForgeManifest;

        _neoForgeManifest = [];
        var localManifests = await JsonHelper.ReadJsonFileAsync<List<NeoForgeManifest>>(manifestPath);
        if (localManifests == null)
            throw new  InvalidOperationException("Neo forge manifest loader not found in the JSON.");
        
        foreach (var manifest in localManifests)
            _neoForgeManifest.Add(manifest);
        
        return _neoForgeManifest;
    }
}