# SellerDashboardServiceExtensions

Static extension class that provides asynchronous helper methods for retrieving various seller‑dashboard data transfer objects (DTOs) from the marketplace engine.

## API

### GetSimplifiedDashboardAsync
```csharp
public static async Task<SimplifiedSellerDashboardDto> GetSimplifiedDashboardAsync()
```
**Purpose:** Retrieves a high‑level summary of the seller’s dashboard, including key metrics such as active listings, recent orders, and pending payouts.  
**Parameters:** None.  
**Return Value:** A `Task` that completes with a `SimplifiedSellerDashboardDto` populated with the current dashboard snapshot.  
**Exceptions:** May propagate exceptions from the underlying data access layer (e.g., `InvalidOperationException` if the seller context cannot be resolved, or `HttpRequestException` if a remote service call fails).

### GetRevenueSummaryAsync
```csharp
public static async Task<SellerRevenueSummaryDto> GetRevenueSummaryAsync()
```
**Purpose:** Obtains a detailed revenue breakdown for the seller, covering gross sales, refunds, fees, and net earnings over the selected period.  
**Parameters:** None.  
**Return Value:** A `Task` that completes with a `SellerRevenueSummaryDto` containing the revenue figures.  
**Exceptions:** May throw exceptions originating from the revenue calculation services, such as `InvalidOperationException` for missing configuration or `HttpRequestException` for communication failures.

### GetListingPerformanceAsync
```csharp
public static async Task<SellerListingPerformanceDto> GetListingPerformanceAsync()
```
**Purpose:** Returns performance metrics for the seller’s listings, including views, clicks, conversion rates, and inventory turnover.  
**Parameters:** None.  
**Return Value:** A `Task` that completes with a `SellerListingPerformanceDto` summarizing listing activity.  
**Exceptions:** May surface exceptions from the analytics store, e.g., `InvalidOperationException` when required analytics data is unavailable, or `HttpRequestException` for service‑level issues.

### GetComparisonDashboardAsync
```csharp
public static async Task<SellerComparisonDashboardDto> GetComparisonDashboardAsync()
```
**Purpose:** Provides a comparative view of the seller’s performance against platform averages or peer groups, highlighting areas of strength and improvement.  
**Parameters:** None.  
**Return Value:** A `Task` that completes with a `SellerComparisonDashboardDto` containing comparative metrics.  
**Exceptions:** May throw exceptions if the comparison data cannot be fetched or computed, such as `InvalidOperationException` for missing peer‑group data or `HttpRequestException` for remote service errors.

## Usage

```csharp
using MarketplaceEngine.Services;

// Example 1: Fetch simplified dashboard and display key stats
var dashboard = await SellerDashboardServiceExtensions.GetSimplifiedDashboardAsync();
Console.WriteLine($"Active Listings: {dashboard.ActiveListings}");
Console.WriteLine($"Pending Payouts: {dashboard.PendingPayouts:C}");
```

```csharp
using MarketplaceEngine.Services;

// Example 2: Retrieve revenue and listing performance concurrently
var revenueTask = SellerDashboardServiceExtensions.GetRevenueSummaryAsync();
var performanceTask = SellerDashboardServiceExtensions.GetListingPerformanceAsync();

await Task.WhenAll(revenueTask, performanceTask);

var revenue = revenueTask.Result;
var performance = performanceTask.Result;

Console.WriteLine($"Net Revenue: {revenue.NetEarnings:C}");
Console.WriteLine($"Conversion Rate: {performance.ConversionRate:P1}");
```

## Notes

- The methods are stateless and rely only on injected services accessed through the application’s service provider; therefore they are thread‑safe and can be invoked concurrently from multiple threads without additional synchronization.  
- If the underlying data source returns no information for a seller, the methods will still return a populated DTO with default or zero values rather than returning `null`.  
- Callers should handle exceptions appropriately, particularly `OperationCanceledException` if a cancellation token is supplied via the surrounding async context, and consider implementing retry logic for transient `HttpRequestException` failures.  
- These extension methods do not modify any global state; they are purely query‑oriented and safe to use in ASP.NET Core request handlers, background workers, or UI threads.
