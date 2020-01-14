# ListingService

Service for managing marketplace listings, including creation, updates, visibility control, retrieval, and seller-specific operations.

## API

### `ListingService`

Initializes a new instance of the `ListingService` class with required dependencies for listing management.

### `async Task<Listing> CreateListingAsync(CreateListingDto dto)`

Creates a new listing from the provided data transfer object. The listing is initially set to visible unless explicitly hidden.

- **Parameters**
  - `dto`: The data transfer object containing listing details such as title, description, category, price, and images.
- **Return value**
  - A `Task` resolving to the created `Listing` entity.
- **Exceptions**
  - Throws `ArgumentException` if required fields in `dto` are invalid or missing.
  - Throws `InvalidOperationException` if the seller associated with the listing is not authorized.

### `async Task<(Listing listing, Guid previousCategoryId)> UpdateListingAsync(Guid listingId, UpdateListingDto dto)`

Updates an existing listing with new data. Returns the updated listing along with the previous category ID.

- **Parameters**
  - `listingId`: The unique identifier of the listing to update.
  - `dto`: The data transfer object containing updated listing fields.
- **Return value**
  - A `Task` resolving to a tuple containing the updated `Listing` and the previous category ID.
- **Exceptions**
  - Throws `KeyNotFoundException` if the listing with `listingId` does not exist.
  - Throws `ArgumentException` if `dto` contains invalid or conflicting data.
  - Throws `InvalidOperationException` if the current user lacks permission to update the listing.

### `async Task<Listing> SetListingVisibilityAsync(Guid listingId, bool isVisible)`

Sets the visibility state of a listing (publicly visible or hidden).

- **Parameters**
  - `listingId`: The unique identifier of the listing.
  - `isVisible`: Boolean indicating whether the listing should be visible.
- **Return value**
  - A `Task` resolving to the updated `Listing` entity.
- **Exceptions**
  - Throws `KeyNotFoundException` if the listing does not exist.
  - Throws `InvalidOperationException` if the current user lacks permission to modify visibility.

### `async Task<Listing> GetListingWithViewAsync(Guid listingId)`

Retrieves a listing by ID and increments its view count atomically.

- **Parameters**
  - `listingId`: The unique identifier of the listing.
- **Return value**
  - A `Task` resolving to the `Listing` with updated view statistics.
- **Exceptions**
  - Throws `KeyNotFoundException` if the listing does not exist.

### `async Task<Listing> RecordInterestAsync(Guid listingId)`

Records a user's interest in a listing (e.g., favoriting or saving).

- **Parameters**
  - `listingId`: The unique identifier of the listing.
- **Return value**
  - A `Task` resolving to the updated `Listing`.
- **Exceptions**
  - Throws `KeyNotFoundException` if the listing does not exist.
  - Throws `InvalidOperationException` if interest recording is not allowed (e.g., by configuration).

### `async Task<Listing> DelistListingAsync(Guid listingId)`

Removes a listing from active marketplace visibility (soft delete). The listing remains in the system for record-keeping.

- **Parameters**
  - `listingId`: The unique identifier of the listing.
- **Return value**
  - A `Task` resolving to the delisted `Listing`.
- **Exceptions**
  - Throws `KeyNotFoundException` if the listing does not exist.
  - Throws `InvalidOperationException` if the current user lacks permission to delist.

### `async Task<List<Listing>> GetSellerListingsAsync(Guid sellerId)`

Retrieves all active listings belonging to a specific seller.

- **Parameters**
  - `sellerId`: The unique identifier of the seller.
- **Return value**
  - A `Task` resolving to a list of `Listing` entities.
- **Exceptions**
  - Throws `KeyNotFoundException` if the seller does not exist.

### `async Task<List<Listing>> GetFeaturedListingsAsync()`

Retrieves a curated list of featured listings, typically prioritized by relevance or promotion.

- **Return value**
  - A `Task` resolving to a list of `Listing` entities marked as featured.
- **Exceptions**
  - Returns an empty list if no featured listings are available.

### `async Task<List<Listing>> GetRecentListingsAsync(int count)`

Retrieves the most recently created listings, limited by `count`.

- **Parameters**
  - `count`: Maximum number of listings to return.
- **Return value**
  - A `Task` resolving to a list of `Listing` entities ordered by creation date.
- **Exceptions**
  - Throws `ArgumentOutOfRangeException` if `count` is less than 1.

### `async Task<(List<Listing> items, int total)> GetPaginatedListingsAsync(int pageNumber, int pageSize, string? categoryId = null, string? searchTerm = null)`

Retrieves a paginated subset of listings, optionally filtered by category or search term.

- **Parameters**
  - `pageNumber`: The 1-based page number to retrieve.
  - `pageSize`: Number of items per page.
  - `categoryId`: Optional category filter.
  - `searchTerm`: Optional free-text search term.
- **Return value**
  - A `Task` resolving to a tuple containing the list of `Listing` entities and the total count of matching listings.
- **Exceptions**
  - Throws `ArgumentOutOfRangeException` if `pageNumber` or `pageSize` is invalid.
  - Throws `ArgumentException` if `categoryId` is invalid.

### `async Task<Listing> MarkAsFeaturedAsync(Guid listingId, bool isFeatured)`

Toggles the featured status of a listing.

- **Parameters**
  - `listingId`: The unique identifier of the listing.
  - `isFeatured`: Boolean indicating whether the listing should be featured.
- **Return value**
  - A `Task` resolving to the updated `Listing`.
- **Exceptions**
  - Throws `KeyNotFoundException` if the listing does not exist.
  - Throws `InvalidOperationException` if the current user lacks permission to modify featured status.

### `async Task<int> GetTotalListingCountAsync()`

Returns the total number of active (non-delisted) listings in the system.

- **Return value**
  - A `Task` resolving to the total count as an integer.

## Usage
