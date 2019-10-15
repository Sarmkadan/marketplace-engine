// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace MarketplaceEngine.Domain.ValueObjects;

/// <summary>
/// Immutable value object representing a geographic location.
/// </summary>
public sealed class Location : IEquatable<Location>
{
    public string City { get; }
    public string State { get; }
    public string CountryCode { get; }
    public string? PostalCode { get; }
    public double? Latitude { get; }
    public double? Longitude { get; }

    public Location(string city, string state, string countryCode, string? postalCode = null, double? latitude = null, double? longitude = null)
    {
        // Validates required fields and coordinates
        if (string.IsNullOrWhiteSpace(city))
            throw new ArgumentException("City name is required", nameof(city));

        if (string.IsNullOrWhiteSpace(state))
            throw new ArgumentException("State/Province is required", nameof(state));

        if (string.IsNullOrWhiteSpace(countryCode) || countryCode.Length != 2)
            throw new ArgumentException("Country code must be a valid 2-letter ISO code", nameof(countryCode));

        if (latitude.HasValue && (latitude < -90 || latitude > 90))
            throw new ArgumentException("Latitude must be between -90 and 90", nameof(latitude));

        if (longitude.HasValue && (longitude < -180 || longitude > 180))
            throw new ArgumentException("Longitude must be between -180 and 180", nameof(longitude));

        City = city.Trim();
        State = state.Trim();
        CountryCode = countryCode.ToUpperInvariant();
        PostalCode = string.IsNullOrWhiteSpace(postalCode) ? null : postalCode.Trim();
        Latitude = latitude;
        Longitude = longitude;
    }

    // Calculates approximate distance in kilometers using Haversine formula
    public double? DistanceTo(Location other)
    {
        if (!Latitude.HasValue || !Longitude.HasValue || !other.Latitude.HasValue || !other.Longitude.HasValue)
            return null;

        const double earthRadiusKm = 6371;
        var dLat = DegreesToRadians(other.Latitude.Value - Latitude.Value);
        var dLon = DegreesToRadians(other.Longitude.Value - Longitude.Value);

        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(DegreesToRadians(Latitude.Value)) * Math.Cos(DegreesToRadians(other.Latitude.Value)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

        var c = 2 * Math.Asin(Math.Sqrt(a));
        return earthRadiusKm * c;
    }

    private static double DegreesToRadians(double degrees) => degrees * Math.PI / 180.0;

    public bool Equals(Location? other)
    {
        return other != null &&
               City == other.City &&
               State == other.State &&
               CountryCode == other.CountryCode &&
               PostalCode == other.PostalCode;
    }

    public override bool Equals(object? obj) => Equals(obj as Location);

    public override int GetHashCode() => HashCode.Combine(City, State, CountryCode, PostalCode);

    public override string ToString() => $"{City}, {State} {CountryCode}";

    public static bool operator ==(Location? left, Location? right) => Equals(left, right);
    public static bool operator !=(Location? left, Location? right) => !Equals(left, right);
}
