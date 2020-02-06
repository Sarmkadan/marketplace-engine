# UsersController

The `UsersController` class serves as the primary API endpoint for managing user-related operations within the `marketplace-engine` project. It encapsulates logic for retrieving user profiles, analyzing seller performance metrics, identifying top-performing sellers, updating user information, and handling email verification processes. All operations are implemented as asynchronous actions returning standard HTTP results to ensure non-blocking I/O operations and scalable request handling.

## API

### `public UsersController()`
Initializes a new instance of the `UsersController` class. This constructor sets up the necessary dependencies required for the controller to interact with the underlying data services and business logic layers.

### `public async Task<IActionResult> GetUserProfile()`
Retrieves the detailed profile information for a specific user.
*   **Purpose**: Fetches current user data including personal details, account status, and preferences.
*   **Parameters**: None explicitly defined in the signature; typically relies on route data or authentication context.
*   **Return Value**: Returns a `Task<IActionResult>` which resolves to an `OkObjectResult` containing the user profile data, or an appropriate error result (e.g., `NotFoundResult`) if the user does not exist.
*   **Throws**: May throw exceptions related to data access failures or invalid authentication states if not handled internally by the framework pipeline.

### `public async Task<IActionResult> GetSellerMetrics()`
Aggregates and returns performance metrics for a seller.
*   **Purpose**: Provides statistical data regarding sales volume, revenue, ratings, and other key performance indicators for a seller account.
*   **Parameters**: None explicitly defined in the signature; context is usually derived from the authenticated user or route parameters.
*   **Return Value**: Returns a `Task<IActionResult>` resolving to metrics data wrapped in an `OkObjectResult`, or an error status if the user is not a seller or data is unavailable.
*   **Throws**: Potential exceptions include data consistency errors or permission denied scenarios if the requesting entity lacks authorization.

### `public async Task<IActionResult> GetTopSellers()`
Retrieves a ranked list of the highest-performing sellers on the marketplace.
*   **Purpose**: Generates a leaderboard or list of top sellers based on predefined criteria such as sales volume or customer ratings.
*   **Parameters**: None explicitly defined in the signature.
*   **Return Value**: Returns a `Task<IActionResult>` containing a collection of seller summaries in an `OkObjectResult`.
*   **Throws**: May throw exceptions if the underlying aggregation service fails or if the dataset cannot be computed within the timeout limits.

### `public async Task<IActionResult> UpdateUserProfile()`
Updates the profile information for the authenticated user.
*   **Purpose**: Persists changes to user details such as display name, bio, or contact information.
*   **Parameters**: None explicitly defined in the signature; expects the updated model to be bound from the request body by the MVC framework.
*   **Return Value**: Returns a `Task<IActionResult>` resolving to `OkObjectResult` upon successful update, or `BadRequestResult` if validation fails.
*   **Throws**: Can throw concurrency exceptions if the record was modified by another process, or validation exceptions if the input data violates business rules.

### `public async Task<IActionResult> VerifyEmail()`
Processes an email verification request.
*   **Purpose**: Validates a user's email address using a provided token or code, activating the account or updating the verification status.
*   **Parameters**: None explicitly defined in the signature; typically consumes query parameters or route values for the verification token.
*   **Return Value**: Returns a `Task<IActionResult>` indicating success (`Ok` or `NoContent`) or failure (`BadRequest` or `NotFound`) based on the token validity.
*   **Throws**: May throw exceptions if the token is malformed, expired, or if the database transaction fails during the status update.

## Usage

### Example 1: Retrieving Seller Metrics
The following example demonstrates how a client might invoke the `GetSellerMetrics` action via an `HttpClient` to display dashboard data.

```csharp
public async Task DisplaySellerDashboard(HttpClient httpClient)
{
    // Assuming the endpoint route is configured as /api/users/metrics
    var response = await httpClient.GetAsync("/api/users/metrics");
    
    if (response.IsSuccessStatusCode)
    {
        var metrics = await response.Content.ReadFromJsonAsync<SellerMetricsDto>();
        Console.WriteLine($"Total Sales: {metrics.TotalSales}");
    }
    else
    {
        Console.WriteLine($"Failed to retrieve metrics: {response.StatusCode}");
    }
}
```

### Example 2: Updating a User Profile
This example illustrates sending a PUT or PATCH request to the `UpdateUserProfile` endpoint with a modified user object.

```csharp
public async Task UpdateUserDetails(HttpClient httpClient, UserProfileDto updatedProfile)
{
    var content = JsonContent.Create(updatedProfile);
    
    // Assuming the endpoint route is configured as /api/users/profile
    var response = await httpClient.PutAsync("/api/users/profile", content);
    
    if (response.IsSuccessStatusCode)
    {
        Console.WriteLine("Profile updated successfully.");
    }
    else
    {
        var error = await response.Content.ReadAsStringAsync();
        Console.WriteLine($"Update failed: {error}");
    }
}
```

## Notes

*   **Thread Safety**: As an ASP.NET Core controller, `UsersController` is instantiated per request. Therefore, the instance itself is inherently thread-safe regarding instance members. However, any injected singleton services utilized within these methods must be thread-safe, particularly for high-concurrency endpoints like `GetTopSellers` which may involve shared cache or aggregation logic.
*   **Asynchronous Execution**: All public methods are asynchronous (`async Task`). Callers must await these tasks to prevent thread pool starvation and ensure proper exception propagation. Blocking on these tasks (e.g., using `.Result` or `.Wait()`) can lead to deadlocks in certain synchronization contexts.
*   **State Management**: Since the methods do not expose explicit parameters in the provided signature, they rely heavily on the HTTP context (RouteData, QueryString, Body, and ClaimsPrincipal). Ensure that the hosting environment correctly binds these values before the method logic executes; otherwise, null reference exceptions may occur within the implementation body.
*   **Idempotency**: `UpdateUserProfile` and `VerifyEmail` are state-changing operations. While `UpdateUserProfile` should generally be idempotent for the same payload, `VerifyEmail` may fail if called multiple times with an already consumed token, requiring client-side handling of specific error codes.
