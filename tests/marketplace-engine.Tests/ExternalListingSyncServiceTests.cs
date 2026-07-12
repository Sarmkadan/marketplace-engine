using FluentAssertions;
using MarketplaceEngine.Domain.Models;
using MarketplaceEngine.Infrastructure.Integration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MarketplaceEngine.Tests;

/// <summary>
/// Tests for the <see cref="ExternalListingSyncService"/> class.
/// </summary>
public class ExternalListingSyncServiceTests
{
    private readonly Mock<IListingProvider> _providerMock;
    private readonly Mock<ILogger<ExternalListingSyncService>> _loggerMock;
    private readonly ExternalListingSyncService _sut;

    /// <summary>
    /// Initializes the test fixture by creating mocks for the listing provider and logger,
    /// and instantiating the system under test.
    /// </summary>
    public ExternalListingSyncServiceTests()
    {
        _providerMock = new Mock<IListingProvider>();
        _loggerMock = new Mock<ILogger<ExternalListingSyncService>>();
        _sut = new ExternalListingSyncService(_providerMock.Object, _loggerMock.Object);
    }

    /// <summary>
    /// Verifies that <see cref="ExternalListingSyncService.SyncListingsAsync(string, Guid)"/>
    /// returns a collection of mapped listings when the provider returns external listings.
    /// </summary>
    /// <param name="category">The category of listings to sync.</param>
    /// <param name="sellerId">The identifier of the seller.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task SyncListingsAsync_WhenProviderReturnsListings_ReturnsMappedListings()
    {
        // Arrange
        var category = "electronics";
        var sellerId = Guid.NewGuid();
        var externalListings = new List<ExternalListingDto>
        {
            new ExternalListingDto { ExternalId = "ext-1", Title = "Phone", Description = "A smart phone", Price = 500m },
            new ExternalListingDto { ExternalId = "ext-2", Title = "Laptop", Description = "A powerful laptop", Price = 1000m }
        };
        _providerMock.Setup(p => p.GetListingsAsync(category, 1)).ReturnsAsync(externalListings);

        // Act
        var result = await _sut.SyncListingsAsync(category, sellerId);

        // Assert
        result.Should().HaveCount(2);
        result[0].Title.Should().Be("Phone");
        result[1].Title.Should().Be("Laptop");
        result[0].SellerId.Should().Be(sellerId);
    }

    /// <summary>
    /// Verifies that <see cref="ExternalListingSyncService.UpdateAvailabilityAsync(string, Listing)"/>
    /// logs an informational message when the external listing is no longer available.
    /// </summary>
    /// <param name="externalId">The external identifier of the listing.</param>
    /// <param name="listing">The local listing instance.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task UpdateAvailabilityAsync_WhenNotAvailable_LogsInformation()
    {
        // Arrange
        var externalId = "ext-1";
        var listing = new Listing { Id = Guid.NewGuid() };
        _providerMock.Setup(p => p.IsListingAvailableAsync(externalId)).ReturnsAsync(false);

        // Act
        await _sut.UpdateAvailabilityAsync(externalId, listing);

        // Assert
        _providerMock.Verify(p => p.IsListingAvailableAsync(externalId), Times.Once);
        // Verify logger was called
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("no longer available")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
