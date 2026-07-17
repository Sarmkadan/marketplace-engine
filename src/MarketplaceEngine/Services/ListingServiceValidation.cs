#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// ====================================================================

using MarketplaceEngine.Domain.Models;
using MarketplaceEngine.Domain.ValueObjects;

namespace MarketplaceEngine.Services;

/// <summary>
/// Provides validation helpers for <see cref="ListingService"/> instances.
/// </summary>
public static class ListingServiceValidation
{
    /// <summary>
    /// Validates a <see cref="ListingService"/> instance and returns a list of human-readable validation problems.
    /// </summary>
    /// <param name="value">The listing service instance to validate.</param>
    /// <returns>A read-only list of validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this ListingService? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        return Array.Empty<string>();
    }

    /// <summary>
    /// Determines whether the specified <see cref="ListingService"/> instance is valid.
    /// </summary>
    /// <param name="value">The listing service instance to check.</param>
    /// <returns>True if valid; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static bool IsValid(this ListingService? value)
    {
        return Validate(value).Count == 0;
    }

    /// <summary>
    /// Ensures that the specified <see cref="ListingService"/> instance is valid.
    /// </summary>
    /// <param name="value">The listing service instance to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="value"/> is not valid.</exception>
    public static void EnsureValid(this ListingService? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = Validate(value);
        if (problems.Count == 0)
        {
            return;
        }

        throw new ArgumentException(
            $"ListingService is not valid. Problems: {string.Join(", ", problems)}");
    }

    /// <summary>
    /// Validates listing creation parameters.
    /// </summary>
    /// <param name="sellerId">The seller ID.</param>
    /// <param name="title">The listing title.</param>
    /// <param name="description">The listing description.</param>
    /// <param name="price">The listing price.</param>
    /// <param name="currency">The currency code.</param>
    /// <param name="categoryId">The category ID.</param>
    /// <param name="imageUrls">List of image URLs.</param>
    /// <returns>A read-only list of validation problems; empty if valid.</returns>
    public static IReadOnlyList<string> ValidateForCreation(
        Guid sellerId,
        string title,
        string description,
        decimal price,
        string currency,
        Guid categoryId,
        IReadOnlyList<string> imageUrls)
    {
        var problems = new List<string>();

        if (sellerId == Guid.Empty)
        {
            problems.Add("Seller ID must be a valid GUID.");
        }

        if (string.IsNullOrWhiteSpace(title))
        {
            problems.Add("Title is required.");
        }
        else if (title.Length < 5)
        {
            problems.Add("Title must be at least 5 characters.");
        }
        else if (title.Length > 100)
        {
            problems.Add("Title cannot exceed 100 characters.");
        }

        if (string.IsNullOrWhiteSpace(description))
        {
            problems.Add("Description is required.");
        }
        else if (description.Length < 20)
        {
            problems.Add("Description must be at least 20 characters.");
        }
        else if (description.Length > 5000)
        {
            problems.Add("Description cannot exceed 5000 characters.");
        }

        if (price <= 0)
        {
            problems.Add("Price must be greater than zero.");
        }

        if (string.IsNullOrWhiteSpace(currency))
        {
            problems.Add("Currency is required.");
        }
        else if (currency.Length != 3)
        {
            problems.Add("Currency must be a 3-letter ISO code.");
        }

        if (categoryId == Guid.Empty)
        {
            problems.Add("Category is required.");
        }

        if (imageUrls is null || imageUrls.Count == 0)
        {
            problems.Add("At least one image is required.");
        }
        else if (imageUrls.Count > 10)
        {
            problems.Add("Cannot exceed 10 images per listing.");
        }
        else
        {
            for (var i = 0; i < imageUrls.Count; i++)
            {
                var imageUrl = imageUrls[i];
                if (string.IsNullOrWhiteSpace(imageUrl))
                {
                    problems.Add($"Image URL at index {i} is empty.");
                }
            }
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates listing update parameters.
    /// </summary>
    /// <param name="listingId">The listing ID.</param>
    /// <param name="requesterId">The ID of the user requesting the update.</param>
    /// <param name="title">Optional new title.</param>
    /// <param name="description">Optional new description.</param>
    /// <param name="price">Optional new price.</param>
    /// <param name="categoryId">Optional new category ID.</param>
    /// <returns>A read-only list of validation problems; empty if valid.</returns>
    public static IReadOnlyList<string> ValidateForUpdate(
        Guid listingId,
        Guid requesterId,
        string? title = null,
        string? description = null,
        Money? price = null,
        Guid? categoryId = null)
    {
        var problems = new List<string>();

        if (listingId == Guid.Empty)
        {
            problems.Add("Listing ID must be a valid GUID.");
        }

        if (requesterId == Guid.Empty)
        {
            problems.Add("Requester ID must be a valid GUID.");
        }

        if (title is not null)
        {
            if (title.Length < 5)
            {
                problems.Add("Title must be at least 5 characters.");
            }
            else if (title.Length > 100)
            {
                problems.Add("Title cannot exceed 100 characters.");
            }
        }

        if (description is not null)
        {
            if (description.Length < 20)
            {
                problems.Add("Description must be at least 20 characters.");
            }
            else if (description.Length > 5000)
            {
                problems.Add("Description cannot exceed 5000 characters.");
            }
        }

        if (price is not null && price.Amount <= 0)
        {
            problems.Add("Price must be greater than zero.");
        }

        if (categoryId.HasValue && categoryId.Value == Guid.Empty)
        {
            problems.Add("Category ID must be a valid GUID.");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates listing visibility change parameters.
    /// </summary>
    /// <param name="listingId">The listing ID.</param>
    /// <param name="requesterId">The ID of the user requesting the change.</param>
    /// <param name="isVisible">Whether the listing should be visible (published).</param>
    /// <returns>A read-only list of validation problems; empty if valid.</returns>
    public static IReadOnlyList<string> ValidateForVisibilityChange(
        Guid listingId,
        Guid requesterId,
        bool isVisible)
    {
        var problems = new List<string>();

        if (listingId == Guid.Empty)
        {
            problems.Add("Listing ID must be a valid GUID.");
        }

        if (requesterId == Guid.Empty)
        {
            problems.Add("Requester ID must be a valid GUID.");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates listing delist parameters.
    /// </summary>
    /// <param name="listingId">The listing ID.</param>
    /// <param name="requesterId">The ID of the user requesting the delist.</param>
    /// <returns>A read-only list of validation problems; empty if valid.</returns>
    public static IReadOnlyList<string> ValidateForDelist(
        Guid listingId,
        Guid requesterId)
    {
        var problems = new List<string>();

        if (listingId == Guid.Empty)
        {
            problems.Add("Listing ID must be a valid GUID.");
        }

        if (requesterId == Guid.Empty)
        {
            problems.Add("Requester ID must be a valid GUID.");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates listing featured status change parameters.
    /// </summary>
    /// <param name="listingId">The listing ID.</param>
    /// <param name="adminId">The ID of the administrator.</param>
    /// <returns>A read-only list of validation problems; empty if valid.</returns>
    public static IReadOnlyList<string> ValidateForFeaturedChange(
        Guid listingId,
        Guid adminId)
    {
        var problems = new List<string>();

        if (listingId == Guid.Empty)
        {
            problems.Add("Listing ID must be a valid GUID.");
        }

        if (adminId == Guid.Empty)
        {
            problems.Add("Administrator ID must be a valid GUID.");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates pagination parameters for listing queries.
    /// </summary>
    /// <param name="pageNumber">The page number (1-based).</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <returns>A read-only list of validation problems; empty if valid.</returns>
    public static IReadOnlyList<string> ValidatePaginationParameters(
        int pageNumber,
        int pageSize)
    {
        var problems = new List<string>();

        if (pageNumber < 1)
        {
            problems.Add("Page number must be at least 1.");
        }

        if (pageSize < 1)
        {
            problems.Add("Page size must be at least 1.");
        }
        else if (pageSize > 100)
        {
            problems.Add("Page size cannot exceed 100.");
        }

        return problems.AsReadOnly();
    }
}