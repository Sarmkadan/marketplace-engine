# CollaborativeFilteringEngineExtensions

The `CollaborativeFilteringEngineExtensions` class provides a suite of static extension methods designed to enhance marketplace personalization by abstracting interactions with the underlying collaborative filtering engine. These utilities streamline the retrieval of personalized and platform-wide recommendations, enabling developers to integrate sophisticated content discovery features such as user-specific suggestions, similarity-based matching, trending items, and affinity-based ranking, while also providing a mechanism for recording interaction signals to improve future recommendation relevance.

## API

### ComputeForUserAsync
Calculates a personalized list of `ScoredListing` objects tailored to a specific user based on historical interaction data and preferences.
*   **Returns:** A `Task` representing the asynchronous operation, containing an `IReadOnlyList<ScoredListing>` of recommended items.
*   **Throws:** `ArgumentNullException` if the user identifier is null.

### ComputeSimilarAsync
Retrieves a list of `ScoredListing` objects that are contextually similar to a specified reference listing.
*   **Returns:** A `Task` representing the asynchronous operation, containing an `IReadOnlyList<ScoredListing>` of similar items.
*   **Throws:** `ArgumentNullException` if the reference listing identifier is null.

### ComputeTrendingAsync
Identifies and returns a list of `ScoredListing` objects currently trending across the platform, based on recent engagement metrics.
*   **Returns:** A `Task` representing the asynchronous operation, containing an `IReadOnlyList<ScoredListing>` of trending items.

### ComputeByAffinityAsync
Generates recommendations by analyzing inferred affinity patterns between the user and specific categories or listing attributes.
*   **Returns:** A `Task` representing the asynchronous operation, containing an `IReadOnlyList<ScoredListing>` of recommended items based on affinity.

### RecordSignalAsync
Persists a user interaction signal (e.g., click, view, purchase) to be used for model training and recommendation refinement.
*   **Returns:** A `Task` representing the completion of the signal recording operation.

## Usage

```csharp
// Example 1: Retrieving personalized recommendations for a user
var userId = "user_12345";
IReadOnlyList<ScoredListing> recommendations = await CollaborativeFilteringEngineExtensions.ComputeForUserAsync(engine, userId);

foreach (var listing in recommendations)
{
    Console.WriteLine($"Recommended: {listing.Title} (Score: {listing.Score})");
}
```

```csharp
// Example 2: Recording a user interaction signal
var signal = new UserInteractionSignal("user_12345", "listing_abc", SignalType.View);
await CollaborativeFilteringEngineExtensions.RecordSignalAsync(engine, signal);
```

## Notes

*   **Thread Safety:** As these methods are static extensions, they rely on the thread-safety of the underlying `CollaborativeFilteringEngine` instance provided as the first argument. Ensure the engine implementation is thread-safe if utilized in a concurrent environment.
*   **Edge Cases:** Methods returning lists (`Compute...Async`) will return an empty `IReadOnlyList<ScoredListing>` if no relevant items are found, rather than returning `null`.
*   **Asynchronous Execution:** All methods are asynchronous; ensure proper awaiting to avoid blocking the calling thread, especially when integrating with I/O-bound operations like database or service calls underlying the recommendation engine.
