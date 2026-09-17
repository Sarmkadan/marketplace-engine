#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text.RegularExpressions;

namespace MarketplaceEngine.Utilities;

/// <summary>
/// Centralized validation utilities for common data validation scenarios.
/// Ensures consistent validation rules across the application.
/// </summary>
public static class ValidationUtility
{
    private const string EmailPattern = @"^[^\s@]+@[^\s@]+\.[^\s@]+$";
    private const string PhonePattern = @"^\+?[1-9]\d{1,14}$";

    private const int MaxEmailLength = 254;
    private const int DefaultMinTextLength = 1;
    private const int DefaultMaxTextLength = int.MaxValue;
    private const decimal DefaultMinPrice = 0.01m;
    private const decimal DefaultMaxPrice = 999999.99m;
    private const decimal DefaultMinRating = 0m;
    private const decimal DefaultMaxRating = 5m;
    private const int DefaultMaxPageSize = 100;
    private const int DefaultMaxInputLength = 1000;
    private const int DefaultMinSearchQueryLength = 2;
    private const int DefaultMaxSearchQueryLength = 100;

    private static readonly Regex EmailRegex = new(
        EmailPattern,
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static readonly Regex PhoneRegex = new(
        PhonePattern,
        RegexOptions.Compiled);

    /// <summary>
    /// Validates email address format.
    /// </summary>
    public static bool IsValidEmail(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        return EmailRegex.IsMatch(email) && email.Length <= MaxEmailLength;
    }

    /// <summary>
    /// Validates phone number format (international format).
    /// </summary>
    public static bool IsValidPhoneNumber(string? phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
            return false;

        return PhoneRegex.IsMatch(phone);
    }

    /// <summary>
    /// Validates that a string is not empty and within length constraints.
    /// </summary>
    public static bool IsValidText(string? text, int minLength = DefaultMinTextLength, int maxLength = DefaultMaxTextLength)
    {
        if (string.IsNullOrWhiteSpace(text))
            return false;

        return text.Length >= minLength && text.Length <= maxLength;
    }

    /// <summary>
    /// Validates a URL format.
    /// </summary>
    public static bool IsValidUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return false;

        return Uri.TryCreate(url, UriKind.Absolute, out var uriResult) &&
               (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
    }

    /// <summary>
    /// Validates a price is positive and within reasonable bounds.
    /// </summary>
    public static bool IsValidPrice(decimal price, decimal minPrice = DefaultMinPrice, decimal maxPrice = DefaultMaxPrice)
    {
        return price >= minPrice && price <= maxPrice;
    }

    /// <summary>
    /// Validates a rating score (typically 1-5 stars).
    /// </summary>
    public static bool IsValidRating(decimal rating, decimal minRating = DefaultMinRating, decimal maxRating = DefaultMaxRating)
    {
        return rating >= minRating && rating <= maxRating;
    }

    /// <summary>
    /// Validates a GUID is not empty.
    /// </summary>
    public static bool IsValidGuid(Guid guid)
    {
        return guid != Guid.Empty;
    }

    /// <summary>
    /// Validates pagination parameters.
    /// </summary>
    public static bool IsValidPagination(int page, int pageSize, int maxPageSize = DefaultMaxPageSize)
    {
        return page >= 1 && pageSize >= 1 && pageSize <= maxPageSize;
    }

    /// <summary>
    /// Sanitizes user input to prevent injection attacks.
    /// Removes potentially harmful characters.
    /// </summary>
    public static string SanitizeInput(string input, int maxLength = DefaultMaxInputLength)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        // Remove null characters and control characters
        var sanitized = new string(input
            .Where(c => !char.IsControl(c) || c == '\n' || c == '\r' || c == '\t')
            .ToArray());

        // Truncate to max length
        return sanitized.Length > maxLength
            ? sanitized.Substring(0, maxLength)
            : sanitized;
    }

    /// <summary>
    /// Validates a search query is appropriate length and not empty.
    /// </summary>
    public static bool IsValidSearchQuery(string? query, int minLength = DefaultMinSearchQueryLength, int maxLength = DefaultMaxSearchQueryLength)
    {
        if (string.IsNullOrWhiteSpace(query))
            return false;

        return query.Length >= minLength && query.Length <= maxLength;
    }
}
