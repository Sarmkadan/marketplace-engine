# ListingsController
The `ListingsController` class is a central component in the `marketplace-engine` project, responsible for managing and providing access to listings. It offers a range of methods to retrieve, create, update, and search listings, making it a crucial part of the marketplace's functionality.

## API
The `ListingsController` class provides the following public members:
- `public ListingsController`: The constructor for the `ListingsController` class, used to initialize a new instance.
- `public async Task<IActionResult> GetListings`: Retrieves a collection of listings. This method returns an `IActionResult` containing the listings, or an error if the retrieval fails. It may throw exceptions if there are issues with data access or processing.
- `public async Task<IActionResult> GetListing`: Retrieves a single listing by its identifier. This method returns an `IActionResult` containing the listing, or an error if the retrieval fails. It may throw exceptions if there are issues with data access or if the listing is not found.
- `public async Task<IActionResult> CreateListing`: Creates a new listing. This method returns an `IActionResult` indicating the success or failure of the creation operation. It may throw exceptions if there are validation errors or issues with data access.
- `public async Task<IActionResult> UpdateListing`: Updates an existing listing. This method returns an `IActionResult` indicating the success or failure of the update operation. It may throw exceptions if there are validation errors, if the listing is not found, or if there are issues with data access.
- `public async Task<IActionResult> SearchListings`: Searches for listings based on specified criteria. This method returns an `IActionResult` containing the search results, or an error if the search fails. It may throw exceptions if there are issues with data access or processing.

## Usage
Here are examples of how to use the `ListingsController` class:
```csharp
// Example 1: Retrieving a listing
var controller = new ListingsController();
var result = await controller.GetListing(1);
if (result.IsSuccessStatusCode)
{
    var listing = (Listing)result.Value;
    Console.WriteLine($"Listing {listing.Id} found: {listing.Name}");
}
else
{
    Console.WriteLine("Failed to retrieve listing");
}

// Example 2: Creating a new listing
var newListing = new Listing { Name = "Example Listing", Description = "This is an example listing" };
var createResult = await controller.CreateListing(newListing);
if (createResult.IsSuccessStatusCode)
{
    var createdListing = (Listing)createResult.Value;
    Console.WriteLine($"Listing created: {createdListing.Id} - {createdListing.Name}");
}
else
{
    Console.WriteLine("Failed to create listing");
}
```

## Notes
When using the `ListingsController` class, consider the following:
- The `GetListings`, `GetListing`, `CreateListing`, `UpdateListing`, and `SearchListings` methods are asynchronous, allowing for non-blocking calls. However, they may still throw exceptions if there are underlying issues with data access or validation.
- The class does not provide explicit thread-safety guarantees beyond what is inherent to the .NET runtime and the async/await pattern. However, since the methods are stateless and primarily interact with external data sources, they can be safely called from multiple threads without fear of data corruption or other concurrency issues.
- Edge cases, such as attempting to retrieve or update a non-existent listing, or creating a listing with invalid data, will result in appropriate error responses or exceptions being thrown. It is the responsibility of the calling code to handle these scenarios appropriately.
