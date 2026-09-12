# ListingService

Service for managing marketplace listings, including creation, updates, visibility control, retrieval, and seller-specific operations.

## API

### `ListingService`

Initializes a new instance of the `ListingService` class with required dependencies for listing management.

### `async Task<Listing> CreateListingAsync(Guid sellerId, string title, string description, decimal price, string currency, Guid categoryId, List<string> imageUrls)`

Creates and publishes a new listing.

- **Parameters**
  - `sellerId`: The ID of the user creating the listing.
  - `title`: Listing title.
  - `description`: Listing description.
  - `price`: Listing price.
  - `currency`: Listing currency code.
  - `categoryId`: The category ID for the listing.
  - `imageUrls`: List of image URLs.
- **Return value**
  - A `Task` resolving to the created `Listing` entity.
- **Exceptions**
  - Throws `ResourceNotFoundException` if seller does not exist.
  - Throws `UnauthorizedException` if seller is not active.

### `async Task<Listing> CreateDraftListingAsync(Guid sellerId, string title, string description, decimal price, string currency, Guid categoryId, List<string> imageUrls)`

Creates a new listing as a draft.

- **Parameters**
  - `sellerId`: The ID of the user creating the listing.
  - `title`: Listing title.
  - `description`: Listing description.
  - `price`: Listing price.
  - `currency`: Listing currency code.
  - `categoryId`: The category ID for the listing.
  - `imageUrls`: List of image URLs.
- **Return value**
  - A `Task` resolving to the created draft `Listing` entity.
- **Exceptions**
  - Throws `ResourceNotFoundException` if seller does not exist.
  - Throws `UnauthorizedException` if seller is not active.

### `async Task<(Listing listing, Guid previousCategoryId)> UpdateListingAsync(Guid listingId, Guid requesterId, string? title = null, string? description = null, Money? price = null, Guid? categoryId = null)`

Updates an existing listing.

- **Parameters**
  - `listingId`: The unique identifier of the listing.
  - `requesterId`: The ID of the user requesting the update.
  - `title`: Optional new title.
  - `description`: Optional new description.
  - `price`: Optional new price.
  - `categoryId`: Optional new category ID.
- **Return value**
  - A `Task` resolving to a tuple containing the updated `Listing` and the previous category ID.
- **Exceptions**
  - Throws `ResourceNotFoundException` if listing does not exist.
  - Throws `UnauthorizedException` if user is not the owner.

### `async Task<Listing> SetListingVisibilityAsync(Guid listingId, Guid requesterId, bool isVisible)`

Sets the visibility (published status) of a listing.

- **Parameters**
  - `listingId`: The unique identifier of the listing.
  - `requesterId`: The ID of the user requesting the change.
  - `isVisible`: Whether the listing should be visible (published).
- **Return value**
  - A `Task` resolving to the updated `Listing`.
- **Exceptions**
  - Throws `ResourceNotFoundException` if listing does not exist.
  - Throws `UnauthorizedException` if user is not the owner.

### `async Task<Listing> PublishDraftAsync(Guid listingId, Guid requesterId)`

Publishes a draft listing to the marketplace.

- **Parameters**
  - `listingId`: The unique identifier of the listing.
  - `requesterId`: The ID of the user requesting to publish.
- **Return value**
  - A `Task` resolving to the updated `Listing`.
- **Exceptions**
  - Throws `ResourceNotFoundException` if listing does not exist.
  - Throws `UnauthorizedException` if user is not the owner.
  - Throws `InvalidOperationException` if listing is not a draft.

### `async Task<Listing> GetListingWithViewAsync(Guid listingId)`

Retrieves a listing and records the view.

- **Parameters**
  - `listingId`: The unique identifier of the listing.
- **Return value**
  - A `Task` resolving to the retrieved `Listing`.
- **Exceptions**
  - Throws `ResourceNotFoundException` if listing does not exist.

### `async Task<Listing> RecordInterestAsync(Guid listingId)`

Records user interest in a listing.

- **Parameters**
  - `listingId`: The unique identifier of the listing.
- **Return value**
  - A `Task` resolving to the updated `Listing`.
- **Exceptions**
  - Throws `ResourceNotFoundException` if listing does not exist.

### `async Task<Listing> DelistListingAsync(Guid listingId, Guid requesterId)`

Marks a listing as sold or delisted.

- **Parameters**
  - `listingId`: The unique identifier of the listing.
  - `requesterId`: The ID of the user requesting the delist.
- **Return value**
  - A `Task` resolving to the updated `Listing`.
- **Exceptions**
  - Throws `ResourceNotFoundException` if listing does not exist.
  - Throws `UnauthorizedException` if user is not the owner.

### `async Task<List<Listing>> GetSellerListingsAsync(Guid sellerId)`

Retrieves all listings belonging to a specific seller.

- **Parameters**
  - `sellerId`: The unique identifier of the seller.
- **Return value**
  - A `Task` resolving to a list of the seller's listings.
- **Exceptions**
  - Throws `ResourceNotFoundException` if seller does not exist.

### `async Task<List<Listing>> GetFeaturedListingsAsync(int limit = AppConstants.DefaultFeaturedListingLimit)`

Retrieves featured listings.

- **Parameters**
  - `limit`: The maximum number of featured listings to return.
- **Return value**
  - A `Task` resolving to a list of featured listings.

### `async Task<List<Listing>> GetRecentListingsAsync(int days = AppConstants.RecentListingDays)`

Retrieves recent listings.

- **Parameters**
  - `days`: The number of days to look back for recent listings.
- **Return value**
  - A `Task` resolving to a list of recent listings.

### `async Task<(List<Listing> items, int total)> GetPaginatedListingsAsync(int pageNumber, int pageSize)`

Gets a paginated list of listings.

- **Parameters**
  - `pageNumber`: The page number to retrieve.
  - `pageSize`: The number of listings per page.
- **Return value**
  - A `Task` resolving to a tuple containing the list of listings and the total count.

### `async Task<Listing> MarkAsFeaturedAsync(Guid listingId, Guid adminId)`

Marks a listing as featured (Administrator only).

- **Parameters**
  - `listingId`: The unique identifier of the listing.
  - `adminId`: The ID of the administrator.
- **Return value**
  - A `Task` resolving to the updated `Listing`.
- **Exceptions**
  - Throws `UnauthorizedException` if requester is not an administrator.
  - Throws `ResourceNotFoundException` if listing does not exist.

### `async Task<int> GetTotalListingCountAsync()`

Gets the total count of all listings.

- **Return value**
  - A `Task` resolving to the total number of listings.

## Usage Example

```csharp
using MarketplaceEngine.Services;
using MarketplaceEngine.Domain.Models;
using MarketplaceEngine.Domain.ValueObjects;
using MarketplaceEngine.Repositories;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

// Example: Creating a new listing
var listingService = new ListingService(listingRepository, userRepository);

var newListing = await listingService.CreateListingAsync(
    sellerId: Guid.NewGuid(),
    title: "Vintage Watch Collection",
    description: "Beautiful collection of vintage watches from the 1950s-1970s",
    price: 1250.00m,
    currency: "USD",
    categoryId: Guid.NewGuid(),
    imageUrls: new List<string> {
        "https://example.com/images/watch1.jpg",
        "https://example.com/images/watch2.jpg"
    }
);

Console.WriteLine($"Created listing: {newListing.Title}");

// Example: Creating a draft listing
var draftListing = await listingService.CreateDraftListingAsync(
    sellerId: Guid.NewGuid(),
    title: "Draft Listing Title",
    description: "This is a draft listing description",
    price: 99.99m,
    currency: "USD",
    categoryId: Guid.NewGuid(),
    imageUrls: new List<string> { "https://example.com/images/draft.jpg" }
);

Console.WriteLine($"Created draft listing with ID: {draftListing.Id}");

// Example: Publishing a draft listing
var publishedListing = await listingService.PublishDraftAsync(
    listingId: draftListing.Id,
    requesterId: draftListing.SellerId
);

Console.WriteLine($"Published listing: {publishedListing.Title}");

// Example: Updating a listing
var (updatedListing, previousCategoryId) = await listingService.UpdateListingAsync(
    listingId: newListing.Id,
    requesterId: newListing.SellerId,
    title: "Updated Vintage Watch Collection",
    price: new Money(1350.00m, "USD")
);

Console.WriteLine($"Updated listing title: {updatedListing.Title}");
Console.WriteLine($"Previous category ID: {previousCategoryId}");

// Example: Setting listing visibility
var hiddenListing = await listingService.SetListingVisibilityAsync(
    listingId: newListing.Id,
    requesterId: newListing.SellerId,
    isVisible: false
);

Console.WriteLine($"Listing visibility set to: {hiddenListing.Status}");

// Example: Getting seller listings
var sellerListings = await listingService.GetSellerListingsAsync(newListing.SellerId);
Console.WriteLine($"Seller has {sellerListings.Count} listings");

// Example: Getting featured listings
var featuredListings = await listingService.GetFeaturedListingsAsync(limit: 5);
Console.WriteLine($"Retrieved {featuredListings.Count} featured listings");

// Example: Getting paginated listings
var (paginatedListings, totalCount) = await listingService.GetPaginatedListingsAsync(
    pageNumber: 1,
    pageSize: 10
);

Console.WriteLine($"Page 1: {paginatedListings.Count} listings (total: {totalCount})");

// Example: Marking as featured (admin only)
var featuredListing = await listingService.MarkAsFeaturedAsync(
    listingId: newListing.Id,
    adminId: Guid.NewGuid() // admin user ID
);

Console.WriteLine($"Listing featured: {featuredListing.IsFeatured}");
```