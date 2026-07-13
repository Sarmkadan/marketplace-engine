#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using MarketplaceEngine.Data;
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
    private readonly MarketplaceDbContext _context;
    private readonly object _reportsLock = new();

    public ModerationService(IUserRepository userRepository, IListingRepository listingRepository)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _listingRepository = listingRepository ?? throw new ArgumentNullException(nameof(listingRepository));
        _context = MarketplaceDbContext.GetInstance();
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

        lock (_reportsLock)
        {
            _context.ModerationReports.Add(report);
        }

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

        lock (_reportsLock)
        {
            _context.ModerationReports.Add(report);
        }

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

    // Applies a moderation action to a single listing as part of a bulk operation
    public async Task ApplyBulkActionAsync(Guid listingId, string action)
    {
        var listing = await _listingRepository.GetByIdAsync(listingId);
        if (listing is null)
            throw new ResourceNotFoundException("Listing", listingId);

        switch (action.Trim().ToLowerInvariant())
        {
            case "approve":
                listing.Status = Domain.Enums.ListingStatus.Active;
                listing.UpdatedAt = DateTime.UtcNow;
                await _listingRepository.UpdateAsync(listing);
                break;
            case "remove":
                listing.Flag();
                await _listingRepository.UpdateAsync(listing);
                break;
            case "escalate":
                listing.FlagForReview("Escalated via bulk moderation");
                await _listingRepository.UpdateAsync(listing);
                break;
            default:
                throw new Exceptions.ValidationException("Action",
                    $"Unknown action '{action}'. Valid values: approve, remove, escalate");
        }
    }

    // Gets pending reports, most recently created first
    public async Task<List<ModerationReport>> GetPendingReportsAsync(int page, int pageSize)
    {
        if (page < 1)
            throw new ArgumentOutOfRangeException(nameof(page), "Page must be greater than or equal to 1.");
        if (pageSize < 1)
            throw new ArgumentOutOfRangeException(nameof(pageSize), "Page size must be greater than or equal to 1.");

        await Task.Delay(5);
        lock (_reportsLock)
        {
            return _context.ModerationReports
                .Where(r => r.Status == ModerationStatus.Pending)
                .OrderByDescending(r => r.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();
        }
    }

    // Gets a moderation report by id
    public async Task<ModerationReport?> GetReportAsync(Guid id)
    {
        await Task.Delay(5);
        lock (_reportsLock)
        {
            return _context.ModerationReports.FirstOrDefault(r => r.Id == id);
        }
    }

    // Updates an existing moderation report's state
    public async Task<ModerationReport> UpdateReportAsync(ModerationReport report)
    {
        ArgumentNullException.ThrowIfNull(report);

        await Task.Delay(5);
        lock (_reportsLock)
        {
            var index = _context.ModerationReports.FindIndex(r => r.Id == report.Id);
            if (index < 0)
                throw new ResourceNotFoundException("ModerationReport", report.Id);

            report.UpdatedAt = DateTime.UtcNow;
            _context.ModerationReports[index] = report;
        }

        return report;
    }

    // Persists a newly created moderation report
    public async Task<ModerationReport> CreateReportAsync(ModerationReport report)
    {
        ArgumentNullException.ThrowIfNull(report);

        await Task.Delay(5);
        lock (_reportsLock)
        {
            if (report.Id == Guid.Empty)
                report.Id = Guid.NewGuid();

            _context.ModerationReports.Add(report);
        }

        return report;
    }

    // Gets reports by status
    public async Task<List<ModerationReport>> GetReportsByStatusAsync(ModerationStatus status)
    {
        await Task.Delay(5);
        lock (_reportsLock)
        {
            return _context.ModerationReports
                .Where(r => r.Status == status)
                .OrderByDescending(r => r.CreatedAt)
                .ToList();
        }
    }

    // Gets reports assigned to moderator
    public async Task<List<ModerationReport>> GetModeratorAssignmentsAsync(Guid moderatorId)
    {
        var moderator = await _userRepository.GetByIdAsync(moderatorId);
        if (moderator is null)
            throw new ResourceNotFoundException("User", moderatorId);

        await Task.Delay(5);
        lock (_reportsLock)
        {
            return _context.ModerationReports
                .Where(r => r.ReviewedBy == moderatorId)
                .OrderByDescending(r => r.CreatedAt)
                .ToList();
        }
    }

    // Gets report statistics: counts of pending, in-review, and resolved reports
    public async Task<(int pending, int inReview, int resolved)> GetReportStatsAsync()
    {
        await Task.Delay(5);
        lock (_reportsLock)
        {
            var pending = _context.ModerationReports.Count(r => r.Status == ModerationStatus.Pending);
            var inReview = _context.ModerationReports.Count(r => r.Status == ModerationStatus.InReview);
            var resolved = _context.ModerationReports.Count(r =>
                r.Status is ModerationStatus.Approved or ModerationStatus.Rejected or ModerationStatus.ContentRemoved);

            return (pending, inReview, resolved);
        }
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
