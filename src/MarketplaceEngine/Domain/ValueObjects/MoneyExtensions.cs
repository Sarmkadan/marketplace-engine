#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Globalization;

namespace MarketplaceEngine.Domain.ValueObjects;

/// <summary>
/// Extension methods for the <see cref="Money"/> type providing common monetary operations.
/// </summary>
public static class MoneyExtensions
{
    /// <summary>
    /// Rounds the money amount to the specified number of decimal places.
    /// </summary>
    /// <param name="money">The money value to round.</param>
    /// <param name="decimals">Number of decimal places to round to.</param>
    /// <returns>A new <see cref="Money"/> instance with the rounded amount.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="money"/> is null.</exception>
    public static Money Round(this Money money, int decimals)
    {
        ArgumentNullException.ThrowIfNull(money);

        if (decimals < 0)
            throw new ArgumentOutOfRangeException(nameof(decimals), "Decimals must be non-negative");

        var roundedAmount = decimal.Round(money.Amount, decimals, MidpointRounding.AwayFromZero);
        return new Money(roundedAmount, money.CurrencyCode);
    }

    /// <summary>
    /// Determines whether the money amount is zero.
    /// </summary>
    /// <param name="money">The money value to check.</param>
    /// <returns><see langword="true"/> if the amount is zero; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="money"/> is null.</exception>
    public static bool IsZero(this Money money)
    {
        ArgumentNullException.ThrowIfNull(money);
        return money.Amount == 0m;
    }

    /// <summary>
    /// Determines whether the money amount is positive (greater than zero).
    /// </summary>
    /// <param name="money">The money value to check.</param>
    /// <returns><see langword="true"/> if the amount is positive; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="money"/> is null.</exception>
    public static bool IsPositive(this Money money)
    {
        ArgumentNullException.ThrowIfNull(money);
        return money.Amount > 0m;
    }

    /// <summary>
    /// Determines whether the money amount is negative (less than zero).
    /// </summary>
    /// <param name="money">The money value to check.</param>
    /// <returns><see langword="true"/> if the amount is negative; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="money"/> is null.</exception>
    public static bool IsNegative(this Money money)
    {
        ArgumentNullException.ThrowIfNull(money);
        return money.Amount < 0m;
    }

    /// <summary>
    /// Calculates the percentage that this money amount represents of a total amount.
    /// </summary>
    /// <param name="money">The money value representing the part.</param>
    /// <param name="total">The total money amount.</param>
    /// <returns>The percentage as a decimal value (0-100).</returns>
    /// <exception cref="ArgumentNullException"><paramref name="money"/> or <paramref name="total"/> is null.</exception>
    /// <exception cref="InvalidOperationException"><paramref name="total"/> amount is zero.</exception>
    public static decimal PercentageOf(this Money money, Money total)
    {
        ArgumentNullException.ThrowIfNull(money);
        ArgumentNullException.ThrowIfNull(total);

        if (total.Amount == 0m)
            throw new InvalidOperationException("Total amount cannot be zero");

        return (money.Amount / total.Amount) * 100m;
    }

    /// <summary>
    /// Calculates the percentage value of this money amount.
    /// </summary>
    /// <param name="money">The money value.</param>
    /// <param name="percentage">The percentage to calculate (0-100).</param>
    /// <returns>A new <see cref="Money"/> instance representing the percentage value.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="money"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="percentage"/> is outside the range 0-100.</exception>
    public static Money Percentage(this Money money, decimal percentage)
    {
        ArgumentNullException.ThrowIfNull(money);

        if (percentage < 0 || percentage > 100)
            throw new ArgumentOutOfRangeException(nameof(percentage), "Percentage must be between 0 and 100");

        var resultAmount = money.Amount * (percentage / 100m);
        return new Money(resultAmount, money.CurrencyCode);
    }

    /// <summary>
    /// Compares two money values to determine which is greater.
    /// </summary>
    /// <param name="left">The first money value.</param>
    /// <param name="right">The second money value.</param>
    /// <returns>
    /// A signed number indicating the relative values of <paramref name="left"/> and <paramref name="right"/>:
    /// Less than zero: <paramref name="left"/> is less than <paramref name="right"/>
    /// Zero: <paramref name="left"/> equals <paramref name="right"/>
    /// Greater than zero: <paramref name="left"/> is greater than <paramref name="right"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="left"/> or <paramref name="right"/> is null.</exception>
    public static int CompareTo(this Money left, Money right)
    {
        ArgumentNullException.ThrowIfNull(left);
        ArgumentNullException.ThrowIfNull(right);

        if (!left.CurrencyCode.Equals(right.CurrencyCode, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Cannot compare amounts in different currencies");

        return left.Amount.CompareTo(right.Amount);
    }

    /// <summary>
    /// Determines whether the first money amount is greater than the second.
    /// </summary>
    /// <param name="left">The first money value.</param>
    /// <param name="right">The second money value.</param>
    /// <returns><see langword="true"/> if <paramref name="left"/> is greater than <paramref name="right"/>; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="left"/> or <paramref name="right"/> is null.</exception>
    public static bool IsGreaterThan(this Money left, Money right)
    {
        ArgumentNullException.ThrowIfNull(left);
        ArgumentNullException.ThrowIfNull(right);

        return left.CompareTo(right) > 0;
    }

    /// <summary>
    /// Determines whether the first money amount is greater than or equal to the second.
    /// </summary>
    /// <param name="left">The first money value.</param>
    /// <param name="right">The second money value.</param>
    /// <returns><see langword="true"/> if <paramref name="left"/> is greater than or equal to <paramref name="right"/>; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="left"/> or <paramref name="right"/> is null.</exception>
    public static bool IsGreaterThanOrEqual(this Money left, Money right)
    {
        ArgumentNullException.ThrowIfNull(left);
        ArgumentNullException.ThrowIfNull(right);

        return left.CompareTo(right) >= 0;
    }

    /// <summary>
    /// Determines whether the first money amount is less than the second.
    /// </summary>
    /// <param name="left">The first money value.</param>
    /// <param name="right">The second money value.</param>
    /// <returns><see langword="true"/> if <paramref name="left"/> is less than <paramref name="right"/>; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="left"/> or <paramref name="right"/> is null.</exception>
    public static bool IsLessThan(this Money left, Money right)
    {
        ArgumentNullException.ThrowIfNull(left);
        ArgumentNullException.ThrowIfNull(right);

        return left.CompareTo(right) < 0;
    }

    /// <summary>
    /// Determines whether the first money amount is less than or equal to the second.
    /// </summary>
    /// <param name="left">The first money value.</param>
    /// <param name="right">The second money value.</param>
    /// <returns><see langword="true"/> if <paramref name="left"/> is less than or equal to <paramref name="right"/>; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="left"/> or <paramref name="right"/> is null.</exception>
    public static bool IsLessThanOrEqual(this Money left, Money right)
    {
        ArgumentNullException.ThrowIfNull(left);
        ArgumentNullException.ThrowIfNull(right);

        return left.CompareTo(right) <= 0;
    }

    /// <summary>
    /// Returns the absolute value of the money amount.
    /// </summary>
    /// <param name="money">The money value.</param>
    /// <returns>A new <see cref="Money"/> instance with the absolute amount.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="money"/> is null.</exception>
    public static Money Abs(this Money money)
    {
        ArgumentNullException.ThrowIfNull(money);
        return money.Amount < 0m
            ? new Money(-money.Amount, money.CurrencyCode)
            : money;
    }

    /// <summary>
    /// Returns a new money value with the amount negated.
    /// </summary>
    /// <param name="money">The money value.</param>
    /// <returns>A new <see cref="Money"/> instance with the negated amount.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="money"/> is null.</exception>
    public static Money Negate(this Money money)
    {
        ArgumentNullException.ThrowIfNull(money);
        return new Money(-money.Amount, money.CurrencyCode);
    }

    /// <summary>
    /// Converts the money amount to a string representation with a currency symbol.
    /// </summary>
    /// <param name="money">The money value.</param>
    /// <returns>A formatted string with currency symbol.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="money"/> is null.</exception>
    public static string ToStringWithSymbol(this Money money)
    {
        ArgumentNullException.ThrowIfNull(money);

        var currencySymbol = money.CurrencyCode switch
        {
            "USD" => "$",
            "EUR" => "€",
            "GBP" => "£",
            "JPY" => "¥",
            "CAD" => "CA$",
            "AUD" => "AU$",
            "CHF" => "CHF",
            "CNY" => "¥",
            "INR" => "₹",
            "MXN" => "$",
            _ => money.CurrencyCode
        };

        return $"{currencySymbol}{money.Amount:F2}";
    }
}