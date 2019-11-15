// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using MarketplaceEngine.Services;
using MarketplaceEngine.Domain.Models;
using MarketplaceEngine.Domain.ValueObjects;
using MarketplaceEngine.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MarketplaceEngine.Examples;

/// <summary>
/// Demonstrates category management and hierarchical organization.
/// This example shows how to:
/// - Create categories and subcategories
/// - Manage category hierarchy
/// - List categories
/// - Get listings by category
/// - Update categories
/// </summary>
public class CategoryManagementExample
{
    public static async Task Main(string[] args)
    {
        var services = new ServiceCollection();
        services.AddMarketplaceServices();
        var provider = services.BuildServiceProvider();

        var categoryService = provider.GetRequiredService<CategoryService>();
        var listingService = provider.GetRequiredService<ListingService>();

        Console.WriteLine("=== Marketplace Engine - Category Management Example ===\n");

        try
        {
            // Example 1: Create main categories
            Console.WriteLine("1. Creating main categories...");
            var electronicsCategory = await categoryService.CreateCategoryAsync(
                name: "Electronics",
                description: "Electronic devices and accessories",
                parentCategoryId: null
            );
            Console.WriteLine($"✓ Created: {electronicsCategory.Name}\n");

            var clothingCategory = await categoryService.CreateCategoryAsync(
                name: "Clothing & Fashion",
                description: "Apparel and fashion items",
                parentCategoryId: null
            );
            Console.WriteLine($"✓ Created: {clothingCategory.Name}\n");

            // Example 2: Create subcategories under Electronics
            Console.WriteLine("2. Creating subcategories under Electronics...");
            var phonesCategory = await categoryService.CreateCategoryAsync(
                name: "Phones & Mobile Devices",
                description: "Smartphones, tablets, and accessories",
                parentCategoryId: electronicsCategory.Id
            );
            Console.WriteLine($"✓ Created: {phonesCategory.Name}\n");

            var laptopsCategory = await categoryService.CreateCategoryAsync(
                name: "Computers & Laptops",
                description: "Desktops, laptops, and peripherals",
                parentCategoryId: electronicsCategory.Id
            );
            Console.WriteLine($"✓ Created: {laptopsCategory.Name}\n");

            var accessoriesCategory = await categoryService.CreateCategoryAsync(
                name: "Accessories",
                description: "Cables, chargers, cases, and more",
                parentCategoryId: electronicsCategory.Id
            );
            Console.WriteLine($"✓ Created: {accessoriesCategory.Name}\n");

            // Example 3: Create subcategories under Clothing
            Console.WriteLine("3. Creating subcategories under Clothing...");
            var mensCategory = await categoryService.CreateCategoryAsync(
                name: "Men's Clothing",
                description: "Shirts, pants, jackets, and more",
                parentCategoryId: clothingCategory.Id
            );
            Console.WriteLine($"✓ Created: {mensCategory.Name}\n");

            var womensCategory = await categoryService.CreateCategoryAsync(
                name: "Women's Clothing",
                description: "Dresses, blouses, pants, and more",
                parentCategoryId: clothingCategory.Id
            );
            Console.WriteLine($"✓ Created: {womensCategory.Name}\n");

            // Example 4: Get all categories
            Console.WriteLine("4. Retrieving all categories...");
            var allCategories = await categoryService.GetAllCategoriesAsync();
            Console.WriteLine($"✓ Total categories: {allCategories.Count}");
            foreach (var category in allCategories.Where(c => c.ParentCategoryId == null))
            {
                Console.WriteLine($"  - {category.Name}");
                var subcats = allCategories.Where(c => c.ParentCategoryId == category.Id);
                foreach (var subcat in subcats)
                {
                    Console.WriteLine($"    └─ {subcat.Name}");
                }
            }
            Console.WriteLine();

            // Example 5: Create listings in different categories
            Console.WriteLine("5. Creating listings in different categories...");
            var iphone = await listingService.CreateListingAsync(
                sellerId: 1,
                title: "iPhone 14 Pro",
                description: "Latest Apple iPhone",
                price: new Money(999.99m, "USD"),
                category: phonesCategory.Name,
                tags: new[] { "phone", "apple" },
                location: new Location { City = "New York", Country = "USA" }
            );
            Console.WriteLine($"✓ Listed in {phonesCategory.Name}: {iphone.Title}\n");

            var macbook = await listingService.CreateListingAsync(
                sellerId: 1,
                title: "MacBook Pro 16\"",
                description: "Professional laptop from Apple",
                price: new Money(3499.99m, "USD"),
                category: laptopsCategory.Name,
                tags: new[] { "laptop", "apple", "professional" },
                location: new Location { City = "San Francisco", Country = "USA" }
            );
            Console.WriteLine($"✓ Listed in {laptopsCategory.Name}: {macbook.Title}\n");

            var tshirt = await listingService.CreateListingAsync(
                sellerId: 2,
                title: "Vintage Band T-Shirt",
                description: "Rare vintage band merchandise",
                price: new Money(45.99m, "USD"),
                category: mensCategory.Name,
                tags: new[] { "vintage", "band", "tshirt" },
                location: new Location { City = "Los Angeles", Country = "USA" }
            );
            Console.WriteLine($"✓ Listed in {mensCategory.Name}: {tshirt.Title}\n");

            // Example 6: Get listings by category
            Console.WriteLine("6. Getting listings by category...");
            var electronicsListings = await categoryService.GetCategoryListingsAsync(electronicsCategory.Id);
            Console.WriteLine($"✓ Electronics category: {electronicsListings.Count} listings");
            foreach (var listing in electronicsListings)
            {
                Console.WriteLine($"  - {listing.Title}");
            }
            Console.WriteLine();

            var clothingListings = await categoryService.GetCategoryListingsAsync(clothingCategory.Id);
            Console.WriteLine($"✓ Clothing category: {clothingListings.Count} listings");
            foreach (var listing in clothingListings)
            {
                Console.WriteLine($"  - {listing.Title}");
            }
            Console.WriteLine();

            // Example 7: Get subcategory listings
            Console.WriteLine("7. Getting listings from Phones subcategory...");
            var phoneListings = await categoryService.GetCategoryListingsAsync(phonesCategory.Id);
            Console.WriteLine($"✓ Found {phoneListings.Count} phone listings:");
            foreach (var listing in phoneListings)
            {
                Console.WriteLine($"  - {listing.Title} (${listing.Price.Amount})");
            }
            Console.WriteLine();

            // Example 8: Get category statistics
            Console.WriteLine("8. Category statistics...");
            var stats = new Dictionary<string, int>();
            foreach (var cat in allCategories)
            {
                var count = (await categoryService.GetCategoryListingsAsync(cat.Id)).Count;
                stats[cat.Name] = count;
            }
            Console.WriteLine($"✓ Listings per category:");
            foreach (var kvp in stats.OrderByDescending(x => x.Value))
            {
                Console.WriteLine($"  - {kvp.Key}: {kvp.Value} listings");
            }
            Console.WriteLine();

            // Example 9: Update category
            Console.WriteLine("9. Updating category description...");
            var categoryToUpdate = await categoryService.GetCategoryAsync(phonesCategory.Id);
            if (categoryToUpdate != null)
            {
                await categoryService.UpdateCategoryAsync(
                    id: categoryToUpdate.Id,
                    name: categoryToUpdate.Name,
                    description: "All mobile devices: smartphones, tablets, smartwatches, and related accessories"
                );
                Console.WriteLine($"✓ Updated: {categoryToUpdate.Name}\n");
            }

            // Example 10: Deactivate category
            Console.WriteLine("10. Deactivating a category...");
            await categoryService.DeactivateCategoryAsync(accessoriesCategory.Id);
            Console.WriteLine($"✓ Deactivated: {accessoriesCategory.Name}\n");

            // Example 11: Get active categories
            Console.WriteLine("11. Getting active categories...");
            var activeCategories = await categoryService.GetAllCategoriesAsync();
            var activeCategoryCount = activeCategories.Count(c => c.IsActive);
            Console.WriteLine($"✓ Active categories: {activeCategoryCount}");
            foreach (var cat in activeCategories.Where(c => c.IsActive && c.ParentCategoryId == null))
            {
                Console.WriteLine($"  - {cat.Name}");
            }
            Console.WriteLine();

            // Example 12: Category hierarchy visualization
            Console.WriteLine("12. Category hierarchy tree...");
            PrintCategoryTree(allCategories, null, "");
            Console.WriteLine();

            Console.WriteLine("=== Example completed successfully ===");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ Error: {ex.Message}");
        }
    }

    private static void PrintCategoryTree(List<Category> categories, int? parentId, string indent)
    {
        var childCategories = categories
            .Where(c => c.ParentCategoryId == parentId)
            .OrderBy(c => c.Name)
            .ToList();

        foreach (var category in childCategories)
        {
            Console.WriteLine($"{indent}├─ {category.Name}");
            PrintCategoryTree(categories, category.Id, indent + "│  ");
        }
    }
}
