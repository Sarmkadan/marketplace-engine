#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace MarketplaceEngine.Domain.ValueObjects;

/// <summary>
/// Extension methods for the <see cref="Location"/> value object providing additional functionality.
/// </summary>
public static class LocationExtensions
{
    /// <summary>
    /// Formats the location as a full address string including postal code and country.
    /// </summary>
    /// <param name="location">The location to format. Cannot be null.</param>
    /// <exception cref="ArgumentNullException"><paramref name="location"/> is null.</exception>
    /// <returns>Formatted address string with city, state, optional postal code, and country code.</returns>
    public static string ToFullAddressString(this Location location)
    {
        ArgumentNullException.ThrowIfNull(location);

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
    /// <param name="location">The location to check. Cannot be null.</param>
    /// <param name="countryCode">The 2-letter country code to compare against. Cannot be null or empty.</param>
    /// <exception cref="ArgumentNullException"><paramref name="location"/> or <paramref name="countryCode"/> is null.</exception>
    /// <exception cref="ArgumentException"><paramref name="countryCode"/> is empty or whitespace.</exception>
    /// <returns>True if the location is in the specified country.</returns>
    public static bool IsInCountry(this Location location, string countryCode)
    {
        ArgumentNullException.ThrowIfNull(location);
        ArgumentException.ThrowIfNullOrEmpty(countryCode, nameof(countryCode));

        return location.CountryCode.Equals(countryCode, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Gets a simplified display name for the location (City, State).
    /// </summary>
    /// <param name="location">The location to format. Cannot be null.</param>
    /// <exception cref="ArgumentNullException"><paramref name="location"/> is null.</exception>
    /// <returns>Simplified location name in format "City, State".</returns>
    public static string ToSimpleName(this Location location)
    {
        ArgumentNullException.ThrowIfNull(location);

        return $"{location.City}, {location.State}";
    }

    /// <summary>
    /// Determines if the location has valid geographic coordinates.
    /// </summary>
    /// <param name="location">The location to check. Cannot be null.</param>
    /// <exception cref="ArgumentNullException"><paramref name="location"/> is null.</exception>
    /// <returns>True if the location has valid latitude and longitude.</returns>
    public static bool HasCoordinates(this Location location)
    {
        ArgumentNullException.ThrowIfNull(location);

        return location.Latitude.HasValue && location.Longitude.HasValue;
    }
}