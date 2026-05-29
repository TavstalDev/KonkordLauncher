using System.Net;
using System.Net.Http.Json;
using System.Net.Security;
using System.Security.Authentication;
using Tavstal.KonkordLauncher.Core.Helpers.Platform;
using Tavstal.KonkordLauncher.Core.Models.Logging;
using Tavstal.KonkordLauncher.Core.Services.Abstractions;

namespace Tavstal.KonkordLauncher.Core.Services.Implementations;

/// <inheritdoc/>
public class HttpService : IHttpService
{
    private readonly ICustomLogger _logger;
    private readonly HttpClient _httpClient;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="HttpService"/> class.
    /// </summary>
    /// <param name="logger">Logger instance used to record diagnostic and error messages for HTTP operations.</param>
    public HttpService(ICustomLogger<HttpService> logger)
    {
        _logger = logger;
        _httpClient = CreateHttpClient();
    }

    /// <inheritdoc/>
    public HttpClient CreateHttpClient()
    {
        HttpClient client;
        if (OSHelper.IsWindows11())
        {
            var handler = new SocketsHttpHandler
            {
                SslOptions = new SslClientAuthenticationOptions
                {
                    EnabledSslProtocols = SslProtocols.Tls12
                },
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
                AllowAutoRedirect = true
            };
            handler.EnableMultipleHttp2Connections = false;
            client = new HttpClient(handler);
        }
        else
        {
            client = new HttpClient();
        }

        client.DefaultRequestHeaders.Accept.ParseAdd("application/json");
        client.DefaultRequestHeaders.UserAgent.ParseAdd("KonkordLauncher/2.0.0 (+https://github.com/TavstalDev/KonkordLauncher)");
        client.Timeout = TimeSpan.FromSeconds(120);
        return client;
    }

    /// <inheritdoc/>
    public async Task<HttpResponseMessage?> GetAsync(string url, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _httpClient.GetAsync(url, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError($"HTTP request to {url} failed with { ex.StatusCode}: {ex.Message}");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, $"Failed to make GET request to {url}:");
            return null;
        }
    }

    /// <inheritdoc/>
    public async Task<string?> GetStringAsync(string url, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _httpClient.GetStringAsync(url, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError($"HTTP request to {url} failed with { ex.StatusCode}: {ex.Message}");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, $"Failed to make GET request to {url}:");
            return null;
        }
    }

    /// <inheritdoc/>
    public async Task<string?> GetStringAsync(string url, IProgress<double>? progress, CancellationToken cancellationToken = default)
    {
        try
        {
            HttpResponseMessage response = await _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            response.EnsureSuccessStatusCode();

            long? contentLength = response.Content.Headers.ContentLength;

            await using Stream responseStream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var memoryStream = new MemoryStream();
            byte[] buffer = new byte[4096];
            int bytesRead;
            long totalBytesRead = 0;

            while ((bytesRead = await responseStream.ReadAsync(buffer, 0, buffer.Length, cancellationToken)) > 0)
            {
                await memoryStream.WriteAsync(buffer, 0, bytesRead, cancellationToken);
                totalBytesRead += bytesRead;

                if (progress != null && contentLength.HasValue)
                {
                    double percentage = ((double)totalBytesRead / contentLength.Value) * 100;
                    progress.Report(percentage);
                }
            }

            return System.Text.Encoding.UTF8.GetString(memoryStream.ToArray());
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (HttpRequestException ex)
        {
             _logger.LogError($"HTTP request to {url} failed with { ex.StatusCode}: {ex.Message}");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, $"Failed to make GET request to {url}:");
            return null;
        }
    }

    /// <inheritdoc/>
    public async Task<byte[]?> GetByteArrayAsync(string url, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _httpClient.GetByteArrayAsync(url, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (HttpRequestException ex)
        {
             _logger.LogError($"HTTP request to {url} failed with { ex.StatusCode}: {ex.Message}");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, $"Failed to make GET request to {url}:");
            return null;
        }
    }

    /// <inheritdoc/>
    public async Task<Stream?> GetStreamAsync(string url, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _httpClient.GetStreamAsync(url, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (HttpRequestException ex)
        {
             _logger.LogError($"HTTP request to {url} failed with { ex.StatusCode}: {ex.Message}");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, $"Failed to make GET request to {url}:");
            return null;
        }
    }

    /// <inheritdoc/>
    public async Task<T?> GetObjectFromJsonAsync<T>(string url, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<T>(url, cancellationToken: cancellationToken);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (HttpRequestException ex)
        {
             _logger.LogError($"HTTP request to {url} failed with { ex.StatusCode}: {ex.Message}");
            return default;
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, $"Failed to make GET request to {url}:");
            return default;
        }
    }

    /// <inheritdoc/>
    public async Task<string?> DownloadFileAsync(string url, string filePath, IProgress<double>? progress, CancellationToken cancellationToken = default)
    {
        try
        {
            HttpResponseMessage response = await _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            response.EnsureSuccessStatusCode();

            long? contentLength = response.Content.Headers.ContentLength;

            await using Stream responseStream = await response.Content.ReadAsStreamAsync(cancellationToken);
            await using var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.Read);

            byte[] buffer = new byte[8192]; // Use a larger buffer for better performance
            int bytesRead;
            long totalBytesRead = 0;

            while ((bytesRead = await responseStream.ReadAsync(buffer, 0, buffer.Length, cancellationToken)) > 0)
            {
                await fileStream.WriteAsync(buffer, 0, bytesRead, cancellationToken);
                totalBytesRead += bytesRead;

                if (progress != null && contentLength.HasValue)
                {
                    double percentage = ((double)totalBytesRead / contentLength.Value) * 100;
                    progress.Report(percentage);
                }
            }

            return filePath; // Return the path to the downloaded file
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (HttpRequestException ex)
        {
             _logger.LogError($"HTTP request to {url} failed with { ex.StatusCode}: {ex.Message}");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, $"Failed to download file from {url}:");
            return null;
        }
    }

    /// <inheritdoc/>
    public async Task<HttpResponseMessage?> PostAsync(string url, HttpContent? content, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _httpClient.PostAsync(url, content, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (HttpRequestException ex)
        {
             _logger.LogError($"HTTP request to {url} failed with { ex.StatusCode}: {ex.Message}");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, $"Failed to make POST request to {url}:");
            return null;
        }
    }

    /// <inheritdoc/>
    public async Task<HttpResponseMessage?> PostJsonAsync<T>(string url, T value, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _httpClient.PostAsJsonAsync(url, value, cancellationToken: cancellationToken);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (HttpRequestException ex)
        {
             _logger.LogError($"HTTP request to {url} failed with { ex.StatusCode}: {ex.Message}");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, $"Failed to make POST request to {url}:");
            return null;
        }
    }
}