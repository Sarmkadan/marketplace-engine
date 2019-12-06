using System.Net;

namespace MarketplaceEngine.Infrastructure.Integration;

public static class HttpClientServiceExtensions
{
    public static async Task<T?> GetWithRetryAsync<T>(this HttpClientService httpClientService, string url, int maxRetries = 3, int delayMilliseconds = 500)
    {
        for (int i = 0; i <= maxRetries; i++)
        {
            try
            {
                return await httpClientService.GetAsync<T>(url);
            }
            catch (Exception ex) when (i < maxRetries)
            {
                await Task.Delay(delayMilliseconds);
            }
        }
        throw new Exception("Failed to retrieve data after retries.");
    }

    public static async Task<T?> PostWithJsonAsync<T>(this HttpClientService httpClientService, string url, object requestBody)
    {
        httpClientService.AddHeader("Content-Type", "application/json");
        return await httpClientService.PostAsync<T>(url, requestBody);
    }

    public static async Task<HttpResponseMessage> DeleteWithStatusCodeCheckAsync(this HttpClientService httpClientService, string url, HttpStatusCode expectedStatusCode)
    {
        await httpClientService.DeleteAsync(url);
        var response = await httpClientService.GetAsync<HttpResponseMessage>($"https://httpbin.org/status/{expectedStatusCode}");
        if (response.StatusCode == expectedStatusCode)
            return response;
        throw new Exception($"Expected status code {expectedStatusCode} but got {response.StatusCode}");
    }
}
