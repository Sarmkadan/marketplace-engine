# ReviewDto

Data transfer object representing a review in the marketplace engine, used for serializing and deserializing review information between services and clients. Contains core review details, seller information, and aggregated statistics for listings or sellers.

## API

### Properties

- **`Id`** (Guid)
  Unique identifier for the review. Used as a primary key in storage and for referencing the review in other operations.

- **`ReviewerId`** (Guid)
  Unique identifier of the user who submitted the review. Required for associating reviews with user accounts.

- **`ReviewerName`** (string)
  Display name of the reviewer. Provided for display purposes in the UI and should reflect the user's chosen public name.

- **`SellerId`** (Guid)
  Unique identifier of the seller being reviewed. Used to aggregate reviews per seller and for seller analytics.

- **`ListingId`** (Guid?)
  Optional unique identifier of the specific listing being reviewed. Null when the review applies to the seller in general rather than a particular listing.

- **`Score`** (int)
  Numeric rating given by the reviewer, typically on a scale (e.g., 1 to 5). Must be validated by the consuming service to ensure it falls within expected bounds.

- **`Comment`** (string)
  Text content of the review provided by the reviewer. May be empty but should not be null. Length and content restrictions are enforced by the API layer.

- **`Status`** (string)
  Current state of the review (e.g., "Pending", "Approved", "Rejected"). Used to manage review moderation and visibility in the system.

- **`SellerReply`** (string?)
  Optional response provided by the seller to the review. Null when no reply has been given. Should be sanitized before display to prevent XSS.

- **`CreatedAt`** (DateTime)
  Timestamp indicating when the review was first submitted. Immutable once set; used for sorting and audit trails.

- **`UpdatedAt`** (DateTime?)
  Timestamp indicating the last time the review or its metadata was modified. Null if the review has never been edited. Updated automatically on status changes or seller replies.

### Constructors

- **`ReviewDto()`**
  Default constructor for deserialization. Initializes all properties to default values (empty strings, nulls, default dates).

- **`ReviewDto(Guid sellerId)`**
  Parameterized constructor accepting the seller identifier. Used when creating a new review to associate it with a seller immediately.

## Usage
