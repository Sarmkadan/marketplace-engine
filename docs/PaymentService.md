# PaymentService

Central service for orchestrating payments within the marketplace, handling buyer-initiated payments, escrow management, refunds, and seller payouts. It coordinates state transitions between payment lifecycle stages and integrates with external payment gateways and the platform’s ledger.

## API

### `PaymentService()`
Constructs a new `PaymentService` instance. Dependencies such as ledger, gateway, and repository clients are injected via constructor parameters.

### `public async Task<Payment> InitiatePaymentAsync(PaymentInitiation initiation)`
Initiates a new payment from a buyer to a seller for a given amount and order. Validates the initiation request, reserves funds with the payment gateway, and records the payment in the ledger.
- **Parameters**
  - `initiation`: Contains buyer ID, seller ID, amount, currency, order reference, and optional metadata.
- **Return value**
  - The created `Payment` entity in `Pending` state.
- **Exceptions**
  - Throws `ArgumentException` if amount ≤ 0 or required fields are missing.
  - Throws `PaymentGatewayException` if gateway reservation fails.
  - Throws `LedgerException` if recording fails.

### `public async Task<Payment> StartProcessingAsync(Guid paymentId)`
Transitions a `Pending` payment to `Processing`, indicating the gateway has begun capturing funds.
- **Parameters**
  - `paymentId`: Unique identifier of the payment.
- **Return value**
  - Updated `Payment` entity in `Processing` state.
- **Exceptions**
  - Throws `InvalidOperationException` if payment is not in `Pending`.
  - Throws `PaymentGatewayException` if capture initiation fails.
  - Throws `KeyNotFoundException` if payment does not exist.

### `public async Task<Payment> CompletePaymentAsync(Guid paymentId)`
Finalizes a `Processing` payment, marking it as `Completed` and releasing funds to escrow.
- **Parameters**
  - `paymentId`: Unique identifier of the payment.
- **Return value**
  - Updated `Payment` entity in `Completed` state.
- **Exceptions**
  - Throws `InvalidOperationException` if payment is not in `Processing`.
  - Throws `LedgerException` if escrow update fails.

### `public async Task<Payment> MoveToEscrowAsync(Guid paymentId)`
Moves a `Completed` payment into escrow, reserving funds for the seller until release or refund.
- **Parameters**
  - `paymentId`: Unique identifier of the payment.
- **Return value**
  - Updated `Payment` entity in `InEscrow` state.
- **Exceptions**
  - Throws `InvalidOperationException` if payment is not in `Completed`.
  - Throws `LedgerException` if escrow reservation fails.

### `public async Task<Payment> ReleaseEscrowAsync(Guid paymentId)`
Releases funds from escrow to the seller, completing the payout.
- **Parameters**
  - `paymentId`: Unique identifier of the payment.
- **Return value**
  - Updated `Payment` entity in `Released` state.
- **Exceptions**
  - Throws `InvalidOperationException` if payment is not in `InEscrow`.
  - Throws `PayoutException` if transfer to seller fails.

### `public async Task<Payment> FailPaymentAsync(Guid paymentId)`
Marks a payment as failed, reversing any reserved funds and notifying stakeholders.
- **Parameters**
  - `paymentId`: Unique identifier of the payment.
- **Return value**
  - Updated `Payment` entity in `Failed` state.
- **Exceptions**
  - Throws `KeyNotFoundException` if payment does not exist.

### `public async Task<Payment> CancelPaymentAsync(Guid paymentId)`
Cancels a payment before completion, releasing reserved funds back to the buyer.
- **Parameters**
  - `paymentId`: Unique identifier of the payment.
- **Return value**
  - Updated `Payment` entity in `Cancelled` state.
- **Exceptions**
  - Throws `InvalidOperationException` if payment is already terminal (`Completed`, `Failed`, `Cancelled`, etc.).
  - Throws `PaymentGatewayException` if release fails.

### `public async Task<Payment> RefundPaymentAsync(Guid paymentId, decimal amount)`
Issues a partial or full refund to the buyer from an `InEscrow` or `Released` payment.
- **Parameters**
  - `paymentId`: Unique identifier of the payment.
  - `amount`: Refund amount; must be ≤ remaining escrow balance.
- **Return value**
  - Updated `Payment` entity in `Refunded` state with refund record.
- **Exceptions**
  - Throws `ArgumentException` if amount ≤ 0 or > escrow balance.
  - Throws `InvalidOperationException` if payment is not in `InEscrow` or `Released`.
  - Throws `RefundException` if gateway refund fails.

### `public async Task<Payment> GetPaymentAsync(Guid paymentId)`
Retrieves a payment by its unique identifier.
- **Parameters**
  - `paymentId`: Unique identifier of the payment.
- **Return value**
  - The `Payment` entity, or `null` if not found.
- **Exceptions**
  - None.

### `public async Task<List<Payment>> GetBuyerPaymentsAsync(Guid buyerId)`
Returns all payments initiated by a specific buyer.
- **Parameters**
  - `buyerId`: Unique identifier of the buyer.
- **Return value**
  - List of `Payment` entities; empty list if none exist.
- **Exceptions**
  - None.

### `public async Task<List<Payment>> GetSellerPaymentsAsync(Guid sellerId)`
Returns all payments associated with a specific seller.
- **Parameters**
  - `sellerId`: Unique identifier of the seller.
- **Return value**
  - List of `Payment` entities; empty list if none exist.
- **Exceptions**
  - None.

### `public async Task<decimal> GetSellerRevenueAsync(Guid sellerId)`
Calculates the total revenue available to a seller from completed and released payments, excluding refunds.
- **Parameters**
  - `sellerId`: Unique identifier of the seller.
- **Return value**
  - Total revenue as a decimal.
- **Exceptions**
  - None.

## Usage

### Example 1: Buyer initiates and completes a payment
