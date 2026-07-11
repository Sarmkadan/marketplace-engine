# ModerationServiceTests

The `ModerationServiceTests` class contains unit tests for the `ModerationService` within the `marketplace-engine` project. It validates the correctness of moderation workflows including user and listing reporting, report assignment, approval, rejection, escalation, and user suspension. Each test method targets a specific scenario—successful operations, expected exceptions, or boundary conditions—to ensure the service behaves as specified.

## API

### `ModerationServiceTests()`
Initializes a new instance of the `ModerationServiceTests` class. Typically used by the test framework to set up shared fixtures (e.g., mock repositories, service instance) before each test run.

### `ReportUserAsync_WhenReporterNotFound_ThrowsResourceNotFoundException`
Tests that reporting a user throws a `ResourceNotFoundException` when the reporter user does not exist in the system.

### `ReportUserAsync_WhenReporterIsInactive_ThrowsUnauthorizedException`
Tests that reporting a user throws an `UnauthorizedException` when the reporter’s account is inactive.

### `ReportUserAsync_WhenReporterEmailNotVerified_ThrowsUnauthorizedException`
Tests that reporting a user throws an `UnauthorizedException` when the reporter’s email address has not been verified.

### `ReportUserAsync_WhenTargetNotFound_ThrowsResourceNotFoundException`
Tests that reporting a user throws a `ResourceNotFoundException` when the target user does not exist.

### `ReportUserAsync_WithValidData_ReturnsSubmittedReport`
Tests that reporting a user with valid data (active reporter, verified email, existing target, valid reason) returns a report in the `Submitted` status.

### `ReportUserAsync_WithShortReason_ThrowsArgumentException`
Tests that reporting a user with a reason string that is too short throws an `ArgumentException`.

### `ReportListingAsync_WhenListingNotFound_ThrowsResourceNotFoundException`
Tests that reporting a listing throws a `ResourceNotFoundException` when the listing does not exist.

### `ReportListingAsync_WithValidData_ReturnsSubmittedReport`
Tests that reporting a listing with valid data returns a report in the `Submitted` status.

### `AssignReportAsync_WhenModeratorNotFound_ThrowsResourceNotFoundException`
Tests that assigning a report throws a `ResourceNotFoundException` when the moderator user does not exist.

### `AssignReportAsync_WhenUserIsRegularUser_ThrowsUnauthorizedException`
Tests that assigning a report throws an `UnauthorizedException` when the assigner is a regular user (not a moderator or administrator).

### `AssignReportAsync_WhenModeratorIsValid_SetsReportInReview`
Tests that assigning a report to a valid moderator sets the report status to `InReview`.

### `AssignReportAsync_WhenAdministrator_SetsReportInReview`
Tests that assigning a report to an administrator sets the report status to `InReview`.

### `ApproveReportAsync_WhenNotAssigned_ThrowsInvalidOperationException`
Tests that approving a report throws an `InvalidOperationException` when the report has not been assigned to any moderator.

### `ApproveReportAsync_WhenAssigned_SetsStatusApproved`
Tests that approving an assigned report sets its status to `Approved`.

### `RejectReportAsync_WhenNotAssigned_ThrowsInvalidOperationException`
Tests that rejecting a report throws an `InvalidOperationException` when the report has not been assigned.

### `RejectReportAsync_WhenAssigned_SetsStatusRejected`
Tests that rejecting an assigned report sets its status to `Rejected`.

### `EscalateReportAsync_WhenPriorityBelow5_IncreasesPriority`
Tests that escalating a report with a priority less than 5 increases its priority by one.

### `EscalateReportAsync_WhenAlreadyAtMaxPriority_DoesNotExceed5`
Tests that escalating a report already at priority 5 does not increase the priority beyond 5.

### `SuspendUserAsync_WhenTargetUserExists_DeactivatesUser`
Tests that suspending an existing user deactivates their account (sets `IsActive` to `false`).

## Usage

The following examples illustrate typical implementations of two test methods from this class. They assume the use of xUnit and Moq for mocking dependencies.

**Example 1: Success path for reporting a user**

```csharp
[Fact]
public async Task ReportUserAsync_WithValidData_ReturnsSubmittedReport()
{
    // Arrange
    var reporter = new User { Id = 1, IsActive = true, EmailVerified = true };
    var target = new User { Id = 2 };
    var reason = "Spam content";
    _userRepositoryMock.Setup(r => r.GetByIdAsync(reporter.Id)).ReturnsAsync(reporter);
    _userRepositoryMock.Setup(r => r.GetByIdAsync(target.Id)).ReturnsAsync(target);
    _reportRepositoryMock.Setup(r => r.AddAsync(It.IsAny<Report>()))
        .ReturnsAsync((Report r) => r);

    // Act
    var result = await _moderationService.ReportUserAsync(reporter.Id, target.Id, reason);

    // Assert
    Assert.NotNull(result);
    Assert.Equal(ReportStatus.Submitted, result.Status);
}
```

**Example 2: Exception when moderator is not found**

```csharp
[Fact]
public async Task AssignReportAsync_WhenModeratorNotFound_ThrowsResourceNotFoundException()
{
    // Arrange
    var reportId = 10;
    var moderatorId = 99;
    _userRepositoryMock.Setup(r => r.GetByIdAsync(moderatorId)).ReturnsAsync((User)null);

    // Act & Assert
    await Assert.ThrowsAsync<ResourceNotFoundException>(() =>
        _moderationService.AssignReportAsync(reportId, moderatorId));
}
```

## Notes

- **Edge cases**: The test suite covers scenarios where input data is missing or invalid (e.g., non‑existent users, inactive accounts, unverified emails, short reasons, unassigned reports). The escalation tests specifically verify that priority does not exceed the maximum value of 5, and that increments only occur when below that threshold.
- **Thread safety**: Each test method is parameterless and relies on instance‑level mock objects. The xUnit framework creates a new instance of the test class for each test, so no mutable state is shared across tests. Parallel execution is safe provided that the underlying mock repositories and service instance are not reused across test classes. No additional synchronization is required within the test methods themselves.
