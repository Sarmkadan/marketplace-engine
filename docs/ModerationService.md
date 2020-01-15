# ModerationService

Central service for handling content moderation workflows in the marketplace engine, including user reporting, content removal, user sanctions, and report lifecycle management.

## API

### `ModerationService`

Entry point for moderation operations. Provides methods to create, assign, approve, reject, and manage moderation reports, as well as apply bulk actions and retrieve report statistics.

---

### `public async Task<ModerationReport> ReportUserAsync(string reporterId, string reportedUserId, string reason)`

Reports a user for violating community guidelines.

- **Parameters**
  - `reporterId`: Unique identifier of the user submitting the report.
  - `reportedUserId`: Unique identifier of the user being reported.
  - `reason`: Human-readable description of the violation.
- **Returns**: A `ModerationReport` representing the newly created report.
- **Throws**: `ArgumentException` if `reporterId` or `reportedUserId` is null or empty; `InvalidOperationException` if the reporter is blocked from reporting.

---

### `public async Task<ModerationReport> ReportListingAsync(string reporterId, string listingId, string reason)`

Reports a listing for violating content policies.

- **Parameters**
  - `reporterId`: Unique identifier of the user submitting the report.
  - `listingId`: Unique identifier of the listing being reported.
  - `reason`: Human-readable description of the violation.
- **Returns**: A `ModerationReport` representing the newly created report.
- **Throws**: `ArgumentException` if any parameter is null or empty; `InvalidOperationException` if the reporter is blocked from reporting.

---

### `public async Task<ModerationReport> AssignReportAsync(string moderatorId, int reportId)`

Assigns a pending moderation report to a moderator for review.

- **Parameters**
  - `moderatorId`: Unique identifier of the moderator to assign the report.
  - `reportId`: Unique identifier of the report to assign.
- **Returns**: The updated `ModerationReport` with assignment details.
- **Throws**: `ArgumentException` if `moderatorId` or `reportId` is invalid; `InvalidOperationException` if the report is not in pending status or the moderator lacks permissions.

---

### `public async Task<ModerationReport> ApproveReportAsync(int reportId)`

Approves a moderation report, indicating the reported content or user complies with policies.

- **Parameters**
  - `reportId`: Unique identifier of the report to approve.
- **Returns**: The updated `ModerationReport` with approval status and metadata.
- **Throws**: `ArgumentException` if `reportId` is invalid; `InvalidOperationException` if the report is not in review status.

---
### `public async Task<ModerationReport> RejectReportAsync(int reportId)`

Rejects a moderation report, indicating the reported content or user does not violate policies.

- **Parameters**
  - `reportId`: Unique identifier of the report to reject.
- **Returns**: The updated `ModerationReport` with rejection status and metadata.
- **Throws**: `ArgumentException` if `reportId` is invalid; `InvalidOperationException` if the report is not in review status.

---
### `public async Task<ModerationReport> RemoveContentAsync(int reportId, string moderatorId)`

Removes content associated with a moderation report and marks the report as resolved.

- **Parameters**
  - `reportId`: Unique identifier of the report whose content is to be removed.
  - `moderatorId`: Unique identifier of the moderator performing the action.
- **Returns**: The updated `ModerationReport` with removal confirmation.
- **Throws**: `ArgumentException` if `reportId` or `moderatorId` is invalid; `InvalidOperationException` if the report is not in review status or content is already removed.

---
### `public async Task<ModerationReport> SuspendUserAsync(int reportId, string moderatorId, TimeSpan duration)`

Suspends a user based on a moderation report, preventing access for a specified duration.

- **Parameters**
  - `reportId`: Unique identifier of the report triggering the suspension.
  - `moderatorId`: Unique identifier of the moderator performing the action.
  - `duration`: Time span for which the user is suspended.
- **Returns**: The updated `ModerationReport` with suspension details.
- **Throws**: `ArgumentException` if any parameter is invalid; `InvalidOperationException` if the report is not in review status or the user is already suspended.

---
### `public async Task<ModerationReport> BanUserAsync(int reportId, string moderatorId)`

Bans a user based on a moderation report, permanently removing access.

- **Parameters**
  - `reportId`: Unique identifier of the report triggering the ban.
  - `moderatorId`: Unique identifier of the moderator performing the action.
- **Returns**: The updated `ModerationReport` with ban details.
- **Throws**: `ArgumentException` if any parameter is invalid; `InvalidOperationException` if the report is not in review status or the user is already banned.

---
### `public ModerationReport EscalateReportAsync(int reportId, string moderatorId)`

Escalates a moderation report to a higher authority for further review.

- **Parameters**
  - `reportId`: Unique identifier of the report to escalate.
  - `moderatorId`: Unique identifier of the moderator performing the escalation.
- **Returns**: The updated `ModerationReport` with escalation status.
- **Throws**: `ArgumentException` if any parameter is invalid; `InvalidOperationException` if the report is not in review status or the moderator lacks escalation privileges.

---
### `public async Task ApplyBulkActionAsync(IEnumerable<int> reportIds, string moderatorId, BulkActionType actionType)`

Applies a bulk action (e.g., approve, reject, remove) to multiple reports.

- **Parameters**
  - `reportIds`: Collection of report identifiers to process.
  - `moderatorId`: Unique identifier of the moderator performing the action.
  - `actionType`: Type of bulk action to apply.
- **Returns**: Task representing the asynchronous operation.
- **Throws**: `ArgumentException` if `reportIds` is null or empty or contains invalid IDs; `InvalidOperationException` if any report is not in a valid state for the action or the moderator lacks permissions.

---
### `public async Task<List<ModerationReport>> GetPendingReportsAsync(int limit = 100)`

Retrieves a list of moderation reports currently in pending status.

- **Parameters**
  - `limit`: Maximum number of reports to return (default: 100).
- **Returns**: List of `ModerationReport` objects in pending status.
- **Throws**: `ArgumentOutOfRangeException` if `limit` is less than 1.

---
### `public async Task<ModerationReport?> GetReportAsync(int reportId)`

Retrieves a single moderation report by its identifier.

- **Parameters**
  - `reportId`: Unique identifier of the report.
- **Returns**: The `ModerationReport` if found; otherwise, `null`.
- **Throws**: `ArgumentException` if `reportId` is invalid.

---
### `public async Task<ModerationReport> UpdateReportAsync(int reportId, string moderatorId, string notes)`

Updates metadata (e.g., notes, status) on an existing moderation report.

- **Parameters**
  - `reportId`: Unique identifier of the report to update.
  - `moderatorId`: Unique identifier of the moderator performing the update.
  - `notes`: Additional context or notes to attach to the report.
- **Returns**: The updated `ModerationReport`.
- **Throws**: `ArgumentException` if any parameter is invalid; `InvalidOperationException` if the report is not in a modifiable state.

---
### `public async Task<ModerationReport> CreateReportAsync(string reporterId, string subjectId, ReportType type, string reason)`

Creates a new moderation report.

- **Parameters**
  - `reporterId`: Unique identifier of the user submitting the report.
  - `subjectId`: Unique identifier of the user or listing being reported.
  - `type`: Type of report (e.g., user, listing).
  - `reason`: Description of the violation.
- **Returns**: The newly created `ModerationReport`.
- **Throws**: `ArgumentException` if any parameter is null or invalid; `InvalidOperationException` if the reporter is blocked from reporting.

---
### `public async Task<List<ModerationReport>> GetReportsByStatusAsync(ReportStatus status, int limit = 100)`

Retrieves a list of moderation reports filtered by status.

- **Parameters**
  - `status`: Status to filter by (e.g., pending, resolved).
  - `limit`: Maximum number of reports to return (default: 100).
- **Returns**: List of `ModerationReport` objects matching the status.
- **Throws**: `ArgumentOutOfRangeException` if `limit` is less than 1.

---
### `public async Task<List<ModerationReport>> GetModeratorAssignmentsAsync(string moderatorId)`

Retrieves all reports currently assigned to a specific moderator.

- **Parameters**
  - `moderatorId`: Unique identifier of the moderator.
- **Returns**: List of `ModerationReport` objects assigned to the moderator.
- **Throws**: `ArgumentException` if `moderatorId` is invalid.

---
### `public async Task<(int pending, int inReview, int resolved)> GetReportStatsAsync()`

Retrieves aggregate statistics on moderation reports.

- **Returns**: Tuple containing counts of reports by status: pending, in review, and resolved.
- **Throws**: None.

## Usage

### Example 1: Reporting and Resolving a User
