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
    /// <param name="controller">The categories controller instance</param>
    /// <param name="parentCategoryId">The parent category ID to filter by</param>
    /// <param name="depth">The depth of subcategories to include (default: 1)</param>
    /// <returns>Filtered list of categories that are children of the specified parent</returns>
    public static async Task<IActionResult> GetChildCategories(
        this CategoriesController controller,
        Guid parentCategoryId,
        int depth = 1)
    {
        if (depth < 1) depth = 1;
        if (depth > 5) depth = 5;

        var allCategories = await controller.GetCategories(depth);

        if (allCategories is not OkObjectResult okResult)
        {
            return new StatusCodeResult(StatusCodes.Status500InternalServerError);
        }

        var categories = (List<CategoryDto>)okResult.Value!;
        var childCategories = categories
            .SelectMany(c => GetAllSubCategories(c))
            .Where(c => c.ParentCategoryId == parentCategoryId)
            .ToList();

        return new OkObjectResult(childCategories);
    }

    /// <summary>
    /// Gets the root categories (categories with no parent).
    /// </summary>
    /// <param name="controller">The categories controller instance</param>
    /// <param name="depth">The depth of subcategories to include (default: 1)</param>
    /// <returns>List of root categories</returns>
    public static async Task<IActionResult> GetRootCategories(
        this CategoriesController controller,
        int depth = 1)
    {
        if (depth < 1) depth = 1;
        if (depth > 5) depth = 5;

        var allCategories = await controller.GetCategories(depth);

        if (allCategories is not OkObjectResult okResult)
        {
            return new StatusCodeResult(StatusCodes.Status500InternalServerError);
        }

        var categories = (List<CategoryDto>)okResult.Value!;
        var rootCategories = categories
            .Where(c => c.ParentCategoryId == null)
            .ToList();

        return new OkObjectResult(rootCategories);
    }

    /// <summary>
    /// Gets the full category hierarchy as a flat list with parent-child relationships.
    /// </summary>
    /// <param name="controller">The categories controller instance</param>
    /// <returns>Flat list of all categories with their parent information</returns>
    public static async Task<IActionResult> GetCategoryHierarchy(
        this CategoriesController controller)
    {
        var allCategories = await controller.GetCategories(5);

        if (allCategories is not OkObjectResult okResult)
        {
            return new StatusCodeResult(StatusCodes.Status500InternalServerError);
        }

        var categories = (List<CategoryDto>)okResult.Value!;
        var hierarchy = categories
            .SelectMany(c => GetAllSubCategories(c))
            .Concat(categories)
            .OrderBy(c => c.Name)
            .ToList();

        return new OkObjectResult(hierarchy);
    }

    /// <summary>
    /// Gets statistics for all categories in a single batch call.
    /// </summary>
    /// <param name="controller">The categories controller instance</param>
    /// <returns>Dictionary mapping category IDs to their statistics</returns>
    public static async Task<IActionResult> GetAllCategoriesStatistics(
        this CategoriesController controller)
    {
        var allCategories = await controller.GetCategories(5);

        if (allCategories is not OkObjectResult okResult)
        {
            return new StatusCodeResult(StatusCodes.Status500InternalServerError);
        }

        var categories = (List<CategoryDto>)okResult.Value!;
        var allCategoriesList = categories
            .SelectMany(c => GetAllSubCategories(c))
            .Concat(categories)
            .ToList();

        var statsTasks = allCategoriesList
            .Select(async c =>
            {
                var statsResult = await controller.GetCategoryStatistics(c.Id);
                return new
                {
                    CategoryId = c.Id,
                    Statistics = statsResult switch
                    {
                        OkObjectResult okStats => okStats.Value as CategoryStatisticsDto,
                        _ => null
                    }
                };
            })
            .ToList();

        var statsResults = await Task.WhenAll(statsTasks);
        var statsDictionary = statsResults
            .Where(r => r.Statistics != null)
            .ToDictionary(r => r.CategoryId, r => r.Statistics!);

        return new OkObjectResult(statsDictionary);
    }

    /// <summary>
    /// Helper method to recursively get all subcategories from a category
    /// </summary>
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
}
