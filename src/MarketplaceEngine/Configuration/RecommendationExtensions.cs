#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using MarketplaceEngine.DTOs;
using MarketplaceEngine.Exceptions;
using MarketplaceEngine.Infrastructure.Events;
using MarketplaceEngine.Recommendations;
using MarketplaceEngine.Services;

namespace MarketplaceEngine.Configuration;

/// <summary>
/// Extension methods for registering the recommendation engine in the DI container
/// and wiring its HTTP endpoints into the ASP.NET Core routing pipeline.
/// </summary>
public static class RecommendationExtensions
{
    /// <summary>
    /// Registers the collaborative filtering recommendation engine and all associated services,
    /// including the activity tracker, configuration options, and event handler.
    /// </summary>
    /// <remarks>
    /// Must be called after <see cref="DependencyInjection.AddMarketplaceServices"/> so that
    /// shared repositories, the cache service, and the event bus are already available in the container.
    /// </remarks>
    /// <param name="services">The service collection to configure.</param>
    /// <param name="configuration">
    /// Application configuration used to bind <see cref="RecommendationOptions"/> from
    /// the <c>Marketplace:Recommendations</c> section. Pass <c>null</c> to use defaults.
    /// </param>
    public static IServiceCollection AddRecommendationEngine(
        this IServiceCollection services,
        IConfiguration? configuration = null)
    {
        var options = configuration is not null
            ? RecommendationOptions.FromConfiguration(configuration)
            : RecommendationOptions.CreateDefault();

        services.AddSingleton(options);
        services.AddSingleton<IUserActivityTracker, UserActivityTracker>();
        services.AddSingleton<IRecommendationEngine, CollaborativeFilteringEngine>();
        services.AddSingleton<RecommendationService>();
        services.AddSingleton<IEventHandler<UserActivityRecordedEvent>, UserActivityRecordedEventHandler>();

        return services;
    }

    /// <summary>
    /// Maps recommendation API endpoints under the <c>/api/v1/recommendations</c> route group.
    /// </summary>
    /// <remarks>
    /// Call this alongside <see cref="DependencyInjection.MapMarketplaceEndpoints"/> in
    /// the application startup pipeline after <see cref="AddRecommendationEngine"/> has
    /// been called during service registration. For a richer controller-based surface
    /// (v2), see <c>RecommendationsController</c>.
    /// </remarks>
    /// <param name="app">The web application to configure.</param>
    public static void MapRecommendationEndpoints(this WebApplication app)
    {
        var api = app.MapGroup("/api/v1/recommendations")
            .WithName("Recommendations API")
            .WithTags("Recommendations");

        api.MapGet("/trending", GetTrending)
            .WithName("Get Trending Listings")
            .WithSummary("Returns globally trending listings based on recent marketplace signal velocity.");

        api.MapGet("/users/{userId}", GetForUser)
            .WithName("Get User Recommendations")
            .WithSummary("Returns a personalised recommendation feed for the specified user.");

        api.MapGet("/users/{userId}/affinity", GetByAffinity)
            .WithName("Get Affinity Recommendations")
            .WithSummary("Returns category-affinity recommendations derived from the user's interaction history.");

        api.MapGet("/listings/{listingId}/similar", GetSimilar)
            .WithName("Get Similar Listings")
            .WithSummary("Returns listings similar to the specified item via co-interaction analysis.");

        api.MapPost("/track", TrackActivity)
            .WithName("Track User Activity")
            .WithSummary("Records a behavioural signal to improve future personalised recommendations.");
    }

    // ── Endpoint handlers ────────────────────────────────────────────────────

    private static async Task<IResult> GetTrending(RecommendationService service, int count = 20)
    {
        var feed = await service.GetTrendingListingsAsync(count).ConfigureAwait(false);
        return Results.Ok(feed);
    }

    private static async Task<IResult> GetForUser(
        Guid userId, RecommendationService service, int count = 20)
    {
        try
        {
            var feed = await service.GetRecommendationsForUserAsync(userId, count).ConfigureAwait(false);
            return Results.Ok(feed);
        }
        catch (ResourceNotFoundException)
        {
            return Results.NotFound();
        }
    }

    private static async Task<IResult> GetByAffinity(
        Guid userId, RecommendationService service, int count = 20)
    {
        try
        {
            var feed = await service.GetAffinityRecommendationsAsync(userId, count).ConfigureAwait(false);
            return Results.Ok(feed);
        }
        catch (ResourceNotFoundException)
        {
            return Results.NotFound();
        }
    }

    private static async Task<IResult> GetSimilar(
        Guid listingId, RecommendationService service, int count = 10)
    {
        try
        {
            var feed = await service.GetSimilarListingsAsync(listingId, count).ConfigureAwait(false);
            return Results.Ok(feed);
        }
        catch (ResourceNotFoundException)
        {
            return Results.NotFound();
        }
    }

    private static async Task<IResult> TrackActivity(
        TrackActivityRequest request, RecommendationService service)
    {
        if (!Enum.TryParse<SignalType>(request.SignalType, ignoreCase: true, out var signalType))
        {
            return Results.BadRequest(new
            {
                error = $"Unknown signal type '{request.SignalType}'.",
                validValues = Enum.GetNames<SignalType>()
            });
        }

        await service.TrackUserActivityAsync(request.UserId, request.ListingId, signalType).ConfigureAwait(false);
        return Results.NoContent();
    }
}
