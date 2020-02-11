# SearchServiceTests

Unit test suite for the `SearchService` class in the marketplace-engine project. It validates the service's ability to perform various search operations, including keyword-based, tag-based, location-based, category-based, and advanced multi-criteria searches. The tests ensure proper parameter validation, error handling, and delegation to underlying repository components.

## API

### `SearchServiceTests`
Test class containing all unit tests for `SearchService` functionality.

### `SearchListingsAsync_WithEmptyQuery_ThrowsValidationException`
Validates that searching with an empty query string throws a `ValidationException`. Ensures the service enforces non-empty query constraints before processing requests.

### `SearchListingsAsync_WithSingleCharQuery_ThrowsValidationException`
Validates that searching with a single-character query throws a `ValidationException`. Confirms minimum query length enforcement for performance and relevance.

### `SearchListingsAsync_WithOverlongQuery_ThrowsValidationException`
Validates that searching with a query exceeding the maximum allowed length throws a `ValidationException`. Prevents excessively long queries that could degrade performance.

### `SearchListingsAsync_WithValidQuery_DelegatesToRepository`
Validates that a properly formatted query string is correctly passed to the repository layer. Ensures the service delegates search execution without modification.

### `SearchByTagsAsync_WithEmptyTagList_ThrowsValidationException`
Validates that searching with an empty tag list throws a `ValidationException`. Ensures at least one tag is provided for meaningful tag-based searches.

### `SearchByTagsAsync_WithNullTagList_ThrowsValidationException`
Validates that searching with a null tag list throws a `ValidationException`. Confirms null-safety enforcement for tag-based search parameters.

### `SearchByTagsAsync_WithValidTags_DelegatesToRepository`
Validates that a non-empty, non-null tag list is correctly passed to the repository layer. Ensures the service delegates tag search execution without modification.

### `FindNearbyListingsAsync_WithLatitudeBelowMinus90_ThrowsValidationException`
Validates that searching with a latitude below -90 throws a `ValidationException`. Ensures geographic coordinate validity for latitude values.

### `FindNearbyListingsAsync_WithLatitudeAbove90_ThrowsValidationException`
Validates that searching with a latitude above 90 throws a `ValidationException`. Ensures geographic coordinate validity for latitude values.

### `FindNearbyListingsAsync_WithLongitudeBelowMinus180_ThrowsValidationException`
Validates that searching with a longitude below -180 throws a `ValidationException`. Ensures geographic coordinate validity for longitude values.

### `FindNearbyListingsAsync_WithRadiusBelowMinimum_ThrowsValidationException`
Validates that searching with a radius below the minimum allowed value throws a `ValidationException`. Ensures meaningful proximity searches with practical distance constraints.

### `FindNearbyListingsAsync_WithRadiusAboveMaximum_ThrowsValidationException`
Validates that searching with a radius above the maximum allowed value throws a `ValidationException`. Prevents excessively large search radii that could impact performance.

### `FindNearbyListingsAsync_WithValidParameters_DelegatesToRepository`
Validates that valid latitude, longitude, and radius parameters are correctly passed to the repository layer. Ensures the service delegates geographic search execution without modification.

### `SearchByCategoryAsync_WithEmptyGuid_ThrowsValidationException`
Validates that searching with an empty category GUID throws a `ValidationException`. Ensures a valid category identifier is provided for category-based searches.

### `SearchByCategoryAsync_WithValidCategoryId_ReturnsPaginatedResults`
Validates that a valid category GUID returns a paginated result set. Confirms the service correctly processes category-based queries and applies pagination.

### `SearchByCategoryAsync_WithPage2_ReturnsCorrectSlice`
Validates that requesting page 2 returns the correct subset of results. Ensures pagination logic correctly slices the result set based on page size.

### `AdvancedSearchAsync_WithKeywordFilter_ReturnsMatchingListings`
Validates that an advanced search with a keyword filter returns only listings matching the specified keyword. Confirms multi-criteria search functionality with keyword constraints.

### `AdvancedSearchAsync_WithPriceRangeFilter_ReturnsListingsInRange`
Validates that an advanced search with a price range filter returns only listings within the specified price bounds. Confirms multi-criteria search functionality with numeric range constraints.

### `AdvancedSearchAsync_WithCategoryFilter_ReturnsOnlyMatchingCategory`
Validates that an advanced search with a category filter returns only listings belonging to the specified category. Confirms multi-criteria search functionality with categorical constraints.

## Usage
