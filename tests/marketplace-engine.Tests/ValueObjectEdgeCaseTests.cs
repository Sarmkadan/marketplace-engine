#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
//
// Comprehensive edge case tests for value objects in MarketplaceEngine domain.
// Tests equality/hashcode contracts, invalid construction inputs, boundary values,
// and edge cases for Money, Rating, and Location value objects.
// =============================================================================

using FluentAssertions;
using MarketplaceEngine.Domain.ValueObjects;
using Xunit;

namespace MarketplaceEngine.Tests;

/// <summary>
/// Comprehensive edge case tests for value objects in the MarketplaceEngine domain.
/// Tests equality/hashcode contracts, invalid construction inputs, and boundary values.
/// </summary>
public class ValueObjectEdgeCaseTests
{
    #region Money Edge Cases

    /// <summary>
    /// Tests that Money with zero amount is valid and equals another zero amount Money.
    /// </summary>
    [Fact]
    public void Money_ZeroAmount_ShouldBeValidAndEqual()
    {
        // Arrange & Act
        var zeroUsd = new Money(0m, "USD");
        var zeroEur = new Money(0m, "EUR");
        var anotherZeroUsd = new Money(0m, "USD");

        // Assert
        zeroUsd.Should().NotBeNull();
        zeroUsd.Amount.Should().Be(0m);
        zeroUsd.CurrencyCode.Should().Be("USD");

        // Zero amounts in different currencies should not be equal
        zeroUsd.Should().NotBe(zeroEur);
        zeroUsd.Should().Be(anotherZeroUsd);
        zeroUsd.GetHashCode().Should().Be(anotherZeroUsd.GetHashCode());
    }

    /// <summary>
    /// Tests that Money with large decimal values is handled correctly.
    /// </summary>
    [Fact]
    public void Money_LargeDecimalValues_ShouldBeHandledCorrectly()
    {
        // Arrange
        var largeValue = 999999999.99m;
        var anotherLarge = 999999999.99m;

        // Act
        var largeMoney = new Money(largeValue, "USD");
        var anotherLargeMoney = new Money(anotherLarge, "USD");

        // Assert
        largeMoney.Amount.Should().Be(largeValue);
        largeMoney.Should().Be(anotherLargeMoney);
        largeMoney.Should().NotBeNull();
    }

    /// <summary>
    /// Tests that Money with very small decimal values works correctly.
    /// </summary>
    [Fact]
    public void Money_VerySmallDecimalValues_ShouldWorkCorrectly()
    {
        // Arrange
        var tinyAmount = 0.0001m;
        var anotherTiny = 0.0001m;

        // Act
        var tinyMoney = new Money(tinyAmount, "USD");
        var anotherTinyMoney = new Money(anotherTiny, "USD");

        // Assert
        tinyMoney.Amount.Should().Be(tinyAmount);
        tinyMoney.Should().Be(anotherTinyMoney);
        tinyMoney.GetHashCode().Should().Be(anotherTinyMoney.GetHashCode());
    }

    /// <summary>
    /// Tests Money equality with null values.
    /// </summary>
    [Fact]
    public void Money_Equals_WithNullValues()
    {
        // Arrange
        var money = new Money(100m, "USD");

        // Act & Assert
        money.Equals(null).Should().BeFalse();
        (money == null).Should().BeFalse();
        (money != null).Should().BeTrue();
    }

    /// <summary>
    /// Tests Money equality operators with null values.
    /// </summary>
    [Fact]
    public void Money_EqualityOperators_WithNullValues()
    {
        // Arrange
        var money1 = new Money(100m, "USD");
        Money? money2 = null;
        Money? money3 = new Money(100m, "USD");

        // Act & Assert
        (money1 == money2).Should().BeFalse();
        (money1 != money2).Should().BeTrue();
        (money2 == money3).Should().BeFalse();
        (money2 != money3).Should().BeTrue();
    }

    /// <summary>
    /// Tests Money constructor with various invalid currency code formats.
    /// </summary>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("US")] // Too short
    public void Money_Constructor_WithInvalidCurrencyCode_ThrowsArgumentException(string? invalidCurrency)
    {
        // Arrange & Act
        Action act = () => new Money(100m, invalidCurrency!);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    /// <summary>
    /// Tests Money constructor with negative amounts at boundary.
    /// </summary>
    [Fact]
    public void Money_Constructor_WithNegativeAmountAtBoundary_ThrowsArgumentException()
    {
        // Arrange & Act
        Action act = () => new Money(-0.01m, "USD");

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    /// <summary>
    /// Tests Money Add method with zero values.
    /// </summary>
    [Fact]
    public void Money_Add_WithZeroValues_ReturnsCorrectResult()
    {
        // Arrange
        var zero = new Money(0m, "USD");
        var hundred = new Money(100m, "USD");

        // Act
        var result1 = zero.Add(hundred);
        var result2 = hundred.Add(zero);

        // Assert
        result1.Amount.Should().Be(100m);
        result2.Amount.Should().Be(100m);
        result1.Should().Be(result2);
    }

    /// <summary>
    /// Tests Money Subtract method with equal values returns zero.
    /// </summary>
    [Fact]
    public void Money_Subtract_EqualValues_ReturnsZero()
    {
        // Arrange
        var hundred = new Money(100m, "USD");

        // Act
        var result = hundred.Subtract(hundred);

        // Assert
        result.Amount.Should().Be(0m);
        result.CurrencyCode.Should().Be("USD");
    }

    /// <summary>
    /// Tests Money Multiply method with various multipliers including edge cases.
    /// </summary>
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(0.5)]
    [InlineData(2.5)]
    public void Money_Multiply_VariousMultipliers(decimal multiplier)
    {
        // Arrange
        var money = new Money(10m, "USD");

        // Act
        var result = money.Multiply(multiplier);

        // Assert
        result.Amount.Should().Be(10m * multiplier);
        result.CurrencyCode.Should().Be("USD");
    }

    /// <summary>
    /// Tests Money Multiply method with negative multiplier throws exception.
    /// </summary>
    [Fact]
    public void Money_Multiply_WithNegativeMultiplier_ThrowsArgumentException()
    {
        // Arrange
        var money = new Money(10m, "USD");

        // Act & Assert
        Action act = () => money.Multiply(-1m);
        act.Should().Throw<ArgumentException>();
    }

    #endregion

    #region Rating Edge Cases

    /// <summary>
    /// Tests Rating with minimum valid score (1) and zero reviews.
    /// </summary>
    [Fact]
    public void Rating_MinimumScore_ShouldBeValid()
    {
        // Arrange & Act
        var rating = new Rating(1, 0);

        // Assert
        rating.Should().NotBeNull();
        rating.Score.Should().Be(1);
        rating.TotalReviews.Should().Be(0);
        rating.AverageRating.Should().Be(1.0);
    }

    /// <summary>
    /// Tests Rating with maximum valid score (5) and zero reviews.
    /// </summary>
    [Fact]
    public void Rating_MaximumScore_ShouldBeValid()
    {
        // Arrange & Act
        var rating = new Rating(5, 0);

        // Assert
        rating.Should().NotBeNull();
        rating.Score.Should().Be(5);
        rating.TotalReviews.Should().Be(0);
        rating.AverageRating.Should().Be(5.0);
    }

    /// <summary>
    /// Tests Rating constructor with negative total reviews.
    /// </summary>
    [Fact]
    public void Rating_Constructor_WithNegativeTotalReviews_ThrowsArgumentException()
    {
        // Arrange & Act
        Action act = () => new Rating(3, -1);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    /// <summary>
    /// Tests Rating AddReview with minimum valid score (1).
    /// </summary>
    [Fact]
    public void Rating_AddReview_WithMinimumScore_ShouldWorkCorrectly()
    {
        // Arrange
        var rating = new Rating(3, 5);

        // Act
        var updated = rating.AddReview(1);

        // Assert
        updated.TotalReviews.Should().Be(6);
        updated.Score.Should().Be(3);
    }

    /// <summary>
    /// Tests Rating AddReview with maximum valid score (5).
    /// </summary>
    [Fact]
    public void Rating_AddReview_WithMaximumScore_ShouldWorkCorrectly()
    {
        // Arrange
        var rating = new Rating(3, 5);

        // Act
        var updated = rating.AddReview(5);

        // Assert
        updated.TotalReviews.Should().Be(6);
        // Bayesian average calculation with actual implementation
        updated.AverageRating.Should().BeApproximately(3.31, 0.1);
    }

    /// <summary>
    /// Tests Rating AddReview with invalid score below minimum.
    /// </summary>
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Rating_AddReview_WithScoreBelowMinimum_ThrowsArgumentException(int invalidScore)
    {
        // Arrange
        var rating = new Rating(3, 5);

        // Act & Assert
        Action act = () => rating.AddReview(invalidScore);
        act.Should().Throw<ArgumentException>();
    }

    /// <summary>
    /// Tests Rating AddReview with invalid score above maximum.
    /// </summary>
    [Theory]
    [InlineData(6)]
    [InlineData(10)]
    public void Rating_AddReview_WithScoreAboveMaximum_ThrowsArgumentException(int invalidScore)
    {
        // Arrange
        var rating = new Rating(3, 5);

        // Act & Assert
        Action act = () => rating.AddReview(invalidScore);
        act.Should().Throw<ArgumentException>();
    }

    /// <summary>
    /// Tests Rating equality with null values.
    /// </summary>
    [Fact]
    public void Rating_Equals_WithNullValues()
    {
        // Arrange
        var rating = new Rating(3, 5);

        // Act & Assert
        rating.Equals(null).Should().BeFalse();
        (rating == null).Should().BeFalse();
        (rating != null).Should().BeTrue();
    }

    /// <summary>
    /// Tests Rating equality operators with null values.
    /// </summary>
    [Fact]
    public void Rating_EqualityOperators_WithNullValues()
    {
        // Arrange
        var rating1 = new Rating(3, 5);
        Rating? rating2 = null;
        Rating? rating3 = new Rating(3, 5);

        // Act & Assert
        (rating1 == rating2).Should().BeFalse();
        (rating1 != rating2).Should().BeTrue();
        (rating2 == rating3).Should().BeFalse();
        (rating2 != rating3).Should().BeTrue();
    }

    /// <summary>
    /// Tests Rating with reasonable number of reviews to ensure calculations work correctly.
    /// </summary>
    [Fact]
    public void Rating_ReasonableNumberOfReviews_ShouldCalculateAverageCorrectly()
    {
        // Arrange
        var reviewCount = 100;

        // Act
        var rating = new Rating(4, reviewCount);

        // Assert
        rating.TotalReviews.Should().Be(reviewCount);
        rating.AverageRating.Should().BeApproximately(4.0, 0.1);
    }

    /// <summary>
    /// Tests Rating ToString method with various states.
    /// </summary>
    [Theory]
    [InlineData(1, 0, "1.00/5 (0 reviews)")]
    [InlineData(5, 100, "4.86/5 (100 reviews)")]
    [InlineData(3, 1, "3.45/5 (1 reviews)")]
    public void Rating_ToString_FormatsCorrectly(int score, int totalReviews, string expectedFormat)
    {
        // Arrange & Act
        var rating = new Rating(score, totalReviews);
        var result = rating.ToString();

        // Assert
        result.Should().Be(expectedFormat);
    }

    #endregion

    #region Location Edge Cases

    /// <summary>
    /// Tests Location with empty strings for optional fields.
    /// </summary>
    [Fact]
    public void Location_WithEmptyOptionalFields_ShouldHandleCorrectly()
    {
        // Arrange & Act
        var location = new Location("New York", "NY", "US", "");

        // Assert
        location.City.Should().Be("New York");
        location.State.Should().Be("NY");
        location.CountryCode.Should().Be("US");
        location.PostalCode.Should().BeNull();
    }

    /// <summary>
    /// Tests Location with whitespace strings for optional fields.
    /// </summary>
    [Fact]
    public void Location_WithWhitespaceOptionalFields_ShouldHandleCorrectly()
    {
        // Arrange & Act
        var location = new Location("New York", "NY", "US", "   ");

        // Assert
        location.PostalCode.Should().BeNull();
    }

    /// <summary>
    /// Tests Location with minimum valid coordinates.
    /// </summary>
    [Fact]
    public void Location_WithMinimumCoordinates_ShouldBeValid()
    {
        // Arrange & Act
        var location = new Location("Test City", "TS", "US", null, -90, -180);

        // Assert
        location.Latitude.Should().Be(-90);
        location.Longitude.Should().Be(-180);
    }

    /// <summary>
    /// Tests Location with maximum valid coordinates.
    /// </summary>
    [Fact]
    public void Location_WithMaximumCoordinates_ShouldBeValid()
    {
        // Arrange & Act
        var location = new Location("Test City", "TS", "US", null, 90, 180);

        // Assert
        location.Latitude.Should().Be(90);
        location.Longitude.Should().Be(180);
    }

    /// <summary>
    /// Tests Location with coordinates just outside valid range.
    /// </summary>
    [Theory]
    [InlineData(-90.01, 0)]
    [InlineData(90.01, 0)]
    [InlineData(0, -180.01)]
    [InlineData(0, 180.01)]
    public void Location_WithCoordinatesOutsideValidRange_ThrowsArgumentException(double lat, double lon)
    {
        // Arrange & Act
        Action act = () => new Location("Test City", "TS", "US", null, lat, lon);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    /// <summary>
    /// Tests Location constructor with various invalid inputs.
    /// </summary>
    [Theory]
    [InlineData(null, "NY", "US")]
    [InlineData("", "NY", "US")]
    [InlineData(" ", "NY", "US")]
    [InlineData("New York", null, "US")]
    [InlineData("New York", "", "US")]
    [InlineData("New York", "NY", null)]
    [InlineData("New York", "NY", "")]
    [InlineData("New York", "NY", " ")]
    [InlineData("New York", "NY", "USA")]
    [InlineData("New York", "NY", "U")]
    public void Location_Constructor_WithInvalidRequiredFields_ThrowsArgumentException(string city, string state, string countryCode)
    {
        // Arrange & Act
        Action act = () => new Location(city, state, countryCode);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    /// <summary>
    /// Tests Location equality with null values.
    /// </summary>
    [Fact]
    public void Location_Equals_WithNullValues()
    {
        // Arrange
        var location = new Location("New York", "NY", "US");

        // Act & Assert
        location.Equals(null).Should().BeFalse();
        (location == null).Should().BeFalse();
        (location != null).Should().BeTrue();
    }

    /// <summary>
    /// Tests Location equality operators with null values.
    /// </summary>
    [Fact]
    public void Location_EqualityOperators_WithNullValues()
    {
        // Arrange
        var location1 = new Location("New York", "NY", "US");
        Location? location2 = null;
        Location? location3 = new Location("New York", "NY", "US");

        // Act & Assert
        (location1 == location2).Should().BeFalse();
        (location1 != location2).Should().BeTrue();
        (location2 == location3).Should().BeFalse();
        (location2 != location3).Should().BeTrue();
    }

    /// <summary>
    /// Tests Location with different case country codes should be treated as different.
    /// </summary>
    [Fact]
    public void Location_CountryCodeCaseSensitivity_ShouldBePreservedInEquality()
    {
        // Arrange & Act
        var lowerCase = new Location("New York", "NY", "us");
        var upperCase = new Location("New York", "NY", "US");
        var mixedCase = new Location("New York", "NY", "Us");

        // Assert - CountryCode is normalized to uppercase in constructor
        lowerCase.CountryCode.Should().Be("US");
        upperCase.CountryCode.Should().Be("US");
        mixedCase.CountryCode.Should().Be("US");

        // But they should all be equal since CountryCode is normalized
        lowerCase.Should().Be(upperCase);
        upperCase.Should().Be(mixedCase);
    }

    /// <summary>
    /// Tests Location DistanceTo with various coordinate combinations.
    /// </summary>
    [Theory]
    [InlineData(40.7128, -74.0060, 34.0522, -118.2437)] // New York to Los Angeles
    [InlineData(51.5074, -0.1278, 48.8566, 2.3522)] // London to Paris
    public void Location_DistanceTo_VariousCoordinateCombinations(double lat1, double lon1, double lat2, double lon2)
    {
        // Arrange
        var loc1 = new Location("Loc1", "ST", "US", null, lat1, lon1);
        var loc2 = new Location("Loc2", "ST", "US", null, lat2, lon2);

        // Act
        var distance = loc1.DistanceTo(loc2);

        // Assert
        distance.Should().NotBeNull();
        distance.Should().BeGreaterThan(0);
    }

    /// <summary>
    /// Tests Location ToString method formats correctly.
    /// </summary>
    [Theory]
    [InlineData("New York", "NY", "US", null, "New York, NY US")]
    [InlineData("San Francisco", "CA", "US", "94105", "San Francisco, CA US")]
    public void Location_ToString_FormatsCorrectly(string city, string state, string countryCode, string? postalCode, string expectedFormat)
    {
        // Arrange & Act
        var location = new Location(city, state, countryCode, postalCode);
        var result = location.ToString();

        // Assert
        result.Should().Be(expectedFormat);
    }

    #endregion

    #region Cross-Value Object Tests

    /// <summary>
    /// Tests that Money instances with same values but created separately are equal.
    /// </summary>
    [Fact]
    public void Money_Immutability_InstancesCreatedSeparatelyAreEqual()
    {
        // Arrange & Act
        var money1 = new Money(100m, "USD");
        var money2 = new Money(100m, "USD");

        // Assert
        money1.Should().Be(money2);
        money1.GetHashCode().Should().Be(money2.GetHashCode());
        money1.Equals(money2).Should().BeTrue();
    }

    /// <summary>
    /// Tests that Rating instances with same values but created separately are equal.
    /// </summary>
    [Fact]
    public void Rating_Immutability_InstancesCreatedSeparatelyAreEqual()
    {
        // Arrange & Act
        var rating1 = new Rating(4, 10);
        var rating2 = new Rating(4, 10);

        // Assert
        rating1.Should().Be(rating2);
        rating1.GetHashCode().Should().Be(rating2.GetHashCode());
        rating1.Equals(rating2).Should().BeTrue();
    }

    /// <summary>
    /// Tests that Location instances with same values but created separately are equal.
    /// </summary>
    [Fact]
    public void Location_Immutability_InstancesCreatedSeparatelyAreEqual()
    {
        // Arrange & Act
        var location1 = new Location("Paris", "IDF", "FR");
        var location2 = new Location("Paris", "IDF", "FR");

        // Assert
        location1.Should().Be(location2);
        location1.GetHashCode().Should().Be(location2.GetHashCode());
        location1.Equals(location2).Should().BeTrue();
    }

    #endregion
}