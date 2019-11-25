#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Collections.Concurrent;
using FluentAssertions;
using MarketplaceEngine.Data;
using MarketplaceEngine.Domain.Enums;
using MarketplaceEngine.Domain.Models;
using MarketplaceEngine.Domain.ValueObjects;
using MarketplaceEngine.Exceptions;
using MarketplaceEngine.Repositories;
using MarketplaceEngine.Services;
using Xunit;

namespace MarketplaceEngine.Tests;

/// <summary>
/// Integration tests that exercise complete multi-service workflows using the
/// real in-memory repositories and shared DbContext.
/// </summary>
[Collection("Database")]
public class MarketplaceWorkflowTests : IDisposable
{
    private readonly MarketplaceDbContext _db;
    private readonly UserRepository _userRepo;
    private readonly ListingRepository _listingRepo;
    private readonly MessageRepository _messageRepo;
    private readonly UserService _userService;
    private readonly ListingService _listingService;
    private readonly MessagingService _messagingService;
    private readonly SearchService _searchService;

    public MarketplaceWorkflowTests()
    {
        _db = MarketplaceDbContext.GetInstance();
        _db.Reset();

        _userRepo = new UserRepository();
        _listingRepo = new ListingRepository();
        _messageRepo = new MessageRepository();

        _userService = new UserService(_userRepo);
        _listingService = new ListingService(_listingRepo, _userRepo);
        _messagingService = new MessagingService(_messageRepo, _userRepo);
        _searchService = new SearchService(_listingRepo, _userRepo);
    }

    public void Dispose() => _db.Reset();

    // ── Full listing lifecycle ─────────────────────────────────────────────

    [Fact]
    public async Task FullListingLifecycle_CreateSearchDelistAndVerify()
    {
        // Register seller
        var seller = await _userService.RegisterUserAsync("seller@flow.test", "Flow Seller");
        seller.IsVerified = true;
        seller.IsActive = true;
        await _userRepo.UpdateAsync(seller);

        var categoryId = _db.Categories.First().Id;

        // Create listing
        var listing = await _listingService.CreateListingAsync(
            seller.Id,
            "Vintage Acoustic Guitar",
            "A beautifully maintained vintage acoustic guitar from the 1980s in excellent condition.",
            250m, "USD", categoryId,
            ["https://img.example.com/guitar.jpg"]);

        listing.Id.Should().NotBeEmpty();
        listing.Status.Should().Be(ListingStatus.Active);
        listing.PublishedAt.Should().NotBeNull();

        // Search for the listing
        var searchResults = await _searchService.SearchListingsAsync("Acoustic Guitar");
        searchResults.Should().ContainSingle(l => l.Id == listing.Id);

        // Record view
        var viewed = await _listingService.GetListingWithViewAsync(listing.Id);
        viewed.Should().NotBeNull();

        // Delist it
        var delisted = await _listingService.DelistListingAsync(listing.Id, seller.Id);
        delisted.Status.Should().Be(ListingStatus.Delisted);

        // Should no longer appear in search
        var afterDelist = await _searchService.SearchListingsAsync("Vintage Acoustic");
        afterDelist.Should().NotContain(l => l.Id == listing.Id);
    }

    // ── Full messaging workflow ────────────────────────────────────────────

    [Fact]
    public async Task FullMessagingWorkflow_SendReadReplyAndDelete()
    {
        // Register two users
        var sender = await _userService.RegisterUserAsync("msguser1@flow.test", "Message Sender");
        var recipient = await _userService.RegisterUserAsync("msguser2@flow.test", "Message Recipient");

        // Send a message
        var message = await _messagingService.SendMessageAsync(
            sender.Id, recipient.Id,
            "Inquiry about listing",
            "Hello, I am interested in your listing. Is it still available?");

        message.Should().NotBeNull();
        message.IsRead.Should().BeFalse();

        // Get received messages
        var received = await _messagingService.GetReceivedMessagesAsync(recipient.Id);
        received.Should().ContainSingle(m => m.Id == message.Id);

        // Mark as read
        var read = await _messagingService.MarkAsReadAsync(message.Id);
        read.IsRead.Should().BeTrue();
        read.ReadAt.Should().NotBeNull();

        // Add a reply
        var reply = await _messagingService.AddReplyAsync(
            message.Id, recipient.Id, "Yes, it is still available. Would you like to meet?");

        reply.Subject.Should().Be("Re: Inquiry about listing");
        reply.SenderId.Should().Be(recipient.Id);

        // Delete original message by sender
        await _messagingService.DeleteMessageAsync(message.Id, sender.Id);

        var received2 = await _messagingService.GetReceivedMessagesAsync(recipient.Id);
        received2.Should().NotContain(m => m.Id == message.Id);
    }

    // ── User registration to premium promotion workflow ────────────────────

    [Fact]
    public async Task UserRegistrationToPremiumPromotion_WhenEligible_Succeeds()
    {
        // Register
        var user = await _userService.RegisterUserAsync("premium@flow.test", "Premium Candidate");
        user.IsVerified = true;
        await _userRepo.UpdateAsync(user);

        // Simulate 5 sales
        for (var i = 0; i < 5; i++)
            await _userService.RecordSaleAsync(user.Id);

        // Set a qualifying rating
        var updatedUser = await _userService.GetUserAsync(user.Id);
        updatedUser.Rating = new Rating(5, 20);
        await _userRepo.UpdateAsync(updatedUser);

        // Promote
        var promoted = await _userService.PromoteToPremiumAsync(user.Id);

        promoted.Role.Should().Be(UserRole.PremiumSeller);
        promoted.TotalSales.Should().Be(5);
    }

    // ── Advanced search with multiple filters ──────────────────────────────

    [Fact]
    public async Task AdvancedSearch_WithPriceAndCategoryFilters_ReturnsCorrectListings()
    {
        // Register seller
        var seller = await _userService.RegisterUserAsync("advsearch@flow.test", "Advanced Seller");
        seller.IsVerified = true;
        seller.IsActive = true;
        await _userRepo.UpdateAsync(seller);

        var electronicsCategoryId = _db.Categories.First(c => c.Name == "Electronics").Id;
        var servicesCategoryId = _db.Categories.First(c => c.Name == "Services").Id;

        // Create listings in different categories with different prices
        var cheapElectronics = await _listingService.CreateListingAsync(
            seller.Id, "Budget Headphones",
            "Affordable budget headphones suitable for everyday listening needs.",
            15m, "USD", electronicsCategoryId,
            ["https://img.example.com/headphones.jpg"]);

        var expensiveElectronics = await _listingService.CreateListingAsync(
            seller.Id, "Premium Laptop Pro",
            "High performance laptop for professionals with long battery life and great display.",
            1200m, "USD", electronicsCategoryId,
            ["https://img.example.com/laptop.jpg"]);

        var serviceItem = await _listingService.CreateListingAsync(
            seller.Id, "Web Development Service",
            "Professional web development service for small businesses and startups needing websites.",
            500m, "USD", servicesCategoryId,
            ["https://img.example.com/service.jpg"]);

        // Search electronics under $100
        var results = await _searchService.AdvancedSearchAsync(
            categoryId: electronicsCategoryId, maxPrice: 100m);

        results.Should().ContainSingle(l => l.Id == cheapElectronics.Id);
        results.Should().NotContain(l => l.Id == expensiveElectronics.Id);
        results.Should().NotContain(l => l.Id == serviceItem.Id);
    }

    // ── Concurrent listing creation ────────────────────────────────────────

    [Fact]
    public async Task ConcurrentListingCreation_AllListingsPersistedWithoutDataCorruption()
    {
        var seller = await _userService.RegisterUserAsync("concurrent@flow.test", "Concurrent Seller");
        seller.IsVerified = true;
        seller.IsActive = true;
        await _userRepo.UpdateAsync(seller);

        var categoryId = _db.Categories.First().Id;
        const int count = 10;
        var errors = new ConcurrentBag<Exception>();
        var createdIds = new ConcurrentBag<Guid>();

        var tasks = Enumerable.Range(0, count).Select(i => Task.Run(async () =>
        {
            try
            {
                var listing = await _listingService.CreateListingAsync(
                    seller.Id,
                    $"Concurrent Item Number {i:D2}",
                    "This is a concurrent listing created from parallel tasks in a test.",
                    10m + i, "USD", categoryId,
                    ["https://img.example.com/concurrent.jpg"]);
                createdIds.Add(listing.Id);
            }
            catch (Exception ex)
            {
                errors.Add(ex);
            }
        }));

        await Task.WhenAll(tasks);

        errors.Should().BeEmpty("no exceptions expected during concurrent creation");
        createdIds.Should().HaveCount(count, "all listings should be persisted");
        createdIds.Distinct().Should().HaveCount(count, "each listing should have a unique ID");
    }

    // ── Email verification workflow ────────────────────────────────────────

    [Fact]
    public async Task EmailVerification_WithCorrectToken_VerifiesUser()
    {
        var user = await _userService.RegisterUserAsync("verify@flow.test", "Verify Me");

        // Token is generated on registration
        user.VerificationToken.Should().NotBeNullOrEmpty();
        user.IsVerified.Should().BeFalse();

        var token = user.VerificationToken!;
        var result = await _userService.VerifyEmailAsync(user.Id, token);

        result.Should().BeTrue();

        var verified = await _userService.GetUserAsync(user.Id);
        verified.IsVerified.Should().BeTrue();
        verified.VerificationToken.Should().BeNull();
    }

    // ── Pagination correctness ─────────────────────────────────────────────

    [Fact]
    public async Task PaginatedListings_SecondPage_ReturnsDistinctItemsFromFirstPage()
    {
        var seller = await _userService.RegisterUserAsync("paginate@flow.test", "Paginate Seller");
        seller.IsVerified = true;
        seller.IsActive = true;
        await _userRepo.UpdateAsync(seller);

        var categoryId = _db.Categories.First().Id;

        // Create 25 listings
        for (var i = 0; i < 25; i++)
        {
            await _listingService.CreateListingAsync(
                seller.Id,
                $"Paged Listing Item Num {i:D2}",
                "Listing created for pagination test to verify correct page slicing.",
                10m + i, "USD", categoryId,
                ["https://img.example.com/page.jpg"]);
        }

        var (page1, total1) = await _listingService.GetPaginatedListingsAsync(1, 10);
        var (page2, total2) = await _listingService.GetPaginatedListingsAsync(2, 10);

        total1.Should().Be(total2);
        total1.Should().BeGreaterThanOrEqualTo(25);

        var page1Ids = page1.Select(l => l.Id).ToHashSet();
        var page2Ids = page2.Select(l => l.Id).ToHashSet();

        page1Ids.Intersect(page2Ids).Should().BeEmpty("pages must not overlap");
    }

    // ── Category-based search with pagination ─────────────────────────────

    [Fact]
    public async Task SearchByCategory_TotalMatchesBothPages()
    {
        var seller = await _userService.RegisterUserAsync("catsearch@flow.test", "Category Seller");
        seller.IsVerified = true;
        seller.IsActive = true;
        await _userRepo.UpdateAsync(seller);

        var categoryId = _db.Categories.First().Id;

        for (var i = 0; i < 15; i++)
        {
            await _listingService.CreateListingAsync(
                seller.Id,
                $"Category Search Item Num {i:D2}",
                "Listing created for category search pagination integration test scenario.",
                10m + i, "USD", categoryId,
                ["https://img.example.com/cat.jpg"]);
        }

        var (page1, total) = await _searchService.SearchByCategoryAsync(categoryId, 1, 10);
        var (page2, _) = await _searchService.SearchByCategoryAsync(categoryId, 2, 10);

        total.Should().BeGreaterThanOrEqualTo(15);
        (page1.Count + page2.Count).Should().Be(Math.Min(total, 20));
    }

    // ── Deactivate user blocks listing creation ────────────────────────────

    [Fact]
    public async Task DeactivatedUser_CannotCreateNewListings()
    {
        var user = await _userService.RegisterUserAsync("deactivated@flow.test", "Will Be Deactivated");
        await _userService.DeactivateAccountAsync(user.Id);

        var categoryId = _db.Categories.First().Id;

        var act = async () => await _listingService.CreateListingAsync(
            user.Id,
            "Should Fail Listing",
            "This listing should not be created by a deactivated user at all.",
            50m, "USD", categoryId,
            ["https://img.example.com/fail.jpg"]);

        await act.Should().ThrowAsync<UnauthorizedException>();
    }
}
