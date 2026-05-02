using Modrinth;
using Modrinth.Models;
using Modrinth.Models.Enums.Project;
using Tavstal.KonkordLauncher.Core.Models;
using Version = Modrinth.Models.Version;

namespace Tavstal.KonkordLauncher.Common.Helpers;

public static class ModrinthHelper
{
    private static readonly CoreLogger _logger = CoreLogger.WithModuleType(typeof(ModrinthHelper));
    
    private static readonly ModrinthClientConfig _config = new()
    {
        UserAgent = "KonkordLauncher/2.0.0 (+https://github.com/TavstalDev/KonkordLauncher)",
        ModrinthToken = null
    };
    
    private static readonly ModrinthClient _client = new (_config);

    public static async Task<SearchResponse?> SearchModsAsync(string? query = null, string? version = null, List<string>? categories = null, int offset = 0, CancellationToken cancellationToken = default)
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
            _logger.Error($"Failed to search mods on modrinth: {ex}");
            return null;
        }
    }
    
    public static async Task<SearchResponse?> SearchModpacksAsync(string? query = null, string? version = null, List<string>? categories = null, int offset = 0, CancellationToken cancellationToken = default)
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
            _logger.Error($"Failed to search modpacks on modrinth: {ex}");
            return null;
        }
    }
    
    public static async Task<SearchResponse?> SearchResourcePackAsync(string? query = null, string? version = null, List<string>? categories = null, int offset = 0, CancellationToken cancellationToken = default)
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
            _logger.Error($"Failed to search resource packs on modrinth: {ex}");
            return null;
        }
    }
    
    public static async Task<SearchResponse?> SearchShaderPacksAsync(string? query = null, string? version = null, List<string>? categories = null, int offset = 0, CancellationToken cancellationToken = default)
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
            _logger.Error($"Failed to search shader packs on modrinth: {ex}");
            return null;
        }
    }

    public static async Task<Project?> GetProjectAsync(string id, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _client.Project.GetAsync(id, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.Error($"Failed to get project from modrinth: {ex}");
            return null;
        }
    }
    
    public static async Task<Project[]> GetProjectsAsync(List<string> ids, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _client.Project.GetMultipleAsync(ids, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.Error($"Failed to get projects from modrinth: {ex}");
            return [];
        }
    }

    public static async Task<Version[]> GetVersionsAsync(List<string> ids, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _client.Version.GetMultipleAsync(ids, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.Error($"Failed to get versions from modrinth: {ex}");
            return [];
        }
    }
}