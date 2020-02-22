# SellerListingPerformanceDto

Data transfer object that aggregates key performance metrics for a seller’s listings within the marketplace engine. It is typically used to convey summary statistics from backend services to API consumers or internal reporting components.

## API

### SellerId
- **Purpose:** Unique identifier of the seller whose performance is being reported.  
- **Type:** `Guid`  
- **Parameters:** None (property).  
- **Return value:** The seller’s identifier.  
- **Exceptions:** The getter does not throw. The setter will throw an `ArgumentException` if supplied with an empty `Guid` (`Guid.Empty`).

### ActiveListings
- **Purpose:** Number of listings currently active and visible to buyers.  
- **Type:** `int`  
- **Parameters:** None.  
- **Return value:** Count of active listings (zero or positive).  
- **Exceptions:** No validation is performed; assigning a negative value is allowed but semantically incorrect.

### InactiveListings
- **Purpose:** Number of listings that are not active (e.g., expired, paused, or disabled).  
- **Type:** `int`  
- **Parameters:** None.  
- **Return value:** Count of inactive listings (zero or positive).  
- **Exceptions:** No validation is performed; assigning a negative value is allowed but semantically incorrect.

### FeaturedListings
- **Purpose:** Number of listings that have been promoted or featured by the seller.  
- **Type:** `int`  
- **Parameters:** None.  
- **Return value:** Count of featured listings (zero or positive).  
- **Exceptions:** No validation is performed; assigning a negative value is allowed but semantically incorrect.

### TotalViews
- **Purpose:** Cumulative number of times all of the seller’s listings have been viewed.  
- **Type:** `long`  
- **Parameters:** None.  
- **Return value:** Total view count (zero or positive).  
- **Exceptions:** No validation is performed; assigning a negative value is allowed but semantically incorrect.

### TotalInterestCount
- **Purpose:** Cumulative number of interest signals (e.g., saves, follows, or wishes) received across all listings.  
- **Type:** `long`  
- **Parameters:** None.  
- **Return value:** Total interest count (zero or positive).  
- **Exceptions:** No validation is performed; assigning a negative value is allowed but semantically incorrect.

### EngagementRate
- **Purpose:** Ratio of interest interactions to views, expressed as a value between 0 and 1 (or 0%–100% when multiplied by 100). Indicates how compelling the listings are to viewers.  
- **Type:** `double`  
- **Parameters:** None.  
- **Return value:** Engagement rate; zero when `TotalViews` is zero.  
- **Exceptions:** The getter does not throw. Setting a value outside the range `[0, 1]` is not enforced by the property.

### ConversionRate
- **Purpose:** Ratio of successful transactions (sales) to views, expressed as a value between 0 and 1 (or 0%–100% when multiplied by 100). Measures effectiveness of listings in generating sales.  
- **Type:** `double`  
- **Parameters:** None.  
- **Return value:** Conversion rate; zero when `TotalViews` is zero.  
- **Exceptions:** The getter does not throw. Setting a value outside the range `[0, 1]` is not enforced by the property.

### TopListings
- **Purpose:** Collection of the seller’s highest‑performing listings, typically sorted by a composite score of views, interest, and conversions.  
- **Type:** `List<TopListingDto>`  
- **Parameters:** None.  
- **Return value:** List of `TopListingDto` instances; may be empty but should not be `null` in a well‑formed DTO.  
- **Exceptions:** The getter does not throw. Assigning `null` is permitted but will cause `NullReferenceException` when the list is accessed.

## Usage

### Example 1: Populating a DTO from service data
```csharp
var dto = new SellerListingPerformanceDto
{
    SellerId = Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6"),
    ActiveListings = 42,
    InactiveListings = 5,
    FeaturedListings = 3,
    TotalViews = 125000,
    TotalInterestCount = 8700,
    EngagementRate = 0.0696, // 6.96%
    ConversionRate = 0.012,  // 1.2%
    TopListings = GetTopListingsForSeller(sellerId) // returns List<TopListingDto>
};
```
This snippet shows how a service layer might assemble the DTO after querying the datastore for counts and computing rates.

### Example 2: Consuming the DTO in an API controller
```csharp
[HttpGet("{sellerId}/performance")]
public IActionResult GetSellerPerformance(Guid sellerId)
{
    var performance = _sellerService.GetListingPerformance(sellerId);
    if (performance == null)
        return NotFound();

    // The DTO is safe to serialize directly; TopListings is guaranteed non‑null.
    return Ok(performance);
}
```
The controller returns the DTO as JSON; clients can rely on the presence of all scalar fields and a (possibly empty) `TopListings` collection.

## Notes
- **Edge cases:**  
  - When `TotalViews` is zero, both `EngagementRate` and `ConversionRate` should be interpreted as zero; the DTO does not enforce this, so consumers must guard against division‑by‑zero if they recompute the rates.  
  - `TopListings` may be empty if the seller has no listings or if the service opts not to populate the field; however, assigning `null` will lead to runtime errors when the list is enumerated.  
  - Negative values for the integer or long counts are not validated by the property setters; callers should ensure they supply semantically correct non‑negative numbers.  
- **Thread‑safety:**  
  - The type contains only exposes mutable; concurrent writes to its fields from multiple threads without external synchronization can result in race conditions and inconsistent state.  
  - Read‑only access after construction is thread‑safe, as the fields are independent and immutable once set.  
  - If the DTO is shared across threads, either treat it as immutable after initialization or synchronize access using locks or concurrent collections.
