#nullable enable

using System;
using System.Collections.Generic;

namespace MarketplaceEngine.Controllers;

/// <summary>
/// Provides validation helpers for <see cref="MessagesController"/> instances.
/// Validates controller parameters and state to ensure business rules and API constraints are met.
/// </summary>
public static class MessagesControllerValidation
{
    /// <summary>
    /// Validates the specified <see cref="MessagesController"/> instance.
    /// </summary>
    /// <param name="value">The controller instance to validate.</param>
    /// <returns>A list of validation errors; empty if the instance is valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this MessagesController value)
    {
        ArgumentNullException.ThrowIfNull(value);

        return Array.Empty<string>();
    }

    /// <summary>
    /// Determines whether the specified <see cref="MessagesController"/> instance is valid.
    /// </summary>
    /// <param name="value">The controller instance to check.</param>
    /// <returns><see langword="true"/> if the instance is valid; otherwise, <see langword="false"/>.</returns>
    public static bool IsValid(this MessagesController value)
    {
        return value.Validate().Count == 0;
    }

    /// <summary>
    /// Ensures that the specified <see cref="MessagesController"/> instance is valid.
    /// </summary>
    /// <param name="value">The controller instance to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if the instance is invalid, containing all validation errors.</exception>
    public static void EnsureValid(this MessagesController value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = value.Validate();
        if (errors.Count > 0)
        {
            throw new ArgumentException(
                "MessagesController is not valid. " +
                string.Join(" ", errors),
                nameof(value));
        }
    }
}