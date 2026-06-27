using FluentAssertions;
using MarketplaceEngine.Domain.Models;
using MarketplaceEngine.Infrastructure.Integration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MarketplaceEngine.Tests;

public class ExternalListingSyncServiceTests
{
    private readonly Mock<IListingProvider> _providerMock;
    private readonly Mock<ILogger<ExternalListingSyncService>> _loggerMock;
    private readonly ExternalListingSyncService _sut;

    public ExternalListingSyncServiceTests()
    {
        _providerMock = new Mock<IListingProvider>();
        _loggerMock = new Mock<ILogger<ExternalListingSyncService>>();
        _sut = new ExternalListingSyncService(_providerMock.Object, _loggerMock.Object);
    }

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
