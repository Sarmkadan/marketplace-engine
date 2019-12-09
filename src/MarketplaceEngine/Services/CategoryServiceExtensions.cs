using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MarketplaceEngine.Domain.Models;   // Assuming Category lives in this namespace
using MarketplaceEngine.Services;        // Same namespace as CategoryService

namespace MarketplaceEngine.Services
{
    /// <summary>
    /// Extension methods that add convenient, reusable operations on <see cref="CategoryService"/>.
    /// </summary>
    public static class CategoryServiceExtensions
    {
        /// <summary>
        /// Retrieves all categories and returns them as a dictionary keyed by their Id.
        /// </summary>
        /// <param name="service">The <see cref="CategoryService"/> instance.</param>
        /// <returns>A dictionary where the key is the category Id and the value is the <see cref="Category"/>.</returns>
        public static async Task<Dictionary<Guid, Category>> ToDictionaryAsync(this CategoryService service)
        {
            if (service == null) throw new ArgumentNullException(nameof(service));

            var categories = await service.GetAllCategoriesAsync().ConfigureAwait(false);
            return categories.ToDictionary(c => c.Id);
        }

        /// <summary>
        /// Gets the display names of the direct sub‑categories of a given parent category.
        /// </summary>
        /// <param name="service">The <see cref="CategoryService"/> instance.</param>
        /// <param name="parentId">The identifier of the parent category.</param>
        /// <returns>A list of sub‑category names.</returns>
        public static async Task<List<string>> GetSubCategoryNamesAsync(this CategoryService service, Guid parentId)
        {
            if (service == null) throw new ArgumentNullException(nameof(service));

            var subCategories = await service.GetSubCategoriesAsync(parentId).ConfigureAwait(false);
            return subCategories.Select(c => c.Name).ToList();
        }

        /// <summary>
        /// Retrieves the slugs of the currently “hot” categories.
        /// </summary>
        /// <param name="service">The <see cref="CategoryService"/> instance.</param>
        /// <returns>A list of slug strings for hot categories.</returns>
        public static async Task<List<string>> GetHotCategorySlugsAsync(this CategoryService service)
        {
            if (service == null) throw new ArgumentNullException(nameof(service));

            var hotCategories = await service.GetHotCategoriesAsync().ConfigureAwait(false);
            return hotCategories
                .Where(c => !string.IsNullOrWhiteSpace(c.Slug))
                .Select(c => c.Slug)
                .ToList();
        }

        /// <summary>
        /// Searches categories by a case‑insensitive name fragment and returns the matching categories.
        /// This is a thin wrapper around <see cref="CategoryService.SearchCategoriesAsync"/> that
        /// normalises the search term.
        /// </summary>
        /// <param name="service">The <see cref="CategoryService"/> instance.</param>
        /// <param name="nameFragment">Part of the category name to search for.</param>
        /// <returns>A list of categories whose names contain the fragment.</returns>
        public static async Task<List<Category>> SearchByNameAsync(this CategoryService service, string nameFragment)
        {
            if (service == null) throw new ArgumentNullException(nameof(service));
            if (string.IsNullOrWhiteSpace(nameFragment)) return new List<Category>();

            // The underlying service expects a search term; we trim and keep the original casing
            // because the service itself may handle case‑insensitivity internally.
            var trimmed = nameFragment.Trim();
            return await service.SearchCategoriesAsync(trimmed).ConfigureAwait(false);
        }
    }
}
