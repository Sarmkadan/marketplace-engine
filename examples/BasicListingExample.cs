#nullable enable
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
/// Demonstrates basic listing creation and retrieval operations.
/// This example shows how to:
/// - Create a new listing
/// - Retrieve listing details
/// - Update a listing
/// - List all listings
/// </summary>
public class BasicListingExample
{
    public static async Task Main(string[] args)
    {
        var services = new ServiceCollection();
        services.AddMarketplaceServices();
        var provider = services.BuildServiceProvider();

        var listingService = provider.GetRequiredService<ListingService>();

        Console.WriteLine("=== Marketplace Engine - Basic Listing Example ===\n");

        try
        {
            // Example 1: Create a new listing
            Console.WriteLine("1. Creating a new listing...");
            var newListing = await listingService.CreateListingAsync(
                sellerId: 1,
                title: "iPhone 14 Pro - 256GB Space Black",
                description: "Excellent condition, barely used. Includes original box and accessories.",
                price: new Money(999.99m, "USD"),
                category: "Electronics",
                tags: new[] { "phone", "apple", "unlocked", "256gb" },
                location: new Location
                {
                    City = "New York",
                    State = "NY",
                    Country = "USA"
                }
            );
            Console.WriteLine($"✓ Created listing ID: {newListing.Id}");
            Console.WriteLine($"  Title: {newListing.Title}");
            Console.WriteLine($"  Price: ${newListing.Price.Amount} {newListing.Price.Currency}\n");

            // Example 2: Retrieve listing details
            Console.WriteLine("2. Retrieving listing details...");
            var listing = await listingService.GetListingAsync(newListing.Id);
            if (listing is not null)
            {
                Console.WriteLine($"✓ Retrieved listing:");
                Console.WriteLine($"  ID: {listing.Id}");
                Console.WriteLine($"  Title: {listing.Title}");
                Console.WriteLine($"  Description: {listing.Description}");
                Console.WriteLine($"  Status: {listing.Status}");
                Console.WriteLine($"  Category: {listing.Category}");
                Console.WriteLine($"  Tags: {string.Join(", ", listing.Tags)}\n");
            }

            // Example 3: Create another listing
            Console.WriteLine("3. Creating a second listing...");
            var secondListing = await listingService.CreateListingAsync(
                sellerId: 1,
                title: "MacBook Pro 16\" M3 Max - 2024",
                description: "Brand new, unopened. Includes AppleCare+ warranty.",
                price: new Money(3499.99m, "USD"),
                category: "Electronics",
                tags: new[] { "laptop", "apple", "macbook", "professional" },
                location: new Location
                {
                    City = "San Francisco",
                    State = "CA",
                    Country = "USA"
                }
            );
            Console.WriteLine($"✓ Created listing ID: {secondListing.Id}\n");

            // Example 4: List all listings
            Console.WriteLine("4. Listing all available listings...");
            var allListings = await listingService.GetAllListingsAsync();
            Console.WriteLine($"✓ Total listings: {allListings.Count}");
            foreach (var item in allListings)
            {
                Console.WriteLine($"  - [{item.Id}] {item.Title} - ${item.Price.Amount}");
            }
            Console.WriteLine();

            // Example 5: Get listings by seller
            Console.WriteLine("5. Getting listings by seller ID 1...");
            var sellerListings = await listingService.GetUserListingsAsync(sellerId: 1);
            Console.WriteLine($"✓ Seller has {sellerListings.Count} listings:");
            foreach (var item in sellerListings)
            {
                Console.WriteLine($"  - {item.Title}");
            }
            Console.WriteLine();

            // Example 6: Update listing price
            Console.WriteLine("6. Updating listing price...");
            var listingToUpdate = await listingService.GetListingAsync(newListing.Id);
            if (listingToUpdate is not null)
            {
                listingToUpdate.UpdatePrice(new Money(949.99m, "USD"));
                await listingService.UpdateListingAsync(listingToUpdate);
                Console.WriteLine($"✓ Price updated to: ${listingToUpdate.Price.Amount}\n");
            }

            // Example 7: Mark listing as inactive
            Console.WriteLine("7. Marking listing as inactive...");
            var listingToDeactivate = await listingService.GetListingAsync(secondListing.Id);
            if (listingToDeactivate is not null)
            {
                listingToDeactivate.Deactivate();
                await listingService.UpdateListingAsync(listingToDeactivate);
                Console.WriteLine($"✓ Listing status: {listingToDeactivate.Status}\n");
            }

            // Example 8: Feature a listing
            Console.WriteLine("8. Featuring a listing...");
            var listingToFeature = await listingService.GetListingAsync(newListing.Id);
            if (listingToFeature is not null)
            {
                listingToFeature.SetFeatured(true);
                await listingService.UpdateListingAsync(listingToFeature);
                Console.WriteLine($"✓ Listing is now featured: {listingToFeature.IsFeatured}\n");
            }

            Console.WriteLine("=== Example completed successfully ===");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ Error: {ex.Message}");
        }
    }
}
