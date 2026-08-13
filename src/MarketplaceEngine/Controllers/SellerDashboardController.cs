#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
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

    /// <summary>
    /// Exports the seller's listing performance data as a CSV file.
    /// </summary>
    /// <param name="sellerId">The identifier of the seller.</param>
    /// <returns>A CSV file containing the performance rows.</returns>
    [HttpGet("performance/csv")]
    [Produces("text/csv")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult ExportSellerListingPerformanceCsv(Guid sellerId)
    {
        _logger.LogInformation("Exporting listing performance CSV for seller {SellerId}", sellerId);

        // TODO: Replace the empty list with a real data source (e.g., a service or repository).
        var performanceData = new List<SellerListingPerformanceDto>();

        var csvContent = GenerateCsv(performanceData);
        var fileName = $"seller_{sellerId}_performance.csv";

        var fileBytes = Encoding.UTF8.GetBytes(csvContent);
        return File(fileBytes, "text/csv", fileName);
    }

    private static string GenerateCsv(IEnumerable<SellerListingPerformanceDto> items)
    {
        var sb = new StringBuilder();

        // Get all public instance properties of the DTO, ordered alphabetically for consistency.
        var properties = typeof(SellerListingPerformanceDto)
            .GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .OrderBy(p => p.Name)
            .ToArray();

        // Header row
        sb.AppendLine(string.Join(",", properties.Select(p => Escape(p.Name))));

        // Data rows
        foreach (var item in items)
        {
            var values = properties.Select(p =>
            {
                var value = p.GetValue(item);
                if (value == null)
                    return string.Empty;

                // Use invariant culture for numbers/dates (except strings).
                if (value is IFormattable formattable && !(value is string))
                    return Escape(formattable.ToString(null, CultureInfo.InvariantCulture));

                return Escape(value.ToString());
            });

            sb.AppendLine(string.Join(",", values));
        }

        return sb.ToString();
    }

    private static string Escape(string input)
    {
        if (input == null)
            return string.Empty;

        // CSV requires fields containing commas, quotes, or line breaks to be quoted.
        var mustQuote = input.Contains('"') ||
                        input.Contains(',') ||
                        input.Contains('\r') ||
                        input.Contains('\n');

        if (mustQuote)
        {
            var escaped = input.Replace("\"", "\"\"");
            return $"\"{escaped}\"";
        }

        return input;
    }
}
