#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Globalization;

namespace MarketplaceEngine.Tests;

/// <summary>
/// Provides validation helpers for <see cref="SearchServiceTests"/> instances.
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

        // SearchServiceTests is a test fixture class that should be properly initialized
        // Validation ensures the instance itself is not null (the constructor initializes private fields)
        return Array.Empty<string>();
    }

    /// <summary>
    /// Determines whether the specified <see cref="SearchServiceTests"/> instance is valid.
    /// </summary>
    /// <param name="value">The instance to check.</param>
    /// <returns>True if valid; otherwise, false.</returns>
    public static bool IsValid(this SearchServiceTests? value)
    {
        return value?.Validate().Count == 0;
    }

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
                $"SearchServiceTests instance is not valid:{Environment.NewLine}{string.Join(Environment.NewLine, problems)}");
        }
    }
}