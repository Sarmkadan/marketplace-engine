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
