# ModerationReport

The `ModerationReport` type represents a formal record of content or behavior reported by a user within the marketplace system. It tracks the reporter, the target (user, listing, or message), the reason for the report, and the lifecycle of the moderation process, including review status, priority, and resolution timestamps.

## API

### `Id`
- **Purpose**: Unique identifier for the report.
- **Type**: `Guid`
- **Notes**: Generated automatically when the report is created. Never null.

### `ReporterId`
- **Purpose**: Foreign key identifying the user who submitted the report.
- **Type**: `Guid`
- **Notes**: Matches the `Id` of the `Reporter` navigation property. Never null.

### `Reporter`
- **Purpose**: Navigation property to the `User` who submitted the report.
- **Type**: `User?`
- **Notes**: May be null if the reporter account has been deleted. Populated via entity framework during queries.

### `TargetUserId`
- **Purpose**: Foreign key identifying the user being reported, if applicable.
- **Type**: `Guid?`
- **Notes**: Null if the report targets a listing or message instead of a user.

### `TargetUser`
- **Purpose**: Navigation property to the `User` being reported.
- **Type**: `User?`
- **Notes**: Null if the report does not target a user or if the target user account has been deleted.

### `TargetListingId`
- **Purpose**: Foreign key identifying the listing being reported, if applicable.
- **Type**: `Guid?`
- **Notes**: Null if the report targets a user or message instead of a listing.

### `TargetListing`
- **Purpose**: Navigation property to the `Listing` being reported.
- **Type**: `Listing?`
- **Notes**: Null if the report does not target a listing or if the listing has been deleted.

### `TargetMessageId`
- **Purpose**: Foreign key identifying the message being reported, if applicable.
- **Type**: `Guid?`
- **Notes**: Null if the report targets a user or listing instead of a message.

### `TargetMessage`
- **Purpose**: Navigation property to the `Message` being reported.
- **Type**: `Message?`
- **Notes**: Null if the report does not target a message or if the message has been deleted.

### `Reason`
- **Purpose**: Short, standardized description of the report reason (e.g., "Spam", "Harassment").
- **Type**: `string`
- **Notes**: Never null. Expected to match predefined values in the system.

### `Details`
- **Purpose**: Optional free-text field for additional context provided by the reporter.
- **Type**: `string?`
- **Notes**: May be null if the reporter did not provide extra details.

### `Evidence`
- **Purpose**: Optional URL or reference to external evidence (e.g., screenshots, logs) supporting the report.
- **Type**: `string?`
- **Notes**: May be null if no evidence was provided.

### `Status`
- **Purpose**: Current state of the report in the moderation workflow.
- **Type**: `ModerationStatus` (enum)
- **Notes**: Defaults to `Pending` on creation. Never null.

### `ReviewedBy`
- **Purpose**: Foreign key identifying the moderator who reviewed the report, if applicable.
- **Type**: `Guid?`
- **Notes**: Null if the report has not been reviewed.

### `Reviewer`
- **Purpose**: Navigation property to the `User` who reviewed the report.
- **Type**: `User?`
- **Notes**: Null if the report has not been reviewed or if the reviewer account has been deleted.

### `ReviewNotes`
- **Purpose**: Optional free-text field for moderator comments or actions taken.
- **Type**: `string?`
- **Notes**: May be null if the moderator did not provide notes.

### `CreatedAt`
- **Purpose**: Timestamp when the report was submitted.
- **Type**: `DateTime`
- **Notes**: Set automatically on creation. Never null.

### `UpdatedAt`
- **Purpose**: Timestamp of the last modification to the report.
- **Type**: `DateTime?`
- **Notes**: Null if the report has not been updated since creation.

### `ResolvedAt`
- **Purpose**: Timestamp when the report was marked as resolved.
- **Type**: `DateTime?`
- **Notes**: Null if the report is still pending or under review.

### `Priority`
- **Purpose**: Numeric value indicating the urgency of the report (higher values = higher priority).
- **Type**: `int`
- **Notes**: Defaults to `0` on creation. May be adjusted by moderators.

## Usage

### Example 1: Creating a Report
