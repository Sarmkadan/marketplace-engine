#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Linq;

namespace MarketplaceEngine.DTOs;

/// <summary>
/// Extension methods for MessageDto providing useful functionality for message processing.
/// </summary>
public static class MessageDtoExtensions
{
    /// <summary>
    /// Determines whether the message is sent by the specified user.
    /// </summary>
    /// <param name="message">The message to check</param>
    /// <param name="userId">The user ID to compare against</param>
    /// <returns>True if the message is sent by the specified user; otherwise, false</returns>
    public static bool IsSentBy(this MessageDto message, Guid userId)
    {
        if (message == null)
        {
            throw new ArgumentNullException(nameof(message));
        }

        return message.SenderId == userId;
    }

    /// <summary>
    /// Determines whether the message is received by the specified user.
    /// </summary>
    /// <param name="message">The message to check</param>
    /// <param name="userId">The user ID to compare against</param>
    /// <returns>True if the message is received by the specified user; otherwise, false</returns>
    public static bool IsReceivedBy(this MessageDto message, Guid userId)
    {
        if (message == null)
        {
            throw new ArgumentNullException(nameof(message));
        }

        return message.RecipientId == userId;
    }

    /// <summary>
    /// Gets the other participant's ID in a conversation (either sender or recipient, whichever is not the current user).
    /// </summary>
    /// <param name="message">The message</param>
    /// <param name="currentUserId">The current user's ID</param>
    /// <returns>The ID of the other participant in the conversation</returns>
    public static Guid GetOtherParticipantId(this MessageDto message, Guid currentUserId)
    {
        if (message == null)
        {
            throw new ArgumentNullException(nameof(message));
        }

        return message.SenderId == currentUserId ? message.RecipientId : message.SenderId;
    }

    /// <summary>
    /// Formats the message content for display, truncating if necessary.
    /// </summary>
    /// <param name="message">The message to format</param>
    /// <param name="maxLength">Maximum length of the formatted message (default: 200 characters)</param>
    /// <returns>Formatted message content, truncated if necessary</returns>
    public static string FormatContent(this MessageDto message, int maxLength = 200)
    {
        if (message == null)
        {
            throw new ArgumentNullException(nameof(message));
        }

        if (string.IsNullOrEmpty(message.Content))
        {
            return string.Empty;
        }

        if (message.Content.Length <= maxLength)
        {
            return message.Content;
        }

        return message.Content.Substring(0, maxLength) + "...";
    }

    /// <summary>
    /// Determines whether the message is part of a conversation between two specific users.
    /// </summary>
    /// <param name="message">The message to check</param>
    /// <param name="userId1">First user ID</param>
    /// <param name="userId2">Second user ID</param>
    /// <returns>True if the message is between the two specified users; otherwise, false</returns>
    public static bool IsBetweenUsers(this MessageDto message, Guid userId1, Guid userId2)
    {
        if (message == null)
        {
            throw new ArgumentNullException(nameof(message));
        }

        var participants = new HashSet<Guid> { message.SenderId, message.RecipientId };
        return participants.Contains(userId1) && participants.Contains(userId2);
    }

    /// <summary>
    /// Gets a display name for the message based on the sender.
    /// </summary>
    /// <param name="message">The message</param>
    /// <param name="currentUserId">The current user's ID</param>
    /// <returns>Display name indicating who sent the message</returns>
    public static string GetDisplayName(this MessageDto message, Guid currentUserId)
    {
        if (message == null)
        {
            throw new ArgumentNullException(nameof(message));
        }

        return message.IsSentBy(currentUserId) ? "You" : "Other User";
    }
}