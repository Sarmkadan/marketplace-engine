#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using MarketplaceEngine.Domain.Enums;
using MarketplaceEngine.Domain.ValueObjects;

namespace MarketplaceEngine.Domain.Models;

/// <summary>
/// Represents a marketplace listing for a product or service. Contains pricing,
/// categorization, seller reference, media, and engagement metrics.
/// Must pass <see cref="ValidateForPublishing"/> before becoming visible.
/// </summary>
public class Listing
{
    /// <summary>Unique listing identifier.</summary>
    public Guid Id { get; set; }
    /// <summary>ID of the user who created this listing.</summary>
    public Guid SellerId { get; set; }
    /// <summary>Navigation property for the seller. Null when not loaded.</summary>
    public User? Seller { get; set; }
    /// <summary>ID of the category this listing belongs to.</summary>
    public Guid CategoryId { get; set; }
    /// <summary>Navigation property for the category. Null when not loaded.</summary>
    public Category? Category { get; set; }
    /// <summary>Listing title (5-100 characters required for publishing).</summary>
    public string Title { get; set; } = string.Empty;
    /// <summary>Detailed description (20-5000 characters required for publishing).</summary>
    public string Description { get; set; } = string.Empty;
    /// <summary>Price with currency. Must be greater than zero for publishing.</summary>
    public Money? Price { get; set; }
    /// <summary>Current lifecycle status (Draft, Active, Sold, Expired, etc.).</summary>
    public ListingStatus Status { get; set; } = ListingStatus.Active;
    /// <summary>Geographic location of the item, if applicable.</summary>
    public Location? Location { get; set; }
    /// <summary>URLs of listing images. At least one required for publishing.</summary>
    public List<string> ImageUrls { get; set; } = [];
    /// <summary>Searchable tags for discovery. Maximum 10 allowed.</summary>
    public List<string> Tags { get; set; } = [];
    /// <summary>Number of times this listing has been viewed.</summary>
    public int ViewCount { get; set; }
    /// <summary>Number of users who expressed interest (saved/favorited).</summary>
    public int InterestCount { get; set; }
    /// <summary>Aggregate rating from buyers, or null if not yet rated.</summary>
    public Rating? Rating { get; set; }
    /// <summary>Whether this listing is promoted/featured in search results.</summary>
    public bool IsFeatured { get; set; }
    /// <summary>UTC timestamp when the listing was created.</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    /// <summary>UTC timestamp of the last modification, or null if never updated.</summary>
    public DateTime? UpdatedAt { get; set; }
    /// <summary>UTC timestamp when the listing was published, or null if still draft.</summary>
    public DateTime? PublishedAt { get; set; }
    /// <summary>Expiration date for time-limited listings, or null for no expiry.</summary>
    public DateTime? DueDate { get; set; }
    /// <summary>Item condition (e.g., "New", "Like New", "Used").</summary>
    public string? Condition { get; set; }

    // Validates listing content before publishing
    public void ValidateForPublishing()
    {
        if (string.IsNullOrWhiteSpace(Title) || Title.Length < 5)
            throw new ArgumentException("Title must be at least 5 characters", nameof(Title));

        if (Title.Length > 100)
            throw new ArgumentException("Title cannot exceed 100 characters", nameof(Title));

        if (string.IsNullOrWhiteSpace(Description) || Description.Length < 20)
            throw new ArgumentException("Description must be at least 20 characters", nameof(Description));

        if (Description.Length > 5000)
            throw new ArgumentException("Description cannot exceed 5000 characters", nameof(Description));

        if (Price is null || Price.Amount <= 0)
            throw new ArgumentException("Price must be greater than zero", nameof(Price));

        if (CategoryId == Guid.Empty)
            throw new ArgumentException("Category is required", nameof(CategoryId));

        if (ImageUrls.Count == 0)
            throw new ArgumentException("At least one image is required", nameof(ImageUrls));

        if (ImageUrls.Count > 10)
            throw new ArgumentException("Cannot exceed 10 images per listing", nameof(ImageUrls));
    }

    // Publishes the listing to the marketplace
    public void Publish()
    {
        ValidateForPublishing();
        Status = ListingStatus.Active;
        PublishedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    // Unpublishes the listing
    public void Unpublish()
    {
        Status = ListingStatus.Inactive;
        UpdatedAt = DateTime.UtcNow;
    }

    // Records a view
    public void RecordView()
    {
        ViewCount++;
        UpdatedAt = DateTime.UtcNow;
    }

    // Records user interest
    public void RecordInterest()
    {
        InterestCount++;
        UpdatedAt = DateTime.UtcNow;
    }

    // Flags listing for moderation review
    public void FlagForReview(string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("Reason for flagging is required", nameof(reason));

        Status = ListingStatus.UnderReview;
        UpdatedAt = DateTime.UtcNow;
    }

    // Marks listing as flagged
    public void Flag()
    {
        Status = ListingStatus.Flagged;
        UpdatedAt = DateTime.UtcNow;
    }

    // Marks listing as sold/delisted
    public void Delist()
    {
        Status = ListingStatus.Delisted;
        UpdatedAt = DateTime.UtcNow;
    }

    // Archives the listing after completion
    public void Archive()
    {
        Status = ListingStatus.Archived;
        UpdatedAt = DateTime.UtcNow;
    }

    // Sets listing as featured
    public void MarkAsFeatured()
    {
        IsFeatured = true;
        UpdatedAt = DateTime.UtcNow;
    }

    // Removes featured status
    public void RemoveFeaturedStatus()
    {
        IsFeatured = false;
        UpdatedAt = DateTime.UtcNow;
    }

    // Adds tag to the listing
    public void AddTag(string tag)
    {
        if (string.IsNullOrWhiteSpace(tag))
            return;

        var normalized = tag.Trim().ToLowerInvariant();
        if (!Tags.Contains(normalized) && Tags.Count < 10)
        {
            Tags.Add(normalized);
            UpdatedAt = DateTime.UtcNow;
        }
    }

    // Removes tag from listing
    public bool RemoveTag(string tag)
    {
        var normalized = tag.Trim().ToLowerInvariant();
        var removed = Tags.Remove(normalized);
        if (removed)
            UpdatedAt = DateTime.UtcNow;

        return removed;
    }

    // Adds image URL to listing
    public void AddImage(string imageUrl)
    {
        if (string.IsNullOrWhiteSpace(imageUrl))
            return;

        if (ImageUrls.Count >= 10)
            throw new InvalidOperationException("Cannot exceed 10 images per listing");

        if (!ImageUrls.Contains(imageUrl))
        {
            ImageUrls.Add(imageUrl);
            UpdatedAt = DateTime.UtcNow;
        }
    }

    // Removes image from listing
    public bool RemoveImage(string imageUrl)
    {
        if (ImageUrls.Count <= 1)
            throw new InvalidOperationException("Listing must have at least one image");

        var removed = ImageUrls.Remove(imageUrl);
        if (removed)
            UpdatedAt = DateTime.UtcNow;

        return removed;
    }

    // Gets primary image URL
    public string? GetPrimaryImageUrl() => ImageUrls.FirstOrDefault();

    // Checks if listing is recent (published within last 7 days)
    public bool IsRecent() => PublishedAt.HasValue && (DateTime.UtcNow - PublishedAt.Value).TotalDays <= 7;
}
