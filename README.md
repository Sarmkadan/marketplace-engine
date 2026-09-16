## MessageTests

The `MessageTests` class provides comprehensive unit tests for the `Message` model's validation logic, specifically for the `ValidateBeforeSending` method. It ensures that messages adhere to business rules regarding sender/recipient identity, subject length, body length, and attachment limits by asserting that appropriate `ArgumentException`s are thrown when validation fails.

### Usage Example

```csharp
using MarketplaceEngine.Domain.Models;
using MarketplaceEngine.Tests;
using System;
using System.Collections.Generic;
using Xunit;
using FluentAssertions;

// Create a valid base message
var message = new Message
{
    SenderId = Guid.NewGuid(),
    RecipientId = Guid.NewGuid(),
    Subject = "Test Subject",
    Body = "Test Body"
};

// Example: Testing validation constraints
// 1. Validate when sender and recipient are the same
message.RecipientId = message.SenderId;
Assert.Throws<ArgumentException>(() => message.ValidateBeforeSending());

// 2. Validate when subject is too short
message.RecipientId = Guid.NewGuid();
message.Subject = "Ab";
Assert.Throws<ArgumentException>(() => message.ValidateBeforeSending());

// 3. Validate when subject is too long
message.Subject = new string('a', 101);
Assert.Throws<ArgumentException>(() => message.ValidateBeforeSending());

// 4. Validate when body is too short
message.Subject = "Test Subject";
message.Body = "Ab";
Assert.Throws<ArgumentException>(() => message.ValidateBeforeSending());

// 5. Validate when body is too long
message.Body = new string('a', 5001);
Assert.Throws<ArgumentException>(() => message.ValidateBeforeSending());

// 6. Validate when too many attachments
message.Body = "Test Body";
message.AttachmentUrls = new List<string>(new[] { "url1", "url2", "url3", "url4", "url5", "url6" });
Assert.Throws<ArgumentException>(() => message.ValidateBeforeSending());
```

## ListingServiceExtendedTestsExtensions

The `ListingServiceExtendedTestsExtensions` class provides a set of extension methods to simplify unit testing of `ListingService`. It facilitates the creation of `Mock<IListingRepository>` setups for various operations and provides helper methods to generate valid `User` and `Listing` instances for test scenarios.

### Usage Example

```csharp
using Moq;
using MarketplaceEngine.Domain.Models;
using MarketplaceEngine.Repositories;
using MarketplaceEngine.Tests;
using Xunit;

// Setup a mock repository
var mockRepo = new Mock<IListingRepository>();
var sellerId = Guid.NewGuid();

// Use extension methods to setup mock behaviors
mockRepo.SetupCreateListingAsync(sellerId)
        .SetupIncrementViewCountAsync(Guid.NewGuid(), 0);

// Use extension methods to create test data
var seller = default(User).CreateActiveSeller();
var listing = default(Listing).CreateActiveListing(sellerId: seller.Id);

Assert.True(seller.IsActive);
Assert.Equal("Valid Listing Title", listing.Title);
```

## ModerationControllerValidation

The `ModerationControllerValidation` class provides validation methods for moderation-related data, including report IDs, pagination parameters, action notes, rejection reasons, and bulk moderation operations. It ensures that moderation data meets expected formats and constraints before processing.

### Usage Example

```csharp
using MarketplaceEngine.Controllers;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;

// Example 1: Validate a single report ID
var reportId = "rep_1234567890";
var isValidReportId = ModerationControllerValidation.IsValidReportId(reportId);
if (isValidReportId)
{
    Console.WriteLine("Report ID is valid");
}
else
{
    Console.WriteLine("Report ID is invalid");
}

// Example 2: Validate pagination parameters
var pageNumber = 1;
var pageSize = 25;
var paginationErrors = ModerationControllerValidation.ValidatePagination(pageNumber, pageSize);
if (paginationErrors.Count == 0)
{
    Console.WriteLine("Pagination parameters are valid");
}
else
{
    Console.WriteLine("Pagination errors:");
    foreach (var error in paginationErrors)
    {
        Console.WriteLine($"- {error}");
    }
}

// Example 3: Validate action notes
var actionNotes = "This content violates our community guidelines by containing inappropriate language.";
var notesErrors = ModerationControllerValidation.ValidateActionNotes(actionNotes);
if (notesErrors.Count == 0)
{
    Console.WriteLine("Action notes are valid");
}
else
{
    Console.WriteLine("Action notes validation failed:");
    foreach (var error in notesErrors)
    {
        Console.WriteLine($"- {error}");
    }
}

// Example 4: Validate rejection reason
var rejectionReason = "Violation of content policy section 3.2";
var rejectionErrors = ModerationControllerValidation.ValidateRejectionReason(rejectionReason);
if (rejectionErrors.Count == 0)
{
    Console.WriteLine("Rejection reason is valid");
}
else
{
    Console.WriteLine("Rejection reason validation failed:");
    foreach (var error in rejectionErrors)
    {
        Console.WriteLine($"- {error}");
    }
}

// Example 5: Validate bulk moderation operation
var bulkOperation = new Dictionary<string, object>
{
    { "reportId", "rep_1234567890" },
    { "action", "Reject" },
    { "reason", "Violation of content policy" }
};

var bulkErrors = ModerationControllerValidation.ValidateBulkModeration(bulkOperation);
if (bulkErrors.Count == 0)
{
    Console.WriteLine("Bulk moderation operation is valid");
}
else
{
    Console.WriteLine("Bulk moderation validation failed:");
    foreach (var error in bulkErrors)
    {
        Console.WriteLine($"- {error}");
    }
}
```

## PaymentsControllerExtensions

The `PaymentsControllerExtensions` class provides extension methods for the `PaymentsController` that simplify payment operations including batch processing, status queries, and bulk operations. It enhances the controller with convenience methods for handling multiple payments simultaneously, filtering payments by status and date range, and managing payment cancellations in bulk.

### Usage Example

```csharp
using MarketplaceEngine.Controllers;
using MarketplaceEngine.DTOs;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

// Initialize controller (typically via dependency injection)
var controller = new PaymentsController();

// Example 1: Initiate multiple payments in a single batch operation
var batchRequests = new List<InitiatePaymentRequest>
{
    new InitiatePaymentRequest
    {
        ListingId = Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6"),
        BuyerId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
        Amount = new Money(99.99m, "USD"),
        Currency = "USD"
    },
    new InitiatePaymentRequest
    {
        ListingId = Guid.Parse("4fa85f64-5717-4562-b3fc-2c963f66afa6"),
        BuyerId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
        Amount = new Money(149.99m, "USD"),
        Currency = "USD"
    }
};

var batchResult = await controller.InitiateBatchPayments(batchRequests);
if (batchResult is ObjectResult objectResult && objectResult.Value is List<Guid> paymentIds)
{
    Console.WriteLine($"Successfully initiated {paymentIds.Count} payments:");
    foreach (var paymentId in paymentIds)
    {
        Console.WriteLine($"- Payment ID: {paymentId}");
    }
}

// Example 2: Check payment status for a specific payment
var paymentId = Guid.Parse("5fa85f64-5717-4562-b3fc-2c963f66afa6");
var statusResult = await controller.GetPaymentStatus(paymentId);
if (statusResult is OkObjectResult statusOkResult && statusOkResult.Value is object statusData)
{
    Console.WriteLine($"\nPayment Status:");
    Console.WriteLine($"- Payment ID: {statusData.PaymentId}");
    Console.WriteLine($"- Status: {statusData.Status}");
    Console.WriteLine($"- Amount: {statusData.Amount} {statusData.Currency}");
    Console.WriteLine($"- Created: {statusData.CreatedAt:yyyy-MM-dd HH:mm:ss}");
    if (statusData.CompletedAt.HasValue)
    {
        Console.WriteLine($"- Completed: {statusData.CompletedAt.Value:yyyy-MM-dd HH:mm:ss}");
    }
}

// Example 3: Cancel multiple payments in a single batch operation
var paymentsToCancel = new List<Guid>
{
    Guid.Parse("6fa85f64-5717-4562-b3fc-2c963f66afa6"),
    Guid.Parse("7fa85f64-5717-4562-b3fc-2c963f66afa6")
};

var cancelResult = await controller.CancelBatchPayments(paymentsToCancel, 
    requesterId: Guid.Parse("11111111-1111-1111-1111-111111111111"));
if (cancelResult is OkObjectResult cancelOkResult)
{
    Console.WriteLine($"\nBatch cancellation completed: {cancelOkResult.Value}");
}

// Example 4: Get payments filtered by status and date range
var startDate = DateTime.UtcNow.AddDays(-30);
var endDate = DateTime.UtcNow;

var filteredResult = await controller.GetPaymentsByStatusAndDate(
    status: "Completed",
    startDate: startDate,
    endDate: endDate
);
if (filteredResult is OkObjectResult filteredOkResult && filteredOkResult.Value is List<PaymentDto> filteredPayments)
{
    Console.WriteLine($"\nFound {filteredPayments.Count} completed payments between {startDate:yyyy-MM-dd} and {endDate:yyyy-MM-dd}:");
    foreach (var payment in filteredPayments.Take(5))
    {
        Console.WriteLine($"- Payment {payment.Id}: {payment.Amount} {payment.Currency} - {payment.Status}");
    }
}
```

## ReviewsControllerExtensions

The `ReviewsControllerExtensions` class provides extension methods for the `ReviewsController` that simplify common review operations including filtering by score, batch processing for multiple sellers, and retrieving summary statistics. It enhances the controller with convenience methods for handling reviews across multiple sellers in a single request and filtering reviews by score ranges.

### Usage Example

```csharp
using MarketplaceEngine.Controllers;
using MarketplaceEngine.DTOs;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// Initialize controller (typically via dependency injection)
var controller = new ReviewsController();

// Example 1: Get seller reviews filtered by minimum score
var sellerId = Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6");
var minScore = 4;
var page = 1;
var pageSize = 25;

var filteredReviewsResult = await controller.GetSellerReviewsByMinScore(
    sellerId, minScore, page, pageSize);

if (filteredReviewsResult is OkObjectResult okResult && okResult.Value is PaginatedResponse<ReviewDto> paginatedResponse)
{
    Console.WriteLine($"Found {paginatedResponse.Total} reviews with score >= {minScore}:");
    foreach (var review in paginatedResponse.Items.Take(5))
    {
        Console.WriteLine($"- Review {review.Id}: Score {review.Score} - {review.Comment}");
    }
}

// Example 2: Get reviews for multiple sellers in a single batch
var sellerIds = new List<Guid>
{
    Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6"),
    Guid.Parse("4fa85f64-5717-4562-b3fc-2c963f66afa6")
};

var batchReviewsResult = await controller.GetMultipleSellersReviews(sellerIds, page, pageSize);

if (batchReviewsResult is OkObjectResult batchOkResult && batchOkResult.Value is Dictionary<Guid, PaginatedResponse<ReviewDto>> batchResponse)
{
    Console.WriteLine($"\nBatch reviews for {batchResponse.Count} sellers:");
    foreach (var sellerReviews in batchResponse)
    {
        Console.WriteLine($"- Seller {sellerReviews.Key}: {sellerReviews.Value.Total} reviews");
    }
}

// Example 3: Get listing reviews filtered by score range
var listingId = Guid.Parse("5fa85f64-5717-4562-b3fc-2c963f66afa6");
var minReviewScore = 3;
var maxReviewScore = 5;

var scoreRangeResult = await controller.GetListingReviewsByScoreRange(
    listingId, minReviewScore, maxReviewScore);

if (scoreRangeResult is OkObjectResult rangeOkResult && rangeOkResult.Value is List<ReviewDto> rangeReviews)
{
    Console.WriteLine($"\nFound {rangeReviews.Count} reviews for listing with score between {minReviewScore} and {maxReviewScore}:");
    foreach (var review in rangeReviews.Take(5))
    {
        Console.WriteLine($"- Review {review.Id}: Score {review.Score}");
    }
}

// Example 4: Get summary statistics for multiple sellers
var summariesResult = await controller.GetMultipleSellersSummaries(sellerIds);

if (summariesResult is OkObjectResult summariesOkResult && summariesResult.Value is Dictionary<Guid, ReviewSummaryDto> summaries)
{
    Console.WriteLine($"\nSummary statistics for {summaries.Count} sellers:");
    foreach (var summary in summaries)
    {
        Console.WriteLine($"- Seller {summary.Key}:");
        Console.WriteLine($"  Average Score: {summary.Value.AverageScore:F2}");
        Console.WriteLine($"  Total Reviews: {summary.Value.TotalReviews}");
        Console.WriteLine($"  Score Distribution: [{string.Join(", ", summary.Value.ScoreDistribution.OrderBy(kv => kv.Key).Select(kv => $"{kv.Key}:{kv.Value}"))}]");
    }
}
```

## PaymentsControllerJsonExtensions

`PaymentsControllerJsonExtensions` provides JSON (de)serialization helpers for payment‑related DTOs. The static methods let you convert a `PaymentDto` to a JSON string, parse JSON back into the various request/response types, and safely attempt deserialization with `TryFromJson` overloads.

### Usage Example

```csharp
using System;
using MarketplaceEngine.Controllers;
using MarketplaceEngine.DTOs;

class JsonDemo
{
    static void Main()
    {
        // Create a sample PaymentDto (properties omitted for brevity)
        var payment = new PaymentDto
        {
            Id = Guid.NewGuid(),
            Amount = new Money(49.99m, "USD"),
            Status = "Pending"
        };

        // Serialize to JSON (indented for readability)
        string json = payment.ToJson(indented: true);
        Console.WriteLine("Serialized JSON:");
        Console.WriteLine(json);

        // Deserialize back to a PaymentDto
        var deserialized = PaymentsControllerJsonExtensions.FromJsonToPaymentDto(json);
        Console.WriteLine($"\nDeserialized Payment ID: {deserialized?.Id}");

        // Attempt to deserialize an InitiatePaymentRequest safely
        bool ok = PaymentsControllerJsonExtensions.TryFromJson(json, out InitiatePaymentRequest? request);
        Console.WriteLine($"\nTryFromJson for InitiatePaymentRequest succeeded: {ok}");
        if (ok && request != null)
        {
            Console.WriteLine($"Request ListingId: {request.ListingId}");
        }

        // Directly deserialize a CompletePaymentRequest
        var completeRequest = PaymentsControllerJsonExtensions.FromJsonToCompletePaymentRequest(json);
        Console.WriteLine($"\nCompletePaymentRequest is null: {completeRequest == null}");
    }
}
## SellerDashboardServiceTestsValidation

The `SellerDashboardServiceTestsValidation` class provides validation helpers for test scenarios related to the `SellerDashboardService`. It offers extension methods to validate `User` (seller), `Listing`, and `Payment` entities, ensuring they meet expected state requirements for testing. This validation class helps maintain consistency in test data and prevents invalid entities from being used in test scenarios.

### Usage Example

```csharp
using MarketplaceEngine.Domain.Models;
using MarketplaceEngine.Domain.ValueObjects;
using MarketplaceEngine.Tests;
using System;
using System.Linq;

// Example 1: Validate a seller entity
var seller = new User
{
    Id = Guid.NewGuid(),
    FullName = "John Doe",
    TotalSales = 1500.50m
};

var sellerProblems = seller.Validate();
if (sellerProblems.Count == 0)
{
    Console.WriteLine("Seller is valid for testing");
}
else
{
    Console.WriteLine("Seller validation problems:");
    foreach (var problem in sellerProblems)
    {
        Console.WriteLine($"- {problem}");
    }
}

// Example 2: Validate a listing entity
var listing = new Listing
{
    Id = Guid.NewGuid(),
    SellerId = seller.Id,
    Title = "Premium Widget",
    Price = new Money(99.99m, "USD"),
    ViewCount = 42
};

var listingProblems = listing.Validate();
if (listingProblems.Count == 0)
{
    Console.WriteLine("Listing is valid for testing");
}
else
{
    Console.WriteLine("Listing validation problems:");
    foreach (var problem in listingProblems)
    {
        Console.WriteLine($"- {problem}");
    }
}

// Example 3: Validate a payment entity
var payment = new Payment
{
    Id = Guid.NewGuid(),
    SellerId = seller.Id,
    Amount = new Money(99.99m, "USD"),
    PlatformFee = new Money(5.00m, "USD"),
    SellerPayout = new Money(94.99m, "USD"),
    CompletedAt = DateTime.UtcNow
};

var paymentProblems = payment.Validate();
if (paymentProblems.Count == 0)
{
    Console.WriteLine("Payment is valid for testing");
}
else
{
    Console.WriteLine("Payment validation problems:");
    foreach (var problem in paymentProblems)
    {
        Console.WriteLine($"- {problem}");
    }
}

// Example 4: Use IsValid extension methods
if (seller.IsValid())
{
    Console.WriteLine("Seller is valid using IsValid extension");
}

if (listing.IsValid())
{
    Console.WriteLine("Listing is valid using IsValid extension");
}

if (payment.IsValid())
{
    Console.WriteLine("Payment is valid using IsValid extension");
}

// Example 5: Use EnsureValid extension methods (throws on invalid)
try
{
    seller.EnsureValid();
    Console.WriteLine("Seller passed EnsureValid check");
}
catch (ArgumentException ex)
{
    Console.WriteLine($"Seller failed EnsureValid: {ex.Message}");
}

try
{
    listing.EnsureValid();
    Console.WriteLine("Listing passed EnsureValid check");
}
catch (ArgumentException ex)
{
    Console.WriteLine($"Listing failed EnsureValid: {ex.Message}");
}
```

## ValueObjectTestsExtensions

The `ValueObjectTestsExtensions` class provides extension methods for creating and asserting value objects in unit tests. It simplifies the creation of test instances for domain value objects like `Money`, `Rating`, and `Location`, and provides fluent assertion methods to verify their properties. This extension class is particularly useful for testing domain logic that depends on these value objects.

### Usage Example

```csharp
using FluentAssertions;
using MarketplaceEngine.Domain.ValueObjects;
using MarketplaceEngine.Tests;

class ValueObjectTestsDemo
{
    static void Main()
    {
        // Example 1: Create Money instances for testing
        var money1 = new ValueObjectTests().CreateMoney(99.99m, "USD");
        var money2 = new ValueObjectTests().CreateMoney(149.99m, "EUR");
        
        Console.WriteLine($"Money 1: {money1.Amount} {money1.CurrencyCode}");
        Console.WriteLine($"Money 2: {money2.Amount} {money2.CurrencyCode}");
        
        // Example 2: Create Rating instances for testing
        var rating1 = new ValueObjectTests().CreateRating(5, 100);
        var rating2 = new ValueObjectTests().CreateRating(4, 50);
        
        Console.WriteLine($"\nRating 1: Score {rating1.Score}, Total Reviews {rating1.TotalReviews}");
        Console.WriteLine($"Rating 2: Score {rating2.Score}, Total Reviews {rating2.TotalReviews}");
        
        // Example 3: Create Location instances for testing
        var location1 = new ValueObjectTests().CreateLocation(
            "New York", 
            "New York", 
            "US",
            "10001",
            40.7128,
            -74.0060
        );
        
        var location2 = new ValueObjectTests().CreateLocation(
            "London",
            "England",
            "GB",
            "SW1A 1AA"
        );
        
        Console.WriteLine($"\nLocation 1: {location1.City}, {location1.State}, {location1.CountryCode}");
        Console.WriteLine($"Location 2: {location2.City}, {location2.State}, {location2.CountryCode}");
        
        // Example 4: Assert Money equivalence
        var expectedMoney = new ValueObjectTests().CreateMoney(100.00m, "USD");
        var actualMoney = new ValueObjectTests().CreateMoney(100.00m, "USD");
        
        new ValueObjectTests().ShouldBeEquivalentTo(expectedMoney, actualMoney);
        Console.WriteLine("\nMoney equivalence assertion passed!");
        
        // Example 5: Assert Rating equivalence
        var expectedRating = new ValueObjectTests().CreateRating(5, 200);
        var actualRating = new ValueObjectTests().CreateRating(5, 200);
        
        new ValueObjectTests().ShouldBeEquivalentTo(expectedRating, actualRating);
        Console.WriteLine("Rating equivalence assertion passed!");
        
        // Example 6: Assert Location equivalence
        var expectedLocation = new ValueObjectTests().CreateLocation("Paris", "Ile-de-France", "FR", "75001");
        var actualLocation = new ValueObjectTests().CreateLocation("Paris", "Ile-de-France", "FR", "75001");
        
        new ValueObjectTests().ShouldBeEquivalentTo(expectedLocation, actualLocation);
        Console.WriteLine("Location equivalence assertion passed!");
    }
}
```

## PaymentService

The `PaymentService` manages the full lifecycle of payment transactions in the marketplace. It integrates with external payment providers via an abstraction layer, allowing real provider implementations to be swapped in without changing business logic.

### Purpose

Handles payment processing workflows including:
- Initiating payments for listing purchases
- Processing payments through external providers
- Managing escrow for buyer/seller protection
- Handling payment failures, cancellations, and refunds
- Tracking payment status and seller revenue

### Public API

| Method | Description |
|--------|-------------|
| `InitiatePaymentAsync(listingId, buyerId, paymentMethod, currency)` | Initiates a new payment for a listing purchase. Validates that the listing is active and the buyer is not the seller. |
| `StartProcessingAsync(paymentId, requesterId)` | Transitions a pending payment to processing state (call before provider charge). |
| `CompletePaymentAsync(paymentId, externalTransactionId)` | Completes a payment after the external provider confirms the charge. Marks the listing as delisted and increments the seller's sale count. |
| `MoveToEscrowAsync(paymentId)` | Moves a processing payment into escrow until delivery is confirmed. |
| `ReleaseEscrowAsync(paymentId, externalTransactionId)` | Releases escrowed funds to the seller after buyer delivery confirmation. |
| `FailPaymentAsync(paymentId, reason)` | Marks a payment as failed with a descriptive reason. |
| `CancelPaymentAsync(paymentId, requesterId)` | Cancels a pending or processing payment. |
| `RefundPaymentAsync(paymentId, reason)` | Refunds a completed payment to the buyer. |
| `GetPaymentAsync(paymentId)` | Retrieves a payment by ID. |
| `GetBuyerPaymentsAsync(buyerId)` | Retrieves all payments made by a specific buyer. |
| `GetSellerPaymentsAsync(sellerId)` | Retrieves all payments received by a specific seller. |
| `GetSellerRevenueAsync(sellerId)` | Retrieves total net revenue earned by a seller. |

### Dependencies

- `IPaymentRepository` - Data access for payment entities
- `IListingRepository` - Data access for listing entities (to verify listings and mark as sold)
- `IUserRepository` - Data access for user entities (to verify buyers/sellers and record sales)
- `ILogger<PaymentService>` - Logging for payment lifecycle events

### Usage Example

```csharp
using MarketplaceEngine.Services;
using MarketplaceEngine.Domain.ValueObjects;

// Example: Initiating a payment for a listing purchase
var paymentService = new PaymentService(
    paymentRepository,
    listingRepository,
    userRepository,
    logger
);

// Initiate payment
var payment = await paymentService.InitiatePaymentAsync(
    listingId: Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6"),
    buyerId: Guid.Parse("11111111-1111-1111-1111-111111111111"),
    paymentMethod: "credit_card",
    currency: "USD"
);

// Process payment through external provider (pseudo-code)
var providerResult = await externalPaymentProvider.Charge(
    amount: payment.Amount,
    paymentMethod: payment.PaymentMethod
);

if (providerResult.Success)
{
    // Mark payment as processing
    await paymentService.StartProcessingAsync(payment.Id, payment.BuyerId);
    
    // Complete payment after provider confirmation
    await paymentService.CompletePaymentAsync(payment.Id, providerResult.TransactionId);
}
else
{
    // Handle payment failure
    await paymentService.FailPaymentAsync(payment.Id, providerResult.ErrorMessage);
}
```

## ReviewService

The `ReviewService` manages buyer reviews for sellers and listings. It validates review submissions, prevents duplicate reviews for the same reviewer/seller/listing combination, supports seller replies and moderation, provides review queries and aggregate statistics, and keeps seller ratings synchronized after reviews are submitted or removed.

### Purpose

- Submit and validate reviews with scores from 1 to 5 and comments from 10 to 2,000 characters
- Prevent inactive users and sellers reviewing themselves from submitting reviews
- Prevent duplicate reviews for the same reviewer, seller, and optional listing
- Allow a reviewed seller to add one reply of 5 to 1,000 characters
- Retrieve reviews by ID, seller, or listing and calculate seller review statistics
- Flag reviews for moderation and allow moderators or administrators to remove them
- Recalculate and persist a seller's rounded aggregate rating after submission or removal

### Public API

| Method | Description |
|--------|-------------|
| `SubmitReviewAsync(reviewerId, sellerId, score, comment, listingId = null)` | Creates an active review after verifying the reviewer, seller, optional listing, and uniqueness of the reviewer/seller/listing combination. Updates the seller's aggregate rating. |
| `AddSellerReplyAsync(reviewId, sellerId, reply)` | Adds a reply to a review when `sellerId` matches the reviewed seller. |
| `GetReviewAsync(reviewId)` | Retrieves a review by ID or throws when it does not exist. |
| `GetSellerReviewsAsync(sellerId, pageNumber = 1, pageSize = 20)` | Verifies the seller exists, then returns a page of seller reviews and the total count. |
| `GetListingReviewsAsync(listingId)` | Verifies the listing exists, then returns all reviews for it. |
| `GetSellerStatsAsync(sellerId)` | Returns the seller's average score rounded to two decimal places, total review count, and score distribution for ratings 1 through 5. |
| `FlagReviewAsync(reviewId)` | Changes an active review to `UnderReview` and persists it. |
| `RemoveReviewAsync(reviewId, moderatorId)` | Allows a moderator or administrator to mark a review as `Removed`, then recalculates the seller's rating. |

### Dependencies

- `IReviewRepository` - Creates, reads, and updates reviews; checks for duplicates; retrieves seller/listing reviews; and provides paginated results
- `IUserRepository` - Verifies reviewers, sellers, and moderators and persists recalculated seller ratings
- `IListingRepository` - Verifies optional listing references and listing-specific review queries
- `Review`, `ReviewStatus`, and `Rating` - Enforce review/reply state and validation rules and represent the seller's aggregate rating
- `ResourceNotFoundException`, `UnauthorizedException`, `DuplicateResourceException`, and `MarketplaceException` - Report missing resources and rejected operations

## UserService

The `UserService` manages the user-account lifecycle and coordinates user persistence with the validation and state-transition rules defined by the `User` domain model. It covers registration and email verification, profile and account state changes, seller metrics and promotion, user queries, activity tracking, and access checks.

## MessagingService

The `MessagingService` manages user-to-user messaging functionality in the marketplace. It handles sending messages, retrieving conversations, managing message states (read/unread/flagged), and provides administrative functions for message moderation and cleanup.

### Purpose

- Send messages between users with optional listing context and attachments
- Retrieve received, sent, and unread messages for users
- Get conversations between two users or about specific listings
- Manage message states (mark as read/unread, flag/unflag)
- Add replies to existing messages
- Provide paginated message retrieval for efficient browsing
- Administrative functions: view flagged messages, cleanup old messages
- Authorization checks for message deletion and moderation operations

### Public API

| Method | Description |
|--------|-------------|
| `SendMessageAsync(senderId, recipientId, subject, body, listingId, attachments)` | Sends a message between users with validation. Optionally associates with a listing and includes attachments. |
| `GetReceivedMessagesAsync(userId)` | Retrieves all messages received by a user. |
| `GetSentMessagesAsync(userId)` | Retrieves all messages sent by a user. |
| `GetUnreadMessagesAsync(userId)` | Retrieves all unread messages for a user. |
| `GetConversationAsync(userId1, userId2)` | Retrieves the conversation between two users. |
| `MarkAsReadAsync(messageId)` | Marks a specific message as read. |
| `MarkMultipleAsReadAsync(messageIds)` | Marks multiple messages as read. |
| `MarkAsUnreadAsync(messageId)` | Marks a specific message as unread. |
| `FlagMessageAsync(messageId, flaggerId)` | Flags a message (requires valid flagger user). |
| `RemoveFlagAsync(messageId)` | Removes flag from a message. |
| `AddReplyAsync(parentMessageId, senderId, body, attachments)` | Adds a reply to an existing message. |
| `GetListingMessagesAsync(listingId)` | Retrieves all messages associated with a specific listing. |
| `GetListingConversationAsync(userId1, userId2, listingId)` | Retrieves conversation between two users about a specific listing. |
| `GetPaginatedMessagesAsync(userId, pageNumber, pageSize)` | Retrieves paginated messages for a user (offset-based). |
| `GetMessagesByCursorAsync(userId, afterId, pageSize)` | Retrieves paginated messages using cursor-based pagination to avoid duplicates on concurrent writes. |
| `GetConversationCountAsync(userId)` | Gets the count of conversations for a user. |
| `DeleteMessageAsync(messageId, requesterId)` | Deletes a message if the requester is sender or recipient. |
| `GetFlaggedMessagesAsync()` | Retrieves all flagged messages (admin only). |
| `CleanupOldMessagesAsync(retentionDays)` | Deletes messages older than specified retention days (admin only). |

### Dependencies

- `IMessageRepository` - Data access for message entities
- `IUserRepository` - Data access for user entities (to validate sender/recipient existence)
- `Message` - Domain model representing a message with validation and state methods

### Usage Example

```csharp
using MarketplaceEngine.Services;

// Example: Sending a message between users
var messagingService = new MessagingService(
    messageRepository,
    userRepository
);

// Send a message
var message = await messagingService.SendMessageAsync(
    senderId: Guid.Parse("11111111-1111-1111-1111-111111111111"),
    recipientId: Guid.Parse("22222222-2222-2222-2222-222222222222"),
    subject: "Question about your listing",
    body: "Is this item still available?",
    listingId: Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6")
);

// Get user's unread messages
var unreadMessages = await messagingService.GetUnreadMessagesAsync(
    Guid.Parse("22222222-2222-2222-2222-222222222222")
);

// Mark a message as read
await messagingService.MarkAsReadAsync(message.Id);

// Add a reply to the message
var reply = await messagingService.AddReplyAsync(
    parentMessageId: message.Id,
    senderId: Guid.Parse("22222222-2222-2222-2222-222222222222"),
    body: "Yes, it's still available!"
);
```

### Purpose

- Register active, unverified users with the default `User` role and a newly generated verification token
- Retrieve users by ID or email and report missing users consistently
- Validate and persist profile changes, email verification, account activation, sales, and ratings
- Promote eligible users with at least five sales and a rating of 4 or higher to premium seller status
- Provide top-seller, paginated-user, active-user, and verified-user queries
- Track public-profile access and reject marketplace actions by inactive or unverified users

### Public API

| Member | Description |
|--------|-------------|
| `UserService(IUserRepository userRepository)` | Creates the service with its required repository; throws `ArgumentNullException` when the repository is `null`. |
| `RegisterUserAsync(email, fullName, phone = null)` | Rejects a duplicate email, creates and validates an active, unverified regular user, generates a verification token, and persists the user. |
| `GetUserAsync(userId)` | Retrieves a user by ID or throws `ResourceNotFoundException`. |
| `GetUserByEmailAsync(email)` | Retrieves a user by email or throws `ResourceNotFoundException`. |
| `UpdateProfileAsync(userId, fullName = null, phone = null, bio = null, location = null)` | Applies the supplied profile fields, converts blank phone and bio values to `null`, validates the resulting profile, and persists it. A `null` argument leaves that field unchanged. |
| `VerifyEmailAsync(userId, verificationToken)` | Verifies and persists the user when the token is accepted; returns `false` without updating when the token is invalid or expired. An already verified user returns `true` and is persisted. |
| `ResendVerificationTokenAsync(email)` | Generates and persists a new verification token for an unverified user; throws when the user is missing or already verified. |
| `PromoteToPremiumAsync(userId)` | Promotes and persists an eligible regular user; requires at least five sales and a rating score of at least 4. |
| `DeactivateAccountAsync(userId)` | Deactivates the user, updates the domain timestamps, and persists the change. |
| `ReactivateAccountAsync(userId)` | Reactivates and persists an inactive user; returns an already active user without writing to the repository. |
| `RecordSaleAsync(userId)` | Increments the user's total sales, updates the domain timestamp, and persists the change. |
| `UpdateRatingAsync(userId, rating)` | Replaces the user's rating, updates the domain timestamp, and persists the change. |
| `GetTopSellersAsync(limit = 10)` | Returns top sellers from the repository. Values outside 1 through 50 are replaced with the default limit of 10. |
| `GetPaginatedUsersAsync(pageNumber, pageSize)` | Returns the requested users and the total user count as `(items, total)`; pagination values are passed through to the repository. |
| `UpdateLastActivityAsync(userId)` | Delegates the last-activity timestamp update directly to the repository. |
| `GetVerifiedUserCountAsync()` | Retrieves verified users and returns their count. |
| `GetActiveUserCountAsync()` | Retrieves active users and returns their count. |
| `ValidateUserAccessAsync(userId)` | Throws `UnauthorizedException` when the user is inactive or has not verified their email. |
| `GetPublicProfileAsync(userId)` | Retrieves the user, updates their last-activity timestamp, and returns the profile. |

All operations are asynchronous. Methods that first load a user by ID inherit `GetUserAsync`'s `ResourceNotFoundException` behavior. Profile validation and domain state transitions can also propagate `ArgumentException` or `InvalidOperationException` from the `User` model.

### Dependencies

- `IUserRepository` - The constructor-injected persistence boundary used to look up, add, update, page, and count users, retrieve top sellers, and update activity timestamps
- `User` and `UserRole` - Represent the account and enforce profile, verification, promotion, activation, sale, and rating state transitions
- `Location` and `Rating` - Value objects accepted by profile and rating updates
- `DuplicateResourceException`, `ResourceNotFoundException`, and `UnauthorizedException` - Communicate duplicate registration, missing users, and denied access

## ModerationService

The `ModerationService` manages marketplace moderation and content review. It lets users report other users or listings, lets moderators and administrators triage those reports, and applies enforcement actions such as removing flagged content, suspending users, or banning users. It also supports bulk moderation of listings and provides report queries and statistics.

### Purpose

- File moderation reports against users or listings with a reason, optional details, and a priority
- Validate that the reporter is an active, verified user and that the target exists
- Assign reports to moderators or administrators and approve or reject them
- Remove flagged listing content, suspend users, or ban users as enforcement actions
- Escalate report priority and apply bulk moderation actions to listings
- Query pending reports, reports by status, and a moderator's assignments, plus report statistics

### Public API

| Method | Description |
|--------|-------------|
| `ReportUserAsync(reporterId, targetUserId, reason, details = null, priority = 1)` | Validates the reporter, verifies the target user exists, and creates a submitted moderation report. |
| `ReportListingAsync(reporterId, listingId, reason, details = null, priority = 1)` | Validates the reporter, verifies the target listing exists, and creates a submitted moderation report. |
| `AssignReportAsync(report, moderatorId)` | Assigns a report to a moderator or administrator; throws `UnauthorizedException` for other roles. |
| `ApproveReportAsync(report, reviewNotes = "")` | Approves a report; throws `InvalidOperationException` when it has not been assigned to a moderator. |
| `RejectReportAsync(report, reviewNotes = "")` | Rejects a report; throws `InvalidOperationException` when it has not been assigned to a moderator. |
| `RemoveContentAsync(report, reviewNotes = "")` | Flags and updates the target listing (when present) and marks the report's content as removed. |
| `SuspendUserAsync(report, reviewNotes = "")` | Deactivates the target user (when present) and marks the report as suspended. |
| `BanUserAsync(report, reviewNotes = "")` | Deactivates the target user (when present) and marks the report as banned. |
| `EscalateReportAsync(report)` | Escalates the report's priority. |
| `ApplyBulkActionAsync(listingId, action)` | Applies `approve`, `remove`, or `escalate` to a listing; throws `ValidationException` for unknown actions. |
| `GetPendingReportsAsync(page, pageSize)` | Returns pending reports ordered by most recently created, with pagination. |
| `GetReportAsync(id)` | Retrieves a moderation report by ID or `null` when not found. |
| `UpdateReportAsync(report)` | Replaces an existing report's state; throws `ResourceNotFoundException` when missing. |
| `CreateReportAsync(report)` | Persists a newly created report, assigning an ID when empty. |
| `GetReportsByStatusAsync(status)` | Returns reports matching a status, ordered by most recently created. |
| `GetModeratorAssignmentsAsync(moderatorId)` | Returns reports assigned to a moderator, ordered by most recently created. |
| `GetReportStatsAsync()` | Returns `(pending, inReview, resolved)` counts across report statuses. |

### Dependencies

- `IUserRepository` - Data access for user entities (to validate reporters, moderators, and targets and to apply suspension/ban actions)
- `IListingRepository` - Data access for listing entities (to verify listing targets and apply removal/approval actions)
- `MarketplaceDbContext` - The in-memory store of `ModerationReport` entities, accessed through a shared singleton instance
- `ModerationReport`, `ModerationStatus`, and `UserRole` - Enforce report state transitions and role checks
- `ResourceNotFoundException`, `UnauthorizedException`, `ValidationException`, and `InvalidOperationException` - Report missing resources, denied access, and invalid operations

### Usage Example

```csharp
using MarketplaceEngine.Services;

// Example: Reporting a listing and triaging the report
var moderationService = new ModerationService(
    userRepository,
    listingRepository
);

// A verified, active user files a report against a listing
var report = await moderationService.ReportListingAsync(
    reporterId: Guid.Parse("11111111-1111-1111-1111-111111111111"),
    listingId: Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6"),
    reason: "Inappropriate content",
    details: "Listing contains prohibited material",
    priority: 2
);

// A moderator picks up the report
var moderatorId = Guid.Parse("22222222-2222-2222-2222-222222222222");
await moderationService.AssignReportAsync(report, moderatorId);

// The moderator removes the flagged content
await moderationService.RemoveContentAsync(report, reviewNotes: "Confirmed violation");

// Check overall moderation load
var (pending, inReview, resolved) = await moderationService.GetReportStatsAsync();
Console.WriteLine($"Pending: {pending}, In review: {inReview}, Resolved: {resolved}");
```

## SearchService

The `SearchService` provides search and discovery across marketplace listings and users. It supports keyword and tag-based listing search, geolocation-based nearby search, category browsing with pagination, multi-filter advanced search, trending and autocomplete suggestions, and user search with top-seller ranking.

### Purpose

- Search listings by keyword, validating query length against `AppConstants` limits
- Search listings by one or more tags
- Find listings near a given latitude/longitude within a configurable radius
- Search users by name or email and retrieve top sellers by rating
- Browse listings by category with offset-based pagination
- Run advanced searches combining keyword, category, price range, and tag filters
- Return trending listings and autocomplete title suggestions

### Public API

| Method | Description |
|--------|-------------|
| `SearchListingsAsync(query)` | Searches listings by keyword. Throws `ValidationException` when the query is empty or outside the configured length bounds. |
| `SearchByTagsAsync(tags)` | Searches listings by tags. Throws `ValidationException` when no tags are supplied. |
| `FindNearbyListingsAsync(latitude, longitude, radiusKm = 10)` | Finds listings near the given coordinates. Throws `ValidationException` when latitude, longitude, or radius are out of range. |
| `SearchUsersAsync(query)` | Searches users by name or email. Throws `ValidationException` when the query is empty or outside the configured length bounds. |
| `GetTopSellersAsync(limit = 10)` | Returns top sellers by rating. Limits outside 1 through 50 are replaced with the default of 10. |
| `SearchByCategoryAsync(categoryId, pageNumber, pageSize)` | Returns `(items, total)` for listings in a category, ordered by `PublishedAt` descending. Throws `ValidationException` when `categoryId` is empty. Page number is clamped to 1; page size is clamped to `[1, MaxPageSize]` with a default of `DefaultPageSize`. |
| `AdvancedSearchAsync(keyword = null, categoryId = null, minPrice = null, maxPrice = null, tags = null)` | Filters active listings by keyword (title/description), category, price range, and tags, then orders by `ViewCount` descending. |
| `GetTrendingListingsAsync(limit = 20)` | Returns active listings ordered by `ViewCount` then `InterestCount`, taking up to `limit`. Limits outside 1 through 100 are replaced with the default of 20. |
| `GetSearchSuggestionsAsync(prefix, limit = 10)` | Returns distinct listing titles starting with the prefix, up to `limit`. Throws `ValidationException` when the prefix is empty or outside the configured length bounds. |

### Dependencies

- `IListingRepository` - Data access for listing search, tag, nearby, category, and active-listing queries
- `IUserRepository` - Data access for user search and top-seller queries
- `Listing` and `User` - Domain models returned by the search operations
- `ValidationException` - Reports invalid search queries, tags, coordinates, radius, and category IDs
- `AppConstants` - Provides search query length limits and default/maximum page sizes

### Usage Example

```csharp
using MarketplaceEngine.Services;

// Construct the service with its repositories (typically via dependency injection)
var searchService = new SearchService(listingRepository, userRepository);

// Keyword search
var listings = await searchService.SearchListingsAsync("laptop");

// Search by tags
var tagged = await searchService.SearchByTagsAsync(new List<string> { "electronics", "refurbished" });

// Nearby search within 25 km
var nearby = await searchService.FindNearbyListingsAsync(40.7128, -74.0060, radiusKm: 25);

// Category browsing with pagination
var (items, total) = await searchService.SearchByCategoryAsync(
    categoryId: Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6"),
    pageNumber: 1,
    pageSize: 20
);

// Advanced search combining filters
var advanced = await searchService.AdvancedSearchAsync(
    keyword: "camera",
    minPrice: 100m,
    maxPrice: 500m,
    tags: new List<string> { "photography" }
);

// Trending and autocomplete
var trending = await searchService.GetTrendingListingsAsync(limit: 10);
var suggestions = await searchService.GetSearchSuggestionsAsync("cam");

// User search and top sellers
var users = await searchService.SearchUsersAsync("john");
var topSellers = await searchService.GetTopSellersAsync(limit: 5);
```
