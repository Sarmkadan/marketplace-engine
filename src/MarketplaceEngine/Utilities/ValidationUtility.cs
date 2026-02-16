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
    private static readonly Regex EmailRegex = new(
        @"^[^\s@]+@[^\s@]+\.[^\s@]+$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static readonly Regex PhoneRegex = new(
        @"^\+?[1-9]\d{1,14}$",
        RegexOptions.Compiled);

    /// <summary>
    /// Validates email address format.
    /// </summary>
    public static bool IsValidEmail(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        return EmailRegex.IsMatch(email) && email.Length <= 254;
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
    public static bool IsValidText(string? text, int minLength = 1, int maxLength = int.MaxValue)
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
    public static bool IsValidPrice(decimal price, decimal minPrice = 0.01m, decimal maxPrice = 999999.99m)
    {
        return price >= minPrice && price <= maxPrice;
    }

    /// <summary>
    /// Validates a rating score (typically 1-5 stars).
    /// </summary>
    public static bool IsValidRating(decimal rating, decimal minRating = 0m, decimal maxRating = 5m)
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
    public static bool IsValidPagination(int page, int pageSize, int maxPageSize = 100)
    {
        return page >= 1 && pageSize >= 1 && pageSize <= maxPageSize;
    }

    /// <summary>
    /// Sanitizes user input to prevent injection attacks.
    /// Removes potentially harmful characters.
    /// </summary>
    public static string SanitizeInput(string input, int maxLength = 1000)
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
    public static bool IsValidSearchQuery(string? query, int minLength = 2, int maxLength = 100)
    {
        if (string.IsNullOrWhiteSpace(query))
            return false;

        return query.Length >= minLength && query.Length <= maxLength;
    }
}
