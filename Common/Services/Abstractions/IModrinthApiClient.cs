using Modrinth.Models;
using Version = Modrinth.Models.Version;

namespace Tavstal.KonkordLauncher.Common.Services.Abstractions;

/// <summary>
/// Provides access to the Modrinth API for searches, project metadata, and version metadata.
/// </summary>
public interface IModrinthApiClient
{
    /// <summary>
    /// Searches Modrinth mods using the supplied filters.
    /// </summary>
    /// <param name="query">Optional search text.</param>
    /// <param name="version">Optional Minecraft version filter.</param>
    /// <param name="categories">Optional list of category filters.</param>
    /// <param name="offset">Pagination offset, in results.</param>
    /// <param name="cancellationToken">Cancellation token observed during the request.</param>
    /// <returns>
    /// A task that resolves to the search response, or <see langword="null"/> if the request fails or no response is available.
    /// </returns>
    Task<SearchResponse?> SearchModsAsync(string? query = null, string? version = null, List<string>? categories = null,
        int offset = 0, CancellationToken cancellationToken = default);

    /// <summary>
    /// Searches Modrinth modpacks using the supplied filters.
    /// </summary>
    /// <param name="query">Optional search text.</param>
    /// <param name="version">Optional Minecraft version filter.</param>
    /// <param name="categories">Optional list of category filters.</param>
    /// <param name="offset">Pagination offset, in results.</param>
    /// <param name="cancellationToken">Cancellation token observed during the request.</param>
    /// <returns>
    /// A task that resolves to the search response, or <see langword="null"/> if the request fails or no response is available.
    /// </returns>
    Task<SearchResponse?> SearchModpacksAsync(string? query = null, string? version = null,
        List<string>? categories = null, int offset = 0, CancellationToken cancellationToken = default);

    /// <summary>
    /// Searches Modrinth resource packs using the supplied filters.
    /// </summary>
    /// <param name="query">Optional search text.</param>
    /// <param name="version">Optional Minecraft version filter.</param>
    /// <param name="categories">Optional list of category filters.</param>
    /// <param name="offset">Pagination offset, in results.</param>
    /// <param name="cancellationToken">Cancellation token observed during the request.</param>
    /// <returns>
    /// A task that resolves to the search response, or <see langword="null"/> if the request fails or no response is available.
    /// </returns>
    Task<SearchResponse?> SearchResourcePackAsync(string? query = null, string? version = null,
        List<string>? categories = null, int offset = 0, CancellationToken cancellationToken = default);

    /// <summary>
    /// Searches Modrinth shader packs using the supplied filters.
    /// </summary>
    /// <param name="query">Optional search text.</param>
    /// <param name="version">Optional Minecraft version filter.</param>
    /// <param name="categories">Optional list of category filters.</param>
    /// <param name="offset">Pagination offset, in results.</param>
    /// <param name="cancellationToken">Cancellation token observed during the request.</param>
    /// <returns>
    /// A task that resolves to the search response, or <see langword="null"/> if the request fails or no response is available.
    /// </returns>
    Task<SearchResponse?> SearchShaderPacksAsync(string? query = null, string? version = null,
        List<string>? categories = null, int offset = 0, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a Modrinth project by its identifier.
    /// </summary>
    /// <param name="id">The project identifier.</param>
    /// <param name="cancellationToken">Cancellation token observed during the request.</param>
    /// <returns>
    /// A task that resolves to the project if found; otherwise, <see langword="null"/>.
    /// </returns>
    Task<Project?> GetProjectAsync(string id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets multiple Modrinth projects by their identifiers.
    /// </summary>
    /// <param name="ids">The project identifiers to fetch.</param>
    /// <param name="cancellationToken">Cancellation token observed during the request.</param>
    /// <returns>
    /// A task that resolves to an array of projects. Missing projects may be omitted from the result.
    /// </returns>
    Task<Project[]> GetProjectsAsync(List<string> ids, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets multiple Modrinth versions by their identifiers.
    /// </summary>
    /// <param name="ids">The version identifiers to fetch.</param>
    /// <param name="cancellationToken">Cancellation token observed during the request.</param>
    /// <returns>
    /// A task that resolves to an array of versions. Missing versions may be omitted from the result.
    /// </returns>
    Task<Version[]> GetVersionsAsync(List<string> ids, CancellationToken cancellationToken = default);
}