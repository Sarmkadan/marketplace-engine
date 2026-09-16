# ReviewService

Manages the lifecycle of buyer reviews for sellers and listings. Enforces a one-review-per-transaction rule, computes aggregate scores, and keeps seller ratings in sync whenever a review is submitted or removed.

## API

### `ReviewService(IReviewRepository, IUserRepository, IListingRepository)`

Entry point for review operations. Consumes three repositories: a review store for persistence, a user store for reviewer/seller/moderation lookups, and a listing store for validating that a listing exists before a review is attached to it.

- **Parameters**
  - `reviewRepository` – Persistence for `Review` entities.
  - `userRepository` – Persistence for `User` entities (reviewers, sellers, moderators).
  - `listingRepository` – Persistence for `Listing` entities.
- **Exceptions**
  - Throws `ArgumentNullException` for any `null` repository argument.

### `async Task<Review> SubmitReviewAsync(Guid reviewerId, Guid sellerId, int score, string comment, Guid? listingId = null)`

Submits a new review for a seller. Validates that the reviewer exists and is active, is not reviewing themselves, and has not already reviewed this seller/listing combination. Updates the seller's aggregate rating after submission.

- **Parameters**
  - `reviewerId` – Unique identifier of the user leaving the review.
  - `sellerId` – Unique identifier of the seller being reviewed.
  - `score` – Numeric rating (validated by `Review.ValidateReview`).
  - `comment` – Free-text review comment.
  - `listingId` – Optional listing the review refers to.
- **Return value**
  - The newly created `Review` in `ReviewStatus.Active`.
- **Exceptions**
  - Throws `ArgumentNullException` if `comment` is `null`.
  - Throws `ResourceNotFoundException("User", ...)` if the reviewer or seller does not exist.
  - Throws `UnauthorizedException` if the reviewer is not active.
  - Throws `MarketplaceException` if the reviewer is the seller (self-review).
  - Throws `ResourceNotFoundException("Listing", ...)` if `listingId` is provided but the listing does not exist.
  - Throws `DuplicateResourceException` if the reviewer has already reviewed this seller/listing combination.
  - Throws validation exceptions from `Review.ValidateReview` for an invalid score.

### `async Task<Review> AddSellerReplyAsync(Guid reviewId, Guid sellerId, string reply)`

Adds a seller reply to an existing review. Only the seller of the reviewed listing may reply.

- **Parameters**
  - `reviewId` – Unique identifier of the review to reply to.
  - `sellerId` – Unique identifier of the seller replying.
  - `reply` – The reply text.
- **Return value**
  - The updated `Review` with the seller reply attached.
- **Exceptions**
  - Throws `ArgumentNullException` if `reply` is `null`.
  - Throws `ResourceNotFoundException("Review", ...)` if the review does not exist.
  - Throws `UnauthorizedException` if `sellerId` is not the review's seller.

### `async Task<Review> GetReviewAsync(Guid reviewId)`

Retrieves a review by its identifier.

- **Parameters**
  - `reviewId` – Unique identifier of the review.
- **Return value**
  - The matching `Review`.
- **Exceptions**
  - Throws `ResourceNotFoundException("Review", ...)` if the review does not exist.

### `async Task<(List<Review> items, int total)> GetSellerReviewsAsync(Guid sellerId, int pageNumber = 1, int pageSize = 20)`

Retrieves paginated reviews for a seller.

- **Parameters**
  - `sellerId` – Unique identifier of the seller.
  - `pageNumber` – One-based page index (default `1`).
  - `pageSize` – Number of reviews per page (default `20`).
- **Return value**
  - A tuple of the requested page of `Review` items and the total number of reviews.
- **Exceptions**
  - Throws `ResourceNotFoundException("User", ...)` if the seller does not exist.

### `async Task<List<Review>> GetListingReviewsAsync(Guid listingId)`

Retrieves all reviews for a specific listing.

- **Parameters**
  - `listingId` – Unique identifier of the listing.
- **Return value**
  - The list of `Review` objects for the listing.
- **Exceptions**
  - Throws `ResourceNotFoundException("Listing", ...)` if the listing does not exist.

### `async Task<(double averageScore, int total, Dictionary<int, int> distribution)> GetSellerStatsAsync(Guid sellerId)`

Returns aggregated review statistics for a seller.

- **Parameters**
  - `sellerId` – Unique identifier of the seller.
- **Return value**
  - A tuple of the average score (rounded to two decimals, `0.0` when there are no reviews), the total review count, and a dictionary mapping each score `1..5` to the number of reviews with that score.
- **Exceptions**
  - Throws `ResourceNotFoundException("User", ...)` if the seller does not exist.

### `async Task<Review> FlagReviewAsync(Guid reviewId)`

Flags a review for moderation.

- **Parameters**
  - `reviewId` – Unique identifier of the review to flag.
- **Return value**
  - The updated `Review` in its flagged state.
- **Exceptions**
  - Throws `ResourceNotFoundException("Review", ...)` if the review does not exist.

### `async Task<Review> RemoveReviewAsync(Guid reviewId, Guid moderatorId)`

Removes a review (moderator action). Updates the seller's aggregate rating afterwards.

- **Parameters**
  - `reviewId` – Unique identifier of the review to remove.
  - `moderatorId` – Unique identifier of the moderator or administrator performing the removal.
- **Return value**
  - The updated `Review` in its removed state.
- **Exceptions**
  - Throws `UnauthorizedException` if the moderator does not exist or is not a `Moderator`/`Administrator`.
  - Throws `ResourceNotFoundException("Review", ...)` if the review does not exist.

## Usage

```csharp
var reviewService = new ReviewService(reviewRepository, userRepository, listingRepository);

// Submit a review
var review = await reviewService.SubmitReviewAsync(reviewerId, sellerId, 5, "Great seller!", listingId);

// Retrieve a seller's stats
var stats = await reviewService.GetSellerStatsAsync(sellerId);
Console.WriteLine($"Average: {stats.averageScore}, Total: {stats.total}");
```