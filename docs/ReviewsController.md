# ReviewsController

The `ReviewsController` class serves as the primary HTTP entry point for managing review-related operations within the `marketplace-engine` application. It encapsulates the business logic required to create, retrieve, summarize, and moderate reviews for sellers and listings, while also facilitating seller responses and administrative actions such as flagging or removing inappropriate content. All operations are implemented asynchronously to ensure non-blocking I/O performance during database interactions.

## API

### `public ReviewsController`
Initializes a new instance of the `ReviewsController` class. This constructor typically injects required dependencies such as data contexts or service layers necessary for processing review transactions.

### `public async Task<IActionResult> CreateReview`
Creates a new review entry for a specific listing or seller.
*   **Parameters**: Accepts a request model containing the review content, rating, and target entity identifiers (typically via the request body).
*   **Return Value**: Returns an `IActionResult`. On success, it typically returns a `CreatedAtAction` or `OkObjectResult` containing the created review data. On failure, it returns appropriate error statuses (e.g., `BadRequest`, `Unauthorized`).
*   **Exceptions**: May throw exceptions if the target listing or seller does not exist, if the user is not authorized to review the entity, or if duplicate review constraints are violated.

### `public async Task<IActionResult> GetReview`
Retrieves a single review by its unique identifier.
*   **Parameters**: Requires the unique ID of the review to fetch.
*   **Return Value**: Returns an `IActionResult`. If found, returns `OkObjectResult` with the review details. If not found, returns `NotFound`.
*   **Exceptions**: Throws validation exceptions if the provided ID format is invalid.

### `public async Task<IActionResult> GetSellerReviews`
Fetches a collection of reviews associated with a specific seller.
*   **Parameters**: Requires the seller's unique identifier and optional pagination parameters (page number, page size).
*   **Return Value**: Returns an `IActionResult` containing a paginated list of reviews (`OkObjectResult`).
*   **Exceptions**: May throw if the seller ID does not correspond to an existing record.

### `public async Task<IActionResult> GetListingReviews`
Fetches a collection of reviews associated with a specific marketplace listing.
*   **Parameters**: Requires the listing's unique identifier and optional pagination parameters.
*   **Return Value**: Returns an `IActionResult` containing a paginated list of reviews.
*   **Exceptions**: May throw if the listing ID is invalid or the listing has been permanently deleted.

### `public async Task<IActionResult> GetSellerSummary`
Generates an aggregated summary of a seller's reputation based on their reviews.
*   **Parameters**: Requires the seller's unique identifier.
*   **Return Value**: Returns an `IActionResult` containing statistical data such as average rating, total review count, and rating distribution.
*   **Exceptions**: Throws if the seller does not exist.

### `public async Task<IActionResult> AddSellerReply`
Allows a seller to post a public reply to a specific review.
*   **Parameters**: Requires the review ID and the reply content.
*   **Return Value**: Returns an `IActionResult`. Success yields `Ok` or `NoContent`. Failure yields `BadRequest` if the user is not the owner of the listing associated with the review.
*   **Exceptions**: Throws if the review already has a reply (if single-reply policy is enforced) or if the review does not exist.

### `public async Task<IActionResult> FlagReview`
Marks a review as inappropriate or suspicious for moderator inspection.
*   **Parameters**: Requires the review ID and a reason code or description for the flag.
*   **Return Value**: Returns an `IActionResult` indicating the flag was successfully recorded (`Accepted` or `Ok`).
*   **Exceptions**: Throws if the user attempting to flag is the author of the review or if the review is already flagged by the same user.

### `public async Task<IActionResult> RemoveReview`
Permanently deletes a review from the system.
*   **Parameters**: Requires the review ID.
*   **Return Value**: Returns an `IActionResult`. Success yields `NoContent`.
*   **Exceptions**: Throws `Forbidden` or `Unauthorized` if the caller lacks administrative privileges or does not own the review. Throws if the review ID is invalid.

## Usage

### Example 1: Creating a Review and Retrieving Seller Summary
This example demonstrates posting a new review for a listing and subsequently fetching the updated summary for the seller.

```csharp
// Assuming an instantiated ReviewsController with dependencies injected
var controller = new ReviewsController(reviewService, userService);

// Create a new review
var reviewRequest = new CreateReviewDto 
{ 
    ListingId = 101, 
    Rating = 5, 
    Comment = "Excellent product quality." 
};

var createResult = await controller.CreateReview(reviewRequest);

if (createResult is CreatedAtActionResult created)
{
    var reviewData = created.Value;
    
    // Fetch the updated seller summary based on the review's seller ID
    int sellerId = ((ReviewDto)reviewData).SellerId;
    var summaryResult = await controller.GetSellerSummary(sellerId);

    if (summaryResult is OkObjectResult summaryOk)
    {
        var summary = (SellerSummaryDto)summaryOk.Value;
        Console.WriteLine($"New Average Rating: {summary.AverageRating}");
    }
}
```

### Example 2: Replying to a Review and Handling Errors
This example shows a seller replying to a customer review and handling potential validation errors.

```csharp
var controller = new ReviewsController(reviewService, userService);
int targetReviewId = 550;
var replyRequest = new SellerReplyDto { Content = "Thank you for your feedback." };

try
{
    var replyResult = await controller.AddSellerReply(targetReviewId, replyRequest);

    if (replyResult is OkResult)
    {
        Console.WriteLine("Reply posted successfully.");
    }
    else if (replyResult is BadRequestObjectResult badRequest)
    {
        Console.WriteLine($"Failed to post reply: {badRequest.Value}");
    }
}
catch (Exception ex)
{
    // Handle unexpected runtime exceptions or database connectivity issues
    Console.WriteLine($"Critical error occurred: {ex.Message}");
}
```

## Notes

*   **Concurrency**: As all public methods are asynchronous (`async Task`), the controller is designed to handle concurrent requests without blocking threads. However, underlying data operations (such as updating aggregate ratings in `CreateReview` or checking duplicate flags in `FlagReview`) must rely on database-level transactions or optimistic concurrency controls to prevent race conditions.
*   **Statelessness**: The controller does not maintain internal state between requests. Each method invocation is independent, relying entirely on the provided parameters and the injected services.
*   **Edge Cases**:
    *   `GetSellerReviews` and `GetListingReviews` may return empty collections rather than errors if no reviews exist for the specified ID; consumers should handle empty lists gracefully.
    *   `RemoveReview` operations should be treated as irreversible; ensure confirmation logic exists on the client side before invocation.
    *   `AddSellerReply` may fail silently or return specific error codes if the seller attempts to reply to their own review or if the review is already removed.
*   **Authorization**: While the signatures do not explicitly show authorization attributes, implementations of `CreateReview`, `AddSellerReply`, and `RemoveReview` implicitly depend on the current user context (e.g., `HttpContext.User`) to validate permissions. Calls made without valid authentication tokens will result in `Unauthorized` results.
