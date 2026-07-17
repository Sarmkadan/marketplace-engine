#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using System.Collections.Generic;
using System.Globalization;

namespace MarketplaceEngine.Infrastructure.Security;

/// <summary>
/// Provides validation helpers for <see cref="PermissionService"/> instances.
/// </summary>
public static class PermissionServiceValidation
{
    /// <summary>
    /// Validates that the <see cref="PermissionService"/> instance is properly configured.
    /// </summary>
    /// <param name="value">The permission service to validate.</param>
    /// <returns>A list of human-readable validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this PermissionService? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified <see cref="PermissionService"/> instance is valid.
    /// </summary>
    /// <param name="value">The permission service to check.</param>
    /// <returns><see langword="true"/> if the service is valid; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static bool IsValid(this PermissionService? value)
    {
        return value is not null && value.Validate().Count == 0;
    }

    /// <summary>
    /// Ensures that the specified <see cref="PermissionService"/> instance is valid.
    /// </summary>
    /// <param name="value">The permission service to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if the service has validation problems.</exception>
    public static void EnsureValid(this PermissionService? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = value.Validate();
        if (problems.Count > 0)
        {
            throw new ArgumentException($"PermissionService is not valid. Problems:\n- {string.Join("\n- ", problems)}".TrimEnd());
        }
    }
}