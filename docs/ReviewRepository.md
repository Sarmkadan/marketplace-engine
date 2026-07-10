# ReviewRepository

Central data-access component for managing `Review` entities within the marketplace engine. Provides asynchronous CRUD operations, existence checks, counts, and specialized queries for reviews keyed by reviewer, seller, listing, or transaction. Designed to isolate persistence logic and support pagination scenarios.

## API

### `public ReviewRepository(...)`
Constructor accepting the required data context or connection factory used by downstream operations. The exact dependencies are implementation-defined and may vary by persistence provider.

### `public async Task<Review?> GetByIdAsync(Guid id)`
Retrieves the review with the specified unique identifier, or `null` if no match exists.
- **Parameters**: `id` – the GUID of the target review.
- **Returns**: A `Review` instance when found; otherwise `null`.
- **Exceptions**: Throws if the underlying store is unavailable or the identifier is malformed.

### `public async Task<List<Review>> GetAllAsync()`
Returns every review currently stored in the system.
- **Returns**: A non-null list (possibly empty) of all reviews.
- **Exceptions**: Throws on persistent store failure.

### `public async Task<Review> AddAsync(Review review)`
Inserts a new review into the repository and returns the persisted entity, now populated with any auto-generated keys or timestamps.
- **Parameters**: `review` – the review to create (must be non-null and valid).
- **Returns**: The saved `Review` instance.
- **Exceptions**: Throws on validation errors, duplicate keys, or persistence failures.

### `public async Task<Review> UpdateAsync(Review review)`
Replaces an existing review with the supplied instance.
- **Parameters**: `review` – the updated review (must already exist).
- **Returns**: The updated `Review` instance.
- **Exceptions**: Throws if the review does not exist, validation fails, or persistence errors occur.

### `public async Task DeleteAsync(Guid id)`
Removes the review identified by `id` from the repository.
- **Parameters**: `id` – the GUID of the review to delete.
- **Exceptions**: Throws if the identifier is invalid or the review is missing.

### `public async Task<bool> ExistsAsync(Guid id)`
Determines whether a review with the given identifier exists.
- **Parameters**: `id` – the GUID to check.
- **Returns**: `true` if the review exists; otherwise `false`.
- **Exceptions**: Throws on persistent store failure.

### `public async Task<int> CountAsync()`
Returns the total number of reviews in the repository.
- **Returns**: The non-negative count of reviews.
- **Exceptions**: Throws on persistent store failure.

### `public async Task<List<Review>> GetByReviewerIdAsync(Guid reviewerId)`
Fetches all reviews authored by the specified reviewer.
- **Parameters**: `reviewerId` – the GUID of the reviewer.
- **Returns**: A list (possibly empty) of matching reviews.
- **Exceptions**: Throws on persistent store failure.

### `public async Task<List<Review>> GetBySellerIdAsync(Guid sellerId)`
Fetches all reviews where the target seller is the recipient.
- **Parameters**: `sellerId` – the GUID of the seller.
- **Returns**: A list (possibly empty) of matching reviews.
- **Exceptions**: Throws on persistent store failure.

### `public async Task<List<Review>> GetByListingIdAsync(Guid listingId)`
Fetches all reviews associated with the specified listing.
- **Parameters**: `listingId` – the GUID of the listing.
- **Returns**: A list (possibly empty) of matching reviews.
- **Exceptions**: Throws on persistent store failure.

### `public async Task<bool> ExistsForTransactionAsync(Guid transactionId)`
Checks whether any review exists for the given transaction.
- **Parameters**: `transactionId` – the GUID of the transaction.
- **Returns**: `true` if at least one review references the transaction; otherwise `false`.
- **Exceptions**: Throws on persistent store failure.

### `public async Task<double> GetAverageScoreAsync()`
Computes the arithmetic mean of all review scores currently stored.
- **Returns**: The average score as a `double`; returns `0.0` if no reviews exist.
- **Exceptions**: Throws on persistent store failure.

### `public async Task<(List<Review> items, int total)> GetPagedBySellerAsync(Guid sellerId, int pageNumber, int pageSize)`
Returns a single page of reviews for a given seller along with the total count of matching reviews.
- **Parameters**:
  - `sellerId` – the GUID of the seller.
  - `pageNumber` – the zero-based page index (must be ≥ 0).
  - `pageSize` – the maximum number of items per page (must be ≥ 1).
- **Returns**: A tuple containing the page of reviews and the total number of reviews for the seller.
- **Exceptions**: Throws if `pageNumber` or `pageSize` are invalid, or on persistent store failure.

## Usage
