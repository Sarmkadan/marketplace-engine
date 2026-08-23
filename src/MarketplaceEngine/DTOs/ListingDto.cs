#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using MarketplaceEngine.Domain.Models;

namespace MarketplaceEngine.DTOs;

/// <summary>
/// Data Transfer Object for Listing entities.
/// Transforms internal domain models into JSON-serializable API responses.
/// Hides internal implementation details from API consumers.
/// </summary>
public class ListingDto
{
    /// <summary>
    /// Unique identifier for the listing.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Title of the listing.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Detailed description of the listing.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Price of the listing item.
    /// </summary>
    public decimal Price { get; set; }

    /// <summary>
    /// Identifier of the seller who owns the listing.
    /// </>
    public Guid SellerId { get; set; }

    /// <summary>
    /// Name of the seller.
    /// </summary>
    public string SellerName { get; set; } = string.Empty;

    /// <summary>
    /// Identifier of the category this listing belongs to.
    /// </summary>
    public Guid CategoryId { get; set; }

    /// <summary>
    /// Current status of the listing (e.g., Active, Inactive, Sold).
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Number of times the listing has been viewed.
    /// </summary>
    public int ViewCount { get; set; }

    /// <summary>
    /// Date and time when the listing was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Date and time when the listing was last updated (nullable).
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Date and time when the listing was published (nullable).
    /// </summary>
    public DateTime? PublishedAt { get; set; }

    /// <summary>
    /// Initializes a new instance of the ListingDto class.
    /// </summary>
    public ListingDto() { }

    /// <summary>
    /// Initializes a new instance of the ListingDto class from a Listing domain model.
    /// </summary>
    /// <param name="listing">The Listing domain model to convert from.</param>
    public ListingDto(Listing listing)
    {
        Id = listing.Id;
        Title = listing.Title;
        Description = listing.Description;
        Price = listing.Price?.Amount ?? 0;
        SellerId = listing.SellerId;
        SellerName = string.Empty; // Hotfix: Listing model does not have SellerName
        CategoryId = listing.CategoryId;
        Status = listing.Status.ToString();
        ViewCount = listing.ViewCount;
        CreatedAt = listing.CreatedAt;
        UpdatedAt = listing.UpdatedAt;
        PublishedAt = listing.PublishedAt;
    }
}

/// <summary>
/// Request DTO for updating an existing listing.
/// All fields are optional; only provided fields are changed.
/// </summary>
public class UpdateListingRequest
{
    public Guid SellerId { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public decimal? Price { get; set; }
    public Guid? CategoryId { get; set; }
}

/// <summary>
/// Request DTO for creating new listings.
/// </summary>
public class CreateListingRequest
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public Guid SellerId { get; set; }
    public Guid CategoryId { get; set; }
    public List<string>? ImageUrls { get; set; } // Hotfix: Add ImageUrls property
}

/// <summary>
/// Cursor-based pagination wrapper for list responses.
/// Use <see cref="NextCursor"/> as the <c>after</c> parameter on the next request.
/// </summary>
public class CursorPaginatedResponse<T>
{
    public List<T> Items { get; set; } = new();
    public Guid? NextCursor { get; set; }
    public int PageSize { get; set; }
    public bool HasMore { get; set; }
}

/// <summary>
/// Pagination wrapper for list responses.
/// </summary>
public class PaginatedResponse<T>
{
    public List<T> Items { get; set; } = new();
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int Total { get; set; }

    public int TotalPages => (Total + PageSize - 1) / PageSize;
    public bool HasNextPage => Page < TotalPages;
    public bool HasPreviousPage => Page > 1;
}

/// <summary>
/// Search result response for full-text search operations.
/// </summary>
public class SearchResultDto
{
    public string Query { get; set; } = string.Empty;
    public List<ListingDto> Results { get; set; } = new();
}
