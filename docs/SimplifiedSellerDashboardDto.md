# SimplifiedSellerDashboardDto

The `SimplifiedSellerDashboardDto` is a data transfer object within the `marketplace-engine` project designed to encapsulate a high-level summary of a seller's current status. It aggregates key performance indicators such as revenue, listing counts, and user ratings into a single immutable snapshot, facilitating efficient serialization for dashboard UIs and API responses without exposing internal domain logic or sensitive operational details.

## API

The following public members define the structure of the `SimplifiedSellerDashboardDto`:

### `SellerId`
*   **Type**: `Guid`
*   **Purpose**: Uniquely identifies the seller within the marketplace system.
*   **Parameters**: None (Property getter).
*   **Return Value**: The globally unique identifier associated with the seller account.
*   **Throws**: Does not throw exceptions.

### `SellerName`
*   **Type**: `string`
*   **Purpose**: Stores the display name of the seller as registered in the system.
*   **Parameters**: None (Property getter).
*   **Return Value**: The textual name of the seller. May be empty but is expected to be non-null.
*   **Throws**: Does not throw exceptions.

### `ActiveListings`
*   **Type**: `int`
*   **Purpose**: Represents the current count of live, purchasable items listed by the seller.
*   **Parameters**: None (Property getter).
*   **Return Value**: A non-negative integer indicating the number of active listings.
*   **Throws**: Does not throw exceptions.

### `TotalRevenue`
*   **Type**: `decimal`
*   **Purpose**: Tracks the cumulative monetary value of all completed transactions attributed to the seller.
*   **Parameters**: None (Property getter).
*   **Return Value**: The total revenue amount calculated using decimal precision to avoid floating-point rounding errors.
*   **Throws**: Does not throw exceptions.

### `PendingPayout`
*   **Type**: `decimal`
*   **Purpose**: Indicates the amount of revenue currently held in escrow or awaiting transfer to the seller's bank account.
*   **Parameters**: None (Property getter).
*   **Return Value**: The pending payout amount using decimal precision.
*   **Throws**: Does not throw exceptions.

### `AverageRating`
*   **Type**: `double`
*   **Purpose**: Provides the arithmetic mean of customer reviews received by the seller.
*   **Parameters**: None (Property getter).
*   **Return Value**: A double-precision floating-point number, typically ranging from 0.0 to 5.0.
*   **Throws**: Does not throw exceptions.

### `LastActivityAt`
*   **Type**: `DateTime?`
*   **Purpose**: Records the timestamp of the seller's most recent interaction with the platform (e.g., login, listing update, order fulfillment).
*   **Parameters**: None (Property getter).
*   **Return Value**: The date and time of the last activity, or `null` if the seller has never logged in or no activity has been recorded.
*   **Throws**: Does not throw exceptions.

## Usage

### Example 1: Serializing for an API Response
This example demonstrates how an instance of the DTO might be populated from a domain entity and returned as JSON in an ASP.NET Core controller.

```csharp
public async Task<ActionResult<SimplifiedSellerDashboardDto>> GetSellerDashboard(Guid sellerId)
{
    var seller = await _sellerRepository.GetByIdAsync(sellerId);
    
    if (seller == null)
    {
        return NotFound();
    }

    var dashboard = new SimplifiedSellerDashboardDto
    {
        SellerId = seller.Id,
        SellerName = seller.BusinessName,
        ActiveListings = seller.Listings.Count(l => l.Status == ListingStatus.Active),
        TotalRevenue = seller.Transactions.Sum(t => t.Amount),
        PendingPayout = seller.Wallet.PendingBalance,
        AverageRating = seller.Reviews.Any() ? seller.Reviews.Average(r => r.Score) : 0.0,
        LastActivityAt = seller.LastLoginTimestamp
    };

    return Ok(dashboard);
}
```

### Example 2: Client-Side Data Consumption
This example shows how a consumer service might utilize the DTO to calculate a health status string based on the provided metrics.

```csharp
public string GetSellerHealthStatus(SimplifiedSellerDashboardDto dashboard)
{
    if (dashboard.ActiveListings == 0)
    {
        return "Inactive: No live listings";
    }

    if (dashboard.AverageRating < 3.5)
    {
        return "Warning: Low average rating";
    }

    if (dashboard.LastActivityAt.HasValue && 
        DateTime.UtcNow.Subtract(dashboard.LastActivityAt.Value).TotalDays > 30)
    {
        return "At Risk: No recent activity";
    }

    return "Healthy";
}
```

## Notes

*   **Nullability**: The `LastActivityAt` property is nullable (`DateTime?`). Consumers must check `HasValue` before accessing the underlying `DateTime` value to avoid `InvalidOperationException`. The `SellerName` property is expected to be non-null, but defensive coding against empty strings is recommended for UI display.
*   **Precision**: Financial fields (`TotalRevenue`, `PendingPayout`) use the `decimal` type to ensure accuracy in monetary calculations. The `AverageRating` uses `double`, which is sufficient for statistical averages but should not be used for financial arithmetic.
*   **Immutability**: As a Data Transfer Object, this type is intended to be immutable after initialization. While the properties are defined with public getters, they should not be modified after the object is constructed and passed to the presentation layer.
*   **Thread Safety**: This class contains only primitive types and immutable value types (`Guid`, `string`, `decimal`, `double`, `DateTime`). Reading from an instance of `SimplifiedSellerDashboardDto` is inherently thread-safe. However, if the object is being populated (via object initializer or constructor) concurrently by multiple threads, external synchronization is required during the construction phase.
