#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using Microsoft.AspNetCore.Mvc;
using MarketplaceEngine.DTOs;
using System.Linq;

namespace MarketplaceEngine.Controllers;

/// <summary>
/// Extension methods for <see cref="MessagesController"/> that provide additional functionality
/// for message management, batch operations, and conversation utilities.
/// </summary>
public static class MessagesControllerExtensions
{
    /// <summary>
    /// Retrieves unread message counts for a user across all conversations.
    /// </summary>
    /// <param name="controller">The messages controller instance</param>
    /// <param name="userId">The user ID to check unread messages for</param>
    /// <returns>Count of unread messages and a list of conversation IDs with unread messages</returns>
    public static async Task<IActionResult> GetUnreadMessageCounts(
        this MessagesController controller,
        Guid userId)
    {
        if (userId == Guid.Empty)
        {
            return new BadRequestObjectResult("User ID is required");
        }

        // Get conversations for the user
        var conversationsResult = await controller.GetConversations(userId);
        if (conversationsResult is not OkObjectResult okResult)
        {
            return conversationsResult;
        }

        var conversations = okResult.Value as List<ConversationDto> ?? new List<ConversationDto>();

        var unreadCount = 0;
        var conversationsWithUnread = new List<Guid>();

        foreach (var conversation in conversations)
        {
            var messagesResult = await controller.GetConversationMessages(
                conversation.ConversationId,
                null,
                100);

            if (messagesResult is OkObjectResult messagesOkResult)
            {
                var paginatedResponse = messagesOkResult.Value as CursorPaginatedResponse<MessageDto>;
                if (paginatedResponse?.Items != null)
                {
                    var unreadMessages = paginatedResponse.Items.Count(m => !m.IsRead);
                    if (unreadMessages > 0)
                    {
                        unreadCount += unreadMessages;
                        conversationsWithUnread.Add(conversation.ConversationId);
                    }
                }
            }
        }

        return new OkObjectResult(new
        {
            TotalUnread = unreadCount,
            ConversationsWithUnread = conversationsWithUnread,
            UserId = userId
        });
    }

    /// <summary>
    /// Marks all messages in a conversation as read for the current user.
    /// </summary>
    /// <param name="controller">The messages controller instance</param>
    /// <param name="conversationId">The conversation ID to mark all messages as read</param>
    /// <returns>Result indicating success or failure</returns>
    public static async Task<IActionResult> MarkConversationAsRead(
        this MessagesController controller,
        Guid conversationId)
    {
        if (conversationId == Guid.Empty)
        {
            return new BadRequestObjectResult("Conversation ID is required");
        }

        // Get all messages in the conversation
        var messagesResult = await controller.GetConversationMessages(conversationId, null, 1000);

        if (messagesResult is not OkObjectResult okResult)
        {
            return messagesResult;
        }

        var paginatedResponse = okResult.Value as CursorPaginatedResponse<MessageDto>;
        if (paginatedResponse?.Items == null)
        {
            return new NotFoundObjectResult("No messages found in conversation");
        }

        var updatedCount = 0;
        foreach (var message in paginatedResponse.Items)
        {
            if (!message.IsRead)
            {
                var markResult = await controller.MarkMessageAsRead(message.Id);
                if (markResult is OkObjectResult)
                {
                    updatedCount++;
                }
            }
        }

        return new OkObjectResult(new
        {
            Message = "Conversation marked as read",
            MessagesUpdated = updatedCount,
            ConversationId = conversationId
        });
    }

    /// <summary>
    /// Sends a batch of messages in a single operation.
    /// Useful for bulk notifications or system messages.
    /// </summary>
    /// <param name="controller">The messages controller instance</param>
    /// <param name="requests">Collection of messages to send</param>
    /// <returns>Collection of created messages</returns>
    public static async Task<IActionResult> SendMessageBatch(
        this MessagesController controller,
        IEnumerable<SendMessageRequest> requests)
    {
        if (requests == null || !requests.Any())
        {
            return new BadRequestObjectResult("At least one message request is required");
        }

        var results = new List<MessageDto>();
        var errors = new List<string>();

        foreach (var request in requests)
        {
            try
            {
                var result = await controller.SendMessage(request);
                if (result is CreatedAtActionResult createdResult)
                {
                    results.Add(createdResult.Value as MessageDto ?? new MessageDto());
                }
            }
            catch (Exception ex)
            {
                errors.Add($"Failed to send message from {request.SenderId} to {request.RecipientId}: {ex.Message}");
            }
        }

        if (errors.Any())
        {
            return new ObjectResult(new
            {
                SuccessCount = results.Count,
                ErrorCount = errors.Count,
                Results = results,
                Errors = errors
            })
            {
                StatusCode = 207 // Multi-Status
            };
        }

        return new OkObjectResult(results);
    }

    /// <summary>
    /// Searches messages across all conversations with optional filters.
    /// </summary>
    /// <param name="controller">The messages controller instance</param>
    /// <param name="userId">The user ID to search messages for</param>
    /// <param name="searchTerm">Optional search term to filter messages</param>
    /// <param name="isRead">Optional filter for read/unread status</param>
    /// <param name="startDate">Optional start date filter</param>
    /// <param name="endDate">Optional end date filter</param>
    /// <param name="pageSize">Number of results to return (default: 50)</param>
    /// <returns>Paginated list of matching messages</returns>
    public static async Task<IActionResult> SearchMessages(
        this MessagesController controller,
        Guid userId,
        string? searchTerm = null,
        bool? isRead = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        int pageSize = 50)
    {
        if (userId == Guid.Empty)
        {
            return new BadRequestObjectResult("User ID is required");
        }

        if (pageSize < 1 || pageSize > 200)
        {
            return new BadRequestObjectResult("Invalid pageSize: must be between 1 and 200");
        }

        // Get all conversations for the user
        var conversationsResult = await controller.GetConversations(userId);
        if (conversationsResult is not OkObjectResult okResult)
        {
            return conversationsResult;
        }

        var conversations = okResult.Value as List<ConversationDto> ?? new List<ConversationDto>();
        var allMessages = new List<MessageDto>();

        // Fetch messages from all conversations
        foreach (var conversation in conversations)
        {
            var messagesResult = await controller.GetConversationMessages(conversation.ConversationId, null, 1000);

            if (messagesResult is OkObjectResult messagesOkResult)
            {
                var paginatedResponse = messagesOkResult.Value as CursorPaginatedResponse<MessageDto>;
                if (paginatedResponse?.Items != null)
                {
                    allMessages.AddRange(paginatedResponse.Items);
                }
            }
        }

        // Apply filters
        var filteredMessages = allMessages.AsEnumerable();

        if (searchTerm != null)
        {
            filteredMessages = filteredMessages.Where(m =>
                m.Content != null && m.Content.Contains(searchTerm, StringComparison.OrdinalIgnoreCase));
        }

        if (isRead.HasValue)
        {
            filteredMessages = filteredMessages.Where(m => m.IsRead == isRead.Value);
        }

        if (startDate.HasValue)
        {
            filteredMessages = filteredMessages.Where(m => m.CreatedAt >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            filteredMessages = filteredMessages.Where(m => m.CreatedAt <= endDate.Value);
        }

        // Apply pagination
        var resultMessages = filteredMessages
            .OrderByDescending(m => m.CreatedAt)
            .Take(pageSize)
            .ToList();

        return new OkObjectResult(new
        {
            TotalFound = allMessages.Count,
            FilteredCount = filteredMessages.Count(),
            ReturnedCount = resultMessages.Count,
            Items = resultMessages,
            PageSize = pageSize
        });
    }
}