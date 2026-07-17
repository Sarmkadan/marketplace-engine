#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using System.Collections.Generic;

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
    public static IReadOnlyList<string> Validate(this ListingDto? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        // Validate required string properties
        ArgumentException.ThrowIfNullOrEmpty(value.Title, nameof(value.Title));
        if (value.Title.Length > 200)
        {
            errors.Add("Title must not exceed 200 characters.");
        }

        ArgumentException.ThrowIfNullOrEmpty(value.Description, nameof(value.Description));
        if (value.Description.Length > 5000)
        {
            errors.Add("Description must not exceed 5000 characters.");
        }

        ArgumentException.ThrowIfNullOrEmpty(value.SellerName, nameof(value.SellerName));
        if (value.SellerName.Length > 100)
        {
            errors.Add("SellerName must not exceed 100 characters.");
        }

        ArgumentException.ThrowIfNullOrEmpty(value.Status, nameof(value.Status));
        if (value.Status.Length > 50)
        {
            errors.Add("Status must not exceed 50 characters.");
        }

        // Validate Status enum values using pattern matching
        if (!Enum.TryParse<Domain.Enums.ListingStatus>(value.Status, ignoreCase: true, out _))
        {
            errors.Add("Status must be a valid ListingStatus enum value.");
        }

        // Validate numeric properties using pattern matching
        if (value.Price < 0)
        {
            errors.Add("Price must be non-negative.");
        }
        else if (value.Price == 0)
        {
            errors.Add("Price must be greater than zero.");
        }
        else if (decimal.Round(value.Price, 2) != value.Price)
        {
            errors.Add("Price must have at most 2 decimal places.");
        }
        else if (value.Price > 1_000_000)
        {
            errors.Add("Price must not exceed 1,000,000.");
        }

        if (value.ViewCount < 0)
        {
            errors.Add("ViewCount must be non-negative.");
        }
        else if (value.ViewCount > 1_000_000)
        {
            errors.Add("ViewCount must not exceed 1,000,000.");
        }

        // Validate GUID properties using pattern matching
        if (value.Id == Guid.Empty)
        {
            errors.Add("Id must be a valid GUID.");
        }

        if (value.SellerId == Guid.Empty)
        {
            errors.Add("SellerId must be a valid GUID.");
        }

        if (value.CategoryId == Guid.Empty)
        {
            errors.Add("CategoryId must be a valid GUID.");
        }

        // Validate date properties using pattern matching
        if (value.CreatedAt == default)
        {
            errors.Add("CreatedAt must be set to a valid DateTime.");
        }
        else if (value.CreatedAt > DateTime.UtcNow.AddMinutes(5))
        {
            errors.Add("CreatedAt must not be in the future.");
        }

        if (value.UpdatedAt is { } updatedAt)
        {
            if (updatedAt == default)
            {
                errors.Add("UpdatedAt must be a valid DateTime when set.");
            }
            else if (updatedAt > DateTime.UtcNow.AddMinutes(5))
            {
                errors.Add("UpdatedAt must not be in the future.");
            }
            else if (updatedAt < value.CreatedAt)
            {
                errors.Add("UpdatedAt must not be earlier than CreatedAt.");
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
    public static bool IsValid(this ListingDto? value)
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
    public static void EnsureValid(this ListingDto? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = Validate(value);
        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"ListingDto validation failed:{Environment.NewLine}- {string.Join($"{Environment.NewLine}- ", errors)}",
                nameof(value));
        }
    }
}