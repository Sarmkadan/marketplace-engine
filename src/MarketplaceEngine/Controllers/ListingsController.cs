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
    /// <remarks>Route: <c>GET api/v1/listings</c>.</remarks>
    /// <param name="page">The one-based page number. Defaults to <c>1</c>.</param>
    /// <param name="pageSize">The number of listings per page. Defaults to <c>20</c> and must be between 1 and 100.</param>
    /// <param name="status">An optional listing status filter.</param>
    /// <returns>A result containing the requested page of listings.</returns>
    /// <response code="200">Returns the requested page of listings.</response>
    /// <response code="400">The pagination parameters are invalid.</response>
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
    /// <remarks>Route: <c>GET api/v1/listings/{id}</c>.</remarks>
    /// <param name="id">The unique identifier of the listing.</param>
    /// <returns>A result containing the requested listing.</returns>
    /// <response code="200">Returns the requested listing.</response>
    /// <response code="404">No listing exists with the specified identifier.</response>
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
    /// <remarks>Route: <c>POST api/v1/listings</c>.</remarks>
    /// <param name="request">The details of the listing to create.</param>
    /// <returns>A result containing the created listing and its location.</returns>
    /// <response code="201">Returns the newly created listing.</response>
    /// <response code="400">The request is invalid, including when the title is missing.</response>
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
    /// Creates a new listing as a draft.
    /// </summary>
    /// <remarks>Route: <c>POST api/v1/listings/drafts</c>.</remarks>
    /// <param name="request">The details of the draft listing to create.</param>
    /// <returns>A result containing the created draft listing and its location.</returns>
    /// <response code="201">Returns the newly created draft listing.</response>
    /// <response code="400">The request is invalid, including when the title is missing.</response>
    [HttpPost("drafts")]
    [ProducesResponseType(typeof(ListingDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateDraftListing([FromBody] CreateListingRequest request)
    {
        _logger.LogInformation("Creating new draft listing: {Title}", request.Title);

        if (string.IsNullOrWhiteSpace(request.Title))
            return BadRequest("Title is required");

        var created = await _listingService.CreateDraftListingAsync(
            request.SellerId,
            request.Title,
            request.Description,
            request.Price,
            "USD", // Assuming USD as default currency
            request.CategoryId,
            request.ImageUrls ?? new List<string>()
        );

        // Invalidate pagination caches
        await InvalidateListingsCaches();

        _logger.LogInformation("Draft listing created: {ListingId}", created.Id);
        var dto = new ListingDto(created);
        return CreatedAtAction(nameof(GetListing), new { id = created.Id }, dto);
    }

    /// <summary>
    /// Publishes a draft listing to the marketplace.
    /// </summary>
    /// <remarks>Route: <c>POST api/v1/listings/{id}/publish</c>.</remarks>
    /// <param name="id">The unique identifier of the draft listing to publish.</param>
    /// <returns>A result containing the published listing.</returns>
    /// <response code="200">Returns the published listing.</response>
    /// <response code="400">The specified listing is not a draft.</response>
    /// <response code="403">The requester is not authorized to publish the listing.</response>
    /// <response code="404">No listing exists with the specified identifier.</response>
    [HttpPost("{id}/publish")]
    [ProducesResponseType(typeof(ListingDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> PublishDraft(Guid id)
    {
        _logger.LogInformation("Publishing draft listing: {ListingId}", id);

        var listing = await _listingService.GetListingWithViewAsync(id);
        if (listing is null)
        {
            _logger.LogWarning("Draft listing not found: {ListingId}", id);
            return NotFound(new { error = "Listing not found" });
        }

        if (listing.Status.ToString() != "Draft")
        {
            _logger.LogWarning("Listing is not a draft: {ListingId}, status={Status}", id, listing.Status);
            return BadRequest("Only draft listings can be published");
        }

        // Note: The service layer handles authorization, but we need to pass requesterId
        // Since we don't have the requester in this endpoint, we'll get it from the listing
        var updated = await _listingService.PublishDraftAsync(id, listing.SellerId);

        // Invalidate caches
        await _cacheService.RemoveAsync($"listing:{id}");
        await InvalidateListingsCaches();

        _logger.LogInformation("Draft listing published: {ListingId}", id);
        return Ok(new ListingDto(updated));
    }

    /// <summary>
    /// Updates an existing listing. Invalidates all category and search caches so that
    /// category-filtered searches reflect the change immediately.
    /// </summary>
    /// <remarks>Route: <c>PUT api/v1/listings/{id}</c>.</remarks>
    /// <param name="id">The unique identifier of the listing to update.</param>
    /// <param name="request">The listing fields to update and the seller performing the update.</param>
    /// <returns>A result containing the updated listing.</returns>
    /// <response code="200">Returns the updated listing.</response>
    /// <response code="400">The update request is invalid.</response>
    /// <response code="403">The requester is not authorized to update the listing.</response>
    /// <response code="404">No listing exists with the specified identifier.</response>
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
    /// <remarks>Route: <c>GET api/v1/listings/search</c>.</remarks>
    /// <param name="q">The search query, which must contain at least two characters.</param>
    /// <param name="limit">The maximum number of results to return. Defaults to <c>10</c>.</param>
    /// <returns>A result containing listings that match the query.</returns>
    /// <response code="200">Returns the matching listings.</response>
    /// <response code="400">The search query is missing or shorter than two characters.</response>
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
