# ListingServiceExtendedTests

Comprehensive test suite for the `ListingService` class, validating core listing operations including creation, updates, visibility changes, view tracking, interest recording, and seller-specific queries. Focuses on edge cases, authorization boundaries, and state transitions under both happy-path and failure scenarios.

## API

### Public Constructors

#### `ListingServiceExtendedTests`
Initializes a new instance of the test class with required service dependencies and test data context.

### Public Methods

#### `Task CreateListingAsync_WithValidData_ReturnsCreatedListing`
Validates that a correctly formed listing request results in a persisted listing with the expected properties. Ensures the returned model matches the input data and contains a valid identifier.

#### `Task CreateListingAsync_WithNegativePrice_ThrowsArgumentException`
Ensures that attempting to create a listing with a negative price value results in an `ArgumentException`, enforcing price validation rules.

#### `Task CreateListingAsync_WithShortTitle_ThrowsArgumentException`
Confirms that listings with titles shorter than the minimum allowed length throw an `ArgumentException`, validating title length constraints.

#### `Task CreateListingAsync_WithNoImages_ThrowsArgumentException`
Verifies that listings without any images throw an `ArgumentException`, enforcing the requirement for at least one image.

#### `Task UpdateListingAsync_WhenListingNotFound_ThrowsResourceNotFoundException`
Ensures that attempting to update a non-existent listing results in a `ResourceNotFoundException`, validating existence checks.

#### `Task UpdateListingAsync_WhenCallerIsNotSeller_ThrowsUnauthorizedException`
Confirms that only the listing seller (or authorized moderators) can update a listing, throwing an `UnauthorizedException` otherwise.

#### `Task UpdateListingAsync_WithNewTitle_ReturnsUpdatedListing`
Validates that updating a listing’s title returns the updated entity with the new title reflected in the response.

#### `Task UpdateListingAsync_WithCategoryChange_ReturnsPreviousCategoryId`
Ensures that changing a listing’s category returns the previous category identifier, allowing callers to track historical state.

#### `Task SetListingVisibilityAsync_WhenListingNotFound_ThrowsResourceNotFoundException`
Confirms that attempting to change visibility on a non-existent listing throws a `ResourceNotFoundException`.

#### `Task SetListingVisibilityAsync_WhenCallerIsNotOwner_ThrowsUnauthorizedException`
Validates that only the listing owner (or authorized moderators) can modify visibility, throwing an `UnauthorizedException` otherwise.

#### `Task SetListingVisibilityAsync_WhenUnpublishing_SetsStatusInactive`
Ensures that unpublishing a listing transitions its status to inactive, reflecting the intended state change.

#### `Task GetListingWithViewAsync_WhenListingNotFound_ThrowsResourceNotFoundException`
Validates that retrieving a non-existent listing throws a `ResourceNotFoundException`, enforcing existence checks.

#### `Task GetListingWithViewAsync_WhenListingExists_IncrementsViewCount`
Confirms that accessing a listing via this method increments its view count, supporting analytics and popularity tracking.

#### `Task RecordInterestAsync_WhenListingNotFound_ThrowsResourceNotFoundException`
Ensures that recording interest in a non-existent listing throws a `ResourceNotFoundException`.

#### `Task RecordInterestAsync_WhenListingExists_IncrementsInterestCount`
Validates that recording interest in an existing listing increments its interest counter, supporting engagement metrics.

#### `Task GetSellerListingsAsync_WhenSellerNotFound_ThrowsResourceNotFoundException`
Confirms that querying listings for a non-existent seller throws a `ResourceNotFoundException`.

#### `Task GetSellerListingsAsync_WhenSellerExists_ReturnsListings`
Validates that querying listings for an existing seller returns the expected collection of listings.

#### `Task GetFeaturedListingsAsync_WithNegativeLimit_ClampsToDefault`
Ensures that a negative limit is clamped to the system default, preventing invalid pagination parameters.

#### `Task GetFeaturedListingsAsync_WithLimitAbove100_ClampsToDefault`
Confirms that limits exceeding 100 are clamped to the maximum allowed value, enforcing reasonable bounds on result sets.

## Usage

### Example 1: Validating Listing Creation
