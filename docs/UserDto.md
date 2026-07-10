# UserDto

DTO representing a user in the marketplace system, used for serialization and transfer between service layers. Contains core identity, rating, and engagement metrics.

## API

### Properties

- **`Id`** (Guid)
  Unique identifier for the user. Immutable after creation.

- **`Email`** (string)
  User’s email address. Used for authentication and contact. Not nullable.

- **`DisplayName`** (string)
  Public alias shown to other users. May be null if not set.

- **`Bio`** (string)
  Free-text user description. Optional; may be null or empty.

- **`Role`** (string)
  Authorization role (e.g., "Buyer", "Seller", "Admin"). Not nullable.

- **`AverageRating`** (double)
  Aggregated rating from reviews, averaged to one decimal place. Defaults to 0.0 if no reviews exist.

- **`ReviewCount`** (int)
  Number of reviews submitted for the user. Always non-negative.

- **`EmailVerified`** (bool)
  Indicates whether the user’s email has been verified via confirmation flow. Defaults to `false`.

- **`CreatedAt`** (DateTime)
  Timestamp when the user record was created. Immutable after creation.

- **`UpdatedAt`** (DateTime?)
  Timestamp of the last update to the user record. Null if never updated.

### Constructors

- **`UserDto()`**
  Default constructor; initializes all properties to defaults (`0`, `null`, `false`, etc.).

- **`UserDto(Guid userId, double averageRating, int totalReviews, int totalSales, string responseTime, int rank)`**
  Constructs a DTO populated with seller-specific metrics. All parameters are required; passing invalid values may result in undefined behavior.

## Usage
