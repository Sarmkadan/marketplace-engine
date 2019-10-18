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
/// Demonstrates advanced search and filtering capabilities.
/// This example shows how to:
/// - Perform full-text search
/// - Filter by category, price range, and location
/// - Sort and paginate results
/// - Get trending listings
/// </summary>
public class SearchExample
{
    public static async Task Main(string[] args)
    {
        var services = new ServiceCollection();
        services.AddMarketplaceServices();
        var provider = services.BuildServiceProvider();

        var listingService = provider.GetRequiredService<ListingService>();
        var searchService = provider.GetRequiredService<SearchService>();

        Console.WriteLine("=== Marketplace Engine - Search Example ===\n");

        try
        {
            // Seed some test data
            Console.WriteLine("Setting up test data...");
            await SeedTestListings(listingService);
            Console.WriteLine("✓ Test data created\n");

            // Example 1: Simple search
            Console.WriteLine("1. Simple search for 'iPhone'...");
            var simpleSearch = await searchService.SearchAsync(
                query: "iPhone",
                pageSize: 10,
                pageNumber: 1
            );
            Console.WriteLine($"✓ Found {simpleSearch.TotalCount} results:");
            foreach (var item in simpleSearch.Items.Take(3))
            {
                Console.WriteLine($"  - {item.Title} (${item.Price.Amount})");
            }
            Console.WriteLine();

            // Example 2: Search with price filter
            Console.WriteLine("2. Search with price range filter (500-1500)...");
            var priceFilterSearch = await searchService.SearchAsync(
                query: "phone",
                filters: new SearchFilters
                {
                    PriceMin = 500,
                    PriceMax = 1500
                },
                pageSize: 10,
                pageNumber: 1
            );
            Console.WriteLine($"✓ Found {priceFilterSearch.TotalCount} results in price range:");
            foreach (var item in priceFilterSearch.Items)
            {
                Console.WriteLine($"  - {item.Title}: ${item.Price.Amount}");
            }
            Console.WriteLine();

            // Example 3: Search with category filter
            Console.WriteLine("3. Search with category filter (Electronics)...");
            var categorySearch = await searchService.SearchAsync(
                query: "*",
                filters: new SearchFilters
                {
                    Category = "Electronics"
                },
                pageSize: 10,
                pageNumber: 1
            );
            Console.WriteLine($"✓ Found {categorySearch.TotalCount} listings in Electronics:");
            foreach (var item in categorySearch.Items.Take(5))
            {
                Console.WriteLine($"  - {item.Title}");
            }
            Console.WriteLine();

            // Example 4: Search with multiple filters
            Console.WriteLine("4. Advanced search with multiple filters...");
            var advancedSearch = await searchService.SearchAsync(
                query: "phone",
                filters: new SearchFilters
                {
                    Category = "Electronics",
                    PriceMin = 500,
                    PriceMax = 1500,
                    Tags = new[] { "unlocked" }
                },
                pageSize: 10,
                pageNumber: 1
            );
            Console.WriteLine($"✓ Found {advancedSearch.TotalCount} matching listings:");
            foreach (var item in advancedSearch.Items)
            {
                Console.WriteLine($"  - {item.Title}");
                Console.WriteLine($"    Price: ${item.Price.Amount}");
                Console.WriteLine($"    Tags: {string.Join(", ", item.Tags)}");
            }
            Console.WriteLine();

            // Example 5: Paginated results
            Console.WriteLine("5. Paginated search results...");
            var page1 = await searchService.SearchAsync(
                query: "phone",
                pageSize: 3,
                pageNumber: 1
            );
            Console.WriteLine($"✓ Page 1 (3 results per page):");
            Console.WriteLine($"  Total: {page1.TotalCount} | Pages: {page1.TotalPages}");
            foreach (var item in page1.Items)
            {
                Console.WriteLine($"  - {item.Title}");
            }
            Console.WriteLine();

            // Example 6: Get trending listings
            Console.WriteLine("6. Getting trending listings...");
            var trendingListings = await searchService.GetTrendingListingsAsync(limit: 5);
            Console.WriteLine($"✓ Top trending listings:");
            foreach (var item in trendingListings)
            {
                Console.WriteLine($"  - {item.Title} (Views: {item.ViewCount ?? 0})");
            }
            Console.WriteLine();

            // Example 7: Get featured listings
            Console.WriteLine("7. Getting featured listings...");
            var featuredListings = await searchService.GetFeaturedListingsAsync(limit: 5);
            Console.WriteLine($"✓ Featured listings: {featuredListings.Count}");
            foreach (var item in featuredListings)
            {
                Console.WriteLine($"  - {item.Title}");
            }
            Console.WriteLine();

            // Example 8: Location-based search
            Console.WriteLine("8. Location-based search (New York)...");
            var locationSearch = await searchService.SearchAsync(
                query: "*",
                filters: new SearchFilters
                {
                    Location = new Location { City = "New York", Country = "USA" }
                },
                pageSize: 10,
                pageNumber: 1
            );
            Console.WriteLine($"✓ Found {locationSearch.TotalCount} listings in New York:");
            foreach (var item in locationSearch.Items)
            {
                Console.WriteLine($"  - {item.Title} ({item.Location?.City})");
            }
            Console.WriteLine();

            Console.WriteLine("=== Example completed successfully ===");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ Error: {ex.Message}");
        }
    }

    private static async Task SeedTestListings(ListingService service)
    {
        var listings = new[]
        {
            ("iPhone 14 Pro", 999.99m, new[] { "phone", "apple", "unlocked" }, "New York"),
            ("iPhone 13", 799.99m, new[] { "phone", "apple" }, "New York"),
            ("Samsung Galaxy S23", 899.99m, new[] { "phone", "samsung", "unlocked" }, "Los Angeles"),
            ("Google Pixel 7", 599.99m, new[] { "phone", "google" }, "San Francisco"),
            ("MacBook Pro 16\"", 3499.99m, new[] { "laptop", "apple", "professional" }, "New York")
        };

        foreach (var (title, price, tags, city) = in listings)
        {
            try
            {
                await service.CreateListingAsync(
                    sellerId: 1,
                    title: title,
                    description: $"Quality {title} in excellent condition",
                    price: new Money(price, "USD"),
                    category: "Electronics",
                    tags: tags,
                    location: new Location { City = city, Country = "USA" }
                );
            }
            catch
            {
                // Listing might already exist
            }
        }
    }
}
