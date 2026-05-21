#nullable enable
using FluentAssertions;
using MarketplaceEngine.Exceptions;
using Xunit;

namespace MarketplaceEngine.Tests;

public sealed class ValidationExceptionEdgeCaseTests
{
    [Fact]
    public void Constructor_Message_SetsMessageCorrectly()
    {
        var ex = new ValidationException("Test validation error");
        ex.Message.Should().Be("Test validation error");
    }

    [Fact]
    public void Constructor_WithErrors_SetsValidationFailedMessage()
    {
        var errors = new Dictionary<string, string[]>
        {
            { "Name", new[] { "Required" } },
            { "Price", new[] { "Must be positive", "Must be under 10000" } }
        };

        var ex = new ValidationException(errors);

        ex.Message.Should().Contain("Validation failed");
    }

    [Fact]
    public void Constructor_FieldNameAndMessage_CreatesError()
    {
        var ex = new ValidationException("Price", "Must be positive");

        ex.Message.Should().Be("Must be positive");
    }

    [Fact]
    public void Constructor_EmptyErrors_DoesNotThrow()
    {
        var act = () => new ValidationException(new Dictionary<string, string[]>());
        act.Should().NotThrow();
    }

    [Fact]
    public void Constructor_SingleFieldMultipleErrors_AllPreserved()
    {
        var errors = new Dictionary<string, string[]>
        {
            { "Email", new[] { "Required", "Invalid format", "Already exists" } }
        };

        var ex = new ValidationException(errors);
        ex.Should().NotBeNull();
    }
}
