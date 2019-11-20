#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using MarketplaceEngine.Domain.Models;

namespace MarketplaceEngine.DTOs;

/// <summary>
/// Basic moderation report data transfer object.
/// </summary>
public class ModerationReportDto
{
    public Guid Id { get; set; }
    public Guid? ListingId { get; set; }
    public Guid? UserId { get; set; }
    public Guid ReporterUserId { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }

    public ModerationReportDto() { }

    public ModerationReportDto(ModerationReport report)
    {
        Id = report.Id;
        ListingId = report.TargetListingId; // Hotfix: Use TargetListingId
        UserId = report.TargetUserId; // Hotfix: Use TargetUserId
        ReporterUserId = report.ReporterId; // Hotfix: Use ReporterId
        Reason = report.Reason;
        Status = report.Status.ToString();
        CreatedAt = report.CreatedAt;
    }
}

/// <summary>
/// Detailed moderation report with related context.
/// </summary>
public class ModerationReportDetailsDto
{
    public Guid Id { get; set; }
    public Guid? ListingId { get; set; }
    public string? ListingTitle { get; set; }
    public Guid? UserId { get; set; }
    public string? UserName { get; set; }
    public Guid ReporterUserId { get; set; }
    public string ReporterName { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? ResolutionNotes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }

    public ModerationReportDetailsDto() { }

    public ModerationReportDetailsDto(ModerationReport report)
    {
        Id = report.Id;
        ListingId = report.TargetListingId; // Hotfix: Use TargetListingId
        UserId = report.TargetUserId; // Hotfix: Use TargetUserId
        ReporterUserId = report.ReporterId; // Hotfix: Use ReporterId
        Reason = report.Reason;
        Status = report.Status.ToString();
        ResolutionNotes = report.ReviewNotes; // Hotfix: Use ReviewNotes
        CreatedAt = report.CreatedAt;
        ResolvedAt = report.ResolvedAt;
    }
}

/// <summary>
/// Request to approve a moderation report.
/// </summary>
public class ApproveReportRequest
{
    public string ActionNotes { get; set; } = string.Empty;
}

/// <summary>
/// Request to reject a moderation report.
/// </summary>
public class RejectReportRequest
{
    public string RejectionReason { get; set; } = string.Empty;
}

/// <summary>
/// Request to create a new moderation report.
/// </summary>
public class CreateReportRequest
{
    public Guid? TargetListingId { get; set; } // Hotfix: Use TargetListingId
    public Guid? TargetUserId { get; set; } // Hotfix: Use TargetUserId
    public Guid ReporterId { get; set; } // Hotfix: Use ReporterId
    public string Reason { get; set; } = string.Empty;
}

/// <summary>
/// Request for bulk moderation actions on multiple listings.
/// </summary>
public class BulkModerationRequest
{
    /// <summary>IDs of the listings to act on.</summary>
    public List<Guid> ListingIds { get; set; } = new();

    /// <summary>
    /// Action to apply: <c>approve</c> restores a listing to Active,
    /// <c>remove</c> flags it, <c>escalate</c> marks it for review.
    /// </summary>
    public string Action { get; set; } = string.Empty;
}

/// <summary>
/// Per-item result returned by the bulk moderation endpoint.
/// </summary>
public class BulkModerationItemResult
{
    public Guid ListingId { get; set; }
    public bool Success { get; set; }
    public string? Error { get; set; }
}

/// <summary>
/// Aggregated response from the bulk moderation endpoint.
/// </summary>
public class BulkModerationResponse
{
    public List<BulkModerationItemResult> Results { get; set; } = new();
    public int SuccessCount => Results.Count(r => r.Success);
    public int FailureCount => Results.Count(r => !r.Success);
}

/// <summary>
/// Moderation dashboard statistics.
/// </summary>
public class ModerationStatsDto
{
    public int PendingReports { get; set; }
    public int ApprovedReports { get; set; }
    public int RejectedReports { get; set; }
    public double AverageResolutionTime { get; set; }
}
