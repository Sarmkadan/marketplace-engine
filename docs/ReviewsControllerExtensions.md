# ReviewsControllerExtensions

The `ReviewsControllerExtensions` class provides a set of static extension methods designed to augment the functionality of ASP.NET Core controllers within the `marketplace-engine` project. Specifically, it encapsulates complex query logic for retrieving seller and listing reviews based on scoring criteria, aggregating data across multiple sellers, and generating summary statistics. By offloading these operations to extension methods, the class promotes code reusability and maintains a clean separation of concerns within the controller layer, ensuring that review retrieval patterns remain consistent across the application.

## API

### GetSellerReviewsByMinScore
Retrieves a collection of reviews for a specific seller where the rating meets or exceeds a defined minimum threshold.
*   **Purpose**: Filters reviews for a single seller to display only high-quality feedback.
*   **Parameters**: Accepts the controller instance, the seller's unique identifier, and the minimum score integer.
*   **Return Value**: Returns a `Task<IActionResult>` containing the filtered list of reviews or an appropriate status code (e.g., 404 if the seller is not found).
*   **Throws**: May throw exceptions related to database connectivity or invalid identifier formats if the underlying repository fails to validate input.

### GetMultipleSellersReviews
Fetches reviews for a specified list of sellers in a single operation.
*   **Purpose**: Efficiently aggregates review data for batch processing or comparative views involving multiple sellers.
*   **Parameters**: Accepts the controller instance and an enumerable collection of seller identifiers.
*   **Return Value**: Returns a `Task<IActionResult>` containing a aggregated dataset of reviews mapped to their respective sellers.
*   **Throws**: Throws an exception if the provided list of seller IDs is null or empty, or if the data source encounters a failure during the batch query.

### GetListingReviewsByScoreRange
Retrieves reviews associated with a specific listing that fall within a defined score range (inclusive).
*   **Purpose**: Allows clients to filter listing feedback by specific rating brackets, such as "critical issues" (1-2 stars) or "excellent" (4-5 stars).
*   **Parameters**: Accepts the controller instance, the listing identifier, the minimum score, and the maximum score.
*   **Return Value**: Returns a `Task<IActionResult>` with the subset of reviews matching the criteria.
*   **Throws**: May throw an argument exception if the minimum score is greater than the maximum score, or if the listing ID is invalid.

### GetMultipleSellersSummaries
Generates statistical summaries (e.g., average rating, total count) for a group of sellers without retrieving the full text of individual reviews.
*   **Purpose**: Optimizes performance for dashboard views or lists where only high-level metrics are required.
*   **Parameters**: Accepts the controller instance and a collection of seller identifiers.
*   **Return Value**: Returns a `Task<IActionResult>` containing a list of summary objects.
*   **Throws**: Throws if the input collection is null or if the aggregation service fails to compute metrics for the requested IDs.

## Usage

The following examples demonstrate how to invoke these extension methods within an ASP.NET Core controller inheriting from `ControllerBase`.

**Example 1: Retrieving high-rated reviews for a specific seller**

```csharp
[HttpGet("seller/{sellerId}/high-rated")]
public async Task<IActionResult> GetHighRatedSellerReviews(Guid sellerId)
{
    // Retrieves reviews with a score of 4 or higher
    return await this.GetSellerReviewsByMinScore(sellerId, 4);
}
```

**Example 2: Generating summary metrics for a batch of sellers**

```csharp
[HttpPost("sellers/summaries")]
public async Task<IActionResult> GetSellerMetrics([FromBody] List<Guid> sellerIds)
{
    if (sellerIds == null || !sellerIds.Any())
    {
        return BadRequest("Seller IDs are required.");
    }

    // Aggregates average ratings and counts for the provided list
    return await this.GetMultipleSellersSummaries(sellerIds);
}
```

## Notes

*   **Asynchronous Execution**: All methods are asynchronous and return a `Task<IActionResult>`. Callers must await these methods to prevent blocking the request thread, which is critical for maintaining scalability under load.
*   **Input Validation**: While the methods encapsulate logic, they rely on the validity of the passed identifiers. Passing null collections to `GetMultipleSellersReviews` or `GetMultipleSellersSummaries` will likely result in an immediate exception or a 400 Bad Request response depending on the internal implementation of the repository layer.
*   **Thread Safety**: As the class consists entirely of static methods that operate on stateless parameters and rely on scoped dependencies injected into the controller instance, the methods are thread-safe. However, the controller instance itself (`this`) is not thread-safe and should not be shared across concurrent requests.
*   **Edge Cases**: When using `GetListingReviewsByScoreRange`, ensure the min and max scores align with the system's defined rating scale (e.g., 1-5). Requesting a range outside these bounds may return empty results rather than an error, depending on the database query behavior.
