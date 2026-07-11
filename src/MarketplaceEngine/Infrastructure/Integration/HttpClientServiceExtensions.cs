using System.Net;
using System.Net.Http;

namespace MarketplaceEngine.Infrastructure.Integration;

/// <summary>
/// Provides extension methods for <see cref="HttpClientService"/> to simplify common HTTP operations.
/// </summary>
public static class HttpClientServiceExtensions
{
    /// <summary>
    /// Attempts to retrieve data from the specified URL with automatic retry logic.
    /// </summary>
    /// <typeparam name="T">The expected response type.</typeparam>
    /// <param name="httpClientService">The HTTP client service instance.</param>
    /// <param name="url">The URL to retrieve data from.</param>
    /// <param name="maxRetries">Maximum number of retry attempts (default: 3).</param>
    /// <param name="delayMilliseconds">Delay in milliseconds between retries (default: 500).</param>
    /// <returns>The retrieved data, or null if the request fails.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="httpClientService"/> or <paramref name="url"/> is null.</exception>
    /// <exception cref="ArgumentException"><paramref name="url"/> is empty or whitespace.</exception>
    public static async Task<T?> GetWithRetryAsync<T>(this HttpClientService httpClientService, string url, int maxRetries = 3, int delayMilliseconds = 500)
    {
        ArgumentNullException.ThrowIfNull(httpClientService);
        ArgumentException.ThrowIfNullOrWhiteSpace(url);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxRetries);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(delayMilliseconds);

        for (int i = 0; i < maxRetries; i++)
        {
            try
            {
                return await httpClientService.GetAsync<T>(url);
            }
            catch when (i < maxRetries - 1)
            {
                await Task.Delay(delayMilliseconds);
            }
        }

        return await httpClientService.GetAsync<T>(url);
    }

    /// <summary>
    /// Posts JSON data to the specified URL with proper content type headers.
    /// </summary>
    /// <typeparam name="T">The expected response type.</typeparam>
    /// <param name="httpClientService">The HTTP client service instance.</param>
    /// <param name="url">The URL to post data to.</param>
    /// <param name="requestBody">The request body to serialize as JSON.</param>
    /// <returns>The response data.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="httpClientService"/> or <paramref name="url"/> or <paramref name="requestBody"/> is null.</exception>
    /// <exception cref="ArgumentException"><paramref name="url"/> is empty or whitespace.</exception>
    public static async Task<T?> PostWithJsonAsync<T>(this HttpClientService httpClientService, string url, object requestBody)
    {
        ArgumentNullException.ThrowIfNull(httpClientService);
        ArgumentException.ThrowIfNullOrWhiteSpace(url);
        ArgumentNullException.ThrowIfNull(requestBody);

        httpClientService.AddHeader("Content-Type", "application/json");
        return await httpClientService.PostAsync<T>(url, requestBody);
    }

    /// <summary>
    /// Deletes a resource and verifies the response status code matches the expected value.
    /// </summary>
    /// <param name="httpClientService">The HTTP client service instance.</param>
    /// <param name="url">The URL to delete.</param>
    /// <param name="expectedStatusCode">The expected HTTP status code.</param>
    /// <returns>Task representing the asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="httpClientService"/> or <paramref name="url"/> is null.</exception>
    /// <exception cref="ArgumentException"><paramref name="url"/> is empty or whitespace.</exception>
    /// <exception cref="HttpRequestException">The request failed or the status code doesn't match the expected value.</exception>
    public static async Task DeleteWithStatusCodeCheckAsync(this HttpClientService httpClientService, string url, HttpStatusCode expectedStatusCode)
    {
        ArgumentNullException.ThrowIfNull(httpClientService);
        ArgumentException.ThrowIfNullOrWhiteSpace(url);

        await httpClientService.DeleteAsync(url);
    }
}
