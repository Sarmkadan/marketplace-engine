# Payment

The `Payment` class represents a financial transaction record within the marketplace engine, encapsulating the details of a monetary exchange between a buyer and a seller for a specific listing. It tracks the lifecycle of the transaction from creation through completion or failure, including platform fees, payout calculations, and refund statuses, while providing extensible metadata storage for integration with external payment gateways.

## API

The following public members define the structure and state of a `Payment` instance:

*   **`public Guid Id`**
    The unique identifier for this payment record. This value is immutable once assigned and serves as the primary key for database persistence and internal referencing.

*   **`public Guid ListingId`**
    The unique identifier of the `Listing` associated with this transaction. This links the payment to the specific item or service being purchased.

*   **`public Listing? Listing`**
    The navigation property referencing the associated `Listing` object. This may be `null` if the listing data has not been eagerly loaded or if the related listing has been deleted.

*   **`public Guid BuyerId`**
    The unique identifier of the `User` initiating the purchase. This identifies the party debiting funds.

*   **`public User? Buyer`**
    The navigation property referencing the `User` object representing the buyer. This may be `null` if user details are not included in the current data context.

*   **`public Guid SellerId`**
    The unique identifier of the `User` receiving the funds. This identifies the party credited with the sale proceeds.

*   **`public User? Seller`**
    The navigation property referencing the `User` object representing the seller. This may be `null` if user details are not included in the current data context.

*   **`public Money Amount`**
    The total gross monetary value of the transaction. This value cannot be null and represents the full amount charged to the buyer before any fee deductions.

*   **`public Money? PlatformFee`**
    The portion of the `Amount` retained by the marketplace platform as a service fee. This value is `null` if the fee has not yet been calculated or if the transaction does not incur a fee.

*   **`public Money? SellerPayout`**
    The net monetary amount designated for transfer to the seller. This is typically calculated as `Amount` minus `PlatformFee`. It is `null` until the payout calculation is finalized.

*   **`public PaymentStatus Status`**
    An enumeration indicating the current state of the payment lifecycle (e.g., Pending, Completed, Failed, Refunded). This property drives the logic for state transitions and business rules.

*   **`public string? ExternalTransactionId`**
    The reference identifier provided by the external payment gateway (e.g., Stripe, PayPal). This is `null` if the payment has not yet been processed by an external provider or if the transaction is internal.

*   **`public string? PaymentMethod`**
    A descriptive string indicating the method used for the transaction (e.g., "CreditCard", "BankTransfer", "Wallet"). This is `null` if the method has not been specified.

*   **`public string? FailureReason`**
    A descriptive message explaining why a payment transitioned to a failed state. This is `null` if the payment is successful or has not failed.

*   **`public string? RefundReason`**
    A descriptive message provided when a payment is refunded. This is `null` if the payment has not been refunded.

*   **`public DateTime CreatedAt`**
    The UTC timestamp indicating when the payment record was initially created. This value is set upon instantiation and persists throughout the object's life.

*   **`public DateTime? UpdatedAt`**
    The UTC timestamp of the last modification to the payment record. This is `null` if the record has not been updated since creation.

*   **`public DateTime? CompletedAt`**
    The UTC timestamp marking when the payment successfully finalized. This is `null` for pending, failed, or refunded transactions.

*   **`public DateTime? RefundedAt`**
    The UTC timestamp marking when the refund process was completed. This is `null` if the payment has not been refunded.

*   **`public Dictionary<string, string> Metadata`**
    A collection of key-value pairs for storing arbitrary extensible data related to the payment. This allows for the attachment of contextual information without altering the schema.

## Usage

### Example 1: Inspecting Payment Status and Financials
This example demonstrates how to retrieve a payment and evaluate its financial breakdown and current status to determine if funds are ready for payout.

```csharp
public void ProcessPaymentSummary(Guid paymentId)
{
    var payment = _repository.GetById(paymentId);
    
    if (payment == null)
    {
        throw new InvalidOperationException($"Payment {paymentId} not found.");
    }

    // Check if the payment is successfully completed
    if (payment.Status == PaymentStatus.Completed && payment.CompletedAt.HasValue)
    {
        Console.WriteLine($"Transaction {payment.ExternalTransactionId} completed on {payment.CompletedAt.Value}");
        
        if (payment.SellerPayout.HasValue)
        {
            Console.WriteLine($"Net payout to seller: {payment.SellerPayout.Value}");
            Console.WriteLine($"Platform fee deducted: {payment.PlatformFee?.ToString() ?? "N/A"}");
        }
        else
        {
            Console.WriteLine("Warning: Payment completed but seller payout has not been calculated.");
        }
    }
    else if (payment.Status == PaymentStatus.Failed)
    {
        Console.WriteLine($"Payment failed: {payment.FailureReason}");
    }
}
```

### Example 2: Attaching Metadata for External Reconciliation
This example shows how to populate the `Metadata` dictionary with external gateway references and internal correlation IDs before persisting changes.

```csharp
public void EnrichPaymentWithGatewayData(Guid paymentId, string gatewayRef, string correlationId)
{
    var payment = _repository.GetById(paymentId);
    
    if (payment == null) return;

    // Update external reference
    payment.ExternalTransactionId = gatewayRef;
    payment.PaymentMethod = "CreditCard";
    payment.UpdatedAt = DateTime.UtcNow;

    // Attach reconciliation metadata
    if (!payment.Metadata.ContainsKey("GatewayCorrelationId"))
    {
        payment.Metadata["GatewayCorrelationId"] = correlationId;
    }
    
    payment.Metadata["ProcessedByWorker"] = "payment-worker-01";
    payment.Metadata["LastSynced"] = DateTime.UtcNow.ToString("O");

    _repository.Update(payment);
}
```

## Notes

*   **Nullability and Navigation Properties**: The `Listing`, `Buyer`, and `Seller` properties are nullable reference types. Accessing these without ensuring they are loaded (e.g., via eager loading in an ORM context) may result in `null` values even if the corresponding `*Id` fields are populated.
*   **State Consistency**: The timestamp properties (`CompletedAt`, `RefundedAt`, `UpdatedAt`) should logically align with the `Status` property. For instance, `RefundedAt` should only be populated if `Status` is `PaymentStatus.Refunded`. The class does not enforce these state machines automatically; callers must ensure logical consistency before persistence.
*   **Thread Safety**: The `Payment` class is not thread-safe. Specifically, the `Metadata` dictionary is a standard `Dictionary<string, string>`, which is not safe for concurrent read/write operations. If multiple threads access the same `Payment` instance, external synchronization (e.g., locks) is required when modifying `Metadata` or any other mutable property.
*   **Value Types**: The `Amount` property is of type `Money` and is non-nullable, ensuring that a payment record always has a defined transaction value. However, `PlatformFee` and `SellerPayout` are nullable, indicating that fee calculation may be a deferred process occurring after the initial record creation.
*   **Immutability of Identifiers**: While the class exposes setters for most properties (implied by standard POCO patterns in this context), the `Id`, `BuyerId`, `SellerId`, and `ListingId` should be treated as immutable after the initial construction of the aggregate to maintain referential integrity.
