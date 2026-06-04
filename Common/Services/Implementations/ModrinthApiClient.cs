using Modrinth;
using Modrinth.Models;
using Modrinth.Models.Enums.Project;
using Tavstal.KonkordLauncher.Common.Models;
using Tavstal.KonkordLauncher.Common.Services.Abstractions;
using Tavstal.KonkordLauncher.Core.Models.Logging;
using Version = Modrinth.Models.Version;

namespace Tavstal.KonkordLauncher.Common.Services.Implementations;

/// <inheritdoc/>
public class ModrinthApiClient : IModrinthApiClient
{
    private readonly ICustomLogger _logger;
    private readonly ModrinthClient _client = new (new ModrinthClientConfig
    {
        UserAgent = "KonkordLauncher/2.0.0 (+https://github.com/TavstalDev/KonkordLauncher)",
        ModrinthToken = null,
        JsonSerializerContext = ModrinthJsonContext.Default
    });
    
    public ModrinthApiClient(ICustomLogger<ModrinthApiClient> logger)
    {
        _logger = logger;
    }
    
    /// <inheritdoc/>
    public async Task<SearchResponse?> SearchModsAsync(string? query = null, string? version = null, List<string>? categories = null, int offset = 0,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var facets = new FacetCollection();
            if (!string.IsNullOrEmpty(version))
            {
                facets.Add(Facet.Version(version));
            }
            if (categories is { Count: > 0 })
            {
                foreach (var category in categories)
                    facets.Add(Facet.Category(category));
            }
            facets.Add(Facet.ProjectType(ProjectType.Mod));
            
            return await _client.Project.SearchAsync(query ?? string.Empty, facets: facets, offset: offset, limit: 25, cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, $"Failed to search mods on modrinth:");
            return null;
        }
    }

    /// <inheritdoc/>
    public async Task<SearchResponse?> SearchModpacksAsync(string? query = null, string? version = null, List<string>? categories = null, int offset = 0,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var facets = new FacetCollection();
            if (!string.IsNullOrEmpty(version))
            {
                facets.Add(Facet.Version(version));
            }
            if (categories is { Count: > 0 })
            {
                foreach (var category in categories)
                    facets.Add(Facet.Category(category));
            }
            facets.Add(Facet.ProjectType(ProjectType.Modpack));
            
            return await _client.Project.SearchAsync(query ?? string.Empty, facets: facets, offset: offset, limit: 25, cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, $"Failed to search modpacks on modrinth:");
            return null;
        }
    }

    /// <inheritdoc/>
    public async Task<SearchResponse?> SearchResourcePackAsync(string? query = null, string? version = null, List<string>? categories = null, int offset = 0,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var facets = new FacetCollection();
            if (!string.IsNullOrEmpty(version))
            {
                facets.Add(Facet.Version(version));
            }
            if (categories is { Count: > 0 })
            {
                foreach (var category in categories)
                    facets.Add(Facet.Category(category));
            }
            facets.Add(Facet.ProjectType(ProjectType.Resourcepack));
            
            return await _client.Project.SearchAsync(query ?? string.Empty, facets: facets, offset: offset, limit: 25, cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, $"Failed to search resource packs on modrinth:");
            return null;
        }
    }

    /// <inheritdoc/>
    public async Task<SearchResponse?> SearchShaderPacksAsync(string? query = null, string? version = null, List<string>? categories = null, int offset = 0,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var facets = new FacetCollection();
            if (!string.IsNullOrEmpty(version))
            {
                facets.Add(Facet.Version(version));
            }
            if (categories is { Count: > 0 })
            {
                foreach (var category in categories)
                    facets.Add(Facet.Category(category));
            }
            facets.Add(Facet.ProjectType(ProjectType.Shader));
            
            return await _client.Project.SearchAsync(query ?? string.Empty, facets: facets, offset: offset, limit: 25, cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, $"Failed to search shader packs on modrinth:");
            return null;
        }
    }

    /// <inheritdoc/>
    public async Task<Project?> GetProjectAsync(string id, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _client.Project.GetAsync(id, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, $"Failed to get project from modrinth:");
            return null;
        }
    }

    /// <inheritdoc/>
    public async Task<Project[]> GetProjectsAsync(List<string> ids, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _client.Project.GetMultipleAsync(ids, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, $"Failed to get projects from modrinth:");
            return [];
        }
    }

    /// <inheritdoc/>
    public async Task<Version[]> GetVersionsAsync(List<string> ids, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _client.Version.GetMultipleAsync(ids, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, $"Failed to get versions from modrinth:");
            return [];
        }
    }
}