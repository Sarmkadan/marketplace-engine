# ApiResponseExtensions

The `ApiResponseExtensions` class provides a comprehensive set of static extension methods designed to simplify the transformation, enrichment, and validation handling of `ApiResponse` and `PagedResponse` objects within the marketplace-engine. These utilities facilitate fluent chaining of API responses, enabling efficient mapping of domain models to Data Transfer Objects (DTOs), propagation of request identifiers across service layers, and standardized reporting of field-level validation errors.

## API

### Map<TSource, TResult>(this ApiResponse<TSource> response, Func<TSource, TResult> mapper)
Transforms the payload of an `ApiResponse<TSource>` into an `ApiResponse<TResult>` using the provided mapping function.
*   **Parameters:** `response` (the source `ApiResponse`), `mapper` (a function to convert `TSource` to `TResult`).
*   **Returns:** A new `ApiResponse<TResult>` containing the mapped result.
*   **Throws:** `ArgumentNullException` if `response` or `mapper` is null.

### Map<TResult>(this ApiResponse response, Func<TResult> mapper)
Transforms a non-generic `ApiResponse` into a generic `ApiResponse<TResult>` by executing the provided function to populate the result.
*   **Parameters:** `response` (the source `ApiResponse`), `mapper` (a function to generate the `TResult`).
*   **Returns:** A new `ApiResponse<TResult>` containing the result from the mapper.
*   **Throws:** `ArgumentNullException` if `response` or `mapper` is null.

### ToPagedResponse<T>(this IEnumerable<T> source, ...)
Converts an `IEnumerable<T>` into a `PagedResponse<T>`, incorporating pagination metadata required for consistent API response structures.
*   **Parameters:** `source` (the data collection), and associated pagination parameters (e.g., page number, page size, total count).
*   **Returns:** A `PagedResponse<T>` populated with the data and pagination metadata.

### WithRequestId<T>(this ApiResponse<T> response, string requestId)
Associates a unique request identifier with the provided generic `ApiResponse<T>`, aiding in distributed tracing and debugging.
*   **Parameters:** `response` (the target `ApiResponse`), `requestId` (the string identifier).
*   **Returns:** The `ApiResponse<T>` instance with the updated request ID.

### WithRequestId(this ApiResponse response, string requestId)
Associates a unique request identifier with the provided non-generic `ApiResponse`.
*   **Parameters:** `response` (the target `ApiResponse`), `requestId` (the string identifier).
*   **Returns:** The `ApiResponse` instance with the updated request ID.

### FieldValidationError<T>(this ApiResponse<T> response, ...)
Marks an `ApiResponse<T>` as containing field-level validation errors, allowing for structured error reporting to clients.
*   **Parameters:** `response` (the target `ApiResponse`), and parameters defining the specific validation error details (e.g., field name and error message).
*   **Returns:** The `ApiResponse<T>` instance updated with validation error information.

## Usage

```csharp
// Example 1: Mapping a domain model to a DTO and attaching a Request ID
var productApiResponse = _productService.GetProduct(productId);
var responseDto = productApiResponse
    .Map(product => new ProductDto(product.Name, product.Price))
    .WithRequestId(correlationId);

// Example 2: Creating a paged response from a filtered collection
var allProducts = _productRepository.List();
var pagedResponse = allProducts
    .Where(p => p.IsActive)
    .ToPagedResponse(page: 1, pageSize: 20, totalCount: activeProductCount);
```

## Notes

*   **Thread Safety:** The methods in this class are stateless and do not modify the internal state of the `ApiResponse` objects in a thread-unsafe manner, provided that the `ApiResponse` instances themselves are not shared concurrently across threads in a way that violates their own thread-safety guarantees.
*   **Null Handling:** Extension methods operating on `ApiResponse` objects will generally throw an `ArgumentNullException` if the `this` parameter is null. It is recommended to ensure response objects are initialized before applying these extensions.
*   **Exception Handling:** Mapping functions passed to `Map` should be handled carefully; any exception thrown within the mapper will propagate to the caller of the extension method.
