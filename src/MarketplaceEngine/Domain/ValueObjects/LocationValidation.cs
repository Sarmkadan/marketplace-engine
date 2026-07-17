#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Globalization;

namespace MarketplaceEngine.Domain.ValueObjects;

/// <summary>
/// Provides validation helpers for <see cref="Location"/> instances.
/// </summary>
public static class LocationValidation
{
    /// <summary>
    /// Validates a <see cref="Location"/> instance and returns a list of human-readable validation problems.
    /// </summary>
    /// <param name="value">The location to validate.</param>
    /// <returns>An empty list if valid; otherwise, a list of validation error messages.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this Location value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        // Validate required string properties
        if (string.IsNullOrWhiteSpace(value.City))
            errors.Add("City name is required and cannot be empty or whitespace.");

        if (string.IsNullOrWhiteSpace(value.State))
            errors.Add("State/Province is required and cannot be empty or whitespace.");

        if (string.IsNullOrWhiteSpace(value.CountryCode))
            errors.Add("Country code is required and cannot be empty or whitespace.");
        else if (value.CountryCode.Length != 2)
            errors.Add("Country code must be exactly 2 characters long.");
        else if (!IsValidCountryCode(value.CountryCode))
            errors.Add($"Country code '{value.CountryCode}' is not a valid 2-letter ISO country code.");

        if (!string.IsNullOrWhiteSpace(value.PostalCode) && string.IsNullOrWhiteSpace(value.PostalCode.Trim()))
            errors.Add("Postal code cannot be empty or whitespace if provided.");

        // Validate coordinate ranges
        if (value.Latitude.HasValue)
        {
            if (value.Latitude < -90 || value.Latitude > 90)
                errors.Add("Latitude must be between -90 and 90 degrees inclusive.");
        }

        if (value.Longitude.HasValue)
        {
            if (value.Longitude < -180 || value.Longitude > 180)
                errors.Add("Longitude must be between -180 and 180 degrees inclusive.");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified <see cref="Location"/> instance is valid.
    /// </summary>
    /// <param name="value">The location to check.</param>
    /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static bool IsValid(this Location value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return Validate(value).Count == 0;
    }

    /// <summary>
    /// Ensures that the specified <see cref="Location"/> instance is valid, throwing an <see cref="ArgumentException"/>
    /// with a detailed message listing all validation problems if it is not.
    /// </summary>
    /// <param name="value">The location to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="value"/> is not valid.</exception>
    public static void EnsureValid(this Location value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = Validate(value);
        if (errors.Count == 0)
            return;

        throw new ArgumentException(
            $"Location is not valid:{Environment.NewLine}{string.Join(Environment.NewLine, errors)}");
    }

    /// <summary>
    /// Validates that a string is a valid 2-letter ISO country code.
    /// </summary>
    /// <param name="countryCode">The country code to validate.</param>
    /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
    private static bool IsValidCountryCode(string countryCode)
    {
        // Validate length and format
        if (countryCode.Length != 2)
            return false;

        // Check for valid format: 2 uppercase letters using pattern matching
        return countryCode is [>= 'A', >= 'A'] and [<= 'Z', <= 'Z'];
    }
}