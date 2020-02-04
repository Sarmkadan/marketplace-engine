# CategoriesController
The `CategoriesController` class is a crucial component of the `marketplace-engine` project, responsible for managing and providing access to category-related data. It offers a range of methods for retrieving categories, their listings, statistics, and hierarchical relationships, making it a central point for category-based queries and operations.

## API
### Constructors
* `public CategoriesController`: Initializes a new instance of the `CategoriesController` class.

### Methods
* `public async Task<IActionResult> GetCategories`: Retrieves a list of categories. Returns an `IActionResult` containing the list of categories. May throw exceptions if database operations fail or if there are issues with the request.
* `public async Task<IActionResult> GetCategory`: Retrieves a specific category by its identifier. Returns an `IActionResult` containing the category details. May throw exceptions if the category is not found, database operations fail, or if there are issues with the request.
* `public async Task<IActionResult> GetCategoryListings`: Retrieves the listings associated with a specific category. Returns an `IActionResult` containing the list of listings. May throw exceptions if the category is not found, database operations fail, or if there are issues with the request.
* `public async Task<IActionResult> GetCategoryStatistics`: Retrieves statistical data about a category, such as total listings and average price. Returns an `IActionResult` containing the statistical data. May throw exceptions if the category is not found, database operations fail, or if there are issues with the request.

### Properties
* `public Guid Id`: The unique identifier of the category.
* `public string Name`: The name of the category.
* `public string Description`: A brief description of the category.
* `public Guid? ParentCategoryId`: The identifier of the parent category, if applicable.
* `public int ListingCount`: The number of listings within the category.
* `public List<CategoryDto> SubCategories`: A list of subcategories.
* `public static CategoryDto FromCategory`: A method to convert a category to a `CategoryDto` object.
* `public Guid CategoryId`: The identifier of the category.
* `public int TotalListings`: The total number of listings in the category.
* `public decimal AveragePrice`: The average price of listings in the category.
* `public int TotalViews`: The total number of views for listings in the category.
* `public double AverageViews`: The average number of views per listing in the category.

## Usage
The following examples demonstrate how to use the `CategoriesController` to retrieve category information and statistics:
```csharp
// Example 1: Retrieving a category by its identifier
var categoriesController = new CategoriesController();
var result = await categoriesController.GetCategory(Guid.Parse("category-guid"));
if (result.IsSuccessStatusCode)
{
    var category = (CategoryDto)result.Value;
    Console.WriteLine($"Category Name: {category.Name}, Description: {category.Description}");
}

// Example 2: Retrieving category statistics
var statisticsResult = await categoriesController.GetCategoryStatistics(Guid.Parse("category-guid"));
if (statisticsResult.IsSuccessStatusCode)
{
    var statistics = (CategoryStatisticsDto)statisticsResult.Value;
    Console.WriteLine($"Total Listings: {statistics.TotalListings}, Average Price: {statistics.AveragePrice}");
}
```

## Notes
When using the `CategoriesController`, consider the following:
- The `GetCategories`, `GetCategory`, `GetCategoryListings`, and `GetCategoryStatistics` methods are asynchronous and may throw exceptions if the database operations fail or if there are issues with the request.
- The `FromCategory` method is a static method that can be used to convert a category to a `CategoryDto` object without needing an instance of `CategoriesController`.
- The `CategoriesController` class does not appear to be thread-safe due to the lack of synchronization mechanisms in its methods. Therefore, it is recommended to use it in a thread-safe context or to implement synchronization mechanisms when accessing its methods from multiple threads.
- Edge cases such as null or empty inputs, and categories with no listings or subcategories, should be handled accordingly to prevent exceptions and ensure robustness.
