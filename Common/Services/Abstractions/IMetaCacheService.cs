using Modrinth.Models;
using Tavstal.KonkordLauncher.Common.Models;
using Version = Modrinth.Models.Version;

namespace Tavstal.KonkordLauncher.Common.Services.Abstractions;

/// <summary>
/// Provides metadata caching for Modrinth API data including projects, versions, images, and search results.
/// </summary>
public interface IMetaCacheService
{
    /// <summary>
    /// Gets a cached image from the specified URL, downloading and caching it if not already available.
    /// </summary>
    /// <param name="imageUrl">The URL of the image to fetch.</param>
    /// <param name="cancellationToken">Cancellation token observed during the download.</param>
    /// <returns>
    /// A task that resolves to the image as a <see cref="BitmapEntry"/> if successful; otherwise, <see langword="null"/>.
    /// </returns>
    Task<BitmapEntry?> GetImageAsync(string imageUrl, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Retrieves the image path from a given URL.
    /// </summary>
    /// <param name="imageUrl">The URL of the image.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation. This parameter is optional and has a default value of CancellationToken.None.</param>
    /// <returns>The path of the image if found; otherwise, null.</returns>
    string? GetImagePath(string imageUrl, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a cached Modrinth project by ID, fetching from the API if not cached or expired.
    /// </summary>
    /// <param name="id">The Modrinth project ID.</param>
    /// <param name="cancellationToken">Cancellation token observed during the request.</param>
    /// <returns>
    /// A task that resolves to the <see cref="Project"/> if found; otherwise, <see langword="null"/>.
    /// </returns>
    Task<Project?> GetProjectAsync(string id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets multiple cached Modrinth projects by ID, fetching from the API if not cached or expired.
    /// </summary>
    /// <param name="ids">A list of Modrinth project IDs to fetch.</param>
    /// <param name="cancellationToken">Cancellation token observed during the request.</param>
    /// <returns>
    /// A task that resolves to an array of <see cref="Project"/> objects. Projects not found are omitted from the result.
    /// </returns>
    Task<Project[]> GetProjectsAsync(List<string> ids, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets multiple cached Modrinth versions by ID, fetching from the API if not cached or expired.
    /// </summary>
    /// <param name="ids">A list of Modrinth version IDs to fetch.</param>
    /// <param name="cancellationToken">Cancellation token observed during the request.</param>
    /// <returns>
    /// A task that resolves to an array of <see cref="Version"/> objects. Versions not found are omitted from the result.
    /// </returns>
    Task<Version[]> GetVersionsAsync(List<string> ids, CancellationToken cancellationToken = default);

    /// <summary>
    /// Searches Modrinth modpacks with optional filtering and caches the result.
    /// </summary>
    /// <param name="query">Optional search query string.</param>
    /// <param name="version">Optional Minecraft version filter.</param>
    /// <param name="categories">Optional list of category filters.</param>
    /// <param name="offset">Pagination offset. Defaults to <c>0</c>.</param>
    /// <param name="cancellationToken">Cancellation token observed during the search.</param>
    /// <returns>
    /// A task that resolves to the <see cref="SearchResponse"/> if successful; otherwise, <see langword="null"/>.
    /// </returns>
    Task<SearchResponse?> SearchModpacksAsync(string? query = null, string? version = null,
        List<string>? categories = null,
        int offset = 0, CancellationToken cancellationToken = default);

    /// <summary>
    /// Searches Modrinth mods with optional filtering and caches the result.
    /// </summary>
    /// <param name="query">Optional search query string.</param>
    /// <param name="version">Optional Minecraft version filter.</param>
    /// <param name="categories">Optional list of category filters.</param>
    /// <param name="offset">Pagination offset. Defaults to <c>0</c>.</param>
    /// <param name="cancellationToken">Cancellation token observed during the search.</param>
    /// <returns>
    /// A task that resolves to the <see cref="SearchResponse"/> if successful; otherwise, <see langword="null"/>.
    /// </returns>
    Task<SearchResponse?> SearchModsAsync(string? query = null, string? version = null, List<string>? categories = null,
        int offset = 0, CancellationToken cancellationToken = default);

    /// <summary>
    /// Searches Modrinth resource packs with optional filtering and caches the result.
    /// </summary>
    /// <param name="query">Optional search query string.</param>
    /// <param name="version">Optional Minecraft version filter.</param>
    /// <param name="categories">Optional list of category filters.</param>
    /// <param name="offset">Pagination offset. Defaults to <c>0</c>.</param>
    /// <param name="cancellationToken">Cancellation token observed during the search.</param>
    /// <returns>
    /// A task that resolves to the <see cref="SearchResponse"/> if successful; otherwise, <see langword="null"/>.
    /// </returns>
    Task<SearchResponse?> SearchResourcePacksAsync(string? query = null, string? version = null,
        List<string>? categories = null,
        int offset = 0, CancellationToken cancellationToken = default);

    /// <summary>
    /// Searches Modrinth shader packs with optional filtering and caches the result.
    /// </summary>
    /// <param name="query">Optional search query string.</param>
    /// <param name="version">Optional Minecraft version filter.</param>
    /// <param name="categories">Optional list of category filters.</param>
    /// <param name="offset">Pagination offset. Defaults to <c>0</c>.</param>
    /// <param name="cancellationToken">Cancellation token observed during the search.</param>
    /// <returns>
    /// A task that resolves to the <see cref="SearchResponse"/> if successful; otherwise, <see langword="null"/>.
    /// </returns>
    Task<SearchResponse?> SearchShaderPacksAsync(string? query = null, string? version = null,
        List<string>? categories = null,
        int offset = 0, CancellationToken cancellationToken = default);
}