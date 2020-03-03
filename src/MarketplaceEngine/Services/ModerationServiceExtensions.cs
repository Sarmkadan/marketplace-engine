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
    /// <summary>User target type</summary>
    User,

    /// <summary>Listing target type</summary>
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
    /// <exception cref="ArgumentNullException"><paramref name="service"/> is <see langword="null"/></exception>
    /// <exception cref="ArgumentNullException"><paramref name="listingIds"/> is <see langword="null"/></exception>
    /// <exception cref="ArgumentException"><paramref name="reason"/> is <see langword="null"/> or whitespace</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="priority"/> is less than 1 or greater than 5</exception>
    /// <returns>List of created moderation reports</returns>
    public static async Task<List<ModerationReport>> ReportListingsBatchAsync(
        this ModerationService service,
        Guid reporterId,
        IEnumerable<Guid> listingIds,
        string reason,
        string? details = null,
        int priority = 1)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentNullException.ThrowIfNull(listingIds);
        ArgumentException.ThrowIfNullOrWhiteSpace(reason);

        if (priority < 1 || priority > 5)
        {
            throw new ArgumentOutOfRangeException(nameof(priority), priority, "Priority must be between 1 and 5");
        }

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
    /// <exception cref="ArgumentNullException"><paramref name="service"/> is <see langword="null"/></exception>
    /// <exception cref="ArgumentNullException"><paramref name="userIds"/> is <see langword="null"/></exception>
    /// <exception cref="ArgumentException"><paramref name="reason"/> is <see langword="null"/> or whitespace</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="priority"/> is less than 1 or greater than 5</exception>
    /// <returns>List of created moderation reports</returns>
    public static async Task<List<ModerationReport>> ReportUsersBatchAsync(
        this ModerationService service,
        Guid reporterId,
        IEnumerable<Guid> userIds,
        string reason,
        string? details = null,
        int priority = 1)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentNullException.ThrowIfNull(userIds);
        ArgumentException.ThrowIfNullOrWhiteSpace(reason);

        if (priority < 1 || priority > 5)
        {
            throw new ArgumentOutOfRangeException(nameof(priority), priority, "Priority must be between 1 and 5");
        }

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
    /// <exception cref="ArgumentNullException"><paramref name="service"/> is <see langword="null"/></exception>
    /// <exception cref="ArgumentNullException"><paramref name="reports"/> is <see langword="null"/></exception>
    /// <exception cref="ArgumentException"><paramref name="moderatorId"/> is <see cref="Guid"/>.Empty</exception>
    /// <returns>List of assigned moderation reports</returns>
    public static async Task<List<ModerationReport>> AssignReportsToModeratorAsync(
        this ModerationService service,
        IEnumerable<ModerationReport> reports,
        Guid moderatorId)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentNullException.ThrowIfNull(reports);
        if (moderatorId == Guid.Empty)
        {
            throw new ArgumentException("Moderator ID cannot be empty", nameof(moderatorId));
        }

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
    /// <exception cref="ArgumentNullException"><paramref name="service"/> is <see langword="null"/></exception>
    /// <exception cref="ArgumentNullException"><paramref name="reports"/> is <see langword="null"/></exception>
    /// <exception cref="ArgumentException"><paramref name="action"/> is <see langword="null"/>, whitespace, or invalid</exception>
    /// <returns>List of processed moderation reports</returns>
    public static async Task<List<ModerationReport>> ProcessReportsBatchAsync(
        this ModerationService service,
        IEnumerable<ModerationReport> reports,
        string action,
        string? reviewNotes = null)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentNullException.ThrowIfNull(reports);
        ArgumentException.ThrowIfNullOrWhiteSpace(action);

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
                    var bannedReport = await service.BanUserAsync(report, reviewNotes ?? string.Empty);
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
    /// <exception cref="ArgumentNullException"><paramref name="service"/> is <see langword="null"/></exception>
    /// <exception cref="ArgumentNullException"><paramref name="reports"/> is <see langword="null"/></exception>
    /// <returns>List of escalated moderation reports</returns>
    public static async Task<List<ModerationReport>> EscalateReportsPriorityAsync(
        this ModerationService service,
        IEnumerable<ModerationReport> reports)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentNullException.ThrowIfNull(reports);

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
    /// <exception cref="ArgumentNullException"><paramref name="service"/> is <see langword="null"/></exception>
    /// <exception cref="ArgumentNullException"><paramref name="reports"/> is <see langword="null"/></exception>
    /// <returns>Filtered list of moderation reports</returns>
    public static async Task<List<ModerationReport>> FilterByTargetTypeAsync(
        this ModerationService service,
        IEnumerable<ModerationReport> reports,
        ModerationTargetType targetType)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentNullException.ThrowIfNull(reports);

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
    /// <exception cref="ArgumentNullException"><paramref name="service"/> is <see langword="null"/></exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="page"/> is less than 0 or <paramref name="pageSize"/> is less than 1</exception>
    /// <returns>Filtered list of pending moderation reports</returns>
    public static async Task<List<ModerationReport>> GetPendingReportsByPriorityAsync(
        this ModerationService service,
        int page,
        int pageSize,
        int? minPriority = null)
    {
        ArgumentNullException.ThrowIfNull(service);
        if (page < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(page), page, "Page must be non-negative");
        }
        if (pageSize < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(pageSize), pageSize, "Page size must be at least 1");
        }

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
    /// <exception cref="ArgumentNullException"><paramref name="service"/> is <see langword="null"/></exception>
    /// <returns>Filtered list of moderation reports by status</returns>
    public static async Task<List<ModerationReport>> GetReportsByStatusWithPriorityAsync(
        this ModerationService service,
        ModerationStatus status,
        int? minPriority = null)
    {
        ArgumentNullException.ThrowIfNull(service);

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
    /// <exception cref="ArgumentNullException"><paramref name="service"/> is <see langword="null"/></exception>
    /// <exception cref="ArgumentException"><paramref name="moderatorId"/> is <see cref="Guid"/>.Empty</exception>
    /// <returns>Filtered list of moderator assignments</returns>
    public static async Task<List<ModerationReport>> GetModeratorAssignmentsByPriorityAsync(
        this ModerationService service,
        Guid moderatorId,
        int? minPriority = null)
    {
        ArgumentNullException.ThrowIfNull(service);
        if (moderatorId == Guid.Empty)
        {
            throw new ArgumentException("Moderator ID cannot be empty", nameof(moderatorId));
        }

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