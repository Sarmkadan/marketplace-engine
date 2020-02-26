# PaymentsControllerExtensions

The `PaymentsControllerExtensions` static class provides extension methods for initiating, querying, and canceling batch payments in the marketplace engine. These methods encapsulate common payment workflow operations and return standard ASP.NET Core `IActionResult` responses, making them suitable for use directly in controller actions or middleware pipelines. Each method is asynchronous and follows the request-response pattern typical of web API endpoints.

## API

### `InitiateBatchPayments`

```csharp
public static async Task<IActionResult> InitiateBatchPayments(
    this ControllerBase controller,
    BatchPaymentRequest request,
    CancellationToken cancellationToken = default)
```

**Purpose**  
Creates a new batch payment from a collection of individual payment instructions. The batch is processed asynchronously; the method returns immediately with a reference to the batch for status tracking.

**Parameters**  
- `controller` – The controller instance on which the extension is invoked.  
- `request` – A `BatchPaymentRequest` object containing the list of payments to initiate. Must not be `null` and must contain at least one payment instruction.  
- `cancellationToken` – Optional token to cancel the operation.

**Returns**  
- `IActionResult` – On success, returns `201 Created` with a location header pointing to the batch resource and a body containing the batch identifier.  
- On validation failure, returns `400 BadRequest` with error details.  
- If the underlying payment provider is unavailable, returns `503 ServiceUnavailable`.

**Throws**  
- `ArgumentNullException` – If `request` is `null`.  
- `ArgumentException` – If `request.Payments` is empty or contains invalid entries.

---

### `GetPaymentStatus`

```csharp
public static async Task<IActionResult> GetPaymentStatus(
    this ControllerBase controller,
    string batchId,
    CancellationToken cancellationToken = default)
```

**Purpose**  
Retrieves the current status of a previously initiated batch payment.

**Parameters**  
- `controller` – The controller instance.  
- `batchId` – The unique identifier of the batch payment. Must not be `null` or empty.  
- `cancellationToken` – Optional cancellation token.

**Returns**  
- `IActionResult` – Returns `200 OK` with a `BatchStatusResponse` containing the overall batch status and per-payment details.  
- If `batchId` does not correspond to an existing batch, returns `404 NotFound`.  
- If the batch has been archived or expired, returns `410 Gone`.

**Throws**  
- `ArgumentNullException` – If `batchId` is `null`.  
- `ArgumentException` – If `batchId` is empty or contains invalid characters.

---

### `CancelBatchPayments`

```csharp
public static async Task<IActionResult> CancelBatchPayments(
    this ControllerBase controller,
    string batchId,
    CancellationToken cancellationToken = default)
```

**Purpose**  
Cancels an entire batch of payments that has not yet been fully processed. Only batches in a pending or partially completed state can be cancelled.

**Parameters**  
- `controller` – The controller instance.  
- `batchId` – The unique identifier of the batch to cancel. Must not be `null` or empty.  
- `cancellationToken` – Optional cancellation token.

**Returns**  
- `IActionResult` – Returns `200 OK` with a cancellation confirmation and the updated batch status.  
- If the batch is already completed or cancelled, returns `409 Conflict` with an explanation.  
- If `batchId` is unknown, returns `404 NotFound`.

**Throws**  
- `ArgumentNullException` – If `batchId` is `null`.  
- `ArgumentException` – If `batchId` is empty or malformed.

---

### `GetPaymentsByStatusAndDate`

```csharp
public static async Task<IActionResult> GetPaymentsByStatusAndDate(
    this ControllerBase controller,
    PaymentStatus status,
    DateTime from,
    DateTime to,
    CancellationToken cancellationToken = default)
```

**Purpose**  
Queries payments that match a given status and fall within a specified date range. Useful for reporting, auditing, or building dashboards.

**Parameters**  
- `controller` – The controller instance.  
- `status` – The `PaymentStatus` enum value to filter by (e.g., `Pending`, `Completed`, `Failed`).  
- `from` – The inclusive start of the date range.  
- `to` – The inclusive end of the date range. Must be greater than or equal to `from`.  
- `cancellationToken` – Optional cancellation token.

**Returns**  
- `IActionResult` – Returns `200 OK` with a list of matching payment records. The list may be empty.  
- If the date range is invalid (e.g., `from` > `to`), returns `400 BadRequest`.  
- If the query times out or the data store is unreachable, returns `503 ServiceUnavailable`.

**Throws**  
- `ArgumentOutOfRangeException` – If `from` is later than `to`.

## Usage

### Example 1: Initiating a batch and checking its status

```csharp
[ApiController]
[Route("api/payments")]
public class PaymentsController : ControllerBase
{
    [HttpPost("batch")]
    public async Task<IActionResult> CreateBatch([FromBody] BatchPaymentRequest request)
    {
        // Initiate the batch
        var initiationResult = await this.InitiateBatchPayments(request);
        if (initiationResult is not ObjectResult createdResult || createdResult.StatusCode != 201)
            return initiationResult;

        // Extract batch ID from the response (simplified)
        var batchId = ((dynamic)createdResult.Value).BatchId;

        // Poll for status after a short delay
        await Task.Delay(500);
        return await this.GetPaymentStatus(batchId);
    }
}
```

### Example 2: Cancelling a batch and querying historical payments

```csharp
[HttpDelete("batch/{batchId}")]
public async Task<IActionResult> CancelAndAudit(string batchId)
{
    // Cancel the batch
    var cancelResult = await this.CancelBatchPayments(batchId);
    if (cancelResult is not OkResult)
        return cancelResult;

    // Retrieve all cancelled payments from the last 24 hours for audit
    var from = DateTime.UtcNow.AddDays(-1);
    var to = DateTime.UtcNow;
    return await this.GetPaymentsByStatusAndDate(PaymentStatus.Cancelled, from, to);
}
```

## Notes

- **Thread safety**: All methods are static and asynchronous. They do not maintain shared mutable state internally, so concurrent calls from multiple threads are safe. However, the underlying payment provider or data store may impose its own concurrency limits.
- **Null and empty parameters**: Passing `null` for `batchId` or `request` will throw `ArgumentNullException`. Always validate inputs before calling these methods, or rely on model binding validation in the controller.
- **Cancellation**: Each method accepts a `CancellationToken`. If the token is cancelled before the operation completes, the method will throw `OperationCanceledException`. Ensure that the calling code handles this appropriately (e.g., by not catching it in a way that masks the cancellation).
- **Idempotency**: `CancelBatchPayments` is idempotent only for batches that are already cancelled or completed – repeated calls will return `409 Conflict`. `InitiateBatchPayments` is not idempotent; each call creates a new batch.
- **Date range queries**: `GetPaymentsByStatusAndDate` may return large result sets. Consider implementing server-side pagination or limiting the date range to avoid performance degradation. The method itself does not apply any limit.
