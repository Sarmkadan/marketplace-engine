# RecommendationService

Provides asynchronous methods to retrieve and track user-specific recommendation feeds for listings in the marketplace, including affinity-based, similarity-based, and trending recommendations.

## API

### `RecommendationService`

Public constructor for the recommendation service. Dependencies such as data access and recommendation providers are injected via constructor parameters.

---

### `public async Task<RecommendationFeedDto> GetRecommendationsForUserAsync()`

Retrieves a consolidated recommendation feed for the specified user, combining multiple recommendation strategies (e.g., affinity, similarity, trending) into a single ranked result set.

- **Parameters**: None
- **Return value**: A `Task<RecommendationFeedDto>` resolving to a `RecommendationFeedDto` containing a paginated list of recommended listings.
- **Exceptions**: May throw `ArgumentNullException` if required user context is missing; may throw `InvalidOperationException` if the recommendation engine is not initialized.

---

### `public async Task<RecommendationFeedDto> GetAffinityRecommendationsAsync()`

Retrieves listings recommended based on the user’s historical behavior, preferences, and engagement patterns.

- **Parameters**: None
- **Return value**: A `Task<RecommendationFeedDto>` resolving to a `RecommendationFeedDto` with affinity-weighted listings.
- **Exceptions**: May throw `InvalidOperationException` if user affinity data is unavailable or corrupted.

---

### `public async Task<RecommendationFeedDto> GetSimilarListingsAsync()`

Retrieves listings similar to those the user has previously viewed or interacted with, using content-based or collaborative filtering similarity.

- **Parameters**: None
- **Return value**: A `Task<RecommendationFeedDto>` resolving to a `RecommendationFeedDto` containing similar listings.
- **Exceptions**: May throw `InvalidOperationException` if the similarity index is not ready or if no seed listings are available for comparison.

---
### `public async Task<RecommendationFeedDto> GetTrendingListingsAsync()`

Retrieves currently trending listings in the marketplace, potentially filtered by user location or category context.

- **Parameters**: None
- **Return value**: A `Task<RecommendationFeedDto>` resolving to a `RecommendationFeedDto` with trending listings.
- **Exceptions**: May throw `InvalidOperationException` if trend data cannot be computed or retrieved.

---
### `public async Task TrackUserActivityAsync()`

Records user actions (e.g., views, clicks, purchases) to inform future recommendation models. This call is idempotent and non-blocking.

- **Parameters**: None
- **Return value**: A `Task` representing the asynchronous operation.
- **Exceptions**: May throw `ArgumentException` if the tracked activity payload is invalid; may throw `InvalidOperationException` if the tracking service is unavailable.

## Usage
