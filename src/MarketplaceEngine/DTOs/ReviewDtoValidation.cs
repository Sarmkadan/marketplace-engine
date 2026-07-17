#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;

namespace MarketplaceEngine.DTOs;

/// <summary>
/// Provides validation helpers for <see cref="ReviewDto"/> instances.
/// Validates business rules and constraints for review data transfer objects.
/// </summary>
public static class ReviewDtoValidation
{
    /// <summary>
    /// Validates a <see cref="ReviewDto"/> instance and returns a list of human-readable validation errors.
    /// </summary>
    /// <param name="value">The review DTO to validate.</param>
    /// <returns>A read-only list of validation error messages. Empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this ReviewDto? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        // Validate Id
        if (value.Id == Guid.Empty)
        {
            errors.Add("Id must be a valid GUID.");
        }

        // Validate ReviewerId
        if (value.ReviewerId == Guid.Empty)
        {
            errors.Add("ReviewerId must be a valid GUID.");
        }

        // Validate ReviewerName
        if (string.IsNullOrWhiteSpace(value.ReviewerName))
        {
            errors.Add("ReviewerName must not be null or whitespace.");
        }
        else if (value.ReviewerName.Length > 200)
        {
            errors.Add("ReviewerName must not exceed 200 characters.");
        }

        // Validate SellerId
        if (value.SellerId == Guid.Empty)
        {
            errors.Add("SellerId must be a valid GUID.");
        }

        // Validate ReviewerId and SellerId are different (business rule)
        if (value.ReviewerId != Guid.Empty && value.SellerId != Guid.Empty && value.ReviewerId == value.SellerId)
        {
            errors.Add("ReviewerId and SellerId must be different (a seller cannot review themselves).");
        }

        // Validate Score (assuming 1-5 scale)
        if (value.Score < 1 || value.Score > 5)
        {
            errors.Add("Score must be between 1 and 5 inclusive.");
        }

        // Validate Comment
        if (string.IsNullOrWhiteSpace(value.Comment))
        {
            errors.Add("Comment must not be null or whitespace.");
        }
        else
        {
            if (value.Comment.Length < 10)
            {
                errors.Add("Comment must be at least 10 characters long.");
            }

            if (value.Comment.Length > 2000)
            {
                errors.Add("Comment must not exceed 2000 characters.");
            }

            if (value.Comment.Trim().Length == 0)
            {
                errors.Add("Comment must not consist entirely of whitespace.");
            }
        }

        // Validate Status
        if (string.IsNullOrWhiteSpace(value.Status))
        {
            errors.Add("Status must not be null or whitespace.");
        }
        else if (value.Status.Length > 50)
        {
            errors.Add("Status must not exceed 50 characters.");
        }

        // Validate CreatedAt
        if (value.CreatedAt == default)
        {
            errors.Add("CreatedAt must be set to a valid DateTime.");
        }
        else if (value.CreatedAt > DateTime.UtcNow.AddMinutes(5))
        {
            errors.Add("CreatedAt must not be in the future.");
        }

        // Validate UpdatedAt (if set)
        if (value.UpdatedAt.HasValue)
        {
            if (value.UpdatedAt.Value == default)
            {
                errors.Add("UpdatedAt must be a valid DateTime when set.");
            }
            else if (value.UpdatedAt.Value > DateTime.UtcNow.AddMinutes(5))
            {
                errors.Add("UpdatedAt must not be in the future.");
            }
            else if (value.UpdatedAt.Value < value.CreatedAt)
            {
                errors.Add("UpdatedAt must not be earlier than CreatedAt.");
            }
        }

        // Validate SellerReply consistency with UpdatedAt
        if (value.SellerReply is not null && value.UpdatedAt is null)
        {
            errors.Add("UpdatedAt must be set when SellerReply is populated.");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified <see cref="ReviewDto"/> is valid.
    /// </summary>
    /// <param name="value">The review DTO to check.</param>
    /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static bool IsValid(this ReviewDto? value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return Validate(value).Count == 0;
    }

    /// <summary>
    /// Ensures that the specified <see cref="ReviewDto"/> is valid, throwing an <see cref="ArgumentException"/> if it is not.
    /// </summary>
    /// <param name="value">The review DTO to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="value"/> is invalid, containing a list of validation errors.</exception>
    public static void EnsureValid(this ReviewDto? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = Validate(value);
        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"ReviewDto validation failed:{Environment.NewLine}- {string.Join($"{Environment.NewLine}- ", errors)}");
        }
    }
}