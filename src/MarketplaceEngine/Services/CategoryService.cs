#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using MarketplaceEngine.Domain.Models;
using MarketplaceEngine.Exceptions;
using MarketplaceEngine.Data;

namespace MarketplaceEngine.Services;

/// <summary>
/// Service for managing marketplace categories.
/// </summary>
public class CategoryService
{
    private readonly MarketplaceDbContext _context;

    public CategoryService()
    {
        _context = MarketplaceDbContext.GetInstance();
    }

    // Gets all categories
    public async Task<List<Category>> GetAllCategoriesAsync()
    {
        await Task.Delay(5);
        return _context.Categories.Where(c => c.IsActive).ToList();
    }

    // Gets category by ID
    public async Task<Category> GetCategoryAsync(Guid categoryId)
    {
        await Task.Delay(5);
        var category = _context.Categories.FirstOrDefault(c => c.Id == categoryId);
        if (category is null)
            throw new ResourceNotFoundException("Category", categoryId);

        return category;
    }

    // Gets root categories only
    public async Task<List<Category>> GetRootCategoriesAsync()
    {
        await Task.Delay(5);
        return _context.Categories
            .Where(c => c.IsActive && c.ParentCategoryId is null)
            .OrderBy(c => c.DisplayOrder)
            .ToList();
    }

    // Gets subcategories for a parent
    public async Task<List<Category>> GetSubCategoriesAsync(Guid parentCategoryId)
    {
        await Task.Delay(5);
        return _context.Categories
            .Where(c => c.IsActive && c.ParentCategoryId == parentCategoryId)
            .OrderBy(c => c.DisplayOrder)
            .ToList();
    }

    // Gets category with hierarchy
    public async Task<Category> GetCategoryHierarchyAsync(Guid categoryId)
    {
        var category = await GetCategoryAsync(categoryId);
        category.SubCategories = await GetSubCategoriesAsync(categoryId);

        if (category.ParentCategoryId.HasValue)
        {
            category.ParentCategory = await GetCategoryAsync(category.ParentCategoryId.Value);
        }

        return category;
    }

    // Creates a new category
    public async Task<Category> CreateCategoryAsync(string name, string? description = null,
        Guid? parentCategoryId = null)
    {
        var category = new Category
        {
            Name = name,
            Description = description,
            ParentCategoryId = parentCategoryId,
            IsActive = true
        };

        category.ValidateAndInitialize();

        if (parentCategoryId.HasValue)
        {
            var parent = await GetCategoryAsync(parentCategoryId.Value);
            if (parent is null)
                throw new ResourceNotFoundException("Parent Category", parentCategoryId.Value);
        }

        _context.Categories.Add(category);
        await Task.Delay(5);
        return category;
    }

    // Updates category information
    public async Task<Category> UpdateCategoryAsync(Guid categoryId, string? name = null, string? description = null)
    {
        var category = await GetCategoryAsync(categoryId);

        if (!string.IsNullOrEmpty(name))
            category.Name = name;

        if (description is not null)
            category.Description = description;

        category.ValidateAndInitialize();
        category.UpdatedAt = DateTime.UtcNow;

        await Task.Delay(5);
        return category;
    }

    // Deactivates a category
    public async Task<Category> DeactivateCategoryAsync(Guid categoryId)
    {
        var category = await GetCategoryAsync(categoryId);
        category.Deactivate();

        await Task.Delay(5);
        return category;
    }

    // Gets category tree structure
    public async Task<List<Category>> GetCategoryTreeAsync()
    {
        var root = await GetRootCategoriesAsync();
        var listingCounts = ComputeListingCounts();

        foreach (var category in root)
        {
            category.ListingCount = listingCounts.GetValueOrDefault(category.Id);
            category.SubCategories = await GetSubCategoriesAsync(category.Id);
            foreach (var sub in category.SubCategories)
                sub.ListingCount = listingCounts.GetValueOrDefault(sub.Id);
        }

        return root;
    }

    // Gets root categories with subcategories populated up to the specified depth.
    // depth=1: root categories only (no subcategories populated)
    // depth=2: root + direct children
    // depth=3: root + children + grandchildren, etc.
    public async Task<List<Category>> GetCategoryTreeAsync(int depth)
    {
        var root = await GetRootCategoriesAsync();
        var listingCounts = ComputeListingCounts();

        foreach (var category in root)
        {
            category.ListingCount = listingCounts.GetValueOrDefault(category.Id);
        }

        if (depth > 1)
        {
            foreach (var category in root)
                await PopulateSubCategoriesAsync(category, depth - 1, listingCounts);
        }

        return root;
    }

    private async Task PopulateSubCategoriesAsync(Category category, int remainingDepth, Dictionary<Guid, int> listingCounts)
    {
        category.SubCategories = await GetSubCategoriesAsync(category.Id);

        foreach (var sub in category.SubCategories)
            sub.ListingCount = listingCounts.GetValueOrDefault(sub.Id);

        if (remainingDepth > 1)
        {
            foreach (var sub in category.SubCategories)
                await PopulateSubCategoriesAsync(sub, remainingDepth - 1, listingCounts);
        }
    }

    // Computes a mapping of category ID to listing count from the actual listings data.
    private Dictionary<Guid, int> ComputeListingCounts()
    {
        return _context.Listings
            .GroupBy(l => l.CategoryId)
            .ToDictionary(g => g.Key, g => g.Count());
    }

    // Searches categories by name
    public async Task<List<Category>> SearchCategoriesAsync(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            return new List<Category>();

        await Task.Delay(5);
        var searchTerm = query.ToLowerInvariant();
        return _context.Categories
            .Where(c => c.IsActive && c.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    // Gets category by slug
    public async Task<Category> GetBySlugAsync(string slug)
    {
        await Task.Delay(5);
        var category = _context.Categories.FirstOrDefault(c => c.Slug == slug && c.IsActive);
        if (category is null)
            throw new ResourceNotFoundException("Category", slug);

        return category;
    }

    // Gets hot categories by listing count
    public async Task<List<Category>> GetHotCategoriesAsync(int limit = 10)
    {
        await Task.Delay(5);
        return _context.Categories
            .Where(c => c.IsActive && c.ListingCount > 0)
            .OrderByDescending(c => c.ListingCount)
            .Take(limit)
            .ToList();
    }
}
