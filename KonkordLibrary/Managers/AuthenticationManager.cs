using KonkordLibrary.Helpers;
using KonkordLibrary.Models;
using System.Net;
using System.IO;
using System.Net.Http;
using System.Windows.Controls;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using static System.Formats.Asn1.AsnWriter;

namespace KonkordLibrary.Managers
{
    public class AuthenticationManager
    {
        private static HttpListener? _httpListener = null;
        private static bool _isListening = false;
        private static readonly string _listeningUrl = "http://localhost:43319/";
        #region Redirectors
        private static readonly string _redirectAuthenticateUrl = Path.Combine(_listeningUrl, "microsoft/authcallback");
        private static readonly string _redirectTokenUrl = Path.Combine(_listeningUrl, "microsoft/tokencallback");
        #endregion

        private static readonly string _msClientId = "a34081c9-9053-4874-b802-075601be2615";
        #region Urls
        #region Microsoft
        private static readonly string _microsoftAuthUrl = $"https://login.microsoftonline.com/consumers/oauth2/v2.0/authorize?client_id={_msClientId}&response_type=code&redirect_uri={Uri.EscapeDataString(_redirectAuthenticateUrl)}&response_mode=query&scope=XboxLive.signin+offline_access";
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
        #endregion
        #endregion

        public static async Task<bool> ValidateAuthentication()
        {
            // Check account data and stuff
            AccountData? accountData = await JsonHelper.ReadJsonFileAsync<AccountData>(Path.Combine(IOHelper.MainDirectory, "accounts.json"));
            if (accountData == null)
            {
                NotificationHelper.SendError("Failed to get the account data.", "Error");
                return false;
            }

            if (accountData.Accounts.TryGetValue(accountData.SelectedAccountId, out Account? account))
            {
                if (account.Type == Enums.EAccountType.MICROSOFT)
                {
                    // TODO: After the authentication
                }
                return true;
            }

            return false;
        }

        #region HTTP Listener
        public static void StartListening()
        {
            if (_httpListener == null)
            {
                _httpListener = new HttpListener();
                _httpListener.Prefixes.Add(_listeningUrl);
            }

            if (_isListening)
            {
                NotificationHelper.SendWarning("The HTTP listener is already active.", "Warning");
                return;
            }

            try
            {
                _httpListener.Start();
                _isListening = true;
            }
            catch (HttpListenerException hlex)
            {
                NotificationHelper.SendError("Can't start the agent to listen transaction" + hlex, "Error");
                _isListening = false;
                return;
            }

            while (_isListening)
            {
                HttpListenerContext context = _httpListener.GetContext(); // get te context 
                if (context != null)
                    OnRequestRecieved(context);
            }
        }

        public static void StopListening()
        {
            if (_httpListener == null)
            {
                NotificationHelper.SendWarning("Can't stop the HTTP listener because it is null.", "Warning");
                return;
            }

            _isListening = false;
            _httpListener.Stop();
        }

        private static async void OnRequestRecieved(HttpListenerContext context)
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
                NotificationHelper.SendError(context.Request.QueryString.Get("error_description") ?? "Unknown error.", "Error");
                return;
            }

            if (context.Request.RawUrl.StartsWith("/microsoft/authcallback"))
            {
                closeBrowser.Invoke();
                await MicrosoftAuthCallback(context.Request);
                return;
            }
            else if (context.Request.RawUrl.StartsWith("/microsoft/tokencallback"))
            {
                await MicrosoftTokenCallback(context.Request);
            }

            // Send Browser response
            closeBrowser.Invoke();
        }

        #region Callbacks
        private static async Task MicrosoftAuthCallback(HttpListenerRequest request)
        {
            if (!request.QueryString.AllKeys.Contains("code"))
            {
                NotificationHelper.SendError("Couldn't get the auth code key from the url.", "Error");
                return;
            }
            string? code = request.QueryString["code"];

            if (string.IsNullOrEmpty(code))
            {
                NotificationHelper.SendError("Couldn't get the auth code value from the url.", "Error");
                return;
            }

            using (HttpClient client = new HttpClient())
            {
                Debug.WriteLine($"## SENDING MICROSOFT AUTH REQUEST");

                var requestParams = new Dictionary<string, string>
                {
                    { "client_id", _msClientId },
                    { "grant_type", "authorization_code" },
                    { "code", code },
                    { "scope", "Files.Read" },
                    { "redirect_uri", _redirectAuthenticateUrl}
                };

                var requestContent = new FormUrlEncodedContent(requestParams);
                var response = await client.PostAsync($"{_microsoftTokenUrl}", requestContent);
                Debug.WriteLine($"Status: {response.StatusCode}");
                var responseString = await response.Content.ReadAsStringAsync();
                Debug.WriteLine("## SENT MICROSOFT AUTH REQUEST");
                JObject obj = JObject.Parse(responseString);
                if (obj.ContainsKey("Token"))
                {
                    await XboxTokenCall(obj["Token"].ToString());
                }
                else
                {
                    NotificationHelper.SendWarning(obj.ToString(Newtonsoft.Json.Formatting.None), "MICROSOFT AUTH CALLBACK");
                }
            }
        }

        private static async Task XboxTokenCall(string token)
        {
            using (HttpClient client = new HttpClient())
            {
                HttpRequestMessage tokenRequest = new HttpRequestMessage();
                tokenRequest.Method = HttpMethod.Post;
                tokenRequest.Headers.Add("Content-Type", "application/json");
                JObject obj = new JObject
                {
                    { "Properties", new JObject() {
                        { "AuthMethod", "RPS" },
                        { "SiteName", "user.auth.xboxlive.com" },
                        { "RpsTicket", $"d={token}" }
                    }},
                    { "RelyingParty", "http://auth.xboxlive.com" },
                    { "TokenType", "JWT" }
                };
                tokenRequest.Content = new StringContent(obj.ToString(Newtonsoft.Json.Formatting.None));
                Debug.WriteLine("## SENT XBOX AUTH REQUEST");
                var result = await client.SendAsync(tokenRequest);
                var response = result.EnsureSuccessStatusCode();
                JObject resultObj = JObject.Parse(await response.Content.ReadAsStringAsync());
                if (resultObj.ContainsKey("Token"))
                {
                    await XboxXstsCall(resultObj["Token"].ToString());
                }
                else
                {
                    NotificationHelper.SendWarning(obj.ToString(Newtonsoft.Json.Formatting.None), "XBOX TOKEN CALLBACK");
                }
            }
        }

        private static async Task XboxXstsCall(string token)
        {
            using (HttpClient client = new HttpClient())
            {
                HttpRequestMessage tokenRequest = new HttpRequestMessage();
                tokenRequest.Method = HttpMethod.Post;
                tokenRequest.Headers.Add("Content-Type", "application/json");
                JObject obj = new JObject
                {
                    { "Properties", new JObject() {
                        { "SandboxId", "RETAIL" },
                        { "UserTokens", $"[\"{token}\"]" }
                    }},
                    { "RelyingParty", "rp://api.minecraftservices.com/" },
                    { "TokenType", "JWT" }
                };
                tokenRequest.Content = new StringContent(obj.ToString(Newtonsoft.Json.Formatting.None));
                Debug.WriteLine("## SENT XBOX XSTS REQUEST");
                var result = await client.SendAsync(tokenRequest);
                var response = result.EnsureSuccessStatusCode();
                JObject resultObj = JObject.Parse(await response.Content.ReadAsStringAsync());
                if (resultObj.ContainsKey("Token"))
                {
                    await MinecraftAccessCall(resultObj["Token"].ToString(), resultObj["DisplayClaims"]["xui"][0]["uhs"].ToString());
                }
                else
                {
                    NotificationHelper.SendWarning(obj.ToString(Newtonsoft.Json.Formatting.None), "XBOX XSTS CALLBACK");
                }
            }
        }

        private static async Task MinecraftAccessCall(string token, string userHash)
        {
            using (HttpClient client = new HttpClient())
            {
                HttpRequestMessage tokenRequest = new HttpRequestMessage();
                tokenRequest.Method = HttpMethod.Post;
                tokenRequest.Headers.Add("Content-Type", "application/json");
                tokenRequest.Content = new StringContent(string.Format("{\"identityToken\": \"XBL3.0 x={0};{1}\"}", userHash, token));
                Debug.WriteLine("## SENT MINECRAFT ACCESS REQUEST");
                var result = await client.SendAsync(tokenRequest);
                var response = result.EnsureSuccessStatusCode();
                JObject resultObj = JObject.Parse(await response.Content.ReadAsStringAsync());
                if (resultObj.ContainsKey("access_token"))
                {
                    await XboxXstsCall(resultObj["access_token"].ToString());
                }
                else
                {
                    NotificationHelper.SendWarning(resultObj.ToString(Newtonsoft.Json.Formatting.None), "MINECRAFT ACCESS CALLBACK");
                }
            }
        }

        private static async Task MicrosoftTokenCallback(HttpListenerRequest request)
        {
            
        }
        #endregion
        #endregion

        public static async Task AttemptLogin(string token)
        {
            using (HttpClient client = new HttpClient())
            {
                HttpRequestMessage tokenRequest = new HttpRequestMessage();
                tokenRequest.Method = HttpMethod.Get;
                tokenRequest.Headers.Add("Authorization", $"Bearer {token}");
                var result = await client.SendAsync(tokenRequest);
                var response = result.EnsureSuccessStatusCode();
                JObject resultObj = JObject.Parse(await response.Content.ReadAsStringAsync());

                NotificationHelper.SendNotification(resultObj.ToString(Newtonsoft.Json.Formatting.None), "AAAA");
            }
        }
    }
}
