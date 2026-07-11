# PaymentDtoExtensions

The `PaymentDtoExtensions` static class provides a set of extension methods for the `PaymentDto` type (defined in the `marketplace-engine` project). These methods encapsulate common operations such as calculating platform revenue and seller payout, checking payment status, formatting monetary values, and retrieving time-related information. They are designed to reduce repetitive code and improve readability when working with payment data transfer objects.

## API

### `GetPlatformRevenue`
```csharp
public static decimal GetPlatformRevenue(this PaymentDto payment)
```
Returns the platform’s share of the payment amount.  
**Parameters:**  
- `payment` – The payment DTO. Must not be `null`.  

**Returns:** The platform revenue as a `decimal`.  

**Throws:** `ArgumentNullException` if `payment` is `null`.

---

### `GetSellerPayout`
```csharp
public static decimal GetSellerPayout(this PaymentDto payment)
```
Returns the amount that will be paid out to the seller after deducting the platform fee.  
**Parameters:**  
- `payment` – The payment DTO. Must not be `null`.  

**Returns:** The seller payout as a `decimal`.  

**Throws:** `ArgumentNullException` if `payment` is `null`.

---

### `GetTotalAmount`
```csharp
public static decimal GetTotalAmount(this PaymentDto payment)
```
Returns the total transaction amount (including platform fee).  
**Parameters:**  
- `payment` – The payment DTO. Must not be `null`.  

**Returns:** The total amount as a `decimal`.  

**Throws:** `ArgumentNullException` if `payment` is `null`.

---

### `IsCompleted`
```csharp
public static bool IsCompleted(this PaymentDto payment)
```
Indicates whether the payment has reached a completed state.  
**Parameters:**  
- `payment` – The payment DTO. Must not be `null`.  

**Returns:** `true` if the payment status is `Completed`; otherwise `false`.  

**Throws:** `ArgumentNullException` if `payment` is `null`.

---

### `IsFailed`
```csharp
public static bool IsFailed(this PaymentDto payment)
```
Indicates whether the payment has failed.  
**Parameters:**  
- `payment` – The payment DTO. Must not be `null`.  

**Returns:** `true` if the payment status is `Failed`; otherwise `false`.  

**Throws:** `ArgumentNullException` if `payment` is `null`.

---

### `IsPending`
```csharp
public static bool IsPending(this PaymentDto payment)
```
Indicates whether the payment is still pending (not yet completed or failed).  
**Parameters:**  
- `payment` – The payment DTO. Must not be `null`.  

**Returns:** `true` if the payment status is `Pending`; otherwise `false`.  

**Throws:** `ArgumentNullException` if `payment` is `null`.

---

### `FormatAmount`
```csharp
public static string FormatAmount(this PaymentDto payment)
```
Formats the total amount as a currency string using the current culture.  
**Parameters:**  
- `payment` – The payment DTO. Must not be `null`.  

**Returns:** A string representation of the total amount (e.g., "$123.45").  

**Throws:** `ArgumentNullException` if `payment` is `null`.

---

### `FormatPlatformFee`
```csharp
public static string FormatPlatformFee(this PaymentDto payment)
```
Formats the platform fee amount as a currency string using the current culture.  
**Parameters:**  
- `payment` – The payment DTO. Must not be `null`.  

**Returns:** A string representation of the platform fee.  

**Throws:** `ArgumentNullException` if `payment` is `null`.

---

### `FormatSellerPayout`
```csharp
public static string FormatSellerPayout(this PaymentDto payment)
```
Formats the seller payout amount as a currency string using the current culture.  
**Parameters:**  
- `payment` – The payment DTO. Must not be `null`.  

**Returns:** A string representation of the seller payout.  

**Throws:** `ArgumentNullException` if `payment` is `null`.

---

### `GetTimeSinceCreation`
```csharp
public static TimeSpan GetTimeSinceCreation(this PaymentDto payment)
```
Calculates the elapsed time since the payment was created.  
**Parameters:**  
- `payment` – The payment DTO. Must not be `null`.  

**Returns:** A `TimeSpan` representing the duration between the creation timestamp and the current UTC time.  

**Throws:** `ArgumentNullException` if `payment` is `null`.

---

### `GetCreatedAtString`
```csharp
public static string GetCreatedAtString(this PaymentDto payment)
```
Returns a human-readable string representation of the payment’s creation timestamp.  
**Parameters:**  
- `payment` – The payment DTO. Must not be `null`.  

**Returns:** A formatted date/time string (e.g., "2025-03-15 14:30:00").  

**Throws:** `ArgumentNullException` if `payment` is `null`.

---

### `GetCompletedAtString`
```csharp
public static string? GetCompletedAtString(this PaymentDto payment)
```
Returns a human-readable string representation of the payment’s completion timestamp, or `null` if the payment has not been completed.  
**Parameters:**  
- `payment` – The payment DTO. Must not be `null`.  

**Returns:** A formatted date/time string if the payment is completed; otherwise `null`.  

**Throws:** `ArgumentNullException` if `payment` is `null`.

---

### `GetPaymentStatus`
```csharp
public static MarketplaceEngine.Domain.Enums.PaymentStatus GetPaymentStatus(this PaymentDto payment)
```
Returns the current payment status as a strongly-typed enum value.  
**Parameters:**  
- `payment` – The payment DTO. Must not be `null`.  

**Returns:** A `PaymentStatus` enum value (e.g., `Pending`, `Completed`, `Failed`).  

**Throws:** `ArgumentNullException` if `payment` is `null`.

## Usage

### Example 1: Status checks and formatted output
```csharp
using MarketplaceEngine.Domain.Extensions;

public void DisplayPaymentInfo(PaymentDto payment)
{
    if (payment.IsCompleted())
    {
        Console.WriteLine($"Payment completed at {payment.GetCompletedAtString()}");
    }
    else if (payment.IsFailed())
    {
        Console.WriteLine("Payment failed.");
    }
    else if (payment.IsPending())
    {
        Console.WriteLine($"Payment pending since {payment.GetCreatedAtString()}");
    }

    Console.WriteLine($"Total: {payment.FormatAmount()}");
    Console.WriteLine($"Platform fee: {payment.FormatPlatformFee()}");
    Console.WriteLine($"Seller payout: {payment.FormatSellerPayout()}");
}
```

### Example 2: Revenue calculation and time tracking
```csharp
using MarketplaceEngine.Domain.Extensions;

public void ProcessPayment(PaymentDto payment)
{
    decimal platformRevenue = payment.GetPlatformRevenue();
    decimal sellerPayout = payment.GetSellerPayout();
    TimeSpan elapsed = payment.GetTimeSinceCreation();

    Console.WriteLine($"Platform revenue: {platformRevenue:C}");
    Console.WriteLine($"Seller payout: {sellerPayout:C}");
    Console.WriteLine($"Time since creation: {elapsed.TotalMinutes:F1} minutes");

    if (payment.GetPaymentStatus() == PaymentStatus.Completed)
    {
        // Archive logic
    }
}
```

## Notes

- **Null handling:** All extension methods throw `ArgumentNullException` if the `payment` argument is `null`. Always ensure the input is not null before calling these methods.
- **Currency formatting:** `FormatAmount`, `FormatPlatformFee`, and `FormatSellerPayout` use the current thread’s culture for formatting. Results may vary across different locales.
- **Time calculations:** `GetTimeSinceCreation` uses `DateTime.UtcNow` for the current time. If the payment’s creation timestamp is in local time, the result may be slightly off; ensure the `PaymentDto` stores timestamps in UTC.
- **Incomplete data:** `GetCompletedAtString` returns `null` for payments that are not yet completed. Callers should check `IsCompleted` or `GetPaymentStatus` before relying on the returned value.
- **Thread safety:** These static methods are inherently thread-safe because they do not modify any shared state. However, if the `PaymentDto` instance is being mutated concurrently by another thread, the results may be inconsistent. It is the caller’s responsibility to synchronize access to the underlying DTO if necessary.
