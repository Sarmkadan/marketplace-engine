#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace MarketplaceEngine.Domain.ValueObjects;

/// <summary>
/// Extension methods for the Location value object providing additional functionality.
/// </summary>
public static class LocationExtensions
{
    /// <summary>
    /// Formats the location as a full address string including postal code and country.
    /// </summary>
    /// <param name="location">The location to format</param>
    /// <returns>Formatted address string</returns>
    public static string ToFullAddressString(this Location location)
    {
        if (location == null)
            throw new ArgumentNullException(nameof(location));

        var parts = new List<string>();
        parts.Add(location.City);
        parts.Add(location.State);

        if (!string.IsNullOrWhiteSpace(location.PostalCode))
            parts.Add(location.PostalCode);

        parts.Add(location.CountryCode);

        return string.Join(", ", parts);
    }

    /// <summary>
    /// Determines if the location is within a specified country.
    /// </summary>
    /// <param name="location">The location to check</param>
    /// <param name="countryCode">The 2-letter country code to compare against</param>
    /// <returns>True if the location is in the specified country</returns>
    public static bool IsInCountry(this Location location, string countryCode)
    {
        if (location == null)
            throw new ArgumentNullException(nameof(location));

        if (string.IsNullOrWhiteSpace(countryCode))
            throw new ArgumentException("Country code cannot be null or empty", nameof(countryCode));

        return location.CountryCode.Equals(countryCode, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Gets a simplified display name for the location (City, State).
    /// </summary>
    /// <param name="location">The location to format</param>
    /// <returns>Simplified location name</returns>
    public static string ToSimpleName(this Location location)
    {
        if (location == null)
            throw new ArgumentNullException(nameof(location));

        return $"{location.City}, {location.State}";
    }

    /// <summary>
    /// Determines if the location has valid geographic coordinates.
    /// </summary>
    /// <param name="location">The location to check</param>
    /// <returns>True if the location has valid latitude and longitude</returns>
    public static bool HasCoordinates(this Location location)
    {
        if (location == null)
            throw new ArgumentNullException(nameof(location));

        return location.Latitude.HasValue && location.Longitude.HasValue;
    }
}