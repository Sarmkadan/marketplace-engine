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
    private const double BackoffBaseSeconds = 2;
    private const string JsonContentType = "application/json";
    private const string MethodGet = "GET";
    private const string MethodPost = "POST";
    private const string MethodPut = "PUT";
    private const string MethodDelete = "DELETE";

    /// <summary>
    /// Initializes a new instance of the <see cref="HttpClientService"/> class.
    /// Sets the underlying client timeout to 30 seconds.
    /// </summary>
    /// <param name="httpClient">The HTTP client used to send requests.</param>
    /// <param name="logger">The logger used to record request attempts and errors.</param>
    public HttpClientService(HttpClient httpClient, ILogger<HttpClientService> logger)
    {
        ArgumentNullException.ThrowIfNull(httpClient);
        ArgumentNullException.ThrowIfNull(logger);

        _httpClient = httpClient;
        _logger = logger;
        _httpClient.Timeout = TimeSpan.FromSeconds(TimeoutSeconds);
    }

    /// <summary>
    /// Makes a GET request with automatic retries.
    /// </summary>
    /// <typeparam name="T">The type to deserialize the response body into.</typeparam>
    /// <param name="url">The request URL.</param>
    /// <returns>The deserialized response body, or <c>null</c> if the JSON deserializes to null.</returns>
    public async Task<T?> GetAsync<T>(string url)
    {
        ArgumentNullException.ThrowIfNull(url);

        return await ExecuteWithRetryAsync(async () =>
        {
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(content);
        }, $"{MethodGet} {url}");
    }

    /// <summary>
    /// Makes a POST request with optional request body.
    /// </summary>
    /// <typeparam name="T">The type to deserialize the response body into.</typeparam>
    /// <param name="url">The request URL.</param>
    /// <param name="data">The object to serialize as JSON into the request body, or <c>null</c> for no body.</param>
    /// <returns>The deserialized response body, or <c>null</c> if the JSON deserializes to null.</returns>
    public async Task<T?> PostAsync<T>(string url, object? data = null)
    {
        ArgumentNullException.ThrowIfNull(url);

        return await ExecuteWithRetryAsync(async () =>
        {
            var content = data is not null
                ? new StringContent(JsonSerializer.Serialize(data), System.Text.Encoding.UTF8, JsonContentType)
                : null;

            var response = await _httpClient.PostAsync(url, content);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(responseContent);
        }, $"{MethodPost} {url}");
    }

    /// <summary>
    /// Makes a PUT request.
    /// </summary>
    /// <typeparam name="T">The type to deserialize the response body into.</typeparam>
    /// <param name="url">The request URL.</param>
    /// <param name="data">The object to serialize as JSON into the request body.</param>
    /// <returns>The deserialized response body, or <c>null</c> if the JSON deserializes to null.</returns>
    public async Task<T?> PutAsync<T>(string url, object data)
    {
        ArgumentNullException.ThrowIfNull(url);
        ArgumentNullException.ThrowIfNull(data);

        return await ExecuteWithRetryAsync(async () =>
        {
            var content = new StringContent(JsonSerializer.Serialize(data), System.Text.Encoding.UTF8, JsonContentType);
            var response = await _httpClient.PutAsync(url, content);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(responseContent);
        }, $"{MethodPut} {url}");
    }

    /// <summary>
    /// Makes a DELETE request.
    /// </summary>
    /// <param name="url">The request URL.</param>
    public async Task DeleteAsync(string url)
    {
        ArgumentNullException.ThrowIfNull(url);

        await ExecuteWithRetryAsync(async () =>
        {
            var response = await _httpClient.DeleteAsync(url);
            response.EnsureSuccessStatusCode();
            return true;
        }, $"{MethodDelete} {url}");
    }

    /// <summary>
    /// Sets authorization header for API requests.
    /// </summary>
    /// <param name="scheme">The authentication scheme (for example, <c>Bearer</c>).</param>
    /// <param name="parameter">The credentials for the scheme (for example, a token).</param>
    public void SetAuthorizationHeader(string scheme, string parameter)
    {
        ArgumentNullException.ThrowIfNull(scheme);
        ArgumentNullException.ThrowIfNull(parameter);

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(scheme, parameter);
    }

    /// <summary>
    /// Adds custom header to all requests.
    /// </summary>
    /// <param name="name">The header name.</param>
    /// <param name="value">The header value.</param>
    public void AddHeader(string name, string value)
    {
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(value);

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

                var result = await operation();

                if (attempt > 1)
                {
                    _logger.LogInformation("{Operation} succeeded after {Attempt} attempts", operationName, attempt);
                }

                return result;
            }
            catch (HttpRequestException ex) when (attempt < MaxRetries)
            {
                _logger.LogWarning(ex, "Transient error in {Operation}, retrying...", operationName);
                await Task.Delay(TimeSpan.FromSeconds(Math.Pow(BackoffBaseSeconds, attempt))); // Exponential backoff
            }
            catch (TimeoutException ex) when (attempt < MaxRetries)
            {
                _logger.LogWarning(ex, "Timeout in {Operation}, retrying...", operationName);
                await Task.Delay(TimeSpan.FromSeconds(Math.Pow(BackoffBaseSeconds, attempt)));
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
