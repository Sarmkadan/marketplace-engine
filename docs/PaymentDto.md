# PaymentDto

Represents a monetary transaction between a buyer and seller within the marketplace. This data-transfer object carries payment details through service boundaries, including amounts, fees, payout calculations, lifecycle status, and optional references to external payment processors.

## API

### Properties

#### `Guid Id`
Unique identifier for the payment record. Immutable after creation.

#### `Guid ListingId`
Identifier of the marketplace listing this payment is associated with. Set at construction and cannot be changed.

#### `Guid BuyerId`
Identifier of the user who initiated the payment. Set at construction and cannot be changed.

#### `Guid SellerId`
Identifier of the user who will receive the seller payout. Assigned by the system based on the listing ownership.

#### `decimal Amount`
Gross transaction amount in the specified currency before platform fees are deducted. Must be non-negative.

#### `string Currency`
ISO 4217 currency code (e.g., `"USD"`, `"EUR"`) for the transaction. Set at construction.

#### `decimal PlatformFee`
Fee retained by the marketplace platform. Calculated as a portion of `Amount`. Must be non-negative and cannot exceed `Amount`.

#### `decimal SellerPayout`
Net amount payable to the seller, computed as `Amount - PlatformFee`. Must be non-negative.

#### `string Status`
Current lifecycle state of the payment. Expected values include `"Pending"`, `"Processing"`, `"Completed"`, `"Failed"`, `"Refunded"`. The system enforces valid state transitions.

#### `string? PaymentMethod`
Optional identifier for the payment method used (e.g., `"credit_card"`, `"paypal"`, `"bank_transfer"`). May be null if not yet specified or not captured.

#### `string? ExternalTransactionId`
Optional reference to a transaction identifier from an external payment processor. Null when no external processing has occurred or the payment remains internal.

#### `string? FailureReason`
Human-readable explanation for a failed payment. Null when `Status` is not `"Failed"`. Should be populated when transitioning to a failed state.

#### `DateTime CreatedAt`
UTC timestamp of when the payment record was created. Set automatically at instantiation.

#### `DateTime? CompletedAt`
UTC timestamp of when the payment reached a terminal completed state. Null for payments that have not yet completed.

### Constructors

#### `PaymentDto()`
Parameterless constructor. Initializes `Id` to a new GUID, `CreatedAt` to `DateTime.UtcNow`, and `Status` to `"Pending"`. All numeric fields default to zero. Reference-type fields default to null. Intended for deserialization or scenarios where properties are populated after construction.

#### `PaymentDto(Guid listingId, Guid buyerId, string paymentMethod, string currency)`
Parameterized constructor for creating a payment with essential transaction context.

| Parameter | Description |
|---|---|
| `Guid listingId` | The listing being purchased. Assigned to `ListingId`. |
| `Guid buyerId` | The purchasing user. Assigned to `BuyerId`. |
| `string paymentMethod` | Payment method identifier. Assigned to `PaymentMethod`. Must not be null or empty. |
| `string currency` | ISO 4217 currency code. Assigned to `Currency`. Must not be null or empty. |

Initializes `Id` to a new GUID, `CreatedAt` to `DateTime.UtcNow`, and `Status` to `"Pending"`. `SellerId`, `Amount`, `PlatformFee`, and `SellerPayout` remain at default values and must be set before the payment is processed.

**Throws:** `ArgumentNullException` if `paymentMethod` or `currency` is null. `ArgumentException` if either string is empty or whitespace.

## Usage

### Example 1: Creating and completing a payment

```csharp
// Buyer initiates payment for a listing
var payment = new PaymentDto(
    listingId: listing.Id,
    buyerId: currentUser.Id,
    paymentMethod: "credit_card",
    currency: "USD"
);

// System assigns seller and calculates amounts
payment.SellerId = listing.SellerId;
payment.Amount = 149.99m;
payment.PlatformFee = 14.99m;  // 10% platform fee
payment.SellerPayout = payment.Amount - payment.PlatformFee;

// External processor returns a transaction ID
payment.ExternalTransactionId = "txn_8a7b3c9d2e";
payment.Status = "Completed";
payment.CompletedAt = DateTime.UtcNow;

await paymentRepository.SaveAsync(payment);
```

### Example 2: Handling a failed payment

```csharp
var payment = new PaymentDto(
    listingId: listingId,
    buyerId: buyerId,
    paymentMethod: "paypal",
    currency: "EUR"
)
{
    SellerId = sellerId,
    Amount = 89.50m,
    PlatformFee = 8.95m,
    SellerPayout = 80.55m
};

try
{
    var result = await paymentGateway.ProcessAsync(payment);
    payment.ExternalTransactionId = result.TransactionId;
    payment.Status = "Completed";
    payment.CompletedAt = DateTime.UtcNow;
}
catch (PaymentDeclinedException ex)
{
    payment.Status = "Failed";
    payment.FailureReason = ex.Reason;
    // CompletedAt remains null
}
finally
{
    await paymentRepository.SaveAsync(payment);
}
```

## Notes

- **Lifecycle integrity:** `Status` transitions are not enforced by the DTO itself. Consumers must validate allowed state changes (e.g., `"Pending"` → `"Processing"` → `"Completed"` or `"Failed"`). Setting `Status` to `"Completed"` without populating `CompletedAt` creates an inconsistent record.
- **Amount consistency:** `SellerPayout` should always equal `Amount - PlatformFee`. The DTO does not enforce this invariant automatically; it is the responsibility of the code populating the object to maintain correctness.
- **Nullability of `FailureReason`:** This field should be non-null only when `Status` is `"Failed"`. Conversely, a failed payment with a null `FailureReason` is semantically incomplete and may cause issues in downstream reporting or user-facing displays.
- **Thread safety:** This type is not thread-safe. Mutable properties can be read and written concurrently without synchronization. In multi-threaded scenarios, external locking or immutable snapshots should be used.
- **Default constructor:** The parameterless constructor sets `Status` to `"Pending"` and `CreatedAt` to `DateTime.UtcNow`. Deserializers that populate these fields afterward may overwrite these defaults; ensure deserialization logic preserves or intentionally replaces them.
- **Currency validation:** The constructor validates that `currency` is non-null and non-empty but does not validate against known ISO 4217 codes. Upstream callers should enforce valid currency codes if required by business rules.
- **`ExternalTransactionId` uniqueness:** No uniqueness constraint is enforced by the DTO. If multiple payments reference the same external transaction ID, reconciliation logic must handle potential duplicates.
