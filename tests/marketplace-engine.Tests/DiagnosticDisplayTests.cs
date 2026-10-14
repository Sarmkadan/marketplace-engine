#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Diagnostics;
using System.Reflection;
using FluentAssertions;
using MarketplaceEngine.Domain.Enums;
using MarketplaceEngine.Domain.Models;
using MarketplaceEngine.Domain.ValueObjects;
using Xunit;

namespace MarketplaceEngine.Tests;

/// <summary>
/// Tests that the diagnostic <c>ToString</c> output and <c>DebuggerDisplay</c> attributes
/// expose the key fields of order-like types.
/// </summary>
public class DiagnosticDisplayTests
{
    /// <summary>
    /// Builds a completed payment, the order-like type in this codebase.
    /// </summary>
    private static Payment CompletedPayment(Guid id, Guid listingId) => new()
    {
        Id = id,
        ListingId = listingId,
        Amount = new Money(25m, "EUR"),
        Status = PaymentStatus.Completed
    };

    [Fact]
    public void Payment_ToString_ContainsIdStatusTotalAndCurrency()
    {
        var id = Guid.NewGuid();
        var listingId = Guid.NewGuid();
        var payment = CompletedPayment(id, listingId);

        var text = payment.ToString();

        text.Should().Contain(id.ToString());
        text.Should().Contain("Completed");
        text.Should().Contain("25.00 EUR");
        text.Should().Contain("EUR");
        text.Should().Contain(listingId.ToString());
    }

    [Fact]
    public void Payment_DebuggerDisplay_ContainsIdStatusTotalAndCurrency()
    {
        var id = Guid.NewGuid();
        var payment = CompletedPayment(id, Guid.NewGuid());

        var attribute = typeof(Payment).GetCustomAttribute<DebuggerDisplayAttribute>();

        attribute.Should().NotBeNull();
        attribute!.Value.Should().Contain("{Id}");
        attribute.Value.Should().Contain("{Status}");
        attribute.Value.Should().Contain("{Amount.Amount}");
        attribute.Value.Should().Contain("{Amount.CurrencyCode}");
        payment.ToString().Should().Contain(id.ToString());
    }
}
