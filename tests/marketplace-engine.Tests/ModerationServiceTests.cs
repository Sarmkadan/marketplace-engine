#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FluentAssertions;
using MarketplaceEngine.Domain.Enums;
using MarketplaceEngine.Domain.Models;
using MarketplaceEngine.Exceptions;
using MarketplaceEngine.Repositories;
using MarketplaceEngine.Services;
using Moq;
using Xunit;

namespace MarketplaceEngine.Tests;

/// <summary>
/// Contains unit tests for the <see cref="ModerationService"/> class, validating moderation workflows
/// such as reporting users and listings, assigning/approving/rejecting reports, and applying bulk actions.
/// </summary>
public class ModerationServiceTests
{
    private readonly Mock<IUserRepository> _userRepoMock;
    private readonly Mock<IListingRepository> _listingRepoMock;
    private readonly ModerationService _sut;

    /// <summary>
    /// Initializes a new instance of the <see cref="ModerationServiceTests"/> class.
    /// Sets up mocks for <see cref="IUserRepository"/> and <see cref="IListingRepository"/>
    /// and initializes the system under test (<see cref="ModerationService"/>).
    /// </summary>
    public ModerationServiceTests()
    {
        _userRepoMock = new Mock<IUserRepository>();
        _listingRepoMock = new Mock<IListingRepository>();
        _sut = new ModerationService(_userRepoMock.Object, _listingRepoMock.Object);
    }

    private static User MakeActiveVerifiedUser(Guid? id = null) => new()
    {
        Id = id ?? Guid.NewGuid(),
        Email = $"{Guid.NewGuid()}@example.com",
        FullName = "Active Verified User",
        IsActive = true,
        IsVerified = true,
        Role = UserRole.User
    };

    // ── ReportUserAsync ────────────────────────────────────────────────────

    /// <summary>
    /// Validates that <see cref="ModerationService.ReportUserAsync"/> throws <see cref="ResourceNotFoundException"/>
    /// when the reporting user is not found.
    /// </summary>
    [Fact]
    public async Task ReportUserAsync_WhenReporterNotFound_ThrowsResourceNotFoundException()
    {
        var reporterId = Guid.NewGuid();
        _userRepoMock.Setup(r => r.GetByIdAsync(reporterId)).ReturnsAsync((User?)null);

        var act = async () => await _sut.ReportUserAsync(reporterId, Guid.NewGuid(), "Spam content here");

        await act.Should().ThrowAsync<ResourceNotFoundException>();
    }

    /// <summary>
    /// Validates that <see cref="ModerationService.ReportUserAsync"/> throws <see cref="UnauthorizedException"/>
    /// when the reporting user is not active.
    /// </summary>
    [Fact]
    public async Task ReportUserAsync_WhenReporterIsInactive_ThrowsUnauthorizedException()
    {
        var reporter = MakeActiveVerifiedUser();
        reporter.IsActive = false;

        _userRepoMock.Setup(r => r.GetByIdAsync(reporter.Id)).ReturnsAsync(reporter);

        var act = async () => await _sut.ReportUserAsync(reporter.Id, Guid.NewGuid(), "Spam content here");

        await act.Should().ThrowAsync<UnauthorizedException>().WithMessage("*file reports*");
    }

    /// <summary>
    /// Validates that <see cref="ModerationService.ReportUserAsync"/> throws <see cref="UnauthorizedException"/>
    /// when the reporter's email is not verified.
    /// </summary>
    [Fact]
    public async Task ReportUserAsync_WhenReporterEmailNotVerified_ThrowsUnauthorizedException()
    {
        var reporter = MakeActiveVerifiedUser();
        reporter.IsVerified = false;

        _userRepoMock.Setup(r => r.GetByIdAsync(reporter.Id)).ReturnsAsync(reporter);

        var act = async () => await _sut.ReportUserAsync(reporter.Id, Guid.NewGuid(), "Spam content here");

        await act.Should().ThrowAsync<UnauthorizedException>().WithMessage("*email not verified*");
    }

    /// <summary>
    /// Validates that <see cref="ModerationService.ReportUserAsync"/> throws <see cref="ResourceNotFoundException"/>
    /// when the target user to be reported is not found.
    /// </summary>
    [Fact]
    public async Task ReportUserAsync_WhenTargetNotFound_ThrowsResourceNotFoundException()
    {
        var reporter = MakeActiveVerifiedUser();
        var targetId = Guid.NewGuid();

        _userRepoMock.Setup(r => r.GetByIdAsync(reporter.Id)).ReturnsAsync(reporter);
        _userRepoMock.Setup(r => r.GetByIdAsync(targetId)).ReturnsAsync((User?)null);

        var act = async () => await _sut.ReportUserAsync(reporter.Id, targetId, "Spam content here");

        await act.Should().ThrowAsync<ResourceNotFoundException>();
    }

    /// <summary>
    /// Validates that <see cref="ModerationService.ReportUserAsync"/> returns a pending report
    /// when provided with valid data for a valid reporter and target.
    /// </summary>
    [Fact]
    public async Task ReportUserAsync_WithValidData_ReturnsSubmittedReport()
    {
        var reporter = MakeActiveVerifiedUser();
        var target = MakeActiveVerifiedUser();

        _userRepoMock.Setup(r => r.GetByIdAsync(reporter.Id)).ReturnsAsync(reporter);
        _userRepoMock.Setup(r => r.GetByIdAsync(target.Id)).ReturnsAsync(target);

        var result = await _sut.ReportUserAsync(reporter.Id, target.Id, "Harassment and spam");

        result.Should().NotBeNull();
        result.ReporterId.Should().Be(reporter.Id);
        result.TargetUserId.Should().Be(target.Id);
        result.Status.Should().Be(ModerationStatus.Pending);
    }

    /// <summary>
    /// Validates that <see cref="ModerationService.ReportUserAsync"/> throws <see cref="ArgumentException"/>
    /// when the provided reason for the report is too short.
    /// </summary>
    [Fact]
    public async Task ReportUserAsync_WithShortReason_ThrowsArgumentException()
    {
        var reporter = MakeActiveVerifiedUser();
        var target = MakeActiveVerifiedUser();

        _userRepoMock.Setup(r => r.GetByIdAsync(reporter.Id)).ReturnsAsync(reporter);
        _userRepoMock.Setup(r => r.GetByIdAsync(target.Id)).ReturnsAsync(target);

        var act = async () => await _sut.ReportUserAsync(reporter.Id, target.Id, "Bad");

        await act.Should().ThrowAsync<ArgumentException>().WithMessage("*Reason*");
    }

    // ── ReportListingAsync ─────────────────────────────────────────────────

    /// <summary>
    /// Validates that <see cref="ModerationService.ReportListingAsync"/> throws <see cref="ResourceNotFoundException"/>
    /// when the listing to be reported is not found.
    /// </summary>
    [Fact]
    public async Task ReportListingAsync_WhenListingNotFound_ThrowsResourceNotFoundException()
    {
        var reporter = MakeActiveVerifiedUser();
        var listingId = Guid.NewGuid();

        _userRepoMock.Setup(r => r.GetByIdAsync(reporter.Id)).ReturnsAsync(reporter);
        _listingRepoMock.Setup(r => r.GetByIdAsync(listingId)).ReturnsAsync((Listing?)null);

        var act = async () => await _sut.ReportListingAsync(reporter.Id, listingId, "Fraudulent listing");

        await act.Should().ThrowAsync<ResourceNotFoundException>().WithMessage("*Listing*not found*");
    }

    /// <summary>
    /// Validates that <see cref="ModerationService.ReportListingAsync"/> returns a pending report
    /// when provided with valid data.
    /// </summary>
    [Fact]
    public async Task ReportListingAsync_WithValidData_ReturnsSubmittedReport()
    {
        var reporter = MakeActiveVerifiedUser();
        var listing = new Listing
        {
            Id = Guid.NewGuid(),
            SellerId = Guid.NewGuid(),
            Title = "Suspicious listing",
            Description = "This looks fraudulent",
            Status = ListingStatus.Active
        };

        _userRepoMock.Setup(r => r.GetByIdAsync(reporter.Id)).ReturnsAsync(reporter);
        _listingRepoMock.Setup(r => r.GetByIdAsync(listing.Id)).ReturnsAsync(listing);

        var result = await _sut.ReportListingAsync(reporter.Id, listing.Id, "Fraudulent listing found");

        result.Should().NotBeNull();
        result.TargetListingId.Should().Be(listing.Id);
        result.Status.Should().Be(ModerationStatus.Pending);
    }

    // ── AssignReportAsync ──────────────────────────────────────────────────

    /// <summary>
    /// Validates that <see cref="ModerationService.AssignReportAsync"/> throws <see cref="ResourceNotFoundException"/>
    /// when the moderator to be assigned to the report is not found.
    /// </summary>
    [Fact]
    public async Task AssignReportAsync_WhenModeratorNotFound_ThrowsResourceNotFoundException()
    {
        var report = new ModerationReport
        {
            Id = Guid.NewGuid(),
            ReporterId = Guid.NewGuid(),
            TargetUserId = Guid.NewGuid(),
            Reason = "Spam"
        };
        var moderatorId = Guid.NewGuid();
        _userRepoMock.Setup(r => r.GetByIdAsync(moderatorId)).ReturnsAsync((User?)null);

        var act = async () => await _sut.AssignReportAsync(report, moderatorId);

        await act.Should().ThrowAsync<ResourceNotFoundException>();
    }

    /// <summary>
    /// Validates that <see cref="ModerationService.AssignReportAsync"/> throws <see cref="UnauthorizedException"/>
    /// when the user assigned to the report is not a moderator or administrator.
    /// </summary>
    [Fact]
    public async Task AssignReportAsync_WhenUserIsRegularUser_ThrowsUnauthorizedException()
    {
        var report = new ModerationReport
        {
            Id = Guid.NewGuid(),
            ReporterId = Guid.NewGuid(),
            TargetUserId = Guid.NewGuid(),
            Reason = "Spam"
        };
        var regularUser = MakeActiveVerifiedUser();
        regularUser.Role = UserRole.User;

        _userRepoMock.Setup(r => r.GetByIdAsync(regularUser.Id)).ReturnsAsync(regularUser);

        var act = async () => await _sut.AssignReportAsync(report, regularUser.Id);

        await act.Should().ThrowAsync<UnauthorizedException>().WithMessage("*moderation reports*");
    }

    /// <summary>
    /// Validates that <see cref="ModerationService.AssignReportAsync"/> sets the report status
    /// to <see cref="ModerationStatus.InReview"/> and assigns the moderator when the moderator is valid.
    /// </summary>
    [Fact]
    public async Task AssignReportAsync_WhenModeratorIsValid_SetsReportInReview()
    {
        var report = new ModerationReport
        {
            Id = Guid.NewGuid(),
            ReporterId = Guid.NewGuid(),
            TargetUserId = Guid.NewGuid(),
            Reason = "Harassment"
        };
        var moderator = MakeActiveVerifiedUser();
        moderator.Role = UserRole.Moderator;

        _userRepoMock.Setup(r => r.GetByIdAsync(moderator.Id)).ReturnsAsync(moderator);

        var result = await _sut.AssignReportAsync(report, moderator.Id);

        result.Status.Should().Be(ModerationStatus.InReview);
        result.ReviewedBy.Should().Be(moderator.Id);
    }

    /// <summary>
    /// Validates that <see cref="ModerationService.AssignReportAsync"/> sets the report status
    /// to <see cref="ModerationStatus.InReview"/> when assigned to an administrator.
    /// </summary>
    [Fact]
    public async Task AssignReportAsync_WhenAdministrator_SetsReportInReview()
    {
        var report = new ModerationReport
        {
            Id = Guid.NewGuid(),
            ReporterId = Guid.NewGuid(),
            TargetUserId = Guid.NewGuid(),
            Reason = "Serious violation"
        };
        var admin = MakeActiveVerifiedUser();
        admin.Role = UserRole.Administrator;

        _userRepoMock.Setup(r => r.GetByIdAsync(admin.Id)).ReturnsAsync(admin);

        var result = await _sut.AssignReportAsync(report, admin.Id);

        result.Status.Should().Be(ModerationStatus.InReview);
    }

    // ── ApproveReportAsync ─────────────────────────────────────────────────

    /// <summary>
    /// Validates that <see cref="ModerationService.ApproveReportAsync"/> throws <see cref="InvalidOperationException"/>
    /// when the report has not been assigned to anyone for review.
    /// </summary>
    [Fact]
    public async Task ApproveReportAsync_WhenNotAssigned_ThrowsInvalidOperationException()
    {
        var report = new ModerationReport
        {
            Id = Guid.NewGuid(),
            ReporterId = Guid.NewGuid(),
            TargetUserId = Guid.NewGuid(),
            Reason = "Spam",
            ReviewedBy = Guid.Empty
        };

        var act = async () => await _sut.ApproveReportAsync(report);

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*assigned*");
    }

    /// <summary>
    /// Validates that <see cref="ModerationService.ApproveReportAsync"/> sets the report status
    /// to <see cref="ModerationStatus.Approved"/> and updates the review notes when successfully approved.
    /// </summary>
    [Fact]
    public async Task ApproveReportAsync_WhenAssigned_SetsStatusApproved()
    {
        var moderatorId = Guid.NewGuid();
        var report = new ModerationReport
        {
            Id = Guid.NewGuid(),
            ReporterId = Guid.NewGuid(),
            TargetUserId = Guid.NewGuid(),
            Reason = "Spam content",
            ReviewedBy = moderatorId,
            Status = ModerationStatus.InReview
        };

        var result = await _sut.ApproveReportAsync(report, "Confirmed spam");

        result.Status.Should().Be(ModerationStatus.Approved);
        result.ReviewNotes.Should().Be("Confirmed spam");
        result.ResolvedAt.Should().NotBeNull();
    }

    // ── RejectReportAsync ──────────────────────────────────────────────────

    /// <summary>
    /// Validates that <see cref="ModerationService.RejectReportAsync"/> throws <see cref="InvalidOperationException"/>
    /// when the report has not been assigned to anyone for review.
    /// </summary>
    [Fact]
    public async Task RejectReportAsync_WhenNotAssigned_ThrowsInvalidOperationException()
    {
        var report = new ModerationReport
        {
            Id = Guid.NewGuid(),
            ReporterId = Guid.NewGuid(),
            TargetUserId = Guid.NewGuid(),
            Reason = "Some reason",
            ReviewedBy = Guid.Empty
        };

        var act = async () => await _sut.RejectReportAsync(report);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    /// <summary>
    /// Validates that <see cref="ModerationService.RejectReportAsync"/> sets the report status
    /// to <see cref="ModerationStatus.Rejected"/> and updates the review notes when successfully rejected.
    /// </summary>
    [Fact]
    public async Task RejectReportAsync_WhenAssigned_SetsStatusRejected()
    {
        var report = new ModerationReport
        {
            Id = Guid.NewGuid(),
            ReporterId = Guid.NewGuid(),
            TargetUserId = Guid.NewGuid(),
            Reason = "Some reason",
            ReviewedBy = Guid.NewGuid(),
            Status = ModerationStatus.InReview
        };

        var result = await _sut.RejectReportAsync(report, "No violation found");

        result.Status.Should().Be(ModerationStatus.Rejected);
        result.ReviewNotes.Should().Be("No violation found");
    }

    // ── EscalateReportAsync ────────────────────────────────────────────────

    /// <summary>
    /// Validates that <see cref="ModerationService.EscalateReportAsync"/> increments the report priority
    /// if the current priority is below 5.
    /// </summary>
    [Fact]
    public void EscalateReportAsync_WhenPriorityBelow5_IncreasesPriority()
    {
        var report = new ModerationReport
        {
            Id = Guid.NewGuid(),
            ReporterId = Guid.NewGuid(),
            TargetUserId = Guid.NewGuid(),
            Reason = "Critical issue",
            Priority = 2
        };

        var result = _sut.EscalateReportAsync(report);

        result.Priority.Should().Be(3);
    }

    /// <summary>
    /// Validates that <see cref="ModerationService.EscalateReportAsync"/> keeps the report priority at 5
    /// if it is already at the maximum value of 5.
    /// </summary>
    [Fact]
    public void EscalateReportAsync_WhenAlreadyAtMaxPriority_DoesNotExceed5()
    {
        var report = new ModerationReport
        {
            Id = Guid.NewGuid(),
            ReporterId = Guid.NewGuid(),
            TargetUserId = Guid.NewGuid(),
            Reason = "Critical issue",
            Priority = 5
        };

        var result = _sut.EscalateReportAsync(report);

        result.Priority.Should().Be(5);
    }

    // ── SuspendUserAsync ───────────────────────────────────────────────────

    /// <summary>
    /// Validates that <see cref="ModerationService.SuspendUserAsync"/> deactivates the target user
    /// and sets the report status to <see cref="ModerationStatus.UserSuspended"/>.
    /// </summary>
    [Fact]
    public async Task SuspendUserAsync_WhenTargetUserExists_DeactivatesUser()
    {
        var targetUser = MakeActiveVerifiedUser();
        var report = new ModerationReport
        {
            Id = Guid.NewGuid(),
            ReporterId = Guid.NewGuid(),
            TargetUserId = targetUser.Id,
            Reason = "Multiple violations",
            ReviewedBy = Guid.NewGuid()
        };

        _userRepoMock.Setup(r => r.GetByIdAsync(targetUser.Id)).ReturnsAsync(targetUser);
        _userRepoMock.Setup(r => r.UpdateAsync(It.IsAny<User>())).ReturnsAsync((User u) => u);

        var result = await _sut.SuspendUserAsync(report, "Repeated spam");

        result.Status.Should().Be(ModerationStatus.UserSuspended);
        _userRepoMock.Verify(r => r.UpdateAsync(It.Is<User>(u => !u.IsActive)), Times.Once);
    }

    // ── ApplyBulkActionAsync ───────────────────────────────────────────────

    /// <summary>
    /// Validates that <see cref="ModerationService.ApplyBulkActionAsync"/> sets the listing status to
    /// <see cref="ListingStatus.Active"/> when the "approve" action is provided.
    /// </summary>
    [Fact]
    public async Task ApplyBulkActionAsync_WithApproveAction_SetsListingActive()
    {
        var listing = new Listing
        {
            Id = Guid.NewGuid(),
            SellerId = Guid.NewGuid(),
            Title = "Under review listing",
            Status = ListingStatus.UnderReview
        };

        _listingRepoMock.Setup(r => r.GetByIdAsync(listing.Id)).ReturnsAsync(listing);
        _listingRepoMock.Setup(r => r.UpdateAsync(It.IsAny<Listing>())).ReturnsAsync((Listing l) => l);

        await _sut.ApplyBulkActionAsync(listing.Id, "approve");

        _listingRepoMock.Verify(r => r.UpdateAsync(It.Is<Listing>(l => l.Status == ListingStatus.Active)), Times.Once);
    }

    /// <summary>
    /// Validates that <see cref="ModerationService.ApplyBulkActionAsync"/> sets the listing status to
    /// <see cref="ListingStatus.Flagged"/> when the "remove" action is provided.
    /// </summary>
    [Fact]
    public async Task ApplyBulkActionAsync_WithRemoveAction_FlagsListing()
    {
        var listing = new Listing
        {
            Id = Guid.NewGuid(),
            SellerId = Guid.NewGuid(),
            Title = "Flagged listing",
            Status = ListingStatus.Active
        };

        _listingRepoMock.Setup(r => r.GetByIdAsync(listing.Id)).ReturnsAsync(listing);
        _listingRepoMock.Setup(r => r.UpdateAsync(It.IsAny<Listing>())).ReturnsAsync((Listing l) => l);

        await _sut.ApplyBulkActionAsync(listing.Id, "remove");

        _listingRepoMock.Verify(r => r.UpdateAsync(It.Is<Listing>(l => l.Status == ListingStatus.Flagged)), Times.Once);
    }

    /// <summary>
    /// Validates that <see cref="ModerationService.ApplyBulkActionAsync"/> throws <see cref="Exceptions.ValidationException"/>
    /// when an unknown bulk action is provided.
    /// </summary>
    [Fact]
    public async Task ApplyBulkActionAsync_WithUnknownAction_ThrowsValidationException()
    {
        var listing = new Listing
        {
            Id = Guid.NewGuid(),
            SellerId = Guid.NewGuid(),
            Title = "Some listing",
            Status = ListingStatus.Active
        };

        _listingRepoMock.Setup(r => r.GetByIdAsync(listing.Id)).ReturnsAsync(listing);

        var act = async () => await _sut.ApplyBulkActionAsync(listing.Id, "delete_everything");

        await act.Should().ThrowAsync<Exceptions.ValidationException>().WithMessage("*Unknown action*");
    }

    /// <summary>
    /// Validates that <see cref="ModerationService.ApplyBulkActionAsync"/> throws <see cref="ResourceNotFoundException"/>
    /// when the target listing for the bulk action is not found.
    /// </summary>
    [Fact]
    public async Task ApplyBulkActionAsync_WhenListingNotFound_ThrowsResourceNotFoundException()
    {
        var listingId = Guid.NewGuid();
        _listingRepoMock.Setup(r => r.GetByIdAsync(listingId)).ReturnsAsync((Listing?)null);

        var act = async () => await _sut.ApplyBulkActionAsync(listingId, "approve");

        await act.Should().ThrowAsync<ResourceNotFoundException>();
    }
}
