#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.AspNetCore.Mvc;
using MarketplaceEngine.DTOs;
using MarketplaceEngine.Services;

namespace MarketplaceEngine.Controllers;

/// <summary>
/// Provides aggregated seller analytics including revenue breakdowns,
/// listing performance statistics, and review summaries.
/// </summary>
[ApiController]
[Route("api/v1/sellers/{sellerId}/dashboard")]
public class SellerDashboardController : ControllerBase
{
    private readonly SellerDashboardService _dashboardService;
    private readonly ILogger<SellerDashboardController> _logger;

    public SellerDashboardController(
        SellerDashboardService dashboardService,
        ILogger<SellerDashboardController> logger)
    {
        _dashboardService = dashboardService;
        _logger = logger;
    }

    /// <summary>
    /// Returns the high-level dashboard overview for the seller.
    /// Includes active listing count, total revenue, pending payout,
    /// average rating and unread message count.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(SellerDashboardDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDashboard(Guid sellerId)
    {
        _logger.LogInformation("Fetching dashboard for seller {SellerId}", sellerId);
        var dashboard = await _dashboardService.GetDashboardAsync(sellerId);
        return Ok(dashboard);
    }

    /// <summary>
    /// Returns detailed revenue data including gross revenue, platform fees,
    /// net revenue, pending payout, and a month-by-month breakdown.
    /// </summary>
    [HttpGet("revenue")]
    [ProducesResponseType(typeof(SellerRevenueDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetRevenue(Guid sellerId)
    {
        _logger.LogInformation("Fetching revenue for seller {SellerId}", sellerId);
        var revenue = await _dashboardService.GetRevenueAsync(sellerId);
        return Ok(revenue);
    }

    /// <summary>
    /// Returns listing performance statistics: view counts, interest counts,
    /// and a top-10 listing ranking by views.
    /// </summary>
    [HttpGet("listings")]
    [ProducesResponseType(typeof(SellerListingStatsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetListingStats(Guid sellerId)
    {
        _logger.LogInformation("Fetching listing stats for seller {SellerId}", sellerId);
        var stats = await _dashboardService.GetListingStatsAsync(sellerId);
        return Ok(stats);
    }
}
