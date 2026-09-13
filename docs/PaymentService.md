# PaymentService

`PaymentService` coordinates payment lifecycle operations with the payment, listing, and user repositories. It creates payments from active listings, delegates state-transition validation to the `Payment` domain model, enforces buyer authorization where required, updates related listing and seller records, and emits structured logs.

The service does not call a payment provider itself. Provider transaction identifiers are supplied by the caller when a payment is completed or escrow is released.

## Dependencies and construction

The service requires:

- `IPaymentRepository` for payment persistence and payment queries.
- `IListingRepository` for validating purchases and delisting a sold listing.
- `IUserRepository` for validating buyers and updating seller sale counts.
- An optional `ILogger<PaymentService>` for lifecycle logging.

```csharp
var service = new PaymentService(
    paymentRepository,
    listingRepository,
    userRepository,
    logger);
```

The three-argument constructor uses `NullLogger<PaymentService>.Instance`. Both constructors throw `ArgumentNullException` when a supplied dependency is `null`.

## Payment lifecycle

The supported state transitions are:

```text
Pending -> Processing -> Completed
                      \-> InEscrow -> Completed

Pending or Processing -> Cancelled
Any state except Completed or Refunded -> Failed
Completed or InEscrow -> Refunded
```

Invalid transitions throw `InvalidOperationException` from the `Payment` domain model. The service does not provide transaction boundaries across its repository calls, so a later failure can leave an earlier update persisted.

## API

### `InitiatePaymentAsync(Guid listingId, Guid buyerId, string paymentMethod, string currency = "USD")`

Creates and persists a `Pending` payment using the listing's price and seller. The method requires a nonblank payment method, an active listing with a price, and an existing active buyer who is not the listing's seller. When `currency` is nonempty, it must match the listing currency, ignoring case. An empty currency skips that comparison; the payment still uses the listing's currency.

Returns the entity produced by `IPaymentRepository.AddAsync`.

May throw:

- `ValidationException` when `paymentMethod` is null, empty, or whitespace.
- `ResourceNotFoundException` when the listing or buyer does not exist.
- `MarketplaceException` when the listing is inactive, has no price, has a different currency, or belongs to the buyer.
- `UnauthorizedException` when the buyer account is inactive.

### `StartProcessingAsync(Guid paymentId, Guid requesterId)`

Moves a `Pending` payment to `Processing` and persists it. Only the payment's buyer may perform this operation.

May throw `ResourceNotFoundException` for an unknown payment, `UnauthorizedException` when `requesterId` is not the buyer, or `InvalidOperationException` when the payment is not pending.

### `CompletePaymentAsync(Guid paymentId, string externalTransactionId)`

Moves a `Processing` payment to `Completed`, stores the external transaction ID, and persists the payment. It then delists the associated listing and records a sale for the seller when those records still exist.

The payment update occurs before the listing and seller updates. Missing related records are ignored. This method records a seller sale, so it should not be combined with the escrow-release path for the same transaction.

May throw `ArgumentNullException` for a null transaction ID, `ArgumentException` for a blank transaction ID, `ResourceNotFoundException` for an unknown payment, or `InvalidOperationException` for an invalid transition.

### `MoveToEscrowAsync(Guid paymentId)`

Moves a `Processing` payment to `InEscrow`, persists it, and logs the transition.

May throw `ResourceNotFoundException` for an unknown payment or `InvalidOperationException` when the payment is not processing.

### `ReleaseEscrowAsync(Guid paymentId, string externalTransactionId)`

Moves an `InEscrow` payment to `Completed`, stores the external transaction ID, and records a sale for the seller when that user exists. The seller update occurs before the payment update.

May throw `ArgumentNullException` for a null transaction ID, `ResourceNotFoundException` for an unknown payment, or `InvalidOperationException` when the payment is not in escrow.

### `FailPaymentAsync(Guid paymentId, string reason)`

Moves a payment to `Failed`, stores the reason, persists it, and writes a warning log. A payment may be failed from any state except `Completed` or `Refunded`.

May throw `ArgumentNullException` for a null reason, `ResourceNotFoundException` for an unknown payment, or `InvalidOperationException` for a disallowed transition.

### `CancelPaymentAsync(Guid paymentId, Guid requesterId)`

Moves a `Pending` or `Processing` payment to `Cancelled` and persists it. Only the payment's buyer may cancel it.

May throw `ResourceNotFoundException` for an unknown payment, `UnauthorizedException` when `requesterId` is not the buyer, or `InvalidOperationException` for a disallowed transition.

### `RefundPaymentAsync(Guid paymentId, string reason)`

Moves a `Completed` or `InEscrow` payment to `Refunded`, stores the reason, and persists it. Refunds are full payment state changes; this method does not accept or calculate a partial refund amount.

May throw `ArgumentNullException` for a null reason, `ValidationException` for an empty or whitespace reason, `ResourceNotFoundException` for an unknown payment, or `InvalidOperationException` for a disallowed transition.

### `GetPaymentAsync(Guid paymentId)`

Returns a payment by ID. Throws `ResourceNotFoundException` when it does not exist.

### `GetBuyerPaymentsAsync(Guid buyerId)`

Verifies that the user exists, then returns the buyer's payments from `IPaymentRepository.GetByBuyerIdAsync`. Throws `ResourceNotFoundException` when the user does not exist.

### `GetSellerPaymentsAsync(Guid sellerId)`

Verifies that the user exists, then returns the seller's payments from `IPaymentRepository.GetBySellerIdAsync`. Throws `ResourceNotFoundException` when the user does not exist.

### `GetSellerRevenueAsync(Guid sellerId)`

Returns the seller's net revenue from `IPaymentRepository.GetTotalRevenueAsync`. Unlike the buyer and seller payment-list methods, this method does not first verify that the user exists.

## Usage

Choose either direct completion or the escrow path after processing a payment:

```csharp
var payment = await paymentService.InitiatePaymentAsync(
    listingId,
    buyerId,
    paymentMethod: "card",
    currency: "USD");

await paymentService.StartProcessingAsync(payment.Id, buyerId);

// Escrow flow:
await paymentService.MoveToEscrowAsync(payment.Id);
Payment completed = await paymentService.ReleaseEscrowAsync(
    payment.Id,
    externalTransactionId: "provider-transaction-123");
```

For a flow without escrow, call `CompletePaymentAsync` immediately after `StartProcessingAsync` instead. Cancellation is buyer-authorized, while completion, escrow, failure, and refund methods do not accept a requester ID; callers must enforce any additional access policy before invoking them.

## Operational notes

- Successful initiation, completion, escrow movement, escrow release, cancellation, and refund operations are logged at information level; failures are logged at warning level.
- Repository and logger exceptions are not caught or translated.
- The service has no built-in retry, idempotency, cancellation-token, or concurrency handling.
- Returned `Payment` instances are the persisted repository results except for `CompletePaymentAsync`, which returns the mutated payment after updating it.
