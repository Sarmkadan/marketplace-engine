# CategoriesControllerExtensions
Static extension class that provides convenient IActionResult‑returning methods for retrieving category data from a `CategoriesController`.

## API
### GetChildCategories
**Purpose** – Returns the immediate sub‑categories of a specified parent category.  
**Parameters**  
- `this CategoriesController controller` – the controller on which the method is invoked.  
- `int? parentId` – identifier of the parent category; `null` returns root‑level children.  
- `int page` (optional) – page number for paginated results.  
- `int pageSize` (optional) – number of items per page.  
- `CancellationToken cancellationToken` – token to observe cancellation.  
**Return value** – `Task<IActionResult>` that yields `200 OK` with a collection of category DTOs, `204 No Content` if no child categories exist, or `404 Not Found` if the supplied `parentId` does not correspond to an existing category.  
**When it throws** – `ArgumentNullException` if `controller` is `null`; `ArgumentOutOfRangeException` if `page` or `pageSize` are invalid; any exception thrown by the underlying category service propagates as a `500 Internal Server Error`.

### GetRootCategories
**Purpose** – Returns the top‑level categories that have no parent.  
**Parameters** – Same as `GetChildCategories` but without a `parentId` argument (the parent is implicitly treated as null).  
**Return value** – `Task<IActionResult>` yielding `200 OK` with a list of root categories, `204 No Content` when none exist.  
**When it throws** – Identical to `GetChildCategories`.

### GetCategoryHierarchy
**Purpose** – Returns the full category tree, optionally limited to a maximum depth.  
**Parameters**  
- `this CategoriesController controller`  
- `int? maxDepth` – maximum depth to traverse; `null` returns the complete hierarchy.  
- `int page` (optional) – page number for paginated results at each level.  
- `int pageSize` (optional) – number of items per page at each level.  
- `CancellationToken cancellationToken`  
**Return value** – `Task<IActionResult>` yielding `200 OK` with a hierarchical representation of categories, or `204 No Content` if the system contains no categories.  
**When it throws** – Same as `GetChildCategories`.

### GetAllCategoriesStatistics
**Purpose** – Returns aggregate statistics about all categories (e.g., total count, average product count).  
**Parameters**  
- `this CategoriesController controller`  
- `DateTime? startDate` – lower bound of the statistics window; `null` means no lower limit.  
- `DateTime? endDate` – upper bound of the statistics window; `null` means no upper limit.  
- `CancellationToken cancellationToken`  
**Return value** – `Task<IActionResult>` yielding `200 OK` with a statistics DTO, or `204 No Content` when no data is available for the requested window.  
**When it throws** – Same as `GetChildCategories`.

## Usage
Example 1: Retrieving child categories inside a controller action.
```csharp
[HttpGet("categories/{parentId:int?}/children")]
public async Task<IActionResult> GetChildren(
    int? parentId,
    int page = 1,
    int pageSize = 20,
    CancellationToken ct = default)
{
    // The extension method is invoked on the current controller instance.
    return await this.GetChildCategories(parentId, page, pageSize, ct);
}
```

Example 2: Obtaining the category hierarchy from a service that creates a controller instance.
```csharp
var controller = new CategoriesController(categoryService, logger);
var result = await controller.GetCategoryHierarchy(
    maxDepth: 3,
    page: 1,
    pageSize: 100,
    CancellationToken.None);

var okResult = result as OkObjectResult;
var hierarchy = okResult?.Value as IEnumerable<CategoryDto>;
```

## Notes
- The extension methods are thread‑safe with respect to the supplied `CategoriesController` instance because they only read its dependencies and do not mutate shared state.
- If the controller instance has not been properly initialized (its required services are `null`), the methods will throw `ArgumentNullException`.
- Pagination arguments (`page`, `pageSize`) are validated; non‑positive values cause `ArgumentOutOfRangeException`.
- Exceptions originating from the underlying data‑access layer are not caught by the extensions; they bubble up and are translated into appropriate HTTP responses by ASP.NET Core’s exception handling middleware.
- Because each method returns `Task<IActionResult>`, the result can be directly awaited in an action method; the returned object already encodes the correct status code and response body.
- No static fields or global state are stored in `CategoriesControllerExtensions`, so there are no thread‑safety concerns at the type level.
