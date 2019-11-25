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
using MarketplaceEngine.Repositories;
using Xunit;

namespace MarketplaceEngine.Tests;

[Collection("Database")]
public class ListingRepositoryConcurrencyTests : IDisposable
{
    private readonly ListingRepository _listingRepository;
    private readonly Guid _testListingId;

    public ListingRepositoryConcurrencyTests()
    {
        // Ensure a clean state for each test by clearing the static list
        MarketplaceDbContext.GetInstance().Listings.Clear(); // Hotfix: Correctly access static Listings property
        _listingRepository = new ListingRepository();

        // Add a test listing
        var listing = new Listing
        {
            Id = Guid.NewGuid(),
            SellerId = Guid.NewGuid(),
            CategoryId = Guid.NewGuid(),
            Title = "Initial Title",
            Description = "Initial Description",
            Status = ListingStatus.Active,
            PublishedAt = DateTime.UtcNow
        };
        _listingRepository.AddAsync(listing).GetAwaiter().GetResult();
        _testListingId = listing.Id;
    }

    [Fact]
    public async Task GetActiveListingsAsync_And_UpdateAsync_ShouldHandleConcurrencyWithoutExceptions()
    {
        // Hotfix: Concurrency test for shared static listing collection access

        var tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(1)); // Run for 1 second
        var cancellationToken = tokenSource.Token;

        var exceptions = new ConcurrentBag<Exception>();

        var readerTask = Task.Run(async () =>
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var listings = await _listingRepository.GetActiveListingsAsync();
                    listings.Should().NotBeNull();
                    listings.Count.Should().BeGreaterOrEqualTo(1);
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                    break;
                }
                await Task.Delay(1); // Small delay to yield
            }
        });

        var writerTask = Task.Run(async () =>
        {
            var counter = 0;
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var listingToUpdate = await _listingRepository.GetByIdAsync(_testListingId);
                    if (listingToUpdate != null)
                    {
                        listingToUpdate.Title = $"Updated Title {counter++}";
                        await _listingRepository.UpdateAsync(listingToUpdate);
                    }
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                    break;
                }
                await Task.Delay(1); // Small delay to yield
            }
        });

        await Task.WhenAll(readerTask, writerTask);

        exceptions.Should().BeEmpty("No exceptions should be thrown during concurrent access.");
    }

    public void Dispose()
    {
        // Clean up static data after all tests in this class are run.
        // This is important because MarketplaceDbContext.Listings is static.
        MarketplaceDbContext.GetInstance().Listings.Clear(); // Hotfix: Correctly access static Listings property
    }
}
