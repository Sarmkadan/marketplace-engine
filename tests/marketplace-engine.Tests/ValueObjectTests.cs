#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FluentAssertions;
using MarketplaceEngine.Domain.ValueObjects;
using Xunit;

/// <summary>
/// Tests for value objects in the MarketplaceEngine domain.
/// </summary>
public class ValueObjectTests
{
    /// <summary>
    /// Verifies that the Money constructor throws an ArgumentException when given a negative amount.
    /// </summary>
    [Fact]
    public void Money_Constructor_WithNegativeAmount_ThrowsArgumentException()
    {
        // Arrange & Act
        var act = () => new Money(-10m, "USD");

        // Assert
        act.Should().Throw<ArgumentException>()
           .WithMessage("*negative*");
    }

    /// <summary>
    /// Verifies that the Add method of the Money class returns the correct sum when adding two Money objects with the same currency.
    /// </summary>
    [Fact]
    public void Money_Add_WithSameCurrency_ReturnsCorrectSum()
    {
        // Arrange
        var price1 = new Money(50m, "USD");
        var price2 = new Money(30m, "USD");

        // Act
        var result = price1.Add(price2);

        // Assert
        result.Amount.Should().Be(80m);
        result.CurrencyCode.Should().Be("USD");
    }

    /// <summary>
    /// Verifies that the Add method of the Money class throws an InvalidOperationException when adding two Money objects with different currencies.
    /// </summary>
    [Fact]
    public void Money_Add_WithDifferentCurrencies_ThrowsInvalidOperationException()
    {
        // Arrange
        var usd = new Money(100m, "USD");
        var eur = new Money(100m, "EUR");

        // Act
        var act = () => usd.Add(eur);

        // Assert
        act.Should().Throw<InvalidOperationException>()
           .WithMessage("*different currencies*");
    }

    /// <summary>
    /// Verifies that the Multiply method of the Money class returns a Money object with an amount of zero when multiplied by zero.
    /// </summary>
    [Fact]
    public void Money_Multiply_ByZero_ReturnsZeroAmount()
    {
        // Arrange
        var price = new Money(99.99m, "USD");

        // Act
        var result = price.Multiply(0m);

        // Assert
        result.Amount.Should().Be(0m);
        result.CurrencyCode.Should().Be("USD");
    }

    /// <summary>
    /// Verifies that the Rating constructor throws an ArgumentException when given a score above 5.
    /// </summary>
    [Fact]
    public void Rating_Constructor_WithScoreAboveFive_ThrowsArgumentException()
    {
        // Arrange & Act
        var act = () => new Rating(6);

        // Assert
        act.Should().Throw<ArgumentException>()
           .WithMessage("*between 1 and 5*");
    }

    /// <summary>
    /// Verifies that the AddReview method of the Rating class increments the total number of reviews.
    /// </summary>
    [Fact]
    public void Rating_AddReview_IncrementsTotalReviews()
    {
        // Arrange
        var rating = new Rating(4, totalReviews: 5);

        // Act
        var updated = rating.AddReview(5);

        // Assert
        updated.TotalReviews.Should().Be(6);
    }

    /// <summary>
    /// Verifies that the Location constructor throws an ArgumentException when given a three-letter country code.
    /// </summary>
    [Fact]
    public void Location_Constructor_WithThreeLetterCountryCode_ThrowsArgumentException()
    {
        // Arrange & Act
        var act = () => new Location("New York", "NY", "USA");

        // Assert
        act.Should().Throw<ArgumentException>()
           .WithMessage("*2-letter ISO*");
    }

    /// <summary>
    /// Verifies that the DistanceTo method of the Location class returns null when the coordinates are not available.
    /// </summary>
    [Fact]
    public void Location_DistanceTo_WithoutCoordinates_ReturnsNull()
    {
        // Arrange
        var london = new Location("London", "England", "GB");
        var paris = new Location("Paris", "Ile-de-France", "FR");

        // Act
        var distance = london.DistanceTo(paris);

        // Assert
        distance.Should().BeNull();
    }
}
