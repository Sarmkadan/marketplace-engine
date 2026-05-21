#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Diagnostics;
using System.Text.RegularExpressions;
using MarketplaceEngine.Constants;
using MarketplaceEngine.Domain.Models;
using MarketplaceEngine.DTOs;
using MarketplaceEngine.Exceptions;
using MarketplaceEngine.Repositories;
using Microsoft.Extensions.Logging;

namespace MarketplaceEngine.Services;

/// <summary>
/// Provides full-text search over marketplace listings with TF-inspired relevance
/// scoring, per-field boosting, and aggregated facet computation.
/// </summary>
public class FullTextSearchService
{
    private readonly IListingRepository _listingRepository;
    private readonly ILogger<FullTextSearchService> _logger;

    // Relative weight applied to each field during scoring
    private const double TitleBoost = 4.0;
    private const double TagBoost = 2.5;
    private const double DescriptionBoost = 1.0;

    // Engagement signal multipliers applied on top of term score
    private const double FeaturedMultiplier = 1.5;
    private const double RecentMultiplier = 1.2;

    /// <summary>
    /// Initialises a new instance of <see cref="FullTextSearchService"/>.
    /// </summary>
    /// <param name="listingRepository">Listing data access layer.</param>
    /// <param name="logger">Logger for diagnostic output.</param>
    public FullTextSearchService(IListingRepository listingRepository, ILogger<FullTextSearchService> logger)
    {
        _listingRepository = listingRepository ?? throw new ArgumentNullException(nameof(listingRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Executes a full-text search and returns scored hits together with aggregated facets.
    /// </summary>
    /// <param name="request">Search parameters including query, filters, and pagination.</param>
    /// <param name="cancellationToken">Token to observe for cancellation.</param>
    /// <returns>A <see cref="FacetedSearchResult"/> with normalised scores and facet buckets.</returns>
    /// <exception cref="ValidationException">Thrown when the query fails length constraints.</exception>
    public async Task<FacetedSearchResult> SearchAsync(
        FullTextSearchRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ValidateQuery(request.Query);

        var sw = Stopwatch.StartNew();
        _logger.LogDebug("Full-text search started. Query={Query} Page={Page} PageSize={PageSize}",
            request.Query, request.Page, request.PageSize);

        var allActive = await _listingRepository.GetActiveListingsAsync().ConfigureAwait(false);
        cancellationToken.ThrowIfCancellationRequested();

        var terms = Tokenize(request.Query);

        var scored = allActive
            .Select(l => ScoreListing(l, terms))
            .Where(s => s.score > 0)
            .ToList();

        // Facets are built from the matched set before additional filters narrow results
        var facets = BuildFacets(scored.Select(s => s.listing).ToList());

        scored = ApplyFilters(scored, request);

        var totalHits = scored.Count;
        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, AppConstants.MinPageSize, AppConstants.MaxPageSize);
        var maxScore = scored.Count > 0 ? scored.Max(s => s.score) : 1.0;

        var hits = scored
            .OrderByDescending(s => s.score)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(s => ToScoredDto(s.listing, s.score, maxScore, s.matchedFields))
            .ToList();

        sw.Stop();
        _logger.LogInformation(
            "Full-text search completed. Query={Query} Hits={TotalHits} ElapsedMs={ElapsedMs}",
            request.Query, totalHits, sw.ElapsedMilliseconds);

        return new FacetedSearchResult
        {
            Query = request.Query,
            TotalHits = totalHits,
            Page = page,
            PageSize = pageSize,
            Hits = hits,
            Facets = facets,
            ElapsedMilliseconds = sw.ElapsedMilliseconds
        };
    }

    /// <summary>
    /// Returns autocomplete suggestions ordered by engagement signals.
    /// </summary>
    /// <param name="prefix">The partial query to complete.</param>
    /// <param name="limit">Maximum suggestions to return (clamped to 1–50).</param>
    /// <param name="cancellationToken">Token to observe for cancellation.</param>
    /// <returns>Distinct title strings that begin with <paramref name="prefix"/>.</returns>
    public async Task<IReadOnlyList<string>> GetSuggestionsAsync(
        string prefix,
        int limit = 10,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(prefix) || prefix.Length < AppConstants.SearchMinQueryLength)
            return [];

        limit = Math.Clamp(limit, 1, 50);

        var active = await _listingRepository.GetActiveListingsAsync().ConfigureAwait(false);
        cancellationToken.ThrowIfCancellationRequested();

        return active
            .Where(l => l.Title.Contains(prefix, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(l => l.IsFeatured)
            .ThenByDescending(l => l.ViewCount)
            .Select(l => l.Title)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Take(limit)
            .ToList();
    }

    // -------------------------------------------------------------------------
    // Scoring
    // -------------------------------------------------------------------------

    private static (Listing listing, double score, List<string> matchedFields) ScoreListing(
        Listing listing, IReadOnlyList<string> terms)
    {
        double score = 0;
        var matchedFields = new List<string>();

        var titleTokens = Tokenize(listing.Title);
        var descTokens = Tokenize(listing.Description);

        foreach (var term in terms)
        {
            var titleHits = titleTokens.Count(t => t.Equals(term, StringComparison.Ordinal));
            if (titleHits > 0)
            {
                score += TitleBoost * Tf(titleHits, titleTokens.Count);
                if (!matchedFields.Contains("title")) matchedFields.Add("title");
            }

            if (listing.Tags.Any(tag => tag.Equals(term, StringComparison.Ordinal)
                                     || tag.Contains(term, StringComparison.Ordinal)))
            {
                score += TagBoost;
                if (!matchedFields.Contains("tags")) matchedFields.Add("tags");
            }

            var descHits = descTokens.Count(t => t.Equals(term, StringComparison.Ordinal));
            if (descHits > 0)
            {
                score += DescriptionBoost * Tf(descHits, descTokens.Count);
                if (!matchedFields.Contains("description")) matchedFields.Add("description");
            }
        }

        if (score > 0)
        {
            if (listing.IsFeatured) score *= FeaturedMultiplier;
            if (listing.IsRecent()) score *= RecentMultiplier;
            if (listing.ViewCount > 0) score += Math.Log10(listing.ViewCount + 1) * 0.1;
            if (listing.InterestCount > 0) score += Math.Log10(listing.InterestCount + 1) * 0.05;
        }

        return (listing, score, matchedFields);
    }

    // Sub-linear TF dampening: score rises quickly at first, then flattens
    private static double Tf(int hits, int fieldLength)
    {
        var k = Math.Sqrt(Math.Max(fieldLength, 1));
        return hits / (hits + k);
    }

    // -------------------------------------------------------------------------
    // Filters
    // -------------------------------------------------------------------------

    private static List<(Listing listing, double score, List<string> matchedFields)> ApplyFilters(
        List<(Listing listing, double score, List<string> matchedFields)> scored,
        FullTextSearchRequest request)
    {
        if (request.CategoryId.HasValue && request.CategoryId.Value != Guid.Empty)
            scored = scored.Where(s => s.listing.CategoryId == request.CategoryId.Value).ToList();

        if (request.MinPrice.HasValue)
            scored = scored.Where(s => s.listing.Price?.Amount >= request.MinPrice.Value).ToList();

        if (request.MaxPrice.HasValue)
            scored = scored.Where(s => s.listing.Price?.Amount <= request.MaxPrice.Value).ToList();

        if (request.Tags is { Count: > 0 })
        {
            var normalized = request.Tags.Select(t => t.ToLowerInvariant()).ToHashSet();
            scored = scored.Where(s => s.listing.Tags.Any(t => normalized.Contains(t))).ToList();
        }

        if (!string.IsNullOrWhiteSpace(request.Condition))
            scored = scored.Where(s => string.Equals(
                s.listing.Condition, request.Condition, StringComparison.OrdinalIgnoreCase)).ToList();

        if (request.FeaturedOnly == true)
            scored = scored.Where(s => s.listing.IsFeatured).ToList();

        return scored;
    }

    // -------------------------------------------------------------------------
    // Facets
    // -------------------------------------------------------------------------

    private static List<Facet> BuildFacets(IReadOnlyList<Listing> listings) =>
    [
        BuildCategoryFacet(listings),
        BuildPriceRangeFacet(listings),
        BuildTagFacet(listings, maxBuckets: 10),
        BuildConditionFacet(listings)
    ];

    private static Facet BuildCategoryFacet(IReadOnlyList<Listing> listings)
    {
        var values = listings
            .GroupBy(l => l.CategoryId)
            .OrderByDescending(g => g.Count())
            .Take(20)
            .Select(g => new FacetValue
            {
                Label = g.Key.ToString(),
                Value = g.Key.ToString(),
                Count = g.Count()
            })
            .ToList();

        return new Facet { Name = "Category", Field = "categoryId", Values = values };
    }

    private static Facet BuildPriceRangeFacet(IReadOnlyList<Listing> listings)
    {
        (string label, decimal min, decimal max)[] ranges =
        [
            ("Under $25",     0m,    25m),
            ("$25 – $100",    25m,   100m),
            ("$100 – $500",   100m,  500m),
            ("$500 – $1,000", 500m,  1000m),
            ("Over $1,000",   1000m, decimal.MaxValue)
        ];

        var values = ranges
            .Select(r => new FacetValue
            {
                Label = r.label,
                Value = $"{r.min}-{(r.max == decimal.MaxValue ? string.Empty : r.max.ToString())}",
                Count = listings.Count(l => l.Price is not null
                                         && l.Price.Amount >= r.min
                                         && l.Price.Amount < r.max)
            })
            .Where(v => v.Count > 0)
            .ToList();

        return new Facet { Name = "Price Range", Field = "price", Values = values };
    }

    private static Facet BuildTagFacet(IReadOnlyList<Listing> listings, int maxBuckets)
    {
        var values = listings
            .SelectMany(l => l.Tags)
            .GroupBy(t => t)
            .OrderByDescending(g => g.Count())
            .Take(maxBuckets)
            .Select(g => new FacetValue { Label = g.Key, Value = g.Key, Count = g.Count() })
            .ToList();

        return new Facet { Name = "Tags", Field = "tags", Values = values };
    }

    private static Facet BuildConditionFacet(IReadOnlyList<Listing> listings)
    {
        var values = listings
            .Where(l => !string.IsNullOrWhiteSpace(l.Condition))
            .GroupBy(l => l.Condition!, StringComparer.OrdinalIgnoreCase)
            .OrderByDescending(g => g.Count())
            .Select(g => new FacetValue
            {
                Label = char.ToUpperInvariant(g.Key[0]) + g.Key[1..],
                Value = g.Key,
                Count = g.Count()
            })
            .ToList();

        return new Facet { Name = "Condition", Field = "condition", Values = values };
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private static IReadOnlyList<string> Tokenize(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return [];

        return Regex.Split(text.ToLowerInvariant(), @"[^a-z0-9]+")
                    .Where(t => t.Length > 1)
                    .ToList();
    }

    private static ScoredListingDto ToScoredDto(
        Listing listing, double rawScore, double maxScore, List<string> matchedFields) =>
        new(listing)
        {
            RelevanceScore = maxScore > 0 ? Math.Round(rawScore / maxScore, 4) : 0,
            MatchedFields = matchedFields.AsReadOnly()
        };

    private static void ValidateQuery(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            throw new ValidationException("Query", "Search query cannot be empty");

        if (query.Length < AppConstants.SearchMinQueryLength)
            throw new ValidationException("Query",
                $"Search query must be at least {AppConstants.SearchMinQueryLength} characters");

        if (query.Length > AppConstants.SearchMaxQueryLength)
            throw new ValidationException("Query",
                $"Search query cannot exceed {AppConstants.SearchMaxQueryLength} characters");
    }
}
