using System.Net;
using System.Net.Http.Json;
using System.Net.Security;
using System.Security.Authentication;
using Tavstal.KonkordLauncher.Core.Helpers.Platform;
using Tavstal.KonkordLauncher.Core.Models;

namespace Tavstal.KonkordLauncher.Core.Helpers.Network;

/// <summary>
/// Provides helper methods for HTTP operations, including GET and POST requests, 
/// as well as methods for handling progress and deserializing JSON responses.
/// </summary>
[Obsolete("This class is deprecated and may be removed in future versions. Please use the new HttpService class instead.")]
public static class HttpHelper
{
    private static readonly HttpClient _httpClient = CreateHttpClient();
    private static CoreLogger? _logger;

    private static CoreLogger? Logger()
    {
        if (_logger != null)
            return _logger;
        _logger = CoreLogger.WithModuleType(typeof(HttpHelper));
        return _logger;
    }

    /// <summary>
    /// Creates and configures an instance of <see cref="HttpClient"/> with default headers.
    /// </summary>
    /// <returns>A configured <see cref="HttpClient"/> instance.</returns>
    public static HttpClient CreateHttpClient()
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

    /// <summary>
    /// Retrieves the shared <see cref="HttpClient"/> instance.
    /// </summary>
    /// <returns>The shared <see cref="HttpClient"/> instance.</returns>
    public static HttpClient GetHttpClient() => _httpClient;

    /// <summary>
    /// Sends a GET request to the specified URL.
    /// </summary>
    /// <param name="request">The URL to send the GET request to.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>The HTTP response, or null if an error occurs.</returns>
    public static async Task<HttpResponseMessage?> GetAsync(string request, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _httpClient.GetAsync(request, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            Logger()?.Exc("Error while making GET request:");
            Logger()?.Error(ex.Message);
            return null;
        }
    }

    /// <summary>
    /// Sends a GET request to retrieve a byte array from the specified URL.
    /// </summary>
    /// <param name="request">The URL to send the GET request to.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>The byte array, or null if an error occurs.</returns>
    [Obsolete]
    public static async Task<byte[]?> GetByteArrayAsync(string request, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _httpClient.GetByteArrayAsync(request, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            Logger()?.Exc("Error while making GET request for byte array:");
            Logger()?.Error(ex.Message);
            return null;
        }
    }
    
    /// <summary>
    /// Downloads a file from the specified URL and saves it to the given file path, 
    /// while reporting progress if a progress reporter is provided.
    /// </summary>
    /// <param name="url">The URL of the file to download.</param>
    /// <param name="filePath">The local file path where the downloaded file will be saved.</param>
    /// <param name="progress">An optional progress reporter to track the download progress as a percentage.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>
    /// The file path of the downloaded file if successful, or null if an error occurs.
    /// </returns>
    public static async Task<string?> DownloadFileAsync(string url, string filePath, IProgress<double>? progress, CancellationToken cancellationToken = default)
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
        catch (Exception ex)
        {
            Logger()?.Exc("Error while downloading file with progress:");
            Logger()?.Exc($"Url: {url}");
            Logger()?.Exc($"File path: {filePath}");
            Logger()?.Error(ex.Message);
            return null;
        }
    }

    /// <summary>
    /// Sends a GET request to retrieve a string from the specified URL.
    /// </summary>
    /// <param name="request">The URL to send the GET request to.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>The string response, or null if an error occurs.</returns>
    public static async Task<string?> GetStringAsync(string request, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _httpClient.GetStringAsync(request, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            Logger()?.Exc("Error while making GET request for string:");
            Logger()?.Error(ex.Message);
            return null;
        }
    }

    /// <summary>
    /// Sends a GET request to retrieve a string from the specified URL, with progress reporting.
    /// </summary>
    /// <param name="url">The URL to send the GET request to.</param>
    /// <param name="progress">An optional progress reporter.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>The string response, or null if an error occurs.</returns>
    public static async Task<string?> GetStringAsync(string url, IProgress<double>? progress, CancellationToken cancellationToken = default)
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
        catch (Exception ex)
        {
            Logger()?.Exc("Error while making GET request for string with progress:");
            Logger()?.Exc($"Url: {url}");
            Logger()?.Error(ex.Message);
            return null;
        }
    }

    /// <summary>
    /// Sends a GET request to retrieve a stream from the specified URL.
    /// </summary>
    /// <param name="request">The URL to send the GET request to.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>The stream response, or null if an error occurs.</returns>
    public static async Task<Stream?> GetStreamAsync(string request, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _httpClient.GetStreamAsync(request, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            Logger()?.Exc("Error while making GET request for stream:");
            Logger()?.Error(ex.Message);
            return null;
        }
    }

    /// <summary>
    /// Sends a GET request and deserializes the JSON response into an object of type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type of the object to deserialize.</typeparam>
    /// <param name="request">The URL to send the GET request to.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>The deserialized object, or default if an error occurs.</returns>
    public static async Task<T?> GetObjectFromJsonAsync<T>(string request, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<T>(request, cancellationToken: cancellationToken);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            Logger()?.Exc("Error while deserializing JSON from GET request:");
            Logger()?.Error(ex.Message);
            return default;
        }
    }

    /// <summary>
    /// Sends a POST request to the specified URL with the provided content.
    /// </summary>
    /// <param name="request">The URL to send the POST request to.</param>
    /// <param name="content">The content to include in the POST request.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>The HTTP response, or null if an error occurs.</returns>
    public static async Task<HttpResponseMessage?> PostAsync(string request, HttpContent? content, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _httpClient.PostAsync(request, content, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            Logger()?.Exc("Error while making POST request:");
            Logger()?.Error(ex.Message);
            return null;
        }
    }

    /// <summary>
    /// Sends a POST request to the specified URL with the provided JSON object.
    /// </summary>
    /// <typeparam name="T">The type of the object to serialize as JSON.</typeparam>
    /// <param name="request">The URL to send the POST request to.</param>
    /// <param name="value">The object to serialize as JSON.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>The HTTP response, or null if an error occurs.</returns>
    public static async Task<HttpResponseMessage?> PostJsonAsync<T>(string request, T value, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _httpClient.PostAsJsonAsync(request, value, cancellationToken: cancellationToken);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            Logger()?.Exc("Error while making POST request with JSON:");
            Logger()?.Error(ex.Message);
            return null;
        }
    }
}