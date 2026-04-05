#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.AspNetCore.Mvc;
using MarketplaceEngine.Services;
using MarketplaceEngine.DTOs;
using MarketplaceEngine.Infrastructure.Caching;

namespace MarketplaceEngine.Controllers;

/// <summary>
/// Manages marketplace categories and category-based listings.
/// Categories are cached longer (30 minutes) as they change infrequently.
/// </summary>
[ApiController]
[Route("api/v1/categories")]
public class CategoriesController : ControllerBase
{
    private readonly CategoryService _categoryService;
    private readonly CacheService _cacheService;
    private readonly ILogger<CategoriesController> _logger;

    public CategoriesController(
        CategoryService categoryService,
        CacheService cacheService,
        ILogger<CategoriesController> logger)
    {
        _categoryService = categoryService;
        _cacheService = cacheService;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves all marketplace categories.
    /// Cached for 30 minutes since category structure is stable.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<CategoryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCategories()
    {
        _logger.LogInformation("Fetching categories");

        var cacheKey = "categories:all";
        var cached = await _cacheService.GetAsync<List<CategoryDto>>(cacheKey);

        if (cached is not null)
        {
            _logger.LogDebug("Cache hit for categories");
            return Ok(cached);
        }

        var categories = await _categoryService.GetAllCategoriesAsync();
        var dtos = categories.Select(c => new CategoryDto
        {
            Id = c.Id,
            Name = c.Name,
            Description = c.Description
        }).ToList();

        // Cache categories for 30 minutes
        await _cacheService.SetAsync(cacheKey, dtos, TimeSpan.FromMinutes(30));

        return Ok(dtos);
    }

    /// <summary>
    /// Retrieves a specific category by ID.
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(CategoryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCategory(Guid id)
    {
        _logger.LogInformation("Fetching category: {CategoryId}", id);

        var cacheKey = $"category:{id}";
        var cached = await _cacheService.GetAsync<CategoryDto>(cacheKey);

        if (cached is not null)
        {
            return Ok(cached);
        }

        var category = await _categoryService.GetCategoryAsync(id);
        if (category is null)
        {
            return NotFound(new { error = "Category not found" });
        }

        var dto = new CategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description
        };

        await _cacheService.SetAsync(cacheKey, dto, TimeSpan.FromMinutes(30));

        return Ok(dto);
    }

    /// <summary>
    /// Retrieves listings in a specific category with pagination.
    /// </summary>
    [HttpGet("{id}/listings")]
    [ProducesResponseType(typeof(PaginatedResponse<ListingDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCategoryListings(
        Guid id,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        _logger.LogInformation("Fetching listings for category: {CategoryId}, page: {Page}", id, page);

        if (page < 1 || pageSize < 1 || pageSize > 100)
            return BadRequest("Invalid pagination parameters");

        var cacheKey = $"category:{id}:listings:page:{page}:size:{pageSize}";
        var cached = await _cacheService.GetAsync<PaginatedResponse<ListingDto>>(cacheKey);

        if (cached is not null)
        {
            return Ok(cached);
        }

        var listings = await _categoryService.GetCategoryListingsAsync(id, page, pageSize);
        var response = new PaginatedResponse<ListingDto>
        {
            Items = listings.Select(l => new ListingDto(l)).ToList(),
            Page = page,
            PageSize = pageSize,
            Total = listings.Count
        };

        // Cache for 10 minutes
        await _cacheService.SetAsync(cacheKey, response, TimeSpan.FromMinutes(10));

        return Ok(response);
    }

    /// <summary>
    /// Retrieves category statistics including listing count and popularity.
    /// </summary>
    [HttpGet("{id}/statistics")]
    [ProducesResponseType(typeof(CategoryStatisticsDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCategoryStatistics(Guid id)
    {
        _logger.LogInformation("Fetching statistics for category: {CategoryId}", id);

        var cacheKey = $"category:{id}:statistics";
        var cached = await _cacheService.GetAsync<CategoryStatisticsDto>(cacheKey);

        if (cached is not null)
        {
            return Ok(cached);
        }

        var listings = await _categoryService.GetCategoryListingsAsync(id, 1, int.MaxValue);

        var stats = new CategoryStatisticsDto
        {
            CategoryId = id,
            TotalListings = listings.Count,
            AveragePrice = listings.Any() ? (decimal)listings.Average(l => l.Price?.Amount ?? 0) : 0,
            TotalViews = listings.Sum(l => l.ViewCount),
            AverageViews = listings.Any() ? listings.Average(l => l.ViewCount) : 0
        };

        await _cacheService.SetAsync(cacheKey, stats, TimeSpan.FromMinutes(15));

        return Ok(stats);
    }
}

/// <summary>
/// DTO for category information.
/// </summary>
public class CategoryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

/// <summary>
/// Category statistics for analytics and trending.
/// </summary>
public class CategoryStatisticsDto
{
    public Guid CategoryId { get; set; }
    public int TotalListings { get; set; }
    public decimal AveragePrice { get; set; }
    public int TotalViews { get; set; }
    public double AverageViews { get; set; }
}
