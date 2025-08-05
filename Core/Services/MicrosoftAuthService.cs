using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Tavstal.KonkordLauncher.Core.Enums;
using Tavstal.KonkordLauncher.Core.Helpers;
using Tavstal.KonkordLauncher.Core.Models;
using Tavstal.KonkordLauncher.Core.Models.Endpoints;
using Tavstal.KonkordLauncher.Core.Models.MojangApi.User;

namespace Tavstal.KonkordLauncher.Core.Services;

/// <summary>
/// Provides services for handling Microsoft authentication and related operations.
/// </summary>
public static class MicrosoftAuthService
{
    private static readonly CoreLogger _logger = new(typeof(MicrosoftAuthService));
    private static string _microsoftClientId = "496a0c42-aa74-41fe-b7bc-0ad155cdaa26"; // TODO: Remove hardcoded client ID and set it via SetClientId method.
    private static readonly string _redirectAuthenticateUrl = Path.Combine(AuthService.ListeningUrl, "microsoft/authcallback");
    private static IProgressReporter? _progressReporter;
    //private static readonly string _redirectTokenUrl = Path.Combine(AuthService.ListeningUrl, "microsoft/tokencallback");
    
    private static EAuthStatus _authStatus = EAuthStatus.NONE;
    public static EAuthStatus AuthStatus => _authStatus;
    
    private static MojangProfile? _mojangProfile;
    public static MojangProfile? MojangProfile => _mojangProfile;
    private static Account? _account;
    public static Account? Account => _account;
    
    /// <summary>
    /// Sets the Microsoft client ID for authentication.
    /// </summary>
    /// <param name="clientId">The client ID to set.</param>
    public static void SetClientId(string clientId)
    {
        if (string.IsNullOrEmpty(clientId))
        {
            _logger.Error("Microsoft client ID cannot be null or empty.");
            return;
        }
        _microsoftClientId = clientId;
    }

    /// <summary>
    /// Resets the authentication state, clearing account and profile information.
    /// </summary>
    public static void Reset()
    {
        _progressReporter = null;
        _account = null;
        _mojangProfile = null;
        _authStatus = EAuthStatus.NONE;
    }
    
    /// <summary>
    /// Opens the Microsoft authentication URL in the default web browser.
    /// </summary>
    public static void OpenAuthenticationUrl()
    {
        if (string.IsNullOrEmpty(_microsoftClientId))
        {
            _logger.Error("Microsoft client ID is not set.");
            return;
        }
        
        string authUrl = MicrosoftEndpoints.MakeMicrosoftAuthUrl(_microsoftClientId, _redirectAuthenticateUrl);
        Process process = new();
        process.StartInfo = new ProcessStartInfo
        {
            FileName = authUrl,
            UseShellExecute = true
        };
        process.Start();
    }
    
    /// <summary>
    /// Generates the Microsoft authentication URL using the client ID and redirect URL.
    /// </summary>
    /// <returns>
    /// A string containing the authentication URL if the client ID is set; 
    /// otherwise, an empty string if the client ID is not set.
    /// </returns>
    public static string GetAuthenticationUrl()
    {
        if (string.IsNullOrEmpty(_microsoftClientId))
        {
            _logger.Error("Microsoft client ID is not set.");
            return string.Empty;
        }
        
        return MicrosoftEndpoints.MakeMicrosoftAuthUrl(_microsoftClientId, _redirectAuthenticateUrl);
    }
    
    /// <summary>
    /// Handles an HTTP request for Microsoft authentication.
    /// Validates the request, extracts the authorization code, and initiates the token exchange process.
    /// </summary>
    /// <param name="request">The HTTP request containing the authentication data.</param>
    /// <param name="progressReporter">An optional progress reporter for tracking the authentication process.</param>
    public static async Task HandleHttpRequestAsync(HttpListenerRequest request, IProgressReporter? progressReporter = null)
    {
        _authStatus = EAuthStatus.PENDING;
        _progressReporter = progressReporter;
        if (string.IsNullOrEmpty(_microsoftClientId))
        {
            _logger.Error("Microsoft client ID is not set.");
            _authStatus = EAuthStatus.FAILED;
            return;
        }
        
        if (!request.QueryString.AllKeys.Contains("code"))
        {
            _logger.Error("HTTP request does not contain 'code' query parameter.");
            _authStatus = EAuthStatus.FAILED;
            return;
        }

        string? code = request.QueryString["code"];
        if (string.IsNullOrEmpty(code))
        {
            _logger.Error("Received 'code' query parameter is null or empty.");
            _authStatus = EAuthStatus.FAILED;
            return;
        }

        string requestUrl = MicrosoftEndpoints.MicrosoftTokenUrl;
        _progressReporter?.SetStatusTranslated("auth.microsoft.authenticating");
        
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

            using HttpClient client = HttpHelper.GetHttpClient();
            var response = await client.PostAsync(requestUrl, requestContent).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                _logger.Error("Failed to get access token from Microsoft. Status code: " + response.StatusCode);
                _authStatus = EAuthStatus.FAILED;
                return;
            }
            
            var responseString = await response.Content.ReadAsStringAsync();

            JObject obj = JObject.Parse(responseString);
            if (!obj.TryGetValue("access_token", out var value))
            {
                _logger.Error("Access token not found in the Microsoft authentication response.\"");
                _authStatus = EAuthStatus.FAILED;
                return;
            }
            
            if (!obj.TryGetValue("refresh_token", out var refreshToken))
            {
                _logger.Error("Refresh token not found in the Microsoft authentication response.");
                _authStatus = EAuthStatus.FAILED;
                return;
            }
           
            // Proceed with the token
            await XboxTokenCallAsync(value.ToString(), refreshToken.ToString());
        }
        catch (Exception ex)
        {
            _logger.Exc("Error while handling HTTP request for Microsoft authentication:");
            _logger.Error(ex.ToString());
            _authStatus = EAuthStatus.FAILED;
        }
    }
    
    /// <summary>
    /// Makes an asynchronous call to the Xbox authentication endpoint to retrieve a token.
    /// </summary>
    /// <param name="token">The Microsoft access token used for authentication.</param>
    /// <param name="refreshToken">The refresh token to be used for subsequent authentication steps.</param>
    private static async Task XboxTokenCallAsync(string token, string refreshToken)
    {
        try
        {
            _progressReporter?.SetStatusTranslated("auth.xbox.authenticating");
            
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

            using HttpClient client = HttpHelper.GetHttpClient();
            var result = await client.PostAsync(MicrosoftEndpoints.XboxAuthUrl, reqContent).ConfigureAwait(false);
            
            JObject resultObj = JObject.Parse(await result.Content.ReadAsStringAsync());
            if (!resultObj.TryGetValue("Token", out var value))
            {
                _logger.Error("Token not found in the Xbox authentication response.");
                _authStatus = EAuthStatus.FAILED;
                return;
            }
            
            await XboxXstsCallAsync(value.ToString(), refreshToken);
        }
        catch (Exception ex)
        {
            _logger.Exc("Error while making Xbox token call:");
            _logger.Error(ex.ToString());
            _authStatus = EAuthStatus.FAILED;
        }
    }
    
    /// <summary>
    /// Makes an asynchronous call to the Xbox XSTS endpoint to retrieve a user hash and token.
    /// </summary>
    /// <param name="token">The Xbox authentication token.</param>
    /// <param name="refreshToken">The refresh token to be used for subsequent authentication steps.</param>
    private static async Task XboxXstsCallAsync(string token, string refreshToken)
    {
        try
        {
            _progressReporter?.SetStatusTranslated("auth.xbox.xsts");
            
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

            using HttpClient client = HttpHelper.GetHttpClient();
            var result = await client.PostAsync(MicrosoftEndpoints.XboxXstsUrl, reqContent).ConfigureAwait(false);

            var rawJson = await result.Content.ReadAsStringAsync();
            JObject resultObj = JObject.Parse(rawJson);
            if (!resultObj.TryGetValue("Token", out var value))
            {
                _logger.Error("Token not found in the Xbox XSTS response.");
                _authStatus = EAuthStatus.FAILED;
                return;
            }

            if (!resultObj.TryGetValue("DisplayClaims", out var displayClaims))
            {
                _logger.Error("DisplayClaims not found in the Xbox XSTS response.");
                _authStatus = EAuthStatus.FAILED;
                return;
            }

            var xui = displayClaims["xui"];
            if (xui == null)
            {
                _logger.Error("xui not found or empty in the Xbox XSTS response.");
                _authStatus = EAuthStatus.FAILED;
                return;
            }

            var firstXui = xui.First;
            if (firstXui == null)
            {
                _logger.Error("User hash (uhs) not found in the Xbox XSTS response.");
                _authStatus = EAuthStatus.FAILED;
                return;
            }
            
            var userHash = firstXui["uhs"];
            if (userHash == null)
            {
                _logger.Error("User hash (uhs) is null in the Xbox XSTS response.");
                _authStatus = EAuthStatus.FAILED;
                return;
            }
            
            await MinecraftAccessCallAsync(value.ToString(), refreshToken, userHash.ToString());
        }
        catch (Exception ex)
        {
            _logger.Exc("Error while making Xbox XSTS call:");
            _logger.Error(ex.ToString());
            _authStatus = EAuthStatus.FAILED;
        }
    }
    
    /// <summary>
    /// Makes an asynchronous call to the Minecraft authentication endpoint to retrieve an access token.
    /// </summary>
    /// <param name="token">The Xbox XSTS token.</param>
    /// <param name="refreshToken">The refresh token to be used for subsequent authentication steps.</param>
    /// <param name="userHash">The user hash retrieved from the Xbox XSTS response.</param>
    private static async Task MinecraftAccessCallAsync(string token, string refreshToken, string userHash)
    {
        try
        {
            _progressReporter?.SetStatusTranslated("auth.minecraft.authenticating");
            
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

            using HttpClient client = HttpHelper.GetHttpClient();
            var result = await client.PostAsync(MicrosoftEndpoints.MinecraftAuthUrl, reqContent).ConfigureAwait(false);
            
            JObject resultObj = JObject.Parse(await result.Content.ReadAsStringAsync());
            if (!resultObj.TryGetValue("access_token", out var minecraftToken))
            {
                _logger.Error("Access token not found in the Minecraft authentication response.");
                _authStatus = EAuthStatus.FAILED;
                return;
            }
            
            if (!resultObj.TryGetValue("expires_in", out var expiresIn))
            {
                _logger.Error("Expiration time not found in the Minecraft authentication response.");
                _authStatus = EAuthStatus.FAILED;
                return;
            }
            
            await CheckMinecraftOwnershipAsync(minecraftToken.ToString(), refreshToken, int.Parse(expiresIn.ToString()));
        }
        catch (Exception ex)
        {
            _logger.Exc("Error while making Minecraft access call:");
            _logger.Error(ex.ToString());
            _authStatus = EAuthStatus.FAILED;
        }
    }
    
    /// <summary>
    /// Checks if the user owns Minecraft by querying the ownership endpoint.
    /// </summary>
    /// <param name="mcToken">The Minecraft access token.</param>
    /// <param name="refreshToken">The refresh token to be used for subsequent authentication steps.</param>
    /// <param name="expireSeconds">The expiration time of the access token in seconds.</param>
    private static async Task CheckMinecraftOwnershipAsync(string mcToken, string refreshToken, int expireSeconds)
    {
        try
        {
            _progressReporter?.SetStatusTranslated("auth.minecraft.ownership");
            
            HttpClient client = HttpHelper.GetHttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", mcToken);
            var result = await client.GetAsync(MicrosoftEndpoints.MinecraftOwnershipUrl);

            OwnershipData? ownershipData =
                JsonConvert.DeserializeObject<OwnershipData>(await result.Content.ReadAsStringAsync());
            
            OwnershipItem? gameOwnership = ownershipData?.Items.Find(x => 
                x.Name == "game_minecraft" ||
                x.Name == "game_minecraft_bedrock" ||
                x.Name == "product_minecraft" ||
                x.Name == "product_minecraft_bedrock"
            );
            if (gameOwnership == null)
            {
                _logger.Error("User does not own Minecraft.");
                _authStatus = EAuthStatus.FAILED;
                return;
            }

            await GetMinecraftProfileAsync(mcToken, refreshToken, expireSeconds);
        }
        catch (Exception ex)
        {
            _logger.Exc("Error while checking Minecraft ownership:");
            _logger.Error(ex.ToString());
            _authStatus = EAuthStatus.FAILED;
        }
    }
    
    /// <summary>
    /// Retrieves the user's Minecraft profile and updates the account information.
    /// </summary>
    /// <param name="mcToken">The Minecraft access token.</param>
    /// <param name="refreshToken">The refresh token to be used for subsequent authentication steps.</param>
    /// <param name="expireSecs">The expiration time of the access token in seconds.</param>
    private static async Task GetMinecraftProfileAsync(string mcToken, string refreshToken, int expireSecs)
    {
        try
        {
            _progressReporter?.SetStatusTranslated("auth.minecraft.profile");
            
            HttpClient client = HttpHelper.GetHttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", mcToken);
            var result = await client.GetAsync(MicrosoftEndpoints.MinecraftProfileUrl);

            _mojangProfile =
                JsonConvert.DeserializeObject<MojangProfile>(await result.Content.ReadAsStringAsync());

            if (_mojangProfile == null)
            {
                _logger.Error("Failed to retrieve Minecraft profile.");
                _authStatus = EAuthStatus.FAILED;
                return;
            }

            _account = new Account(Guid.NewGuid().ToString(),_mojangProfile.Id, _mojangProfile.Name, EAccountType.MICROSOFT, mcToken, refreshToken, DateTime.Now.AddSeconds(expireSecs));
            
            _authStatus = EAuthStatus.SUCCESS;
            AuthService.StopListening(false);
        }
        catch (Exception ex)
        {
            _logger.Error("Error while getting Minecraft profile:");
            _logger.Error(ex.ToString());
            _authStatus = EAuthStatus.FAILED;
        }
    }
    
    public static async Task<bool> RefreshLoginAsync(string token)
    {
        if (string.IsNullOrEmpty(_microsoftClientId))
        {
            _logger.Error("Microsoft client ID is not set.");
            _authStatus = EAuthStatus.FAILED;
            return false;
        }

        string requestUrl = MicrosoftEndpoints.MicrosoftTokenUrl;
        
        try
        {
            var requestParams = new Dictionary<string, string>
            {
                { "client_id", _microsoftClientId },
                { "grant_type", "refresh_token" },
                { "refresh_token", token }
            };
            var requestContent = new FormUrlEncodedContent(requestParams);

            using HttpClient client = HttpHelper.GetHttpClient();
            var response = await client.PostAsync(requestUrl, requestContent).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                _logger.Error("Failed to get access token from Microsoft. Status code: " + response.StatusCode);
                _authStatus = EAuthStatus.FAILED;
                return false;
            }
            
            var responseString = await response.Content.ReadAsStringAsync();

            JObject obj = JObject.Parse(responseString);
            if (!obj.TryGetValue("access_token", out var value))
            {
                _logger.Error("Access token not found in the Microsoft authentication response.\"");
                _authStatus = EAuthStatus.FAILED;
                return false;
            }
            
            if (!obj.TryGetValue("refresh_token", out var refreshToken))
            {
                _logger.Error("Refresh token not found in the Microsoft authentication response.");
                _authStatus = EAuthStatus.FAILED;
                return false;
            }
           
            // Proceed with the token
            await XboxTokenCallAsync(value.ToString(), refreshToken.ToString());
            // Wait for the authentication process to complete
            _logger.Debug("Refresh auth status: " + AuthStatus);
            return true;
        }
        catch (Exception ex)
        {
            _logger.Exc("Error while handling HTTP request for Microsoft authentication:");
            _logger.Error(ex.ToString());
            _authStatus = EAuthStatus.FAILED;
            return false;
        }
    }
}