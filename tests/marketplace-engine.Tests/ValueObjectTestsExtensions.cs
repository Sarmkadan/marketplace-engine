#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FluentAssertions;
using MarketplaceEngine.Domain.ValueObjects;

namespace MarketplaceEngine.Tests;

public static class ValueObjectTestsExtensions
{
    /// <summary>
    /// Creates a Money instance with the specified amount and currency for testing purposes.
    /// </summary>
    /// <param name="amount">The monetary amount</param>
    /// <param name="currencyCode">The 3-letter ISO currency code (e.g., "USD", "EUR")</param>
    /// <returns>A new Money instance</returns>
    public static Money CreateMoney(this ValueObjectTests _, decimal amount, string currencyCode = "USD")
    {
        return new Money(amount, currencyCode);
    }

    /// <summary>
    /// Creates a Rating instance with the specified score and optional total reviews for testing purposes.
    /// </summary>
    /// <param name="score">The rating score (1-5)</param>
    /// <param name="totalReviews">The total number of reviews</param>
    /// <returns>A new Rating instance</returns>
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
    /// <returns>A new Location instance</returns>
    public static Location CreateLocation(this ValueObjectTests _, string city, string region, string countryCode)
    {
        return new Location(city, region, countryCode);
    }

    /// <summary>
    /// Asserts that two Money instances are equal in both amount and currency.
    /// </summary>
    /// <param name="expected">The expected Money instance</param>
    /// <param name="actual">The actual Money instance to test</param>
    public static void ShouldBeEquivalentTo(this ValueObjectTests _, Money expected, Money actual)
    {
        actual.Amount.Should().Be(expected.Amount);
        actual.CurrencyCode.Should().Be(expected.CurrencyCode);
    }
}