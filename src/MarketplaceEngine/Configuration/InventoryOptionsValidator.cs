#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.Extensions.Options;

namespace MarketplaceEngine.Configuration;

/// <summary>
/// Validates <see cref="InventoryOptions"/> and reports every invalid field in a single failure.
/// </summary>
public sealed class InventoryOptionsValidator : IValidateOptions<InventoryOptions>
{
    /// <summary>
    /// Smallest accepted <see cref="InventoryOptions.LowStockThreshold"/>.
    /// </summary>
    public const int MinLowStockThreshold = 0;

    /// <summary>
    /// Smallest accepted <see cref="InventoryOptions.ReservationTtlMinutes"/>.
    /// </summary>
    public const int MinReservationTtlMinutes = 1;

    /// <summary>
    /// Largest accepted <see cref="InventoryOptions.ReservationTtlMinutes"/> (one day).
    /// </summary>
    public const int MaxReservationTtlMinutes = 1440;

    /// <summary>
    /// Validates the inventory options.
    /// </summary>
    /// <param name="name">The options instance name, if any; not used.</param>
    /// <param name="options">The options to validate.</param>
    /// <returns>Success when all rules hold; otherwise a failure listing each invalid field.</returns>
    public ValidateOptionsResult Validate(string? name, InventoryOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        var failures = new List<string>();

        if (options.LowStockThreshold < MinLowStockThreshold)
            failures.Add($"{InventoryOptions.SectionName}:LowStockThreshold must be {MinLowStockThreshold} or greater (was {options.LowStockThreshold}).");

        if (options.ReservationTtlMinutes < MinReservationTtlMinutes || options.ReservationTtlMinutes > MaxReservationTtlMinutes)
            failures.Add($"{InventoryOptions.SectionName}:ReservationTtlMinutes must be between {MinReservationTtlMinutes} and {MaxReservationTtlMinutes} (was {options.ReservationTtlMinutes}).");

        return failures.Count == 0
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail(failures);
    }
}
