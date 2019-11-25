#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FluentAssertions;
using MarketplaceEngine.Domain.Enums;
using MarketplaceEngine.Domain.Models;
using MarketplaceEngine.Domain.ValueObjects;
using MarketplaceEngine.Exceptions;
using MarketplaceEngine.Repositories;
using MarketplaceEngine.Services;
using Moq;
using Xunit;

namespace MarketplaceEngine.Tests;

public class SearchServiceTests
{
    private readonly Mock<IListingRepository> _listingRepoMock;
    private readonly Mock<IUserRepository> _userRepoMock;
    private readonly SearchService _sut;

    public SearchServiceTests()
    {
        _listingRepoMock = new Mock<IListingRepository>();
        _userRepoMock = new Mock<IUserRepository>();
        _sut = new SearchService(_listingRepoMock.Object, _userRepoMock.Object);
    }

    private static Listing MakeListing(string title = "Active Listing Title Here",
        string description = "This is a valid description that is long enough.",
        decimal price = 50m, Guid? categoryId = null, ListingStatus status = ListingStatus.Active) =>
        new()
        {
            Id = Guid.NewGuid(),
            SellerId = Guid.NewGuid(),
            Title = title,
            Description = description,
            Price = new Money(price, "USD"),
            CategoryId = categoryId ?? Guid.NewGuid(),
            Status = status,
            ImageUrls = ["https://img.example.com/img.jpg"],
            PublishedAt = DateTime.UtcNow,
            ViewCount = 0,
            InterestCount = 0
        };

    // ── SearchListingsAsync ────────────────────────────────────────────────

    [Fact]
    public async Task SearchListingsAsync_WithEmptyQuery_ThrowsValidationException()
    {
        var act = async () => await _sut.SearchListingsAsync("   ");

        await act.Should().ThrowAsync<Exceptions.ValidationException>();
    }

    [Fact]
    public async Task SearchListingsAsync_WithSingleCharQuery_ThrowsValidationException()
    {
        var act = async () => await _sut.SearchListingsAsync("x");

        await act.Should().ThrowAsync<Exceptions.ValidationException>().WithMessage("*at least*");
    }

    [Fact]
    public async Task SearchListingsAsync_WithOverlongQuery_ThrowsValidationException()
    {
        var longQuery = new string('a', 201);

        var act = async () => await _sut.SearchListingsAsync(longQuery);

        await act.Should().ThrowAsync<Exceptions.ValidationException>().WithMessage("*cannot exceed*");
    }

    [Fact]
    public async Task SearchListingsAsync_WithValidQuery_DelegatesToRepository()
    {
        var listings = new List<Listing> { MakeListing() };
        _listingRepoMock.Setup(r => r.SearchAsync("guitar")).ReturnsAsync(listings);

        var result = await _sut.SearchListingsAsync("guitar");

        result.Should().HaveCount(1);
        _listingRepoMock.Verify(r => r.SearchAsync("guitar"), Times.Once);
    }

    // ── SearchByTagsAsync ──────────────────────────────────────────────────

    [Fact]
    public async Task SearchByTagsAsync_WithEmptyTagList_ThrowsValidationException()
    {
        var act = async () => await _sut.SearchByTagsAsync(new List<string>());

        await act.Should().ThrowAsync<Exceptions.ValidationException>();
    }

    [Fact]
    public async Task SearchByTagsAsync_WithNullTagList_ThrowsValidationException()
    {
        var act = async () => await _sut.SearchByTagsAsync(null!);

        await act.Should().ThrowAsync<Exceptions.ValidationException>();
    }

    [Fact]
    public async Task SearchByTagsAsync_WithValidTags_DelegatesToRepository()
    {
        var tags = new List<string> { "electronics", "phone" };
        var listings = new List<Listing> { MakeListing() };
        _listingRepoMock.Setup(r => r.GetByTagsAsync(tags)).ReturnsAsync(listings);

        var result = await _sut.SearchByTagsAsync(tags);

        result.Should().HaveCount(1);
    }

    // ── FindNearbyListingsAsync ────────────────────────────────────────────

    [Fact]
    public async Task FindNearbyListingsAsync_WithLatitudeBelowMinus90_ThrowsValidationException()
    {
        var act = async () => await _sut.FindNearbyListingsAsync(-91, 0);

        await act.Should().ThrowAsync<Exceptions.ValidationException>().WithMessage("*Latitude*");
    }

    [Fact]
    public async Task FindNearbyListingsAsync_WithLatitudeAbove90_ThrowsValidationException()
    {
        var act = async () => await _sut.FindNearbyListingsAsync(91, 0);

        await act.Should().ThrowAsync<Exceptions.ValidationException>().WithMessage("*Latitude*");
    }

    [Fact]
    public async Task FindNearbyListingsAsync_WithLongitudeBelowMinus180_ThrowsValidationException()
    {
        var act = async () => await _sut.FindNearbyListingsAsync(0, -181);

        await act.Should().ThrowAsync<Exceptions.ValidationException>().WithMessage("*Longitude*");
    }

    [Fact]
    public async Task FindNearbyListingsAsync_WithRadiusBelowMinimum_ThrowsValidationException()
    {
        var act = async () => await _sut.FindNearbyListingsAsync(40, -74, 0.05);

        await act.Should().ThrowAsync<Exceptions.ValidationException>().WithMessage("*Radius*");
    }

    [Fact]
    public async Task FindNearbyListingsAsync_WithRadiusAboveMaximum_ThrowsValidationException()
    {
        var act = async () => await _sut.FindNearbyListingsAsync(40, -74, 600);

        await act.Should().ThrowAsync<Exceptions.ValidationException>().WithMessage("*Radius*");
    }

    [Fact]
    public async Task FindNearbyListingsAsync_WithValidParameters_DelegatesToRepository()
    {
        var listings = new List<Listing> { MakeListing() };
        _listingRepoMock.Setup(r => r.GetNearbyAsync(40.7, -74.0, 10)).ReturnsAsync(listings);

        var result = await _sut.FindNearbyListingsAsync(40.7, -74.0, 10);

        result.Should().HaveCount(1);
    }

    // ── SearchByCategoryAsync ──────────────────────────────────────────────

    [Fact]
    public async Task SearchByCategoryAsync_WithEmptyGuid_ThrowsValidationException()
    {
        var act = async () => await _sut.SearchByCategoryAsync(Guid.Empty, 1, 20);

        await act.Should().ThrowAsync<Exceptions.ValidationException>().WithMessage("*Category*");
    }

    [Fact]
    public async Task SearchByCategoryAsync_WithValidCategoryId_ReturnsPaginatedResults()
    {
        var categoryId = Guid.NewGuid();
        var listings = Enumerable.Range(0, 15).Select(_ => MakeListing(categoryId: categoryId)).ToList();
        _listingRepoMock.Setup(r => r.GetByCategoryIdAsync(categoryId)).ReturnsAsync(listings);

        var (items, total) = await _sut.SearchByCategoryAsync(categoryId, 1, 10);

        total.Should().Be(15);
        items.Should().HaveCount(10);
    }

    [Fact]
    public async Task SearchByCategoryAsync_WithPage2_ReturnsCorrectSlice()
    {
        var categoryId = Guid.NewGuid();
        var now = DateTime.UtcNow;
        var listings = Enumerable.Range(0, 25)
            .Select(i =>
            {
                var l = MakeListing(categoryId: categoryId);
                l.PublishedAt = now.AddSeconds(-i);
                return l;
            })
            .ToList();
        _listingRepoMock.Setup(r => r.GetByCategoryIdAsync(categoryId)).ReturnsAsync(listings);

        var (items, total) = await _sut.SearchByCategoryAsync(categoryId, 2, 10);

        total.Should().Be(25);
        items.Should().HaveCount(10);
    }

    // ── AdvancedSearchAsync ────────────────────────────────────────────────

    [Fact]
    public async Task AdvancedSearchAsync_WithKeywordFilter_ReturnsMatchingListings()
    {
        var active = new List<Listing>
        {
            MakeListing("Acoustic Guitar", "A well-maintained acoustic guitar for musicians."),
            MakeListing("Electric Guitar", "Powerful electric guitar for stage performances."),
            MakeListing("Vintage Piano", "Beautifully restored vintage piano in mint condition.")
        };
        _listingRepoMock.Setup(r => r.GetActiveListingsAsync()).ReturnsAsync(active);

        var result = await _sut.AdvancedSearchAsync(keyword: "guitar");

        result.Should().HaveCount(2);
        result.Should().AllSatisfy(l =>
            l.Title.Contains("Guitar", StringComparison.OrdinalIgnoreCase).Should().BeTrue());
    }

    [Fact]
    public async Task AdvancedSearchAsync_WithPriceRangeFilter_ReturnsListingsInRange()
    {
        var active = new List<Listing>
        {
            MakeListing("Cheap Item", "This is a cheap item description for testing.", 10m),
            MakeListing("Mid Range Item", "This is a mid-range item for a normal price.", 50m),
            MakeListing("Expensive Item", "This is a very expensive item for high-end buyers.", 200m)
        };
        _listingRepoMock.Setup(r => r.GetActiveListingsAsync()).ReturnsAsync(active);

        var result = await _sut.AdvancedSearchAsync(minPrice: 30m, maxPrice: 100m);

        result.Should().HaveCount(1);
        result[0].Title.Should().Contain("Mid");
    }

    [Fact]
    public async Task AdvancedSearchAsync_WithCategoryFilter_ReturnsOnlyMatchingCategory()
    {
        var targetCategory = Guid.NewGuid();
        var otherCategory = Guid.NewGuid();
        var active = new List<Listing>
        {
            MakeListing(categoryId: targetCategory),
            MakeListing(categoryId: targetCategory),
            MakeListing(categoryId: otherCategory)
        };
        _listingRepoMock.Setup(r => r.GetActiveListingsAsync()).ReturnsAsync(active);

        var result = await _sut.AdvancedSearchAsync(categoryId: targetCategory);

        result.Should().HaveCount(2);
        result.Should().AllSatisfy(l => l.CategoryId.Should().Be(targetCategory));
    }

    [Fact]
    public async Task AdvancedSearchAsync_WithTagsFilter_ReturnsListingsWithMatchingTags()
    {
        var listingWithTag = MakeListing();
        listingWithTag.Tags.Add("vintage");

        var listingWithoutTag = MakeListing();

        var active = new List<Listing> { listingWithTag, listingWithoutTag };
        _listingRepoMock.Setup(r => r.GetActiveListingsAsync()).ReturnsAsync(active);

        var result = await _sut.AdvancedSearchAsync(tags: ["vintage"]);

        result.Should().HaveCount(1);
        result[0].Tags.Should().Contain("vintage");
    }

    [Fact]
    public async Task AdvancedSearchAsync_WithNoFilters_ReturnsAllActiveListingsByViewCount()
    {
        var l1 = MakeListing(); l1.ViewCount = 5;
        var l2 = MakeListing(); l2.ViewCount = 100;
        var l3 = MakeListing(); l3.ViewCount = 20;
        var active = new List<Listing> { l1, l2, l3 };
        _listingRepoMock.Setup(r => r.GetActiveListingsAsync()).ReturnsAsync(active);

        var result = await _sut.AdvancedSearchAsync();

        result.Should().HaveCount(3);
        result[0].ViewCount.Should().Be(100);
        result[1].ViewCount.Should().Be(20);
    }

    // ── GetTrendingListingsAsync ───────────────────────────────────────────

    [Fact]
    public async Task GetTrendingListingsAsync_SortsDescendingByViewCountThenInterestCount()
    {
        var t1 = MakeListing(); t1.ViewCount = 10; t1.InterestCount = 5;
        var t2 = MakeListing(); t2.ViewCount = 50; t2.InterestCount = 1;
        var t3 = MakeListing(); t3.ViewCount = 10; t3.InterestCount = 20;
        var active = new List<Listing> { t1, t2, t3 };
        _listingRepoMock.Setup(r => r.GetActiveListingsAsync()).ReturnsAsync(active);

        var result = await _sut.GetTrendingListingsAsync(3);

        result[0].ViewCount.Should().Be(50);
        result[1].InterestCount.Should().Be(20); // tiebreak by interest
        result[2].InterestCount.Should().Be(5);
    }

    [Fact]
    public async Task GetTrendingListingsAsync_WithLimitExceeding100_ClampsToTwenty()
    {
        var active = Enumerable.Range(0, 30).Select(_ => MakeListing()).ToList();
        _listingRepoMock.Setup(r => r.GetActiveListingsAsync()).ReturnsAsync(active);

        var result = await _sut.GetTrendingListingsAsync(999);

        result.Should().HaveCount(20);
    }

    // ── GetSearchSuggestionsAsync ──────────────────────────────────────────

    [Fact]
    public async Task GetSearchSuggestionsAsync_WithMatchingPrefix_ReturnsSuggestions()
    {
        var active = new List<Listing>
        {
            MakeListing("Acoustic Guitar"),
            MakeListing("Acoustic Keyboard"),
            MakeListing("Electric Piano")
        };
        _listingRepoMock.Setup(r => r.GetActiveListingsAsync()).ReturnsAsync(active);

        var result = await _sut.GetSearchSuggestionsAsync("Acoustic");

        result.Should().HaveCount(2);
        result.Should().AllSatisfy(s => s.Should().StartWith("Acoustic"));
    }

    [Fact]
    public async Task GetSearchSuggestionsAsync_WithEmptyPrefix_ThrowsValidationException()
    {
        var act = async () => await _sut.GetSearchSuggestionsAsync("  ");

        await act.Should().ThrowAsync<Exceptions.ValidationException>();
    }

    // ── SearchUsersAsync ───────────────────────────────────────────────────

    [Fact]
    public async Task SearchUsersAsync_WithValidQuery_DelegatesToUserRepository()
    {
        var users = new List<User> { new() { Id = Guid.NewGuid(), Email = "u@x.com", FullName = "Alice Smith" } };
        _userRepoMock.Setup(r => r.SearchAsync("alice")).ReturnsAsync(users);

        var result = await _sut.SearchUsersAsync("alice");

        result.Should().HaveCount(1);
    }
}
