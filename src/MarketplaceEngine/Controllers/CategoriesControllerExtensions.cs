#nullable enable

using Microsoft.AspNetCore.Mvc;
using MarketplaceEngine.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MarketplaceEngine.Controllers;

/// <summary>
/// Extension methods for <see cref="CategoriesController"/> providing additional functionality
/// for working with marketplace categories and their statistics.
/// </summary>
public static class CategoriesControllerExtensions
{
    /// <summary>
    /// Filters categories by a specific parent category ID.
    /// </summary>
    /// <param name="controller">The categories controller instance.</param>
    /// <param name="parentCategoryId">The parent category ID to filter by.</param>
    /// <param name="depth">The depth of subcategories to include (default: 1).</param>
    /// <returns>Filtered list of categories that are children of the specified parent.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="controller"/> is <see langword="null"/>.</exception>
    public static async Task<IActionResult> GetChildCategories(
        this CategoriesController controller,
        Guid parentCategoryId,
        int depth = 1)
    {
        ArgumentNullException.ThrowIfNull(controller);

        if (depth < 1) depth = 1;
        if (depth > 5) depth = 5;

        var allCategories = await controller.GetCategories(depth);

        return allCategories switch
        {
            OkObjectResult okResult => GetChildCategoriesResult(okResult, parentCategoryId),
            _ => new StatusCodeResult(StatusCodes.Status500InternalServerError)
        };
    }

    /// <summary>
    /// Gets the root categories (categories with no parent).
    /// </summary>
    /// <param name="controller">The categories controller instance.</param>
    /// <param name="depth">The depth of subcategories to include (default: 1).</param>
    /// <returns>List of root categories.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="controller"/> is <see langword="null"/>.</exception>
    public static async Task<IActionResult> GetRootCategories(
        this CategoriesController controller,
        int depth = 1)
    {
        ArgumentNullException.ThrowIfNull(controller);

        if (depth < 1) depth = 1;
        if (depth > 5) depth = 5;

        var allCategories = await controller.GetCategories(depth);

        return allCategories switch
        {
            OkObjectResult okResult => GetRootCategoriesResult(okResult),
            _ => new StatusCodeResult(StatusCodes.Status500InternalServerError)
        };
    }

    /// <summary>
    /// Gets the full category hierarchy as a flat list with parent-child relationships.
    /// </summary>
    /// <param name="controller">The categories controller instance.</param>
    /// <returns>Flat list of all categories with their parent information.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="controller"/> is <see langword="null"/>.</exception>
    public static async Task<IActionResult> GetCategoryHierarchy(
        this CategoriesController controller)
    {
        ArgumentNullException.ThrowIfNull(controller);

        var allCategories = await controller.GetCategories(5);

        return allCategories switch
        {
            OkObjectResult okResult => GetCategoryHierarchyResult(okResult),
            _ => new StatusCodeResult(StatusCodes.Status500InternalServerError)
        };
    }

    /// <summary>
    /// Gets statistics for all categories in a single batch call.
    /// </summary>
    /// <param name="controller">The categories controller instance.</param>
    /// <returns>Dictionary mapping category IDs to their statistics.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="controller"/> is <see langword="null"/>.</exception>
    public static async Task<IActionResult> GetAllCategoriesStatistics(
        this CategoriesController controller)
    {
        ArgumentNullException.ThrowIfNull(controller);

        var allCategories = await controller.GetCategories(5);

        return allCategories switch
        {
            OkObjectResult okResult => await GetAllCategoriesStatisticsResultAsync(okResult),
            _ => new StatusCodeResult(StatusCodes.Status500InternalServerError)
        };
    }

    /// <summary>
    /// Helper method to recursively get all subcategories from a category.
    /// </summary>
    /// <param name="category">The category to get subcategories from.</param>
    /// <returns>Enumeration of all subcategories.</returns>
    private static IEnumerable<CategoryDto> GetAllSubCategories(CategoryDto category)
    {
        foreach (var subCategory in category.SubCategories)
        {
            yield return subCategory;
            foreach (var nestedSubCategory in GetAllSubCategories(subCategory))
            {
                yield return nestedSubCategory;
            }
        }
    }

    /// <summary>
    /// Processes the result of GetCategories to extract child categories.
    /// </summary>
    /// <param name="result">The result from GetCategories call.</param>
    /// <param name="parentCategoryId">The parent category ID to filter by.</param>
    /// <returns>Filtered list of child categories.</returns>
    private static IActionResult GetChildCategoriesResult(OkObjectResult result, Guid parentCategoryId)
    {
        var categories = (List<CategoryDto>)result.Value!;
        var childCategories = categories
            .SelectMany(c => GetAllSubCategories(c))
            .Where(c => c.ParentCategoryId == parentCategoryId)
            .ToList();

        return new OkObjectResult(childCategories);
    }

    /// <summary>
    /// Processes the result of GetCategories to extract root categories.
    /// </summary>
    /// <param name="result">The result from GetCategories call.</param>
    /// <returns>List of root categories.</returns>
    private static IActionResult GetRootCategoriesResult(OkObjectResult result)
    {
        var categories = (List<CategoryDto>)result.Value!;
        var rootCategories = categories
            .Where(c => c.ParentCategoryId == null)
            .ToList();

        return new OkObjectResult(rootCategories);
    }

    /// <summary>
    /// Processes the result of GetCategories to build category hierarchy.
    /// </summary>
    /// <param name="result">The result from GetCategories call.</param>
    /// <returns>Flat list of all categories with parent information.</returns>
    private static IActionResult GetCategoryHierarchyResult(OkObjectResult result)
    {
        var categories = (List<CategoryDto>)result.Value!;
        var hierarchy = categories
            .SelectMany(c => GetAllSubCategories(c))
            .Concat(categories)
            .OrderBy(c => c.Name)
            .ToList();

        return new OkObjectResult(hierarchy);
    }

    /// <summary>
    /// Processes the result of GetCategories and GetCategoryStatistics to build statistics dictionary.
    /// </summary>
    /// <param name="result">The result from GetCategories call.</param>
    /// <returns>Dictionary mapping category IDs to their statistics.</returns>
    private static async Task<IActionResult> GetAllCategoriesStatisticsResultAsync(OkObjectResult result)
    {
        var categories = (List<CategoryDto>)result.Value!;
        var allCategoriesList = categories
            .SelectMany(c => GetAllSubCategories(c))
            .Concat(categories)
            .ToList();

        var statsTasks = allCategoriesList
            .Select(async c =>
            {
                var statsResult = await c.Id.GetCategoryStatisticsAsync(c);
                return statsResult;
            })
            .ToList();

        var statsResults = await Task.WhenAll(statsTasks);
        var statsDictionary = statsResults
            .Where(r => r.Statistics is not null)
            .ToDictionary(r => r.CategoryId, r => r.Statistics!);

        return new OkObjectResult(statsDictionary);
    }

    /// <summary>
    /// Helper record to hold category statistics result.
    /// </summary>
    /// <param name="CategoryId">The category ID.</param>
    /// <param name="Statistics">The statistics DTO or null.</param>
    private sealed record CategoryStatisticsResult
    {
        public Guid CategoryId { get; init; }
        public CategoryStatisticsDto? Statistics { get; init; }
    }

    /// <summary>
    /// Extension method to get category statistics for a category ID.
    /// </summary>
    /// <param name="categoryId">The category ID.</param>
    /// <param name="category">The category DTO.</param>
    /// <returns>The category statistics result.</returns>
    private static async Task<CategoryStatisticsResult> GetCategoryStatisticsAsync(
        this Guid categoryId,
        CategoryDto category)
    {
        var statsResult = await ((CategoriesController)null!).GetCategoryStatistics(categoryId);

        return new CategoryStatisticsResult
        {
            CategoryId = categoryId,
            Statistics = statsResult switch
            {
                OkObjectResult okStats => okStats.Value as CategoryStatisticsDto,
                _ => null
            }
        };
    }
}