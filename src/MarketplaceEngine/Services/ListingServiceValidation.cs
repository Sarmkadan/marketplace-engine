#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System.Globalization;
using MarketplaceEngine.Constants;
using MarketplaceEngine.Domain.Enums;
using MarketplaceEngine.Domain.Models;
using MarketplaceEngine.Domain.ValueObjects;

namespace MarketplaceEngine.Services;

/// <summary>
/// Provides validation helpers for <see cref="ListingService"/> instances.
/// </summary>
public static class ListingServiceValidation
{
    /// <summary>
    /// Validates a <see cref="ListingService"/> instance and returns a list of human-readable validation problems.
    /// </summary>
    /// <param name="value">The listing service instance to validate.</param>
    /// <returns>A read-only list of validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this ListingService value)
    {
        ArgumentNullException.ThrowIfNull(value);

        return Array.Empty<string>();
    }

    /// <summary>
    /// Determines whether the specified <see cref="ListingService"/> instance is valid.
    /// </summary>
    /// <param name="value">The listing service instance to check.</param>
    /// <returns>True if valid; otherwise, false.</returns>
    public static bool IsValid(this ListingService value)
    {
        return Validate(value).Count == 0;
    }

    /// <summary>
    /// Ensures that the specified <see cref="ListingService"/> instance is valid.
    /// </summary>
    /// <param name="value">The listing service instance to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="value"/> is not valid.</exception>
    public static void EnsureValid(this ListingService value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = Validate(value);
        if (problems.Count == 0)
        {
            return;
        }

        throw new ArgumentException(
            $"ListingService is not valid. Problems: {string.Join(", ", problems)}");
    }
}