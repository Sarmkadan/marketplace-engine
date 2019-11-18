#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace MarketplaceEngine.Domain.Models;

/// <summary>
/// Represents a marketplace category for organizing listings.
/// </summary>
public class Category
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? IconUrl { get; set; }
    public Guid? ParentCategoryId { get; set; }
    public Category? ParentCategory { get; set; }
    public List<Category> SubCategories { get; set; } = [];
    public List<Listing> Listings { get; set; } = [];
    public int ListingCount { get; set; }
    public bool IsActive { get; set; } = true;
    public int DisplayOrder { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Validates category name and generates slug
    public void ValidateAndInitialize()
    {
        if (string.IsNullOrWhiteSpace(Name) || Name.Length < 2)
            throw new ArgumentException("Category name must be at least 2 characters", nameof(Name));

        if (Name.Length > 50)
            throw new ArgumentException("Category name cannot exceed 50 characters", nameof(Name));

        GenerateSlug();
    }

    // Generates URL-friendly slug from category name
    private void GenerateSlug()
    {
        var name = Name.ToLowerInvariant().Trim();
        var slug = System.Text.RegularExpressions.Regex.Replace(name, @"[^a-z0-9\s-]", "");
        slug = System.Text.RegularExpressions.Regex.Replace(slug, @"\s+", "-");
        slug = System.Text.RegularExpressions.Regex.Replace(slug, @"-+", "-");
        Slug = slug.Trim('-');
    }

    // Adds a subcategory
    public void AddSubCategory(Category subCategory)
    {
        if (subCategory is null)
            throw new ArgumentNullException(nameof(subCategory));

        if (subCategory.ParentCategoryId == Id)
            return;

        subCategory.ParentCategoryId = Id;
        subCategory.ParentCategory = this;
        SubCategories.Add(subCategory);
        UpdatedAt = DateTime.UtcNow;
    }

    // Removes a subcategory
    public bool RemoveSubCategory(Guid subCategoryId)
    {
        var removed = SubCategories.RemoveAll(sc => sc.Id == subCategoryId);
        if (removed > 0)
            UpdatedAt = DateTime.UtcNow;

        return removed > 0;
    }

    // Checks if category has active listings
    public bool HasActiveListings()
    {
        return Listings.Any(l => l.Status == Enums.ListingStatus.Active);
    }

    // Returns the full category path (e.g., "Electronics > Smartphones")
    public string GetFullPath()
    {
        if (ParentCategory is null)
            return Name;

        return $"{ParentCategory.GetFullPath()} > {Name}";
    }

    // Increments listing count
    public void IncrementListingCount()
    {
        ListingCount++;
        UpdatedAt = DateTime.UtcNow;
    }

    // Decrements listing count
    public void DecrementListingCount()
    {
        if (ListingCount > 0)
        {
            ListingCount--;
            UpdatedAt = DateTime.UtcNow;
        }
    }

    // Deactivates category
    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }
}
