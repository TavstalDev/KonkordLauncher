using Tavstal.KonkordLauncher.Core.Models.MojangApi.User;

namespace Tavstal.KonkordLauncher.Core.Services.Abstractions;

/// <summary>
/// Defines a service contract for managing Minecraft player skins and capes through the Mojang/Microsoft API.
/// Provides methods to change, upload, reset skins and manage cape visibility for authenticated players.
/// </summary>
public interface IMojangSkinService
{
    /// <summary>
    /// Changes the player's skin to the one specified by the provided URL.
    /// The skin must be a valid PNG image file accessible at the given URL.
    /// </summary>
    /// <param name="mcToken">The Minecraft authentication token obtained from Microsoft/Mojang login.</param>
    /// <param name="variant">The skin variant type, typically "classic" for standard 8x8 textures or "slim" for 3x4 arm textures.</param>
    /// <param name="url">The URL pointing to the skin image file. Must be a publicly accessible PNG image.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>The updated <see cref="MojangProfile"/> containing the player's profile data with the new skin applied, or null if the operation failed.</returns>
    Task<MojangProfile?> ChangeSkin(string mcToken, string variant, string url,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Uploads a skin file from the local file system to the player's Mojang account.
    /// The file must be a valid PNG image with dimensions of 64x64 or 64x32 pixels.
    /// </summary>
    /// <param name="mcToken">The Minecraft authentication token obtained from Microsoft/Mojang login.</param>
    /// <param name="variant">The skin variant type, typically "classic" for standard 8x8 textures or "slim" for 3x4 arm textures.</param>
    /// <param name="skinPath">The local file system path to the skin PNG file to be uploaded.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>The updated <see cref="MojangProfile"/> containing the player's profile data with the uploaded skin applied, or null if the operation failed.</returns>
    Task<MojangProfile?> UploadSkin(string mcToken, string variant, string skinPath,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Resets the player's skin to the default Minecraft skin based on their UUID.
    /// </summary>
    /// <param name="mcToken">The Minecraft authentication token obtained from Microsoft/Mojang login.</param>
    /// <param name="playerId">The unique player ID (typically the numeric representation of the player's UUID).</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>The updated <see cref="MojangProfile"/> with the default skin restored, or null if the operation failed.</returns>
    Task<MojangProfile?> ResetSkin(string mcToken, long playerId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Displays a specific cape on the player's character if they own it.
    /// Only capes owned by the player (promotional, Minecon, etc.) can be shown.
    /// </summary>
    /// <param name="mcToken">The Minecraft authentication token obtained from Microsoft/Mojang login.</param>
    /// <param name="capeId">The identifier of the cape to display. This must be a cape the player owns.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>The updated <see cref="MojangProfile"/> with the cape now visible, or null if the operation failed.</returns>
    Task<MojangProfile?> ShowCape(string mcToken, string capeId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Hides the currently displayed cape from the player's character.
    /// The cape is not removed from the player's account, only hidden from display.
    /// </summary>
    /// <param name="mcToken">The Minecraft authentication token obtained from Microsoft/Mojang login.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>The updated <see cref="MojangProfile"/> with the cape now hidden, or null if the operation failed.</returns>
    Task<MojangProfile?> HideCape(string mcToken, CancellationToken cancellationToken = default);
}