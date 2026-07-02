#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using MarketplaceEngine.Services;
using MarketplaceEngine.Domain.Models;
using MarketplaceEngine.Domain.ValueObjects;
using Microsoft.Extensions.DependencyInjection;

namespace MarketplaceEngine.Examples;

/// <summary>
/// Basic usage example showing minimal setup and first API call.
/// Perfect for getting started with Marketplace Engine.
/// </summary>
public class BasicUsage
{
    /// <summary>
    /// Demonstrates the simplest possible usage: creating a user and a listing.
    /// </summary>
    public static async Task Main(string[] args)
    {
        // Setup: Configure dependency injection (minimal setup)
        var services = new ServiceCollection();
        services.AddMarketplaceServices(); // Registers all marketplace services
        var serviceProvider = services.BuildServiceProvider();

        // Get the services we need
        var userService = serviceProvider.GetRequiredService<UserService>();
        var listingService = serviceProvider.GetRequiredService<ListingService>();

        Console.WriteLine("=== Marketplace Engine - Basic Usage Example ===\n");

        try
        {
            // Step 1: Create a user (required before creating listings)
            Console.WriteLine("1. Creating a new user...");
            var user = await userService.RegisterUserAsync(
                email: "john.doe@example.com",
                username: "johndoe",
                fullName: "John Doe",
                password: "SecurePass123!"
            );

            Console.WriteLine($"✓ User created: {user.Id} - {user.Username}\n");

            // Step 2: Create a listing (first API call)
            Console.WriteLine("2. Creating first listing...");
            var listing = await listingService.CreateListingAsync(
                sellerId: user.Id,
                title: "Vintage Camera Lens",
                description: "Excellent condition, fully functional",
                price: new Money(149.99m, "USD"),
                category: "Electronics",
                tags: new[] { "camera", "lens", "vintage", "photography" },
                location: new Location
                {
                    City = "London",
                    Country = "UK"
                }
            );

            Console.WriteLine($"✓ Listing created: {listing.Id}");
            Console.WriteLine($"  Title: {listing.Title}");
            Console.WriteLine($"  Price: ${listing.Price.Amount} {listing.Price.Currency}\n");

            // Step 3: Retrieve the listing to confirm
            Console.WriteLine("3. Retrieving listing to confirm...");
            var retrievedListing = await listingService.GetListingAsync(listing.Id);

            if (retrievedListing != null)
            {
                Console.WriteLine($"✓ Listing retrieved successfully:");
                Console.WriteLine($"  ID: {retrievedListing.Id}");
                Console.WriteLine($"  Title: {retrievedListing.Title}");
                Console.WriteLine($"  Status: {retrievedListing.Status}");
                Console.WriteLine($"  Seller ID: {retrievedListing.SellerId}\n");
            }

            Console.WriteLine("=== Basic usage example completed successfully ===");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ Error occurred: {ex.Message}");
            Console.WriteLine("Make sure the Marketplace Engine API is running:");
            Console.WriteLine("  dotnet run --project src/MarketplaceEngine");
        }
    }
}