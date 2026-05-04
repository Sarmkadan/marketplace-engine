// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.Extensions.DependencyInjection;
using MarketplaceEngine.Repositories;
using MarketplaceEngine.Services;

namespace MarketplaceEngine.Configuration;

/// <summary>
/// Configures dependency injection for the marketplace application.
/// </summary>
public static class DependencyInjection
{
    // Registers all services and repositories
    public static IServiceCollection AddMarketplaceServices(this IServiceCollection services)
    {
        // Register repositories
        services.AddSingleton<IListingRepository, ListingRepository>();
        services.AddSingleton<IUserRepository, UserRepository>();
        services.AddSingleton<IMessageRepository, MessageRepository>();

        // Register services
        services.AddSingleton<ListingService>();
        services.AddSingleton<SearchService>();
        services.AddSingleton<UserService>();
        services.AddSingleton<ModerationService>();
        services.AddSingleton<CategoryService>();
        services.AddSingleton<MessagingService>();

        return services;
    }

    // Registers API endpoints
    public static void MapMarketplaceEndpoints(this WebApplication app)
    {
        var api = app.MapGroup("/api/v1")
            .WithName("Marketplace API");

        // Health check
        api.MapGet("/health", HealthCheck)
            .WithName("Health Check");

        // Listing endpoints
        var listings = api.MapGroup("/listings");
        listings.MapGet("", GetListings)
            .WithName("Get Listings");

        listings.MapGet("/{id}", GetListing)
            .WithName("Get Listing");

        listings.MapGet("/search", SearchListings)
            .WithName("Search Listings");

        // User endpoints
        var users = api.MapGroup("/users");
        users.MapGet("/{id}", GetUser)
            .WithName("Get User Profile");

        users.MapGet("/sellers/top", GetTopSellers)
            .WithName("Get Top Sellers");

        // Category endpoints
        var categories = api.MapGroup("/categories");
        categories.MapGet("", GetCategories)
            .WithName("Get Categories");

        categories.MapGet("/{id}/listings", GetCategoryListings)
            .WithName("Get Category Listings");
    }

    // Health check endpoint
    private static async Task<IResult> HealthCheck()
    {
        return Results.Ok(new
        {
            status = "healthy",
            timestamp = DateTime.UtcNow,
            version = "1.0.0"
        });
    }

    // Get listings endpoint
    private static async Task<IResult> GetListings(IListingRepository repository, int page = 1, int pageSize = 20)
    {
        var (items, total) = await repository.GetPagedAsync(page, pageSize);
        return Results.Ok(new { items, total, page, pageSize });
    }

    // Get listing by ID endpoint
    private static async Task<IResult> GetListing(Guid id, IListingRepository repository)
    {
        var listing = await repository.GetByIdAsync(id);
        if (listing == null)
            return Results.NotFound();

        await repository.IncrementViewCountAsync(id);
        return Results.Ok(listing);
    }

    // Search listings endpoint
    private static async Task<IResult> SearchListings(string q, SearchService service)
    {
        var results = await service.SearchListingsAsync(q);
        return Results.Ok(new { query = q, results, count = results.Count });
    }

    // Get user profile endpoint
    private static async Task<IResult> GetUser(Guid id, UserService service)
    {
        try
        {
            var user = await service.GetUserAsync(id);
            return Results.Ok(user);
        }
        catch
        {
            return Results.NotFound();
        }
    }

    // Get top sellers endpoint
    private static async Task<IResult> GetTopSellers(SearchService service)
    {
        var sellers = await service.GetTopSellersAsync(10);
        return Results.Ok(sellers);
    }

    // Get categories endpoint
    private static async Task<IResult> GetCategories(IListingRepository repository)
    {
        var allListings = await repository.GetAllAsync();
        var categories = allListings
            .GroupBy(l => l.CategoryId)
            .Select(g => new { categoryId = g.Key, count = g.Count() })
            .ToList();

        return Results.Ok(categories);
    }

    // Get category listings endpoint
    private static async Task<IResult> GetCategoryListings(Guid id, IListingRepository repository)
    {
        var listings = await repository.GetByCategoryIdAsync(id);
        return Results.Ok(new { categoryId = id, listings, count = listings.Count });
    }
}
