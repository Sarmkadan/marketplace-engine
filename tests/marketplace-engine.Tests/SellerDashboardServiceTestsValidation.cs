#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Globalization;
using MarketplaceEngine.Domain.Models;
using MarketplaceEngine.Domain.ValueObjects;

namespace MarketplaceEngine.Tests;

/// <summary>
/// Provides validation helpers for test scenarios related to <see cref="SellerDashboardService"/>.
/// Validates test data structures, mock configurations, and expected outcomes.
/// </summary>
public static class SellerDashboardServiceTestsValidation
{
    /// <summary>
    /// Validates that a seller entity is in a valid state for testing.
    /// </summary>
    /// <param name="seller">The seller entity to validate.</param>
    /// <returns>A list of human-readable validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="seller"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this User? seller)
    {
        ArgumentNullException.ThrowIfNull(seller);

        var problems = new List<string>();

        if (string.IsNullOrWhiteSpace(seller.FullName))
        {
            problems.Add("Seller FullName cannot be null or whitespace.");
        }

        if (seller.Id == Guid.Empty)
        {
            problems.Add("Seller Id must be a non-empty GUID.");
        }

        if (seller.TotalSales < 0)
        {
            problems.Add("Seller TotalSales cannot be negative.");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates that a listing entity is in a valid state for testing.
    /// </summary>
    /// <param name="listing">The listing entity to validate.</param>
    /// <returns>A list of human-readable validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="listing"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this Listing? listing)
    {
        ArgumentNullException.ThrowIfNull(listing);

        var problems = new List<string>();

        if (string.IsNullOrWhiteSpace(listing.Title))
        {
            problems.Add("Listing Title cannot be null or whitespace.");
        }

        if (listing.Id == Guid.Empty)
        {
            problems.Add("Listing Id must be a non-empty GUID.");
        }

        if (listing.SellerId == Guid.Empty)
        {
            problems.Add("Listing SellerId must be a non-empty GUID.");
        }

        if (listing.Price is null)
        {
            problems.Add("Listing Price cannot be null.");
        }
        else if (listing.Price.Amount <= 0)
        {
            problems.Add("Listing Price.Amount must be positive.");
        }

        if (listing.ViewCount < 0)
        {
            problems.Add("Listing ViewCount cannot be negative.");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates that a payment entity is in a valid state for testing.
    /// </summary>
    /// <param name="payment">The payment entity to validate.</param>
    /// <returns>A list of human-readable validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="payment"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this Payment? payment)
    {
        ArgumentNullException.ThrowIfNull(payment);

        var problems = new List<string>();

        if (payment.Id == Guid.Empty)
        {
            problems.Add("Payment Id must be a non-empty GUID.");
        }

        if (payment.SellerId == Guid.Empty)
        {
            problems.Add("Payment SellerId must be a non-empty GUID.");
        }

        if (payment.Amount is null)
        {
            problems.Add("Payment Amount cannot be null.");
        }
        else if (payment.Amount.Amount <= 0)
        {
            problems.Add("Payment Amount.Amount must be positive.");
        }

        if (payment.PlatformFee is null)
        {
            problems.Add("Payment PlatformFee cannot be null.");
        }
        else if (payment.PlatformFee.Amount < 0)
        {
            problems.Add("Payment PlatformFee.Amount cannot be negative.");
        }

        if (payment.SellerPayout is null)
        {
            problems.Add("Payment SellerPayout cannot be null.");
        }
        else if (payment.SellerPayout.Amount < 0)
        {
            problems.Add("Payment SellerPayout.Amount cannot be negative.");
        }

        if (payment.CompletedAt.HasValue && payment.CompletedAt.Value > DateTime.UtcNow)
        {
            problems.Add("Payment CompletedAt cannot be in the future.");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether a <see cref="User"/> entity is in a valid state.
    /// </summary>
    /// <param name="seller">The seller entity to check.</param>
    /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
    public static bool IsValid(this User? seller)
    {
        return seller?.Validate() is null or { Count: 0 };
    }

    /// <summary>
    /// Determines whether a <see cref="Listing"/> entity is in a valid state.
    /// </summary>
    /// <param name="listing">The listing entity to check.</param>
    /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
    public static bool IsValid(this Listing? listing)
    {
        return listing?.Validate() is null or { Count: 0 };
    }

    /// <summary>
    /// Determines whether a <see cref="Payment"/> entity is in a valid state.
    /// </summary>
    /// <param name="payment">The payment entity to check.</param>
    /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
    public static bool IsValid(this Payment? payment)
    {
        return payment?.Validate() is null or { Count: 0 };
    }

    /// <summary>
    /// Ensures that a <see cref="User"/> entity is in a valid state.
    /// </summary>
    /// <param name="seller">The seller entity to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="seller"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if the entity contains validation problems.</exception>
    public static void EnsureValid(this User? seller)
    {
        ArgumentNullException.ThrowIfNull(seller);

        var problems = seller.Validate();
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"User entity is invalid. Problems: {string.Join(", ", problems)}",
                nameof(seller));
        }
    }

    /// <summary>
    /// Ensures that a <see cref="Listing"/> entity is in a valid state.
    /// </summary>
    /// <param name="listing">The listing entity to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="listing"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if the entity contains validation problems.</exception>
    public static void EnsureValid(this Listing? listing)
    {
        ArgumentNullException.ThrowIfNull(listing);

        var problems = listing.Validate();
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"Listing entity is invalid. Problems: {string.Join(", ", problems)}",
                nameof(listing));
        }
    }

    /// <summary>
    /// Ensures that a <see cref="Payment"/> entity is in a valid state.
    /// </summary>
    /// <param name="payment">The payment entity to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="payment"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if the entity contains validation problems.</exception>
    public static void EnsureValid(this Payment? payment)
    {
        ArgumentNullException.ThrowIfNull(payment);

        var problems = payment.Validate();
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"Payment entity is invalid. Problems: {string.Join(", ", problems)}",
                nameof(payment));
        }
    }
}