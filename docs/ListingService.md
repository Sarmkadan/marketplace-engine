# ListingService

`ListingService` coordinates listing lifecycle operations and listing queries. It is defined in `src/MarketplaceEngine/Services/ListingService.cs` and depends on `IListingRepository` and `IUserRepository`.

## Responsibilities

- Create published or draft listings after confirming that the seller exists and is active.
- Enforce listing ownership for updates, visibility changes, draft publication, and delisting.
- Apply the domain model's validation and lifecycle operations before persisting changes.
- Update the seller's `TotalListings` count when a listing or draft is created.
- Record listing views and expressions of interest.
- Retrieve seller, featured, recent, and paginated listing collections.
- Restrict featured-listing administration to users with the `Administrator` role.
- Report the total number of listings.

The service does not own persistence or transaction handling; those concerns remain with the injected repositories.

## Construction

```csharp
public ListingService(IListingRepository listingRepository, IUserRepository userRepository)
```

Both dependencies are required. The constructor throws `ArgumentNullException` when either dependency is `null`.

## Public methods

### CreateListingAsync

```csharp
public async Task<Listing> CreateListingAsync(
    Guid sellerId,
    string title,
    string description,
    decimal price,
    string currency,
    Guid categoryId,
    List<string> imageUrls)
```

Creates, validates, publishes, and persists an active listing. It then increments the seller's `TotalListings` value and persists the seller update. Returns the listing produced by `IListingRepository.AddAsync`.

Required string and list arguments cannot be `null`. A missing seller causes `ResourceNotFoundException`; an inactive seller causes `UnauthorizedException`. Price, currency, and publication rules are enforced by `Money` and `Listing`.

### CreateDraftListingAsync

```csharp
public async Task<Listing> CreateDraftListingAsync(
    Guid sellerId,
    string title,
    string description,
    decimal price,
    string currency,
    Guid categoryId,
    List<string> imageUrls)
```

Creates and persists a draft, then increments and persists the seller's `TotalListings` value. Returns the listing produced by `IListingRepository.AddAsync`.

Required string and list arguments cannot be `null`. A missing seller causes `ResourceNotFoundException`; an inactive seller causes `UnauthorizedException`. Draft and money rules are enforced by the domain objects.

### UpdateListingAsync

```csharp
public async Task<(Listing listing, Guid previousCategoryId)> UpdateListingAsync(
    Guid listingId,
    Guid requesterId,
    string? title = null,
    string? description = null,
    Money? price = null,
    Guid? categoryId = null)
```

Updates the supplied non-empty values, validates the resulting listing for publication, and persists it. A `null` or empty title/description leaves that field unchanged; a `null` price leaves the price unchanged; and a `null` or empty category ID leaves the category unchanged. The returned tuple contains the persisted listing and its category ID from before the update.

A missing listing causes `ResourceNotFoundException`. A requester other than the seller causes `UnauthorizedException`. Domain validation can also reject the resulting listing.

### SetListingVisibilityAsync

```csharp
public async Task<Listing> SetListingVisibilityAsync(
    Guid listingId,
    Guid requesterId,
    bool isVisible)
```

Publishes the listing when `isVisible` is `true`, or unpublishes it when `false`, then persists and returns it. A missing listing causes `ResourceNotFoundException`; a requester other than the seller causes `UnauthorizedException`. Domain lifecycle rules may reject the transition.

### PublishDraftAsync

```csharp
public async Task<Listing> PublishDraftAsync(Guid listingId, Guid requesterId)
```

Publishes and persists a listing whose current status is `ListingStatus.Draft`. A missing listing causes `ResourceNotFoundException`, a requester other than the seller causes `UnauthorizedException`, and a non-draft listing causes `InvalidOperationException`. Domain publication validation may also reject an invalid draft.

### GetListingWithViewAsync

```csharp
public async Task<Listing> GetListingWithViewAsync(Guid listingId)
```

Loads a listing and asks the repository to increment its view count. A missing listing causes `ResourceNotFoundException`.

The returned value is the instance loaded before `IncrementViewCountAsync` is called. Whether it immediately reflects the new count depends on the repository implementation; reload the listing when the updated count is required.

### RecordInterestAsync

```csharp
public async Task<Listing> RecordInterestAsync(Guid listingId)
```

Loads a listing and asks the repository to increment its interest count. A missing listing causes `ResourceNotFoundException`.

The returned value is the instance loaded before `IncrementInterestCountAsync` is called. Whether it immediately reflects the new count depends on the repository implementation; reload the listing when the updated count is required.

### DelistListingAsync

```csharp
public async Task<Listing> DelistListingAsync(Guid listingId, Guid requesterId)
```

Applies the listing's `Delist` domain operation, then persists and returns the result. A missing listing causes `ResourceNotFoundException`; a requester other than the seller causes `UnauthorizedException`. Domain lifecycle rules may reject the transition.

### GetSellerListingsAsync

```csharp
public async Task<List<Listing>> GetSellerListingsAsync(Guid sellerId)
```

Confirms that the seller exists, then returns all listings supplied by `IListingRepository.GetBySellerIdAsync`. A missing seller causes `ResourceNotFoundException`.

### GetFeaturedListingsAsync

```csharp
public async Task<List<Listing>> GetFeaturedListingsAsync(
    int limit = AppConstants.DefaultFeaturedListingLimit)
```

Returns featured listings up to `limit`. Values outside `AppConstants.FeaturedListingMinLimit` through `AppConstants.FeaturedListingMaxLimit` are replaced with `AppConstants.DefaultFeaturedListingLimit` rather than rejected.

### GetRecentListingsAsync

```csharp
public async Task<List<Listing>> GetRecentListingsAsync(
    int days = AppConstants.RecentListingDays)
```

Returns listings from the requested recent-day window. Values outside `AppConstants.RecentListingMinDays` through `AppConstants.RecentListingMaxDays` are replaced with `AppConstants.RecentListingDays` rather than rejected.

### GetPaginatedListingsAsync

```csharp
public async Task<(List<Listing> items, int total)> GetPaginatedListingsAsync(
    int pageNumber,
    int pageSize)
```

Passes the paging arguments to `IListingRepository.GetPagedAsync` and returns both the current page and the total matching count. The service does not validate or normalize the paging arguments.

### MarkAsFeaturedAsync

```csharp
public async Task<Listing> MarkAsFeaturedAsync(Guid listingId, Guid adminId)
```

Confirms that `adminId` identifies an administrator, marks the listing as featured through its domain operation, then persists and returns it. A missing or non-administrator user causes `UnauthorizedException`. A missing listing causes `ResourceNotFoundException`.

### GetTotalListingCountAsync

```csharp
public async Task<int> GetTotalListingCountAsync()
```

Returns the value supplied by `IListingRepository.CountAsync`.

## Example usage

The repositories are normally supplied by dependency injection. The following example assumes they have already been resolved:

```csharp
using MarketplaceEngine.Domain.ValueObjects;
using MarketplaceEngine.Services;

var listingService = new ListingService(listingRepository, userRepository);

var listing = await listingService.CreateListingAsync(
    sellerId,
    title: "Vintage mechanical watch",
    description: "Serviced watch with its original box.",
    price: 850m,
    currency: "USD",
    categoryId,
    imageUrls: new List<string>
    {
        "https://example.test/images/watch-front.jpg"
    });

var (updatedListing, previousCategoryId) =
    await listingService.UpdateListingAsync(
        listing.Id,
        requesterId: sellerId,
        price: new Money(825m, "USD"),
        categoryId: replacementCategoryId);

await listingService.SetListingVisibilityAsync(
    updatedListing.Id,
    requesterId: sellerId,
    isVisible: false);

var (items, total) = await listingService.GetPaginatedListingsAsync(
    pageNumber: 1,
    pageSize: 20);

Console.WriteLine($"Loaded {items.Count} of {total} listings; previous category was {previousCategoryId}.");
```

To create and later publish a draft:

```csharp
var draft = await listingService.CreateDraftListingAsync(
    sellerId,
    title: "Restored desk lamp",
    description: "Draft description to review before publishing.",
    price: 120m,
    currency: "USD",
    categoryId,
    imageUrls: new List<string>());

var published = await listingService.PublishDraftAsync(
    draft.Id,
    requesterId: sellerId);
```
