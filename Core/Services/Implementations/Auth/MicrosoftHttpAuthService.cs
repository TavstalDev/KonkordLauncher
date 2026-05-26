using System.Net;
using Microsoft.Extensions.Logging;
using Tavstal.KonkordLauncher.Core.Enums;
using Tavstal.KonkordLauncher.Core.Models;
using Tavstal.KonkordLauncher.Core.Services.Abstractions.Auth;

namespace Tavstal.KonkordLauncher.Core.Services.Implementations.Auth;

/// <inheritdoc/>
public class MicrosoftHttpAuthService : IMicrosoftHttpAuthService
{
    private readonly ILogger _logger;
    private readonly IMicrosoftAuthService _microsoftAuthService;
    private bool _isListening;
    private HttpListener? _httpListener;
    public const string ListeningUrl = "http://localhost:43319/";
    private IProgressReporter? _progressReporter;

    public MicrosoftHttpAuthService(ILogger<MicrosoftHttpAuthService> logger,
        IMicrosoftAuthService microsoftAuthService)
    {
        _logger = logger;
        _microsoftAuthService = microsoftAuthService;
        _microsoftAuthService.OnAuthStatusChanged += OnAuthStatusChanged;
    }

    private void OnAuthStatusChanged(EAuthStatus status)
    {
        if (status == EAuthStatus.SUCCESS)
            Task.Run(async () => await StopListeningAsync());
    }

    /// <inheritdoc/>
    public async Task StartListeningAsync(IProgressReporter? progressReporter = null,
        CancellationToken cancellationToken = default)
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
            _logger.LogError($"Failed to start HTTP listener: {ex}");
            _isListening = false;
            return;
        }

        while (_isListening)
        {
            HttpListenerContext context = await _httpListener.GetContextAsync(); // get te context 
            await HandleHttpRequestAsync(context, cancellationToken);
        }
    }

    /// <inheritdoc/>
    public Task StopListeningAsync(bool cancelled = true, CancellationToken cancellationToken = default)
    {
        if (_httpListener == null)
            return Task.CompletedTask;

        if (!_isListening)
            return Task.CompletedTask;

        if (cancelled)
            _progressReporter?.UpdateStatusTranslated("auth.listener.cancelled");
        _isListening = false;
        _httpListener.Stop();
        return Task.CompletedTask;
    }

    /// <summary>
    /// Handles an incoming HTTP request from the local authentication listener.
    /// Routes the request to the appropriate handler based on the URL path.
    /// </summary>
    /// <param name="context">The HTTP context containing the request and response objects.</param>
    /// <param name="cancellationToken">Token to cancel the operation if needed.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task HandleHttpRequestAsync(HttpListenerContext context,
        CancellationToken cancellationToken = default)
    {
        if (context.Request.RawUrl == null)
            return;

        _logger.LogDebug("Received HTTP request: " + context.Request.RawUrl);
        if (context.Request.QueryString.AllKeys.Any(x => x == "LogError"))
        {
            _logger.LogError("Authentication LogError received in callback.");
            _logger.LogError(context.Request.QueryString.Get("LogError_description") ?? "Unknown LogError");
            return;
        }

        if (context.Request.RawUrl.StartsWith("/microsoft/authcallback"))
        {
            await CloseBrowserAsync(context, cancellationToken);
            _progressReporter?.UpdateStatusTranslated("auth.listener.callback");
            await _microsoftAuthService.HandleHttpRequestAsync(context.Request, _progressReporter, cancellationToken);
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
    /// Sends an HTML response to the client that closes the browser window.
    /// </summary>
    /// <param name="context">The HTTP context containing the response object to write to.</param>
    /// <param name="cancellationToken">Token to cancel the write operation if needed.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task CloseBrowserAsync(HttpListenerContext context, CancellationToken cancellationToken = default)
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