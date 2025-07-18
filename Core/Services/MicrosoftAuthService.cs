using System.Net;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Tavstal.KonkordLauncher.Core.Enums;
using Tavstal.KonkordLauncher.Core.Helpers;
using Tavstal.KonkordLauncher.Core.Models;
using Tavstal.KonkordLauncher.Core.Models.Launcher;
using Tavstal.KonkordLauncher.Core.Models.MojangApi.User;

namespace Tavstal.KonkordLauncher.Core.Services;

/// <summary>
/// Provides services for handling Microsoft authentication and related operations.
/// </summary>
public static class MicrosoftAuthService
{
    private static readonly CoreLogger _logger = new(typeof(MicrosoftAuthService), false);
    private static string _microsoftClientId = "496a0c42-aa74-41fe-b7bc-0ad155cdaa26"; // TODO: Remove hardcoded client ID and set it via SetClientId method.
    private static readonly string _redirectAuthenticateUrl = Path.Combine(AuthService.ListeningUrl, "microsoft/authcallback");
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
        _account = null;
        _mojangProfile = null;
        _authStatus = EAuthStatus.NONE;
    }
    
    /// <summary>
    /// Handles an HTTP request for Microsoft authentication.
    /// </summary>
    /// <param name="request">The HTTP request to handle.</param>
    public static async Task HandleHttpRequestAsync(HttpListenerRequest request)
    {
        _authStatus = EAuthStatus.PENDING;
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

        string requestUrl = MicrosoftEndpoints.MakeMicrosoftAuthUrl(_microsoftClientId, _redirectAuthenticateUrl);
        
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
                _logger.Error("Access token not found in the response.");
                _authStatus = EAuthStatus.FAILED;
                return;
            }
           
            // Proceed with the token
            await XboxTokenCallAsync(value.ToString());
        }
        catch (Exception ex)
        {
            _logger.Exc("Error while handling HTTP request for Microsoft authentication:");
            _logger.Error(ex.ToString());
            _authStatus = EAuthStatus.FAILED;
        }
    }
    
    /// <summary>
    /// Makes a call to the Xbox authentication service with the provided token.
    /// </summary>
    /// <param name="token">The token to use for authentication.</param>
    private static async Task XboxTokenCallAsync(string token)
    {
        try
        {
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
            
            await XboxXstsCallAsync(value.ToString());
        }
        catch (Exception ex)
        {
            _logger.Exc("Error while making Xbox token call:");
            _logger.Error(ex.ToString());
            _authStatus = EAuthStatus.FAILED;
        }
    }
    
    /// <summary>
    /// Makes a call to the Xbox XSTS service with the provided token.
    /// </summary>
    /// <param name="token">The token to use for the XSTS call.</param>
    private static async Task XboxXstsCallAsync(string token)
    {
        try
        {
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

            JObject resultObj = JObject.Parse(await result.Content.ReadAsStringAsync());
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
            if (xui is not { HasValues: true })
            {
                _logger.Error("xui not found or empty in the Xbox XSTS response.");
                _authStatus = EAuthStatus.FAILED;
                return;
            }

            var firstXui = xui[0];
            if (firstXui is not { HasValues: true })
            {
                _logger.Error("User hash (uhs) not found in the Xbox XSTS response.");
                _authStatus = EAuthStatus.FAILED;
                return;
            }
            
            var userHash = firstXui["uhs"];
            if (userHash is not { HasValues: true })
            {
                _logger.Error("User hash (uhs) is null in the Xbox XSTS response.");
                _authStatus = EAuthStatus.FAILED;
                return;
            }
            
            await MinecraftAccessCallAsync(value.ToString(), userHash.ToString());
        }
        catch (Exception ex)
        {
            _logger.Exc("Error while making Xbox XSTS call:");
            _logger.Error(ex.ToString());
            _authStatus = EAuthStatus.FAILED;
        }
    }
    
    /// <summary>
    /// Makes a call to the Minecraft authentication service with the provided token and user hash.
    /// </summary>
    /// <param name="token">The token to use for authentication.</param>
    /// <param name="userHash">The user hash associated with the token.</param>
    private static async Task MinecraftAccessCallAsync(string token, string userHash)
    {
        try
        {
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
            
            await CheckMinecraftOwnershipAsync(minecraftToken.ToString(), int.Parse(expiresIn.ToString()));
        }
        catch (Exception ex)
        {
            _logger.Exc("Error while making Minecraft access call:");
            _logger.Error(ex.ToString());
            _authStatus = EAuthStatus.FAILED;
        }
    }
    
    /// <summary>
    /// Checks if the user owns Minecraft using the provided token.
    /// </summary>
    /// <param name="mcToken">The Minecraft access token.</param>
    /// <param name="expireSeconds">The expiration time of the token in seconds.</param>
    private static async Task CheckMinecraftOwnershipAsync(string mcToken, int expireSeconds)
    {
        try
        {
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

            await GetMinecraftProfileAsync(mcToken, expireSeconds);
        }
        catch (Exception ex)
        {
            _logger.Exc("Error while checking Minecraft ownership:");
            _logger.Error(ex.ToString());
            _authStatus = EAuthStatus.FAILED;
        }
    }
    
    /// <summary>
    /// Retrieves the Minecraft profile of the user using the provided token.
    /// </summary>
    /// <param name="mcToken">The Minecraft access token.</param>
    /// <param name="expireSecs">The expiration time of the token in seconds.</param>
    private static async Task GetMinecraftProfileAsync(string mcToken, int expireSecs)
    {
        try
        {
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

            _account = new Account()
            {
                AccessToken = mcToken,
                AccessTokenExpireDate = DateTime.Now.AddSeconds(expireSecs),
                DisplayName = _mojangProfile.Name,
                UUID = _mojangProfile.Id,
                Type = EAccountType.MICROSOFT,
                UserId = _mojangProfile.Id
            };
            
            _authStatus = EAuthStatus.SUCCESS;
            AuthService.StopListening();
        }
        catch (Exception ex)
        {
            _logger.Error("Error while getting Minecraft profile:");
            _logger.Error(ex.ToString());
            _authStatus = EAuthStatus.FAILED;
        }
    }
    
    /// <summary>
    /// Attempts to log in to the Minecraft service using the provided access token.
    /// </summary>
    /// <param name="mcToken">The Minecraft access token to authenticate the user.</param>
    /// <returns>
    /// A <see cref="MojangProfile"/> object containing the user's profile information if the login is successful; 
    /// otherwise, <c>null</c> if the login fails or an error occurs.
    /// </returns>
    public static async Task<MojangProfile?> AttemptLoginAsync(string mcToken)
    {
        try
        {
            HttpClient client = HttpHelper.GetHttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", mcToken);
            
            var result = await client.GetAsync(MicrosoftEndpoints.MinecraftProfileUrl);

            if (!result.IsSuccessStatusCode)
                return null;
            
            return JsonConvert.DeserializeObject<MojangProfile>(await result.Content.ReadAsStringAsync());
        }
        catch (Exception ex)
        {
            _logger.Exc("Error while attempting to login:");
            _logger.Error(ex.ToString());
            return null;
        }
    }
    
    /*
     * OLD CODE TO REAUTHENTICATE MICROSOFT ACCOUNT
     * if (account.Type == Enums.EAccountType.MICROSOFT)
       {
           if (account.AccessTokenExpireDate > DateTime.Now)
               if (await AttemptLogin(account.AccessToken))
                   return true;

           // Start reauthenticate
           StartListening();

           var psi = new ProcessStartInfo
           {
               FileName = MicrosoftAuthUrl,
               UseShellExecute = true
           };
           Process.Start(psi);

           while (IsListening)
           {
               await Task.Delay(50);
               if (GetMicrosoftAuthStatus())
               {
                   LWindow window = Activator.CreateInstance<LWindow>();
                   window.Show();
                   startWindow.Close();
                   break;
               }

               if (_wasAuthCancelled)
               {
                   AWindow window = Activator.CreateInstance<AWindow>();
                   window.Show();
                   startWindow.Close();
                   break;
               }
           }

           return null;
       }
     */
}