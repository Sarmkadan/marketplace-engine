# SellerDashboardService

The `SellerDashboardService` provides aggregated data for a seller’s dashboard in the marketplace engine. It exposes asynchronous methods to retrieve a comprehensive dashboard summary, revenue details, and listing statistics for a given seller. The service is intended to be used in presentation or API layers that require a unified view of seller performance.

## API

### `public SellerDashboardService`

Initializes a new instance of the `SellerDashboardService`.  
No parameters are required. The constructor does not throw exceptions.

---

### `public async Task<SellerDashboardDto> GetDashboardAsync(string sellerId)`

Retrieves the full dashboard data for the specified seller.

- **Parameters**  
  `sellerId` – A non-null, non-empty string identifying the seller.

- **Returns**  
  A `Task<SellerDashboardDto>` representing the asynchronous operation. The result contains the seller’s overall dashboard information (e.g., total listings, active orders, recent activity).

- **Exceptions**  
  - `ArgumentNullException` – Thrown when `sellerId` is `null`.  
  - `ArgumentException` – Thrown when `sellerId` is empty or consists only of white-space characters.  
  - `SellerNotFoundException` – Thrown when no seller with the given identifier exists.

---

### `public async Task<SellerRevenueDto> GetRevenueAsync(string sellerId)`

Retrieves revenue data for the specified seller.

- **Parameters**  
  `sellerId` – A non-null, non-empty string identifying the seller.

- **Returns**  
  A `Task<SellerRevenueDto>` representing the asynchronous operation. The result contains revenue metrics (e.g., total revenue, revenue by period, pending payouts).

- **Exceptions**  
  - `ArgumentNullException` – Thrown when `sellerId` is `null`.  
  - `ArgumentException` – Thrown when `sellerId` is empty or consists only of white-space characters.  
  - `SellerNotFoundException` – Thrown when no seller with the given identifier exists.

---

### `public async Task<SellerListingStatsDto> GetListingStatsAsync(string sellerId)`

Retrieves listing statistics for the specified seller.

- **Parameters**  
  `sellerId` – A non-null, non-empty string identifying the seller.

- **Returns**  
  A `Task<SellerListingStatsDto>` representing the asynchronous operation. The result contains listing metrics (e.g., total listings, active listings, average price, views).

- **Exceptions**  
  - `ArgumentNullException` – Thrown when `sellerId` is `null`.  
  - `ArgumentException` – Thrown when `sellerId` is empty or consists only of white-space characters.  
  - `SellerNotFoundException` – Thrown when no seller with the given identifier exists.

## Usage

### Example 1: Basic dashboard retrieval

```csharp
public async Task DisplaySellerDashboard(string sellerId)
{
    var service = new SellerDashboardService();
    var dashboard = await service.GetDashboardAsync(sellerId);
    Console.WriteLine($"Active listings: {dashboard.ActiveListings}");
    Console.WriteLine($"Pending orders: {dashboard.PendingOrders}");
}
```

### Example 2: Handling exceptions and retrieving multiple data sets

```csharp
public async Task<SellerReport> GenerateSellerReport(string sellerId)
{
    var service = new SellerDashboardService();
    try
    {
        var dashboardTask = service.GetDashboardAsync(sellerId);
        var revenueTask = service.GetRevenueAsync(sellerId);
        var statsTask = service.GetListingStatsAsync(sellerId);

        await Task.WhenAll(dashboardTask, revenueTask, statsTask);

        return new SellerReport
        {
            Dashboard = dashboardTask.Result,
            Revenue = revenueTask.Result,
            ListingStats = statsTask.Result
        };
    }
    catch (SellerNotFoundException ex)
    {
        throw new InvalidOperationException($"Seller '{sellerId}' not found.", ex);
    }
}
```

## Notes

- **Edge cases**  
  - All methods throw `ArgumentNullException` or `ArgumentException` if `sellerId` is invalid.  
  - If the seller does not exist, a `SellerNotFoundException` is thrown.  
  - When the seller exists but has no data (e.g., no listings, no revenue), the returned DTOs contain default or zero values; no exception is raised.

- **Thread safety**  
  The `SellerDashboardService` is not inherently thread-safe. It does not maintain mutable shared state between calls, but its underlying data access dependencies (e.g., database contexts, HTTP clients) may not be safe for concurrent use. Instances should be scoped per request (e.g., as a transient or scoped service in dependency injection) and not shared across threads without synchronization.
