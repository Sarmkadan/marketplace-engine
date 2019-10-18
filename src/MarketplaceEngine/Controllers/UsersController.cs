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
/// Manages user profiles, ratings, and seller reputation.
/// User data is cached longer (15 minutes) as profile changes are infrequent.
/// </summary>
[ApiController]
[Route("api/v1/users")]
public class UsersController : ControllerBase
{
    private readonly UserService _userService;
    private readonly SearchService _searchService;
    private readonly CacheService _cacheService;
    private readonly ILogger<UsersController> _logger;

    public UsersController(
        UserService userService,
        SearchService searchService,
        CacheService cacheService,
        ILogger<UsersController> logger)
    {
        _userService = userService;
        _searchService = searchService;
        _cacheService = cacheService;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves user profile with cached reputation metrics.
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUserProfile(Guid id)
    {
        _logger.LogInformation("Fetching user profile: {UserId}", id);

        var cacheKey = $"user:{id}:profile";
        var cached = await _cacheService.GetAsync<UserDto>(cacheKey);

        if (cached != null)
        {
            return Ok(cached);
        }

        var user = await _userService.GetUserAsync(id);
        if (user == null)
        {
            _logger.LogWarning("User not found: {UserId}", id);
            return NotFound(new { error = "User not found" });
        }

        var dto = new UserDto(user);
        // Cache user profiles for 15 minutes
        await _cacheService.SetAsync(cacheKey, dto, TimeSpan.FromMinutes(15));

        return Ok(dto);
    }

    /// <summary>
    /// Retrieves user's seller metrics including ratings and sales volume.
    /// Cached separately from profile data to allow different refresh rates.
    /// </summary>
    [HttpGet("{id}/seller-metrics")]
    [ProducesResponseType(typeof(SellerMetricsDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSellerMetrics(Guid id)
    {
        _logger.LogInformation("Fetching seller metrics: {UserId}", id);

        var cacheKey = $"user:{id}:metrics";
        var cached = await _cacheService.GetAsync<SellerMetricsDto>(cacheKey);

        if (cached != null)
        {
            return Ok(cached);
        }

        var user = await _userService.GetUserAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        // Calculate metrics from user data
        var metrics = new SellerMetricsDto
        {
            UserId = id,
            AverageRating = user.Rating?.Score ?? 0,
            TotalReviews = user.Rating?.ReviewCount ?? 0,
            TotalSales = 0, // Would be calculated from listings
            ResponseTime = "< 1 hour"
        };

        await _cacheService.SetAsync(cacheKey, metrics, TimeSpan.FromMinutes(10));

        return Ok(metrics);
    }

    /// <summary>
    /// Retrieves top sellers ranked by rating and sales volume.
    /// Cached for 30 minutes since rankings change slowly.
    /// </summary>
    [HttpGet("top-sellers")]
    [ProducesResponseType(typeof(List<SellerRankingDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTopSellers([FromQuery] int limit = 10)
    {
        _logger.LogInformation("Fetching top sellers: limit={Limit}", limit);

        var cacheKey = $"sellers:top:{limit}";
        var cached = await _cacheService.GetAsync<List<SellerRankingDto>>(cacheKey);

        if (cached != null)
        {
            return Ok(cached);
        }

        var sellers = await _searchService.GetTopSellersAsync(limit);
        var rankings = sellers.Select((s, index) => new SellerRankingDto
        {
            Rank = index + 1,
            UserId = s.Id,
            DisplayName = s.DisplayName,
            AverageRating = s.Rating?.Score ?? 0,
            TotalReviews = s.Rating?.ReviewCount ?? 0
        }).ToList();

        await _cacheService.SetAsync(cacheKey, rankings, TimeSpan.FromMinutes(30));

        return Ok(rankings);
    }

    /// <summary>
    /// Updates user profile information.
    /// Invalidates caches to ensure consistency across requests.
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateUserProfile(Guid id, [FromBody] UpdateUserRequest request)
    {
        _logger.LogInformation("Updating user profile: {UserId}", id);

        if (string.IsNullOrWhiteSpace(request.DisplayName))
            return BadRequest("Display name is required");

        var user = await _userService.GetUserAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        user.DisplayName = request.DisplayName;
        user.Email = request.Email;
        user.Bio = request.Bio;

        var updated = await _userService.UpdateUserAsync(user);

        // Invalidate related caches
        await _cacheService.RemoveAsync($"user:{id}:*");
        await _cacheService.RemoveAsync("sellers:top:*");

        _logger.LogInformation("User profile updated: {UserId}", id);
        return Ok(new UserDto(updated));
    }

    /// <summary>
    /// Verifies user's email address for account validation.
    /// </summary>
    [HttpPost("{id}/verify-email")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> VerifyEmail(Guid id)
    {
        _logger.LogInformation("Verifying email for user: {UserId}", id);

        var user = await _userService.GetUserAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        user.EmailVerified = true;
        await _userService.UpdateUserAsync(user);

        await _cacheService.RemoveAsync($"user:{id}:*");

        return Ok(new { message = "Email verified successfully" });
    }
}
