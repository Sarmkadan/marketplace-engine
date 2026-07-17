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