#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using System.Collections.Generic;
using System.Globalization;
using MarketplaceEngine.Domain.Enums;

namespace MarketplaceEngine.Domain.Models;

/// <summary>
/// Provides validation helpers for <see cref="Review"/> instances.
/// </summary>
public static class ReviewValidation
{
    /// <summary>
    /// Validates a review and returns a list of human-readable problems.
    /// </summary>
    /// <param name="value">The review to validate.</param>
    /// <returns>A list of validation problems (empty if valid).</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this Review value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        // Validate Id
        if (value.Id == Guid.Empty)
        {
            errors.Add("Review Id must be a valid GUID.");
        }

        // Validate ReviewerId and SellerId
        if (value.ReviewerId == Guid.Empty)
        {
            errors.Add("ReviewerId must be a valid GUID.");
        }

        if (value.SellerId == Guid.Empty)
        {
            errors.Add("SellerId must be a valid GUID.");
        }

        // Validate Reviewer and Seller navigation properties
        if (value.ReviewerId != Guid.Empty && value.Reviewer is null)
        {
            errors.Add("Reviewer navigation property must be populated when ReviewerId is set.");
        }

        if (value.SellerId != Guid.Empty && value.Seller is null)
        {
            errors.Add("Seller navigation property must be populated when SellerId is set.");
        }

        // Validate that Reviewer and Seller are different
        if (value.ReviewerId != Guid.Empty && value.SellerId != Guid.Empty && value.ReviewerId == value.SellerId)
        {
            errors.Add("A seller cannot be reviewed by themselves (ReviewerId and SellerId must be different).");
        }

        // Validate ListingId and Listing navigation property
        if (value.ListingId.HasValue && value.Listing is null)
        {
            errors.Add("Listing navigation property must be populated when ListingId is set.");
        }

        // Validate Score
        if (value.Score < 1 || value.Score > 5)
        {
            errors.Add("Score must be between 1 and 5 inclusive.");
        }

        // Validate Comment
        if (string.IsNullOrWhiteSpace(value.Comment))
        {
            errors.Add("Comment cannot be null, empty, or whitespace.");
        }
        else
        {
            if (value.Comment.Length < 10)
            {
                errors.Add("Comment must be at least 10 characters long.");
            }

            if (value.Comment.Length > 2000)
            {
                errors.Add("Comment cannot exceed 2000 characters.");
            }

            // Check for whitespace-only content
            if (value.Comment.Trim().Length == 0)
            {
                errors.Add("Comment cannot consist entirely of whitespace.");
            }
        }

        // Validate Status
        if (!Enum.IsDefined(typeof(ReviewStatus), value.Status))
        {
            errors.Add("Status must be a valid ReviewStatus value.");
        }

        // Validate SellerReply and RepliedAt consistency
        if (value.SellerReply is not null && value.RepliedAt is null)
        {
            errors.Add("RepliedAt must be set when SellerReply is populated.");
        }
        else if (value.SellerReply is null && value.RepliedAt.HasValue)
        {
            errors.Add("SellerReply must be populated when RepliedAt is set.");
        }

        // Validate SellerReply content if present
        if (value.SellerReply is not null)
        {
            if (value.SellerReply.Length < 5)
            {
                errors.Add("SellerReply must be at least 5 characters long.");
            }

            if (value.SellerReply.Length > 1000)
            {
                errors.Add("SellerReply cannot exceed 1000 characters.");
            }

            if (value.SellerReply.Trim().Length == 0)
            {
                errors.Add("SellerReply cannot consist entirely of whitespace.");
            }
        }

        // Validate CreatedAt
        if (value.CreatedAt == default)
        {
            errors.Add("CreatedAt must be set to a valid DateTime.");
        }
        else if (value.CreatedAt > DateTime.UtcNow.AddMinutes(5))
        {
            errors.Add("CreatedAt cannot be in the future.");
        }

        // Validate UpdatedAt consistency
        if (value.UpdatedAt.HasValue)
        {
            if (value.UpdatedAt.Value < value.CreatedAt)
            {
                errors.Add("UpdatedAt cannot be earlier than CreatedAt.");
            }

            if (value.UpdatedAt.Value > DateTime.UtcNow.AddMinutes(5))
            {
                errors.Add("UpdatedAt cannot be in the future.");
            }
        }

        // Validate status-specific rules
        switch (value.Status)
        {
            case ReviewStatus.Removed:
                // Removed reviews should not have active content
                if (value.Score != default)
                {
                    errors.Add("Removed reviews should have Score set to default (0).");
                }
                break;

            case ReviewStatus.UnderReview:
                // Under review reviews should have valid timestamps
                if (!value.CreatedAt.IsValidTimestamp())
                {
                    errors.Add("UnderReview reviews must have a valid CreatedAt timestamp.");
                }
                break;
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether a review is valid.
    /// </summary>
    /// <param name="value">The review to check.</param>
    /// <returns>True if the review is valid; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static bool IsValid(this Review value)
    {
        return value.Validate().Count == 0;
    }

    /// <summary>
    /// Ensures that a review is valid, throwing an exception with detailed error messages if not.
    /// </summary>
    /// <param name="value">The review to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if the review is invalid, containing a list of problems.</exception>
    public static void EnsureValid(this Review value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = value.Validate();

        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"Review validation failed:{Environment.NewLine}- {
                    string.Join($"{Environment.NewLine}- ", errors)}");
        }
    }

    /// <summary>
    /// Validates that a DateTime is not default and is a reasonable timestamp.
    /// </summary>
    private static bool IsValidTimestamp(this DateTime dateTime)
    {
        return dateTime != default && dateTime <= DateTime.UtcNow.AddMinutes(5);
    }
}