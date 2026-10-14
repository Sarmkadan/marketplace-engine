#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.Extensions.DependencyInjection;
using MarketplaceEngine.Repositories;
using MarketplaceEngine.Services;
using MarketplaceEngine.Infrastructure.Caching;
using MarketplaceEngine.Infrastructure.Events;
using MarketplaceEngine.Infrastructure.Background;
using MarketplaceEngine.Infrastructure.Integration;
using MarketplaceEngine.Infrastructure.Security;
using MarketplaceEngine.Infrastructure.Formatters;
using MarketplaceEngine.Utilities;

namespace MarketplaceEngine.Configuration;

/// <summary>
/// Configures dependency injection for the marketplace application.
/// Registers all Phase 1 and Phase 2 services for the marketplace engine.
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
        services.AddSingleton<IPaymentRepository, PaymentRepository>();
        services.AddSingleton<IReviewRepository, ReviewRepository>();

        // Register domain services
        services.AddSingleton<ListingService>();
        services.AddSingleton<SearchService>();
        services.AddSingleton<UserService>();
        services.AddSingleton<ModerationService>();
        services.AddSingleton<CategoryService>();
        services.AddSingleton<MessagingService>();
        services.AddSingleton<PaymentService>();
        services.AddSingleton<ReviewService>();
        services.AddSingleton<SellerDashboardService>();

        // Register caching services
        services.AddSingleton<CacheService>();

        // Register event bus and handlers
        services.AddSingleton<EventBus>();
        services.RegisterEventHandlers();

        // Register background job queue
        services.AddSingleton<BackgroundJobQueue>();

        // Register integration services
        services.AddHttpClient<HttpClientService>();
        services.AddSingleton<IListingProvider, DropshipProviderClient>(provider =>
        {
            var httpClient = provider.GetRequiredService<IHttpClientFactory>().CreateClient();
            var logger = provider.GetRequiredService<ILogger<DropshipProviderClient>>();
            var config = provider.GetRequiredService<IConfiguration>();
            return new DropshipProviderClient(
                new HttpClientService(httpClient, provider.GetRequiredService<ILogger<HttpClientService>>()),
                logger,
                config["Marketplace:Dropship:BaseUrl"] ?? "https://api.example.com",
                config["Marketplace:Dropship:ApiKey"] ?? "dev-placeholder-api-key");
        });
        services.AddSingleton<ExternalListingSyncService>();
        services.AddSingleton<WebhookService>(provider =>
        {
            var logger = provider.GetRequiredService<ILogger<WebhookService>>();
            var config = provider.GetRequiredService<IConfiguration>();
            return new WebhookService(logger,
                config["Marketplace:Webhooks:Secret"] ?? "dev-placeholder-webhook-secret");
        });

        // Register security services
        services.AddSingleton<TokenService>(provider =>
        {
            var logger = provider.GetRequiredService<ILogger<TokenService>>();
            var config = provider.GetRequiredService<IConfiguration>();
            return new TokenService(logger,
                config["Marketplace:Security:TokenSecret"] ?? "dev-placeholder-token-secret");
        });
        services.AddSingleton<ApiKeyValidator>();
        services.AddSingleton<PermissionService>();

        // Register formatters
        services.AddSingleton<FormatterFactory>();

        return services;
    }

    /// <summary>
    /// Registers event handlers with the event bus.
    /// </summary>
    private static void RegisterEventHandlers(this IServiceCollection services)
    {
        services.AddSingleton<IEventHandler<ListingCreatedEvent>, ListingCreatedEventHandler>();
        services.AddSingleton<IEventHandler<MessageSentEvent>, MessageSentEventHandler>();
        services.AddSingleton<IEventHandler<ReportCreatedEvent>, ReportCreatedEventHandler>();
        services.AddSingleton<IEventHandler<UserCreatedEvent>, UserCreatedEventHandler>();
        services.AddSingleton<IEventHandler<UserEmailVerifiedEvent>, UserEmailVerifiedEventHandler>();
        services.AddSingleton<IEventHandler<RatingSubmittedEvent>, RatingSubmittedEventHandler>();
    }

    /// <summary>
    /// Subscribes the DI-registered <see cref="IEventHandler{TEvent}"/> implementations
    /// to the singleton <see cref="EventBus"/>. Must be called at startup; DI
    /// registration alone does not wire handlers to the bus.
    /// </summary>
    public static void SubscribeMarketplaceEventHandlers(this WebApplication app)
    {
        var bus = app.Services.GetRequiredService<EventBus>();
        SubscribeHandler<ListingCreatedEvent>(app.Services, bus);
        SubscribeHandler<MessageSentEvent>(app.Services, bus);
        SubscribeHandler<ReportCreatedEvent>(app.Services, bus);
        SubscribeHandler<UserCreatedEvent>(app.Services, bus);
        SubscribeHandler<UserEmailVerifiedEvent>(app.Services, bus);
        SubscribeHandler<RatingSubmittedEvent>(app.Services, bus);
    }

    private static void SubscribeHandler<TEvent>(IServiceProvider provider, EventBus bus)
        where TEvent : IEvent
    {
        var handler = provider.GetRequiredService<IEventHandler<TEvent>>();
        bus.Subscribe<TEvent>(handler.HandleAsync);
    }

    // Registers minimal-API endpoints.
    // NOTE: attribute-routed controllers already serve /api/v1/health, /listings,
    // /users/{id} and /categories; mapping the same paths here again would make
    // requests fail with AmbiguousMatchException at runtime, so only routes
    // WITHOUT a controller equivalent are mapped.
    public static void MapMarketplaceEndpoints(this WebApplication app)
    {
        var api = app.MapGroup("/api/v1")
            .WithName("Marketplace API");

        // User endpoints
        var users = api.MapGroup("/users");
        users.MapGet("/sellers/top", GetTopSellers)
            .WithName("Get Top Sellers (minimal API)");
    }

    // Get top sellers endpoint
    private static async Task<IResult> GetTopSellers(SearchService service)
    {
        var sellers = await service.GetTopSellersAsync(10);
        return Results.Ok(sellers);
    }

}
