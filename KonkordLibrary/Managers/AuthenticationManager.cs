using Tavstal.KonkordLibrary.Helpers;
using Tavstal.KonkordLibrary.Models.Launcher;
using Tavstal.KonkordLibrary.Models.Minecraft.API;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Windows;

namespace Tavstal.KonkordLibrary.Managers
{
    public class AuthenticationManager
    {
        private static HttpListener? _httpListener = null;
        private static bool _isListening = false;
        public static bool IsListening { get { return _isListening; } }
        private static readonly string _listeningUrl = "http://localhost:43319/";
        private static bool _wasAuthSuccessful = false;
        private static bool _wasAuthCancelled = false;
        #region Redirectors
        private static readonly string _redirectAuthenticateUrl = Path.Combine(_listeningUrl, "microsoft/authcallback");
        private static readonly string _redirectTokenUrl = Path.Combine(_listeningUrl, "microsoft/tokencallback");
        #endregion

        private static readonly string _msClientId = "496a0c42-aa74-41fe-b7bc-0ad155cdaa26";
        #region Urls
        #region Microsoft
        private static readonly string _microsoftAuthUrl = $"https://login.microsoftonline.com/consumers/oauth2/v2.0/authorize?client_id={_msClientId}&response_type=code&redirect_uri={Uri.EscapeDataString(_redirectAuthenticateUrl)}&response_mode=query&scope=XboxLive.signin%20offline_access";
        public static string MicrosoftAuthUrl { get { return _microsoftAuthUrl; } }
        private static readonly string _microsoftTokenUrl = $"https://login.microsoftonline.com/consumers/oauth2/v2.0/token";
        public static string MicrosoftTokenUrl { get { return _microsoftTokenUrl; } }
        #endregion
        #region Xbox
        private static readonly string _xboxAuthUrl = $"https://user.auth.xboxlive.com/user/authenticate";
        public static string XboxAuthUrl { get { return _xboxAuthUrl; } }
        private static readonly string _xboxXSTSUrl = $"https://xsts.auth.xboxlive.com/xsts/authorize";
        public static string XboxXSTSUrl { get { return _xboxXSTSUrl; } }
        #endregion
        #region Minecraft
        private static readonly string _minecraftAuthUrl = "https://api.minecraftservices.com/authentication/login_with_xbox";
        public static string MinecraftAuthUrl { get { return _minecraftAuthUrl; } }
        private static readonly string _minecraftProfileUrl = "https://api.minecraftservices.com/minecraft/profile";
        public static string MinecraftProfileUrl { get { return _minecraftProfileUrl; } }
        private static readonly string _minecraftOwnershipUrl = "https://api.minecraftservices.com/entitlements/mcstore";
        public static string MinecraftOwnershipUrl { get { return _minecraftOwnershipUrl; } }
        #endregion
        #endregion

        /// <summary>
        /// Asynchronously validates the authentication.
        /// </summary>
        /// <returns>
        /// A <see cref="Task{TResult}"/> representing the asynchronous operation. The task result contains a <see cref="bool"/> value indicating whether the authentication is valid or not.
        /// </returns>
        public static async Task<bool?> ValidateAuthentication<LWindow, AWindow>(Window startWindow) where LWindow : Window where AWindow : Window
        {
            // Check account data and stuff
            AccountData? accountData = await JsonHelper.ReadJsonFileAsync<AccountData>(Path.Combine(IOHelper.MainDirectory, "accounts.json"));
            if (accountData == null)
            {
                NotificationHelper.SendErrorTranslated("launcher_account_not_found", "messagebox_error");
                return false;
            }

            if (accountData.Accounts.TryGetValue(accountData.SelectedAccountId, out Account? account))
            {
                if (account.Type == Enums.EAccountType.MICROSOFT)
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
                return true;
            }

            return false;
        }

        #region HTTP Listener
        /// <summary>
        /// Starts an HTTP listener to handle incoming HTTP requests.
        /// </summary>
        public static async void StartListening()
        {
            if (_httpListener == null)
            {
                _httpListener = new HttpListener();
                _httpListener.Prefixes.Add(_listeningUrl);
            }

            if (_isListening)
            {
                NotificationHelper.SendWarningTranslated("httplistener_already_active", "messagebox_warning");
                return;
            }

            try
            {
                _httpListener.Start();
                _isListening = true;
            }
            catch (HttpListenerException hlex)
            {
                NotificationHelper.SendErrorTranslated("httplistener_start_fail", "messagebox_error", new object[] { hlex });
                _isListening = false;
                return;
            }

            try
            {
                while (_isListening)
                {
                    HttpListenerContext context = await _httpListener.GetContextAsync(); // get te context 
                    if (context != null)
                        await OnRequestRecieved(context);
                }
            }
            catch { }
        }

        /// <summary>
        /// Stops the HTTP listener from accepting incoming HTTP requests.
        /// </summary>
        public static void StopListening()
        {
            if (_httpListener == null)
            {
                NotificationHelper.SendWarningTranslated("httplistener_stop_not_valid", "messagebox_warning");
                return;
            }

            _isListening = false;
            _httpListener.Stop();
        }

        /// <summary>
        /// Handles the asynchronous processing of an incoming HTTP request.
        /// </summary>
        /// <param name="context">The <see cref="HttpListenerContext"/> object representing the context of the incoming HTTP request.</param>
        private static async Task OnRequestRecieved(HttpListenerContext context)
        {
            Action closeBrowser = new Action(() =>
            {
                string responseString = "<html><head><title>Close Window</title></head><body><script>window.close();</script></body></html>";
                byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);

                HttpListenerResponse response = context.Response;
                response.ContentType = "text/html";
                response.ContentLength64 = buffer.Length;
                response.OutputStream.Write(buffer, 0, buffer.Length);
                response.Close();
            });

            if (context.Request.RawUrl == null)
                return;

            Debug.WriteLine($"### RECIEVED CALLBACK FROM {context.Request.RawUrl}");
            if (context.Request.QueryString.AllKeys.Any(x => x == "error"))
            {
                NotificationHelper.SendErrorTranslated(context.Request.QueryString.Get("error_description") ?? TranslationManager.Translate("error_unknown"), "messagebox_error");
                return;
            }

            if (context.Request.RawUrl.StartsWith("/microsoft/authcallback"))
            {
                closeBrowser.Invoke();
                _wasAuthSuccessful = false;
                _wasAuthCancelled = false;
                await MicrosoftAuthCallback(context.Request);
                return;
            }

            if (context.Request.RawUrl.StartsWith("/cancel"))
            {
                closeBrowser.Invoke();
                _wasAuthCancelled = true;
                _wasAuthSuccessful = false;
                return;
            }

            // Send Browser response
            closeBrowser.Invoke();
        }

        #region Callbacks
        /// <summary>
        /// Handles the Microsoft authentication callback asynchronously.
        /// </summary>
        /// <param name="request">The <see cref="HttpListenerRequest"/> object representing the incoming HTTP request.</param>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous operation.
        /// </returns>
        private static async Task MicrosoftAuthCallback(HttpListenerRequest request)
        {
            if (!request.QueryString.AllKeys.Contains("code"))
            {
                NotificationHelper.SendErrorTranslated("httplistener_authcode_not_found", "messagebox_error");
                return;
            }
            string? code = request.QueryString["code"];

            if (string.IsNullOrEmpty(code))
            {
                NotificationHelper.SendErrorTranslated("httplistener_authcode_not_found", "messagebox_error");
                return;
            }


            try
            {
                Debug.WriteLine($"## SENDING MICROSOFT AUTH REQUEST");

                var requestParams = new Dictionary<string, string>
                    {
                        { "client_id", _msClientId },
                        { "grant_type", "authorization_code" },
                        { "code", code },
                        { "redirect_uri", _redirectAuthenticateUrl}
                    };

                var requestContent = new FormUrlEncodedContent(requestParams);

                using (HttpClient client = HttpHelper.GetHttpClient())
                {
                    Debug.WriteLine("## SENT MICROSOFT AUTH REQUEST");
                    var response = await client.PostAsync($"{_microsoftTokenUrl}", requestContent).ConfigureAwait(false);
                    if (response == null)
                        return;
                    Debug.WriteLine($"## MICROSOFT AUTH REQUEST STATUS: {response.StatusCode}");
                    var responseString = await response.Content.ReadAsStringAsync();

                    JObject obj = JObject.Parse(responseString);
                    if (obj.ContainsKey("access_token"))
                    {
                        Debug.WriteLine("## FINISHED MICROSOFT AUTH REQUEST");
                        await XboxTokenCall(obj["access_token"].ToString());
                    }
                    else
                    {
                        Debug.WriteLine("## FAILED TO GET THE TOKEN FROM THE MICROSOFT AUTH REQUEST");
                        Debug.WriteLine(obj.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.Fail(ex.ToString());
            }
        }

        /// <summary>
        /// Makes an asynchronous call to Xbox token with the provided token.
        /// </summary>
        /// <param name="token">The microsoft token to be used for the call.</param>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous operation.
        /// </returns>
        private static async Task XboxTokenCall(string token)
        {
            Debug.WriteLine($"## BUILDING XBOX TOKEN REQUEST");
            try
            {
                object reqRawContent = new
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

                StringContent reqContent = new StringContent(JsonConvert.SerializeObject(reqRawContent));
                if (reqContent.Headers.Contains("Content-Type"))
                {
                    reqContent.Headers.Remove("Content-Type");
                    reqContent.Headers.Add("Content-Type", "application/json");
                }
                else
                    reqContent.Headers.Add("Content-Type", "application/json");

                using (HttpClient client = HttpHelper.GetHttpClient())
                {
                    Debug.WriteLine("## SENT XBOX AUTH REQUEST");
                    var result = await client.PostAsync(XboxAuthUrl, reqContent).ConfigureAwait(false);
                    if (result == null)
                        return;
                    Debug.WriteLine($"## XBOX AUTH REQUEST STATUS: {result.StatusCode}");
                    JObject resultObj = JObject.Parse(await result.Content.ReadAsStringAsync());
                    if (resultObj.ContainsKey("Token"))
                    {
                        Debug.WriteLine("## FINISHED XBOX AUTH REQUEST");
                        await XboxXstsCall(resultObj["Token"].ToString());
                    }
                    else
                    {
                        Debug.WriteLine("## FAILED TO GET THE TOKEN FROM XBOX AUTH REQUEST");
                        NotificationHelper.SendWarningMsg(resultObj.ToString(Newtonsoft.Json.Formatting.None), "XBOX TOKEN CALLBACK");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.Fail(ex.ToString());
            }
        }

        /// <summary>
        /// Makes an asynchronous call to Xbox XSTS (Xbox Secure Token Service) with the provided token.
        /// </summary>
        /// <param name="token">The Xbox token to be used for the call.</param>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous operation.
        /// </returns>
        private static async Task XboxXstsCall(string token)
        {
            try
            {
                object reqRawContent = new
                {
                    Properties = new
                    {
                        SandboxId = "RETAIL",
                        UserTokens = new[] { token }
                    },
                    RelyingParty = "rp://api.minecraftservices.com/",
                    TokenType = "JWT"
                };

                StringContent reqContent = new StringContent(JsonConvert.SerializeObject(reqRawContent));
                if (reqContent.Headers.Contains("Content-Type"))
                {
                    reqContent.Headers.Remove("Content-Type");
                    reqContent.Headers.Add("Content-Type", "application/json");
                }
                else
                    reqContent.Headers.Add("Content-Type", "application/json");

                using (HttpClient client = HttpHelper.GetHttpClient())
                {
                    Debug.WriteLine("## SENT XBOX XSTS REQUEST");
                    var result = await client.PostAsync(XboxXSTSUrl, reqContent).ConfigureAwait(false);
                    if (result == null)
                        return;
                    Debug.WriteLine("## XBOX XSTS REQUEST STATUS: " + result.StatusCode);
                    JObject resultObj = JObject.Parse(await result.Content.ReadAsStringAsync());
                    if (resultObj.ContainsKey("Token"))
                    {
                        await MinecraftAccessCall(resultObj["Token"].ToString(), resultObj["DisplayClaims"]["xui"][0]["uhs"].ToString());
                    }
                    else
                    {
                        NotificationHelper.SendWarningMsg(resultObj.ToString(Newtonsoft.Json.Formatting.None), "XBOX XSTS CALLBACK");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.Fail(ex.ToString());
            }
        }

        /// <summary>
        /// Makes an asynchronous call to Minecraft access with the provided token and user hash.
        /// </summary>
        /// <param name="token">The access token to be used for the call.</param>
        /// <param name="userHash">The user hash to be used for the call.</param>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous operation.
        /// </returns>
        private static async Task MinecraftAccessCall(string token, string userHash)
        {
            try
            {
                object reqObj = new
                {
                    identityToken = $"XBL3.0 x={userHash};{token}",
                    ensureLegacyEnabled = true
                };

                StringContent reqContent = new StringContent(JsonConvert.SerializeObject(reqObj));
                if (reqContent.Headers.Contains("Content-Type"))
                {
                    reqContent.Headers.Remove("Content-Type");
                    reqContent.Headers.Add("Content-Type", "application/json");
                }
                else
                    reqContent.Headers.Add("Content-Type", "application/json");

                using (HttpClient client = HttpHelper.GetHttpClient())
                {
                    Debug.WriteLine("## SENT MINECRAFT ACCESS REQUEST");
                    var result = await client.PostAsync(MinecraftAuthUrl, reqContent).ConfigureAwait(false);
                    if (result == null)
                        return;
                    Debug.WriteLine("## MINECRAFT ACCESS REQUEST STATUS: " + result.StatusCode);

                    JObject resultObj = JObject.Parse(await result.Content.ReadAsStringAsync());
                    if (resultObj.ContainsKey("access_token"))
                    {
                        await MinecraftCheckOwnership(resultObj["access_token"].ToString(), int.Parse(resultObj["expires_in"].ToString()));
                    }
                    else
                    {
                        NotificationHelper.SendWarningMsg(resultObj.ToString(Newtonsoft.Json.Formatting.None), "MINECRAFT ACCESS CALLBACK");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.Fail(ex.ToString());
            }
        }

        /// <summary>
        /// Asynchronously checks ownership of Minecraft using the provided access token.
        /// </summary>
        /// <param name="mcToken">The Minecraft access token.</param>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous operation.
        /// </returns>
        private static async Task MinecraftCheckOwnership(string mcToken, int expireSecs)
        {
            try
            {
                HttpClient client = HttpHelper.GetHttpClient();
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", mcToken);


                Debug.WriteLine("## SENT MINECRAFT OWNERSHIP REQUEST");
                var result = await client.GetAsync(_minecraftOwnershipUrl);
                Debug.WriteLine("## MINECRAFT OWNERSHIP REQUEST STATUS: " + result.StatusCode);

                OwnershipData? ownershipData = JsonConvert.DeserializeObject<OwnershipData>(await result.Content.ReadAsStringAsync());

                bool hasMinecraft = false;
                if (ownershipData != null)
                {
                    OwnershipItem? gameOwnership = ownershipData.Items.Find(x => x.Name == "game_minecraft");
                    if (gameOwnership != null)
                    {
                        hasMinecraft = true;
                    }
                }

                if (hasMinecraft)
                {
                    await MinecraftGetProfile(mcToken, expireSecs);
                }
                else
                {
                    NotificationHelper.SendWarningTranslated("minecraft_ownership_validate_fail", "MINECRAFT OWNERSHIP CALLBACK");
                }
            }
            catch (Exception ex)
            {
                Debug.Fail(ex.ToString());
            }
        }

        /// <summary>
        /// Asynchronously retrieves the Minecraft profile using the provided access token.
        /// </summary>
        /// <param name="mcToken">The Minecraft access token.</param>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous operation. The task result contains the profile data as a string.
        /// </returns>
        private static async Task MinecraftGetProfile(string mcToken, int expireSecs)
        {
            try
            {
                HttpClient client = HttpHelper.GetHttpClient();
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", mcToken);


                Debug.WriteLine("## SENT MINECRAFT PROFILE REQUEST");
                var result = await client.GetAsync(_minecraftProfileUrl);
                Debug.WriteLine("## MINECRAFT PROFILE REQUEST STATUS: " + result.StatusCode);

                MojangProfile? profile = JsonConvert.DeserializeObject<MojangProfile>(await result.Content.ReadAsStringAsync());

                if (profile != null)
                {
                    AccountData? accountData = await IOHelper.GetAccountDataAsync();
                    if (accountData == null)
                        return;

                    if (accountData.Accounts.TryGetValue(profile.Id, out Account? account))
                    {
                        account.AccessToken = mcToken;
                        account.AccessTokenExpireDate = DateTime.Now.AddSeconds(expireSecs); 
                        account.UUID = profile.Id;
                        account.DisplayName = profile.Name;
                        accountData.Accounts[profile.Id] = account;
                        accountData.SelectedAccountId = profile.Id;
                        await JsonHelper.WriteJsonFileAsync(IOHelper.AccountsJsonFile, accountData);
                    }
                    else
                    {
                        account = new Account()
                        {
                            AccessToken = mcToken,
                            AccessTokenExpireDate = DateTime.Now.AddSeconds(expireSecs),  
                            DisplayName = profile.Name,
                            UUID = profile.Id,
                            Type = Enums.EAccountType.MICROSOFT,
                            UserId = profile.Id,
                        };
                        accountData.Accounts.Add(profile.Id, account);
                        accountData.SelectedAccountId = profile.Id;
                        await JsonHelper.WriteJsonFileAsync(IOHelper.AccountsJsonFile, accountData);
                    }

                    // Cache profile
                    string profileCachePath = Path.Combine(IOHelper.CacheDir, $"{profile?.Id}.json");
                    await File.WriteAllTextAsync(profileCachePath, JsonConvert.SerializeObject(profile, Formatting.None));

                    _wasAuthSuccessful = true;
                    StopListening();
                }
                else
                {
                    NotificationHelper.SendWarningTranslated("minecraft_profile_not_found", "MINECRAFT PROFILE CALLBACK");
                }
            }
            catch (Exception ex)
            {
                Debug.Fail(ex.ToString());
            }
        }
        #endregion
        #endregion

        /// <summary>
        /// Gets the Microsoft authentication status.
        /// </summary>
        /// <returns>
        /// The Microsoft authentication status as a boolean value.
        /// </returns>
        public static bool GetMicrosoftAuthStatus()
        {
            return _wasAuthSuccessful;
        }

        /// <summary>
        /// Attempts to log in using the provided token asynchronously.
        /// </summary>
        /// <param name="token">The token to be used for login.</param>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous login attempt.
        /// </returns>
        public static async Task<bool> AttemptLogin(string mcToken)
        {
            try
            {
                HttpClient client = HttpHelper.GetHttpClient();
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", mcToken);


                Debug.WriteLine("## SENT MINECRAFT PROFILE REQUEST");
                var result = await client.GetAsync(_minecraftProfileUrl);
                Debug.WriteLine("## MINECRAFT PROFILE REQUEST STATUS: " + result.StatusCode);
                if (result.StatusCode == HttpStatusCode.Unauthorized || result.StatusCode == HttpStatusCode.NotFound || result.StatusCode == HttpStatusCode.BadRequest)
                    return false;

                MojangProfile? profile = JsonConvert.DeserializeObject<MojangProfile>(await result.Content.ReadAsStringAsync());
                // Save profile cache
                if (profile != null)
                {
                    string profileCachePath = Path.Combine(IOHelper.CacheDir, $"{profile.Id}.json");
                    await File.WriteAllTextAsync(profileCachePath, JsonConvert.SerializeObject(profile, Formatting.None));
                }

                return profile != null;
            }
            catch (Exception ex)
            {
                Debug.Fail(ex.ToString());
                return false;
            }
        }

        public static async Task<MojangProfile?> GetMojangProfileAsync()
        {
            try
            {
                AccountData? accountData = await JsonHelper.ReadJsonFileAsync<AccountData>(IOHelper.AccountsJsonFile);
                if (accountData == null)
                    return null;

                if (!accountData.Accounts.TryGetValue(accountData.SelectedAccountId, out Account? account))
                    return null;

                string profileCachePath = Path.Combine(IOHelper.CacheDir, $"{account.UUID}.json");
                return JsonConvert.DeserializeObject<MojangProfile>(await File.ReadAllTextAsync(profileCachePath));
            }
            catch (Exception ex)
            {
                Debug.Fail(ex.ToString());
                return null;
            }
        }


        public static async Task UpdateSkin(string mcToken)
        {

        }
    }
}
