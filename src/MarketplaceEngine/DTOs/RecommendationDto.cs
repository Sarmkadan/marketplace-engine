#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using MarketplaceEngine.Domain.Models;
using MarketplaceEngine.Recommendations;

namespace MarketplaceEngine.DTOs;

/// <summary>
/// API response DTO representing a single recommendation entry.
/// Combines listing metadata with relevance scoring and consumer-facing explainability.
/// </summary>
public class RecommendationDto
{
    /// <summary>Unique identifier of the recommended listing.</summary>
    public Guid ListingId { get; set; }

    /// <summary>Display title of the listing.</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>Listed price amount.</summary>
    public decimal Price { get; set; }

    /// <summary>ISO 4217 currency code.</summary>
    public string Currency { get; set; } = "USD";

    /// <summary>URL of the listing's primary thumbnail image; <c>null</c> when no images are present.</summary>
    public string? ThumbnailUrl { get; set; }

    /// <summary>Normalised relevance score in the range [0, 1]; higher values indicate greater relevance.</summary>
    public double Score { get; set; }

    /// <summary>Human-readable explanation of why this item was recommended.</summary>
    public string Reason { get; set; } = string.Empty;

    /// <summary>Identifier of the category this listing belongs to.</summary>
    public Guid CategoryId { get; set; }

    /// <summary>Identifier of the seller who owns this listing.</summary>
    public Guid SellerId { get; set; }

    /// <summary>Parameterless constructor required for JSON deserialization.</summary>
    public RecommendationDto() { }

    /// <summary>
    /// Constructs a response DTO from the domain listing model and its engine-produced score.
    /// </summary>
    /// <param name="listing">The hydrated listing domain entity.</param>
    /// <param name="scored">The score and rationale produced by the recommendation engine.</param>
    public RecommendationDto(Listing listing, ScoredListing scored)
    {
        ListingId = listing.Id;
        Title = listing.Title;
        Price = listing.Price?.Amount ?? 0m;
        Currency = listing.Price?.CurrencyCode ?? "USD";
        ThumbnailUrl = listing.ImageUrls.FirstOrDefault();
        Score = Math.Round(scored.Score, 4);
        Reason = scored.Reason.ToDisplayString();
        CategoryId = listing.CategoryId;
        SellerId = listing.SellerId;
    }
}

/// <summary>
/// Request parameters for retrieving a recommendation feed from the API.
/// </summary>
public class RecommendationRequest
{
    /// <summary>
    /// The user requesting recommendations.
    /// When <c>null</c>, the engine returns a non-personalised trending feed.
    /// </summary>
    public Guid? UserId { get; set; }

    /// <summary>
    /// Optional anchor listing for item-based similarity recommendations.
    /// When provided alongside <see cref="Strategy"/> set to <see cref="RecommendationStrategy.ItemBased"/>,
    /// the engine returns items similar to this listing.
    /// </summary>
    public Guid? BasedOnListingId { get; set; }

    /// <summary>Maximum number of recommendations to return. Clamped server-side to [1, 100].</summary>
    public int Count { get; set; } = 20;

    /// <summary>
    /// Algorithm strategy override. When <c>null</c>, the engine selects the most
    /// appropriate strategy based on available personalisation data.
    /// </summary>
    public RecommendationStrategy? Strategy { get; set; }
}

/// <summary>
/// Response envelope for recommendation feeds returned by the API.
/// Contains ordered results alongside metadata about the generation strategy.
/// </summary>
public class RecommendationFeedDto
{
    /// <summary>Ordered list of recommended listings, ranked by descending relevance score.</summary>
    public List<RecommendationDto> Items { get; set; } = [];

    /// <summary>Number of items contained in this response.</summary>
    public int Count => Items.Count;

    /// <summary>
    /// Indicates whether user-specific personalisation was applied to this feed.
    /// <c>false</c> means the feed is trending or editorial.
    /// </summary>
    public bool IsPersonalised { get; set; }

    /// <summary>String identifier of the algorithm strategy that generated this feed.</summary>
    public string Strategy { get; set; } = string.Empty;

    /// <summary>UTC timestamp at which this feed was generated.</summary>
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Request body for recording a user activity signal via the tracking endpoint.
/// </summary>
public class TrackActivityRequest
{
    /// <summary>Identifier of the user who performed the action.</summary>
    public required Guid UserId { get; set; }

    /// <summary>Identifier of the listing the action was performed on.</summary>
    public required Guid ListingId { get; set; }

    /// <summary>
    /// String name of the interaction type.
    /// Accepted values (case-insensitive): <c>View</c>, <c>Save</c>, <c>Enquiry</c>, <c>Purchase</c>.
    /// </summary>
    public required string SignalType { get; set; }
}

/// <summary>
/// Selects the recommendation algorithm applied to a given request.
/// </summary>
public enum RecommendationStrategy
{
    /// <summary>Engine selects the best strategy automatically based on available personalisation data.</summary>
    Auto,

    /// <summary>User-based collaborative filtering via cosine similarity on interaction vectors.</summary>
    CollaborativeFiltering,

    /// <summary>Item-to-item co-interaction similarity derived from audience overlap.</summary>
    ItemBased,

    /// <summary>Popularity-based trending feed derived from recent signal velocity.</summary>
    Trending
}

/// <summary>
/// Presentation helpers for converting <see cref="RecommendationReason"/> values
/// into consumer-facing display strings.
/// </summary>
internal static class RecommendationReasonExtensions
{
    /// <summary>
    /// Returns a localisation-ready, consumer-facing description of the recommendation rationale.
    /// </summary>
    internal static string ToDisplayString(this RecommendationReason reason) => reason switch
    {
        RecommendationReason.CollaborativeFiltering => "Users with similar taste enjoyed this",
        RecommendationReason.ItemSimilarity        => "Similar to items you've viewed",
        RecommendationReason.Trending              => "Trending in the marketplace",
        RecommendationReason.CategoryAffinity      => "Matches your preferred categories",
        RecommendationReason.Editorial             => "Handpicked for you",
        _                                          => "Recommended for you"
    };
}
