// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using MarketplaceEngine.DTOs;
using MarketplaceEngine.Exceptions;
using MarketplaceEngine.Services;
using Microsoft.Extensions.DependencyInjection;

namespace MarketplaceEngine.Configuration;

/// <summary>
/// Extension methods for registering the full-text search feature with the DI container
/// and mapping its HTTP endpoints onto a <see cref="WebApplication"/>.
/// </summary>
public static class FullTextSearchExtensions
{
    /// <summary>
    /// Registers <see cref="FullTextSearchService"/> as a singleton.
    /// Intended to be called inside <c>AddMarketplaceServices</c> or directly from
    /// <c>Program.cs</c>.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <returns>The same <see cref="IServiceCollection"/> for chaining.</returns>
    public static IServiceCollection AddFullTextSearch(this IServiceCollection services)
    {
        services.AddSingleton<FullTextSearchService>();
        return services;
    }

    /// <summary>
    /// Maps <c>GET /api/v1/search/full-text</c> and <c>GET /api/v1/search/suggestions</c>
    /// endpoints onto the supplied <see cref="WebApplication"/>.
    /// </summary>
    /// <param name="app">The web application to extend.</param>
    public static void MapFullTextSearchEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/v1/search")
            .WithTags("Search");

        group.MapGet("/full-text", FullTextSearch)
            .WithName("Full-Text Search")
            .WithSummary("Search listings with relevance scoring and facets");

        group.MapGet("/suggestions", GetSuggestions)
            .WithName("Search Suggestions")
            .WithSummary("Autocomplete suggestions for the search input");
    }

    // GET /api/v1/search/full-text?q=...&categoryId=...&minPrice=...&maxPrice=...
    private static async Task<IResult> FullTextSearch(
        FullTextSearchService service,
        string q,
        Guid? categoryId = null,
        decimal? minPrice = null,
        decimal? maxPrice = null,
        string? condition = null,
        bool? featuredOnly = null,
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new FullTextSearchRequest
            {
                Query = q,
                CategoryId = categoryId,
                MinPrice = minPrice,
                MaxPrice = maxPrice,
                Condition = condition,
                FeaturedOnly = featuredOnly,
                Page = page,
                PageSize = pageSize
            };

            var result = await service.SearchAsync(request, cancellationToken);
            return Results.Ok(result);
        }
        catch (ValidationException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }

    // GET /api/v1/search/suggestions?prefix=...&limit=10
    private static async Task<IResult> GetSuggestions(
        FullTextSearchService service,
        string prefix,
        int limit = 10,
        CancellationToken cancellationToken = default)
    {
        var suggestions = await service.GetSuggestionsAsync(prefix, limit, cancellationToken);
        return Results.Ok(new { prefix, suggestions });
    }
}
