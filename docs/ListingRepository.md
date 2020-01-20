# ListingRepository

`ListingRepository` provides the persistence layer for `Listing` entities within the marketplace engine. It abstracts all database operations—creation, retrieval, update, deletion, and specialized queries—behind asynchronous methods that return domain objects or collections. The repository is designed to be injected into services and handlers, isolating them from the underlying data store implementation.

## API

### ListingRepository
Constructor. Initializes a new instance of the repository with the required data context, connection factory, or equivalent infrastructure. The exact dependencies are determined by the concrete implementation and are not exposed on the public surface.

### async Task<Listing?> GetByIdAsync
Retrieves a single listing by its unique identifier.

| Parameter | Type | Description |
|-----------|------|-------------|
| `id` | `Guid` or `string` | The primary key of the listing. |

**Returns:** The matching `Listing` instance, or `null` if no record with the given identifier exists.

**Throws:** `ArgumentNullException` when `id` is an empty GUID or null string. `OperationCanceledException` on cancellation. Underlying data store exceptions (e.g., connection failure) are propagated as `DataAccessException` or the provider-specific type.

### async Task<List<Listing>> GetAllAsync
Returns every listing in the system. Intended for administrative or bulk-export scenarios; callers should prefer filtered or paged methods for user-facing queries.

**Returns:** A `List<Listing>` containing all listings. The list is empty when no listings exist.

**Throws:** `OperationCanceledException` on cancellation. Data store exceptions are propagated.

### async Task<Listing> AddAsync
Persists a new listing and returns the fully materialized entity, including any server-generated values such as the assigned identifier, creation timestamp, or default status.

| Parameter | Type | Description |
|-----------|------|-------------|
| `listing` | `Listing` | The domain object to insert. Must pass validation. |

**Returns:** The inserted `Listing` with all server-populated fields set.

**Throws:** `ArgumentNullException` when `listing` is null. `ValidationException` when required fields (title, seller ID, category ID) are missing or invalid. Data store exceptions on conflict or connection failure.

### async Task<Listing> UpdateAsync
Replaces an existing listing with the supplied state. The record must already exist; otherwise the behavior is implementation-defined (typically throws or returns a faulted result).

| Parameter | Type | Description |
|-----------|------|-------------|
| `listing` | `Listing` | The domain object containing updated values. The identifier must match an existing record. |

**Returns:** The updated `Listing` as persisted, reflecting any triggers or computed column changes.

**Throws:** `ArgumentNullException` when `listing` is null. `ConcurrencyException` when an optimistic concurrency token mismatch is detected. `NotFoundException` when the record does not exist. `ValidationException` on invalid state.

### async Task DeleteAsync
Removes a listing permanently. Soft-delete semantics, if any, are an implementation detail of the concrete repository.

| Parameter | Type | Description |
|-----------|------|-------------|
| `id` | `Guid` or `string` | The identifier of the listing to remove. |

**Returns:** A completed task. No value is returned.

**Throws:** `ArgumentNullException` when `id` is invalid. `NotFoundException` when the listing does not exist. Data store exceptions on constraint violations (e.g., foreign key references that prevent deletion).

### async Task<bool> ExistsAsync
Checks whether a listing with the given identifier is present in the data store.

| Parameter | Type | Description |
|-----------|------|-------------|
| `id` | `Guid` or `string` | The identifier to test. |

**Returns:** `true` if the listing exists; `false` otherwise.

**Throws:** `ArgumentNullException` when `id` is invalid. Data store exceptions on connection failure.

### async Task<int> CountAsync
Returns the total number of listings in the system. May reflect soft-deleted records depending on implementation.

**Returns:** A non-negative integer representing the count.

**Throws:** Data store exceptions on connection failure.

### async Task<List<Listing>> GetBySellerIdAsync
Retrieves all listings owned by a specific seller.

| Parameter | Type | Description |
|-----------|------|-------------|
| `sellerId` | `Guid` or `string` | The identifier of the seller. |

**Returns:** A list of listings belonging to that seller, ordered by implementation-defined criteria (typically creation date descending). Empty list when the seller has no listings.

**Throws:** `ArgumentNullException` when `sellerId` is invalid. Data store exceptions.

### async Task<List<Listing>> GetByCategoryIdAsync
Retrieves all listings assigned to a given category.

| Parameter | Type | Description |
|-----------|------|-------------|
| `categoryId` | `Guid` or `string` | The category identifier. |

**Returns:** A list of listings in that category. Empty list when the category is empty or does not exist.

**Throws:** `ArgumentNullException` when `categoryId` is invalid. Data store exceptions.

### async Task<List<Listing>> GetByStatusAsync
Filters listings by their current status value (e.g., `Active`, `Draft`, `Sold`, `Archived`).

| Parameter | Type | Description |
|-----------|------|-------------|
| `status` | `ListingStatus` (enum) | The status to filter by. |

**Returns:** A list of listings matching the status. Empty list when no listings have that status.

**Throws:** `ArgumentOutOfRangeException` when `status` is an undefined enum value. Data store exceptions.

### async Task<List<Listing>> GetActiveListingsAsync
Convenience method that returns all listings with `Active` status. Equivalent to `GetByStatusAsync(ListingStatus.Active)` but may include additional default ordering or caching behavior.

**Returns:** A list of active listings.

**Throws:** Data store exceptions.

### async Task<List<Listing>> GetFeaturedListingsAsync
Returns listings explicitly marked as featured. The featured flag is typically a boolean column or a membership in a featured collection.

**Returns:** A list of featured listings, often ordered by a priority or featured-date field.

**Throws:** Data store exceptions.

### async Task<List<Listing>> GetRecentListingsAsync
Returns the most recently created listings, bounded by an implementation-defined limit (e.g., 50 or 100) and ordered by creation timestamp descending.

**Returns:** A list of recent listings. May be empty if no listings exist.

**Throws:** Data store exceptions.

### async Task<List<Listing>> SearchAsync
Performs a free-text search across listing titles, descriptions, and possibly tags. The search implementation may use full-text indexing, `LIKE` queries, or an external search provider.

| Parameter | Type | Description |
|-----------|------|-------------|
| `query` | `string` | The search term. Null or whitespace may return an empty list or all listings depending on implementation. |

**Returns:** A relevance-ordered list of matching listings. Empty when no matches are found.

**Throws:** `ArgumentNullException` when `query` is null (whitespace-only may be treated as null). Data store exceptions.

### async Task<List<Listing>> GetByTagsAsync
Retrieves listings that are associated with at least one of the supplied tags.

| Parameter | Type | Description |
|-----------|------|-------------|
| `tags` | `IEnumerable<string>` | A collection of tag values. Duplicates are ignored. |

**Returns:** A list of listings matching any of the tags. Empty when no listings match or `tags` is empty.

**Throws:** `ArgumentNullException` when `tags` is null. Data store exceptions.

### async Task<List<Listing>> GetNearbyAsync
Returns listings whose geolocation falls within a specified radius of a center point. Requires listings to have latitude/longitude coordinates populated.

| Parameter | Type | Description |
|-----------|------|-------------|
| `latitude` | `double` | Center point latitude. |
| `longitude` | `double` | Center point longitude. |
| `radiusKm` | `double` | Search radius in kilometers. Must be positive. |

**Returns:** A distance-ordered list of listings within the radius. Empty when no listings are nearby.

**Throws:** `ArgumentOutOfRangeException` when `radiusKm` is zero or negative. `ArgumentException` when coordinates are outside valid ranges. Data store exceptions.

### async Task<(List<Listing> items, int total)> GetPagedAsync
Returns a paginated slice of listings along with the total count of records matching the optional filter criteria.

| Parameter | Type | Description |
|-----------|------|-------------|
| `page` | `int` | 1-based page number. Must be >= 1. |
| `pageSize` | `int` | Number of items per page. Typically bounded (e.g., 1–100). |
| `filter` | `ListingFilter?` | Optional filter object with status, category, seller, and other criteria. Null means no filtering. |
| `sort` | `ListingSort?` | Optional sort specification (field and direction). Null means default ordering. |

**Returns:** A tuple where `items` is the page of listings and `total` is the unfiltered count of all matching records.

**Throws:** `ArgumentOutOfRangeException` when `page` < 1 or `pageSize` is outside allowed bounds. Data store exceptions.

### async Task IncrementViewCountAsync
Atomically increments the view counter for a listing. Typically called each time a listing detail page is served.

| Parameter | Type | Description |
|-----------|------|-------------|
| `id` | `Guid` or `string` | The listing identifier. |

**Returns:** A completed task. The updated count is not returned; callers must re-fetch the listing if the new value is needed.

**Throws:** `ArgumentNullException` when `id` is invalid. `NotFoundException` when the listing does not exist. Data store exceptions.

### async Task IncrementInterestCountAsync
Atomically increments the interest or "watch" counter for a listing. Called when a user expresses interest (e.g., clicking "I'm interested" or adding to a watchlist).

| Parameter | Type | Description |
|-----------|------|-------------|
| `id` | `Guid` or `string` | The listing identifier. |

**Returns:** A completed task. The updated count is not returned.

**Throws:** `ArgumentNullException` when `id` is invalid. `NotFoundException` when the listing does not exist. Data store exceptions.

## Usage

### Example 1: Creating and retrieving a listing

```csharp
// Assume _listingRepo is injected via constructor
var newListing = new Listing
{
    Title = "Vintage Road Bike",
    Description = "Well-maintained, ready to ride.",
    SellerId = currentUser.Id,
    CategoryId = cyclingCategory.Id,
    Price = 350.00m,
    Status = ListingStatus.Active
};

Listing created = await _listingRepo.AddAsync(newListing);

// Later, fetch it back
Listing? fetched = await _listingRepo.GetByIdAsync(created.Id);
if (fetched is not null)
{
    Console.WriteLine($"Listing '{fetched.Title}' is {fetched.Status}");
}
```

### Example 2: Paged search with filtering

```csharp
var filter = new ListingFilter
{
    CategoryId = electronicsCategory.Id,
    Status = ListingStatus.Active,
    MinPrice = 50m,
    MaxPrice = 500m
};

var sort = new ListingSort
{
    Field = ListingSortField.Price,
    Direction = SortDirection.Ascending
};

(List<Listing> items, int total) = await _listingRepo.GetPagedAsync(
    page: 1,
    pageSize: 20,
    filter: filter,
    sort: sort
);

Console.WriteLine($"Showing {items.Count} of {total} electronics listings.");
foreach (var listing in items)
{
    Console.WriteLine($"{listing.Title} — ${listing.Price}");
}
```

## Notes

- **Null returns:** `GetByIdAsync` returns `null` for missing identifiers; all collection-returning methods return empty lists, never `null`.
- **Validation:** `AddAsync` and `UpdateAsync` enforce domain invariants. Callers should validate input before calling these methods to avoid `ValidationException`.
- **Concurrency:** `UpdateAsync` uses optimistic concurrency control when the underlying data store supports it. Callers must be prepared to catch `ConcurrencyException` and retry or reconcile.
- **Thread safety:** The repository itself is not guaranteed to be thread-safe. Instances should be scoped (e.g., per-request via dependency injection) and not shared across concurrent operations without external synchronization.
- **Increment methods:** `IncrementViewCountAsync` and `IncrementInterestCountAsync` perform fire-and-forget atomic updates. They do not return the new value; a subsequent `GetByIdAsync` call is required to observe the updated count. These methods assume the listing exists and will throw `NotFoundException` otherwise.
- **Geolocation:** `GetNearbyAsync` requires listings to have non-null latitude and longitude. Listings without coordinates are silently excluded from results.
- **Soft deletes:** Whether `DeleteAsync` performs a hard or soft delete is implementation-defined. Methods like `GetActiveListingsAsync` and `GetByStatusAsync` will exclude soft-deleted records if that pattern is used.
- **Paging bounds:** `GetPagedAsync` enforces minimum and maximum page sizes. Passing a `pageSize` of 0 or a negative value throws `ArgumentOutOfRangeException`. The maximum is typically 100; consult the concrete implementation for the exact limit.
- **Search behavior:** `SearchAsync` with a null or whitespace-only query may return an empty list or all listings, depending on the implementation. Callers should guard against empty queries if a specific behavior is required.
