#nullable enable

namespace MarketplaceEngine.DTOs;

/// <summary>
/// Provides validation helpers for <see cref="SellerDashboardDto"/> instances.
/// </summary>
public static class SellerDashboardDtoValidation
{
    /// <summary>
    /// Validates a <see cref="SellerDashboardDto"/> instance and returns a list of validation problems.
    /// </summary>
    /// <param name="value">The DTO to validate.</param>
    /// <returns>An empty list if valid; otherwise, a list of human-readable validation errors.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this SellerDashboardDto value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        // Validate SellerId
        if (value.SellerId == Guid.Empty)
        {
            errors.Add("SellerId must be a non-empty GUID.");
        }

        // Validate SellerName
        if (string.IsNullOrWhiteSpace(value.SellerName))
        {
            errors.Add("SellerName cannot be null or whitespace.");
        }
        else if (value.SellerName.Length > 200)
        {
            errors.Add("SellerName cannot exceed 200 characters.");
        }

        // Validate ActiveListings
        if (value.ActiveListings < 0)
        {
            errors.Add("ActiveListings cannot be negative.");
        }

        // Validate TotalListings
        if (value.TotalListings < 0)
        {
            errors.Add("TotalListings cannot be negative.");
        }

        // Validate TotalSales
        if (value.TotalSales < 0)
        {
            errors.Add("TotalSales cannot be negative.");
        }

        // Validate TotalRevenue
        if (value.TotalRevenue < 0)
        {
            errors.Add("TotalRevenue cannot be negative.");
        }

        // Validate PendingPayout
        if (value.PendingPayout < 0)
        {
            errors.Add("PendingPayout cannot be negative.");
        }

        // Validate AverageRating
        if (value.AverageRating < 0 || value.AverageRating > 5)
        {
            errors.Add("AverageRating must be between 0 and 5.");
        }

        // Validate TotalReviews
        if (value.TotalReviews < 0)
        {
            errors.Add("TotalReviews cannot be negative.");
        }

        // Validate UnreadMessages
        if (value.UnreadMessages < 0)
        {
            errors.Add("UnreadMessages cannot be negative.");
        }

        // Validate LastActivityAt
        if (value.LastActivityAt.HasValue && value.LastActivityAt.Value > DateTime.UtcNow)
        {
            errors.Add("LastActivityAt cannot be in the future.");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether a <see cref="SellerDashboardDto"/> instance is valid.
    /// </summary>
    /// <param name="value">The DTO to check.</param>
    /// <returns>True if valid; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static bool IsValid(this SellerDashboardDto value)
    {
        return value.Validate().Count == 0;
    }

    /// <summary>
    /// Ensures that a <see cref="SellerDashboardDto"/> instance is valid, throwing an exception if not.
    /// </summary>
    /// <param name="value">The DTO to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="value"/> is invalid, containing a list of validation errors.</exception>
    public static void EnsureValid(this SellerDashboardDto value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = value.Validate();
        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"SellerDashboardDto validation failed:{Environment.NewLine}{string.Join(Environment.NewLine, errors)}");
        }
    }
}