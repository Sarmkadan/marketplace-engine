#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using MarketplaceEngine.Services;
using MarketplaceEngine.Domain.Models;
using MarketplaceEngine.Domain.ValueObjects;
using MarketplaceEngine.Domain.Enums;
using MarketplaceEngine.Domain.Exceptions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

namespace MarketplaceEngine.Examples;

/// <summary>
/// Advanced usage example showing configuration, custom options, and error handling.
/// Demonstrates production-ready usage patterns.
/// </summary>
public class AdvancedUsage
{
    /// <summary>
    /// Demonstrates advanced features: configuration, error handling, bulk operations, and custom configurations.
    /// </summary>
    public static async Task Main(string[] args)
    {
        // Setup: Configure dependency injection with configuration
        var services = new ServiceCollection();

        // Load configuration (in real app, this would come from appsettings.json, environment variables, etc.)
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                ["MarketplaceConfiguration:MaxListingsPerUser"] = "100",
                ["MarketplaceConfiguration:DefaultCurrency"] = "USD",
                ["MarketplaceConfiguration:EnableCaching"] = "true"
            })
            .Build();

        services.AddSingleton(configuration);
        services.AddMarketplaceServices(); // Registers all marketplace services with configuration

        var serviceProvider = services.BuildServiceProvider();

        // Get services
        var userService = serviceProvider.GetRequiredService<UserService>();
        var listingService = serviceProvider.GetRequiredService<ListingService>();
        var searchService = serviceProvider.GetRequiredService<SearchService>();
        var messagingService = serviceProvider.GetRequiredService<MessagingService>();

        Console.WriteLine("=== Marketplace Engine - Advanced Usage Example ===\n");

        try
        {
            // Example 1: Error handling and validation
            Console.WriteLine("1. Demonstrating error handling...");
            try
            {
                // Attempt to create a user with invalid email
                await userService.RegisterUserAsync(
                    email: "invalid-email", // Invalid email
                    username: "testuser",
                    fullName: "Test User",
                    password: "password123"
                );
            }
            catch (ValidationException ex)
            {
                Console.WriteLine($"✓ Caught expected validation error: {ex.Message}");
                Console.WriteLine($"  Error code: {ex.ErrorCode}");
                if (ex.Errors != null && ex.Errors.Any())
                {
                    Console.WriteLine($"  Validation errors: {string.Join(", ", ex.Errors)}");
                }
            }
            Console.WriteLine();

            // Example 2: Creating a user with proper validation
            Console.WriteLine("2. Creating validated user...");
            var seller = await userService.RegisterUserAsync(
                email: "seller@example.com",
                username: "tech_seller",
                fullName: "Tech Seller Inc.",
                password: "SecurePassword456!"
            );

            Console.WriteLine($"✓ Seller created: {seller.Id} - {seller.Username}\n");

            // Example 3: Creating multiple listings with different configurations
            Console.WriteLine("3. Creating multiple listings with various configurations...");

            var listings = new List<(string title, Money price, string category, string[] tags)>
            {
                ("Gaming Laptop RTX 4080", new Money(2299.99m, "USD"), "Electronics", new[] { "laptop", "gaming", "rtx4080", "windows" }),
                ("Designer Office Chair", new Money(450.00m, "USD"), "Furniture", new[] { "office", "chair", "ergonomic", "leather" }),
                ("Vintage Vinyl Collection", new Money(350.00m, "USD"), "Collectibles", new[] { "vinyl", "records", "collection", "vintage", "music" })
            };

            var createdListings = new List<int>();

            foreach (var (title, price, category, tags) in listings)
            {
                var listing = await listingService.CreateListingAsync(
                    sellerId: seller.Id,
                    title: title,
                    description: $"High-quality {title.ToLower()} for sale. Excellent condition.",
                    price: price,
                    category: category,
                    tags: tags,
                    location: new Location
                    {
                        City = "New York",
                        State = "NY",
                        Country = "USA"
                    }
                );

                createdListings.Add(listing.Id);
                Console.WriteLine($"✓ Created: {listing.Title} (ID: {listing.Id}) - ${listing.Price.Amount}");
            }

            Console.WriteLine();

            // Example 4: Advanced search with filtering and pagination
            Console.WriteLine("4. Performing advanced search with filters...");
            var searchResults = await searchService.SearchAsync(
                query: "laptop",
                filters: new MarketplaceEngine.Domain.Models.SearchFilters
                {
                    Category = "Electronics",
                    PriceMin = 1000,
                    PriceMax = 3000,
                    Status = ListingStatus.Active,
                    Location = new Location { City = "New York" }
                },
                pageSize: 10,
                pageNumber: 1,
                sortBy: "price",
                sortOrder: "desc"
            );

            Console.WriteLine($"✓ Search results: {searchResults.TotalCount} total listings");
            Console.WriteLine($"  Page {searchResults.CurrentPage} of {searchResults.TotalPages}");
            Console.WriteLine($"  Showing {searchResults.Items.Count} items:\n");

            foreach (var listing in searchResults.Items)
            {
                Console.WriteLine($"  - {listing.Title} - ${listing.Price.Amount} ({listing.Category})");
            }
            Console.WriteLine();

            // Example 5: Bulk operations
            Console.WriteLine("5. Performing bulk operations...");
            if (createdListings.Count >= 2)
            {
                // Feature the first two listings
                foreach (var listingId in createdListings.Take(2))
                {
                    var listing = await listingService.GetListingAsync(listingId);
                    if (listing != null)
                    {
                        listing.SetFeatured(true);
                        await listingService.UpdateListingAsync(listing);
                        Console.WriteLine($"✓ Listing {listingId} featured");
                    }
                }
            }

            // Example 6: Messaging between users
            Console.WriteLine("\n6. Demonstrating messaging functionality...");

            // Create a buyer user
            var buyer = await userService.RegisterUserAsync(
                email: "buyer@example.com",
                username: "smart_buyer",
                fullName: "Smart Buyer",
                password: "BuyerPass789!"
            );

            // Send a message about the first listing
            if (createdListings.Any())
            {
                var firstListingId = createdListings.First();
                var message = await messagingService.SendMessageAsync(
                    senderId: buyer.Id,
                    recipientId: seller.Id,
                    content: $"Hi, I'm interested in your listing #{firstListingId}. Is it still available?",
                    listingId: firstListingId
                );

                Console.WriteLine($"✓ Message sent: {message.Id} at {message.CreatedAt:t}");

                // Get conversations for the seller
                var conversations = await messagingService.GetUserConversationsAsync(
                    userId: seller.Id,
                    pageSize: 10
                );

                Console.WriteLine($"✓ Seller has {conversations.Items.Count} conversation(s)");

                // Get messages from the conversation
                if (conversations.Items.Any())
                {
                    var conversation = conversations.Items.First();
                    var messages = await messagingService.GetConversationMessagesAsync(
                        conversationId: conversation.Id,
                        pageSize: 50
                    );

                    Console.WriteLine($"  Conversation has {messages.Count} message(s)");
                    foreach (var msg in messages)
                    {
                        Console.WriteLine($"  - From {msg.SenderId}: {msg.Content}");
                    }
                }
            }

            Console.WriteLine("\n=== Advanced usage example completed successfully ===");
        }
        catch (ValidationException ex)
        {
            Console.WriteLine($"✗ Validation error: {ex.Message}");
            Console.WriteLine($"  Error code: {ex.ErrorCode}");
            if (ex.Errors != null)
            {
                Console.WriteLine($"  Details: {string.Join(", ", ex.Errors)}");
            }
        }
        catch (NotFoundException ex)
        {
            Console.WriteLine($"✗ Resource not found: {ex.Message}");
        }
        catch (UnauthorizedException ex)
        {
            Console.WriteLine($"✗ Unauthorized access: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ Unexpected error: {ex.Message}");
            Console.WriteLine("Make sure the Marketplace Engine API is running:");
            Console.WriteLine("  dotnet run --project src/MarketplaceEngine");
        }
    }
}