namespace Tavstal.KonkordLauncher.Core.Services.Abstractions;

/// <summary>
/// Provides HTTP client capabilities for GET/POST operations with support for streaming, progress reporting, and JSON serialization.
/// </summary>
public interface IHttpService
{
    /// <summary>
    /// Creates and configures an <see cref="HttpClient"/> with platform-specific settings and default headers.
    /// </summary>
    /// <returns>A configured <see cref="HttpClient"/> instance.</returns>
    HttpClient CreateHttpClient();
    
    /// <summary>
    /// Performs a GET request and returns the full response message.
    /// </summary>
    /// <param name="url">The request URL.</param>
    /// <param name="cancellationToken">Cancellation token observed during the request.</param>
    /// <returns>
    /// A task that resolves to the <see cref="HttpResponseMessage"/> if successful; otherwise, <see langword="null"/>.
    /// Caller is responsible for disposing the returned response.
    /// </returns>
    Task<HttpResponseMessage?> GetAsync(string url, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Performs a GET request and returns the response content as a string.
    /// </summary>
    /// <param name="url">The request URL.</param>
    /// <param name="cancellationToken">Cancellation token observed during the request.</param>
    /// <returns>
    /// A task that resolves to the response content as a string if successful; otherwise, <see langword="null"/>.
    /// </returns>
    Task<string?> GetStringAsync(string url, CancellationToken cancellationToken = default);

    /// <summary>
    /// Performs a GET request and returns the response content as a string with progress tracking.
    /// </summary>
    /// <param name="url">The request URL.</param>
    /// <param name="progress">
    /// Optional progress reporter that receives percentage complete (0-100) as data is received.
    /// </param>
    /// <param name="cancellationToken">Cancellation token observed during the request.</param>
    /// <returns>
    /// A task that resolves to the response content as a string if successful; otherwise, <see langword="null"/>.
    /// </returns>
    Task<string?> GetStringAsync(string url, IProgress<double>? progress,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Performs a GET request and returns the response content as a byte array.
    /// </summary>
    /// <param name="url">The request URL.</param>
    /// <param name="cancellationToken">Cancellation token observed during the request.</param>
    /// <returns>
    /// A task that resolves to the response content as bytes if successful; otherwise, <see langword="null"/>.
    /// </returns>
    Task<byte[]?> GetByteArrayAsync(string url, CancellationToken cancellationToken = default);

    /// <summary>
    /// Performs a GET request and returns the response content as a stream.
    /// </summary>
    /// <param name="url">The request URL.</param>
    /// <param name="cancellationToken">Cancellation token observed during the request.</param>
    /// <returns>
    /// A task that resolves to the response content stream if successful; otherwise, <see langword="null"/>.
    /// </returns>
    Task<Stream?> GetStreamAsync(string url, CancellationToken cancellationToken = default);

    /// <summary>
    /// Performs a GET request and deserializes the JSON response into the specified type.
    /// </summary>
    /// <typeparam name="T">The type to deserialize the response into.</typeparam>
    /// <param name="url">The request URL.</param>
    /// <param name="cancellationToken">Cancellation token observed during the request.</param>
    /// <returns>
    /// A task that resolves to the deserialized object if successful; otherwise, <see langword="null"/>.
    /// </returns>
    Task<T?> GetObjectFromJsonAsync<T>(string url, CancellationToken cancellationToken = default);

    /// <summary>
    /// Downloads a file from the specified URL and saves it to the target path with progress tracking.
    /// </summary>
    /// <param name="url">The URL of the file to download.</param>
    /// <param name="filePath">The local file path where the downloaded content will be saved.</param>
    /// <param name="progress">
    /// Optional progress reporter that receives percentage complete (0-100) as data is downloaded.
    /// </param>
    /// <param name="cancellationToken">Cancellation token observed during the download.</param>
    /// <returns>
    /// A task that resolves to the file path if the download succeeds; otherwise, <see langword="null"/>.
    /// </returns>
    Task<string?> DownloadFileAsync(string url, string filePath, IProgress<double>? progress,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Performs a POST request with the specified content.
    /// </summary>
    /// <param name="url">The request URL.</param>
    /// <param name="content">Optional HTTP content to send in the request body.</param>
    /// <param name="cancellationToken">Cancellation token observed during the request.</param>
    /// <returns>
    /// A task that resolves to the <see cref="HttpResponseMessage"/> if successful; otherwise, <see langword="null"/>.
    /// Caller is responsible for disposing the returned response.
    /// </returns>
    Task<HttpResponseMessage?> PostAsync(string url, HttpContent? content,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Performs a POST request with the specified object serialized as JSON.
    /// </summary>
    /// <typeparam name="T">The type of the value to serialize.</typeparam>
    /// <param name="url">The request URL.</param>
    /// <param name="value">The object to serialize and send as JSON in the request body.</param>
    /// <param name="cancellationToken">Cancellation token observed during the request.</param>
    /// <returns>
    /// A task that resolves to the <see cref="HttpResponseMessage"/> if successful; otherwise, <see langword="null"/>.
    /// Caller is responsible for disposing the returned response.
    /// </returns>
    Task<HttpResponseMessage?> PostJsonAsync<T>(string url, T value, CancellationToken cancellationToken = default);
}