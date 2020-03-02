#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FluentAssertions;
using MarketplaceEngine.Domain.Models;
using Xunit;

namespace MarketplaceEngine.Tests;

public class MessageTests
{
    [Fact]
    public void ValidateBeforeSending_WhenSenderAndRecipientAreTheSame_ThrowsArgumentException()
    {
        // Arrange
        var message = new Message
        {
            SenderId = Guid.NewGuid(),
            RecipientId = Guid.NewGuid(),
            Subject = "Test Subject",
            Body = "Test Body"
        };

        // Act and Assert
        var act = () => message.ValidateBeforeSending();
        act.Should().NotThrow();

        // Update recipient ID to match sender ID
        message.RecipientId = message.SenderId;

        // Act and Assert
        act = () => message.ValidateBeforeSending();
        act.Should().Throw<ArgumentException>().WithMessage("*to yourself*");
    }

    [Fact]
    public void ValidateBeforeSending_WhenSubjectIsTooShort_ThrowsArgumentException()
    {
        // Arrange
        var message = new Message
        {
            SenderId = Guid.NewGuid(),
            RecipientId = Guid.NewGuid(),
            Subject = "Ab",
            Body = "Test Body"
        };

        // Act and Assert
        var act = () => message.ValidateBeforeSending();
        act.Should().Throw<ArgumentException>().WithMessage("*at least 3 characters*");
    }

    [Fact]
    public void ValidateBeforeSending_WhenSubjectIsTooLong_ThrowsArgumentException()
    {
        // Arrange
        var message = new Message
        {
            SenderId = Guid.NewGuid(),
            RecipientId = Guid.NewGuid(),
            Subject = new string('a', 101),
            Body = "Test Body"
        };

        // Act and Assert
        var act = () => message.ValidateBeforeSending();
        act.Should().Throw<ArgumentException>().WithMessage("*cannot exceed 100 characters*");
    }

    [Fact]
    public void ValidateBeforeSending_WhenBodyIsTooShort_ThrowsArgumentException()
    {
        // Arrange
        var message = new Message
        {
            SenderId = Guid.NewGuid(),
            RecipientId = Guid.NewGuid(),
            Subject = "Test Subject",
            Body = "Ab"
        };

        // Act and Assert
        var act = () => message.ValidateBeforeSending();
        act.Should().Throw<ArgumentException>().WithMessage("*at least 5 characters*");
    }

    [Fact]
    public void ValidateBeforeSending_WhenBodyIsTooLong_ThrowsArgumentException()
    {
        // Arrange
        var message = new Message
        {
            SenderId = Guid.NewGuid(),
            RecipientId = Guid.NewGuid(),
            Subject = "Test Subject",
            Body = new string('a', 5001)
        };

        // Act and Assert
        var act = () => message.ValidateBeforeSending();
        act.Should().Throw<ArgumentException>().WithMessage("*cannot exceed 5000 characters*");
    }

    [Fact]
    public void ValidateBeforeSending_WhenTooManyAttachments_ThrowsArgumentException()
    {
        // Arrange
        var message = new Message
        {
            SenderId = Guid.NewGuid(),
            RecipientId = Guid.NewGuid(),
            Subject = "Test Subject",
            Body = "Test Body",
            AttachmentUrls = new List<string>(new[] { "url1", "url2", "url3", "url4", "url5", "url6" })
        };

        // Act and Assert
        var act = () => message.ValidateBeforeSending();
        act.Should().Throw<ArgumentException>().WithMessage("*exceed 5 attachments*");
    }
}
