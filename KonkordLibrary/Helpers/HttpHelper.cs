using Newtonsoft.Json;
using System.IO;
using System.Net.Http;
using System.Net.Http.Json;

namespace KonkordLibrary.Helpers
{
    public static class HttpHelper
    {
        private static HttpClient _httpClient = new HttpClient();
        /// <summary>
        /// Gets an instance of HttpClient.
        /// </summary>
        /// <returns>
        /// An instance of HttpClient.
        /// </returns>
        public static HttpClient GetHttpClient()
        {
            return _httpClient;
        }

        /// <summary>
        /// Sends a GET request to the specified URI.
        /// </summary>
        /// <param name="request">The URI of the resource to request.</param>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous operation. The task result contains the HTTP response message, or null if the request fails.
        /// </returns>
        public static async Task<HttpResponseMessage?> GetAsync(string request)
        {
            try
            {
                return await _httpClient.GetAsync(request);
            }
            catch (Exception ex)
            {
                NotificationHelper.SendErrorMsg(ex.ToString(), "HTTP Error");
                return null;
            }
        }

        /// <summary>
        /// Sends a GET request to the specified URI and returns the response body as a byte array.
        /// </summary>
        /// <param name="request">The URI of the resource to request.</param>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous operation. The task result contains the response body as a byte array, or null if the request fails.
        /// </returns>
        public static async Task<byte[]?> GetByteArrayAsync(string request)
        {
            try
            {
                return await _httpClient.GetByteArrayAsync(request);
            }
            catch (Exception ex)
            {
                NotificationHelper.SendErrorMsg(ex.ToString(), "HTTP Error");
                return null;
            }
        }

        /// <summary>
        /// Sends a GET request to the specified URL and returns the response body as a byte array.
        /// </summary>
        /// <param name="url">The URL of the resource to request.</param>
        /// <param name="progress">Optional: An instance of <see cref="IProgress{T}"/> to report download progress.</param>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous operation. The task result contains the response body as a byte array, or null if the request fails.
        /// </returns>
        public static async Task<byte[]?> GetByteArrayAsync(string url, IProgress<double>? progress = null)
        {
            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
                response.EnsureSuccessStatusCode();

                long? contentLength = response.Content.Headers.ContentLength;

                using (Stream responseStream = await response.Content.ReadAsStreamAsync())
                {
                    using (var memoryStream = new MemoryStream())
                    {
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
                }
            }
            catch (Exception ex)
            {
                NotificationHelper.SendErrorMsg(ex.ToString(), "HTTP Error");
                return null;
            }
        }

        /// <summary>
        /// Sends a GET request to the specified URI and returns the response body as a string.
        /// </summary>
        /// <param name="request">The URI of the resource to request.</param>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous operation. The task result contains the response body as a string, or null if the request fails.
        /// </returns>
        public static async Task<string?> GetStringAsync(string request)
        {
            try
            {
                return await _httpClient.GetStringAsync(request);
            }
            catch (Exception ex)
            {
                NotificationHelper.SendErrorMsg(ex.ToString(), "HTTP Error");
                return null;
            }
        }

        /// <summary>
        /// Sends a GET request to the specified URL and returns the response body as a string.
        /// </summary>
        /// <param name="url">The URL of the resource to request.</param>
        /// <param name="progress">Optional: An instance of <see cref="IProgress{T}"/> to report download progress.</param>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous operation. The task result contains the response body as a string, or null if the request fails.
        /// </returns>
        public static async Task<string?> GetStringAsync(string url, IProgress<double>? progress = null)
        {
            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
                response.EnsureSuccessStatusCode();

                long? contentLength = response.Content.Headers.ContentLength;

                using (Stream responseStream = await response.Content.ReadAsStreamAsync())
                {
                    using (var memoryStream = new MemoryStream())
                    {
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
                }
            }
            catch (Exception ex)
            {
                NotificationHelper.SendErrorMsg(ex.ToString(), "HTTP Error");
                return null;
            }
        }

        /// <summary>
        /// Sends a GET request to the specified URI and returns the response body as a stream.
        /// </summary>
        /// <param name="request">The URI of the resource to request.</param>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous operation. The task result contains the response body as a stream, or null if the request fails.
        /// </returns>
        public static async Task<Stream?> GetStreamAsync(string request)
        {
            try
            {
                return await _httpClient.GetStreamAsync(request);
            }
            catch (Exception ex)
            {
                NotificationHelper.SendErrorMsg(ex.ToString(), "HTTP Error");
                return null;
            }
        }

        /// <summary>
        /// Sends a GET request to the specified URI and deserializes the response body as an object of type T.
        /// </summary>
        /// <typeparam name="T">The type of object to deserialize the response body to.</typeparam>
        /// <param name="request">The URI of the resource to request.</param>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous operation. The task result contains the deserialized object of type T, or null if the request fails.
        /// </returns>
        public static async Task<T?> GetObjectFromJsonAsync<T>(string request)
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<T>(request);
            }
            catch (Exception ex)
            {
                NotificationHelper.SendErrorMsg(ex.ToString(), "HTTP Error");
                return default;
            }
        }

        /// <summary>
        /// Sends a GET request to the specified URL and deserializes the response body as an object of type T.
        /// </summary>
        /// <typeparam name="T">The type of object to deserialize the response body to.</typeparam>
        /// <param name="url">The URL of the resource to request.</param>
        /// <param name="progress">Optional: An instance of <see cref="IProgress{T}"/> to report download progress.</param>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous operation. The task result contains the deserialized object of type T, or null if the request fails.
        /// </returns>
        public static async Task<T?> GetObjectFromJsonAsync<T>(string url, IProgress<double>? progress = null)
        {
            string? rawContent = await GetStringAsync(url, progress);
            if (rawContent == null)
                return default;
            return JsonConvert.DeserializeObject<T>(rawContent);
        }

        /// <summary>
        /// Sends a POST request to the specified URI with the provided content.
        /// </summary>
        /// <param name="request">The URI of the resource to request.</param>
        /// <param name="content">Optional: The content to send with the request.</param>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous operation. The task result contains the HTTP response message, or null if the request fails.
        /// </returns>
        public static async Task<HttpResponseMessage?> PostAsync(string request, HttpContent? content)
        {
            try
            {
                return await _httpClient.PostAsync(request, content);
            }
            catch (Exception ex)
            {
                NotificationHelper.SendErrorMsg(ex.ToString(), "HTTP Error");
                return null;
            }
        }

        /// <summary>
        /// Sends a POST request to the specified URI with the provided object serialized as JSON in the request body.
        /// </summary>
        /// <typeparam name="T">The type of the object to serialize as JSON.</typeparam>
        /// <param name="request">The URI of the resource to request.</param>
        /// <param name="value">The object to serialize as JSON and send with the request.</param>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous operation. The task result contains the HTTP response message, or null if the request fails.
        /// </returns>
        public static async Task<HttpResponseMessage?> PostJsonAsync<T>(string request, T value)
        {
            try
            {
                return await _httpClient.PostAsJsonAsync<T>(request, value);
            }
            catch (Exception ex)
            {
                NotificationHelper.SendErrorMsg(ex.ToString(), "HTTP Error");
                return null;
            }
        }
    }
}
