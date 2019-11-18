#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text;

namespace MarketplaceEngine.Utilities;

/// <summary>
/// String manipulation and formatting utilities for common text operations.
/// </summary>
public static class StringUtility
{
    /// <summary>
    /// Truncates a string to a maximum length with ellipsis.
    /// </summary>
    public static string Truncate(string? text, int maxLength, string suffix = "...")
    {
        if (string.IsNullOrEmpty(text))
            return string.Empty;

        if (text.Length <= maxLength)
            return text;

        return text[..Math.Max(0, maxLength - suffix.Length)] + suffix;
    }

    /// <summary>
    /// Converts a string to title case (capitalize first letter of each word).
    /// </summary>
    public static string ToTitleCase(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return string.Empty;

        var words = text.Split(' ');
        var titleCased = new StringBuilder();

        foreach (var word in words)
        {
            if (word.Length == 0)
                continue;

            titleCased.Append(char.ToUpper(word[0]));
            if (word.Length > 1)
                titleCased.Append(word[1..].ToLower());

            titleCased.Append(' ');
        }

        return titleCased.ToString().TrimEnd();
    }

    /// <summary>
    /// Converts a string to slug format suitable for URLs.
    /// Removes special characters and converts spaces to hyphens.
    /// </summary>
    public static string ToSlug(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return string.Empty;

        var slug = new StringBuilder();
        foreach (var c in text.ToLower())
        {
            if (char.IsLetterOrDigit(c))
            {
                slug.Append(c);
            }
            else if (c == ' ' || c == '-')
            {
                if (slug.Length > 0 && slug[^1] != '-')
                    slug.Append('-');
            }
        }

        return slug.ToString().TrimEnd('-');
    }

    /// <summary>
    /// Repeats a string N times.
    /// </summary>
    public static string Repeat(string text, int count)
    {
        return string.Concat(Enumerable.Repeat(text, count));
    }

    /// <summary>
    /// Checks if a string contains any of the specified substrings (case-insensitive).
    /// </summary>
    public static bool ContainsAny(string text, params string[] substrings)
    {
        if (string.IsNullOrEmpty(text))
            return false;

        return substrings.Any(s => text.Contains(s, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Masks sensitive information like email addresses or phone numbers.
    /// </summary>
    public static string MaskEmail(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return string.Empty;

        var parts = email.Split('@');
        if (parts.Length != 2)
            return email;

        var localPart = parts[0];
        var domain = parts[1];

        if (localPart.Length <= 2)
            return $"*@{domain}";

        var visibleLength = Math.Max(1, localPart.Length / 2);
        var masked = localPart[..visibleLength] + new string('*', localPart.Length - visibleLength);

        return $"{masked}@{domain}";
    }

    /// <summary>
    /// Masks a phone number showing only last 4 digits.
    /// </summary>
    public static string MaskPhoneNumber(string? phone)
    {
        if (string.IsNullOrWhiteSpace(phone) || phone.Length < 4)
            return phone ?? string.Empty;

        return new string('*', phone.Length - 4) + phone[^4..];
    }

    /// <summary>
    /// Removes all non-alphanumeric characters from a string.
    /// </summary>
    public static string RemoveSpecialCharacters(string? text)
    {
        if (string.IsNullOrEmpty(text))
            return string.Empty;

        return new string(text.Where(c => char.IsLetterOrDigit(c) || c == ' ').ToArray());
    }

    /// <summary>
    /// Generates a random string of specified length using alphanumeric characters.
    /// </summary>
    public static string GenerateRandomString(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        var random = new Random();
        return new string(Enumerable.Range(0, length)
            .Select(_ => chars[random.Next(chars.Length)])
            .ToArray());
    }
}
