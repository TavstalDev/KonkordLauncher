using System.Net.Http.Headers;
using Newtonsoft.Json;
using Tavstal.KonkordLauncher.Core.Helpers;
using Tavstal.KonkordLauncher.Core.Models;
using Tavstal.KonkordLauncher.Core.Models.Endpoints;
using Tavstal.KonkordLauncher.Core.Models.MojangApi.User;

namespace Tavstal.KonkordLauncher.Core.Services;

/// <summary>
/// Provides methods to interact with Mojang's skin and cape services.
/// </summary>
public static class MojangSkinService
{
    private static readonly CoreLogger _logger = new(nameof(MojangSkinService));
    

    public static async Task<MojangProfile?> ChangeSkin(string mcToken, string variant, string url, CancellationToken cancellationToken = default)
    {
        try
        {
            const string endpoint = $"{MicrosoftEndpoints.PlayerConfigUrl}/skins";
            object body = new
            {
                variant,
                url
            };

            var reqContent = new StringContent(
                JsonConvert.SerializeObject(body), 
                System.Text.Encoding.UTF8, 
                "application/json"
            );

            HttpClient client = HttpHelper.GetHttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", mcToken);
            var result = await client.PostAsync(endpoint, reqContent, cancellationToken).ConfigureAwait(false);
            if (!result.IsSuccessStatusCode)
            {
                _logger.Error($"Failed to change skin (HTTP {result.StatusCode}): " + await result.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false));
                return null;
            }
            return JsonConvert.DeserializeObject<MojangProfile>(await result.Content.ReadAsStringAsync(cancellationToken));
        }
        catch (Exception ex)
        {
            _logger.Error("Failed to change skin: " + ex.Message);
            return null;
        }
    }

    
    public static async Task<MojangProfile?> UploadSkin(string mcToken, string variant, string skinPath, CancellationToken cancellationToken = default)
    {
        try
        {
            const string endpoint = $"{MicrosoftEndpoints.PlayerConfigUrl}/skins";

            using var form = new MultipartFormDataContent();
            form.Add(new StringContent(variant), "variant");
            await using var fs = File.OpenRead(skinPath);
            var fileContent = new StreamContent(fs);
            fileContent.Headers.ContentType = new MediaTypeHeaderValue("image/png");
            form.Add(fileContent, "file", Path.GetFileName(skinPath));

            HttpClient client = HttpHelper.GetHttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", mcToken);
            var result = await client.PostAsync(endpoint, form, cancellationToken).ConfigureAwait(false);
            if (!result.IsSuccessStatusCode)
            {
                _logger.Error($"Failed to upload skin (HTTP {result.StatusCode}): " + await result.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false));
                return null;
            }
            return JsonConvert.DeserializeObject<MojangProfile>(await result.Content.ReadAsStringAsync(cancellationToken));
        }
        catch (Exception ex)
        {
            _logger.Error("Failed to upload skin: " + ex.Message);
            return null;
        }
    }

    
    public static async Task<MojangProfile?> ResetSkin(string mcToken, long playerId, CancellationToken cancellationToken = default)
    {
        try
        {
            string endpoint = $"{MicrosoftEndpoints.PlayerConfigUrl}/skins/active?uuid={playerId}";
            HttpClient client = HttpHelper.GetHttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", mcToken);
            var result = await client.DeleteAsync(endpoint, cancellationToken).ConfigureAwait(false);
            if (!result.IsSuccessStatusCode)
            {
                _logger.Error($"Failed to reset skin (HTTP {result.StatusCode}): " + await result.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false));
                return null;
            }
            return JsonConvert.DeserializeObject<MojangProfile>(await result.Content.ReadAsStringAsync(cancellationToken));
        }
        catch (Exception ex)
        {
            _logger.Error("Failed to reset skin: " + ex.Message);
            return null;
        }
    }

    
    public static async Task<MojangProfile?> ShowCape(string mcToken, string capeId, CancellationToken cancellationToken = default)
    {
        try
        {
            const string endpoint = $"{MicrosoftEndpoints.PlayerConfigUrl}/capes/active";
            object body = new
            {
                capeId
            };

            var reqContent = new StringContent(
                JsonConvert.SerializeObject(body), 
                System.Text.Encoding.UTF8, 
                "application/json"
            );
            HttpClient client = HttpHelper.GetHttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", mcToken);
            var result = await client.PutAsync(endpoint, reqContent).ConfigureAwait(false);
            if (!result.IsSuccessStatusCode)
            {
                _logger.Error($"Failed to show cape (HTTP {result.StatusCode}): " + await result.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false));
                return null;
            }
            return JsonConvert.DeserializeObject<MojangProfile>(await result.Content.ReadAsStringAsync(cancellationToken));
        }
        catch (Exception ex)
        {
            _logger.Error("Failed to show cape: " + ex.Message);
            return null;
        }
    }
    
    
    public static async Task<MojangProfile?> HideCape(string mcToken, CancellationToken cancellationToken = default)
    {
        try
        {
            const string endpoint = $"{MicrosoftEndpoints.PlayerConfigUrl}/capes/active";
            HttpClient client = HttpHelper.GetHttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", mcToken);
            var result = await client.DeleteAsync(endpoint, cancellationToken).ConfigureAwait(false);
            if (!result.IsSuccessStatusCode)
            {
                _logger.Error($"Failed to hide cape (HTTP {result.StatusCode}): " + await result.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false));
                return null;
            }
            return JsonConvert.DeserializeObject<MojangProfile>(await result.Content.ReadAsStringAsync(cancellationToken));
        }
        catch (Exception ex)
        {
            _logger.Error("Failed to hide cape: " + ex.Message);
            return null;
        }
    }
}