// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace MarketplaceEngine.Infrastructure.Integration;

/// <summary>
/// Contract for external listing data providers.
/// Allows integration with dropshipping APIs, supplier catalogs, etc.
/// </summary>
public interface IListingProvider
{
    Task<List<ExternalListingDto>> GetListingsAsync(string category, int page = 1);
    Task<ExternalListingDto?> GetListingAsync(string externalId);
    Task<bool> IsListingAvailableAsync(string externalId);
}

/// <summary>
/// DTO for external listing data from third-party providers.
/// </summary>
public class ExternalListingDto
{
    public string ExternalId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Currency { get; set; } = "USD";
    public int StockQuantity { get; set; }
    public string Category { get; set; } = string.Empty;
    public List<string> ImageUrls { get; set; } = new();
    public Dictionary<string, string> Attributes { get; set; } = new();
}

/// <summary>
/// Implementation of external listing provider for a hypothetical dropshipping API.
/// In production, implement concrete providers for your actual suppliers.
/// </summary>
public class DropshipProviderClient : IListingProvider
{
    private readonly HttpClientService _httpClient;
    private readonly ILogger<DropshipProviderClient> _logger;
    private readonly string _apiBaseUrl;
    private readonly string _apiKey;

    public DropshipProviderClient(
        HttpClientService httpClient,
        ILogger<DropshipProviderClient> logger,
        string apiBaseUrl,
        string apiKey)
    {
        _httpClient = httpClient;
        _logger = logger;
        _apiBaseUrl = apiBaseUrl;
        _apiKey = apiKey;

        // Configure authentication
        _httpClient.SetAuthorizationHeader("Bearer", apiKey);
    }

    /// <summary>
    /// Fetches listings from external provider with pagination.
    /// </summary>
    public async Task<List<ExternalListingDto>> GetListingsAsync(string category, int page = 1)
    {
        try
        {
            _logger.LogInformation("Fetching listings from external provider: category={Category}, page={Page}", category, page);

            var url = $"{_apiBaseUrl}/listings?category={category}&page={page}";
            var response = await _httpClient.GetAsync<GetListingsResponse>(url);

            return response?.Items ?? new();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching listings from external provider");
            return new();
        }
    }

    /// <summary>
    /// Fetches a specific listing from external provider.
    /// </summary>
    public async Task<ExternalListingDto?> GetListingAsync(string externalId)
    {
        try
        {
            _logger.LogInformation("Fetching listing from external provider: externalId={ExternalId}", externalId);

            var url = $"{_apiBaseUrl}/listings/{externalId}";
            return await _httpClient.GetAsync<ExternalListingDto>(url);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching listing {ExternalId} from external provider", externalId);
            return null;
        }
    }

    /// <summary>
    /// Checks if a listing is still available at external provider.
    /// </summary>
    public async Task<bool> IsListingAvailableAsync(string externalId)
    {
        try
        {
            var listing = await GetListingAsync(externalId);
            return listing != null && listing.StockQuantity > 0;
        }
        catch
        {
            return false;
        }
    }

    private class GetListingsResponse
    {
        [System.Text.Json.Serialization.JsonPropertyName("items")]
        public List<ExternalListingDto> Items { get; set; } = new();

        [System.Text.Json.Serialization.JsonPropertyName("total")]
        public int Total { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("page")]
        public int Page { get; set; }
    }
}

/// <summary>
/// Service for syncing external listings into the marketplace.
/// </summary>
public class ExternalListingSyncService
{
    private readonly IListingProvider _provider;
    private readonly ILogger<ExternalListingSyncService> _logger;

    public ExternalListingSyncService(
        IListingProvider provider,
        ILogger<ExternalListingSyncService> logger)
    {
        _provider = provider;
        _logger = logger;
    }

    /// <summary>
    /// Syncs external listings into marketplace listings.
    /// Maps external format to internal domain model.
    /// </summary>
    public async Task<List<Domain.Models.Listing>> SyncListingsAsync(string category, Guid sellerId)
    {
        _logger.LogInformation("Syncing external listings for category: {Category}", category);

        var externalListings = await _provider.GetListingsAsync(category);
        var results = new List<Domain.Models.Listing>();

        foreach (var external in externalListings)
        {
            // Map external listing to domain model
            var listing = new Domain.Models.Listing
            {
                Id = Guid.NewGuid(),
                Title = external.Title,
                Description = external.Description,
                SellerId = sellerId,
                CategoryId = Guid.NewGuid(), // In production, map to actual category
                CreatedAt = DateTime.UtcNow
            };

            results.Add(listing);
        }

        _logger.LogInformation("Synced {Count} listings for category {Category}", results.Count, category);
        return results;
    }

    /// <summary>
    /// Updates a synced listing's availability from external provider.
    /// </summary>
    public async Task UpdateAvailabilityAsync(string externalId, Domain.Models.Listing listing)
    {
        var available = await _provider.IsListingAvailableAsync(externalId);

        if (!available)
        {
            _logger.LogInformation("External listing no longer available: {ExternalId}", externalId);
            // Mark listing as unavailable in marketplace
        }
    }
}
