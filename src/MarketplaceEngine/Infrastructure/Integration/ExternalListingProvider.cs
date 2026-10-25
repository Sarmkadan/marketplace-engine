#nullable enable
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
    /// <summary>
    /// Retrieves one page of listings for a category from the external provider.
    /// </summary>
    /// <param name="category">Category identifier understood by the provider.</param>
    /// <param name="page">1-based page number.</param>
    /// <returns>The listings on the requested page; empty when there are none.</returns>
    Task<List<ExternalListingDto>> GetListingsAsync(string category, int page = 1);

    /// <summary>
    /// Retrieves a single listing by its external identifier.
    /// </summary>
    /// <param name="externalId">Identifier assigned to the listing by the provider.</param>
    /// <returns>The listing, or <c>null</c> if the provider does not return one.</returns>
    Task<ExternalListingDto?> GetListingAsync(string externalId);

    /// <summary>
    /// Determines whether a listing can currently be ordered from the provider.
    /// </summary>
    /// <param name="externalId">Identifier assigned to the listing by the provider.</param>
    /// <returns><c>true</c> if the listing exists and has stock; otherwise <c>false</c>.</returns>
    Task<bool> IsListingAvailableAsync(string externalId);
}

/// <summary>
/// DTO for external listing data from third-party providers.
/// </summary>
public class ExternalListingDto
{
    /// <summary>
    /// Identifier assigned to the listing by the external provider.
    /// </summary>
    public string ExternalId { get; set; } = string.Empty;

    /// <summary>
    /// Listing title.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Full listing description.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Unit price, expressed in <see cref="Currency"/>.
    /// </summary>
    public decimal Price { get; set; }

    /// <summary>
    /// Currency code for <see cref="Price"/>. Defaults to USD.
    /// </summary>
    public string Currency { get; set; } = "USD";

    /// <summary>
    /// Units currently in stock at the provider.
    /// </summary>
    public int StockQuantity { get; set; }

    /// <summary>
    /// Category of the listing as reported by the provider.
    /// </summary>
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// URLs of the listing's product images.
    /// </summary>
    public List<string> ImageUrls { get; set; } = new();

    /// <summary>
    /// Additional provider-specific key/value attributes.
    /// </summary>
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

    /// <summary>
    /// Initializes a new instance of the <see cref="DropshipProviderClient"/> class
    /// and sets the bearer authorization header on the HTTP client.
    /// </summary>
    /// <param name="httpClient">HTTP client used to call the provider API.</param>
    /// <param name="logger">Logger for request and error diagnostics.</param>
    /// <param name="apiBaseUrl">Base URL of the provider API, without a trailing slash.</param>
    /// <param name="apiKey">API key sent to the provider as a bearer token.</param>
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
    /// <param name="category">Category identifier understood by the provider.</param>
    /// <param name="page">1-based page number.</param>
    /// <returns>The listings on the requested page; an empty list if the request fails.</returns>
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
    /// <param name="externalId">Identifier assigned to the listing by the provider.</param>
    /// <returns>The listing, or <c>null</c> if the request fails.</returns>
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
    /// <param name="externalId">Identifier assigned to the listing by the provider.</param>
    /// <returns><c>true</c> if the listing exists and has stock; <c>false</c> otherwise or on error.</returns>
    public async Task<bool> IsListingAvailableAsync(string externalId)
    {
        try
        {
            var listing = await GetListingAsync(externalId);
            return listing is not null && listing.StockQuantity > 0;
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

    /// <summary>
    /// Initializes a new instance of the <see cref="ExternalListingSyncService"/> class.
    /// </summary>
    /// <param name="provider">Provider to read external listings from.</param>
    /// <param name="logger">Logger for sync diagnostics.</param>
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
    /// <param name="category">Category identifier to sync from the provider.</param>
    /// <param name="sellerId">Seller that will own the synced listings.</param>
    /// <returns>The mapped marketplace listings. They are not persisted by this method.</returns>
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
    /// Currently only logs when the listing is unavailable; marking the marketplace listing is not implemented yet.
    /// </summary>
    /// <param name="externalId">Identifier assigned to the listing by the provider.</param>
    /// <param name="listing">Marketplace listing that mirrors the external listing.</param>
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
