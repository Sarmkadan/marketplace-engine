using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using MarketplaceEngine.Domain.Models;
using MarketplaceEngine.Domain.ValueObjects;
using MarketplaceEngine.Repositories;
using MarketplaceEngine.Services;
using Moq;

/// <summary>
/// Contains benchmarks for the ListingService.
/// </summary>
[MemoryDiagnoser]
public class ListingServiceBenchmarks
{
    /// <summary>
    /// Initializes the benchmark setup.
    /// </summary>
    private ListingService? _listingService;
    private Guid _sellerId;
    private Guid _categoryId;
    private Mock<IListingRepository>? _listingRepositoryMock;
    private Mock<IUserRepository>? _userRepositoryMock;

    /// <summary>
    /// Sets up the benchmark environment.
    /// </summary>
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

    /// <summary>
    /// Creates a new listing and measures the time it takes.
    /// </summary>
    /// <returns>A task representing the creation of the listing.</returns>
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

/// <summary>
/// Contains benchmarks for the SearchService.
/// </summary>
[MemoryDiagnoser]
public class SearchServiceBenchmarks
{
    /// <summary>
    /// Initializes the benchmark setup.
    /// </summary>
    private SearchService? _searchService;
    private Mock<IListingRepository>? _listingRepositoryMock;
    private Mock<IUserRepository>? _userRepositoryMock;

    /// <summary>
    /// Sets up the benchmark environment.
    /// </summary>
    [GlobalSetup]
    public void Setup()
    {
        _listingRepositoryMock = new Mock<IListingRepository>();
        _userRepositoryMock = new Mock<IUserRepository>();

        _searchService = new SearchService(_listingRepositoryMock.Object, _userRepositoryMock.Object);
    }

    /// <summary>
    /// Searches for listings and measures the time it takes.
    /// </summary>
    /// <returns>A task representing the search operation.</returns>
    [Benchmark]
    public async Task SearchListingsBenchmark()
    {
        await _searchService!.SearchListingsAsync("test");
    }
}

/// <summary>
/// The main entry point for the application.
/// </summary>
public class Program
{
    /// <summary>
    /// Runs the benchmark for the ListingService.
    /// </summary>
    /// <param name="args">The command-line arguments.</param>
    public static void Main(string[] args)
    {
        BenchmarkRunner.Run<ListingServiceBenchmarks>();
        BenchmarkRunner.Run<SearchServiceBenchmarks>();
    }
}
