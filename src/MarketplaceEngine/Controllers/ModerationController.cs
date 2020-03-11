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
        report.ReviewNotes = request.ActionNotes; // Hotfix: Use ReviewNotes
        report.ResolvedAt = DateTime.UtcNow;

        var updated = await _moderationService.UpdateReportAsync(report);

        // Invalidate listing cache if it was the subject of the report
        if (report.TargetListingId.HasValue) // Hotfix: Use TargetListingId
        {
            await _cacheService.RemoveAsync($"listing:{report.TargetListingId}"); // Hotfix: Use TargetListingId
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
        report.ReviewNotes = request.RejectionReason; // Hotfix: Use ReviewNotes
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
        _logger.LogInformation("Creating report: targetListingId={TargetListingId}", request.TargetListingId); // Hotfix: Use TargetListingId

        if (string.IsNullOrWhiteSpace(request.Reason))
            return BadRequest("Report reason is required");

        if (request.Reason.Length < 10)
            return BadRequest("Report reason must be at least 10 characters");

        var report = new ModerationReport
        {
            Id = Guid.NewGuid(),
            TargetListingId = request.TargetListingId, // Hotfix: Use TargetListingId
            TargetUserId = request.TargetUserId, // Hotfix: Use TargetUserId
            ReporterId = request.ReporterId, // Hotfix: Use ReporterId
            Reason = request.Reason,
            Status = ModerationStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        var created = await _moderationService.CreateReportAsync(report);

        _logger.LogInformation("Report created: {ReportId}", created.Id);
        return CreatedAtAction(nameof(GetReportDetails), new { id = created.Id }, new ModerationReportDto(created));
    }

    /// <summary>
    /// Applies a moderation action to multiple listings in a single request.
    /// Each item is processed independently; the response includes per-item
    /// success/failure status so partial failures can be retried.
    /// Restricted to users with the Moderator or Administrator role.
    /// </summary>
    [HttpPost("bulk")]
    [ProducesResponseType(typeof(BulkModerationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> BulkModerate(
        [FromBody] BulkModerationRequest request,
        [FromHeader(Name = "X-User-Role")] string? userRole)
    {
        _logger.LogInformation("Bulk moderation request: action={Action}, count={Count}",
            request.Action, request.ListingIds?.Count);

        if (userRole != "Moderator" && userRole != "Administrator")
            return Forbid();

        if (request.ListingIds is null || request.ListingIds.Count == 0)
            return BadRequest("At least one listing ID is required");

        if (string.IsNullOrWhiteSpace(request.Action))
            return BadRequest("Action is required (approve, remove, escalate)");

        var results = new List<BulkModerationItemResult>();

        foreach (var listingId in request.ListingIds)
        {
            try
            {
                await _moderationService.ApplyBulkActionAsync(listingId, request.Action);

                // Invalidate cached listing data after moderation action
                await _cacheService.RemoveAsync($"listing:{listingId}");

                results.Add(new BulkModerationItemResult { ListingId = listingId, Success = true });
                _logger.LogInformation("Bulk action '{Action}' applied to listing {ListingId}",
                    request.Action, listingId);
            }
            catch (Exception ex)
            {
                results.Add(new BulkModerationItemResult
                {
                    ListingId = listingId,
                    Success = false,
                    Error = ex.Message
                });
                _logger.LogWarning("Bulk action '{Action}' failed for listing {ListingId}: {Error}",
                    request.Action, listingId, ex.Message);
            }
        }

        await _cacheService.RemoveAsync("listings:*");

        return Ok(new BulkModerationResponse { Results = results });
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

        var approvedReports = await _moderationService.GetReportsByStatusAsync(ModerationStatus.Approved);
        var rejectedReports = await _moderationService.GetReportsByStatusAsync(ModerationStatus.Rejected);
        var (pendingCount, _, _) = await _moderationService.GetReportStatsAsync();

        var resolvedReports = approvedReports.Concat(rejectedReports)
            .Where(r => r.ResolvedAt.HasValue)
            .ToList();

        var averageResolutionHours = resolvedReports.Count > 0
            ? resolvedReports.Average(r => (r.ResolvedAt!.Value - r.CreatedAt).TotalHours)
            : 0;

        var stats = new ModerationStatsDto
        {
            PendingReports = pendingCount,
            ApprovedReports = approvedReports.Count,
            RejectedReports = rejectedReports.Count,
            AverageResolutionTime = Math.Round(averageResolutionHours, 2)
        };

        await _cacheService.SetAsync(cacheKey, stats, TimeSpan.FromMinutes(5));

        return Ok(stats);
    }
}
