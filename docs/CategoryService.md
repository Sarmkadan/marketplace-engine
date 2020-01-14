# CategoryService

The `CategoryService` provides asynchronous operations for managing and querying the hierarchical category data used throughout the marketplace engine. It encapsulates access to the underlying category repository and implements business rules such as activation state handling, slug uniqueness, and tree traversal.

## API

### GetAllCategoriesAsync
- **Purpose:** Retrieves every category record stored in the system, regardless of its position in the hierarchy or activation state.  
- **Parameters:** None.  
- **Return Value:** `Task<List<Category>>` containing all categories.  
- **Throws:**  
  - `InvalidOperationException` if the underlying data store is unavailable.  
  - `OperationCanceledException` if the supplied cancellation token is triggered.

### GetCategoryAsync
- **Purpose:** Returns a single category identified by its unique identifier.  
- **Parameters:**  
  - `categoryId` – the unique identifier of the category to fetch.  
- **Return Value:** `Task<Category>` with the requested category, or `null` if no matching record exists.  
- **Throws:**  
  - `ArgumentNullException` if `categoryId` is `null`.  
  - `InvalidOperationException` on data‑access failures.  
  - `OperationCanceledException` if cancellation is requested.

### GetRootCategoriesAsync
- **Purpose:** Returns the top‑level categories that have no parent.  
- **Parameters:** None.  
- **Return Value:** `Task<List<Category>>` of root categories.  
- **Throws:** Same as `GetCategoryAsync` (data‑access and cancellation exceptions).

### GetSubCategoriesAsync
- **Purpose:** Retrieves the immediate children of a specified parent category.  
- **Parameters:**  
  - `parentCategoryId` – identifier of the parent whose children are requested.  
- **Return Value:** `Task<List<Category>>` containing the sub‑categories.  
- **Throws:**  
  - `ArgumentNullException` if `parentCategoryId` is `null`.  
  - `InvalidOperationException` if the parent does not exist.  
  - `OperationCanceledException` on cancellation.

### GetCategoryHierarchyAsync
- **Purpose:** Returns a category together with all of its descendants, forming a nested hierarchy.  
- **Parameters:**  
  - `categoryId` – identifier of the root category for the hierarchy.  
- **Return Value:** `Task<Category>` where the `SubCategories` property is recursively populated.  
- **Throws:**  
  - `ArgumentNullException` if `categoryId` is `null`.  
  - `InvalidOperationException` if the category cannot be found.  
  - `OperationCanceledException` if the operation is cancelled.

### CreateCategoryAsync
- **Purpose:** Persists a new category to the store.  
- **Parameters:**  
  - `category` – a `Category` instance with the desired properties (Id should be empty/default).  
- **Return Value:** `Task<Category>` representing the saved entity, including any database‑generated identifier.  
- **Throws:**  
  - `ArgumentNullException` if `category` is `null`.  
  - `InvalidOperationException` if a category with the same slug already exists.  
  - `OperationCanceledException` on cancellation.

### UpdateCategoryAsync
- **Purpose:** Updates an existing category’s mutable properties.  
- **Parameters:**  
  - `category` – a `Category` instance containing the identifier of the record to update and the new values.  
- **Return Value:** `Task<Category>` with the updated category.  
- **Throws:**  
  - `ArgumentNullException` if `category` is `null` or its `Id` is empty.  
  - `InvalidOperationException` if the category does not exist or the new slug conflicts with another active category.  
  - `OperationCanceledException` if cancellation occurs.

### DeactivateCategoryAsync
- **Purpose:** Marks a category as inactive without removing it from the data store.  
- **Parameters:**  
  - `categoryId` – identifier of the category to deactivate.  
- **Return Value:** `Task<Category>` representing the category with its `IsActive` flag set to `false`.  
- **Throws:**  
  - `ArgumentNullException` if `categoryId` is `null`.  
  - `InvalidOperationException` if the category is not found or is already inactive.  
  - `OperationCanceledException` on cancellation.

### GetCategoryTreeAsync
- **Purpose:** Returns a flat list of categories arranged in tree order (depth‑first traversal) starting from the root nodes.  
- **Parameters:** None.  
- **Return Value:** `Task<List<Category>>` ordered to reflect the hierarchical structure.  
- **Throws:** Same as the other query methods (data‑access and cancellation exceptions).

### SearchCategoriesAsync
- **Purpose:** Performs a case‑insensitive search across category names and descriptions.  
- **Parameters:**  
  - `searchTerm` – the string to match against category fields.  
  - `includeInactive` (optional) – whether to include deactivated categories in the results (defaults to `false`).  
- **Return Value:** `Task<List<Category>>` of matching categories.  
- **Throws:**  
  - `ArgumentNullException` if `searchTerm` is `null`.  
  - `InvalidOperationException` on query execution errors.  
  - `OperationCanceledException` if the operation is cancelled.

### GetBySlugAsync
- **Purpose:** Retrieves a category by its unique slug (URL‑friendly identifier).  
- **Parameters:**  
  - `slug` – the slug string to look up.  
- **Return Value:** `Task<Category>` with the matching category, or `null` if none exists.  
- **Throws:**  
  - `ArgumentNullException` if `slug` is `null`.  
  - `InvalidOperationException` if the slug is not found.  
  - `OperationCanceledException` on cancellation.

### GetHotCategoriesAsync
- **Purpose:** Returns a list of categories currently flagged as “hot” (e.g., high traffic or promotional).  
- **Parameters:** None.  
- **Return Value:** `Task<List<Category>>` of hot categories.  
- **Throws:** Same as other query methods.

## Usage

```csharp
// Example 1: Obtain all root categories and display their names.
var rootCategories = await categoryService.GetRootCategoriesAsync();
foreach (var cat in rootCategories)
{
    Console.WriteLine($"- {cat.Name}");
}

// Example 2: Create a new sub‑category under an existing parent.
var parent = await categoryService.GetCategoryAsync(parentId);
if (parent != null)
{
    var newCat = new Category
    {
        Name = "Eco‑Friendly Gadgets",
        Slug = "eco-friendly-gadgets",
        ParentId = parent.Id,
        IsActive = true
    };
    var created = await categoryService.CreateCategoryAsync(newCat);
    Console.WriteLine($"Created category Id: {created.Id}");
}
```

```csharp
// Example 3: Search for categories matching a keyword and retrieve the hierarchy for the first hit.
var searchResults = await categoryService.SearchCategoriesAsync("camera", includeInactive: false);
if (searchResults.Any())
{
    var first = searchResults.First();
    var hierarchy = await categoryService.GetCategoryHierarchyAsync(first.Id);
    PrintTree(hierarchy, 0); // helper method to indent SubCategories
}

// Helper to print a category tree.
void PrintTree(Category node, int depth)
{
    Console.WriteLine($"{new string(' ', depth * 2)}- {node.Name}");
    foreach (var child in node.SubCategories ?? Enumerable.Empty<Category>())
    {
        PrintTree(child, depth + 1);
    }
}
```

## Notes

- **Edge Cases:**  
  - Slug uniqueness is enforced only among active categories; creating an inactive category with a duplicate slug of an active one will fail.  
  - `DeactivateCategoryAsync` does not cascade to child categories; children remain in whatever activation state they had.  
  - Search results respect the `includeInactive` flag; when `false`, only categories with `IsActive == true` are returned.  
  - Passing `null` for any required identifier or model argument results in an `ArgumentNullException`.  

- **Thread‑Safety:**  
  The `CategoryService` class itself holds no mutable state; all operations delegate to injected repositories that are expected to be thread‑safe. Consequently, multiple threads may invoke any of the public methods concurrently without additional synchronization, provided the underlying data store supports concurrent access. Consumers should still observe cancellation tokens correctly to avoid leaking resources.
