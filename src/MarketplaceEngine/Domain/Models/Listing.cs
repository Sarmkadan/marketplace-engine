// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using MarketplaceEngine.Domain.Enums;
using MarketplaceEngine.Domain.ValueObjects;

namespace MarketplaceEngine.Domain.Models;

/// <summary>
/// Represents a marketplace listing for a product or service.
/// </summary>
public class Listing
{
    public Guid Id { get; set; }
    public Guid SellerId { get; set; }
    public User? Seller { get; set; }
    public Guid CategoryId { get; set; }
    public Category? Category { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Money? Price { get; set; }
    public ListingStatus Status { get; set; } = ListingStatus.Active;
    public Location? Location { get; set; }
    public List<string> ImageUrls { get; set; } = [];
    public List<string> Tags { get; set; } = [];
    public int ViewCount { get; set; }
    public int InterestCount { get; set; }
    public Rating? Rating { get; set; }
    public bool IsFeatured { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public DateTime? PublishedAt { get; set; }
    public DateTime? DueDate { get; set; }
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

        if (Price == null || Price.Amount <= 0)
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
