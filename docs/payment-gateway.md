# PaymentGateway

`PaymentGateway` (`src/MarketplaceEngine/Services/PaymentGateway.cs`) is the component that stands in for an external payment provider. It handles authorization, capture, refund and transaction lookup for a `Payment` record. Today it is an in-process simulation: no network calls are made, and the "provider" is just the repository plus a random decline switch. The class is written so a real provider adapter (Stripe, PayPal, etc.) can replace the simulated logic without changing the public method signatures.

The class is not registered in `Configuration/DependencyInjection.cs` and no other production code references it. It is currently used only by its own construction paths and is available for direct instantiation or for future wiring.

## Dependencies and construction

- `IPaymentRepository` - loads and persists `Payment` entities.
- `ILogger<PaymentGateway>` - logs authorization, capture and refund events.

Two constructors are available:

```csharp
// No logging, simulated failures disabled (failure rate 0.0).
var gateway = new PaymentGateway(paymentRepository);

// Explicit logger and simulated failure rate.
var gateway = new PaymentGateway(paymentRepository, logger, simulatedFailureRate: 0.2);
```

- `simulatedFailureRate` is clamped to the range `0.0` to `1.0`. Values outside that range are not rejected.
- Both constructors throw `ArgumentNullException` when `paymentRepository` or `logger` is `null`.

Unlike the rest of the codebase, the class is not `sealed`.

## Configuration

Gateway settings are bound from the `PaymentGateway` configuration section into `PaymentGatewayOptions` (`src/MarketplaceEngine/Configuration/PaymentGatewayOptions.cs`). They are registered by `AddPaymentGatewayOptions()`, which `AddMarketplaceServices()` calls, and validated when the host starts by `PaymentGatewayOptionsValidator`.

| Key | Required | Rule | Default |
|---|---|---|---|
| `ApiKey` | yes | Non-empty. Supply it outside source control, for example `PaymentGateway__ApiKey` as an environment variable. | (none) |
| `BaseUrl` | yes | Absolute `https://` URL. | (none) |
| `TimeoutSeconds` | no | 1 to 120. | `30` |
| `MaxRetries` | no | 0 to 10. | `3` |
| `Currency` | no | Three uppercase ASCII letters (ISO-4217 shape; the code is not checked against the official list). | `USD` |

An invalid configuration stops startup with an `OptionsValidationException` that lists every invalid field. The check runs when the host starts, not when the first payment is made. Without `ApiKey` or `BaseUrl`, the application will not start.

## Constants

| Constant | Value | Purpose |
|---|---|---|
| `PlatformFeeRate` | `0.05m` | Platform fee taken on capture (5%). |
| `MoneyDecimalPlaces` | `2` | Rounding precision for the fee. |
| `TransactionIdPrefix` | `"txn_"` | Prefix of generated provider transaction IDs. |
| `TransactionIdLength` | `24` | Total length of a generated transaction ID, prefix included. |
| `SimulatedDeclineReason` | `"Authorization declined by provider (simulated)."` | Written to `Payment.FailureReason` on a simulated decline. |
| `DefaultSimulatedFailureRate` | `0.0` | Default failure rate when none is given. |

## Payment flow

The gateway expects a payment to move through these statuses:

```text
Pending --AuthorizeAsync--> Processing --CaptureAsync--> Completed --RefundAsync--> Refunded
   |                            |
   +-- (simulated decline) -> Failed
```

## API

### `AuthorizeAsync(Guid paymentId)`

Authorizes a `Pending` payment.

- Loads the payment. Throws `MarketplaceException` if it does not exist.
- Throws `MarketplaceException` if the status is not `Pending`.
- If the simulated failure check fires, sets the status to `Failed`, sets `FailureReason` to `SimulatedDeclineReason`, saves, logs a warning and returns the payment. No exception is thrown on a decline.
- Otherwise sets the status to `Processing`, assigns `ExternalTransactionId` (`txn_` followed by the first 20 hex characters of a new GUID, 24 characters total), saves, logs and returns the payment.

### `CaptureAsync(Guid paymentId)`

Completes a `Processing` payment.

- Throws `MarketplaceException` if the payment is missing or not in `Processing`.
- Sets the status to `Completed` and `CompletedAt` to the current UTC time.
- Computes `PlatformFee = Round(Amount * 0.05, 2, AwayFromZero)` and `SellerPayout = Amount - PlatformFee`, both in the payment's currency.
- Saves and returns the payment.

Example: an amount of `10.01` gives a fee of `0.50` (0.5005 rounded away from zero) and a payout of `9.51`.

### `RefundAsync(Guid paymentId, string reason)`

Performs a full refund of a `Completed` payment.

- Throws `ArgumentNullException` when `reason` is `null`. An empty or whitespace string is accepted.
- Throws `MarketplaceException` if the payment is missing or not `Completed`.
- Sets the status to `Refunded`, stores `RefundReason` and `RefundedAt`, saves and returns the payment.
- Partial refunds are not supported. The fee and payout fields are not changed.

### `GetByTransactionIdAsync(string externalTransactionId)`

Looks up a payment by its provider transaction ID.

- Returns `null` when the argument is null, empty or whitespace, without touching the repository.
- Otherwise loads all payments with `GetAllAsync()` and returns the first one whose `ExternalTransactionId` matches using ordinal, case-sensitive comparison, or `null` if none matches.
- This is a linear scan. It does not scale to large payment volumes and should move to a repository query if the gateway is used for real traffic.

## Error handling

| Situation | Behavior |
|---|---|
| Payment not found (authorize, capture, refund) | `MarketplaceException` |
| Wrong status for the operation | `MarketplaceException` |
| Simulated decline | Payment saved with `Failed`; no exception |
| Null `reason` on refund | `ArgumentNullException` |
| Null constructor argument | `ArgumentNullException` |

Errors are thrown from the `MarketplaceException` hierarchy, so `ErrorHandlingMiddleware` converts them to the standard JSON error envelope when the gateway is called from an endpoint.

## Simulation

Simulated declines are controlled by `simulatedFailureRate`. With the default of `0.0`, `ShouldSimulateFailure()` returns `false` without drawing a random number, so authorization always succeeds. For a non-zero rate, each authorization call draws from `Random.Shared.NextDouble()` and declines when the draw is below the rate. Tests that need deterministic outcomes should use `0.0` or `1.0`.

## Known limitations

- No transaction boundaries. Each method performs separate repository reads and writes. A failure between them can leave the payment in an intermediate state.
- Not wired into DI. Consumers must construct the gateway themselves.
- No idempotency. Calling `AuthorizeAsync` twice fails on the second call because the status has already changed, but there is no idempotency key for retries from external callers.
- `RefundAsync` only supports full refunds and does not reverse `PlatformFee` or `SellerPayout`.
- `GetByTransactionIdAsync` loads every payment on each call.

## Replacing the simulation with a real provider

When a real provider is integrated:

1. Move the provider calls into `AuthorizeAsync`, `CaptureAsync` and `RefundAsync`, and use the provider's transaction ID instead of the generated `txn_` value.
2. Map provider declines to `Failed` with the provider's reason instead of `SimulatedDeclineReason`.
3. Remove `simulatedFailureRate` and `ShouldSimulateFailure()` once the provider path is covered by tests.
4. Register the implementation in `DependencyInjection.cs` (as a singleton, consistent with the rest of the in-memory store) and document the change in `docs/ARCHITECTURE.md`.
