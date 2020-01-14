# WatchlistService

Central service for managing user watchlists within the marketplace engine. It provides functionality to add or remove listings from a user's watchlist, check whether a listing is being watched, retrieve the list of watched listings, and inspect the set of users watching a given listing.

## API

### `WatchlistService`

Entry point for watchlist operations. Consumes a backing store for persistence and a cache for performance.

### `async Task<bool> AddAsync(Guid listingId)`

Adds the specified listing to the current user's watchlist.

- **Parameters**
  - `listingId` – Unique identifier of the listing to add.
- **Return value**
  - `true` if the listing was not already watched and was successfully added; `false` if it was already present.
- **Exceptions**
  - Throws `ArgumentException` if `listingId` is empty.

### `async Task<bool> RemoveAsync(Guid listingId)`

Removes the specified listing from the current user's watchlist.

- **Parameters**
  - `listingId` – Unique identifier of the listing to remove.
- **Return value**
  - `true` if the listing was present and removed; `false` if it was not in the watchlist.
- **Exceptions**
  - Throws `ArgumentException` if `listingId` is empty.

### `bool IsWatching(Guid listingId)`

Determines whether the current user is watching the specified listing.

- **Parameters**
  - `listingId` – Unique identifier of the listing to check.
- **Return value**
  - `true` if the listing is in the watchlist; otherwise `false`.
- **Exceptions**
  - Throws `ArgumentException` if `listingId` is empty.

### `async Task<IReadOnlyList<Listing>> GetWatchedListingsAsync()`

Retrieves the complete set of listings the current user is watching.

- **Return value**
  - An immutable list of `Listing` objects representing the watched items. The list is empty if nothing is watched.
- **Exceptions**
  - None.

### `int GetWatcherCount(Guid listingId)`

Returns the number of distinct users watching the specified listing.

- **Parameters**
  - `listingId` – Unique identifier of the listing whose watchers are to be counted.
- **Return value**
  - The count of users watching the listing.
- **Exceptions**
  - Throws `ArgumentException` if `listingId` is empty.

### `IReadOnlyList<Guid> GetWatchers(Guid listingId)`

Returns the unique identifiers of users watching the specified listing.

- **Parameters**
  - `listingId` – Unique identifier of the listing whose watchers are to be retrieved.
- **Return value**
  - An immutable list of user GUIDs. The list is empty if no users are watching the listing.
- **Exceptions**
  - Throws `ArgumentException` if `listingId` is empty.

## Usage
