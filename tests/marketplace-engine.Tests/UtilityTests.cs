#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FluentAssertions;
using MarketplaceEngine.Utilities;
using Xunit;

namespace MarketplaceEngine.Tests;

public class UtilityTests
{
    [Theory]
    [InlineData("user@example.com", true)]
    [InlineData("user.name+tag@sub.domain.co", true)]
    [InlineData("notanemail", false)]
    [InlineData("missing@", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public void IsValidEmail_VariousInputs_ReturnsExpectedResult(string? email, bool expected)
    {
        var result = ValidationUtility.IsValidEmail(email);

        result.Should().Be(expected);
    }

    [Fact]
    public void IsValidPrice_BelowMinimum_ReturnsFalse()
    {
        // Arrange & Act
        var result = ValidationUtility.IsValidPrice(0.001m);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void SanitizeInput_WithNullControlCharacters_RemovesThem()
    {
        // Arrange
        var input = "Hello\x00World\x01Test";

        // Act
        var result = ValidationUtility.SanitizeInput(input);

        // Assert
        result.Should().Be("HelloWorldTest");
    }

    [Fact]
    public void Truncate_WhenTextExceedsMaxLength_TruncatesAndAppendsEllipsis()
    {
        // Arrange
        var text = "This is a very long description for a marketplace listing";

        // Act
        var result = StringUtility.Truncate(text, 20);

        // Assert
        result.Should().HaveLength(20);
        result.Should().EndWith("...");
    }

    [Fact]
    public void ToSlug_WithSpecialCharactersAndSpaces_ReturnsUrlFriendlySlug()
    {
        // Arrange
        var title = "Brand New iPhone 15 Pro! (Sealed)";

        // Act
        var slug = StringUtility.ToSlug(title);

        // Assert
        slug.Should().NotContain(" ");
        slug.Should().NotContain("!");
        slug.Should().NotContain("(");
        slug.Should().MatchRegex("^[a-z0-9-]+$");
    }

    [Fact]
    public void MaskEmail_WithTypicalEmail_MasksLocalPartAndPreservesDomain()
    {
        // Arrange
        var email = "john.doe@example.com";

        // Act
        var masked = StringUtility.MaskEmail(email);

        // Assert
        masked.Should().EndWith("@example.com");
        masked.Should().Contain("*");
    }

    [Fact]
    public void CalculateOffset_ForPage2WithSize10_Returns10()
    {
        // Arrange & Act
        var offset = PaginationUtility.CalculateOffset(2, 10);

        // Assert
        offset.Should().Be(10);
    }

    [Fact]
    public void CalculateTotalPages_WithNonDivisibleTotal_CeilsUp()
    {
        // Arrange & Act
        var totalPages = PaginationUtility.CalculateTotalPages(25, 10);

        // Assert
        totalPages.Should().Be(3);
    }
}
