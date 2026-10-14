#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Collections.Concurrent;
using System.Threading.Tasks;
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

[Collection("Database")]
public class ProductCatalogTests : IDisposable
{
    private readonly ListingRepository _listingRepository;
    private readonly ProductCatalog _productCatalog;
    private readonly PricingEngine _pricingEngine;
    private readonly OrderProcessor _orderProcessor;
    private readonly IUserRepository _userRepository;
    private readonly IPaymentRepository _paymentRepository;

    public ProductCatalogTests()
    {
        // Ensure a clean state for each test by clearing the static list
        MarketplaceDbContext.GetInstance().Listings.Clear();
        MarketplaceDbContext.GetInstance().Users.Clear();
        MarketplaceDbContext.GetInstance().Categories.Clear();
        MarketplaceDbContext.GetInstance().Payments.Clear();

        _listingRepository = new ListingRepository();
        _userRepository = new UserRepository();
        _paymentRepository = new PaymentRepository();
        _productCatalog = new ProductCatalog(_listingRepository);
        _pricingEngine = new PricingEngine(_listingRepository);
        _orderProcessor = new OrderProcessor(_paymentRepository, _listingRepository, _userRepository);

        // Seed a test user for use in tests
        var seller = new User
        {
            Id = Guid.NewGuid(),
            Email = "seller@example.com",
            FullName = "Test Seller",
            IsActive = true,
            IsVerified = true
        };
        _userRepository.AddAsync(seller).GetAwaiter().GetResult();

        SellerId = seller.Id;
        // We don't need to seed a category because ProductCatalog filters by CategoryId on the listing,
        // not by checking the category's existence in the categories list.
        CategoryId = Guid.NewGuid();
    }

    public Guid SellerId { get; }
    public Guid CategoryId { get; }

    public void Dispose()
    {
        // Clean up static data after all tests in this class are run.
        MarketplaceDbContext.GetInstance().Listings.Clear();
        MarketplaceDbContext.GetInstance().Users.Clear();
        MarketplaceDbContext.GetInstance().Categories.Clear();
        MarketplaceDbContext.GetInstance().Payments.Clear();
    }

    [Fact]
    public async Task BulkAddSearchAndPaging_ShouldWorkCorrectly()
    {
        // Arrange: Bulk add 1,000 products
        var productsToAdd = 1000;
        var baseTitle = "Test Product ";
        var baseDescription = "This is a test product description.";

        for (int i = 0; i < productsToAdd; i++)
        {
            var listing = new Listing
            {
                Id = Guid.NewGuid(),
                SellerId = SellerId,
                CategoryId = CategoryId,
                Title = $"{baseTitle}{i}",
                Description = baseDescription,
                Price = new Money(10.00m + i, "USD"),
                ImageUrls = new List<string> { "https://example.com/image.jpg" },
                Status = ListingStatus.Active
            };

            await _listingRepository.AddAsync(listing);
        }

        // Act: Search by name substring (search for "Test Product 123")
        var searchTerm = "Test Product 123";
        var searchResults = await _productCatalog.SearchAsync(searchTerm, 50);

        // Assert: Should find at least one product (the exact match and possibly others with substring)
        searchResults.Should().NotBeNullOrEmpty();
        searchResults.Should().ContainSingle(l => l.Title.Contains(searchTerm, StringComparison.OrdinalIgnoreCase));

        // Act: Get all active listings in category via ProductCatalog
        var allInCategory = await _productCatalog.BrowseAsync(CategoryId);

        // Assert: We have 1000 items
        allInCategory.Should().HaveCount(1000);

        // Act: Now test paging via the ProductCatalog results (in-memory paging)
        var pageSize = 50;
        var firstPage = allInCategory.Take(pageSize).ToList();
        var secondPage = allInCategory.Skip(pageSize).Take(pageSize).ToList();

        // Assert: First page has 50 items, second page has 50 items, and they are different
        firstPage.Should().HaveCount(50);
        secondPage.Should().HaveCount(50);
        firstPage.Should().NotBeEquivalentTo(secondPage);

        // And the total count should be 1000
        allInCategory.Count.Should().Be(1000);

        // Verify stable ordering: the first item of the second page should be the 51st item in the full list.
        // Since ProductCatalog orders by CreatedAt descending, we expect the first page to be the 50 most recently added items.
        // We added items in a loop from i=0 to 999, so the most recently added item is i=999, then 998, etc.
        // Therefore, the first page should contain items with i from 999 down to 950.
        // We'll check the titles of the first page to ensure they are in the expected range.
        foreach (var item in firstPage)
        {
            // Extract the number from the title "Test Product {i}"
            var title = item.Title;
            if (int.TryParse(title.Split(' ').Last(), out var number))
            {
                number.Should().BeGreaterOrEqualTo(950).And.BeLessThanOrEqualTo(999);
            }
            else
            {
                Assert.Fail($"Title '{title}' does not end with a number.");
            }
        }

        // Similarly, the second page should contain items with i from 949 down to 900.
        foreach (var item in secondPage)
        {
            var title = item.Title;
            if (int.TryParse(title.Split(' ').Last(), out var number))
            {
                number.Should().BeGreaterOrEqualTo(900).And.BeLessThanOrEqualTo(949);
            }
            else
            {
                Assert.Fail($"Title '{title}' does not end with a number.");
            }
        }
    }

    [Fact]
    public async Task UpdatePriceAndVerifyPricingEngine_ShouldPickUpNewPrice()
    {
        // Arrange: Create a product
        var listing = new Listing
        {
            Id = Guid.NewGuid(),
            SellerId = SellerId,
            CategoryId = CategoryId,
            Title = "Test Product for Price Update",
            Description = "This is a test description that is long enough to meet the requirement.", // At least 20 characters
            Price = new Money(20.00m, "USD"),
            ImageUrls = new List<string> { "https://example.com/image.jpg" },
            Status = ListingStatus.Active
        };

        await _listingRepository.AddAsync(listing);

        // Act: Get initial price from PricingEngine
        var initialBreakdown = await _pricingEngine.CalculateTotalAsync(listing.Id);
        initialBreakdown.BuyerTotal.Amount.Should().Be(20.00m);

        // Update the price via ListingService
        var listingService = new ListingService(_listingRepository, _userRepository);
        await listingService.UpdateListingAsync(listing.Id, SellerId, price: new Money(30.00m, "USD"));

        // Act: Get updated price from PricingEngine
        var updatedBreakdown = await _pricingEngine.CalculateTotalAsync(listing.Id);
        updatedBreakdown.BuyerTotal.Amount.Should().Be(30.00m);
    }

    [Fact]
    public async Task RemoveProductAndVerifyOrderProcessor_ShouldRejectOrder()
    {
        // Arrange: Create a product
        var listing = new Listing
        {
            Id = Guid.NewGuid(),
            SellerId = SellerId,
            CategoryId = CategoryId,
            Title = "Test Product for Removal",
            Description = "This is a test description that is long enough.",
            Price = new Money(15.00m, "USD"),
            ImageUrls = new List<string> { "https://example.com/image.jpg" },
            Status = ListingStatus.Active
        };

        await _listingRepository.AddAsync(listing);

        // Create a buyer user
        var buyer = new User
        {
            Id = Guid.NewGuid(),
            Email = "buyer@example.com",
            FullName = "Test Buyer",
            IsActive = true,
            IsVerified = true
        };
        await _userRepository.AddAsync(buyer);

        // Act: Remove the product (delist)
        var listingService = new ListingService(_listingRepository, _userRepository);
        await listingService.DelistListingAsync(listing.Id, SellerId);

        // Act: Try to place an order for the delisted product
        Func<Task> act = async () => await _orderProcessor.PlaceOrderAsync(listing.Id, buyer.Id, "credit_card");

        // Assert: OrderProcessor should throw a MarketplaceException because the listing is not active
        await act.Should().ThrowAsync<MarketplaceException>()
            .WithMessage("*not available for purchase*");
    }

    [Fact]
    public async Task ConcurrentReadWriteDuringBulkUpdate_ShouldNotThrowAndMaintainConsistency()
    {
        // Arrange: Start with a few products
        var initialCount = 10;
        for (int i = 0; i < initialCount; i++)
        {
            var listing = new Listing
            {
                Id = Guid.NewGuid(),
                SellerId = SellerId,
                CategoryId = CategoryId,
                Title = $"Initial Product {i}",
                Description = "Initial description",
                Price = new Money(10.00m, "USD"),
                ImageUrls = new List<string> { "https://example.com/image.jpg" },
                Status = ListingStatus.Active
            };
            await _listingRepository.AddAsync(listing);
        }

        // We'll simulate concurrent readers and writers.
        var tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        var cancellationToken = tokenSource.Token;

        var exceptions = new ConcurrentBag<Exception>();
        var readCounts = new ConcurrentBag<int>();
        var writtenCount = 0;

        // Writer task: bulk add more products (up to 1,000 total)
        var writerTask = Task.Run(async () =>
        {
            var localWritten = 0;
            while (!cancellationToken.IsCancellationRequested && localWritten < 500) // Add 500 more
            {
                try
                {
                    var listing = new Listing
                    {
                        Id = Guid.NewGuid(),
                        SellerId = SellerId,
                        CategoryId = CategoryId,
                        Title = $"Concurrent Product {localWritten}",
                        Description = "Concurrent description",
                        Price = new Money(20.00m + localWritten, "USD"),
                        ImageUrls = new List<string> { "https://example.com/image.jpg" },
                        Status = ListingStatus.Active
                    };

                    await _listingRepository.AddAsync(listing);
                    Interlocked.Increment(ref writtenCount);
                    localWritten++;

                    await Task.Delay(10); // Small delay to allow readers to run
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                    break;
                }
            }
        }, cancellationToken);

        // Reader task: continuously browse and search
        var readerTask = Task.Run(async () =>
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    // Browse all active in category
                    var browseResults = await _productCatalog.BrowseAsync(CategoryId);
                    readCounts.Add(browseResults.Count);

                    // Search for a term that might exist
                    var searchResults = await _productCatalog.SearchAsync("Product", 10);
                    readCounts.Add(searchResults.Count);

                    await Task.Delay(5);
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                    break;
                }
            }
        }, cancellationToken);

        await Task.WhenAll(writerTask, readerTask);

        // Assert: No exceptions
        exceptions.Should().BeEmpty();

        // Assert: We have at least the initial count plus what was written
        var finalCount = (await _productCatalog.BrowseAsync(CategoryId)).Count;
        finalCount.Should().BeGreaterOrEqualTo(initialCount + writtenCount);

        // Additionally, we can check that the read counts are reasonable (non-negative and not crazy high)
        readCounts.Should().OnlyContain(c => c >= 0);
    }
}