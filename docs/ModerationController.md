# ModerationController

Provides endpoints for managing and processing moderation reports within the marketplace system, including report retrieval, review actions, and bulk operations.

## API

### `public ModerationController`

Constructor for the moderation controller. Initializes dependencies required for report management and processing.

### `public async Task<IActionResult> GetPendingReports([FromQuery] int page = 1, [FromQuery] int pageSize = 10)`

Retrieves a paginated list of reports awaiting moderation.

- **Parameters**:
  - `page` (optional): The page number to retrieve. Defaults to 1.
  - `pageSize` (optional): The number of items per page. Defaults to 10.
- **Return value**: An `IActionResult` containing a paginated list of pending reports.
- **Throws**: May throw if the underlying data access fails or if validation of pagination parameters fails.

### `public async Task<IActionResult> GetReportDetails(int reportId)`

Retrieves detailed information about a specific report.

- **Parameters**:
  - `reportId`: The unique identifier of the report to retrieve.
- **Return value**: An `IActionResult` containing the report details.
- **Throws**: May throw if the report does not exist or if data access fails.

### `public async Task<IActionResult> ApproveReport(int reportId)`

Approves a moderation report, indicating that the reported content or behavior complies with guidelines.

- **Parameters**:
  - `reportId`: The unique identifier of the report to approve.
- **Return value**: An `IActionResult` indicating success or failure.
- **Throws**: May throw if the report does not exist, if it is not in a pending state, or if data access fails.

### `public async Task<IActionResult> RejectReport(int reportId)`

Rejects a moderation report, indicating that the reported content or behavior does not violate guidelines.

- **Parameters**:
  - `reportId`: The unique identifier of the report to reject.
- **Return value**: An `IActionResult` indicating success or failure.
- **Throws**: May throw if the report does not exist, if it is not in a pending state, or if data access fails.

### `public async Task<IActionResult> CreateReport(CreateReportRequest request)`

Creates a new moderation report for a specific item or user.

- **Parameters**:
  - `request`: The request payload containing report details (e.g., reporter ID, reported item ID, reason).
- **Return value**: An `IActionResult` containing the created report or validation errors.
- **Throws**: May throw if validation of the request fails or if data access fails.

### `public async Task<IActionResult> BulkModerate(BulkModerateRequest request)`

Processes multiple moderation actions in a single request, such as bulk approval or rejection of reports.

- **Parameters**:
  - `request`: The request payload containing a list of report IDs and the moderation action to apply.
- **Return value**: An `IActionResult` indicating success or failure, including results for each report.
- **Throws**: May throw if validation of the request fails, if any report does not exist, or if data access fails.

### `public async Task<IActionResult> GetStatistics()`

Retrieves aggregate statistics about moderation activity, such as total reports, pending counts, and approval rates.

- **Return value**: An `IActionResult` containing moderation statistics.
- **Throws**: May throw if data access fails.

## Usage

### Example 1: Retrieving and approving a pending report
