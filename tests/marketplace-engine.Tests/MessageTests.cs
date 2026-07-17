#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FluentAssertions;
using MarketplaceEngine.Domain.Models;
using Xunit;

namespace MarketplaceEngine.Tests;

/// <summary>
/// Contains unit tests for the <see cref="Message"/> class validation logic.
/// </summary>
public class MessageTests
{
    /// <summary>
    /// Tests that <see cref="Message.ValidateBeforeSending"/> throws an <see cref="ArgumentException"/> when sender and recipient are the same.
    /// </summary>
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

    /// <summary>
    /// Tests that <see cref="Message.ValidateBeforeSending"/> throws an <see cref="ArgumentException"/> when subject is too short.
    /// </summary>
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

    /// <summary>
    /// Tests that <see cref="Message.ValidateBeforeSending"/> throws an <see cref="ArgumentException"/> when subject exceeds maximum length.
    /// </summary>
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

    /// <summary>
    /// Tests that <see cref="Message.ValidateBeforeSending"/> throws an <see cref="ArgumentException"/> when body is too short.
    /// </summary>
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

    /// <summary>
    /// Tests that <see cref="Message.ValidateBeforeSending"/> throws an <see cref="ArgumentException"/> when body exceeds maximum length.
    /// </summary>
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

    /// <summary>
    /// Tests that <see cref="Message.ValidateBeforeSending"/> throws an <see cref="ArgumentException"/> when message has too many attachments.
    /// </summary>
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
