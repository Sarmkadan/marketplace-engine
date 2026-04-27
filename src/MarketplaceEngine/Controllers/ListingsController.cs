#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.AspNetCore.Mvc;
using MarketplaceEngine.Services;
using MarketplaceEngine.Domain.Models;
using MarketplaceEngine.Domain.ValueObjects; // Hotfix: Add missing using directive
using MarketplaceEngine.DTOs;
using MarketplaceEngine.Infrastructure.Caching;

namespace MarketplaceEngine.Controllers;

/// <summary>
/// Handles all marketplace listing operations including CRUD, search, and filtering.
/// Uses caching to reduce database load on frequently accessed listings.
/// </summary>
[ApiController]
[Route("api/v1/listings")]
public class ListingsController : ControllerBase
{
    private readonly ListingService _listingService;
    private readonly SearchService _searchService;
    private readonly CacheService _cacheService;
    private readonly ILogger<ListingsController> _logger;

    public ListingsController(
        ListingService listingService,
        SearchService searchService,
        CacheService cacheService,
        ILogger<ListingsController> logger)
    {
        _listingService = listingService;
        _searchService = searchService;
        _cacheService = cacheService;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves paginated listings with optional filtering.
    /// Results are cached for 5 minutes to improve performance on high-traffic endpoints.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResponse<ListingDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetListings(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? status = null)
    {
        _logger.LogInformation("Fetching listings: page={Page}, pageSize={PageSize}, status={Status}", page, status);

        var cacheKey = $"listings:page:{page}:size:{pageSize}:status:{status}";
        var cachedResult = await _cacheService.GetAsync<PaginatedResponse<ListingDto>>(cacheKey);

        if (cachedResult is not null)
        {
            _logger.LogInformation("Cache hit for listings");
            return Ok(cachedResult);
        }

        // Validation prevents negative page numbers or excessive page sizes
        if (page < 1 || pageSize < 1 || pageSize > 100)
            return BadRequest("Invalid pagination parameters");

        var (items, total) = await _listingService.GetPaginatedListingsAsync(page, pageSize); // Hotfix: Use GetPaginatedListingsAsync
        var response = new PaginatedResponse<ListingDto>
        {
            Items = items.Select(l => new ListingDto(l)).ToList(),
            Page = page,
            PageSize = pageSize,
            Total = total // Hotfix: Use total from GetPaginatedListingsAsync
        };

        // Cache for 5 minutes since listing data changes infrequently
        await _cacheService.SetAsync(cacheKey, response, TimeSpan.FromMinutes(5));

        return Ok(response);
    }

    /// <summary>
    /// Retrieves a specific listing by ID with view count increment.
    /// Individual listing caches are shorter (2 minutes) to balance freshness and performance.
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ListingDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetListing(Guid id)
    {
        _logger.LogInformation("Fetching listing: {ListingId}", id);

        var cacheKey = $"listing:{id}";
        var cached = await _cacheService.GetAsync<ListingDto>(cacheKey);

        if (cached is not null)
        {
            return Ok(cached);
        }

        var listing = await _listingService.GetListingWithViewAsync(id); // Hotfix: Use GetListingWithViewAsync
        if (listing is null)
        {
            _logger.LogWarning("Listing not found: {ListingId}", id);
            return NotFound(new { error = "Listing not found" });
        }

        var dto = new ListingDto(listing);
        await _cacheService.SetAsync(cacheKey, dto, TimeSpan.FromMinutes(2));

        return Ok(dto);
    }

    /// <summary>
    /// Creates a new listing. Invalidates pagination cache after creation
    /// since the total count has changed.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ListingDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateListing([FromBody] CreateListingRequest request)
    {
        _logger.LogInformation("Creating new listing: {Title}", request.Title);

        if (string.IsNullOrWhiteSpace(request.Title))
            return BadRequest("Title is required");

        var listing = new Listing
        {
            Id = Guid.NewGuid(),
            Title = request.Title,
            Description = request.Description,
            // Hotfix: Instantiate Money object for price and assume "USD"
            Price = new Money(request.Price, "USD"),
            SellerId = request.SellerId,
            CategoryId = request.CategoryId,
            CreatedAt = DateTime.UtcNow
        };

        // Hotfix: Call CreateListingAsync with individual parameters
        var created = await _listingService.CreateListingAsync(
            request.SellerId,
            request.Title,
            request.Description,
            request.Price,
            "USD", // Assuming USD as default currency
            request.CategoryId,
            request.ImageUrls ?? new List<string>() // Handle nullable ImageUrls
        );

        // Invalidate pagination caches
        await InvalidateListingsCaches();

        _logger.LogInformation("Listing created: {ListingId}", created.Id);
        var dto = new ListingDto(created);
        return CreatedAtAction(nameof(GetListing), new { id = created.Id }, dto);
    }

    /// <summary>
    /// Updates an existing listing. Invalidates all category and search caches so that
    /// category-filtered searches reflect the change immediately.
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ListingDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateListing(Guid id, [FromBody] UpdateListingRequest request)
    {
        _logger.LogInformation("Updating listing: {ListingId}", id);

        var price = request.Price.HasValue ? new Money(request.Price.Value, "USD") : null;

        var (updated, previousCategoryId) = await _listingService.UpdateListingAsync(
            id, request.SellerId, request.Title, request.Description, price, request.CategoryId);

        // Invalidate the specific listing cache
        await _cacheService.RemoveAsync($"listing:{id}");

        // Invalidate category-level listing and statistics caches for both old and new categories
        await _cacheService.RemoveAsync($"category:{previousCategoryId}:listings:*");
        await _cacheService.RemoveAsync($"category:{previousCategoryId}:statistics");
        if (request.CategoryId.HasValue && request.CategoryId.Value != previousCategoryId)
        {
            await _cacheService.RemoveAsync($"category:{request.CategoryId.Value}:listings:*");
            await _cacheService.RemoveAsync($"category:{request.CategoryId.Value}:statistics");
        }

        // Invalidate all search result caches so stale category data is not served
        await _cacheService.RemoveAsync("search:*");
        await InvalidateListingsCaches();

        _logger.LogInformation("Listing updated and caches invalidated: {ListingId}", id);
        return Ok(new ListingDto(updated));
    }

    /// <summary>
    /// Searches listings with full-text search capability.
    /// Search results are cached separately from listing listings.
    /// </summary>
    [HttpGet("search")]
    [ProducesResponseType(typeof(SearchResultDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> SearchListings([FromQuery] string q, [FromQuery] int limit = 10)
    {
        _logger.LogInformation("Searching listings: query={Query}, limit={Limit}", q, limit);

        if (string.IsNullOrWhiteSpace(q) || q.Length < 2)
            return BadRequest("Search query must be at least 2 characters");

        var cacheKey = $"search:{q}:limit:{limit}";
        var cached = await _cacheService.GetAsync<List<ListingDto>>(cacheKey);

        if (cached is not null)
        {
            return Ok(new SearchResultDto { Query = q, Results = cached });
        }

        var results = await _searchService.SearchListingsAsync(q);
        var dtos = results.Take(limit).Select(l => new ListingDto(l)).ToList();

        // Cache search results for 10 minutes
        await _cacheService.SetAsync(cacheKey, dtos, TimeSpan.FromMinutes(10));

        return Ok(new SearchResultDto { Query = q, Results = dtos });
    }

    private async Task InvalidateListingsCaches()
    {
        // Clear all listing-related caches to maintain consistency
        await _cacheService.RemoveAsync("listings:*");
    }
}
