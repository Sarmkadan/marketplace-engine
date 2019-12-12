#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using MarketplaceEngine.Domain.Enums;
using MarketplaceEngine.Domain.Models;

namespace MarketplaceEngine.Services;

/// <summary>
/// Enum representing the target type of a moderation report.
/// </summary>
public enum ModerationTargetType
{
    User,
    Listing
}

/// <summary>
/// Extension methods for <see cref="ModerationService"/> providing additional functionality
/// for moderation workflows and batch operations.
/// </summary>
public static class ModerationServiceExtensions
{
    /// <summary>
    /// Creates multiple moderation reports for listings in a single batch operation.
    /// </summary>
    /// <param name="service">The moderation service instance</param>
    /// <param name="reporterId">The ID of the user filing the reports</param>
    /// <param name="listingIds">Collection of listing IDs to report</param>
    /// <param name="reason">Reason for reporting</param>
    /// <param name="details">Optional details about the reports</param>
    /// <param name="priority">Priority level (1-5)</param>
    /// <returns>List of created moderation reports</returns>
    public static async Task<List<ModerationReport>> ReportListingsBatchAsync(
        this ModerationService service,
        Guid reporterId,
        IEnumerable<Guid> listingIds,
        string reason,
        string? details = null,
        int priority = 1)
    {
        var reports = new List<ModerationReport>();

        foreach (var listingId in listingIds)
        {
            var report = await service.ReportListingAsync(reporterId, listingId, reason, details, priority);
            reports.Add(report);
        }

        return reports;
    }

    /// <summary>
    /// Creates multiple moderation reports for users in a single batch operation.
    /// </summary>
    /// <param name="service">The moderation service instance</param>
    /// <param name="reporterId">The ID of the user filing the reports</param>
    /// <param name="userIds">Collection of user IDs to report</param>
    /// <param name="reason">Reason for reporting</param>
    /// <param name="details">Optional details about the reports</param>
    /// <param name="priority">Priority level (1-5)</param>
    /// <returns>List of created moderation reports</returns>
    public static async Task<List<ModerationReport>> ReportUsersBatchAsync(
        this ModerationService service,
        Guid reporterId,
        IEnumerable<Guid> userIds,
        string reason,
        string? details = null,
        int priority = 1)
    {
        var reports = new List<ModerationReport>();

        foreach (var userId in userIds)
        {
            var report = await service.ReportUserAsync(reporterId, userId, reason, details, priority);
            reports.Add(report);
        }

        return reports;
    }

    /// <summary>
    /// Assigns multiple reports to a moderator in a single operation.
    /// </summary>
    /// <param name="service">The moderation service instance</param>
    /// <param name="reports">Collection of reports to assign</param>
    /// <param name="moderatorId">ID of the moderator to assign to</param>
    /// <returns>List of assigned moderation reports</returns>
    public static async Task<List<ModerationReport>> AssignReportsToModeratorAsync(
        this ModerationService service,
        IEnumerable<ModerationReport> reports,
        Guid moderatorId)
    {
        var assignedReports = new List<ModerationReport>();

        foreach (var report in reports)
        {
            var assignedReport = await service.AssignReportAsync(report, moderatorId);
            assignedReports.Add(assignedReport);
        }

        return assignedReports;
    }

    /// <summary>
    /// Applies the same action to multiple reports in a single batch operation.
    /// </summary>
    /// <param name="service">The moderation service instance</param>
    /// <param name="reports">Collection of reports to process</param>
    /// <param name="action">Action to apply: 'approve', 'reject', 'remove', 'suspend', or 'ban'</param>
    /// <param name="reviewNotes">Optional review notes to add to each report</param>
    /// <returns>List of processed moderation reports</returns>
    public static async Task<List<ModerationReport>> ProcessReportsBatchAsync(
        this ModerationService service,
        IEnumerable<ModerationReport> reports,
        string action,
        string? reviewNotes = null)
    {
        var processedReports = new List<ModerationReport>();

        foreach (var report in reports)
        {
            switch (action.Trim().ToLowerInvariant())
            {
                case "approve":
                    var approvedReport = await service.ApproveReportAsync(report, reviewNotes ?? "");
                    processedReports.Add(approvedReport);
                    break;

                case "reject":
                    var rejectedReport = await service.RejectReportAsync(report, reviewNotes ?? "");
                    processedReports.Add(rejectedReport);
                    break;

                case "remove":
                    var removedReport = await service.RemoveContentAsync(report, reviewNotes ?? "");
                    processedReports.Add(removedReport);
                    break;

                case "suspend":
                    var suspendedReport = await service.SuspendUserAsync(report, reviewNotes ?? "");
                    processedReports.Add(suspendedReport);
                    break;

                case "ban":
                    var bannedReport = await service.BanUserAsync(report, reviewNotes ?? "");
                    processedReports.Add(bannedReport);
                    break;

                default:
                    throw new ArgumentException(
                        $"Unknown action '{action}'. Valid values: approve, reject, remove, suspend, ban",
                        nameof(action));
            }
        }

        return processedReports;
    }

    /// <summary>
    /// Escalates the priority of multiple reports in a single operation.
    /// </summary>
    /// <param name="service">The moderation service instance</param>
    /// <param name="reports">Collection of reports to escalate</param>
    /// <returns>List of escalated moderation reports</returns>
    public static async Task<List<ModerationReport>> EscalateReportsPriorityAsync(
        this ModerationService service,
        IEnumerable<ModerationReport> reports)
    {
        var escalatedReports = new List<ModerationReport>();

        foreach (var report in reports)
        {
            var escalatedReport = service.EscalateReportAsync(report);
            escalatedReports.Add(escalatedReport);
        }

        return escalatedReports;
    }

    /// <summary>
    /// Filters reports by target type (user or listing) and returns only matching reports.
    /// </summary>
    /// <param name="service">The moderation service instance</param>
    /// <param name="reports">Collection of reports to filter</param>
    /// <param name="targetType">Type of target to filter by</param>
    /// <returns>Filtered list of moderation reports</returns>
    public static async Task<List<ModerationReport>> FilterByTargetTypeAsync(
        this ModerationService service,
        IEnumerable<ModerationReport> reports,
        ModerationTargetType targetType)
    {
        return targetType switch
        {
            ModerationTargetType.User => reports
                .Where(r => r.TargetUserId.HasValue && r.TargetUserId != Guid.Empty)
                .ToList(),
            ModerationTargetType.Listing => reports
                .Where(r => r.TargetListingId.HasValue && r.TargetListingId != Guid.Empty)
                .ToList(),
            _ => reports.ToList()
        };
    }

    /// <summary>
    /// Gets all pending reports with optional filtering by priority.
    /// </summary>
    /// <param name="service">The moderation service instance</param>
    /// <param name="page">Page number for pagination</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <param name="minPriority">Minimum priority level to include (null for all)</param>
    /// <returns>Filtered list of pending moderation reports</returns>
    public static async Task<List<ModerationReport>> GetPendingReportsByPriorityAsync(
        this ModerationService service,
        int page,
        int pageSize,
        int? minPriority = null)
    {
        var allReports = await service.GetPendingReportsAsync(page, pageSize);

        if (minPriority.HasValue)
        {
            return allReports
                .Where(r => r.Priority >= minPriority.Value)
                .ToList();
        }

        return allReports;
    }

    /// <summary>
    /// Gets reports by status with optional priority filtering.
    /// </summary>
    /// <param name="service">The moderation service instance</param>
    /// <param name="status">Moderation status to filter by</param>
    /// <param name="minPriority">Minimum priority level to include (null for all)</param>
    /// <returns>Filtered list of moderation reports by status</returns>
    public static async Task<List<ModerationReport>> GetReportsByStatusWithPriorityAsync(
        this ModerationService service,
        ModerationStatus status,
        int? minPriority = null)
    {
        var reports = await service.GetReportsByStatusAsync(status);

        if (minPriority.HasValue)
        {
            return reports
                .Where(r => r.Priority >= minPriority.Value)
                .ToList();
        }

        return reports;
    }

    /// <summary>
    /// Gets moderator assignments with optional priority filtering.
    /// </summary>
    /// <param name="service">The moderation service instance</param>
    /// <param name="moderatorId">ID of the moderator to get assignments for</param>
    /// <param name="minPriority">Minimum priority level to include (null for all)</param>
    /// <returns>Filtered list of moderator assignments</returns>
    public static async Task<List<ModerationReport>> GetModeratorAssignmentsByPriorityAsync(
        this ModerationService service,
        Guid moderatorId,
        int? minPriority = null)
    {
        var assignments = await service.GetModeratorAssignmentsAsync(moderatorId);

        if (minPriority.HasValue)
        {
            return assignments
                .Where(r => r.Priority >= minPriority.Value)
                .ToList();
        }

        return assignments;
    }
}