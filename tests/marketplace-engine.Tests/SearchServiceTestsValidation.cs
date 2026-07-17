#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;

namespace MarketplaceEngine.Tests;

/// <summary>
/// Provides validation helpers for <see cref="SearchServiceTests"/> instances.
/// This class contains test fixture validation logic to ensure test setup is correct.
/// </summary>
public static class SearchServiceTestsValidation
{
    /// <summary>
    /// Validates the specified <see cref="SearchServiceTests"/> instance.
    /// </summary>
    /// <param name="value">The instance to validate.</param>
    /// <returns>A read-only list of human-readable validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this SearchServiceTests? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        // SearchServiceTests is a test fixture class that uses mock repositories.
        // The constructor initializes all required dependencies, so no additional validation is needed.
        // This method exists for API consistency with other test validation helpers.
        return Array.Empty<string>();
    }

    /// <summary>
    /// Determines whether the specified <see cref="SearchServiceTests"/> instance is valid.
    /// </summary>
    /// <param name="value">The instance to check.</param>
    /// <returns>True if valid; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static bool IsValid(this SearchServiceTests? value) => value?.Validate().Count == 0;

    /// <summary>
    /// Ensures that the specified <see cref="SearchServiceTests"/> instance is valid.
    /// </summary>
    /// <param name="value">The instance to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if the instance is not valid, containing a list of problems.</exception>
    public static void EnsureValid(this SearchServiceTests? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = value.Validate();
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"SearchServiceTests instance is not valid:{Environment.NewLine}{string.Join(Environment.NewLine, problems)}",
                nameof(value));
        }
    }
}