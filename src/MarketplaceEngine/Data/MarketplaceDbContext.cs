#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using MarketplaceEngine.Domain.Models;

namespace MarketplaceEngine.Data;

/// <summary>
/// In-memory database context for marketplace entities.
/// </summary>
public class MarketplaceDbContext
{
    private static MarketplaceDbContext? _instance;
    private static readonly object _lock = new();

    public List<User> Users { get; set; } = [];
    public List<Category> Categories { get; set; } = [];
    public List<Listing> Listings { get; set; } = [];
    public List<Message> Messages { get; set; } = [];
    public List<ModerationReport> ModerationReports { get; set; } = [];

    private MarketplaceDbContext()
    {
        InitializeDefaultData();
    }

    // Gets singleton instance of the database context
    public static MarketplaceDbContext GetInstance()
    {
        if (_instance is null)
        {
            lock (_lock)
            {
                _instance ??= new MarketplaceDbContext();
            }
        }

        return _instance;
    }

    // Initializes default/seed data
    private void InitializeDefaultData()
    {
        // Seed default categories
        var electronics = new Category
        {
            Id = Guid.NewGuid(),
            Name = "Electronics",
            Description = "Electronic devices and gadgets",
            ListingCount = 0,
            IsActive = true
        };
        electronics.ValidateAndInitialize();
        Categories.Add(electronics);

        var smartphones = new Category
        {
            Id = Guid.NewGuid(),
            Name = "Smartphones",
            Description = "Mobile phones and accessories",
            ParentCategoryId = electronics.Id,
            ListingCount = 0,
            IsActive = true
        };
        smartphones.ValidateAndInitialize();
        Categories.Add(smartphones);

        var services = new Category
        {
            Id = Guid.NewGuid(),
            Name = "Services",
            Description = "Professional and personal services",
            ListingCount = 0,
            IsActive = true
        };
        services.ValidateAndInitialize();
        Categories.Add(services);

        var realestate = new Category
        {
            Id = Guid.NewGuid(),
            Name = "Real Estate",
            Description = "Property listings and rentals",
            ListingCount = 0,
            IsActive = true
        };
        realestate.ValidateAndInitialize();
        Categories.Add(realestate);

        // Seed default users
        var seller1 = new User
        {
            Id = Guid.NewGuid(),
            Email = "seller@example.com",
            FullName = "John Seller",
            Phone = "+1234567890",
            Role = Domain.Enums.UserRole.User,
            IsVerified = true,
            IsActive = true,
            TotalListings = 0,
            TotalSales = 5
        };
        seller1.ValidateProfile();
        Users.Add(seller1);

        var buyer1 = new User
        {
            Id = Guid.NewGuid(),
            Email = "buyer@example.com",
            FullName = "Jane Buyer",
            Phone = "+0987654321",
            Role = Domain.Enums.UserRole.User,
            IsVerified = true,
            IsActive = true,
            TotalListings = 0,
            TotalSales = 0
        };
        buyer1.ValidateProfile();
        Users.Add(buyer1);
    }

    // Clears all data from context
    public void Clear()
    {
        Users.Clear();
        Categories.Clear();
        Listings.Clear();
        Messages.Clear();
        ModerationReports.Clear();
    }

    // Resets context to initial state
    public void Reset()
    {
        Clear();
        InitializeDefaultData();
    }

    // Gets total count of all entities
    public int GetTotalEntityCount()
    {
        return Users.Count + Categories.Count + Listings.Count + Messages.Count + ModerationReports.Count;
    }
}
