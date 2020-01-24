#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Globalization;

namespace MarketplaceEngine.DTOs;

/// <summary>
/// Provides validation helpers for <see cref="ReviewDto"/> instances.
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
            errors.Add("Review Id cannot be empty.");
        }

        // Validate ReviewerId
        if (value.ReviewerId == Guid.Empty)
        {
            errors.Add("Reviewer Id cannot be empty.");
        }

        // Validate ReviewerName
        if (string.IsNullOrWhiteSpace(value.ReviewerName))
        {
            errors.Add("Reviewer name cannot be null or whitespace.");
        }
        else if (value.ReviewerName.Length > 200)
        {
            errors.Add("Reviewer name cannot exceed 200 characters.");
        }

        // Validate SellerId
        if (value.SellerId == Guid.Empty)
        {
            errors.Add("Seller Id cannot be empty.");
        }

        // Validate Score (assuming 1-5 scale)
        if (value.Score < 1 || value.Score > 5)
        {
            errors.Add("Score must be between 1 and 5 inclusive.");
        }

        // Validate Comment
        if (string.IsNullOrWhiteSpace(value.Comment))
        {
            errors.Add("Comment cannot be null or whitespace.");
        }
        else if (value.Comment.Length > 2000)
        {
            errors.Add("Comment cannot exceed 2000 characters.");
        }

        // Validate Status
        if (string.IsNullOrWhiteSpace(value.Status))
        {
            errors.Add("Status cannot be null or whitespace.");
        }
        else if (value.Status.Length > 50)
        {
            errors.Add("Status cannot exceed 50 characters.");
        }

        // Validate CreatedAt
        if (value.CreatedAt == default)
        {
            errors.Add("CreatedAt cannot be the default DateTime value.");
        }
        else if (value.CreatedAt > DateTime.UtcNow.AddMinutes(5))
        {
            errors.Add("CreatedAt cannot be in the future.");
        }

        // Validate UpdatedAt (if set)
        if (value.UpdatedAt.HasValue)
        {
            if (value.UpdatedAt.Value == default)
            {
                errors.Add("UpdatedAt cannot be the default DateTime value.");
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
    /// Determines whether the specified <see cref="ReviewDto"/> is valid.
    /// </summary>
    /// <param name="value">The review DTO to check.</param>
    /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static bool IsValid(this ReviewDto? value)
    {
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
                $"ReviewDto is invalid. Validation errors: {string.Join(", ", errors)}");
        }
    }
}