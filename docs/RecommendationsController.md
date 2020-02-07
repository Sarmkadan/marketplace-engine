# RecommendationsController

Provides HTTP endpoints for generating and retrieving personalized content recommendations. The controller exposes methods to fetch trending items, user-specific suggestions, affinity-based groupings, similar-item lookups, and activity tracking. It also includes a diagnostics endpoint for inspecting internal recommendation state.

## API

### `public RecommendationsController`

Constructor. Initializes the controller with its required dependencies (injected via the framework). No explicit parameters are documented here because dependency injection details are internal; consumers obtain an instance through the ASP.NET Core service container.

### `public async Task<IActionResult> GetTrending`

Returns a list of items currently trending across the platform. This endpoint does not require user context and typically serves global or category-scoped popularity data.

- **Parameters:** None (query-string filters such as category or time window may be bound by the framework from the HTTP request).
- **Returns:** `IActionResult` wrapping a collection of trending item representations. On success, HTTP 200 with the payload.
- **Throws:** May throw `InvalidOperationException` if the underlying recommendation service is unavailable. Argument exceptions can occur for malformed filter parameters.

### `public async Task<IActionResult> GetForUser`

Generates personalized recommendations for a specific user, identified by the current authentication context or an explicit user identifier parameter.

- **Parameters:** User identity is resolved from the request context. An optional `count` parameter limits the number of results.
- **Returns:** `IActionResult` containing a ranked list of recommended items. HTTP 200 on success; HTTP 401/403 if authentication is required but missing.
- **Throws:** `ArgumentException` if the user identifier is invalid. Service-level exceptions propagate as HTTP 500 responses.

### `public async Task<IActionResult> GetByAffinity`

Retrieves recommendations grouped by affinity signals such as shared interests, behavioral clusters, or collaborative filtering segments.

- **Parameters:** Expects an affinity key or category from the request. Optional `limit` and `offset` for pagination.
- **Returns:** `IActionResult` with affinity-grouped recommendation sets. HTTP 200 with the grouped payload.
- **Throws:** `ArgumentNullException` when the affinity key is missing. `TimeoutException` possible if the affinity computation exceeds the configured deadline.

### `public async Task<IActionResult> GetSimilar`

Given an item identifier, returns a set of similar items based on content attributes or co-occurrence patterns.

- **Parameters:** An item identifier (from route or query). Optional `count` to control result size.
- **Returns:** `IActionResult` with a list of similar items and their similarity scores. HTTP 200; HTTP 404 if the source item is unknown.
- **Throws:** `ArgumentException` for an empty or malformed item identifier.

### `public async Task<IActionResult> TrackActivity`

Records a user activity event (view, click, purchase, etc.) that feeds into the recommendation models. This is a fire-and-forget ingestion endpoint.

- **Parameters:** An activity payload in the request body containing user ID, item ID, event type, and timestamp.
- **Returns:** `IActionResult` with HTTP 202 (Accepted) on successful queuing. Validation errors return HTTP 400.
- **Throws:** `ArgumentNullException` if the activity payload is null. Serialization exceptions if the body cannot be parsed.

### `public Task<IActionResult> GetDiagnostics`

Exposes internal diagnostics such as model freshness, cache hit ratios, and service health checks. Intended for operational monitoring.

- **Parameters:** None.
- **Returns:** `Task<IActionResult>` wrapping a diagnostics snapshot. HTTP 200 with a structured diagnostics object.
- **Throws:** Does not throw by design; errors are captured in the diagnostics payload with a degraded status indicator.

## Usage

**Example 1: Fetch trending items for a category**

```csharp
// In a client or integration test, calling the endpoint via HttpClient
var response = await httpClient.GetAsync("/api/recommendations/trending?category=electronics");
response.EnsureSuccessStatusCode();
var trendingItems = await response.Content.ReadFromJsonAsync<List<TrendingItem>>();
foreach (var item in trendingItems)
{
    Console.WriteLine($"{item.Title} — {item.Score}");
}
```

**Example 2: Track a user view event, then retrieve personalized recommendations**

```csharp
// Track activity
var activityPayload = new
{
    UserId = "user-42",
    ItemId = "item-789",
    EventType = "view",
    Timestamp = DateTime.UtcNow
};
var trackResponse = await httpClient.PostAsJsonAsync("/api/recommendations/activity", activityPayload);
trackResponse.EnsureSuccessStatusCode();

// Later, fetch recommendations for the same user
var recsResponse = await httpClient.GetAsync("/api/recommendations/user?count=10");
var recommendations = await recsResponse.Content.ReadFromJsonAsync<List<Recommendation>>();
```

## Notes

- **Thread safety:** Controller instances are transient per request; no shared mutable state exists between invocations. All public methods are safe to call concurrently from separate HTTP requests.
- **Edge cases:**
  - `GetForUser` and `GetByAffinity` may return empty collections when insufficient data exists for the user or affinity group. This is a normal response (HTTP 200 with an empty array), not an error.
  - `GetSimilar` returns HTTP 404 when the source item has no interaction history or content embeddings, distinguishing “unknown item” from “no similar items found.”
  - `TrackActivity` accepts events out of temporal order; the ingestion pipeline handles deduplication and late-arriving data internally.
  - `GetDiagnostics` may report degraded status when downstream services are unreachable, but the endpoint itself always returns HTTP 200 to avoid masking the diagnostic payload behind transport errors.
- **Authentication:** Endpoints that resolve user identity (`GetForUser`, `GetByAffinity`) rely on the ambient authentication middleware. If anonymous access is configured, these endpoints may return empty or global-fallback results rather than throwing authorization errors.
