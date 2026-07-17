#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Globalization;

namespace MarketplaceEngine.Tests;

/// <summary>
/// Validation helpers for test utility classes and test data.
/// Provides comprehensive validation for test configurations and utility method inputs.
/// </summary>
public static class UtilityTestsValidation
{
    /// <summary>
    /// Validates a UtilityTests instance and returns a list of human-readable problems.
    /// </summary>
    /// <param name="value">The UtilityTests instance to validate.</param>
    /// <returns>A list of validation problems (empty if valid).</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this UtilityTests value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Validate Email for IsValidEmail tests
        if (string.IsNullOrWhiteSpace(value.EmailForIsValidEmailTest))
        {
            problems.Add("EmailForIsValidEmailTest must not be null or empty.");
        }
        else if (value.EmailForIsValidEmailTest.Length > 254)
        {
            problems.Add("EmailForIsValidEmailTest exceeds maximum length of 254 characters.");
        }

        // Validate Price for IsValidPrice tests
        if (value.PriceForIsValidPriceTest < 0.01m)
        {
            problems.Add("PriceForIsValidPriceTest must be at least 0.01.");
        }
        else if (value.PriceForIsValidPriceTest > 999999.99m)
        {
            problems.Add("PriceForIsValidPriceTest exceeds maximum value of 999999.99.");
        }

        // Validate Input for SanitizeInput tests
        if (value.InputForSanitizeTest is null)
        {
            problems.Add("InputForSanitizeTest must not be null.");
        }
        else if (value.InputForSanitizeTest.Length > 1000)
        {
            problems.Add("InputForSanitizeTest exceeds maximum length of 1000 characters.");
        }

        // Validate Text for Truncate tests
        if (string.IsNullOrWhiteSpace(value.TextForTruncateTest))
        {
            problems.Add("TextForTruncateTest must not be null or empty.");
        }
        else if (value.TextForTruncateTest.Length > 10000)
        {
            problems.Add("TextForTruncateTest exceeds maximum length of 10000 characters.");
        }

        // Validate Title for ToSlug tests
        if (string.IsNullOrWhiteSpace(value.TitleForToSlugTest))
        {
            problems.Add("TitleForToSlugTest must not be null or empty.");
        }
        else if (value.TitleForToSlugTest.Length > 500)
        {
            problems.Add("TitleForToSlugTest exceeds maximum length of 500 characters.");
        }

        // Validate Email for MaskEmail tests
        if (string.IsNullOrWhiteSpace(value.EmailForMaskEmailTest))
        {
            problems.Add("EmailForMaskEmailTest must not be null or empty.");
        }
        else if (value.EmailForMaskEmailTest.Length > 254)
        {
            problems.Add("EmailForMaskEmailTest exceeds maximum length of 254 characters.");
        }

        // Validate Page and PageSize for pagination tests
        if (value.PageForCalculateOffsetTest < 1)
        {
            problems.Add("PageForCalculateOffsetTest must be at least 1.");
        }
        if (value.PageSizeForCalculateOffsetTest < 1)
        {
            problems.Add("PageSizeForCalculateOffsetTest must be at least 1.");
        }
        else if (value.PageSizeForCalculateOffsetTest > 100)
        {
            problems.Add("PageSizeForCalculateOffsetTest exceeds maximum value of 100.");
        }

        if (value.TotalItemsForCalculateTotalPagesTest < 0)
        {
            problems.Add("TotalItemsForCalculateTotalPagesTest must be non-negative.");
        }
        if (value.PageSizeForCalculateTotalPagesTest < 1)
        {
            problems.Add("PageSizeForCalculateTotalPagesTest must be at least 1.");
        }
        else if (value.PageSizeForCalculateTotalPagesTest > 100)
        {
            problems.Add("PageSizeForCalculateTotalPagesTest exceeds maximum value of 100.");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether a UtilityTests instance is valid.
    /// </summary>
    /// <param name="value">The UtilityTests instance to check.</param>
    /// <returns>True if valid; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static bool IsValid(this UtilityTests value)
    {
        return value.Validate().Count is 0;
    }

    /// <summary>
    /// Ensures that a UtilityTests instance is valid, throwing an exception if not.
    /// </summary>
    /// <param name="value">The UtilityTests instance to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="value"/> contains validation problems.</exception>
    public static void EnsureValid(this UtilityTests value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = value.Validate();

        if (problems.Count is not 0)
        {
            throw new ArgumentException(
                $"UtilityTests validation failed:{Environment.NewLine}- {
                    string.Join($"{Environment.NewLine}- ", problems)}");
        }
    }
}
