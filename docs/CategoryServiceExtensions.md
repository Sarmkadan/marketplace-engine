# CategoryServiceExtensions

The `CategoryServiceExtensions` class provides a set of static asynchronous extension methods designed to simplify common data retrieval and transformation operations for category management within the marketplace engine. By encapsulating logic for dictionary conversion, hierarchical name resolution, trending slug identification, and name-based searching, this utility reduces boilerplate code in service layers and controllers while ensuring consistent interaction patterns with the underlying category data source.

## API

### `ToDictionaryAsync`
Converts a sequence of categories into a dictionary mapped by their unique identifiers.
*   **Purpose**: Efficiently indexes a collection of `Category` objects for O(1) lookup by `Guid`.
*   **Parameters**: Accepts a `Task<IEnumerable<Category>>` (or similar enumerable source depending on implementation context) representing the asynchronous stream of categories.
*   **Return Value**: Returns a `Task<Dictionary<Guid, Category>>` where the key is the category ID and the value is the category instance.
*   **Throws**: Throws `ArgumentNullException` if the source task or resulting enumeration is null. May throw `ArgumentException` if duplicate keys are encountered.

### `GetSubCategoryNamesAsync`
Retrieves a flat list of names for all immediate subcategories belonging to a specified parent.
*   **Purpose**: Facilitates UI rendering of dropdowns or navigation menus for child categories.
*   **Parameters**: Requires the parent `Category` object or its `Guid` identifier.
*   **Return Value**: Returns a `Task<List<string>>` containing the names of the subcategories.
*   **Throws**: Throws `NotFoundException` if the specified parent category does not exist.

### `GetHotCategorySlugsAsync`
Fetches a list of URL-friendly slugs for categories currently flagged as "hot" or trending.
*   **Purpose**: Supports marketing features, homepage carousels, or SEO optimization by identifying high-traffic categories.
*   **Parameters**: No parameters required; relies on internal state or configuration to define "hot" status.
*   **Return Value**: Returns a `Task<List<string>>` containing the slugs.
*   **Throws**: Generally does not throw under normal conditions; may propagate database connectivity exceptions.

### `SearchByNameAsync`
Performs a text-based search to find categories matching a specific name pattern.
*   **Purpose**: Enables user-facing search functionality or administrative filtering.
*   **Parameters**: Accepts a `string` query term.
*   **Return Value**: Returns a `Task<List<Category>>` containing all matching category entities.
*   **Throws**: Throws `ArgumentNullException` if the search term is null. Returns an empty list if no matches are found.

## Usage

### Example 1: Indexing Categories for Fast Lookup
This example demonstrates loading all top-level categories and converting them into a dictionary for efficient access during order processing.

```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Marketplace.Engine.Services;
using Marketplace.Engine.Models;

public class OrderProcessor
{
    private readonly ICategoryRepository _repository;

    public OrderProcessor(ICategoryRepository repository)
    {
        _repository = repository;
    }

    public async Task ProcessOrderAsync(Guid categoryId)
    {
        // Load categories and transform to dictionary immediately
        var categoryMap = await _repository.GetAllAsync()
                                           .ToDictionaryAsync();

        if (categoryMap.TryGetValue(categoryId, out var category))
        {
            Console.WriteLine($"Processing order for category: {category.Name}");
        }
        else
        {
            throw new InvalidOperationException("Invalid category ID provided.");
        }
    }
}
```

### Example 2: Retrieving Trending Slugs for SEO
This example fetches hot category slugs to generate dynamic sitemap entries or navigation links.

```csharp
using System;
using System.Threading.Tasks;
using Marketplace.Engine.Services;

public class SeoService
{
    public async Task GenerateTrendingLinksAsync()
    {
        // Retrieve slugs for hot categories
        var hotSlugs = await CategoryServiceExtensions.GetHotCategorySlugsAsync();

        foreach (var slug in hotSlugs)
        {
            var url = $"/marketplace/browse/{slug}";
            await AddToSitemapAsync(url);
        }
    }

    private Task AddToSitemapAsync(string url)
    {
        // Implementation omitted for brevity
        return Task.CompletedTask;
    }
}
```

## Notes

*   **Thread Safety**: As all members are static and operate exclusively on passed-in parameters or stateless internal logic without modifying shared static fields, the methods are thread-safe. However, the caller must ensure that the input collections (e.g., the source enumerable for `ToDictionaryAsync`) are not modified concurrently during enumeration.
*   **Asynchronous Execution**: All methods return `Task` objects and should be awaited. Blocking on these tasks (e.g., using `.Result` or `.Wait()`) in UI or ASP.NET contexts may lead to deadlocks.
*   **Empty Results**: Methods returning lists (`GetSubCategoryNamesAsync`, `GetHotCategorySlugsAsync`, `SearchByNameAsync`) will return an empty `List<T>` rather than `null` when no data is found, preventing null-reference exceptions in consumer code.
*   **Duplicate Keys**: When using `ToDictionaryAsync`, ensure the source data contains unique `Guid` identifiers. If duplicates exist, the underlying `ToDictionary` LINQ operation will throw an exception.
