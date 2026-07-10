# ModerationReportDto

Data transfer object representing a moderation report submitted by a user regarding a listing or user in the marketplace system. Used to communicate report details between service layers and API endpoints.

## API

### Properties

- **`Id`** (Guid)
  Unique identifier for the moderation report. Read-only; assigned by the system on creation.

- **`ListingId`** (Guid?)
  Identifier of the listing being reported, if applicable. Null if the report targets a user rather than a listing.

- **`ListingTitle`** (string?)
  Title of the reported listing, if available. Populated when the report is associated with a listing and the title is accessible at the time of report creation.

- **`UserId`** (Guid?)
  Identifier of the user being reported, if applicable. Null if the report targets a listing rather than a user.

- **`UserName`** (string?)
  Username of the reported user, if available. Populated when the report is associated with a user and the username is accessible at the time of report creation.

- **`ReporterUserId`** (Guid)
  Identifier of the user who submitted the moderation report. Required; must correspond to an authenticated user in the system.

- **`ReporterName`** (string)
  Display name of the user who submitted the report. Populated from user profile data at the time of report creation.

- **`Reason`** (string)
  Detailed explanation provided by the reporter explaining why the listing or user is being reported. Required; must be non-empty.

- **`Status`** (string)
  Current state of the moderation report (e.g., "Pending", "UnderReview", "Resolved", "Rejected"). Defaults to "Pending" on creation.

- **`ResolutionNotes`** (string?)
  Additional notes added by moderators when resolving the report. Null if the report is unresolved or no notes have been added.

- **`CreatedAt`** (DateTime)
  Timestamp indicating when the report was created. Assigned by the system on creation; immutable.

### Constructors

- **`ModerationReportDto()`**
  Initializes a new instance of the `ModerationReportDto` with default values. All reference-type properties are initialized to null, and value-type properties are initialized to their default values.

- **`ModerationReportDto(...)`**
  Initializes a new instance of the `ModerationReportDto` with the specified values. All parameters are validated by the caller; no validation is performed by the constructor.

## Usage
