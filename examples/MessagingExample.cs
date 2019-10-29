#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using MarketplaceEngine.Services;
using MarketplaceEngine.Domain.Models;
using MarketplaceEngine.Domain.ValueObjects;
using MarketplaceEngine.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MarketplaceEngine.Examples;

/// <summary>
/// Demonstrates user-to-user messaging capabilities.
/// This example shows how to:
/// - Send messages
/// - Retrieve conversations
/// - Get message history
/// - Mark messages as read
/// </summary>
public class MessagingExample
{
    public static async Task Main(string[] args)
    {
        var services = new ServiceCollection();
        services.AddMarketplaceServices();
        var provider = services.BuildServiceProvider();

        var messagingService = provider.GetRequiredService<MessagingService>();
        var listingService = provider.GetRequiredService<ListingService>();
        var userService = provider.GetRequiredService<UserService>();

        Console.WriteLine("=== Marketplace Engine - Messaging Example ===\n");

        try
        {
            // Set up test data
            Console.WriteLine("Setting up test data...");
            var listing = await listingService.CreateListingAsync(
                sellerId: 1,
                title: "iPhone 14 Pro",
                description: "Excellent condition",
                price: new Money(999.99m, "USD"),
                category: "Electronics",
                tags: new[] { "phone", "apple" },
                location: new Location { City = "New York", Country = "USA" }
            );
            Console.WriteLine("✓ Test data created\n");

            // Example 1: Send a message
            Console.WriteLine("1. Sending a message from user 2 to user 1...");
            var message1 = await messagingService.SendMessageAsync(
                senderId: 2,
                recipientId: 1,
                content: "Hi, is the iPhone still available?",
                listingId: listing.Id
            );
            Console.WriteLine($"✓ Message sent:");
            Console.WriteLine($"  ID: {message1.Id}");
            Console.WriteLine($"  From: User {message1.SenderId}");
            Console.WriteLine($"  To: User {message1.RecipientId}");
            Console.WriteLine($"  Content: {message1.Content}");
            Console.WriteLine($"  Time: {message1.CreatedAt}\n");

            // Example 2: Send a reply
            Console.WriteLine("2. Sending a reply from user 1 to user 2...");
            var message2 = await messagingService.SendMessageAsync(
                senderId: 1,
                recipientId: 2,
                content: "Yes, it's still available! Interested?",
                listingId: listing.Id
            );
            Console.WriteLine($"✓ Reply sent: \"{message2.Content}\"\n");

            // Example 3: Continue conversation
            Console.WriteLine("3. Continuing the conversation...");
            var message3 = await messagingService.SendMessageAsync(
                senderId: 2,
                recipientId: 1,
                content: "Yes! What's your lowest price?",
                listingId: listing.Id
            );
            Console.WriteLine($"✓ Message sent\n");

            var message4 = await messagingService.SendMessageAsync(
                senderId: 1,
                recipientId: 2,
                content: "I can do $950. Condition is mint.",
                listingId: listing.Id
            );
            Console.WriteLine($"✓ Message sent\n");

            // Example 4: Get conversation between two users
            Console.WriteLine("4. Retrieving conversation between user 1 and user 2...");
            var conversation = await messagingService.GetConversationAsync(userId1: 1, userId2: 2).ConfigureAwait(false);
            Console.WriteLine($"✓ Conversation retrieved:");
            Console.WriteLine($"  Total messages: {conversation.Count}");
            foreach (var msg in conversation)
            {
                var senderLabel = msg.SenderId == 1 ? "Seller" : "Buyer";
                Console.WriteLine($"  [{senderLabel}] {msg.Content}");
                Console.WriteLine($"           Sent: {msg.CreatedAt}");
            }
            Console.WriteLine();

            // Example 5: Get user's recent conversations
            Console.WriteLine("5. Getting recent conversations for user 1...");
            var conversations = await messagingService.GetUserConversationsAsync(userId: 1, pageSize: 10, pageNumber: 1).ConfigureAwait(false);
            Console.WriteLine($"✓ User 1 has {conversations.Count} conversation(s):");
            foreach (var conv in conversations)
            {
                Console.WriteLine($"  - With user {conv.OtherUserId}: {conv.MessageCount} messages");
                Console.WriteLine($"    Last message: {conv.LastMessageContent}");
            }
            Console.WriteLine();

            // Example 6: Get conversations for user 2
            Console.WriteLine("6. Getting conversations for user 2...");
            var user2Conversations = await messagingService.GetUserConversationsAsync(userId: 2, pageSize: 10, pageNumber: 1).ConfigureAwait(false);
            Console.WriteLine($"✓ User 2 has {user2Conversations.Count} conversation(s):");
            foreach (var conv in user2Conversations)
            {
                Console.WriteLine($"  - With user {conv.OtherUserId}: {conv.MessageCount} messages");
            }
            Console.WriteLine();

            // Example 7: Get unread message count
            Console.WriteLine("7. Checking unread messages for user 1...");
            var unreadCount = await messagingService.GetUnreadMessageCountAsync(userId: 1).ConfigureAwait(false);
            Console.WriteLine($"✓ User 1 has {unreadCount} unread message(s)\n");

            // Example 8: Mark messages as read
            Console.WriteLine("8. Marking messages as read...");
            var unreadMessages = conversation.Where(m => m.SenderId != 1 && !m.IsRead).ToList();
            foreach (var msg in unreadMessages)
            {
                msg.MarkAsRead();
                Console.WriteLine($"✓ Message {msg.Id} marked as read");
            }
            Console.WriteLine();

            // Example 9: Send message without listing reference
            Console.WriteLine("9. Sending a general message (not listing-specific)...");
            var generalMessage = await messagingService.SendMessageAsync(
                senderId: 2,
                recipientId: 1,
                content: "Thanks for the great service!",
                listingId: null
            );
            Console.WriteLine($"✓ Message sent\n");

            // Example 10: Message statistics
            Console.WriteLine("10. Message statistics...");
            var totalMessages = conversation.Count + 1; // Including general message
            var user1Messages = conversation.Count(m => m.SenderId == 1) + 1;
            var user2Messages = conversation.Count(m => m.SenderId == 2);
            Console.WriteLine($"✓ Total messages: {totalMessages}");
            Console.WriteLine($"  From user 1 (seller): {user1Messages}");
            Console.WriteLine($"  From user 2 (buyer): {user2Messages}\n");

            Console.WriteLine("=== Example completed successfully ===");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ Error: {ex.Message}");
        }
    }
}
