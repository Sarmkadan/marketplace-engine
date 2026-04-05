#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.AspNetCore.Mvc;
using MarketplaceEngine.Services;
using MarketplaceEngine.Domain.Models;
using MarketplaceEngine.Domain.Enums;
using MarketplaceEngine.DTOs;
using MarketplaceEngine.Infrastructure.Caching;

namespace MarketplaceEngine.Controllers;

/// <summary>
/// Manages moderation tasks including report review and action enforcement.
/// Reports are not cached to ensure moderators always see latest information.
/// </summary>
[ApiController]
[Route("api/v1/moderation")]
public class ModerationController : ControllerBase
{
    private readonly ModerationService _moderationService;
    private readonly CacheService _cacheService;
    private readonly ILogger<ModerationController> _logger;

    public ModerationController(
        ModerationService moderationService,
        CacheService cacheService,
        ILogger<ModerationController> logger)
    {
        _moderationService = moderationService;
        _cacheService = cacheService;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves pending moderation reports with pagination.
    /// Not cached to ensure moderators see real-time updates.
    /// </summary>
    [HttpGet("reports")]
    [ProducesResponseType(typeof(PaginatedResponse<ModerationReportDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPendingReports(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        _logger.LogInformation("Fetching pending reports: page={Page}", page);

        if (page < 1 || pageSize < 1 || pageSize > 100)
            return BadRequest("Invalid pagination parameters");

        var reports = await _moderationService.GetPendingReportsAsync(page, pageSize);
        var response = new PaginatedResponse<ModerationReportDto>
        {
            Items = reports.Select(r => new ModerationReportDto(r)).ToList(),
            Page = page,
            PageSize = pageSize,
            Total = reports.Count
        };

        return Ok(response);
    }

    /// <summary>
    /// Retrieves details of a specific report.
    /// Includes related listing and user information for context.
    /// </summary>
    [HttpGet("reports/{id}")]
    [ProducesResponseType(typeof(ModerationReportDetailsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetReportDetails(Guid id)
    {
        _logger.LogInformation("Fetching report details: {ReportId}", id);

        var report = await _moderationService.GetReportAsync(id);
        if (report is null)
        {
            return NotFound();
        }

        var details = new ModerationReportDetailsDto(report);
        return Ok(details);
    }

    /// <summary>
    /// Approves a moderation report and takes action against the listing/user.
    /// Action type determines consequence: warning, suspension, permanent removal.
    /// </summary>
    [HttpPost("reports/{id}/approve")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ApproveReport(Guid id, [FromBody] ApproveReportRequest request)
    {
        _logger.LogInformation("Approving report: {ReportId}", id);

        if (string.IsNullOrWhiteSpace(request.ActionNotes))
            return BadRequest("Action notes are required");

        var report = await _moderationService.GetReportAsync(id);
        if (report is null)
        {
            return NotFound();
        }

        report.Status = ModerationStatus.Approved;
        report.ResolutionNotes = request.ActionNotes;
        report.ResolvedAt = DateTime.UtcNow;

        var updated = await _moderationService.UpdateReportAsync(report);

        // Invalidate listing cache if it was the subject of the report
        if (report.ListingId.HasValue)
        {
            await _cacheService.RemoveAsync($"listing:{report.ListingId}");
            await _cacheService.RemoveAsync("listings:*");
        }

        _logger.LogInformation("Report approved: {ReportId}", id);
        return Ok(new { message = "Report approved", reportId = id });
    }

    /// <summary>
    /// Rejects a moderation report and returns it to pending.
    /// Rejection requires documented reasoning for audit trail.
    /// </summary>
    [HttpPost("reports/{id}/reject")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RejectReport(Guid id, [FromBody] RejectReportRequest request)
    {
        _logger.LogInformation("Rejecting report: {ReportId}", id);

        if (string.IsNullOrWhiteSpace(request.RejectionReason))
            return BadRequest("Rejection reason is required");

        var report = await _moderationService.GetReportAsync(id);
        if (report is null)
        {
            return NotFound();
        }

        report.Status = ModerationStatus.Rejected;
        report.ResolutionNotes = request.RejectionReason;
        report.ResolvedAt = DateTime.UtcNow;

        await _moderationService.UpdateReportAsync(report);

        _logger.LogInformation("Report rejected: {ReportId}", id);
        return Ok(new { message = "Report rejected", reportId = id });
    }

    /// <summary>
    /// Creates a moderation report for a listing or user.
    /// Reports are queued for moderator review.
    /// </summary>
    [HttpPost("reports")]
    [ProducesResponseType(typeof(ModerationReportDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateReport([FromBody] CreateReportRequest request)
    {
        _logger.LogInformation("Creating report: listingId={ListingId}", request.ListingId);

        if (string.IsNullOrWhiteSpace(request.Reason))
            return BadRequest("Report reason is required");

        if (request.Reason.Length < 10)
            return BadRequest("Report reason must be at least 10 characters");

        var report = new ModerationReport
        {
            Id = Guid.NewGuid(),
            ListingId = request.ListingId,
            UserId = request.UserId,
            ReporterUserId = request.ReporterUserId,
            Reason = request.Reason,
            Status = ModerationStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        var created = await _moderationService.CreateReportAsync(report);

        _logger.LogInformation("Report created: {ReportId}", created.Id);
        return CreatedAtAction(nameof(GetReportDetails), new { id = created.Id }, new ModerationReportDto(created));
    }

    /// <summary>
    /// Retrieves moderation statistics for dashboard.
    /// Provides overview of pending, approved, and rejected reports.
    /// </summary>
    [HttpGet("statistics")]
    [ProducesResponseType(typeof(ModerationStatsDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStatistics()
    {
        _logger.LogInformation("Fetching moderation statistics");

        var cacheKey = "moderation:statistics";
        var cached = await _cacheService.GetAsync<ModerationStatsDto>(cacheKey);

        if (cached is not null)
        {
            return Ok(cached);
        }

        // In production, these would query actual report data
        var stats = new ModerationStatsDto
        {
            PendingReports = 0,
            ApprovedReports = 0,
            RejectedReports = 0,
            AverageResolutionTime = 0
        };

        await _cacheService.SetAsync(cacheKey, stats, TimeSpan.FromMinutes(5));

        return Ok(stats);
    }
}
