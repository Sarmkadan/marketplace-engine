#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Net.Http.Headers;
using System.Text.Json;

namespace MarketplaceEngine.Infrastructure.Integration;

/// <summary>
/// Wrapper around HttpClient for calling external APIs.
/// Adds retry logic, timeout handling, and logging.
/// </summary>
public class HttpClientService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<HttpClientService> _logger;

    private const int MaxRetries = 3;
    private const int TimeoutSeconds = 30;

    public HttpClientService(HttpClient httpClient, ILogger<HttpClientService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _httpClient.Timeout = TimeSpan.FromSeconds(TimeoutSeconds);
    }

    /// <summary>
    /// Makes a GET request with automatic retries.
    /// </summary>
    public async Task<T?> GetAsync<T>(string url)
    {
        return await ExecuteWithRetryAsync(async () =>
        {
            var response = await _httpClient.GetAsync(url).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            return JsonSerializer.Deserialize<T>(content);
        }, $"GET {url}");
    }

    /// <summary>
    /// Makes a POST request with optional request body.
    /// </summary>
    public async Task<T?> PostAsync<T>(string url, object? data = null)
    {
        return await ExecuteWithRetryAsync(async () =>
        {
            var content = data is not null
                ? new StringContent(JsonSerializer.Serialize(data), System.Text.Encoding.UTF8, "application/json")
                : null;

            var response = await _httpClient.PostAsync(url, content).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            return JsonSerializer.Deserialize<T>(responseContent);
        }, $"POST {url}");
    }

    /// <summary>
    /// Makes a PUT request.
    /// </summary>
    public async Task<T?> PutAsync<T>(string url, object data)
    {
        return await ExecuteWithRetryAsync(async () =>
        {
            var content = new StringContent(JsonSerializer.Serialize(data), System.Text.Encoding.UTF8, "application/json");
            var response = await _httpClient.PutAsync(url, content).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            return JsonSerializer.Deserialize<T>(responseContent);
        }, $"PUT {url}");
    }

    /// <summary>
    /// Makes a DELETE request.
    /// </summary>
    public async Task DeleteAsync(string url)
    {
        await ExecuteWithRetryAsync(async () =>
        {
            var response = await _httpClient.DeleteAsync(url).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            return true;
        }, $"DELETE {url}");
    }

    /// <summary>
    /// Sets authorization header for API requests.
    /// </summary>
    public void SetAuthorizationHeader(string scheme, string parameter)
    {
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(scheme, parameter);
    }

    /// <summary>
    /// Adds custom header to all requests.
    /// </summary>
    public void AddHeader(string name, string value)
    {
        _httpClient.DefaultRequestHeaders.Add(name, value);
    }

    private async Task<T?> ExecuteWithRetryAsync<T>(Func<Task<T>> operation, string operationName)
    {
        int attempt = 0;

        while (attempt < MaxRetries)
        {
            try
            {
                attempt++;
                _logger.LogInformation("Executing {Operation} (attempt {Attempt}/{MaxRetries})", operationName, attempt, MaxRetries);

                var result = await operation().ConfigureAwait(false);

                if (attempt > 1)
                {
                    _logger.LogInformation("{Operation} succeeded after {Attempt} attempts", operationName, attempt);
                }

                return result;
            }
            catch (HttpRequestException ex) when (attempt < MaxRetries)
            {
                _logger.LogWarning(ex, "Transient error in {Operation}, retrying...", operationName);
                await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, attempt))).ConfigureAwait(false); // Exponential backoff
            }
            catch (TimeoutException ex) when (attempt < MaxRetries)
            {
                _logger.LogWarning(ex, "Timeout in {Operation}, retrying...", operationName);
                await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, attempt))).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in {Operation} (attempt {Attempt})", operationName, attempt);
                throw;
            }
        }

        throw new InvalidOperationException($"Failed to execute {operationName} after {MaxRetries} retries");
    }
}
