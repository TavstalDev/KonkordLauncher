using System.Net;
using Tavstal.KonkordLauncher.Core.Models;

namespace Tavstal.KonkordLauncher.Core.Services;

/// <summary>
/// Provides authentication services, including starting and stopping an HTTP listener
/// and handling HTTP requests for authentication callbacks.
/// </summary>
public static class AuthHttpListener
{
    private static bool _isListening;
    private static HttpListener? _httpListener;
    public const string ListeningUrl = "http://localhost:43319/";
    private static readonly CoreLogger _logger = new(typeof(AuthHttpListener));
    private static IProgressReporter? _progressReporter;
    
    /// <summary>
    /// Starts the HTTP listener to handle incoming authentication requests.
    /// </summary>
    public static async Task StartListening(IProgressReporter? progressReporter = null, CancellationToken cancellationToken = default)
    {
        _progressReporter = progressReporter;
        
        if (_httpListener == null)
        {
            _httpListener = new HttpListener();
            _httpListener.Prefixes.Add(ListeningUrl);
        }

        if (_isListening)
            return;

        try
        {
            progressReporter?.UpdateStatusTranslated("auth.listener.starting");
            _httpListener.Start();
            _isListening = true;
        }
        catch (HttpListenerException ex)
        {
            progressReporter?.UpdateStatusTranslated("auth.listener.failed");
            _logger.Exc("Failed to start HTTP listener:");
            _logger.Error(ex.ToString());
            _isListening = false;
            return;
        }

        while (_isListening)
        {
            HttpListenerContext context = await _httpListener.GetContextAsync(); // get te context 
            await HandleHttpRequestAsync(context, cancellationToken);
        }
    }
    
    /// <summary>
    /// Stops the HTTP listener if it is currently active.
    /// </summary>
    public static void StopListening(bool cancelled = true)
    {
        if (_httpListener == null)
            return;
        
        if (!_isListening)
            return;

        if (cancelled)
            _progressReporter?.UpdateStatusTranslated("auth.listener.cancelled");
        _isListening = false;
        _httpListener.Stop();
    }

    
    /// <summary>
    /// Handles an incoming HTTP request and processes it based on the request URL.
    /// </summary>
    /// <param name="context">The HTTP context containing the request and response.</param>
    private static async Task HandleHttpRequestAsync(HttpListenerContext context, CancellationToken cancellationToken = default)
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
            await CloseBrowserAsync(context, cancellationToken);
            _progressReporter?.UpdateStatusTranslated("auth.listener.callback");
            await MicrosoftAuthService.HandleHttpRequestAsync(context.Request, _progressReporter);
            return;
        }

        if (context.Request.RawUrl.StartsWith("/cancel"))
        {
            await CloseBrowserAsync(context, cancellationToken);
            return;
        }

        // Send Browser response
        await CloseBrowserAsync(context, cancellationToken);
    }

    /// <summary>
    /// Sends a response to the browser indicating that the authentication process is complete
    /// and the browser window can be closed.
    /// </summary>
    /// <param name="context">The HTTP context containing the response to send.</param>
    private static async Task CloseBrowserAsync(HttpListenerContext context, CancellationToken cancellationToken = default)
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
        await response.OutputStream.WriteAsync(buffer, 0, buffer.Length, cancellationToken);
        response.Close();
    }
}