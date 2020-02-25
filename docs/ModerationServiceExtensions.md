# ModerationServiceExtensions

Extension methods for the `ModerationService` that provide batch processing and query capabilities for moderation reports. These methods simplify common moderation workflows by handling multiple reports in a single call and supporting priority-based filtering and assignment.

## API

### `ReportListingsBatchAsync`
Reports multiple listing IDs for moderation review in a single batch operation. Each listing ID is validated before submission, and only valid IDs are processed. Failed validations are reported in the returned list.

- **Parameters**
  - `listingIds` (`IEnumerable<Guid>`): Collection of listing GUIDs to report.
  - `reporterId` (`Guid`): Identifier of the user submitting the reports.
  - `reason` (`string`): Human-readable reason for the reports.
  - `cancellationToken` (`CancellationToken`, optional): Token to monitor for cancellation requests.
- **Returns**
  - `Task<List<ModerationReport>>`: List of `ModerationReport` objects representing the submitted reports, including any validation failures.
- **Throws**
  - `ArgumentNullException`: If `listingIds` or `reason` is `null`.
  - `ArgumentException`: If `listingIds` is empty or contains invalid GUIDs.

---

### `ReportUsersBatchAsync`
Reports multiple user IDs for moderation review in a single batch operation. Each user ID is validated before submission, and only valid IDs are processed. Failed validations are reported in the returned list.

- **Parameters**
  - `userIds` (`IEnumerable<Guid>`): Collection of user GUIDs to report.
  - `reporterId` (`Guid`): Identifier of the user submitting the reports.
  - `reason` (`string`): Human-readable reason for the reports.
  - `cancellationToken` (`CancellationToken`, optional): Token to monitor for cancellation requests.
- **Returns**
  - `Task<List<ModerationReport>>`: List of `ModerationReport` objects representing the submitted reports, including any validation failures.
- **Throws**
  - `ArgumentNullException`: If `userIds` or `reason` is `null`.
  - `ArgumentException`: If `userIds` is empty or contains invalid GUIDs.

---
### `AssignReportsToModeratorAsync`
Assigns a batch of moderation reports to a specified moderator. Reports are validated for assignment eligibility (e.g., not already assigned or closed) before processing.

- **Parameters**
  - `reportIds` (`IEnumerable<Guid>`): Collection of report GUIDs to assign.
  - `moderatorId` (`Guid`): Identifier of the moderator to assign the reports to.
  - `cancellationToken` (`CancellationToken`, optional): Token to monitor for cancellation requests.
- **Returns**
  - `Task<List<ModerationReport>>`: List of `ModerationReport` objects representing the updated reports, including any assignment failures.
- **Throws**
  - `ArgumentNullException`: If `reportIds` is `null`.
  - `ArgumentException`: If `reportIds` is empty or contains invalid GUIDs.

---
### `ProcessReportsBatchAsync`
Processes a batch of moderation reports by applying a moderation action (e.g., approve, reject, escalate) to each report. Reports are validated for processing eligibility (e.g., not already processed) before applying the action.

- **Parameters**
  - `reportIds` (`IEnumerable<Guid>`): Collection of report GUIDs to process.
  - `action` (`ModerationAction`): The moderation action to apply to each report.
  - `moderatorId` (`Guid`): Identifier of the moderator processing the reports.
  - `notes` (`string`, optional): Optional notes to associate with the action.
  - `cancellationToken` (`CancellationToken`, optional): Token to monitor for cancellation requests.
- **Returns**
  - `Task<List<ModerationReport>>`: List of `ModerationReport` objects representing the updated reports, including any processing failures.
- **Throws**
  - `ArgumentNullException`: If `reportIds` is `null`.
  - `ArgumentException`: If `reportIds` is empty, contains invalid GUIDs, or `action` is invalid.

---
### `EscalateReportsPriorityAsync`
Increases the priority level of a batch of moderation reports. Reports are validated for escalation eligibility (e.g., not already at maximum priority) before processing.

- **Parameters**
  - `reportIds` (`IEnumerable<Guid>`): Collection of report GUIDs to escalate.
  - `moderatorId` (`Guid`): Identifier of the moderator escalating the reports.
  - `cancellationToken` (`CancellationToken`, optional): Token to monitor for cancellation requests.
- **Returns**
  - `Task<List<ModerationReport>>`: List of `ModerationReport` objects representing the updated reports, including any escalation failures.
- **Throws**
  - `ArgumentNullException`: If `reportIds` is `null`.
  - `ArgumentException`: If `reportIds` is empty or contains invalid GUIDs.

---
### `FilterByTargetTypeAsync`
Filters a batch of moderation reports by their target type (e.g., listing or user). Reports not matching the specified target type are excluded from the results.

- **Parameters**
  - `reportIds` (`IEnumerable<Guid>`): Collection of report GUIDs to filter.
  - `targetType` (`ModerationTargetType`): The target type to filter by (e.g., `Listing` or `User`).
  - `cancellationToken` (`CancellationToken`, optional): Token to monitor for cancellation requests.
- **Returns**
  - `Task<List<ModerationReport>>`: List of `ModerationReport` objects matching the specified target type.
- **Throws**
  - `ArgumentNullException`: If `reportIds` is `null`.
  - `ArgumentException`: If `reportIds` is empty or contains invalid GUIDs.

---
### `GetPendingReportsByPriorityAsync`
Retrieves a batch of pending moderation reports ordered by priority (highest first). Reports are filtered to include only those in a pending state (e.g., not assigned or processed).

- **Parameters**
  - `limit` (`int`): Maximum number of reports to return.
  - `cancellationToken` (`CancellationToken`, optional): Token to monitor for cancellation requests.
- **Returns**
  - `Task<List<ModerationReport>>`: List of `ModerationReport` objects ordered by priority.
- **Throws**
  - `ArgumentOutOfRangeException`: If `limit` is less than or equal to zero.

---
### `GetReportsByStatusWithPriorityAsync`
Retrieves a batch of moderation reports filtered by status and ordered by priority. Reports are filtered to include only those matching the specified status (e.g., assigned, processed).

- **Parameters**
  - `status` (`ModerationReportStatus`): The status to filter by.
  - `limit` (`int`): Maximum number of reports to return.
  - `cancellationToken` (`CancellationToken`, optional): Token to monitor for cancellation requests.
- **Returns**
  - `Task<List<ModerationReport>>`: List of `ModerationReport` objects matching the specified status and ordered by priority.
- **Throws**
  - `ArgumentOutOfRangeException`: If `limit` is less than or equal to zero.

---
### `GetModeratorAssignmentsByPriorityAsync`
Retrieves a batch of moderation reports assigned to a specific moderator, ordered by priority (highest first). Reports are filtered to include only those assigned to the specified moderator.

- **Parameters**
  - `moderatorId` (`Guid`): Identifier of the moderator to retrieve assignments for.
  - `limit` (`int`): Maximum number of reports to return.
  - `cancellationToken` (`CancellationToken`, optional): Token to monitor for cancellation requests.
- **Returns**
  - `Task<List<ModerationReport>>`: List of `ModerationReport` objects assigned to the moderator and ordered by priority.
- **Throws**
  - `ArgumentOutOfRangeException`: If `limit` is less than or equal to zero.

## Usage

### Example 1: Batch Reporting and Processing
