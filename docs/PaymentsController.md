# PaymentsController
The `PaymentsController` exposes HTTP endpoints for managing the lifecycle of payments within the marketplace engine, including initiation, retrieval, processing, escrow handling, and refunds. It is designed to be invoked by clients or internal services to orchestrate payment state transitions and to query payment information.

## API
### InitiatePayment
- **Purpose**: Starts a new payment transaction for a buyer and seller pair, creating a pending payment record.
- **Parameters**: Typically receives a payload containing buyer identifier, seller identifier, amount, currency, and optional metadata.
- **Return value**: Returns an `IActionResult` wrapped in a `Task`. On success, yields `201 Created` with the newly created payment details; on validation failure, yields `400 Bad Request`; on unauthorized access, yields `401 Unauthorized`.
- **Throws**: May throw `ArgumentNullException` if required payload is null, or `InvalidOperationException` if the payment cannot be initiated due to business rule violations (e.g., insufficient funds, duplicate request).

### GetPayment
- **Purpose**: Retrieves the current state and details of a specific payment by its identifier.
- **Parameters**: Accepts a payment ID (usually a GUID or string) to locate the payment.
- **Return value**: Returns an `IActionResult` wrapped in a `Task`. On success, yields `200 OK` with the payment object; if the payment does not exist, yields `404 Not Found`.
- **Throws**: May throw `ArgumentException` if the supplied ID is malformed.

### StartProcessing
- **Purpose**: Moves a payment from the *Pending* state to *Processing*, indicating that the payment gateway has been invoked.
- **Parameters**: Requires the payment ID and optionally a processing token or gateway response.
- **Return value**: Returns an `IActionResult` wrapped in a `Task`. On success, yields `200 OK`; if the payment is not in a cancellable state, yields `409 Conflict`.
- **Throws**: May throw `InvalidOperationException` when the payment is not in the expected state for processing.

### CompletePayment
- **Purpose**: Marks a processing payment as successfully completed, transitioning it to the *Completed* state.
- **Parameters**: Takes the payment ID and any confirmation data returned by the payment gateway.
- **Return value**: Returns an `IActionResult` wrapped in a `Task`. On success, yields `200 OK`; if the payment is not in *Processing*, yields `400 Bad Request`.
- **Throws**: May throw `InvalidOperationException` for state mismatches or `TimeoutException` if confirmation is overdue.

### MoveToEscrow
- **Purpose**: Transfers a completed payment into escrow, holding funds until certain conditions are met.
- **Parameters**: Requires the payment ID and escrow terms (e.g., release conditions).
- **Return value**: Returns an `IActionResult` wrapped in a `Task`. On success, yields `200 OK`; if the payment is not *Completed*, yields `400 Bad Request`.
- **Throws**: May throw `InvalidOperationException` when escrow cannot be applied (e.g., already in escrow).

### ReleaseEscrow
- **Purpose**: Releases escrowed funds to the seller, completing the transaction.
- **Parameters**: Takes the payment ID and optionally a release authorization token.
- **Return value**: Returns an `IActionResult` wrapped in a `Task`. On success, yields `200 OK`; if escrow is not present or conditions unmet, yields `400 Bad Request`.
- **Throws**: May throw `InvalidOperationException` for incorrect state or missing authorization.

### CancelPayment
- **Purpose**: Cancels a payment that has not yet been completed, returning any captured funds to the buyer.
- **Parameters**: Requires the payment ID and a cancellation reason.
- **Return value**: Returns an `IActionResult` wrapped in a `Task`. On success, yields `200 OK`; if the payment is already completed or in escrow, yields `409 Conflict`.
- **Throws**: May throw `InvalidOperationException` when cancellation is not permissible.

### RefundPayment
- **Purpose**: Issues a refund for a completed payment, returning funds to the buyer’s original payment method.
- **Parameters**: Accepts the payment ID and the refund amount (which may be partial or full).
- **Return value**: Returns an `IActionResult` wrapped in a `Task`. On success, yields `200 OK` with refund details; if the payment is not eligible for refund, yields `400 Bad Request`.
- **Throws**: May throw `InvalidOperationException` for state issues or `ArgumentOutOfRangeException` if the refund amount exceeds the payment total.

### GetBuyerPayments
- **Purpose**: Returns a list of payments where the specified buyer is the payer.
- **Parameters**: Takes a buyer identifier and optional pagination/filtering parameters.
- **Return value**: Returns an `IActionResult` wrapped in a `Task`. On success, yields `200 OK` with a collection of payment summaries; if the buyer has no payments, yields an empty list.
- **Throws**: May throw `ArgumentNullException` if the buyer ID is missing.

### GetSellerPayments
- **Purpose**: Returns a list of payments where the specified seller is the payee.
- **Parameters**: Takes a seller identifier and optional pagination/filtering parameters.
- **Return value**: Returns an `IActionResult` wrapped in a `Task`. On success, yields `200 OK` with a collection of payment summaries; if the seller has no payments, yields an empty list.
- **Throws**: May throw `ArgumentNullException` if the seller ID is missing.

## Usage
```csharp
// Example 1: Initiating a payment via HttpClient
var client = new HttpClient { BaseAddress = new Uri("https://api.marketplace.example") };
var request = new { BuyerId = "b-123", SellerId = "s-456", Amount = 99.95m, Currency = "USD" };
var content = new JsonContent(request);
var response = await client.PostAsync("/payments/initiate", content);
response.EnsureSuccessStatusCode();
var payment = await response.Content.ReadFromJsonAsync<PaymentDto>();
```

```csharp
// Example 2: Retrieving a seller's payments using a service proxy
var paymentsService = new PaymentsService("https://api.marketplace.example");
var sellerPayments = await paymentsService.GetSellerPaymentsAsync("s-789", new PaginationParams { Page = 1, PageSize = 20 });
foreach (var p in sellerPayments.Items)
{
    Console.WriteLine($"Payment {p.Id}: {p.Amount} {p.Currency} - {p.Status}");
}
```

## Notes
- The controller is instantiated per request; it holds no mutable state, making it thread‑safe for concurrent calls.
- All methods are asynchronous and should be awaited; failure to do so may result in unobserved exceptions.
- Idempotency is not guaranteed by the endpoints; callers should implement idempotency keys at the transport layer if duplicate submissions are a concern.
- State‑transition methods (`StartProcessing`, `CompletePayment`, `MoveToEscrow`, `ReleaseEscrow`, `CancelPayment`, `RefundPayment`) validate the current payment state and will throw `InvalidOperationException` if the requested transition is not permissible.
- Input validation is performed before any business logic; malformed identifiers or missing required fields result in `400 Bad Request` responses.
- Exception handling within the controller translates unexpected errors into `500 Internal Server Error` responses while preserving the original exception details in logs for diagnostics.
