# ReviewExtensions

The `ReviewExtensions` static class provides a suite of extension methods designed to simplify interactions with `Review` objects in the `marketplace-engine`. These methods encapsulate common business logic for evaluating sentiment, calculating temporal metrics, determining the presence of seller interaction, and formatting review data for presentation layers.

## API

### `bool IsPositive(this Review review)`
Evaluates whether the review is classified as positive based on the underlying score.
- **Returns**: `true` if the review is positive; otherwise, `false`.

### `bool IsNegative(this Review review)`
Evaluates whether the review is classified as negative based on the underlying score.
- **Returns**: `true` if the review is negative; otherwise, `false`.

### `int GetAgeInDays(this Review review)`
Calculates the number of full days elapsed since the `Review` was created.
- **Returns**: An integer representing the age in days.

### `bool HasSellerReply(this Review review)`
Checks if the seller has provided a response to the review.
- **Returns**: `true` if a seller reply exists; otherwise, `false`.

### `string GetStatusString(this Review review)`
Retrieves the current status of the review as a human-readable string (e.g., "Pending", "Approved", "Flagged").
- **Returns**: The status string.

### `string GetScorePercentage(this Review review)`
Formats the review score as a percentage string (e.g., "85%").
- **Returns**: A formatted percentage string.

### `bool IsRecent(this Review review)`
Determines if the review is considered recent based on the system's defined temporal threshold.
- **Returns**: `true` if the review was created within the recent threshold; otherwise, `false`.

### `string GetCommentSummary(this Review review)`
Provides a shortened or summarized version of the review comment, suitable for UI display.
- **Returns**: A string containing the summarized comment.

## Usage

### Example 1: Basic Sentiment and Status Check
```csharp
var review = _reviewService.GetById(reviewId);

if (review.IsPositive())
{
    Console.WriteLine($"Review score: {review.GetScorePercentage()}");
}

if (!review.HasSellerReply())
{
    Console.WriteLine("Action required: Seller has not yet replied to this review.");
}
```

### Example 2: Filtering Recent Reviews
```csharp
var allReviews = _reviewService.GetAll();

var recentNegativeReviews = allReviews
    .Where(r => r.IsRecent() && r.IsNegative())
    .ToList();

foreach (var review in recentNegativeReviews)
{
    Console.WriteLine($"Alert: New negative review received: {review.GetCommentSummary()}");
}
```

## Notes

- **Null Reference Safety**: These extension methods do not internally check for `null` `Review` objects. Passing a `null` instance to any of these methods will result in a `NullReferenceException`. Ensure the `Review` instance is validated before invocation.
- **Thread Safety**: These extension methods are stateless and operate on the state of the provided `Review` instance. They are inherently thread-safe provided that the underlying `Review` object is not being concurrently modified by another thread.
- **Data Dependencies**: `GetAgeInDays` and `IsRecent` rely on the current system time at the moment of execution.
