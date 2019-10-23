// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace MarketplaceEngine.Domain.ValueObjects;

/// <summary>
/// Immutable value object representing monetary amounts with currency.
/// </summary>
public sealed class Money : IEquatable<Money>
{
    public decimal Amount { get; }
    public string CurrencyCode { get; }

    public Money(decimal amount, string currencyCode = "USD")
    {
        // Validates that amount is non-negative and currency code is valid
        if (amount < 0)
            throw new ArgumentException("Amount cannot be negative", nameof(amount));

        if (string.IsNullOrWhiteSpace(currencyCode) || currencyCode.Length != 3)
            throw new ArgumentException("Currency code must be a valid 3-letter ISO code", nameof(currencyCode));

        Amount = amount;
        CurrencyCode = currencyCode.ToUpperInvariant();
    }

    // Adds two money values, ensures same currency
    public Money Add(Money other)
    {
        if (!CurrencyCode.Equals(other.CurrencyCode, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Cannot add amounts in different currencies");

        return new Money(Amount + other.Amount, CurrencyCode);
    }

    // Subtracts two money values, ensures same currency
    public Money Subtract(Money other)
    {
        if (!CurrencyCode.Equals(other.CurrencyCode, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Cannot subtract amounts in different currencies");

        return new Money(Amount - other.Amount, CurrencyCode);
    }

    // Multiplies money by a scalar value
    public Money Multiply(decimal multiplier)
    {
        if (multiplier < 0)
            throw new ArgumentException("Multiplier cannot be negative", nameof(multiplier));

        return new Money(Amount * multiplier, CurrencyCode);
    }

    public bool Equals(Money? other)
    {
        return other != null &&
               Amount == other.Amount &&
               CurrencyCode == other.CurrencyCode;
    }

    public override bool Equals(object? obj) => Equals(obj as Money);

    public override int GetHashCode() => HashCode.Combine(Amount, CurrencyCode);

    public override string ToString() => $"{CurrencyCode} {Amount:F2}";

    public static bool operator ==(Money? left, Money? right) => Equals(left, right);
    public static bool operator !=(Money? left, Money? right) => !Equals(left, right);
}
