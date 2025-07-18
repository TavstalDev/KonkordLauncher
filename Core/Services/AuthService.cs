using System.Net;
using Tavstal.KonkordLauncher.Core.Models;

namespace Tavstal.KonkordLauncher.Core.Services;

/// <summary>
/// Provides authentication services, including starting and stopping an HTTP listener
/// and handling HTTP requests for authentication callbacks.
/// </summary>
public static class AuthService
{
    private static bool _isListening;
    private static HttpListener? _httpListener;
    public const string ListeningUrl = "http://localhost:43319/";
    private static readonly CoreLogger _logger = new(typeof(AuthService), false);
    
    /// <summary>
    /// Starts the HTTP listener to handle incoming authentication requests.
    /// </summary>
    public static async Task StartListening()
    {
        if (_httpListener == null)
        {
            _httpListener = new HttpListener();
            _httpListener.Prefixes.Add(ListeningUrl);
        }

        if (_isListening)
            return;

        try
        {
            _httpListener.Start();
            _isListening = true;
        }
        catch (HttpListenerException ex)
        {
            _logger.Exc("Failed to start HTTP listener:");
            _logger.Error(ex.ToString());
            _isListening = false;
            return;
        }

        while (_isListening)
        {
            HttpListenerContext context = await _httpListener.GetContextAsync(); // get te context 
            await HandleHttpRequestAsync(context);
        }
    }
    
    /// <summary>
    /// Stops the HTTP listener if it is currently active.
    /// </summary>
    public static void StopListening()
    {
        if (_httpListener == null)
        {
            _logger.Error("HTTP listener is not initialized.");
            return;
        }
        
        if (!_isListening)
        {
            _logger.Error("HTTP listener is not currently listening.");
            return;
        }

        _isListening = false;
        _httpListener.Stop();
    }

    
    /// <summary>
    /// Handles an incoming HTTP request and processes it based on the request URL.
    /// </summary>
    /// <param name="context">The HTTP context containing the request and response.</param>
    private static async Task HandleHttpRequestAsync(HttpListenerContext context)
    {
        if (context.Request.RawUrl == null)
            return;
        
        _logger.Debug("Received HTTP request: " + context.Request.RawUrl);
        if (context.Request.QueryString.AllKeys.Any(x => x == "error"))
        {
            _logger.Error("Authentication error received in callback.");
            _logger.Error(context.Request.QueryString.Get("error_description") ?? "Unknown error");
            return;
        }

        if (context.Request.RawUrl.StartsWith("/microsoft/authcallback"))
        {
            await CloseBrowserAsync(context);
            await MicrosoftAuthService.HandleHttpRequestAsync(context.Request);
            return;
        }

        if (context.Request.RawUrl.StartsWith("/cancel"))
        {
            await CloseBrowserAsync(context);
            return;
        }

        // Send Browser response
        await CloseBrowserAsync(context);
    }

    /// <summary>
    /// Sends a response to the browser indicating that the authentication process is complete
    /// and the browser window can be closed.
    /// </summary>
    /// <param name="context">The HTTP context containing the response to send.</param>
    private static async Task CloseBrowserAsync(HttpListenerContext context)
    {
        const string responseString = @"
                    <!DOCTYPE html>
                    <html>
                    <head>
                        <title>Microsoft Authentication</title>
                    </head>
                    <body>
                        <h1>You may close this window.</h1>
                    </body>
                    <script>
                        window.open('', '_self').close();
                    </script>
                    </html>";
        byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);

        HttpListenerResponse response = context.Response;
        response.ContentType = "text/html";
        response.ContentLength64 = buffer.Length;
        await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
        response.Close();
    }
}