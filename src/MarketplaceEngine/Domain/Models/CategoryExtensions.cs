using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace MarketplaceEngine.Domain.Models;

/// <summary>
/// Extension methods for <see cref="Category"/> that provide common navigation and display functionality.
/// </summary>
public static class CategoryExtensions
{
    /// <summary>
    /// Returns the sequence of ancestor categories, starting with the immediate parent and ending with the root.
    /// </summary>
    /// <param name="category">The category whose ancestors are to be retrieved.</param>
    /// <returns>An <see cref="IEnumerable{Category}"/> containing the ancestors in hierarchical order.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="category"/> is <c>null</c>.</exception>
    public static IEnumerable<Category> GetAncestors(this Category category)
    {
        ArgumentNullException.ThrowIfNull(category);

        var current = category.ParentCategory;
        while (current is not null)
        {
            yield return current;
            current = current.ParentCategory;
        }
    }

    /// <summary>
    /// Returns all descendant categories (children, grandchildren, …) of the specified category.
    /// </summary>
    /// <param name="category">The category whose descendants are to be retrieved.</param>
    /// <returns>An <see cref="IEnumerable{Category}"/> containing every descendant in depth‑first order.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="category"/> is <c>null</c>.</exception>
    public static IEnumerable<Category> GetAllDescendants(this Category category)
    {
        ArgumentNullException.ThrowIfNull(category);

        foreach (var sub in category.SubCategories ?? Enumerable.Empty<Category>())
        {
            yield return sub;
            foreach (var descendant in sub.GetAllDescendants())
            {
                yield return descendant;
            }
        }
    }

    /// <summary>
    /// Returns a human‑readable string that combines the category name with its full hierarchical path.
    /// </summary>
    /// <param name="category">The category to format.</param>
    /// <returns>A string in the form <c>"{Name} ({FullPath})</c>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="category"/> is <c>null</c>.</exception>
    public static string ToDisplayString(this Category category)
    {
        ArgumentNullException.ThrowIfNull(category);
        return $"{category.Name} ({category.GetFullPath()})";
    }

    /// <summary>
    /// Determines whether the category is a leaf node (i.e., it has no sub‑categories).
    /// </summary>
    /// <param name="category">The category to evaluate.</param>
    /// <returns><c>true</c> if <see cref="Category.SubCategories"/> is empty or null; otherwise, <c>false</c>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="category"/> is <c>null</c>.</exception>
    public static bool IsLeaf(this Category category) => category.SubCategories is null or { Count: 0 };
}