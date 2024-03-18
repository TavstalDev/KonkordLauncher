using KonkordLauncher.API.Constants;
using KonkordLauncher.API.Helpers;
using KonkordLauncher.API.Models;
using System.IO;
using System.Net;
using System.Net.Http;

namespace KonkordLauncher.API.Managers
{
    public class AuthenticationManager
    {
        private static HttpListener? _httpListener = null;
        private static bool _isListening = false;
        private static readonly string _listeningUrl = "http://localhost:43319/";
        private static readonly string _redirectAuthenticateUrl = Path.Combine(_listeningUrl, "authenticate");
        private static readonly string _redirectTokenUrl = Path.Combine(_listeningUrl, "token");

        private static readonly string _microsoftAuthorizeUrl = $"https://login.microsoftonline.com/consumers/oauth2/v2.0/authorize?client_id={Secrets.MSClientId}&response_type=code&redirect_uri={Uri.EscapeDataString(_redirectAuthenticateUrl)}&response_mode=query&scope=XboxLive.signin+offline_access&prompt=select_account";
        public static string MicrosoftAuthorizeUrl {  get { return _microsoftAuthorizeUrl; } }
        private static readonly string _microsoftTokenUrl = $"https://login.microsoftonline.com/consumers/oauth2/v2.0/token?client_id={Secrets.MSClientId}&grant_type=authorization_code&redirect_uri={Uri.EscapeDataString(_redirectTokenUrl)}&scope=XboxLive.signin+offline_access&prompt=select_account";
        public static string MicrosoftTokenUrl { get { return _microsoftTokenUrl; } }

        private static readonly string _minecraftAccountUrl = "https://api.minecraft.com/v1/account";

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
            {
                //httpResponseAction.Invoke();
                return;
            }

            if (context.Request.QueryString.AllKeys.Any(x => x == "error"))
            {
                NotificationHelper.SendError(context.Request.QueryString.Get("error_description") ?? "Unknown error.", "Error");
            }

            if (context.Request.RawUrl.StartsWith("/authenticate"))
            {
                string code = context.Request.QueryString["code"];
                closeBrowser.Invoke();

                // should get access token to minecraft and stuff

                return;

            }
            else if (context.Request.RawUrl.StartsWith("/token"))
            {
                NotificationHelper.SendInfo($"{context.Request.RawUrl}", "Debug");
                string token = context.Request.QueryString["token"];
                // Get Minecraft Account Details
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

                    var clientResponse = await client.PostAsync(_minecraftAccountUrl, null);
                    if (clientResponse.IsSuccessStatusCode)
                    {
                        string clientContent = await clientResponse.Content.ReadAsStringAsync();
                        NotificationHelper.SendInfo(clientContent, "Info");
                    }
                    else
                    {
                        NotificationHelper.SendError($"Failed to retrieve Minecraft account details. Status code: {clientResponse.StatusCode}", "Error");
                    }
                }
            }

            // Send Browser response
            closeBrowser.Invoke();
        }

        public static async Task<bool> ValidateAuthentication()
        {
            // Check account data and stuff
            AccountData? accountData = await JsonHelper.ReadJsonFile<AccountData>(Path.Combine(IOHelper.MainDirectory, "accounts.json"));
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

        
    }
}
