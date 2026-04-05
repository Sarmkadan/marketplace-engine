#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using MarketplaceEngine.Domain.Models;

namespace MarketplaceEngine.DTOs;

/// <summary>
/// Represents a full-text search request with optional filters and pagination.
/// </summary>
public class FullTextSearchRequest
{
    /// <summary>Gets or sets the search query string.</summary>
    public string Query { get; set; } = string.Empty;

    /// <summary>Gets or sets an optional category filter.</summary>
    public Guid? CategoryId { get; set; }

    /// <summary>Gets or sets the minimum price filter.</summary>
    public decimal? MinPrice { get; set; }

    /// <summary>Gets or sets the maximum price filter.</summary>
    public decimal? MaxPrice { get; set; }

    /// <summary>Gets or sets optional tag filters. Results must match at least one tag.</summary>
    public List<string>? Tags { get; set; }

    /// <summary>Gets or sets an optional condition filter (e.g. "new", "used").</summary>
    public string? Condition { get; set; }

    /// <summary>Gets or sets whether to restrict results to featured listings only.</summary>
    public bool? FeaturedOnly { get; set; }

    /// <summary>Gets or sets the page number (1-based). Defaults to 1.</summary>
    public int Page { get; set; } = 1;

    /// <summary>Gets or sets the number of results per page. Defaults to 20.</summary>
    public int PageSize { get; set; } = 20;
}

/// <summary>
/// Represents a single bucket within a facet (e.g., "Electronics" with a count of 42).
/// </summary>
public class FacetValue
{
    /// <summary>Gets or sets the human-readable label for this bucket.</summary>
    public string Label { get; set; } = string.Empty;

    /// <summary>Gets or sets the filter value to send when this bucket is selected.</summary>
    public string Value { get; set; } = string.Empty;

    /// <summary>Gets or sets the number of matching listings in this bucket.</summary>
    public int Count { get; set; }
}

/// <summary>
/// Represents a named facet dimension containing a set of value buckets.
/// </summary>
public class Facet
{
    /// <summary>Gets or sets the display name of this facet dimension.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets the field identifier used for client-side filter construction.</summary>
    public string Field { get; set; } = string.Empty;

    /// <summary>Gets or sets the individual value buckets for this facet.</summary>
    public List<FacetValue> Values { get; set; } = [];
}

/// <summary>
/// Extends <see cref="ListingDto"/> with a relevance score and matched-field indicators
/// produced by the full-text search engine.
/// </summary>
public class ScoredListingDto : ListingDto
{
    /// <summary>Gets or sets the normalised relevance score in the range [0, 1].</summary>
    public double RelevanceScore { get; set; }

    /// <summary>Gets or sets the names of the fields that contributed to the score.</summary>
    public IReadOnlyList<string> MatchedFields { get; set; } = [];

    /// <summary>Initialises a <see cref="ScoredListingDto"/> from a domain <see cref="Listing"/>.</summary>
    /// <param name="listing">The source domain entity.</param>
    public ScoredListingDto(Listing listing) : base(listing) { }

    /// <summary>Parameterless constructor for serialisation.</summary>
    public ScoredListingDto() { }
}

/// <summary>
/// The complete response returned by the full-text search engine, including scored hits,
/// aggregated facets, and pagination metadata.
/// </summary>
public class FacetedSearchResult
{
    /// <summary>Gets or sets the original query string.</summary>
    public string Query { get; set; } = string.Empty;

    /// <summary>Gets or sets the total number of matching listings before pagination.</summary>
    public int TotalHits { get; set; }

    /// <summary>Gets or sets the current page number.</summary>
    public int Page { get; set; }

    /// <summary>Gets or sets the page size used for this result.</summary>
    public int PageSize { get; set; }

    /// <summary>Gets the total number of pages.</summary>
    public int TotalPages => PageSize > 0 ? (TotalHits + PageSize - 1) / PageSize : 0;

    /// <summary>Gets whether a next page exists.</summary>
    public bool HasNextPage => Page < TotalPages;

    /// <summary>Gets whether a previous page exists.</summary>
    public bool HasPreviousPage => Page > 1;

    /// <summary>Gets or sets the scored and paginated listing hits.</summary>
    public List<ScoredListingDto> Hits { get; set; } = [];

    /// <summary>Gets or sets the aggregated facets computed from the full matching set.</summary>
    public List<Facet> Facets { get; set; } = [];

    /// <summary>Gets or sets the wall-clock time the search took in milliseconds.</summary>
    public long ElapsedMilliseconds { get; set; }
}
