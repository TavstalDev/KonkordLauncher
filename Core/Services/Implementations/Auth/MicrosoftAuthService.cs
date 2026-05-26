using System.Net;
using System.Net.Http.Headers;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Tavstal.KonkordLauncher.Core.Enums;
using Tavstal.KonkordLauncher.Core.Helpers.Platform;
using Tavstal.KonkordLauncher.Core.Models;
using Tavstal.KonkordLauncher.Core.Models.Accounts;
using Tavstal.KonkordLauncher.Core.Models.Endpoints;
using Tavstal.KonkordLauncher.Core.Models.Microsoft;
using Tavstal.KonkordLauncher.Core.Models.MojangApi.User;
using Tavstal.KonkordLauncher.Core.Services.Abstractions;
using Tavstal.KonkordLauncher.Core.Services.Abstractions.Auth;

namespace Tavstal.KonkordLauncher.Core.Services.Implementations.Auth;

/// <inheritdoc/>
public class MicrosoftAuthService : IMicrosoftAuthService
{
    private readonly ILogger _logger;
    private readonly IHttpService _httpService;
    private string _microsoftClientId = "496a0c42-aa74-41fe-b7bc-0ad155cdaa26";
    private readonly string _redirectAuthenticateUrl = Path.Combine(MicrosoftHttpAuthService.ListeningUrl, "microsoft/authcallback");
    private IProgressReporter? _progressReporter;
    
    private EAuthStatus _authStatus = EAuthStatus.NONE;
    public EAuthStatus AuthStatus => _authStatus;
    
    private MojangProfile? _mojangProfile;
    public MojangProfile? MojangProfile => _mojangProfile;
    private Account? _account;
    public Account? Account => _account;
    
    public event IMicrosoftAuthService.AuthStatusChangedHandler? OnAuthStatusChanged;
    
    public MicrosoftAuthService(ILogger<MicrosoftAuthService> logger, IHttpService httpService)
    {
        _logger = logger;
        _httpService = httpService;
    }
    
    /// <inheritdoc/>
    public void SetClientId(string clientId)
    {
        if (string.IsNullOrEmpty(clientId))
        {
            _logger.LogError("Microsoft client ID cannot be null or empty.");
            return;
        }
        _microsoftClientId = clientId;
    }

    /// <inheritdoc/>
    public void Reset()
    {
        _progressReporter = null;
        _account = null;
        _mojangProfile = null;
        _authStatus = EAuthStatus.NONE;
        OnAuthStatusChanged?.Invoke(_authStatus);
    }

    /// <inheritdoc/>
    public void OpenAuthenticationUrl()
    {
        if (string.IsNullOrEmpty(_microsoftClientId))
        {
            _logger.LogError("Microsoft client ID is not set.");
            return;
        }
        
        string authUrl = MicrosoftEndpoints.MakeMicrosoftAuthUrl(_microsoftClientId, _redirectAuthenticateUrl);
        OSHelper.OpenUrl(authUrl);
    }

    /// <inheritdoc/>
    public async Task HandleHttpRequestAsync(HttpListenerRequest request, IProgressReporter? progressReporter = null,
        CancellationToken cancellationToken = default)
    {
        _authStatus = EAuthStatus.PENDING;
        OnAuthStatusChanged?.Invoke(_authStatus);
        _progressReporter = progressReporter;
        if (string.IsNullOrEmpty(_microsoftClientId))
        {
            _logger.LogError("Microsoft client ID is not set.");
            _authStatus = EAuthStatus.FAILED;
            OnAuthStatusChanged?.Invoke(_authStatus);
            return;
        }
        
        if (!request.QueryString.AllKeys.Contains("code"))
        {
            _logger.LogError("HTTP request does not contain 'code' query parameter.");
            _authStatus = EAuthStatus.FAILED;
            OnAuthStatusChanged?.Invoke(_authStatus);
            return;
        }

        string? code = request.QueryString["code"];
        if (string.IsNullOrEmpty(code))
        {
            _logger.LogError("Received 'code' query parameter is null or empty.");
            _authStatus = EAuthStatus.FAILED;
            OnAuthStatusChanged?.Invoke(_authStatus);
            return;
        }

        const string requestUrl = MicrosoftEndpoints.MicrosoftTokenUrl;
        _progressReporter?.UpdateStatusTranslated("auth.microsoft.authenticating");
        
        try
        {
            var requestParams = new Dictionary<string, string>
            {
                { "client_id", _microsoftClientId },
                { "grant_type", "authorization_code" },
                { "code", code },
                { "redirect_uri", _redirectAuthenticateUrl }
            };
            var requestContent = new FormUrlEncodedContent(requestParams);

            HttpClient client = _httpService.CreateHttpClient();
            var response = await client.PostAsync(requestUrl, requestContent, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to get access token from Microsoft. Status code: " + response.StatusCode);
                _authStatus = EAuthStatus.FAILED;
                OnAuthStatusChanged?.Invoke(_authStatus);
                return;
            }
            
            var responseString = await response.Content.ReadAsStringAsync(cancellationToken);
            JObject obj = JObject.Parse(responseString);
            if (!obj.TryGetValue("access_token", out var value))
            {
                _logger.LogError("Access token not found in the Microsoft authentication response.\"");
                _authStatus = EAuthStatus.FAILED;
                OnAuthStatusChanged?.Invoke(_authStatus);
                return;
            }
            
            if (!obj.TryGetValue("refresh_token", out var refreshToken))
            {
                _logger.LogError("Refresh token not found in the Microsoft authentication response.");
                _authStatus = EAuthStatus.FAILED;
                OnAuthStatusChanged?.Invoke(_authStatus);
                return;
            }

            _authStatus = EAuthStatus.PROCESSING;
            OnAuthStatusChanged?.Invoke(_authStatus);
            
            // Proceed with the token
            await XboxTokenCallAsync(value.ToString(), refreshToken.ToString(), cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogCritical($"Error while handling HTTP request for Microsoft authentication: {ex}");
            _authStatus = EAuthStatus.FAILED;
            OnAuthStatusChanged?.Invoke(_authStatus);
        }
    }

    /// <inheritdoc/>
    public async Task<DeviceCodeResult?> CreateDeviceCodeAsync(IProgressReporter? progressReporter = null, CancellationToken cancellationToken = default)
    {
        try
        {
            _progressReporter?.UpdateStatusTranslated("auth.code.creating");
            
            var parameters = new Dictionary<string, string>
            {
                { "client_id", _microsoftClientId },
                { "scope", "XboxLive.signin offline_access" }
            };

            var formContent = new FormUrlEncodedContent(parameters);

            HttpClient client = _httpService.CreateHttpClient();
            var result = await client.PostAsync(MicrosoftEndpoints.MicrosoftDeviceUrl, formContent, cancellationToken);
            
            var rawJson = await result.Content.ReadAsStringAsync(cancellationToken);
            return JsonConvert.DeserializeObject<DeviceCodeResult>(rawJson);
        }
        catch (Exception ex)
        {
            _logger.LogCritical($"Error while create device code: {ex}");
            _authStatus = EAuthStatus.FAILED;
            OnAuthStatusChanged?.Invoke(_authStatus);
            return null;
        }
    }

    /// <inheritdoc/>
    public async Task CheckDeviceCodeAsync(string deviceCode, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Device code checked at " + DateTime.Now.ToString("HH:mm:ss"));
            var parameters = new Dictionary<string, string>
            {
                { "client_id", _microsoftClientId },
                { "device_code", deviceCode },
                { "grant_type", "urn:ietf:params:oauth:grant-type:device_code" }
            };

            var formContent = new FormUrlEncodedContent(parameters);

            HttpClient client = _httpService.CreateHttpClient();
            var response = await client.PostAsync(MicrosoftEndpoints.MicrosoftDeviceTokenUrl, formContent, cancellationToken);
            
            var responseString = await response.Content.ReadAsStringAsync(cancellationToken);
            JObject obj = JObject.Parse(responseString);
            if (!obj.TryGetValue("access_token", out var value))
                return;
            
            if (!obj.TryGetValue("refresh_token", out var refreshToken))
                return;
            
            _authStatus = EAuthStatus.PROCESSING;
            OnAuthStatusChanged?.Invoke(_authStatus);
            
            await XboxTokenCallAsync(value.ToString(), refreshToken.ToString(), cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogCritical($"Error while create device code: {ex}");
            _authStatus = EAuthStatus.FAILED;
            OnAuthStatusChanged?.Invoke(_authStatus);
        }
    }

    /// <inheritdoc/>
    public async Task XboxTokenCallAsync(string token, string refreshToken, CancellationToken cancellationToken = default)
    {
        try
        {
            _progressReporter?.UpdateStatusTranslated("auth.xbox.authenticating");
            
            object body = new
            {
                Properties = new
                {
                    AuthMethod = "RPS",
                    SiteName = "user.auth.xboxlive.com",
                    RpsTicket = $"d={token}"
                },
                RelyingParty = "http://auth.xboxlive.com",
                TokenType = "JWT"
            };

            var reqContent = new StringContent(
                JsonConvert.SerializeObject(body), 
                System.Text.Encoding.UTF8, 
                "application/json"
            );

            HttpClient client = _httpService.CreateHttpClient();
            var result = await client.PostAsync(MicrosoftEndpoints.XboxAuthUrl, reqContent, cancellationToken);
            
            JObject resultObj = JObject.Parse(await result.Content.ReadAsStringAsync(cancellationToken));
            if (!resultObj.TryGetValue("Token", out var value))
            {
                _logger.LogError("Token not found in the Xbox authentication response.");
                _authStatus = EAuthStatus.FAILED;
                OnAuthStatusChanged?.Invoke(_authStatus);
                return;
            }
            
            await XboxXstsCallAsync(value.ToString(), refreshToken, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogCritical($"Error while making Xbox token call: {ex}");
            _authStatus = EAuthStatus.FAILED;
            OnAuthStatusChanged?.Invoke(_authStatus);
        }
    }

    /// <inheritdoc/>
    public async Task XboxXstsCallAsync(string token, string refreshToken, CancellationToken cancellationToken = default)
    {
         try
         {
             _progressReporter?.UpdateStatusTranslated("auth.xbox.xsts");
            
             object body = new
             {
                 Properties = new
                 {
                     SandboxId = "RETAIL",
                     UserTokens = new[] { token }
                 },
                 RelyingParty = "rp://api.minecraftservices.com/",
                 TokenType = "JWT"
             };

             var reqContent = new StringContent(
                 JsonConvert.SerializeObject(body), 
                 System.Text.Encoding.UTF8, 
                 "application/json"
             );

             HttpClient client = _httpService.CreateHttpClient();
             var result = await client.PostAsync(MicrosoftEndpoints.XboxXstsUrl, reqContent, cancellationToken);

             var rawJson = await result.Content.ReadAsStringAsync(cancellationToken);
             JObject resultObj = JObject.Parse(rawJson);
             if (!resultObj.TryGetValue("Token", out var value))
             {
                 _logger.LogError("Token not found in the Xbox XSTS response.");
                 _authStatus = EAuthStatus.FAILED;
                 OnAuthStatusChanged?.Invoke(_authStatus);
                 return;
             }

             if (!resultObj.TryGetValue("DisplayClaims", out var displayClaims))
             {
                 _logger.LogError("DisplayClaims not found in the Xbox XSTS response.");
                 _authStatus = EAuthStatus.FAILED;
                 OnAuthStatusChanged?.Invoke(_authStatus);
                 return;
             }

             var xui = displayClaims["xui"];
             if (xui == null)
             {
                 _logger.LogError("xui not found or empty in the Xbox XSTS response.");
                 _authStatus = EAuthStatus.FAILED;
                 OnAuthStatusChanged?.Invoke(_authStatus);
                 return;
             }

             var firstXui = xui.First;
             if (firstXui == null)
             {
                 _logger.LogError("User hash (uhs) not found in the Xbox XSTS response.");
                 _authStatus = EAuthStatus.FAILED;
                 OnAuthStatusChanged?.Invoke(_authStatus);
                 return;
             }
            
             var userHash = firstXui["uhs"];
             if (userHash == null)
             {
                 _logger.LogError("User hash (uhs) is null in the Xbox XSTS response.");
                 _authStatus = EAuthStatus.FAILED;
                 OnAuthStatusChanged?.Invoke(_authStatus);
                 return;
             }
            
             await MinecraftAccessCallAsync(value.ToString(), refreshToken, userHash.ToString(), cancellationToken);
         }
         catch (Exception ex)
         {
             _logger.LogCritical($"Error while making Xbox XSTS call: {ex}");
             _authStatus = EAuthStatus.FAILED;
             OnAuthStatusChanged?.Invoke(_authStatus);
         }
    }

    /// <inheritdoc/>
    public async Task MinecraftAccessCallAsync(string token, string refreshToken, string userHash,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _progressReporter?.UpdateStatusTranslated("auth.minecraft.authenticating");
            
            object body = new
            {
                identityToken = $"XBL3.0 x={userHash};{token}",
                ensureLegacyEnabled = true
            };

            var reqContent = new StringContent(
                JsonConvert.SerializeObject(body), 
                System.Text.Encoding.UTF8, 
                "application/json"
            );

            HttpClient client = _httpService.CreateHttpClient();
            var result = await client.PostAsync(MicrosoftEndpoints.MinecraftAuthUrl, reqContent, cancellationToken);
            
            JObject resultObj = JObject.Parse(await result.Content.ReadAsStringAsync(cancellationToken));
            if (!resultObj.TryGetValue("access_token", out var minecraftToken))
            {
                _logger.LogError("Access token not found in the Minecraft authentication response.");
                _authStatus = EAuthStatus.FAILED;
                OnAuthStatusChanged?.Invoke(_authStatus);
                return;
            }
            
            if (!resultObj.TryGetValue("expires_in", out var expiresIn))
            {
                _logger.LogError("Expiration time not found in the Minecraft authentication response.");
                _authStatus = EAuthStatus.FAILED;
                OnAuthStatusChanged?.Invoke(_authStatus);
                return;
            }
            
            await CheckMinecraftOwnershipAsync(minecraftToken.ToString(), refreshToken, int.Parse(expiresIn.ToString()), cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogCritical($"Error while making Minecraft access call: {ex}");
            _authStatus = EAuthStatus.FAILED;
            OnAuthStatusChanged?.Invoke(_authStatus);
        }
    }

    /// <inheritdoc/>
    public async Task CheckMinecraftOwnershipAsync(string mcToken, string refreshToken, int expireSeconds,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _progressReporter?.UpdateStatusTranslated("auth.minecraft.ownership");
            
            HttpClient client = _httpService.CreateHttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", mcToken);
            var result = await client.GetAsync(MicrosoftEndpoints.MinecraftOwnershipUrl, cancellationToken);

            OwnershipData? ownershipData =
                JsonConvert.DeserializeObject<OwnershipData>(await result.Content.ReadAsStringAsync(cancellationToken));
            
            OwnershipItem? gameOwnership = ownershipData?.Items.Find(x => 
                x.Name is "game_minecraft" or "game_minecraft_bedrock" ||
                x.Name == "product_minecraft" ||
                x.Name == "product_minecraft_bedrock"
            );
            if (gameOwnership == null)
            {
                _logger.LogError("User does not own Minecraft.");
                _authStatus = EAuthStatus.FAILED;
                OnAuthStatusChanged?.Invoke(_authStatus);
                return;
            }

            await GetMinecraftProfileAsync(mcToken, refreshToken, expireSeconds, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogCritical($"Error while checking Minecraft ownership: {ex}");
            _authStatus = EAuthStatus.FAILED;
            OnAuthStatusChanged?.Invoke(_authStatus);
        }
    }

    /// <inheritdoc/>
    public async Task GetMinecraftProfileAsync(string mcToken, string refreshToken, int expireSecs,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _progressReporter?.UpdateStatusTranslated("auth.minecraft.profile");
            
            HttpClient client = _httpService.CreateHttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", mcToken);
            var result = await client.GetAsync(MicrosoftEndpoints.MinecraftProfileUrl, cancellationToken);

            _mojangProfile =
                JsonConvert.DeserializeObject<MojangProfile>(await result.Content.ReadAsStringAsync(cancellationToken));

            if (_mojangProfile == null)
            {
                _logger.LogError("Failed to retrieve Minecraft profile.");
                _authStatus = EAuthStatus.FAILED;
                OnAuthStatusChanged?.Invoke(_authStatus);
                return;
            }

            List<AccountSkin> skins = [];
            if (_mojangProfile.Skins.Count > 0)
            {
                Cape? cape = _mojangProfile.Capes.Find(x => x.State.Equals("active", StringComparison.OrdinalIgnoreCase));
                foreach (var skin in _mojangProfile.Skins)
                    skins.Add(new AccountSkin(Guid.NewGuid().ToString(), skin.Variant, cape?.Id, skin.Id));
            }
            
            _account = new Account
            {
                Id = Guid.NewGuid().ToString(),
                Uuid = _mojangProfile.Id,
                DisplayName = _mojangProfile.Name,
                Type = EAccountType.MICROSOFT,
                AccessTokenExpireDate = DateTime.Now.AddSeconds(expireSecs),
                Skins = skins,
                MojangProfile = _mojangProfile
            };
            _account.SetAccessToken(mcToken);
            _account.SetRefreshToken(refreshToken);
            
            _authStatus = EAuthStatus.SUCCESS;
            OnAuthStatusChanged?.Invoke(_authStatus);
        }
        catch (Exception ex)
        {
            _logger.LogCritical($"Error while getting Minecraft profile: {ex}");
            _authStatus = EAuthStatus.FAILED;
            OnAuthStatusChanged?.Invoke(_authStatus);
        }
    }

    /// <inheritdoc/>
    public async Task<bool> RefreshLoginAsync(string token, CancellationToken cancellationToken = default)
    {
       if (string.IsNullOrEmpty(_microsoftClientId))
       {
           _logger.LogError("Microsoft client ID is not set.");
           _authStatus = EAuthStatus.FAILED;
           OnAuthStatusChanged?.Invoke(_authStatus);
           return false;
       }

       const string requestUrl = MicrosoftEndpoints.MicrosoftTokenUrl;
        
       try
       {
           var requestParams = new Dictionary<string, string>
           {
               { "client_id", _microsoftClientId },
               { "grant_type", "refresh_token" },
               { "refresh_token", token }
           };
           var requestContent = new FormUrlEncodedContent(requestParams);

           HttpClient client = _httpService.CreateHttpClient();
           var response = await client.PostAsync(requestUrl, requestContent, cancellationToken);

           if (!response.IsSuccessStatusCode)
           {
               _logger.LogError("Failed to get access token from Microsoft. Status code: " + response.StatusCode);
               _authStatus = EAuthStatus.FAILED;
               OnAuthStatusChanged?.Invoke(_authStatus);
               return false;
           }
            
           var responseString = await response.Content.ReadAsStringAsync(cancellationToken);

           JObject obj = JObject.Parse(responseString);
           if (!obj.TryGetValue("access_token", out var value))
           {
               _logger.LogError("Access token not found in the Microsoft authentication response.\"");
               _authStatus = EAuthStatus.FAILED;
               OnAuthStatusChanged?.Invoke(_authStatus);
               return false;
           }
            
           if (!obj.TryGetValue("refresh_token", out var refreshToken))
           {
               _logger.LogError("Refresh token not found in the Microsoft authentication response.");
               _authStatus = EAuthStatus.FAILED;
               OnAuthStatusChanged?.Invoke(_authStatus);
               return false;
           }
           
           // Proceed with the token
           await XboxTokenCallAsync(value.ToString(), refreshToken.ToString(), cancellationToken);
           // Wait for the authentication process to complete
           _logger.LogDebug("Refresh auth status: " + AuthStatus);
           return true;
       }
       catch (Exception ex)
       {
           _logger.LogCritical($"Error while handling HTTP request for Microsoft authentication: {ex}");
           _authStatus = EAuthStatus.FAILED;
           OnAuthStatusChanged?.Invoke(_authStatus);
           return false;
       }
    }
}