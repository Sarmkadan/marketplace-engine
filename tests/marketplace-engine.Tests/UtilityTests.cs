#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// ===========================================================================

using FluentAssertions;
using MarketplaceEngine.Utilities;
using Xunit;

namespace MarketplaceEngine.Tests;

/// <summary>
/// Represents test data and configurations for utility class testing.
/// This class holds input values used by the UtilityTests test methods.
/// </summary>
public class UtilityTests
{
    /// <summary>
    /// Email address used for IsValidEmail_VariousInputs_ReturnsExpectedResult tests.
    /// </summary>
    public string? EmailForIsValidEmailTest { get; set; }

    /// <summary>
    /// Price value used for IsValidPrice_BelowMinimum_ReturnsFalse tests.
    /// </summary>
    public decimal PriceForIsValidPriceTest { get; set; }

    /// <summary>
    /// Input string used for SanitizeInput_WithNullControlCharacters_RemovesThem tests.
    /// </summary>
    public string? InputForSanitizeTest { get; set; }

    /// <summary>
    /// Text used for Truncate_WhenTextExceedsMaxLength_TruncatesAndAppendsEllipsis tests.
    /// </summary>
    public string? TextForTruncateTest { get; set; }

    /// <summary>
    /// Title used for ToSlug_WithSpecialCharactersAndSpaces_ReturnsUrlFriendlySlug tests.
    /// </summary>
    public string? TitleForToSlugTest { get; set; }

    /// <summary>
    /// Email address used for MaskEmail_WithTypicalEmail_MasksLocalPartAndPreservesDomain tests.
    /// </summary>
    public string? EmailForMaskEmailTest { get; set; }

    /// <summary>
    /// Page number used for CalculateOffset_ForPage2WithSize10_Returns10 tests.
    /// </summary>
    public int PageForCalculateOffsetTest { get; set; }

    /// <summary>
    /// Page size used for CalculateOffset_ForPage2WithSize10_Returns10 tests.
    /// </summary>
    public int PageSizeForCalculateOffsetTest { get; set; }

    /// <summary>
    /// Total items used for CalculateTotalPages_WithNonDivisibleTotal_CeilsUp tests.
    /// </summary>
    public int TotalItemsForCalculateTotalPagesTest { get; set; }

    /// <summary>
    /// Page size used for CalculateTotalPages_WithNonDivisibleTotal_CeilsUp tests.
    /// </summary>
    public int PageSizeForCalculateTotalPagesTest { get; set; }

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
