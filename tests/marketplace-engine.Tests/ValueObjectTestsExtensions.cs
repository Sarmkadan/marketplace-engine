#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using FluentAssertions;
using MarketplaceEngine.Domain.ValueObjects;

namespace MarketplaceEngine.Tests;

/// <summary>
/// Provides extension methods for creating and asserting value objects in tests.
/// </summary>
public static class ValueObjectTestsExtensions
{
    /// <summary>
    /// Creates a Money instance with the specified amount and currency for testing purposes.
    /// </summary>
    /// <param name="amount">The monetary amount</param>
    /// <param name="currencyCode">The 3-letter ISO currency code (e.g., "USD", "EUR")</param>
    /// <returns>A new Money instance</returns>
    /// <exception cref="ArgumentException">Thrown when currency code is null, whitespace, or not 3 characters</exception>
    public static Money CreateMoney(this ValueObjectTests _, decimal amount, string currencyCode = "USD")
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(currencyCode);
        if (currencyCode.Length != 3)
        {
            throw new ArgumentException("Currency code must be exactly 3 characters", nameof(currencyCode));
        }

        return new Money(amount, currencyCode);
    }

    /// <summary>
    /// Creates a Rating instance with the specified score and optional total reviews for testing purposes.
    /// </summary>
    /// <param name="score">The rating score (1-5)</param>
    /// <param name="totalReviews">The total number of reviews</param>
    /// <returns>A new Rating instance</returns>
    /// <exception cref="ArgumentException">Thrown when score is not between 1 and 5, or totalReviews is negative</exception>
    public static Rating CreateRating(this ValueObjectTests _, int score, int totalReviews = 0)
    {
        return new Rating(score, totalReviews);
    }

    /// <summary>
    /// Creates a Location instance with the specified parameters for testing purposes.
    /// </summary>
    /// <param name="city">The city name</param>
    /// <param name="region">The region/state</param>
    /// <param name="countryCode">The 2-letter ISO country code (e.g., "US", "GB")</param>
    /// <param name="postalCode">Optional postal code</param>
    /// <param name="latitude">Optional latitude coordinate</param>
    /// <param name="longitude">Optional longitude coordinate</param>
    /// <returns>A new Location instance</returns>
    /// <exception cref="ArgumentException">Thrown when required parameters are null, whitespace, or invalid</exception>
    public static Location CreateLocation(
        this ValueObjectTests _,
        string city,
        string region,
        string countryCode,
        string? postalCode = null,
        double? latitude = null,
        double? longitude = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(city);
        ArgumentException.ThrowIfNullOrWhiteSpace(region);
        ArgumentException.ThrowIfNullOrWhiteSpace(countryCode);

        return new Location(city, region, countryCode, postalCode, latitude, longitude);
    }

    /// <summary>
    /// Asserts that two Money instances are equal in both amount and currency.
    /// </summary>
    /// <param name="expected">The expected Money instance</param>
    /// <param name="actual">The actual Money instance to test</param>
    /// <exception cref="ArgumentNullException">Thrown when expected or actual is null</exception>
    public static void ShouldBeEquivalentTo(this ValueObjectTests _, Money expected, Money actual)
    {
        ArgumentNullException.ThrowIfNull(expected);
        ArgumentNullException.ThrowIfNull(actual);

        actual.Amount.Should().Be(expected.Amount);
        actual.CurrencyCode.Should().Be(expected.CurrencyCode);
    }

    /// <summary>
    /// Asserts that two Rating instances are equal in both score and total reviews.
    /// </summary>
    /// <param name="expected">The expected Rating instance</param>
    /// <param name="actual">The actual Rating instance to test</param>
    /// <exception cref="ArgumentNullException">Thrown when expected or actual is null</exception>
    public static void ShouldBeEquivalentTo(this ValueObjectTests _, Rating expected, Rating actual)
    {
        ArgumentNullException.ThrowIfNull(expected);
        ArgumentNullException.ThrowIfNull(actual);

        expected.Score.Should().Be(actual.Score);
        expected.TotalReviews.Should().Be(actual.TotalReviews);
    }

    /// <summary>
    /// Asserts that two Location instances are equal in their key properties.
    /// </summary>
    /// <param name="expected">The expected Location instance</param>
    /// <param name="actual">The actual Location instance to test</param>
    /// <exception cref="ArgumentNullException">Thrown when expected or actual is null</exception>
    public static void ShouldBeEquivalentTo(this ValueObjectTests _, Location expected, Location actual)
    {
        ArgumentNullException.ThrowIfNull(expected);
        ArgumentNullException.ThrowIfNull(actual);

        expected.City.Should().Be(actual.City);
        expected.State.Should().Be(actual.State);
        expected.CountryCode.Should().Be(actual.CountryCode);
        expected.PostalCode.Should().Be(actual.PostalCode);
    }
}