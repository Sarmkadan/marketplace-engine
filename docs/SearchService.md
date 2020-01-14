# SearchService

Central service for executing search operations against the marketplace catalog, including listings, users, categories, tags, and geospatial queries. It provides asynchronous methods to retrieve listings by keywords, tags, categories, proximity, and advanced filters, as well as user ranking and suggestion retrieval.

## API

### `SearchService`

Entry point for marketplace search operations. Initialized with required dependencies for data access and indexing.

### `public async Task<List<Listing>> SearchListingsAsync(string query)`

Performs a free-text search across listing titles, descriptions, and metadata. Returns all matching listings sorted by relevance.

- **Parameters**
  - `query`: Search term to match against indexed text fields.
- **Returns**
  - `List<Listing>`: Matching listings, potentially empty.
- **Exceptions**
  - Throws `ArgumentException` if `query` is null or whitespace.

### `public async Task<List<Listing>> SearchByTagsAsync(IEnumerable<string> tags)`

Finds listings associated with one or more tags. Matching is case-insensitive and supports partial tag overlap.

- **Parameters**
  - `tags`: Collection of tag strings to match.
- **Returns**
  - `List<Listing>`: Listings containing any of the specified tags.
- **Exceptions**
  - Throws `ArgumentNullException` if `tags` is null.
  - Throws `ArgumentException` if any tag is null or whitespace.

### `public async Task<List<Listing>> FindNearbyListingsAsync(double latitude, double longitude, double radiusKm)`

Retrieves listings within a geographic radius around specified coordinates using Haversine distance.

- **Parameters**
  - `latitude`: Geographic latitude in decimal degrees.
  - `longitude`: Geographic longitude in decimal degrees.
  - `radiusKm`: Search radius in kilometers.
- **Returns**
  - `List<Listing>`: Listings within the radius, sorted by proximity.
- **Exceptions**
  - Throws `ArgumentOutOfRangeException` if `radiusKm` ≤ 0.
  - Throws `ArgumentException` if coordinates are invalid (e.g., latitude outside [-90, 90]).

### `public async Task<List<User>> SearchUsersAsync(string query)`

Searches user profiles by name, username, or email. Matching is case-insensitive and supports partial matches.

- **Parameters**
  - `query`: Search term to match against user fields.
- **Returns**
  - `List<User>`: Matching users, potentially empty.
- **Exceptions**
  - Throws `ArgumentException` if `query` is null or whitespace.

### `public async Task<List<User>> GetTopSellersAsync(int count)`

Returns the highest-rated sellers based on aggregated positive feedback and transaction volume.

- **Parameters**
  - `count`: Maximum number of sellers to return.
- **Returns**
  - `List<User>`: Top sellers, sorted by ranking score descending.
- **Exceptions**
  - Throws `ArgumentOutOfRangeException` if `count` < 0.

### `public async Task<(List<Listing> items, int total)> SearchByCategoryAsync(string categoryId, int page = 1, int pageSize = 20)`

Paginated search for listings under a specific category. Returns both items and total count for pagination.

- **Parameters**
  - `categoryId`: Unique identifier of the category.
  - `page`: Page number (1-based).
  - `pageSize`: Number of items per page.
- **Returns**
  - `(List<Listing> items, int total)`: Tuple containing page items and total matching listings.
- **Exceptions**
  - Throws `ArgumentException` if `categoryId` is null or whitespace.
  - Throws `ArgumentOutOfRangeException` if `page` < 1 or `pageSize` < 1.

### `public async Task<List<Listing>> AdvancedSearchAsync(string query, string categoryId = null, double? minPrice = null, double? maxPrice = null, string[] tags = null, string location = null)`

Combines multiple filters into a single search operation. All parameters are optional; omitted filters are ignored.

- **Parameters**
  - `query`: Free-text search term.
  - `categoryId`: Optional category filter.
  - `minPrice`: Optional minimum price filter.
  - `maxPrice`: Optional maximum price filter.
  - `tags`: Optional array of tags to require.
  - `location`: Optional location string for geospatial filtering.
- **Returns**
  - `List<Listing>`: Filtered and sorted listings.
- **Exceptions**
  - Throws `ArgumentException` if `query` is null or whitespace and no other filters are provided.
  - Throws `ArgumentOutOfRangeException` if `minPrice` > `maxPrice`.

### `public async Task<List<Listing>> GetTrendingListingsAsync(int count)`

Returns the most recently viewed or interacted-with listings, based on aggregated view and interaction metrics.

- **Parameters**
  - `count`: Number of trending listings to return.
- **Returns**
  - `List<Listing>`: Trending listings, sorted by trend score descending.
- **Exceptions**
  - Throws `ArgumentOutOfRangeException` if `count` < 0.

### `public async Task<List<string>> GetSearchSuggestionsAsync(string query, int maxSuggestions = 5)`

Generates autocomplete suggestions based on indexed terms and query prefix.

- **Parameters**
  - `query`: Prefix to match for suggestions.
  - `maxSuggestions`: Maximum number of suggestions to return.
- **Returns**
  - `List<string>`: Sorted list of matching suggestions.
- **Exceptions**
  - Throws `ArgumentException` if `query` is null or whitespace.
  - Throws `ArgumentOutOfRangeException` if `maxSuggestions` < 0.

## Usage
