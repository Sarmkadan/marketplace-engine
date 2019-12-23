# CollaborativeFilteringEngine

A service that generates personalized listing recommendations for users or items using collaborative filtering techniques. It computes scored listings based on user behavior signals, similarity metrics, trending patterns, or affinity-based heuristics, and records user interactions to refine future recommendations.

## API

### `public CollaborativeFilteringEngine`

Initializes a new instance of the collaborative filtering recommendation engine. Dependencies such as user behavior storage, listing metadata, and similarity calculators are injected via constructor parameters.

### `public async Task<IReadOnlyList<ScoredListing>> ComputeForUserAsync(Guid userId, int maxResults = 20)`

Computes personalized recommendations for the specified user based on their historical interactions and inferred preferences.

- **userId**: The unique identifier of the user for whom recommendations are generated.
- **maxResults**: The maximum number of scored listings to return. Defaults to 20.
- Returns: An asynchronous task that resolves to a read-only list of `ScoredListing` items, ordered by relevance.
- Throws: `ArgumentException` if `userId` is empty.
- Throws: `InvalidOperationException` if required user data is missing or corrupted.

### `public async Task<IReadOnlyList<ScoredListing>> ComputeSimilarAsync(Guid referenceId, int maxResults = 20)`

Generates recommendations similar to a given reference listing, typically used to power "similar items" features.

- **referenceId**: The unique identifier of the reference listing to find similar items for.
- **maxResults**: The maximum number of scored listings to return. Defaults to 20.
- Returns: An asynchronous task that resolves to a read-only list of `ScoredListing` items, ordered by similarity.
- Throws: `ArgumentException` if `referenceId` is empty.
- Throws: `KeyNotFoundException` if the reference listing does not exist.

### `public async Task<IReadOnlyList<ScoredListing>> ComputeTrendingAsync(DateTime? since = null, int maxResults = 20)`

Computes trending or popular listings within a specified time window, useful for discovery and feed surfaces.

- **since**: Optional start of the time window. If `null`, defaults to a recent period (e.g., last 7 days).
- **maxResults**: The maximum number of scored listings to return. Defaults to 20.
- Returns: An asynchronous task that resolves to a read-only list of `ScoredListing` items, ordered by trend score.
- Throws: `InvalidOperationException` if the trend data source is unavailable.

### `public async Task<IReadOnlyList<ScoredListing>> ComputeByAffinityAsync(Guid targetId, int maxResults = 20)`

Computes recommendations for a target entity (e.g., user or listing) based on affinity scores derived from collaborative signals such as co-occurrence or interaction overlap.

- **targetId**: The unique identifier of the target entity (user or listing).
- **maxResults**: The maximum number of scored listings to return. Defaults to 20.
- Returns: An asynchronous task that resolves to a read-only list of `ScoredListing` items, ordered by affinity.
- Throws: `ArgumentException` if `targetId` is empty.
- Throws: `InvalidOperationException` if affinity data is not available for the target.

### `public async Task RecordSignalAsync(Guid userId, Guid listingId, SignalType signal)`

Records a user interaction signal (e.g., view, click, purchase) to improve future recommendations.

- **userId**: The unique identifier of the user who performed the action.
- **listingId**: The unique identifier of the listing involved in the action.
- **signal**: The type of signal being recorded (e.g., view, click, purchase).
- Returns: An asynchronous task that completes when the signal is recorded.
- Throws: `ArgumentException` if either `userId` or `listingId` is empty.
- Throws: `InvalidOperationException` if the signal cannot be stored due to system constraints.

## Usage

### Example 1: Personalized Recommendations for a User
