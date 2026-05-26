using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;
using Avalonia.Media.Imaging;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Modrinth.Models;
using Tavstal.KonkordLauncher.Common.Models.MetaCache;
using Tavstal.KonkordLauncher.Common.Services.Abstractions;
using Tavstal.KonkordLauncher.Core.Helpers.IO;
using Tavstal.KonkordLauncher.Core.Helpers.Serialization;
using File = System.IO.File;
using Version = Modrinth.Models.Version;

namespace Tavstal.KonkordLauncher.Common.Services.Implementations;

/// <inheritdoc cref="IMetaCacheService" />
public class MetaCacheService : BackgroundService, IMetaCacheService
{
    private readonly ILogger _logger;
    private readonly ILauncherStore _launcherStore;
    private readonly IModrinthApiClient _modrinthApiClient;
    private static readonly HttpClient _httpClient = new();
    private static readonly ConcurrentDictionary<string, MetaCache> _cache = new();
    private static readonly ConcurrentDictionary<string, Bitmap> _bitmaps = new();
    private static readonly TimeSpan _searchCacheDuration = TimeSpan.FromHours(1);
    private static readonly TimeSpan _projectCacheDuration = TimeSpan.FromHours(24);

    /// <summary>
    /// Initializes a new instance of the <see cref="MetaCacheService"/> class.
    /// </summary>
    /// <param name="logger">Logger used to record initialization failures and other diagnostics.</param>
    /// <param name="launcherStore">Launcher store used to resolve cache directory settings.</param>
    /// <param name="modrinthApiClient">API client used to fetch data from Modrinth when cache misses occur.</param>
    public MetaCacheService(ILogger<MetaCacheService> logger, ILauncherStore launcherStore, IModrinthApiClient modrinthApiClient)
    {
        _logger = logger;
        _launcherStore = launcherStore;
        _modrinthApiClient = modrinthApiClient;
    }
    
    /// <summary>
    /// Performs background initialization of the meta cache by loading persisted cache entries from disk.
    /// </summary>
    /// <param name="stoppingToken"> Cancellation token supplied by the hosting infrastructure for graceful shutdown.</param>
    /// <returns>A task that completes once initialization has finished.</returns>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            if (!File.Exists(PathHelper.MetaCachePath))
                return;

            var result = await JsonHelper.ReadJsonFileAsync<List<MetaCache>>(PathHelper.MetaCachePath);
            if (result != null)
                foreach (var metaCache in result)
                {
                    if (!File.Exists(metaCache.Path) || !metaCache.IsValid())
                        continue;
                    
                    _cache.TryAdd(metaCache.Id, metaCache);
                }
        }
        catch (Exception ex)
        {
            _logger.LogCritical($"Failed to initialize meta cache: {ex}");
        }
    }

    /// <inheritdoc/>
    public async Task<Bitmap?> GetImageAsync(string imageUrl, CancellationToken cancellationToken = default)
    {
        try
        {
            var sha1 = SHA1.HashData(Encoding.UTF8.GetBytes($"image:{imageUrl}"));
            string hash = Convert.ToHexString(sha1);

            if (_cache.TryGetValue(hash, out var cached) && cached.IsValid())
            {
                if (_bitmaps.TryGetValue(hash, out var cachedBitmap))
                    return cachedBitmap;
                await using var stream = File.OpenRead(cached.Path);
                var bitmap = new Bitmap(stream);
                _bitmaps.TryAdd(hash, bitmap);
                return bitmap;
            }
            
            var response = await _httpClient.GetAsync(imageUrl, cancellationToken);
            response.EnsureSuccessStatusCode();
            var data = await response.Content.ReadAsByteArrayAsync(cancellationToken);
            
            var settings = await _launcherStore.GetSettingsAsync(cancellationToken: cancellationToken);
            var cacheDir = settings.Launcher.CacheDirectoryPath;
            Directory.CreateDirectory(cacheDir);
            string imagesDir = Path.Combine(cacheDir, "images");
            Directory.CreateDirectory(imagesDir);
            string cachedFilePath = Path.Combine(imagesDir, $"{hash}.png");
            await File.WriteAllBytesAsync(cachedFilePath, data, cancellationToken);
                
            _cache.TryAdd(hash, new MetaCache
            {
                Id = hash,
                Type = EMetaCacheType.IMAGE,
                Path = cachedFilePath,
                ValidUntil = DateTime.UtcNow.Add(_projectCacheDuration)
            });
            await SaveCacheAsync(cancellationToken);
            
            using var ms = new MemoryStream(data);
            Bitmap image = new Bitmap(ms);
            _bitmaps.TryAdd(hash, image);
            return image;
        }
        catch (Exception ex)
        {
            _logger.LogCritical($"Failed to get image in meta cache: {ex}");
            return null;
        }
    }

    /// <inheritdoc/>
    public async Task<Project?> GetProjectAsync(string id, CancellationToken cancellationToken = default)
    {
        try
        {
            var sha1 = SHA1.HashData(Encoding.UTF8.GetBytes($"project:{id}"));
            string hash = Convert.ToHexString(sha1);

            if (_cache.TryGetValue(hash, out var cached) && cached.IsValid())
                return await JsonHelper.ReadJsonFileAsync<Project>(cached.Path);
            
            var response = await _modrinthApiClient.GetProjectAsync(id, cancellationToken);
            if (response != null)
            {
                var settings = await _launcherStore.GetSettingsAsync(cancellationToken: cancellationToken);
                var cacheDir = settings.Launcher.CacheDirectoryPath;
                Directory.CreateDirectory(cacheDir);
                string projectDir = Path.Combine(cacheDir, "projects");
                Directory.CreateDirectory(projectDir);
                string cachedFilePath = Path.Combine(projectDir, $"{hash}.json");
                await JsonHelper.WriteJsonFileAsync(cachedFilePath, response, cancellationToken);
                
                _cache.TryAdd(hash, new MetaCache
                {
                    Id = hash,
                    Type = EMetaCacheType.PROJECT,
                    Path = cachedFilePath,
                    ValidUntil = DateTime.UtcNow.Add(_projectCacheDuration)
                });
                await SaveCacheAsync(cancellationToken);
            }
            
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogCritical($"Failed to get project in meta cache: {ex}");
            return null;
        }
    }

    /// <inheritdoc/>
    public async Task<Project[]> GetProjectsAsync(List<string> ids, CancellationToken cancellationToken = default)
    {
         try
         {
             List<string> idsToFetch = [];
             List<Project> projects = [];
             var cacheReadTasks = new List<Task<Project>>();

             foreach (var id in ids)
             {
                 var sha1 = SHA1.HashData(Encoding.UTF8.GetBytes($"project:{id}"));
                 string hash = Convert.ToHexString(sha1);

                 if (_cache.TryGetValue(hash, out var cached) && cached.IsValid())
                     cacheReadTasks.Add(JsonHelper.ReadJsonFileAsync<Project>(cached.Path)!);
                 else
                     idsToFetch.Add(id);
             }
            
             var apiTask = idsToFetch.Count > 0 
                 ? _modrinthApiClient.GetProjectsAsync(idsToFetch, cancellationToken) 
                 : Task.FromResult(Array.Empty<Project>());

             await Task.WhenAll(Task.WhenAll(cacheReadTasks), apiTask);

             projects.AddRange(cacheReadTasks.Select(t => t.Result));
             var remaining = await apiTask;
             if (remaining.Any())
                 projects.AddRange(remaining);

             _ = Task.Run(async () =>
             {
                 var settings = await _launcherStore.GetSettingsAsync(cancellationToken: cancellationToken);
                 var cacheDir = settings.Launcher.CacheDirectoryPath;
                 Directory.CreateDirectory(cacheDir);
                 string projectDir = Path.Combine(cacheDir, "projects");
                 Directory.CreateDirectory(projectDir);

                 var cacheTasks = remaining.Select(x =>
                 {
                     var sha1 = SHA1.HashData(Encoding.UTF8.GetBytes($"project:{x.Id}"));
                     string hash = Convert.ToHexString(sha1);
                     string cachedFilePath = Path.Combine(projectDir, $"{hash}.json");

                     _cache.TryAdd(hash, new MetaCache
                     {
                         Id = hash,
                         Type = EMetaCacheType.PROJECT,
                         Path = cachedFilePath,
                         ValidUntil = DateTime.UtcNow.Add(_projectCacheDuration)
                     });

                     return JsonHelper.WriteJsonFileAsync(cachedFilePath, x, cancellationToken);
                 });
                 await Task.WhenAll(cacheTasks);
                 await SaveCacheAsync(
                     cancellationToken);
             }, cancellationToken);

             return projects.ToArray();
         }
         catch (Exception ex)
         {
             _logger.LogCritical($"Failed to get projects in meta cache: {ex}");
             return [];
         }
    }

    /// <inheritdoc/>
    public async Task<Version[]> GetVersionsAsync(List<string> ids, CancellationToken cancellationToken = default)
    {
        try
        {
            List<string> idsToFetch = [];
            List<Version> versions = [];
            var cacheReadTasks = new List<Task<Version>>();

            foreach (var id in ids)
            {
                var sha1 = SHA1.HashData(Encoding.UTF8.GetBytes($"version:{id}"));
                string hash = Convert.ToHexString(sha1);

                if (_cache.TryGetValue(hash, out var cached) && cached.IsValid())
                    cacheReadTasks.Add(JsonHelper.ReadJsonFileAsync<Version>(cached.Path)!);
                else
                    idsToFetch.Add(id);
            }
            
            var apiTask = idsToFetch.Any() 
                ? _modrinthApiClient.GetVersionsAsync(idsToFetch, cancellationToken) 
                : Task.FromResult(Array.Empty<Version>());

            await Task.WhenAll(Task.WhenAll(cacheReadTasks), apiTask);

            versions.AddRange(cacheReadTasks.Select(t => t.Result));
            var remaining = await apiTask;
            if (remaining.Any())
                versions.AddRange(remaining);

            _ = Task.Run(async () =>
            {
                var settings = await _launcherStore.GetSettingsAsync(cancellationToken: cancellationToken);
                var cacheDir = settings.Launcher.CacheDirectoryPath;
                Directory.CreateDirectory(cacheDir);
                string projectDir = Path.Combine(cacheDir, "versions");
                Directory.CreateDirectory(projectDir);

                var cacheTasks = remaining.Select(x =>
                {
                    var sha1 = SHA1.HashData(Encoding.UTF8.GetBytes($"version:{x.Id}"));
                    string hash = Convert.ToHexString(sha1);
                    string cachedFilePath = Path.Combine(projectDir, $"{hash}.json");

                    _cache.TryAdd(hash, new MetaCache
                    {
                        Id = hash,
                        Type = EMetaCacheType.VERSION,
                        Path = cachedFilePath,
                        ValidUntil = DateTime.UtcNow.Add(_projectCacheDuration)
                    });

                    return JsonHelper.WriteJsonFileAsync(cachedFilePath, x, cancellationToken);
                });
                await Task.WhenAll(cacheTasks);
                await SaveCacheAsync(
                    cancellationToken);
            }, cancellationToken);

            return versions.ToArray();
        }
        catch (Exception ex)
        {
            _logger.LogCritical($"Failed to get versions in meta cache: {ex}");
            return [];
        }
    }

    /// <inheritdoc/>
    public async Task<SearchResponse?> SearchModpacksAsync(string? query = null, string? version = null, List<string>? categories = null, int offset = 0,
        CancellationToken cancellationToken = default)
    {
       try
       {
           string hash = BuildQueryHash("modpack", query, version, categories, offset);
           
           if (_cache.TryGetValue(hash, out var cached) && cached.IsValid())
               return await JsonHelper.ReadJsonFileAsync<SearchResponse>(cached.Path);
            
           var response = await _modrinthApiClient.SearchModpacksAsync(query, version, categories, offset, cancellationToken);
           return await HandleSearchResponse(hash, response, cancellationToken);
       }
       catch (Exception ex)
       {
           _logger.LogCritical($"Failed to search modpacks in meta cache: {ex}");
           return null;
       }
    }

    /// <inheritdoc/>
    public async Task<SearchResponse?> SearchModsAsync(string? query = null, string? version = null, List<string>? categories = null, int offset = 0,
        CancellationToken cancellationToken = default)
    {
       try
       {
           string hash = BuildQueryHash("mod", query, version, categories, offset);
            
           if (_cache.TryGetValue(hash, out var cached) && cached.IsValid())
               return await JsonHelper.ReadJsonFileAsync<SearchResponse>(cached.Path);
            
           var response = await _modrinthApiClient.SearchModsAsync(query, version, categories, offset, cancellationToken);
           return await HandleSearchResponse(hash, response, cancellationToken);
       }
       catch (Exception ex)
       {
           _logger.LogCritical($"Failed to search mods in meta cache: {ex}");
           return null;
       }
    }

    /// <inheritdoc/>
    public async Task<SearchResponse?> SearchResourcePacksAsync(string? query = null, string? version = null, List<string>? categories = null, int offset = 0,
        CancellationToken cancellationToken = default)
    {
        try
        {
            string hash = BuildQueryHash("resource_pack", query, version, categories, offset);
            
            if (_cache.TryGetValue(hash, out var cached) && cached.IsValid())
                return await JsonHelper.ReadJsonFileAsync<SearchResponse>(cached.Path);
            
            var response = await _modrinthApiClient.SearchResourcePackAsync(query, version, categories, offset, cancellationToken);
            return await HandleSearchResponse(hash, response, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogCritical($"Failed to search resource packs in meta cache: {ex}");
            return null;
        }
    }

    /// <inheritdoc/>
    public async Task<SearchResponse?> SearchShaderPacksAsync(string? query = null, string? version = null, List<string>? categories = null, int offset = 0,
        CancellationToken cancellationToken = default)
    {
         try
         {
             string hash = BuildQueryHash("shader_pack", query, version, categories, offset);
            
             if (_cache.TryGetValue(hash, out var cached) && cached.IsValid())
                 return await JsonHelper.ReadJsonFileAsync<SearchResponse>(cached.Path);
            
             var response = await _modrinthApiClient.SearchShaderPacksAsync(query, version, categories, offset, cancellationToken);
             return await HandleSearchResponse(hash, response, cancellationToken);
         }
         catch (Exception ex)
         {
             _logger.LogCritical($"Failed to search shaders in meta cache: {ex}");
             return null;
         }
    }

    /// <summary>
    /// Saves a search response to the meta cache and persists the updated cache index.
    /// </summary>
    /// <param name="hash">The cache key generated for the search query.</param>
    /// <param name="response">The Modrinth search response to cache.</param>
    /// <param name="cancellationToken">Cancellation token observed while writing cache files.</param>
    /// <returns>
    /// A task that resolves to the original <paramref name="response"/> if it was cached successfully;
    /// otherwise, <see langword="null"/>.
    /// </returns>
    private async Task<SearchResponse?> HandleSearchResponse(string hash, SearchResponse? response, CancellationToken cancellationToken = default)
    {
        if (response == null)
            return null;
        
        var settings = await _launcherStore.GetSettingsAsync(cancellationToken: cancellationToken);
        var cacheDir = settings.Launcher.CacheDirectoryPath;
        Directory.CreateDirectory(cacheDir);
        string searchDir = Path.Combine(cacheDir, "searchs");
        Directory.CreateDirectory(searchDir);
        string cachedFilePath = Path.Combine(searchDir, $"{hash}.json");
        await JsonHelper.WriteJsonFileAsync(cachedFilePath, response, cancellationToken);
                
        _cache.TryAdd(hash, new MetaCache
        {
            Id = hash,
            Type = EMetaCacheType.SEARCH_RESULT,
            Path = cachedFilePath,
            ValidUntil = DateTime.UtcNow.Add(_searchCacheDuration)
        });
        await SaveCacheAsync(cancellationToken);
        
        return response;
    }

    /// <summary>
    /// Builds a deterministic SHA-1 hash for a Modrinth search query.
    /// </summary>
    /// <param name="name">A prefix identifying the search type (for example, modpack, mod, resource_pack, or shader_pack).</param>
    /// <param name="query">Optional search text.</param>
    /// <param name="version">Optional Minecraft version filter.</param>
    /// <param name="categories">Optional category filters used by the search.</param>
    /// <param name="offset">The result offset used for paging.</param>
    /// <returns>
    /// A hexadecimal SHA-1 hash string representing the full query input.
    /// </returns>
    private string BuildQueryHash(string name, string? query = null, string? version = null, List<string>? categories = null, int offset = 0)
    {
        StringBuilder stringBuilder = new();
        stringBuilder.AppendLine($"{name}:");
        if (!string.IsNullOrEmpty(query))
            stringBuilder.AppendLine("q=" + query);
        if (!string.IsNullOrEmpty(version))
            stringBuilder.AppendLine("v=" + version);
        if (categories is { Count: > 0 })
            stringBuilder.AppendLine("categories=" + string.Join(", ", categories));
        stringBuilder.AppendLine("offset=" + offset);
            
        var sha1 = SHA1.HashData(Encoding.UTF8.GetBytes(stringBuilder.ToString()));
        string hash = Convert.ToHexString(sha1);
        return hash;
    }
    
    /// <summary>
    /// Persists the current meta cache snapshot to disk.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token observed while writing the cache file.</param>
    /// <returns>A task that completes after the cache snapshot has been written.</returns>
    private async Task SaveCacheAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var cacheSnapshot = _cache.Values.ToArray();
            await JsonHelper.WriteJsonFileAsync(PathHelper.MetaCachePath, cacheSnapshot, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogCritical($"Failed to save meta cache: {ex}");
        }
    }
}