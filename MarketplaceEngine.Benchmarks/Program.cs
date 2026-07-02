using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using MarketplaceEngine.Domain.Models;
using MarketplaceEngine.Domain.ValueObjects;
using MarketplaceEngine.Repositories;
using MarketplaceEngine.Services;
using Moq;

namespace MarketplaceEngine.Benchmarks;

[MemoryDiagnoser]
public class ListingServiceBenchmarks
{
    private ListingService? _listingService;
    private Guid _sellerId;
    private Guid _categoryId;
    private Mock<IListingRepository>? _listingRepositoryMock;
    private Mock<IUserRepository>? _userRepositoryMock;

    [GlobalSetup]
    public void Setup()
    {
        _listingRepositoryMock = new Mock<IListingRepository>();
        _userRepositoryMock = new Mock<IUserRepository>();

        _sellerId = Guid.NewGuid();
        _categoryId = Guid.NewGuid();

        _userRepositoryMock.Setup(repo => repo.GetByIdAsync(_sellerId))
            .ReturnsAsync(new User { Id = _sellerId, IsActive = true });

        _listingService = new ListingService(_listingRepositoryMock.Object, _userRepositoryMock.Object);
    }

    [Benchmark]
    public async Task CreateListingBenchmark()
    {
        await _listingService!.CreateListingAsync(
            _sellerId,
            "Test Listing",
            "This is a test description for the listing.",
            100.00m,
            "USD",
            _categoryId,
            new List<string> { "http://image1.jpg" }
        );
    }
}

[MemoryDiagnoser]
public class SearchServiceBenchmarks
{
    private SearchService? _searchService;
    private Mock<IListingRepository>? _listingRepositoryMock;
    private Mock<IUserRepository>? _userRepositoryMock;

    [GlobalSetup]
    public void Setup()
    {
        _listingRepositoryMock = new Mock<IListingRepository>();
        _userRepositoryMock = new Mock<IUserRepository>();

        _searchService = new SearchService(_listingRepositoryMock.Object, _userRepositoryMock.Object);
    }

    [Benchmark]
    public async Task SearchListingsBenchmark()
    {
        await _searchService!.SearchListingsAsync("test");
    }
}

public class Program
{
    public static void Main(string[] args)
    {
        BenchmarkRunner.Run<ListingServiceBenchmarks>();
        BenchmarkRunner.Run<SearchServiceBenchmarks>();
    }
}
