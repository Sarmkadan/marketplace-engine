# FullTextSearchRequest

Represents a request for full-text search in the marketplace engine. It encapsulates the search query, optional filters (category, price range, tags, condition, featured status), and pagination parameters. This type is used to submit a search operation and is typically populated by the caller before being passed to a search service.

## API

- **`public string Query`**  
  The search query string. This is the primary text used for full-text matching against listing content. Must not be null or empty; behavior is undefined if null.

- **`public Guid? CategoryId`**  
  Optional category identifier. When set, results are filtered to listings belonging to the specified category. A null value means no category filter is applied.

- **`public decimal? MinPrice`**  
  Optional minimum price filter. When set, only listings with a price greater than or equal to this value are returned. A null value means no lower price bound.

- **`public decimal? MaxPrice`**  
  Optional maximum price filter. When set, only listings with a price less than or equal to this value are returned. A null value means no upper price bound.

- **`public List<string>? Tags`**  
  Optional list of tags. When provided, results are filtered to listings that contain at least one of the specified tags. A null value means no tag filter is applied.

- **`public string? Condition`**  
  Optional condition filter (e.g., "New", "Used", "Refurbished"). When set, only listings matching the given condition are returned. A null value means no condition filter.

- **`public bool? FeaturedOnly`**  
  Optional flag to restrict results to featured listings only. When true, only listings marked as featured are returned. When false or null, all listings matching other criteria are returned.

- **`public int Page`**  
  The page number for paginated results. Must be a positive integer (1-based). Behavior is undefined for non-positive values.

- **`public int PageSize`**  
  The number of results per page. Must be a positive integer. Behavior is undefined for non-positive values.

## Usage

**Example 1: Basic search with pagination**  
Performs a simple full-text search for "laptop" and requests the first page with 20 results.

```csharp
var request = new FullTextSearchRequest
{
    Query = "laptop",
    Page = 1,
    PageSize = 20
};
```

**Example 2: Filtered search with category and price range**  
Searches for "camera" within a specific category, priced between $100 and $500, and only returns featured listings.

```csharp
var request = new FullTextSearchRequest
{
    Query = "camera",
    CategoryId = Guid.Parse("a1b2c3d4-e5f6-7890-abcd-ef1234567890"),
    MinPrice = 100m,
    MaxPrice = 500m,
    FeaturedOnly = true,
    Page = 1,
    PageSize = 10
};
```

## Notes

- All filter properties (`CategoryId`, `MinPrice`, `MaxPrice`, `Tags`, `Condition`, `FeaturedOnly`) are optional. A null value indicates that no filter is applied for that criterion.
- `Page` and `PageSize` must be positive integers. Passing zero or negative values results in undefined behavior (may cause exceptions or incorrect pagination).
- The `Query` property is required for meaningful search results; an empty or null query may produce unexpected behavior depending on the search implementation.
- Thread-safety: Instances of `FullTextSearchRequest` are not thread-safe for concurrent modification. They are intended to be constructed and used within a single thread. If the same instance must be accessed from multiple threads, external synchronization is required.
