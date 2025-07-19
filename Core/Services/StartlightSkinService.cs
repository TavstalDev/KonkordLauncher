using Tavstal.KonkordLauncher.Core.Helpers;
using Tavstal.KonkordLauncher.Core.Models;

namespace Tavstal.KonkordLauncher.Core.Services;

/// <summary>
/// Provides services for interacting with the Starlight Skin API to retrieve skin and cape data.
/// </summary>
public static class StartlightSkinService
{
    /// <summary>
    /// Logger instance for logging errors and information related to the StartlightSkinService.
    /// </summary>
    private static readonly CoreLogger _logger = CoreLogger.WithModuleType(typeof(StartlightSkinService));

    /// <summary>
    /// HTTP client used for making requests to the Starlight Skin API.
    /// </summary>
    private static HttpClient? _httpClient = new()
    {
        DefaultRequestHeaders =
        {
            { "User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:125.0) Gecko/20100101 Firefox/125.0" },
            { "Accept", "*/*" },
            { "Accept-Encoding", "gzip, deflate, br" },
            { "Connection", "keep-alive" }
        }
    };

    /// <summary>
    /// Retrieves the full skin model for a given username.
    /// </summary>
    /// <param name="username">The username of the player whose skin is to be retrieved.</param>
    /// <param name="skinUrl">Optional custom skin URL to override the default skin.</param>
    /// <param name="enableCape">Indicates whether the cape should be included in the skin model.</param>
    /// <returns>A byte array containing the skin data, or null if the request fails.</returns>
    public static async Task<byte[]?> GetFullSkinAsync(string username, string? skinUrl = null, bool enableCape = true)
    {
        try
        {
            string requestUrl =
                $"https://starlightskins.lunareclipse.studio/render/default/{username}/full?capeEnabled={enableCape}";
            if (skinUrl != null)
                requestUrl += $"&skinUrl={skinUrl}";

            return await HttpHelper.GetByteArrayAsync(requestUrl);
        }
        catch (Exception ex)
        {
            _logger.Error("Failed to download skin model");
            _logger.Error(ex.Message);
            return null;
        }
    }

    /// <summary>
    /// Retrieves a cape view for a given username and cape URL.
    /// </summary>
    /// <param name="username">The username of the player whose cape view is to be retrieved.</param>
    /// <param name="capeUrl">The URL of the cape texture to be used.</param>
    /// <param name="skinUrl">Optional custom skin URL to override the default skin.</param>
    /// <returns>A byte array containing the cape view data, or null if the request fails.</returns>
    public static async Task<byte[]?> GetCapeViewAsync(string username, string capeUrl, string? skinUrl = null)
    {
        try
        {
            string requestUrl =
                $"https://starlightskins.lunareclipse.studio/render/default/${username}/full?cameraPosition=%22x%22:%220%22,%22y%22:%2216%22,%22z%22:%2232%22&cameraFocalPoint=%22x%22:%223.67%22,%22y%22:%2216.31%22,%22z%22:%223.35%22&capeEnabled=false&capeTexture={capeUrl}";

            if (skinUrl != null)
                requestUrl += $"&skinUrl={skinUrl}";

            return await HttpHelper.GetByteArrayAsync(requestUrl);
        }
        catch (Exception ex)
        {
            _logger.Error("Failed to download cape view");
            _logger.Error(ex.Message);
            return null;
        }
    }
}