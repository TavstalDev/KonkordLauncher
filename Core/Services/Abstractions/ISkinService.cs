using Tavstal.KonkordLauncher.Core.Models.Accounts;
using Tavstal.KonkordLauncher.Core.Models.MojangApi.User;

namespace Tavstal.KonkordLauncher.Core.Services.Abstractions;

/// <summary>
/// Defines a service contract for managing player skins and capes in the Minecraft launcher.
/// Provides methods to fetch, retrieve, and cache skin and cape data for both online and offline accounts.
/// </summary>
public interface ISkinService
{
    /// <summary>
    /// Retrieves the complete skin image for a player, optionally including their cape overlay.
    /// </summary>
    /// <param name="username">The username of the player whose skin to retrieve.</param>
    /// <param name="skinUrl">Optional custom skin URL. If provided, this URL will be used instead of fetching from Mojang.</param>
    /// <param name="enableCape">Determines whether to include the player's cape in the returned image. Default is true.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>A byte array containing the skin image data, or null if the skin could not be retrieved.</returns>
    Task<byte[]?> GetFullSkinAsync(string username, string? skinUrl = null, bool enableCape = true,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Fetches and caches skins for offline/demo accounts that are not connected to a Mojang account.
    /// </summary>
    /// <param name="cacheDir">The directory path where skin data should be cached for offline access.</param>
    /// <param name="accountId">The unique identifier of the offline account.</param>
    /// <param name="name">The display name of the offline account player.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    Task FetchOfflineSkinsAsync(string cacheDir, string accountId, string name,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Fetches and caches skins for online accounts from the Mojang or Microsoft authentication service.
    /// </summary>
    /// <param name="cacheDir">The directory path where skin data should be cached for offline access.</param>
    /// <param name="accountId">The unique identifier of the online account.</param>
    /// <param name="uuid">The player's UUID (Universally Unique Identifier) from the Mojang profile.</param>
    /// <param name="skin">The account's skin metadata containing URL and variant information.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    Task FetchSkinsAsync(string cacheDir, string accountId, string uuid, AccountSkin skin,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Fetches and caches cape images for the specified list of capes.
    /// </summary>
    /// <param name="cacheDir">The directory path where cape data should be cached for offline access.</param>
    /// <param name="capes">A list of cape identifiers to fetch. Can include Minecon, Mojang staff, or promotional capes.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    Task FetchCapesAsync(string cacheDir, List<Cape> capes, CancellationToken cancellationToken = default);

    /// <summary>
    /// Fetches a preview image of a player's skin for display in the launcher UI.
    /// The preview is typically a small, optimized version of the full skin suitable for profile views.
    /// </summary>
    /// <param name="cacheDir">The directory path where preview skin data should be cached.</param>
    /// <param name="uuid">The player's UUID (Universally Unique Identifier).</param>
    /// <param name="username">The player's username for identification and fallback purposes.</param>
    /// <param name="skinId">The identifier of the specific skin variant, or null for the default skin.</param>
    /// <param name="isWide">Determines the aspect ratio of the preview. True for widescreen (9:16), false for standard (8:8).</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    Task FetchPreviewSkinAsync(string cacheDir, string uuid, string username, string? skinId, bool isWide,
        CancellationToken cancellationToken = default);
}