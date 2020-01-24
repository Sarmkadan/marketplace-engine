#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Globalization;
using MarketplaceEngine.Domain.Enums;
using MarketplaceEngine.Domain.ValueObjects;

namespace MarketplaceEngine.Domain.Models;

/// <summary>
/// Provides validation helpers for <see cref="Listing"/> instances.
/// </summary>
public static class ListingValidation
{
    /// <summary>
    /// Validates a listing and returns a list of human-readable validation problems.
    /// </summary>
    /// <param name="value">The listing to validate.</param>
    /// <returns>A read-only list of validation problems, or empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this Listing value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Validate required properties
        if (value.Id == Guid.Empty)
            problems.Add("Listing Id cannot be empty");

        if (value.SellerId == Guid.Empty)
            problems.Add("Seller Id cannot be empty");

        if (string.IsNullOrWhiteSpace(value.Title))
            problems.Add("Title cannot be null or empty");
        else
        {
            if (value.Title.Length < 5)
                problems.Add("Title must be at least 5 characters");

            if (value.Title.Length > 100)
                problems.Add("Title cannot exceed 100 characters");
        }

        if (string.IsNullOrWhiteSpace(value.Description))
            problems.Add("Description cannot be null or empty");
        else
        {
            if (value.Description.Length < 20)
                problems.Add("Description must be at least 20 characters");

            if (value.Description.Length > 5000)
                problems.Add("Description cannot exceed 5000 characters");
        }

        if (value.CategoryId == Guid.Empty)
            problems.Add("Category Id cannot be empty");

        // Validate Price
        if (value.Price is null)
            problems.Add("Price is required");
        else if (value.Price.Amount <= 0)
            problems.Add("Price must be greater than zero");

        // Validate ImageUrls
        if (value.ImageUrls is null)
            problems.Add("ImageUrls collection cannot be null");
        else if (value.ImageUrls.Count == 0)
            problems.Add("At least one image is required");
        else if (value.ImageUrls.Count > 10)
            problems.Add("Cannot exceed 10 images per listing");
        else
        {
            for (var i = 0; i < value.ImageUrls.Count; i++)
            {
                var imageUrl = value.ImageUrls[i];
                if (string.IsNullOrWhiteSpace(imageUrl))
                    problems.Add($"ImageUrl at index {i} cannot be null or empty");
            }
        }

        // Validate Tags
        if (value.Tags is not null)
        {
            for (var i = 0; i < value.Tags.Count; i++)
            {
                var tag = value.Tags[i];
                if (string.IsNullOrWhiteSpace(tag))
                    problems.Add($"Tag at index {i} cannot be null or empty");
                else if (tag.Length > 50)
                    problems.Add($"Tag at index {i} cannot exceed 50 characters");
            }
        }

        // Validate ViewCount and InterestCount
        if (value.ViewCount < 0)
            problems.Add("ViewCount cannot be negative");

        if (value.InterestCount < 0)
            problems.Add("InterestCount cannot be negative");

        // Validate Rating
        if (value.Rating is not null)
        {
            if (value.Rating.AverageRating < 1 || value.Rating.AverageRating > 5)
                problems.Add("Rating must be between 1 and 5");
        }

        // Validate dates
        if (value.CreatedAt == default)
            problems.Add("CreatedAt cannot be default DateTime");
        else if (value.CreatedAt > DateTime.UtcNow.AddMinutes(5))
            problems.Add("CreatedAt cannot be in the future");

        if (value.UpdatedAt.HasValue)
        {
            if (value.UpdatedAt.Value > DateTime.UtcNow.AddMinutes(5))
                problems.Add("UpdatedAt cannot be in the future");

            if (value.UpdatedAt.Value < value.CreatedAt)
                problems.Add("UpdatedAt cannot be before CreatedAt");
        }

        if (value.PublishedAt.HasValue)
        {
            if (value.PublishedAt.Value > DateTime.UtcNow.AddMinutes(5))
                problems.Add("PublishedAt cannot be in the future");

            if (value.PublishedAt.Value < value.CreatedAt)
                problems.Add("PublishedAt cannot be before CreatedAt");
        }

        if (value.DueDate.HasValue)
        {
            if (value.DueDate.Value == default)
                problems.Add("DueDate cannot be default DateTime");
            else if (value.DueDate.Value < DateTime.UtcNow.AddDays(-1))
                problems.Add("DueDate cannot be in the past");
            else if (value.DueDate.Value > DateTime.UtcNow.AddYears(1))
                problems.Add("DueDate cannot be more than 1 year in the future");

            if (value.PublishedAt.HasValue && value.DueDate.Value < value.PublishedAt.Value)
                problems.Add("DueDate cannot be before PublishedAt");
        }

        // Validate Status
        if (!Enum.IsDefined(typeof(ListingStatus), value.Status))
            problems.Add("Status must be a valid ListingStatus value");

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified listing is valid.
    /// </summary>
    /// <param name="value">The listing to check.</param>
    /// <returns><see langword="true"/> if the listing is valid; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static bool IsValid(this Listing value)
    {
        return Validate(value).Count == 0;
    }

    /// <summary>
    /// Ensures that the specified listing is valid, throwing an <see cref="ArgumentException"/> with detailed validation messages if not.
    /// </summary>
    /// <param name="value">The listing to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the listing has validation problems.</exception>
    public static void EnsureValid(this Listing value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = Validate(value);
        if (problems.Count == 0)
            return;

        throw new ArgumentException(
            $"Listing validation failed:{Environment.NewLine}- {string.Join($"{Environment.NewLine}- ", problems)}");
    }
}