#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using MarketplaceEngine.Domain.Enums;
using MarketplaceEngine.Domain.Models;
using MarketplaceEngine.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace MarketplaceEngine.Services;

/// <summary>
/// Provides validation helpers for <see cref="UserService"/> to ensure data integrity
/// before operations are performed on user accounts and profiles.
/// </summary>
public static class UserServiceValidation
{
    /// <summary>
    /// Validates that a <see cref="UserService"/> instance is in a valid state.
    /// </summary>
    /// <param name="value">The user service instance to validate</param>
    /// <returns>A list of human-readable validation problems, or an empty list if valid</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null</exception>
    public static IReadOnlyList<string> Validate(this UserService? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        return Array.Empty<string>();
    }

    /// <summary>
    /// Determines whether the specified <see cref="UserService"/> instance is valid.
    /// </summary>
    /// <param name="value">The user service instance to check</param>
    /// <returns>True if the instance is valid; otherwise, false</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null</exception>
    public static bool IsValid(this UserService? value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return Validate(value).Count == 0;
    }

    /// <summary>
    /// Ensures that the specified <see cref="UserService"/> instance is in a valid state,
    /// throwing an <see cref="ArgumentException"/> with detailed validation messages if not.
    /// </summary>
    /// <param name="value">The user service instance to validate</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null</exception>
    /// <exception cref="ArgumentException">Thrown when the instance is invalid, containing all validation problems</exception>
    public static void EnsureValid(this UserService? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = Validate(value);
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"UserService validation failed:{Environment.NewLine}- {
                    string.Join($"{Environment.NewLine}- ", problems)}",
                nameof(value));
        }
    }

    /// <summary>
    /// Validates a user entity for registration.
    /// </summary>
    /// <param name="email">The user's email address</param>
    /// <param name="fullName">The user's full name</param>
    /// <param name="phone">Optional phone number</param>
    /// <returns>A list of validation problems, or empty if valid</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="email"/> or <paramref name="fullName"/> is null</exception>
    public static IReadOnlyList<string> ValidateForRegistration(
        string? email,
        string? fullName,
        string? phone = null)
    {
        var problems = new List<string>();

        ArgumentException.ThrowIfNullOrEmpty(email);
        ArgumentException.ThrowIfNullOrEmpty(fullName);

        if (string.IsNullOrWhiteSpace(email))
        {
            problems.Add("Email is required");
        }
        else if (!IsValidEmail(email))
        {
            problems.Add("Email must be a valid email address");
        }

        if (string.IsNullOrWhiteSpace(fullName))
        {
            problems.Add("Full name is required");
        }
        else if (fullName.Length < 2)
        {
            problems.Add("Full name must be at least 2 characters long");
        }
        else if (fullName.Length > 100)
        {
            problems.Add("Full name cannot exceed 100 characters");
        }

        if (!string.IsNullOrWhiteSpace(phone) && phone.Length > 20)
        {
            problems.Add("Phone number cannot exceed 20 characters");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates a user entity for profile updates.
    /// </summary>
    /// <param name="fullName">The user's full name</param>
    /// <param name="phone">Optional phone number</param>
    /// <param name="bio">Optional user biography</param>
    /// <param name="location">Optional geographic location</param>
    /// <returns>A list of validation problems, or empty if valid</returns>
    public static IReadOnlyList<string> ValidateForProfileUpdate(
        string? fullName,
        string? phone,
        string? bio,
        Location? location)
    {
        var problems = new List<string>();

        if (!string.IsNullOrWhiteSpace(fullName))
        {
            if (fullName.Length < 2)
            {
                problems.Add("Full name must be at least 2 characters long");
            }
            else if (fullName.Length > 100)
            {
                problems.Add("Full name cannot exceed 100 characters");
            }
        }

        if (!string.IsNullOrWhiteSpace(phone) && phone.Length > 20)
        {
            problems.Add("Phone number cannot exceed 20 characters");
        }

        if (!string.IsNullOrWhiteSpace(bio) && bio.Length > 500)
        {
            problems.Add("Bio cannot exceed 500 characters");
        }

        if (location is not null)
        {
            if (string.IsNullOrWhiteSpace(location.City))
            {
                problems.Add("Location city is required");
            }

            if (string.IsNullOrWhiteSpace(location.State))
            {
                problems.Add("Location state/province is required");
            }

            if (string.IsNullOrWhiteSpace(location.CountryCode) || location.CountryCode.Length != 2)
            {
                problems.Add("Location country code must be a valid 2-letter ISO code");
            }

            if (location.Latitude.HasValue && (location.Latitude < -90 || location.Latitude > 90))
            {
                problems.Add("Location latitude must be between -90 and 90");
            }

            if (location.Longitude.HasValue && (location.Longitude < -180 || location.Longitude > 180))
            {
                problems.Add("Location longitude must be between -180 and 180");
            }
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates a user entity for email verification.
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="verificationToken">The verification token to validate</param>
    /// <returns>A list of validation problems, or empty if valid</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="verificationToken"/> is null</exception>
    public static IReadOnlyList<string> ValidateForEmailVerification(
        Guid userId,
        string? verificationToken)
    {
        var problems = new List<string>();

        if (userId == Guid.Empty)
        {
            problems.Add("User ID cannot be empty");
        }

        ArgumentException.ThrowIfNullOrEmpty(verificationToken);

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates a user entity for premium promotion.
    /// </summary>
    /// <param name="totalSales">The user's total sales count</param>
    /// <param name="rating">The user's rating</param>
    /// <returns>A list of validation problems, or empty if valid</returns>
    public static IReadOnlyList<string> ValidateForPremiumPromotion(
        int totalSales,
        Rating? rating)
    {
        var problems = new List<string>();

        if (totalSales < 5)
        {
            problems.Add("User must have at least 5 sales to be promoted to premium");
        }

        if (rating is null)
        {
            problems.Add("User must have a rating to be promoted to premium");
        }
        else if (rating.Score < 4)
        {
            problems.Add("User must have a rating of 4+ stars to be promoted to premium");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates a user entity for account deactivation/reactivation.
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="isActive">Whether the account should be active</param>
    /// <returns>A list of validation problems, or empty if valid</returns>
    public static IReadOnlyList<string> ValidateForAccountStatusChange(
        Guid userId,
        bool isActive)
    {
        var problems = new List<string>();

        if (userId == Guid.Empty)
        {
            problems.Add("User ID cannot be empty");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates a user entity for sale recording.
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <returns>A list of validation problems, or empty if valid</returns>
    public static IReadOnlyList<string> ValidateForSaleRecording(Guid userId)
    {
        var problems = new List<string>();

        if (userId == Guid.Empty)
        {
            problems.Add("User ID cannot be empty");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates a user entity for rating updates.
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="rating">The new rating</param>
    /// <returns>A list of validation problems, or empty if valid</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="rating"/> is null</exception>
    public static IReadOnlyList<string> ValidateForRatingUpdate(
        Guid userId,
        Rating? rating)
    {
        var problems = new List<string>();

        if (userId == Guid.Empty)
        {
            problems.Add("User ID cannot be empty");
        }

        ArgumentNullException.ThrowIfNull(rating);

        if (rating.Score < 1 || rating.Score > 5)
        {
            problems.Add("Rating score must be between 1 and 5");
        }

        if (rating.TotalReviews < 0)
        {
            problems.Add("Rating total reviews cannot be negative");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates pagination parameters for user queries.
    /// </summary>
    /// <param name="pageNumber">The page number (1-based)</param>
    /// <param name="pageSize">The number of items per page</param>
    /// <returns>A list of validation problems, or empty if valid</returns>
    public static IReadOnlyList<string> ValidatePaginationParameters(
        int pageNumber,
        int pageSize)
    {
        var problems = new List<string>();

        if (pageNumber < 1)
        {
            problems.Add("Page number must be at least 1");
        }

        if (pageSize < 1)
        {
            problems.Add("Page size must be at least 1");
        }
        else if (pageSize > 100)
        {
            problems.Add("Page size cannot exceed 100");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates user access permissions.
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="isActive">Whether the user account is active</param>
    /// <param name="isVerified">Whether the user email is verified</param>
    /// <returns>A list of validation problems, or empty if valid</returns>
    public static IReadOnlyList<string> ValidateUserAccess(
        Guid userId,
        bool isActive,
        bool isVerified)
    {
        var problems = new List<string>();

        if (userId == Guid.Empty)
        {
            problems.Add("User ID cannot be empty");
        }

        if (!isActive)
        {
            problems.Add("User account is not active");
        }

        if (!isVerified)
        {
            problems.Add("User email is not verified");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates a user entity for public profile display.
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <returns>A list of validation problems, or empty if valid</returns>
    public static IReadOnlyList<string> ValidateForPublicProfile(Guid userId)
    {
        var problems = new List<string>();

        if (userId == Guid.Empty)
        {
            problems.Add("User ID cannot be empty");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates that an email address has a valid format.
    /// </summary>
    /// <param name="email">The email address to validate</param>
    /// <returns>True if the email is valid; otherwise, false</returns>
    private static bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return false;
        }

        try
        {
            // Basic email regex pattern
            var pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            return Regex.IsMatch(email, pattern, RegexOptions.IgnoreCase);
        }
        catch (RegexMatchTimeoutException)
        {
            return false;
        }
    }
}