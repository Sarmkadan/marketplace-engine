#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using MarketplaceEngine.Domain.Enums;

namespace MarketplaceEngine.Domain.Models;

/// <summary>
/// Represents a moderation report for flagged content or user violations.
/// </summary>
public class ModerationReport
{
    public Guid Id { get; set; }
    public Guid ReporterId { get; set; }
    public User? Reporter { get; set; }
    public Guid? TargetUserId { get; set; }
    public User? TargetUser { get; set; }
    public Guid? TargetListingId { get; set; }
    public Listing? TargetListing { get; set; }
    public Guid? TargetMessageId { get; set; }
    public Message? TargetMessage { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string? Details { get; set; }
    public string? Evidence { get; set; }
    public ModerationStatus Status { get; set; } = ModerationStatus.Pending;
    public Guid? ReviewedBy { get; set; }
    public User? Reviewer { get; set; }
    public string? ReviewNotes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public int Priority { get; set; } = 1; // 1-5, higher is more urgent

    // Validates the report before creation
    public void ValidateReport()
    {
        if (ReporterId == Guid.Empty)
            throw new ArgumentException("Reporter ID is required", nameof(ReporterId));

        if (TargetUserId == Guid.Empty && TargetListingId == Guid.Empty && TargetMessageId == Guid.Empty)
            throw new ArgumentException("At least one target (user, listing, or message) is required");

        if (string.IsNullOrWhiteSpace(Reason) || Reason.Length < 5)
            throw new ArgumentException("Reason must be at least 5 characters", nameof(Reason));

        if (Reason.Length > 500)
            throw new ArgumentException("Reason cannot exceed 500 characters", nameof(Reason));

        if (Priority < 1 || Priority > 5)
            throw new ArgumentException("Priority must be between 1 and 5", nameof(Priority));
    }

    // Submits the report for review
    public void Submit()
    {
        ValidateReport();
        Status = ModerationStatus.Pending;
    }

    // Assigns report to a moderator for review
    public void AssignToModerator(Guid moderatorId)
    {
        if (moderatorId == Guid.Empty)
            throw new ArgumentException("Moderator ID is required", nameof(moderatorId));

        ReviewedBy = moderatorId;
        Status = ModerationStatus.InReview;
        UpdatedAt = DateTime.UtcNow;
    }

    // Approves the report
    public void Approve(string notes = "")
    {
        if (Status == ModerationStatus.Approved)
            return;

        Status = ModerationStatus.Approved;
        ReviewNotes = notes;
        ResolvedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    // Rejects the report
    public void Reject(string notes = "")
    {
        if (Status == ModerationStatus.Rejected)
            return;

        Status = ModerationStatus.Rejected;
        ReviewNotes = notes;
        ResolvedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    // Marks content as removed
    public void RemoveContent(string notes = "")
    {
        Status = ModerationStatus.ContentRemoved;
        ReviewNotes = notes;
        ResolvedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    // Suspends the target user
    public void SuspendUser(string notes = "")
    {
        Status = ModerationStatus.UserSuspended;
        ReviewNotes = notes;
        ResolvedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    // Bans the target user
    public void BanUser(string notes = "")
    {
        Status = ModerationStatus.UserBanned;
        ReviewNotes = notes;
        ResolvedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    // Gets report type description
    public string GetReportType()
    {
        if (TargetUserId.HasValue && TargetUserId != Guid.Empty)
            return "User Report";
        if (TargetListingId.HasValue && TargetListingId != Guid.Empty)
            return "Listing Report";
        if (TargetMessageId.HasValue && TargetMessageId != Guid.Empty)
            return "Message Report";

        return "Unknown";
    }

    // Checks if report is pending resolution
    public bool IsPending() => Status == ModerationStatus.Pending || Status == ModerationStatus.InReview;

    // Checks if report has been resolved
    public bool IsResolved() => ResolvedAt.HasValue;

    // Gets priority description
    public string GetPriorityLabel() => Priority switch
    {
        1 => "Low",
        2 => "Medium",
        3 => "High",
        4 => "Critical",
        5 => "Urgent",
        _ => "Unknown"
    };

    // Escalates report priority
    public void EscalatePriority()
    {
        if (Priority < 5)
        {
            Priority++;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
