# ValidationUtility

`ValidationUtility` is a static utility class that provides centralized input validation and sanitization for the marketplace engine. It offers a set of deterministic, stateless methods for verifying common data types—such as email addresses, phone numbers, URLs, prices, ratings, GUIDs, and search queries—and for sanitizing free-text input. All methods are thread-safe and do not rely on shared mutable state.

## API

### `public static bool IsValidEmail(string email)`
Validates that the provided string conforms to a standard email address format.  
**Parameters:** `email` — the candidate string to evaluate.  
**Returns:** `true` if the string matches the expected email pattern; otherwise `false`.  
**Throws:** `ArgumentNullException` when `email` is `null`.

### `public static bool IsValidPhoneNumber(string phoneNumber)`
Determines whether the given string represents a valid phone number according to the engine’s accepted formats.  
**Parameters:** `phoneNumber` — the candidate string to evaluate.  
**Returns:** `true` if the string is a recognized phone number; otherwise `false`.  
**Throws:** `ArgumentNullException` when `phoneNumber` is `null`.

### `public static bool IsValidText(string text, int minLength, int maxLength)`
Checks that a text string falls within the specified length bounds and contains only permitted characters.  
**Parameters:** `text` — the string to validate; `minLength` — inclusive minimum character count; `maxLength` — inclusive maximum character count.  
**Returns:** `true` if the text satisfies length and character constraints; otherwise `false`.  
**Throws:** `ArgumentNullException` when `text` is `null`. `ArgumentOutOfRangeException` when `minLength` is negative or `maxLength` is less than `minLength`.

### `public static bool IsValidUrl(string url)`
Validates that the supplied string is a well-formed absolute or relative URL accepted by the marketplace engine.  
**Parameters:** `url` — the candidate string to evaluate.  
**Returns:** `true` if the string is a valid URL; otherwise `false`.  
**Throws:** `ArgumentNullException` when `url` is `null`.

### `public static bool IsValidPrice(decimal price)`
Verifies that a monetary value is within the allowed price range (non-negative and not exceeding the engine’s maximum listing price).  
**Parameters:** `price` — the decimal value to check.  
**Returns:** `true` if the price is valid; otherwise `false`.  
**Throws:** No exceptions — accepts any `decimal` value.

### `public static bool IsValidRating(double rating)`
Checks that a rating value falls within the permitted scale (typically 0.0 through 5.0 inclusive) and adheres to allowed precision.  
**Parameters:** `rating` — the double value to check.  
**Returns:** `true` if the rating is within range and precision constraints; otherwise `false`.  
**Throws:** No exceptions — accepts any `double` value, including `NaN` and infinities (which return `false`).

### `public static bool IsValidGuid(string guid)`
Determines whether the given string represents a valid GUID in the formats recognized by the engine.  
**Parameters:** `guid` — the candidate string to evaluate.  
**Returns:** `true` if the string parses to a valid GUID; otherwise `false`.  
**Throws:** `ArgumentNullException` when `guid` is `null`.

### `public static bool IsValidPagination(int page, int pageSize, int totalItems)`
Validates pagination parameters to ensure the requested page and page size are coherent and within acceptable bounds relative to the total item count.  
**Parameters:** `page` — the 1-based page number; `pageSize` — the number of items per page; `totalItems` — the total number of available items.  
**Returns:** `true` if the pagination parameters are valid; otherwise `false`.  
**Throws:** `ArgumentOutOfRangeException` when `page` is less than 1, `pageSize` is less than 1, or `totalItems` is negative.

### `public static string SanitizeInput(string input)`
Removes or escapes potentially dangerous content from free-form text input. This includes stripping control characters, trimming excess whitespace, and neutralizing HTML/script injection vectors where applicable.  
**Parameters:** `input` — the raw string to sanitize.  
**Returns:** A sanitized string. If `input` is `null`, returns `string.Empty`.  
**Throws:** No exceptions — handles `null` gracefully.

### `public static bool IsValidSearchQuery(string query)`
Validates that a search query string is non-empty after trimming, does not exceed the maximum allowed query length, and contains no prohibited characters or sequences.  
**Parameters:** `query` — the candidate search string.  
**Returns:** `true` if the query is acceptable for search execution; otherwise `false`.  
**Throws:** `ArgumentNullException` when `query` is `null`.

## Usage

### Example 1: Validating a product listing submission
```csharp
public bool TryCreateListing(ListingDraft draft, out string errorMessage)
{
    if (!ValidationUtility.IsValidText(draft.Title, minLength: 5, maxLength: 200))
    {
        errorMessage = "Title must be between 5 and 200 characters.";
        return false;
    }

    if (!ValidationUtility.IsValidPrice(draft.Price))
    {
        errorMessage = "Price is outside the allowed range.";
        return false;
    }

    if (!string.IsNullOrEmpty(draft.ImageUrl) && !ValidationUtility.IsValidUrl(draft.ImageUrl))
    {
        errorMessage = "Image URL is malformed.";
        return false;
    }

    draft.Description = ValidationUtility.SanitizeInput(draft.Description);

    errorMessage = null;
    return true;
}
```

### Example 2: Processing a search request with pagination
```csharp
public SearchResult PerformSearch(string query, int page, int pageSize)
{
    if (!ValidationUtility.IsValidSearchQuery(query))
        throw new ArgumentException("Invalid search query.");

    var totalItems = _index.CountMatches(query);

    if (!ValidationUtility.IsValidPagination(page, pageSize, totalItems))
    {
        page = 1;
        pageSize = Math.Min(pageSize, 50);
    }

    var items = _index.Search(query, page, pageSize);
    return new SearchResult(items, totalItems, page, pageSize);
}
```

## Notes

- All methods returning `bool` are pure predicates: they perform no side effects and do not mutate global or instance state. They are safe to call concurrently from multiple threads without synchronization.
- `SanitizeInput` is likewise stateless and thread-safe. Its output depends solely on the provided input string.
- Methods that accept reference-type parameters throw `ArgumentNullException` for `null` arguments where documented. Callers should perform null checks upstream or rely on the exception for defensive programming.
- `IsValidPrice` and `IsValidRating` accept value types and do not throw; they handle edge-case floating-point values (`NaN`, `PositiveInfinity`, `NegativeInfinity`) by returning `false`.
- `IsValidPagination` treats `page` as 1-based. A request for page 1 with `totalItems` of 0 is considered valid (returning an empty result set), whereas a request for page 2 under the same condition is invalid.
- `SanitizeInput` returns `string.Empty` for `null` input, making it safe to chain directly with storage or display logic without additional null handling.
- Validation patterns for `IsValidEmail`, `IsValidPhoneNumber`, and `IsValidUrl` are intentionally conservative and may reject technically legal but unusual formats that the marketplace engine does not support.
