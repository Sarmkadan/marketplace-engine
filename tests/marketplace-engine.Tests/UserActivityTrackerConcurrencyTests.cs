#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Collections.Concurrent;
using FluentAssertions;
using MarketplaceEngine.Recommendations;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace MarketplaceEngine.Tests;

public class UserActivityTrackerConcurrencyTests
{
    private const int WriterCount = 8;
    private const int SignalsPerWriter = 500;

    [Fact]
    public async Task RecordAsync_FromConcurrentWriters_ShouldNotLoseSignals()
    {
        var options = new RecommendationOptions { MaxSignalsPerUser = int.MaxValue };
        var sut = new UserActivityTracker(options, NullLogger<UserActivityTracker>.Instance);

        // All writers share the same users and listings so that they contend on the same collections
        var users = Enumerable.Range(0, 4).Select(_ => Guid.NewGuid()).ToArray();
        var listings = Enumerable.Range(0, 10).Select(_ => Guid.NewGuid()).ToArray();
        var exceptions = new ConcurrentBag<Exception>();

        var writers = Enumerable.Range(0, WriterCount).Select(w => Task.Run(async () =>
        {
            try
            {
                for (var i = 0; i < SignalsPerWriter; i++)
                {
                    await sut.RecordAsync(new UserActivitySignal
                    {
                        UserId = users[(w + i) % users.Length],
                        ListingId = listings[(w * 7 + i) % listings.Length],
                        SignalType = SignalType.View
                    });
                }
            }
            catch (Exception ex)
            {
                exceptions.Add(ex);
            }
        })).ToArray();

        await Task.WhenAll(writers);

        exceptions.Should().BeEmpty();
        sut.TotalSignalCount.Should().Be(WriterCount * SignalsPerWriter);
        sut.TrackedUserCount.Should().Be(users.Length);

        foreach (var user in users)
        {
            var history = await sut.GetUserHistoryAsync(user);
            history.Should().NotBeEmpty();
        }

        // Every listing was touched by at least one writer, so each must appear in the audience index
        sut.IndexedListingCount.Should().Be(listings.Length);
        foreach (var listing in listings)
        {
            var audience = await sut.GetListingAudienceAsync(listing);
            audience.Should().NotBeEmpty();
            audience.Should().OnlyHaveUniqueItems();
        }
    }

    [Fact]
    public async Task RecordAsync_WithReadersRunningConcurrently_ShouldNotThrowAndKeepFifoCap()
    {
        const int maxSignals = 50;
        var options = new RecommendationOptions { MaxSignalsPerUser = maxSignals };
        var sut = new UserActivityTracker(options, NullLogger<UserActivityTracker>.Instance);

        var userId = Guid.NewGuid();
        var listingId = Guid.NewGuid();
        var exceptions = new ConcurrentBag<Exception>();
        using var cts = new CancellationTokenSource();

        // Readers iterate the per-user lists and the audience sets while writers mutate them
        var readers = Enumerable.Range(0, 4).Select(_ => Task.Run(async () =>
        {
            try
            {
                while (!cts.IsCancellationRequested)
                {
                    var history = await sut.GetUserHistoryAsync(userId, TimeSpan.FromHours(1));
                    history.Count.Should().BeLessThanOrEqualTo(maxSignals);

                    await sut.GetListingAudienceAsync(listingId);
                    await sut.GetSignalsInWindowAsync(TimeSpan.FromHours(1));
                    _ = sut.TotalSignalCount;
                }
            }
            catch (Exception ex)
            {
                exceptions.Add(ex);
            }
        })).ToArray();

        var writers = Enumerable.Range(0, WriterCount).Select(_ => Task.Run(async () =>
        {
            try
            {
                for (var i = 0; i < SignalsPerWriter; i++)
                {
                    await sut.RecordAsync(new UserActivitySignal
                    {
                        UserId = userId,
                        ListingId = listingId,
                        SignalType = SignalType.Save
                    });
                }
            }
            catch (Exception ex)
            {
                exceptions.Add(ex);
            }
        })).ToArray();

        await Task.WhenAll(writers);
        await cts.CancelAsync();
        await Task.WhenAll(readers);

        exceptions.Should().BeEmpty();
        sut.TotalSignalCount.Should().Be(maxSignals);

        var finalHistory = await sut.GetUserHistoryAsync(userId);
        finalHistory.Should().HaveCount(maxSignals);
        finalHistory.Should().OnlyContain(s => s.UserId == userId && s.ListingId == listingId);
    }
}
