// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace MarketplaceEngine.Utilities;

/// <summary>
/// Pagination calculation utilities to standardize paging across the application.
/// Handles offset calculation, bounds checking, and page number validation.
/// </summary>
public static class PaginationUtility
{
    private const int DefaultPageSize = 20;
    private const int MaxPageSize = 100;
    private const int MinPageSize = 1;

    /// <summary>
    /// Calculates the offset for a database query based on page and page size.
    /// </summary>
    public static int CalculateOffset(int page, int pageSize)
    {
        ValidatePageParameters(ref page, ref pageSize);
        return (page - 1) * pageSize;
    }

    /// <summary>
    /// Validates and normalizes page parameters, constraining them to safe values.
    /// </summary>
    public static void ValidatePageParameters(ref int page, ref int pageSize)
    {
        if (page < 1)
            page = 1;

        if (pageSize < MinPageSize)
            pageSize = DefaultPageSize;

        if (pageSize > MaxPageSize)
            pageSize = MaxPageSize;
    }

    /// <summary>
    /// Calculates total number of pages for a given total item count.
    /// </summary>
    public static int CalculateTotalPages(int totalItems, int pageSize)
    {
        if (pageSize <= 0)
            pageSize = DefaultPageSize;

        return (int)Math.Ceiling((double)totalItems / pageSize);
    }

    /// <summary>
    /// Determines if there are more items beyond the current page.
    /// </summary>
    public static bool HasNextPage(int currentPage, int pageSize, int totalItems)
    {
        var totalPages = CalculateTotalPages(totalItems, pageSize);
        return currentPage < totalPages;
    }

    /// <summary>
    /// Determines if there are items before the current page.
    /// </summary>
    public static bool HasPreviousPage(int currentPage)
    {
        return currentPage > 1;
    }

    /// <summary>
    /// Gets the next page number if available.
    /// </summary>
    public static int? GetNextPage(int currentPage, int pageSize, int totalItems)
    {
        return HasNextPage(currentPage, pageSize, totalItems) ? currentPage + 1 : null;
    }

    /// <summary>
    /// Gets the previous page number if available.
    /// </summary>
    public static int? GetPreviousPage(int currentPage)
    {
        return HasPreviousPage(currentPage) ? currentPage - 1 : null;
    }

    /// <summary>
    /// Returns default page size for the application.
    /// </summary>
    public static int GetDefaultPageSize()
    {
        return DefaultPageSize;
    }

    /// <summary>
    /// Returns the maximum allowed page size.
    /// </summary>
    public static int GetMaxPageSize()
    {
        return MaxPageSize;
    }
}

/// <summary>
/// Generic pagination metadata holder.
/// </summary>
public class PaginationInfo
{
    public int CurrentPage { get; set; }
    public int PageSize { get; set; }
    public int TotalItems { get; set; }
    public int TotalPages => PaginationUtility.CalculateTotalPages(TotalItems, PageSize);
    public bool HasNextPage => PaginationUtility.HasNextPage(CurrentPage, PageSize, TotalItems);
    public bool HasPreviousPage => PaginationUtility.HasPreviousPage(CurrentPage);
    public int? NextPage => PaginationUtility.GetNextPage(CurrentPage, PageSize, TotalItems);
    public int? PreviousPage => PaginationUtility.GetPreviousPage(CurrentPage);
}
