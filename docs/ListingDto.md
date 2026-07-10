# ListingDto

The `ListingDto` is a data-transfer object representing a marketplace listing within the `marketplace-engine` project. It carries listing details—including identity, seller information, categorization, pricing, status, and timestamps—across application boundaries without exposing internal domain entities.

## API

### Constructors

```csharp
public ListingDto()
```
Default parameterless constructor. Initializes all reference-type members to `null` and value-type members to their defaults (e.g., `Guid.Empty`, `0` for `decimal` and `int`, `DateTime.MinValue`). Useful for deserialization or incremental property population.

```csharp
public ListingDto(
    Guid SellerId,
    string? Title,
    string? Description,
    decimal? Price,
    Guid? CategoryId,
    string Title,
    string Description)
```
**Purpose:** Overloaded constructor accepting a mix of required and optional listing fields.  
**Parameters:**
- `Guid SellerId` – Identifier of the seller owning the listing.
- `string? Title` – Optional listing title (nullable).
- `string? Description` – Optional listing description (nullable).
- `decimal? Price` – Optional listing price (nullable).
- `Guid? CategoryId` – Optional category identifier (nullable).
- `string Title` – Required listing title (non-nullable).
- `string Description` – Required listing description (non-nullable).

**Remarks:** The presence of both nullable and non-nullable `Title`/`Description` parameters suggests an overload that allows partial construction while enforcing non-null values for the final string properties. The compiler will select this overload based on argument nullability at the call site. No exceptions are thrown by the constructor itself.

### Properties

```csharp
public Guid Id
```
Unique identifier of the listing. Assigned upon creation and remains immutable for the lifetime of the listing.

```csharp
public string Title
```
Non-nullable title of the listing. Holds the display title shown to buyers. Defaults to `null` after parameterless construction; callers should ensure it is populated before use.

```csharp
public string Description
```
Non-nullable description of the listing. Contains the full descriptive text. Defaults to `null` after parameterless construction; callers should ensure it is populated before use.

```csharp
public decimal Price
```
Non-nullable price of the listing. Represents the asking price in the marketplace’s base currency. Defaults to `0m` after parameterless construction.

```csharp
public Guid SellerId
```
Identifier of the seller who owns the listing. Used to correlate the listing with seller profiles and inventory.

```csharp
public string SellerName
```
Display name of the seller. Typically denormalized from the seller service for read-side convenience. Defaults to `null` after parameterless construction.

```csharp
public Guid CategoryId
```
Identifier of the category under which the listing is classified. Used for browsing and filtering.

```csharp
public string Status
```
Current status of the listing (e.g., `"Active"`, `"Sold"`, `"Draft"`, `"Removed"`). The exact allowed values are defined by the marketplace domain. Defaults to `null` after parameterless construction.

```csharp
public int ViewCount
```
Number of times the listing has been viewed by potential buyers. Defaults to `0`.

```csharp
public DateTime CreatedAt
```
UTC timestamp indicating when the listing was originally created. Defaults to `DateTime.MinValue` after parameterless construction.

```csharp
public DateTime? UpdatedAt
```
Nullable UTC timestamp indicating the last modification time. `null` if the listing has never been updated since creation.

## Usage

### Example 1: Creating a listing DTO for a new draft

```csharp
var newListing = new ListingDto
{
    Id = Guid.NewGuid(),
    Title = "Vintage Acoustic Guitar",
    Description = "Well-maintained vintage acoustic guitar, mahogany body, includes hard case.",
    Price = 349.99m,
    SellerId = sellerId,
    SellerName = "MusicShop42",
    CategoryId = instrumentsCategoryId,
    Status = "Draft",
    ViewCount = 0,
    CreatedAt = DateTime.UtcNow,
    UpdatedAt = null
};

// newListing is now ready to be passed to a service or mapped to an entity.
```

### Example 2: Hydrating a listing DTO from a database read and preparing it for an API response

```csharp
ListingDto dto;
using (var reader = await command.ExecuteReaderAsync())
{
    if (await reader.ReadAsync())
    {
        dto = new ListingDto
        {
            Id = reader.GetGuid(0),
            Title = reader.GetString(1),
            Description = reader.GetString(2),
            Price = reader.GetDecimal(3),
            SellerId = reader.GetGuid(4),
            SellerName = reader.GetString(5),
            CategoryId = reader.GetGuid(6),
            Status = reader.GetString(7),
            ViewCount = reader.GetInt32(8),
            CreatedAt = reader.GetDateTime(9),
            UpdatedAt = reader.IsDBNull(10) ? null : reader.GetDateTime(10)
        };
    }
}

// Increment view count before returning to client
dto.ViewCount += 1;

return Ok(dto);
```

## Notes

- **Nullability:** After using the parameterless constructor, `Title`, `Description`, `SellerName`, and `Status` are `null`. Consumers must initialize these before serialization or risk null-reference exceptions in downstream code that expects non-null strings.
- **Default values:** `Price` defaults to `0m`, which may be indistinguishable from a genuinely free listing. Callers should validate business rules before persisting.
- **`UpdatedAt` semantics:** A `null` `UpdatedAt` indicates the listing has never been modified. Code that compares `CreatedAt` and `UpdatedAt` for equality should handle the `null` case explicitly.
- **Thread safety:** `ListingDto` is a plain mutable object with no internal synchronization. It is not safe for concurrent writes. When shared across threads, external synchronization or immutable copies should be used.
- **Constructor overload ambiguity:** The overload with mixed nullable/non-nullable `Title` and `Description` parameters is unusual. Callers must be explicit about argument nullability to avoid unintended overload resolution. Prefer the parameterless constructor with property initializers for clarity.
- **Status values:** The `Status` property carries no built-in validation. Assigning an unrecognized string may cause downstream failures in domain logic or state machines. Always use constants or enums defined by the marketplace domain when setting this property.
