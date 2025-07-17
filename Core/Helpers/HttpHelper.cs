using System.Net.Http.Json;
using Tavstal.KonkordLauncher.Core.Models;

namespace Tavstal.KonkordLauncher.Core.Helpers;

/// <summary>
/// Provides helper methods for HTTP operations, including GET and POST requests, 
/// as well as methods for handling progress and deserializing JSON responses.
/// </summary>
public static class HttpHelper
{
    private static readonly HttpClient _httpClient = CreateHttpClient();
    private static readonly CoreLogger _logger = CoreLogger.WithModuleType(typeof(HttpHelper));

    /// <summary>
    /// Creates and configures an instance of <see cref="HttpClient"/> with default headers.
    /// </summary>
    /// <returns>A configured <see cref="HttpClient"/> instance.</returns>
    private static HttpClient CreateHttpClient()
    {
        var client = new HttpClient();
        client.DefaultRequestHeaders.UserAgent.ParseAdd("KonkordLauncher/2.0.0 (+https://tavstaldev.github.io)");
        return client;
    }

    /// <summary>
    /// Retrieves the shared <see cref="HttpClient"/> instance.
    /// </summary>
    /// <returns>The shared <see cref="HttpClient"/> instance.</returns>
    public static HttpClient GetHttpClient()
    {
        return _httpClient;
    }

    /// <summary>
    /// Sends a GET request to the specified URL.
    /// </summary>
    /// <param name="request">The URL to send the GET request to.</param>
    /// <returns>The HTTP response, or null if an error occurs.</returns>
    public static async Task<HttpResponseMessage?> GetAsync(string request)
    {
        try
        {
            return await _httpClient.GetAsync(request);
        }
        catch (Exception ex)
        {
            _logger.Exc("Error while making GET request:");
            _logger.Error(ex.ToString());
            return null;
        }
    }

    /// <summary>
    /// Sends a GET request to retrieve a byte array from the specified URL.
    /// </summary>
    /// <param name="request">The URL to send the GET request to.</param>
    /// <returns>The byte array, or null if an error occurs.</returns>
    public static async Task<byte[]?> GetByteArrayAsync(string request)
    {
        try
        {
            return await _httpClient.GetByteArrayAsync(request);
        }
        catch (Exception ex)
        {
            _logger.Exc("Error while making GET request for byte array:");
            _logger.Error(ex.ToString());
            return null;
        }
    }

    /// <summary>
    /// Sends a GET request to retrieve a byte array from the specified URL, with progress reporting.
    /// </summary>
    /// <param name="url">The URL to send the GET request to.</param>
    /// <param name="progress">An optional progress reporter.</param>
    /// <returns>The byte array, or null if an error occurs.</returns>
    public static async Task<byte[]?> GetByteArrayAsync(string url, IProgress<double>? progress)
    {
        try
        {
            HttpResponseMessage response = await _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();

            long? contentLength = response.Content.Headers.ContentLength;

            await using Stream responseStream = await response.Content.ReadAsStreamAsync();
            using var memoryStream = new MemoryStream();
            byte[] buffer = new byte[4096];
            int bytesRead;
            long totalBytesRead = 0;

            while ((bytesRead = await responseStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
            {
                await memoryStream.WriteAsync(buffer, 0, bytesRead);
                totalBytesRead += bytesRead;

                if (progress != null && contentLength.HasValue)
                {
                    double percentage = ((double)totalBytesRead / contentLength.Value) * 100;
                    progress.Report(percentage);
                }
            }

            return memoryStream.ToArray();
        }
        catch (Exception ex)
        {
            _logger.Exc("Error while making GET request for byte array with progress:");
            _logger.Error(ex.ToString());
            return null;
        }
    }

    /// <summary>
    /// Sends a GET request to retrieve a string from the specified URL.
    /// </summary>
    /// <param name="request">The URL to send the GET request to.</param>
    /// <returns>The string response, or null if an error occurs.</returns>
    public static async Task<string?> GetStringAsync(string request)
    {
        try
        {
            return await _httpClient.GetStringAsync(request);
        }
        catch (Exception ex)
        {
            _logger.Exc("Error while making GET request for string:");
            _logger.Error(ex.ToString());
            return null;
        }
    }

    /// <summary>
    /// Sends a GET request to retrieve a string from the specified URL, with progress reporting.
    /// </summary>
    /// <param name="url">The URL to send the GET request to.</param>
    /// <param name="progress">An optional progress reporter.</param>
    /// <returns>The string response, or null if an error occurs.</returns>
    public static async Task<string?> GetStringAsync(string url, IProgress<double>? progress)
    {
        try
        {
            HttpResponseMessage response = await _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();

            long? contentLength = response.Content.Headers.ContentLength;

            await using Stream responseStream = await response.Content.ReadAsStreamAsync();
            using var memoryStream = new MemoryStream();
            byte[] buffer = new byte[4096];
            int bytesRead;
            long totalBytesRead = 0;

            while ((bytesRead = await responseStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
            {
                await memoryStream.WriteAsync(buffer, 0, bytesRead);
                totalBytesRead += bytesRead;

                if (progress != null && contentLength.HasValue)
                {
                    double percentage = ((double)totalBytesRead / contentLength.Value) * 100;
                    progress.Report(percentage);
                }
            }

            return System.Text.Encoding.UTF8.GetString(memoryStream.ToArray());
        }
        catch (Exception ex)
        {
            _logger.Exc("Error while making GET request for string with progress:");
            _logger.Error(ex.ToString());
            return null;
        }
    }

    /// <summary>
    /// Sends a GET request to retrieve a stream from the specified URL.
    /// </summary>
    /// <param name="request">The URL to send the GET request to.</param>
    /// <returns>The stream response, or null if an error occurs.</returns>
    public static async Task<Stream?> GetStreamAsync(string request)
    {
        try
        {
            return await _httpClient.GetStreamAsync(request);
        }
        catch (Exception ex)
        {
            _logger.Exc("Error while making GET request for stream:");
            _logger.Error(ex.ToString());
            return null;
        }
    }

    /// <summary>
    /// Sends a GET request and deserializes the JSON response into an object of type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type of the object to deserialize.</typeparam>
    /// <param name="request">The URL to send the GET request to.</param>
    /// <returns>The deserialized object, or default if an error occurs.</returns>
    public static async Task<T?> GetObjectFromJsonAsync<T>(string request)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<T>(request);
        }
        catch (Exception ex)
        {
            _logger.Exc("Error while deserializing JSON from GET request:");
            _logger.Error(ex.ToString());
            return default;
        }
    }

    /// <summary>
    /// Sends a POST request to the specified URL with the provided content.
    /// </summary>
    /// <param name="request">The URL to send the POST request to.</param>
    /// <param name="content">The content to include in the POST request.</param>
    /// <returns>The HTTP response, or null if an error occurs.</returns>
    public static async Task<HttpResponseMessage?> PostAsync(string request, HttpContent? content)
    {
        try
        {
            return await _httpClient.PostAsync(request, content);
        }
        catch (Exception ex)
        {
            _logger.Exc("Error while making POST request:");
            _logger.Error(ex.ToString());
            return null;
        }
    }

    /// <summary>
    /// Sends a POST request to the specified URL with the provided JSON object.
    /// </summary>
    /// <typeparam name="T">The type of the object to serialize as JSON.</typeparam>
    /// <param name="request">The URL to send the POST request to.</param>
    /// <param name="value">The object to serialize as JSON.</param>
    /// <returns>The HTTP response, or null if an error occurs.</returns>
    public static async Task<HttpResponseMessage?> PostJsonAsync<T>(string request, T value)
    {
        try
        {
            return await _httpClient.PostAsJsonAsync(request, value);
        }
        catch (Exception ex)
        {
            _logger.Exc("Error while making POST request with JSON:");
            _logger.Error(ex.ToString());
            return null;
        }
    }
}