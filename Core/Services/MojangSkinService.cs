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
    private static readonly int MaxParallelDownloads = 16;
    

    public static async Task<MojangProfile?> ChangeSkin(string mcToken, string variant, string url)
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
            var result = await client.PostAsync(endpoint, reqContent).ConfigureAwait(false);
            if (!result.IsSuccessStatusCode)
            {
                _logger.Error($"Failed to change skin (HTTP {result.StatusCode}): " + await result.Content.ReadAsStringAsync().ConfigureAwait(false));
                return null;
            }
            return JsonConvert.DeserializeObject<MojangProfile>(await result.Content.ReadAsStringAsync());
        }
        catch (Exception ex)
        {
            _logger.Error("Failed to change skin: " + ex.Message);
            return null;
        }
    }

    
    public static async Task<MojangProfile?> UploadSkin(string mcToken, string variant, string skinPath)
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
            var result = await client.PostAsync(endpoint, form).ConfigureAwait(false);
            if (!result.IsSuccessStatusCode)
            {
                _logger.Error($"Failed to upload skin (HTTP {result.StatusCode}): " + await result.Content.ReadAsStringAsync().ConfigureAwait(false));
                return null;
            }
            return JsonConvert.DeserializeObject<MojangProfile>(await result.Content.ReadAsStringAsync());
        }
        catch (Exception ex)
        {
            _logger.Error("Failed to upload skin: " + ex.Message);
            return null;
        }
    }

    
    public static async Task<MojangProfile?> ResetSkin(string mcToken, long playerId)
    {
        try
        {
            string endpoint = $"{MicrosoftEndpoints.PlayerConfigUrl}/skins/active?uuid={playerId}";
            HttpClient client = HttpHelper.GetHttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", mcToken);
            var result = await client.DeleteAsync(endpoint).ConfigureAwait(false);
            if (!result.IsSuccessStatusCode)
            {
                _logger.Error($"Failed to reset skin (HTTP {result.StatusCode}): " + await result.Content.ReadAsStringAsync().ConfigureAwait(false));
                return null;
            }
            return JsonConvert.DeserializeObject<MojangProfile>(await result.Content.ReadAsStringAsync());
        }
        catch (Exception ex)
        {
            _logger.Error("Failed to reset skin: " + ex.Message);
            return null;
        }
    }

    
    public static async Task<MojangProfile?> ShowCape(string mcToken, string capeId)
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
                _logger.Error($"Failed to show cape (HTTP {result.StatusCode}): " + await result.Content.ReadAsStringAsync().ConfigureAwait(false));
                return null;
            }
            return JsonConvert.DeserializeObject<MojangProfile>(await result.Content.ReadAsStringAsync());
        }
        catch (Exception ex)
        {
            _logger.Error("Failed to show cape: " + ex.Message);
            return null;
        }
    }
    
    
    public static async Task<MojangProfile?> HideCape(string mcToken)
    {
        try
        {
            const string endpoint = $"{MicrosoftEndpoints.PlayerConfigUrl}/capes/active";
            HttpClient client = HttpHelper.GetHttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", mcToken);
            var result = await client.DeleteAsync(endpoint).ConfigureAwait(false);
            if (!result.IsSuccessStatusCode)
            {
                _logger.Error($"Failed to hide cape (HTTP {result.StatusCode}): " + await result.Content.ReadAsStringAsync().ConfigureAwait(false));
                return null;
            }
            return JsonConvert.DeserializeObject<MojangProfile>(await result.Content.ReadAsStringAsync());
        }
        catch (Exception ex)
        {
            _logger.Error("Failed to hide cape: " + ex.Message);
            return null;
        }
    }

    
    public static async Task FetchSkins(string cacheDir, string uuid, string username, List<Cape> capes)
    {
        string skinsDir = Path.Combine(cacheDir, "skins", uuid);
        string capesDir = Path.Combine(cacheDir, "capes");
        if (!Directory.Exists(skinsDir))
            Directory.CreateDirectory(skinsDir);
        if (!Directory.Exists(capesDir))
            Directory.CreateDirectory(capesDir);
        
        var semaphore = new SemaphoreSlim(MaxParallelDownloads);
        var tasks = new List<Task>();

        // Fetch headshot
        Task t = Task.Run(async () =>
        {
            byte[]? skinResult = await StartlightSkinService.GetHeadshotAsync(username);
            if (skinResult != null)
            {
                string skinPath = Path.Combine(skinsDir, "head.png");
                await File.WriteAllBytesAsync(skinPath, skinResult);
            }
        });
        tasks.Add(t);
            
        // Fetch full skin
        t = Task.Run(async () =>
        {
            byte[]? skinResult = await StartlightSkinService.GetFullSkinAsync(username, enableCape: false);
            if (skinResult != null)
            {
                string skinPath = Path.Combine(skinsDir, "preview.png");
                await File.WriteAllBytesAsync(skinPath, skinResult);
            }
        });
        tasks.Add(t);
                
        // Fetch capes if not already cached
        foreach (Cape cape in capes)
        {
            await semaphore.WaitAsync();
            t = Task.Run(async () =>
            {
                try
                {
                    string capePath = Path.Combine(capesDir, $"{cape.Id}.png");
                    if (File.Exists(capePath))
                        return;
                    byte[]? capeResult =
                        await StartlightSkinService.GetCapeViewAsync(username, cape.Url,
                            "https://textures.minecraft.net/texture/9d4f187f41cae641558f8787bf1e7be72a6d72911b21c97d916f0a7faaf28f7");
                    if (capeResult == null)
                    {
                        _logger.Warn("Failed to download cape view for cape ID: " + cape.Id);
                        return;
                    }

                    await File.WriteAllBytesAsync(capePath, capeResult);
                }
                finally
                {
                    semaphore.Release();
                }
            });
            tasks.Add(t);
        }
        
        await Task.WhenAll(tasks);
    } 
}