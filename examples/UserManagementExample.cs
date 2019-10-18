// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using MarketplaceEngine.Services;
using MarketplaceEngine.Domain.Models;
using MarketplaceEngine.Domain.Enums;
using MarketplaceEngine.Domain.ValueObjects;
using MarketplaceEngine.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MarketplaceEngine.Examples;

/// <summary>
/// Demonstrates user management capabilities.
/// This example shows how to:
/// - Register new users
/// - Retrieve user profiles
/// - Update user roles
/// - View user ratings
/// - Get top sellers
/// - Manage user status
/// </summary>
public class UserManagementExample
{
    public static async Task Main(string[] args)
    {
        var services = new ServiceCollection();
        services.AddMarketplaceServices();
        var provider = services.BuildServiceProvider();

        var userService = provider.GetRequiredService<UserService>();
        var listingService = provider.GetRequiredService<ListingService>();

        Console.WriteLine("=== Marketplace Engine - User Management Example ===\n");

        try
        {
            // Example 1: Register multiple users
            Console.WriteLine("1. Registering new users...");
            var seller = await userService.RegisterUserAsync(
                email: "seller@example.com",
                username: "john_seller",
                fullName: "John Smith",
                password: "SecurePassword123!"
            );
            Console.WriteLine($"✓ Seller registered: {seller.FullName} ({seller.Email})\n");

            var buyer = await userService.RegisterUserAsync(
                email: "buyer@example.com",
                username: "jane_buyer",
                fullName: "Jane Doe",
                password: "SecurePassword123!"
            );
            Console.WriteLine($"✓ Buyer registered: {buyer.FullName} ({buyer.Email})\n");

            var moderator = await userService.RegisterUserAsync(
                email: "moderator@example.com",
                username: "mod_user",
                fullName: "Moderator User",
                password: "SecurePassword123!"
            );
            Console.WriteLine($"✓ Moderator registered: {moderator.FullName}\n");

            // Example 2: Get user details
            Console.WriteLine("2. Retrieving user details...");
            var userDetails = await userService.GetUserAsync(seller.Id);
            if (userDetails != null)
            {
                Console.WriteLine($"✓ User Details:");
                Console.WriteLine($"  ID: {userDetails.Id}");
                Console.WriteLine($"  Username: {userDetails.Username}");
                Console.WriteLine($"  Email: {userDetails.Email}");
                Console.WriteLine($"  Full Name: {userDetails.FullName}");
                Console.WriteLine($"  Role: {userDetails.Role}");
                Console.WriteLine($"  Status: {userDetails.IsActive}");
                Console.WriteLine($"  Joined: {userDetails.CreatedAt}\n");
            }

            // Example 3: Update user role
            Console.WriteLine("3. Promoting user to Premium Seller...");
            await userService.UpdateUserRoleAsync(seller.Id, UserRole.PremiumSeller);
            var updatedUser = await userService.GetUserAsync(seller.Id);
            Console.WriteLine($"✓ New role: {updatedUser?.Role}\n");

            // Example 4: Set moderator role
            Console.WriteLine("4. Setting moderator role...");
            await userService.UpdateUserRoleAsync(moderator.Id, UserRole.Moderator);
            var moderatorUser = await userService.GetUserAsync(moderator.Id);
            Console.WriteLine($"✓ Moderator role set: {moderatorUser?.Role}\n");

            // Example 5: Create listings to build seller rating
            Console.WriteLine("5. Creating listings to build seller reputation...");
            for (int i = 1; i <= 3; i++)
            {
                await listingService.CreateListingAsync(
                    sellerId: seller.Id,
                    title: $"Premium Item #{i}",
                    description: "Quality product in excellent condition",
                    price: new Money(99.99m * i, "USD"),
                    category: "Electronics",
                    tags: new[] { "quality", "verified" },
                    location: new Location { City = "New York", Country = "USA" }
                );
            }
            Console.WriteLine($"✓ Created 3 listings for the seller\n");

            // Example 6: Simulate user ratings
            Console.WriteLine("6. Adding ratings to user...");
            var sellerWithRatings = await userService.GetUserAsync(seller.Id);
            if (sellerWithRatings != null)
            {
                Console.WriteLine($"✓ Seller Ratings:");
                Console.WriteLine($"  Average Rating: {sellerWithRatings.Rating.AverageRating}");
                Console.WriteLine($"  Total Reviews: {sellerWithRatings.Rating.ReviewCount}");
                Console.WriteLine($"  Positive: {sellerWithRatings.Rating.PositiveCount}");
                Console.WriteLine($"  Negative: {sellerWithRatings.Rating.NegativeCount}\n");
            }

            // Example 7: Get user listings
            Console.WriteLine("7. Getting seller's listings...");
            var userListings = await listingService.GetUserListingsAsync(seller.Id);
            Console.WriteLine($"✓ Seller has {userListings.Count} listings:");
            foreach (var listing in userListings)
            {
                Console.WriteLine($"  - {listing.Title} (${listing.Price.Amount})");
            }
            Console.WriteLine();

            // Example 8: Get top sellers
            Console.WriteLine("8. Getting top sellers...");
            var topSellers = await userService.GetTopSellersAsync(limit: 5);
            Console.WriteLine($"✓ Top {topSellers.Count} sellers:");
            foreach (var topSeller in topSellers)
            {
                Console.WriteLine($"  - {topSeller.FullName} (Rating: {topSeller.Rating.AverageRating})");
            }
            Console.WriteLine();

            // Example 9: Verify user email
            Console.WriteLine("9. Verifying user email...");
            await userService.VerifyUserEmailAsync(seller.Id);
            var verifiedUser = await userService.GetUserAsync(seller.Id);
            Console.WriteLine($"✓ Email verified: {verifiedUser?.IsVerified}\n");

            // Example 10: Get user by username
            Console.WriteLine("10. Finding user by username...");
            var foundUser = await userService.GetUserByUsernameAsync("john_seller");
            if (foundUser != null)
            {
                Console.WriteLine($"✓ Found: {foundUser.FullName} ({foundUser.Email})\n");
            }

            // Example 11: Get user by email
            Console.WriteLine("11. Finding user by email...");
            var emailUser = await userService.GetUserByEmailAsync("seller@example.com");
            if (emailUser != null)
            {
                Console.WriteLine($"✓ Found: {emailUser.FullName} (ID: {emailUser.Id})\n");
            }

            // Example 12: Update user profile
            Console.WriteLine("12. Updating user profile...");
            if (sellerWithRatings != null)
            {
                await userService.UpdateUserProfileAsync(
                    userId: seller.Id,
                    fullName: "John Smith Jr.",
                    location: new Location { City = "Los Angeles", State = "CA", Country = "USA" }
                );
                var updatedProfile = await userService.GetUserAsync(seller.Id);
                Console.WriteLine($"✓ Profile updated:");
                Console.WriteLine($"  Name: {updatedProfile?.FullName}");
                Console.WriteLine($"  Location: {updatedProfile?.Location?.City}, {updatedProfile?.Location?.State}\n");
            }

            // Example 13: Deactivate user account
            Console.WriteLine("13. Deactivating user account...");
            await userService.DeactivateUserAsync(buyer.Id);
            var deactivatedUser = await userService.GetUserAsync(buyer.Id);
            Console.WriteLine($"✓ Account active: {deactivatedUser?.IsActive}\n");

            // Example 14: Reactivate user account
            Console.WriteLine("14. Reactivating user account...");
            await userService.ActivateUserAsync(buyer.Id);
            var reactivatedUser = await userService.GetUserAsync(buyer.Id);
            Console.WriteLine($"✓ Account active: {reactivatedUser?.IsActive}\n");

            // Example 15: User statistics
            Console.WriteLine("15. User statistics...");
            var allUsers = await userService.GetAllUsersAsync();
            var sellerCount = allUsers.Count(u => u.Role == UserRole.PremiumSeller || u.Role == UserRole.User);
            var moderatorCount = allUsers.Count(u => u.Role == UserRole.Moderator);
            var activeCount = allUsers.Count(u => u.IsActive);

            Console.WriteLine($"✓ Statistics:");
            Console.WriteLine($"  Total Users: {allUsers.Count}");
            Console.WriteLine($"  Active Users: {activeCount}");
            Console.WriteLine($"  Sellers: {sellerCount}");
            Console.WriteLine($"  Moderators: {moderatorCount}\n");

            Console.WriteLine("=== Example completed successfully ===");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ Error: {ex.Message}");
        }
    }
}
