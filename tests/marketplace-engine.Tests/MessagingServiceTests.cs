#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FluentAssertions;
using MarketplaceEngine.Domain.Models;
using MarketplaceEngine.Exceptions;
using MarketplaceEngine.Repositories;
using MarketplaceEngine.Services;
using Moq;
using Xunit;

namespace MarketplaceEngine.Tests;

/// <summary>
/// Unit tests for the <see cref="MessagingService"/> class.
/// Tests messaging functionality including sending messages, retrieving messages,
/// marking messages as read/unread, deleting messages, flagging messages,
/// and managing message replies and conversations.
/// </summary>
public class MessagingServiceTests
{
    /// <summary>
    /// Mock repository for message operations used in testing.
    /// </summary>
    private readonly Mock<IMessageRepository> _messageRepoMock;

    /// <summary>
    /// Mock repository for user operations used in testing.
    /// </summary>
    private readonly Mock<IUserRepository> _userRepoMock;

    /// <summary>
    /// System under test - the <see cref="MessagingService"/> instance being tested.
    /// </summary>
    private readonly MessagingService _sut;

    /// <summary>
    /// Initializes a new instance of the <see cref="MessagingServiceTests"/> class.
    /// Sets up mock repositories for testing messaging service functionality.
    /// </summary>
    public MessagingServiceTests()
    {
        _messageRepoMock = new Mock<IMessageRepository>();
        _userRepoMock = new Mock<IUserRepository>();
        _sut = new MessagingService(_messageRepoMock.Object, _userRepoMock.Object);
    }

    /// <summary>
    /// Creates a test user with optional specific ID.
    /// </summary>
    /// <param name="id">Optional user ID. If null, a new GUID will be generated.</param>
    /// <returns>A configured <see cref="User"/> instance for testing.</returns>
    private static User MakeUser(Guid? id = null) => new User
    {
        Id = id ?? Guid.NewGuid(),
        Email = $"{Guid.NewGuid()}@example.com",
        FullName = "Test User",
        IsActive = true
    };

    // ── SendMessageAsync ───────────────────────────────────────────────────

    [Fact]
    /// <summary>
    /// Tests that <see cref="MessagingService.SendMessageAsync"/> throws <see cref="ResourceNotFoundException"/>
    /// when the sender user is not found in the repository.
    /// </summary>
    public async Task SendMessageAsync_WhenSenderNotFound_ThrowsResourceNotFoundException()
    {
        var senderId = Guid.NewGuid();
        var recipientId = Guid.NewGuid();

        _userRepoMock.Setup(r => r.GetByIdAsync(senderId)).ReturnsAsync((User?)null);

        var act = async () => await _sut.SendMessageAsync(
            senderId, recipientId, "Hello", "This is a message body.");

        await act.Should().ThrowAsync<ResourceNotFoundException>().WithMessage("*User*not found*");
    }

    [Fact]
    /// <summary>
    /// Tests that <see cref="MessagingService.SendMessageAsync"/> throws <see cref="ResourceNotFoundException"/>
    /// when the recipient user is not found in the repository.
    /// </summary>
    public async Task SendMessageAsync_WhenRecipientNotFound_ThrowsResourceNotFoundException()
    {
        var sender = MakeUser();
        var recipientId = Guid.NewGuid();

        _userRepoMock.Setup(r => r.GetByIdAsync(sender.Id)).ReturnsAsync(sender);
        _userRepoMock.Setup(r => r.GetByIdAsync(recipientId)).ReturnsAsync((User?)null);

        var act = async () => await _sut.SendMessageAsync(
            sender.Id, recipientId, "Hello", "This is a message body.");

        await act.Should().ThrowAsync<ResourceNotFoundException>().WithMessage("*User*not found*");
    }

    [Fact]
    /// <summary>
    /// Tests that <see cref="MessagingService.SendMessageAsync"/> successfully creates and returns a message
    /// when both sender and recipient are valid users.
    /// </summary>
    public async Task SendMessageAsync_WithValidData_ReturnsCreatedMessage()
    {
        var sender = MakeUser();
        var recipient = MakeUser();

        var expected = new Message
        {
            Id = Guid.NewGuid(),
            SenderId = sender.Id,
            RecipientId = recipient.Id,
            Subject = "Hello",
            Body = "This is a message body."
        };

        _userRepoMock.Setup(r => r.GetByIdAsync(sender.Id)).ReturnsAsync(sender);
        _userRepoMock.Setup(r => r.GetByIdAsync(recipient.Id)).ReturnsAsync(recipient);
        _messageRepoMock.Setup(r => r.AddAsync(It.IsAny<Message>())).ReturnsAsync(expected);

        var result = await _sut.SendMessageAsync(sender.Id, recipient.Id, "Hello", "This is a message body.");

        result.Should().NotBeNull();
        result.Subject.Should().Be("Hello");
        result.SenderId.Should().Be(sender.Id);
    }

    [Fact]
    /// <summary>
    /// Tests that <see cref="MessagingService.SendMessageAsync"/> throws <see cref="ArgumentException"/>
    /// when sender ID equals recipient ID (user trying to message themselves).
    /// </summary>
    public async Task SendMessageAsync_WithSenderEqualToRecipient_ThrowsArgumentException()
    {
        var user = MakeUser();

        _userRepoMock.Setup(r => r.GetByIdAsync(user.Id)).ReturnsAsync(user);

        var act = async () => await _sut.SendMessageAsync(
            user.Id, user.Id, "Hello", "This is a message body.");

        await act.Should().ThrowAsync<ArgumentException>().WithMessage("*yourself*");
    }

    // ── GetReceivedMessagesAsync ───────────────────────────────────────────

    [Fact]
    /// <summary>
    /// Tests that <see cref="MessagingService.GetReceivedMessagesAsync"/> throws <see cref="ResourceNotFoundException"/>
    /// when the user is not found in the repository.
    /// </summary>
    public async Task GetReceivedMessagesAsync_WhenUserNotFound_ThrowsResourceNotFoundException()
    {
        var userId = Guid.NewGuid();
        _userRepoMock.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync((User?)null);

        var act = async () => await _sut.GetReceivedMessagesAsync(userId);

        await act.Should().ThrowAsync<ResourceNotFoundException>();
    }

    [Fact]
    /// <summary>
    /// Tests that <see cref="MessagingService.GetReceivedMessagesAsync"/> successfully returns messages
    /// when the user exists in the repository.
    /// </summary>
    public async Task GetReceivedMessagesAsync_WhenUserExists_ReturnsMessages()
    {
        var user = MakeUser();
        var messages = new List<Message>
        {
            new() { Id = Guid.NewGuid(), SenderId = Guid.NewGuid(), RecipientId = user.Id, Subject = "Hi", Body = "Hello there" }
        };

        _userRepoMock.Setup(r => r.GetByIdAsync(user.Id)).ReturnsAsync(user);
        _messageRepoMock.Setup(r => r.GetReceivedMessagesAsync(user.Id)).ReturnsAsync(messages);

        var result = await _sut.GetReceivedMessagesAsync(user.Id);

        result.Should().HaveCount(1);
        result[0].RecipientId.Should().Be(user.Id);
    }

    // ── MarkAsReadAsync ────────────────────────────────────────────────────

    [Fact]
    /// <summary>
    /// Tests that <see cref="MessagingService.MarkAsReadAsync"/> throws <see cref="ResourceNotFoundException"/>
    /// when the message is not found in the repository.
    /// </summary>
    public async Task MarkAsReadAsync_WhenMessageNotFound_ThrowsResourceNotFoundException()
    {
        var messageId = Guid.NewGuid();
        _messageRepoMock.Setup(r => r.GetByIdAsync(messageId)).ReturnsAsync((Message?)null);

        var act = async () => await _sut.MarkAsReadAsync(messageId);

        await act.Should().ThrowAsync<ResourceNotFoundException>().WithMessage("*Message*not found*");
    }

    [Fact]
    /// <summary>
    /// Tests that <see cref="MessagingService.MarkAsReadAsync"/> successfully marks a message as read
    /// and sets the ReadAt timestamp when the message exists.
    /// </summary>
    public async Task MarkAsReadAsync_WhenMessageExists_MarksMessageAsRead()
    {
        var messageId = Guid.NewGuid();
        var message = new Message
        {
            Id = messageId,
            SenderId = Guid.NewGuid(),
            RecipientId = Guid.NewGuid(),
            Subject = "Test Subject",
            Body = "Test body content",
            IsRead = false
        };

        _messageRepoMock.Setup(r => r.GetByIdAsync(messageId)).ReturnsAsync(message);
        _messageRepoMock.Setup(r => r.UpdateAsync(It.IsAny<Message>())).ReturnsAsync((Message m) => m);

        var result = await _sut.MarkAsReadAsync(messageId);

        result.IsRead.Should().BeTrue();
        result.ReadAt.Should().NotBeNull();
    }

    // ── DeleteMessageAsync ─────────────────────────────────────────────────

    [Fact]
    /// <summary>
    /// Tests that <see cref="MessagingService.DeleteMessageAsync"/> successfully deletes a message
    /// when the requester is the sender of the message.
    /// </summary>
    public async Task DeleteMessageAsync_WhenRequesterIsSender_DeletesSuccessfully()
    {
        var senderId = Guid.NewGuid();
        var messageId = Guid.NewGuid();
        var message = new Message
        {
            Id = messageId,
            SenderId = senderId,
            RecipientId = Guid.NewGuid(),
            Subject = "Delete me",
            Body = "This should be deleted."
        };

        _messageRepoMock.Setup(r => r.GetByIdAsync(messageId)).ReturnsAsync(message);
        _messageRepoMock.Setup(r => r.DeleteAsync(messageId)).Returns(Task.CompletedTask);

        var act = async () => await _sut.DeleteMessageAsync(messageId, senderId);

        await act.Should().NotThrowAsync();
        _messageRepoMock.Verify(r => r.DeleteAsync(messageId), Times.Once);
    }

    [Fact]
    /// <summary>
    /// Tests that <see cref="MessagingService.DeleteMessageAsync"/> successfully deletes a message
    /// when the requester is the recipient of the message.
    /// </summary>
    public async Task DeleteMessageAsync_WhenRequesterIsRecipient_DeletesSuccessfully()
    {
        var recipientId = Guid.NewGuid();
        var messageId = Guid.NewGuid();
        var message = new Message
        {
            Id = messageId,
            SenderId = Guid.NewGuid(),
            RecipientId = recipientId,
            Subject = "Delete me",
            Body = "This should be deleted."
        };

        _messageRepoMock.Setup(r => r.GetByIdAsync(messageId)).ReturnsAsync(message);
        _messageRepoMock.Setup(r => r.DeleteAsync(messageId)).Returns(Task.CompletedTask);

        var act = async () => await _sut.DeleteMessageAsync(messageId, recipientId);

        await act.Should().NotThrowAsync();
    }

    [Fact]
    /// <summary>
    /// Tests that <see cref="MessagingService.DeleteMessageAsync"/> throws <see cref="UnauthorizedException"/>
    /// when the requester is neither the sender nor the recipient of the message.
    /// </summary>
    public async Task DeleteMessageAsync_WhenRequesterIsUnrelated_ThrowsUnauthorizedException()
    {
        var messageId = Guid.NewGuid();
        var unrelatedUserId = Guid.NewGuid();
        var message = new Message
        {
            Id = messageId,
            SenderId = Guid.NewGuid(),
            RecipientId = Guid.NewGuid(),
            Subject = "Delete me",
            Body = "This should be deleted."
        };

        _messageRepoMock.Setup(r => r.GetByIdAsync(messageId)).ReturnsAsync(message);

        var act = async () => await _sut.DeleteMessageAsync(messageId, unrelatedUserId);

        await act.Should().ThrowAsync<UnauthorizedException>();
    }

    // ── FlagMessageAsync ───────────────────────────────────────────────────

    [Fact]
    /// <summary>
    /// Tests that <see cref="MessagingService.FlagMessageAsync"/> successfully sets IsFlagged to true
    /// when the message exists and the flagger is a valid user.
    /// </summary>
    public async Task FlagMessageAsync_WhenMessageExists_SetsIsFlaggedTrue()
    {
        var flaggerId = Guid.NewGuid();
        var messageId = Guid.NewGuid();
        var message = new Message
        {
            Id = messageId,
            SenderId = Guid.NewGuid(),
            RecipientId = flaggerId,
            Subject = "Suspicious",
            Body = "Suspicious content here",
            IsFlagged = false
        };
        var flagger = MakeUser(flaggerId);

        _messageRepoMock.Setup(r => r.GetByIdAsync(messageId)).ReturnsAsync(message);
        _userRepoMock.Setup(r => r.GetByIdAsync(flaggerId)).ReturnsAsync(flagger);
        _messageRepoMock.Setup(r => r.UpdateAsync(It.IsAny<Message>())).ReturnsAsync((Message m) => m);

        var result = await _sut.FlagMessageAsync(messageId, flaggerId);

        result.IsFlagged.Should().BeTrue();
    }

    [Fact]
    /// <summary>
    /// Tests that <see cref="MessagingService.FlagMessageAsync"/> throws <see cref="ResourceNotFoundException"/>
    /// when the flagger user is not found in the repository.
    /// </summary>
    public async Task FlagMessageAsync_WhenFlaggerNotFound_ThrowsResourceNotFoundException()
    {
        var flaggerId = Guid.NewGuid();
        var messageId = Guid.NewGuid();
        var message = new Message
        {
            Id = messageId,
            SenderId = Guid.NewGuid(),
            RecipientId = Guid.NewGuid(),
            Subject = "Subject",
            Body = "Body content"
        };

        _messageRepoMock.Setup(r => r.GetByIdAsync(messageId)).ReturnsAsync(message);
        _userRepoMock.Setup(r => r.GetByIdAsync(flaggerId)).ReturnsAsync((User?)null);

        var act = async () => await _sut.FlagMessageAsync(messageId, flaggerId);

        await act.Should().ThrowAsync<ResourceNotFoundException>();
    }

    // ── AddReplyAsync ──────────────────────────────────────────────────────

    [Fact]
    /// <summary>
    /// Tests that <see cref="MessagingService.AddReplyAsync"/> successfully creates a reply message
    /// with a subject prefixed with "Re: " when the parent message exists.
    /// </summary>
    public async Task AddReplyAsync_WithValidData_CreatesReplyWithPrefixedSubject()
    {
        var parentId = Guid.NewGuid();
        var sender = MakeUser();
        var parentMessage = new Message
        {
            Id = parentId,
            SenderId = Guid.NewGuid(),
            RecipientId = Guid.NewGuid(),
            Subject = "Original subject",
            Body = "Original message body"
        };

        var replyMessage = new Message
        {
            Id = Guid.NewGuid(),
            SenderId = sender.Id,
            RecipientId = parentMessage.SenderId,
            Subject = "Re: Original subject",
            Body = "Reply body here"
        };

        _messageRepoMock.Setup(r => r.GetByIdAsync(parentId)).ReturnsAsync(parentMessage);
        _userRepoMock.Setup(r => r.GetByIdAsync(sender.Id)).ReturnsAsync(sender);
        _messageRepoMock.Setup(r => r.AddAsync(It.IsAny<Message>())).ReturnsAsync(replyMessage);
        _messageRepoMock.Setup(r => r.UpdateAsync(It.IsAny<Message>())).ReturnsAsync(parentMessage);

        var result = await _sut.AddReplyAsync(parentId, sender.Id, "Reply body here");

        result.Should().NotBeNull();
        result.Subject.Should().StartWith("Re: ");
    }

    [Fact]
    /// <summary>
    /// Tests that <see cref="MessagingService.AddReplyAsync"/> throws <see cref="ResourceNotFoundException"/>
    /// when the parent message is not found in the repository.
    /// </summary>
    public async Task AddReplyAsync_WhenParentNotFound_ThrowsResourceNotFoundException()
    {
        var parentId = Guid.NewGuid();
        var senderId = Guid.NewGuid();

        _messageRepoMock.Setup(r => r.GetByIdAsync(parentId)).ReturnsAsync((Message?)null);

        var act = async () => await _sut.AddReplyAsync(parentId, senderId, "Reply body here");

        await act.Should().ThrowAsync<ResourceNotFoundException>().WithMessage("*Message*not found*");
    }

    // ── GetConversationAsync ───────────────────────────────────────────────

    [Fact]
    /// <summary>
    /// Tests that <see cref="MessagingService.GetConversationAsync"/> throws <see cref="ResourceNotFoundException"/>
    /// when the first user is not found in the repository.
    /// </summary>
    public async Task GetConversationAsync_WhenFirstUserNotFound_ThrowsResourceNotFoundException()
    {
        var user1Id = Guid.NewGuid();
        var user2Id = Guid.NewGuid();

        _userRepoMock.Setup(r => r.GetByIdAsync(user1Id)).ReturnsAsync((User?)null);

        var act = async () => await _sut.GetConversationAsync(user1Id, user2Id);

        await act.Should().ThrowAsync<ResourceNotFoundException>();
    }

    // ── MarkAsUnreadAsync ──────────────────────────────────────────────────

    [Fact]
    /// <summary>
    /// Tests that <see cref="MessagingService.MarkAsUnreadAsync"/> successfully sets IsRead to false
    /// and clears the ReadAt timestamp when the message was previously read.
    /// </summary>
    public async Task MarkAsUnreadAsync_WhenMessageWasRead_SetsIsReadFalse()
    {
        var messageId = Guid.NewGuid();
        var message = new Message
        {
            Id = messageId,
            SenderId = Guid.NewGuid(),
            RecipientId = Guid.NewGuid(),
            Subject = "Subject",
            Body = "Body content",
            IsRead = true,
            ReadAt = DateTime.UtcNow
        };

        _messageRepoMock.Setup(r => r.GetByIdAsync(messageId)).ReturnsAsync(message);
        _messageRepoMock.Setup(r => r.UpdateAsync(It.IsAny<Message>())).ReturnsAsync((Message m) => m);

        var result = await _sut.MarkAsUnreadAsync(messageId);

        result.IsRead.Should().BeFalse();
        result.ReadAt.Should().BeNull();
    }
}