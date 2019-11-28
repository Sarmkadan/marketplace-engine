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
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MarketplaceEngine.Examples;

/// <summary>
/// Integration example showing how to wire Marketplace Engine into ASP.NET Core DI.
/// Demonstrates real-world application setup with logging, configuration, and hosting.
/// </summary>
public class IntegrationExample
{
    /// <summary>
    /// Demonstrates integrating Marketplace Engine into a real ASP.NET Core application.
    /// Shows proper DI setup, logging configuration, and service lifecycle management.
    /// </summary>
    public static async Task Main(string[] args)
    {
        Console.WriteLine("=== Marketplace Engine - ASP.NET Core Integration Example ===\n");

        try
        {
            // Setup: Configure ASP.NET Core host with Marketplace Engine services
            var host = Host.CreateDefaultBuilder(args)
                .ConfigureServices((context, services) =>
                {
                    // Configure Marketplace Engine services
                    services.AddMarketplaceServices();

                    // Configure logging (optional - Marketplace Engine will use default logging if not configured)
                    services.AddLogging(loggingBuilder =>
                    {
                        loggingBuilder.ClearProviders();
                        loggingBuilder.AddConsole();
                        loggingBuilder.SetMinimumLevel(LogLevel.Information);
                    });

                    // Configure Marketplace Engine configuration (optional - can be loaded from appsettings.json)
                    services.Configure<MarketplaceConfiguration>(context.Configuration.GetSection("MarketplaceConfiguration"));

                    // You can also register your own services that depend on Marketplace Engine
                    services.AddScoped<IUserNotificationService, UserNotificationService>();
                })
                .Build();

            // Get the service provider from the host
            var serviceProvider = host.Services;

            // Get services from DI container
            var userService = serviceProvider.GetRequiredService<UserService>();
            var listingService = serviceProvider.GetRequiredService<ListingService>();
            var searchService = serviceProvider.GetRequiredService<SearchService>();
            var logger = serviceProvider.GetRequiredService<ILogger<IntegrationExample>>();

            logger.LogInformation("Starting Marketplace Engine integration example...");

            // Example 1: Register a user through the integrated service
            logger.LogInformation("Creating user via integrated service...");
            var user = await userService.RegisterUserAsync(
                email: "integrated.user@example.com",
                username: "integrated_user",
                fullName: "Integrated User",
                password: "IntegrationPass123!"
            );

            logger.LogInformation("User created: {UserId} - {Username}", user.Id, user.Username);

            // Example 2: Create a listing using the integrated services
            logger.LogInformation("Creating listing via integrated service...");
            var listing = await listingService.CreateListingAsync(
                sellerId: user.Id,
                title: "Professional DSLR Camera Body",
                description: "Canon EOS 5D Mark IV with 24-105mm lens. Includes extra battery and charger.",
                price: new Money(1799.99m, "USD"),
                category: "Photography",
                tags: new[] { "camera", "dslr", "canon", "photography", "professional" },
                location: new Location
                {
                    City = "San Francisco",
                    State = "CA",
                    Country = "USA"
                }
            );

            logger.LogInformation("Listing created: {ListingId} - {Title}", listing.Id, listing.Title);

            // Example 3: Search using integrated search service
            logger.LogInformation("Performing search via integrated service...");
            var searchResults = await searchService.SearchAsync(
                query: "camera",
                filters: new SearchFilters
                {
                    Category = "Photography",
                    PriceMin = 1000,
                    PriceMax = 2500,
                    Status = ListingStatus.Active
                },
                pageSize: 20,
                pageNumber: 1
            );

            logger.LogInformation("Search completed: {TotalResults} results found", searchResults.TotalCount);

            // Example 4: Display results
            Console.WriteLine("\n=== Integration Results ===");
            Console.WriteLine($"User: {user.Username} (ID: {user.Id})");
            Console.WriteLine($"Listing: {listing.Title} (ID: {listing.Id}) - ${listing.Price.Amount}");
            Console.WriteLine($"Search: Found {searchResults.TotalCount} results\n");

            // Example 5: Demonstrate service lifecycle (services are scoped to requests in ASP.NET Core)
            logger.LogInformation("Demonstrating service lifecycle...");

            // In a real web app, each request would get its own service scope
            using (var scope = serviceProvider.CreateScope())
            {
                var scopedUserService = scope.ServiceProvider.GetRequiredService<UserService>();
                var scopedListingService = scope.ServiceProvider.GetRequiredService<ListingService>();

                logger.LogInformation("Services retrieved from new scope - demonstrating proper DI lifecycle");

                // Verify services are working in the scope
                var userListings = await scopedListingService.GetUserListingsAsync(user.Id);
                logger.LogInformation("User has {ListingCount} listings", userListings.Count);
            }

            logger.LogInformation("Service scope disposed successfully");

            Console.WriteLine("\n=== Integration example completed successfully ===");
            Console.WriteLine("\nThis demonstrates how to integrate Marketplace Engine into a real ASP.NET Core application.");
            Console.WriteLine("In a web application, you would typically:");
            Console.WriteLine("1. Register services in Startup.cs or Program.cs");
            Console.WriteLine("2. Use controllers to expose API endpoints");
            Console.WriteLine("3. Inject services into your controllers");
            Console.WriteLine("4. Handle requests and return responses");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ Integration error: {ex.Message}");
            Console.WriteLine("\nMake sure:");
            Console.WriteLine("1. The Marketplace Engine API is running");
            Console.WriteLine("2. All required services are registered in DI container");
            Console.WriteLine("3. Configuration is properly set up");
        }
    }
}

/// <summary>
/// Example service that depends on Marketplace Engine services.
/// Demonstrates how to extend functionality by integrating with your own services.
/// </summary>
public interface IUserNotificationService
{
    Task NotifyUserAsync(int userId, string message);
}

public class UserNotificationService : IUserNotificationService
{
    private readonly UserService _userService;
    private readonly ILogger<UserNotificationService> _logger;

    public UserNotificationService(UserService userService, ILogger<UserNotificationService> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    public async Task NotifyUserAsync(int userId, string message)
    {
        _logger.LogInformation("Sending notification to user {UserId}: {Message}", userId, message);

        // In a real app, you would send an email, push notification, etc.
        await Task.Delay(100); // Simulate async operation

        _logger.LogInformation("Notification sent successfully");
    }
}