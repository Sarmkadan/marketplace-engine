# Listing

Represents a single item offered for sale in the marketplace engine. It aggregates all data needed to display, search, and transact a product or service, including ownership, categorization, pricing, status, media, tags, and interaction metrics.

## API

| Member | Type | Purpose | Nullability | Remarks / Exceptions |
|--------|------|---------|-------------|----------------------|
| `Id` | `Guid` | Unique identifier for the listing. | Non‑nullable | Setting to `Guid.Empty` is allowed but usually indicates an unsaved entity. |
| `SellerId` | `Guid` | Identifier of the user who owns the listing. | Non‑nullable | Must correspond to an existing `User.Id`; assigning an unrelated value does not throw but may break referential integrity. |
| `Seller` | `User?` | Navigation property to the seller’s user object. | Nullable | May be `null` if the seller data has not been loaded; assigning `null` is permitted. |
| `CategoryId` | `Guid` | Identifier of the category to which the listing belongs. | Non‑nullable | Similar to `SellerId`; `Guid.Empty` signals an unset category. |
| `Category` | `Category?` | Navigation property to the category object. | Nullable | Can be `null` when the category has not been fetched. |
| `Title` | `string` | Short headline shown in listings and search results. | Non‑nullable | Assigning `null` compiles but will cause a `NullReferenceException` when the title is accessed for display. |
| `Description` | `string` | Detailed description of the item or service. | Non‑nullable | Same null‑assignment caveat as `Title`. |
| `Price` | `Money?` | Monetary amount the seller asks for the item. | Nullable | `null` indicates a price‑on‑request or free listing; setting a non‑null `Money` with a negative amount is allowed but semantically invalid. |
| `Status` | `ListingStatus` | Current lifecycle state (e.g., Draft, Active, Sold, Archived). | Non‑nullable | Changing status does not throw; invalid transitions should be validated by business logic. |
| `Location` | `Location?` | Geographic information associated with the listing (e.g., city, coordinates). | Nullable | May be `null` for online‑only items. |
| `ImageUrls` | `List<string>` | Collection of URLs pointing to images of the item. | Non‑nullable (the list itself) | The list can be empty; assigning `null` replaces the reference and later accesses (e.g., `ImageUrls.Count`) will throw a `NullReferenceException`. Individual entries may be `null` or empty strings, but such values are discouraged. |
| `Tags` | `List<string>` | Free‑form tags used for filtering and discovery. | Non‑nullable (the list itself) | Same null‑reference considerations as `ImageUrls`. |
| `ViewCount` | `int` | Number of times the listing has been viewed. | Non‑nullable | Incrementing beyond `int.MaxValue` will wrap to negative values; avoid overflow by resetting or using a larger type if needed. |
| `InterestCount` | `int` | Number of users who have expressed interest (e.g., saved or followed). | Non‑nullable | Same overflow caveat as `ViewCount`. |
| `Rating` | `Rating?` | Aggregate rating object (e.g., average score and review count). | Nullable | `null` indicates no ratings yet. |
| `IsFeatured` | `bool` | Flag indicating whether the listing receives promoted placement. | Non‑nullable | Setting to `true` may trigger additional billing or visibility logic elsewhere. |
| `CreatedAt` | `DateTime` | Timestamp when the listing was first persisted. | Non‑nullable | Should be set once on creation; later modification is allowed but not recommended. |
| `UpdatedAt` | `DateTime?` | Timestamp of the last modification to any mutable field. | Nullable | Set automatically by the persistence layer; assigning `null` clears the stamp. |
| `PublishedAt` | `DateTime?` | Timestamp when the listing transitioned to an active, publicly visible state. | Nullable | `null` while the listing remains in draft or inactive status. |
| `DueDate` | `DateTime?` | Optional expiration date after which the listing is considered stale. | Nullable | If set, business logic may automatically change `Status` when the date passes. |

## Usage

### Creating a new listing

```csharp
var listing = new Listing
{
    Id = Guid.NewGuid(),
    SellerId = seller.Id,
    Seller = seller,
    CategoryId = category.Id,
    Category = category,
    Title = "Vintage Leather Jacket",
    Description = "Genuine leather, size M, excellent condition.",
    Price = new Money(89.99m, Currency.USD),
    Status = ListingStatus.Draft,
    Location = new Location { City = "Portland", State = "OR" },
    ImageUrls = new List<string> { "https://example.com/img1.jpg" },
    Tags = new List<string> { "fashion", "vintage", "jacket" },
    ViewCount = 0,
    InterestCount = 0,
    IsFeatured = false,
    CreatedAt = DateTime.UtcNow
};
```

### Updating a listing’s status and metrics

```csharp
listing.Status = ListingStatus.Active;
listing.PublishedAt = DateTime.UtcNow;
listing.ViewCount += 1; // Simulate a view
listing.InterestCount += 1; // Simulate a user saving the listing
listing.UpdatedAt = DateTime.UtcNow;
```

## Notes

- **Nullability**: Reference‑type members (`Seller`, `Category`, `Price`, `Price`, `Rating`, `Location`) and the nullable `Price` can be `null`. Assigning `null` to non‑nullable reference types such as `Title` or `Description` does not cause a compile‑time error but will lead to runtime `NullReferenceException` when the value is dereferenced. Defensive code should check for `null` where appropriate.
- **Collections**: `ImageUrls` and `Tags` are instantiated lists. Replacing the list reference with `null` is permitted but will cause subsequent accesses (e.g., `list.Count`, enumeration) to throw. It is safer to clear the existing list (`list.Clear()`) rather than assign `null`.
- **Thread safety**: None of the members are immutable or synchronized. Concurrent reads and writes from multiple threads can result in race conditions, lost updates, or inconsistent state (e.g., `ViewCount` increment lost). If the `Listing` instance is shared across threads, external synchronization (e.g., `lock`, `ReaderWriterLockSlim`, or using concurrent collections) is required.
- **Value ranges**: Numeric counters (`ViewCount`, `InterestCount`) are `int`. Incrementing past `int.MaxValue` will wrap to `Int32.MinValue`. For high‑traffic listings consider using `long` or resetting counters periodically.
- **Guid defaults**: Setting `Id`, `SellerId`, or `CategoryId` to `Guid.Empty` does not throw but may violate foreign‑key constraints in the persistence layer; treat `Guid.Empty` as an unset identifier.
- **Status transitions**: The `Status` enum does not enforce valid state changes; assigning any `ListingStatus` value is allowed. Business logic should validate transitions (e.g., from `Draft` to `Active` only when required fields are populated). 
- **DateTime semantics**: All `DateTime` members are expected to be UTC. Mixing local and UTC values can produce incorrect comparisons; store and compare using `DateTime.UtcNow` or `DateTimeOffset` if zone awareness is needed. 
- **Derived data**: The `Listing` type does not compute derived properties (e.g., age, popularity score). Such calculations should be performed in service or query layers based on the fields above. 

--- 

*End of documentation.*
