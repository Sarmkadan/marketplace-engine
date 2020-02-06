# SellerDashboardController

The `SellerDashboardController` is an ASP.NET Core MVC controller that provides endpoints for retrieving seller‑specific dashboard information, revenue summaries, and listing statistics within the marketplace engine. It is intended to be used by authenticated seller clients to obtain data required for the seller dashboard UI.

## API

### SellerDashboardController()
Initializes a new instance of the controller. Dependencies (such as services for accessing seller data) are supplied through dependency injection. The constructor does not take any parameters directly; all required services are resolved by the ASP.NET Core DI container.

### GetDashboard()
**Purpose:** Retrieves a composite dashboard view for the currently authenticated seller, including recent orders, active listings, and performance indicators.  
**Parameters:** None (the seller identity is obtained from the request’s authentication context).  
**Return Value:** `Task<IActionResult>` that yields an `OkObjectResult` containing a dashboard model when successful, or an appropriate error result (e.g., `UnauthorizedResult`, `BadRequestObjectResult`) when the request cannot be fulfilled.  
**When it throws:** May propagate exceptions thrown by underlying services, such as `InvalidOperationException` if required data cannot be loaded, or `UnauthorizedAccessException` if the seller context is missing or invalid.

### GetRevenue()
**Purpose:** Returns revenue statistics (e.g., total sales, period‑over‑period growth) for the authenticated seller.  
**Parameters:** None (seller identity derived from authentication).  
**Return Value:** `Task<IActionResult>` yielding an `OkObjectResult` with a revenue summary object on success, or an error result indicating authentication or service failures.  
**When it throws:** Exceptions from the revenue calculation service are bubbled up; typical cases include data access failures or invalid state, resulting in a non‑200 response.

### GetListingStats()
**Purpose:** Provides aggregated statistics about the seller’s listings (e.g., views, clicks, conversion rates).  
**Parameters:** None (seller identity taken from the request).  
**Return Value:** `Task<IActionResult>` that returns an `OkObjectResult` containing a listing statistics payload when the operation succeeds, or an error result for missing authentication or internal faults.  
**When it throws:** Any exception raised by the listing statistics service (e.g., timeout, database error) will cause the method to return a failure status code rather than swallow the exception.

## Usage

```csharp
// Example 1: Using the controller in an ASP.NET Core middleware or test
var controller = HttpContext.RequestServices.GetRequiredService<SellerDashboardController>();
var dashboardResult = await controller.GetDashboard();
if (dashboardResult is OkObjectResult ok)
{
    var dashboard = ok.Value as DashboardModel;
    // Process dashboard data...
}
```

```csharp
// Example 2: Unit‑test style invocation with mocked dependencies
var mockSellerService = new Mock<ISellerService>();
mockSellerService.Setup(s => s.GetDashboardAsync(It.IsAny<string>()))
                 .ReturnsAsync(new DashboardModel { /* sample data */ });

var controller = new SellerDashboardController(mockSellerService.Object);
// Assume controller.User is set to a ClaimsPrincipal representing a seller
var result = await controller.GetDashboard();
Assert.IsType<OkObjectResult>(result);
```

## Notes

- The controller is stateless; all state is held in injected services. Consequently, multiple concurrent calls to any of its action methods are safe provided the underlying services are thread‑safe.
- If the request lacks a valid seller authentication token, the methods will return an `UnauthorizedResult` (HTTP 401) rather than throwing.
- Services invoked by the controller are expected to handle their own transient faults; unhandled exceptions will result in an HTTP 500 response, and the action method will not swallow them.
- No method accepts explicit parameters; all seller‑specific data is inferred from the HTTP request’s user principal. Supplying incorrect or missing principal information will lead to authentication‑related error responses.
