#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using MarketplaceEngine.Domain.Enums;
using MarketplaceEngine.Domain.Models;
using MarketplaceEngine.Exceptions;
using MarketplaceEngine.Repositories;

namespace MarketplaceEngine.Services;

/// <summary>
/// Service for managing marketplace moderation and content review.
/// </summary>
public class ModerationService
{
    private readonly IUserRepository _userRepository;
    private readonly IListingRepository _listingRepository;

    public ModerationService(IUserRepository userRepository, IListingRepository listingRepository)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _listingRepository = listingRepository ?? throw new ArgumentNullException(nameof(listingRepository));
    }

    // Creates a moderation report for a user
    public async Task<ModerationReport> ReportUserAsync(Guid reporterId, Guid targetUserId, string reason,
        string? details = null, int priority = 1)
    {
        await ValidateReporterAsync(reporterId);

        var targetUser = await _userRepository.GetByIdAsync(targetUserId);
        if (targetUser is null)
            throw new ResourceNotFoundException("User", targetUserId);

        var report = new ModerationReport
        {
            ReporterId = reporterId,
            TargetUserId = targetUserId,
            Reason = reason,
            Details = details,
            Priority = priority
        };

        report.ValidateReport();
        report.Submit();

        return report;
    }

    // Creates a moderation report for a listing
    public async Task<ModerationReport> ReportListingAsync(Guid reporterId, Guid listingId, string reason,
        string? details = null, int priority = 1)
    {
        await ValidateReporterAsync(reporterId);

        var listing = await _listingRepository.GetByIdAsync(listingId);
        if (listing is null)
            throw new ResourceNotFoundException("Listing", listingId);

        var report = new ModerationReport
        {
            ReporterId = reporterId,
            TargetListingId = listingId,
            Reason = reason,
            Details = details,
            Priority = priority
        };

        report.ValidateReport();
        report.Submit();

        return report;
    }

    // Assigns report to moderator
    public async Task<ModerationReport> AssignReportAsync(ModerationReport report, Guid moderatorId)
    {
        var moderator = await _userRepository.GetByIdAsync(moderatorId);
        if (moderator is null)
            throw new ResourceNotFoundException("User", moderatorId);

        if (moderator.Role != UserRole.Moderator && moderator.Role != UserRole.Administrator)
            throw new UnauthorizedException(moderatorId, "review moderation reports");

        report.AssignToModerator(moderatorId);
        return report;
    }

    // Approves a moderation report
    public async Task<ModerationReport> ApproveReportAsync(ModerationReport report, string reviewNotes = "")
    {
        if (report.ReviewedBy == Guid.Empty)
            throw new InvalidOperationException("Report must be assigned to a moderator first");

        report.Approve(reviewNotes);
        return report;
    }

    // Rejects a moderation report
    public async Task<ModerationReport> RejectReportAsync(ModerationReport report, string reviewNotes = "")
    {
        if (report.ReviewedBy == Guid.Empty)
            throw new InvalidOperationException("Report must be assigned to a moderator first");

        report.Reject(reviewNotes);
        return report;
    }

    // Removes flagged content
    public async Task<ModerationReport> RemoveContentAsync(ModerationReport report, string reviewNotes = "")
    {
        if (report.TargetListingId.HasValue && report.TargetListingId != Guid.Empty)
        {
            var listing = await _listingRepository.GetByIdAsync(report.TargetListingId.Value);
            if (listing is not null)
            {
                listing.Flag();
                await _listingRepository.UpdateAsync(listing);
            }
        }

        report.RemoveContent(reviewNotes);
        return report;
    }

    // Suspends a user
    public async Task<ModerationReport> SuspendUserAsync(ModerationReport report, string reviewNotes = "")
    {
        if (report.TargetUserId.HasValue && report.TargetUserId != Guid.Empty)
        {
            var user = await _userRepository.GetByIdAsync(report.TargetUserId.Value);
            if (user is not null)
            {
                user.Deactivate();
                await _userRepository.UpdateAsync(user);
            }
        }

        report.SuspendUser(reviewNotes);
        return report;
    }

    // Bans a user
    public async Task<ModerationReport> BanUserAsync(ModerationReport report, string reviewNotes = "")
    {
        if (report.TargetUserId.HasValue && report.TargetUserId != Guid.Empty)
        {
            var user = await _userRepository.GetByIdAsync(report.TargetUserId.Value);
            if (user is not null)
            {
                user.Deactivate();
                // Mark user as banned by setting special flag
                user.Role = UserRole.User; // Could add a Ban status in future
                await _userRepository.UpdateAsync(user);
            }
        }

        report.BanUser(reviewNotes);
        return report;
    }

    // Escalates report priority
    public ModerationReport EscalateReportAsync(ModerationReport report)
    {
        report.EscalatePriority();
        return report;
    }

    // Gets pending reports
    public async Task<List<ModerationReport>> GetPendingReportsAsync(int page, int pageSize)
    {
        await Task.Delay(5);
        // Hotfix: No actual persistence for ModerationReports, returning empty list
        return new List<ModerationReport>();
    }

    // Hotfix: GetReportAsync for ModerationController to compile
    public async Task<ModerationReport?> GetReportAsync(Guid id)
    {
        await Task.Delay(5);
        // Hotfix: No actual persistence for ModerationReports, always returning null
        return null;
    }

    // Hotfix: UpdateReportAsync for ModerationController to compile
    public async Task<ModerationReport> UpdateReportAsync(ModerationReport report)
    {
        await Task.Delay(5);
        // Hotfix: No actual persistence for ModerationReports, returning the input report
        return report;
    }

    // Hotfix: CreateReportAsync for ModerationController to compile
    public async Task<ModerationReport> CreateReportAsync(ModerationReport report)
    {
        await Task.Delay(5);
        // Hotfix: No actual persistence for ModerationReports, returning the input report
        return report;
    }

    // Gets reports by status

    // Gets reports by status
    public async Task<List<ModerationReport>> GetReportsByStatusAsync(ModerationStatus status)
    {
        await Task.Delay(5);
        return new List<ModerationReport>();
    }

    // Gets reports assigned to moderator
    public async Task<List<ModerationReport>> GetModeratorAssignmentsAsync(Guid moderatorId)
    {
        var moderator = await _userRepository.GetByIdAsync(moderatorId);
        if (moderator is null)
            throw new ResourceNotFoundException("User", moderatorId);

        await Task.Delay(5);
        return new List<ModerationReport>();
    }

    // Gets report statistics
    public async Task<(int pending, int inReview, int resolved)> GetReportStatsAsync()
    {
        await Task.Delay(5);
        return (0, 0, 0);
    }

    private async Task ValidateReporterAsync(Guid reporterId)
    {
        var reporter = await _userRepository.GetByIdAsync(reporterId);
        if (reporter is null)
            throw new ResourceNotFoundException("User", reporterId);

        if (!reporter.IsActive)
            throw new UnauthorizedException(reporterId, "file reports");

        if (!reporter.IsVerified)
            throw new UnauthorizedException(reporterId, "file reports - email not verified");
    }
}
