#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using System.Collections.Generic;

namespace MarketplaceEngine.Controllers;

/// <summary>
/// Provides validation helpers for <see cref="PaymentsController"/> instances.
/// Validates controller state and dependencies.
/// </summary>
public static class PaymentsControllerValidation
{
    /// <summary>
    /// Validates that the controller instance is in a valid state.
    /// </summary>
    /// <param name="value">The controller instance to validate.</param>
    /// <returns>A list of human-readable validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if value is null.</exception>
    public static IReadOnlyList<string> Validate(this PaymentsController value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        if (value.GetPaymentService() is null)
        {
            errors.Add("PaymentService dependency is not initialized.");
        }

        if (value.GetLogger() is null)
        {
            errors.Add("Logger dependency is not initialized.");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the controller instance is in a valid state.
    /// </summary>
    /// <param name="value">The controller instance to check.</param>
    /// <returns>True if valid; otherwise, false.</returns>
    public static bool IsValid(this PaymentsController value)
    {
        return value.Validate().Count == 0;
    }

    /// <summary>
    /// Ensures that the controller instance is in a valid state.
    /// </summary>
    /// <param name="value">The controller instance to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if value is null.</exception>
    /// <exception cref="ArgumentException">Thrown if validation fails, containing the list of problems.</exception>
    public static void EnsureValid(this PaymentsController value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = value.Validate();
        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"PaymentsController validation failed:{Environment.NewLine}- {string.Join($"{Environment.NewLine}- ", errors)}");
        }
    }
}