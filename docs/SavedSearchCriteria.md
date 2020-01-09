# SavedSearchCriteria

Represents a user-defined search query that can be saved, executed, and monitored for new matching listings. Used to persist search preferences and enable alerting when new listings match the criteria.

## API

### `public Guid Id`
Unique identifier for the saved search. Assigned on creation and immutable thereafter.

### `public required Guid UserId`
Identifier of the user who owns this saved search. Must be provided when creating a new instance.

### `public string? Keywords`
Optional free-text keywords to match against listing titles and descriptions. `null` means no keyword filtering.

### `public Guid? CategoryId`
Optional category identifier to restrict results to a specific category. `null` means no category restriction.

### `public decimal? MaxPrice`
Optional maximum price to filter listings. `null` means no price restriction.

### `public List<string> Tags`
List of tags that must all be present on a listing for it to match. Empty list means no tag restriction.

### `public DateTime CreatedAt`
Timestamp indicating when the saved search was created. Set automatically on creation.

### `public SavedSearchAlertService`
Service used to manage alerting for this saved search. Provides methods to enable, disable, and configure alerts.

### `public SavedSearchCriteria Save()`
Persists the current instance to storage. Returns the saved instance, which may include updated fields such as `Id` or `CreatedAt`. Throws if persistence fails.

### `public bool Remove()`
Deletes the saved search from storage. Returns `true` if the search existed and was removed, `false` if it did not exist. Throws if deletion fails.

### `public IReadOnlyList<SavedSearchCriteria> GetForUser(Guid userId)`
Retrieves all saved searches belonging to the specified user. Returns an empty list if no searches exist for the user. Throws if retrieval fails.

### `public bool Matches(Listing listing)`
Determines whether the given listing matches the criteria. Returns `true` if the listing satisfies all specified filters (keywords, category, price, tags), otherwise `false`.

### `public IReadOnlyList<Guid> FindUsersToNotify()`
Identifies users who should be notified of new listings matching this criteria. Returns a list of user identifiers. Throws if the operation fails.

### `public async Task<IReadOnlyList<Listing>> ExecuteAsync()`
Executes the saved search against the current marketplace state and returns all matching listings. The operation is asynchronous and may take significant time for large datasets. Returns an empty list if no listings match. Throws if execution fails.

## Usage
