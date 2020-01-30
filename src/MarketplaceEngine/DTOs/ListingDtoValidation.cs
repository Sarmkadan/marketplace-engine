#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Globalization;

namespace MarketplaceEngine.DTOs;

/// <summary>
/// Provides validation helpers for <see cref="ListingDto"/> instances.
/// Validates business rules and constraints for listing data transfer objects.
/// </summary>
public static class ListingDtoValidation
{
    /// <summary>
    /// Validates the specified <see cref="ListingDto"/> instance.
    /// </summary>
    /// <param name="value">The listing DTO to validate.</param>
    /// <returns>A read-only list of validation error messages. Empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this ListingDto value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        // Validate required string properties
        if (string.IsNullOrWhiteSpace(value.Title))
        {
            errors.Add("Title cannot be null or whitespace.");
        }
        else if (value.Title.Length > 200)
        {
            errors.Add("Title cannot exceed 200 characters.");
        }

        if (string.IsNullOrWhiteSpace(value.Description))
        {
            errors.Add("Description cannot be null or whitespace.");
        }
        else if (value.Description.Length > 5000)
        {
            errors.Add("Description cannot exceed 5000 characters.");
        }

        if (string.IsNullOrWhiteSpace(value.SellerName))
        {
            errors.Add("SellerName cannot be null or whitespace.");
        }
        else if (value.SellerName.Length > 100)
        {
            errors.Add("SellerName cannot exceed 100 characters.");
        }

        if (string.IsNullOrWhiteSpace(value.Status))
        {
            errors.Add("Status cannot be null or whitespace.");
        }
        else if (value.Status.Length > 50)
        {
            errors.Add("Status cannot exceed 50 characters.");
        }

        // Validate numeric properties
        if (value.Price < 0)
        {
            errors.Add("Price cannot be negative.");
        }
        else if (value.Price > 1_000_000)
        {
            errors.Add("Price cannot exceed 1,000,000.");
        }

        if (value.ViewCount < 0)
        {
            errors.Add("ViewCount cannot be negative.");
        }

        // Validate GUID properties
        if (value.Id == Guid.Empty)
        {
            errors.Add("Id cannot be empty.");
        }

        if (value.SellerId == Guid.Empty)
        {
            errors.Add("SellerId cannot be empty.");
        }

        if (value.CategoryId == Guid.Empty)
        {
            errors.Add("CategoryId cannot be empty.");
        }

        // Validate date properties
        if (value.CreatedAt == default)
        {
            errors.Add("CreatedAt cannot be default (Unix epoch).");
        }
        else if (value.CreatedAt > DateTime.UtcNow.AddMinutes(5))
        {
            errors.Add("CreatedAt cannot be in the future.");
        }

        if (value.UpdatedAt.HasValue)
        {
            if (value.UpdatedAt.Value == default)
            {
                errors.Add("UpdatedAt cannot be default (Unix epoch) when set.");
            }
            else if (value.UpdatedAt.Value > DateTime.UtcNow.AddMinutes(5))
            {
                errors.Add("UpdatedAt cannot be in the future.");
            }
            else if (value.UpdatedAt.Value < value.CreatedAt)
            {
                errors.Add("UpdatedAt cannot be earlier than CreatedAt.");
            }
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified <see cref="ListingDto"/> instance is valid.
    /// </summary>
    /// <param name="value">The listing DTO to check.</param>
    /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static bool IsValid(this ListingDto value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return Validate(value).Count == 0;
    }

    /// <summary>
    /// Ensures that the specified <see cref="ListingDto"/> instance is valid.
    /// </summary>
    /// <param name="value">The listing DTO to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="value"/> is invalid.
    /// The exception message contains all validation errors.</exception>
    public static void EnsureValid(this ListingDto value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = Validate(value);
        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"ListingDto validation failed:{Environment.NewLine}{string.Join(Environment.NewLine, errors)}");
        }
    }
}