#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FluentAssertions;
using MarketplaceEngine.Domain.Enums;
using MarketplaceEngine.Domain.Models;
using MarketplaceEngine.Domain.ValueObjects;
using MarketplaceEngine.Exceptions;
using MarketplaceEngine.Repositories;
using MarketplaceEngine.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MarketplaceEngine.Tests;

/// <summary>
/// Verifies the structured log output of <see cref="PaymentGateway"/> using a captured logger provider.
/// </summary>
public class PaymentGatewayLoggingTests
{
    /// <summary>
    /// Stand-in for card data. It is placed on the payment so the tests can prove it never reaches a log entry.
    /// </summary>
    private const string CardNumberCanary = "4242424242424242";

    private readonly CapturingLoggerProvider _loggerProvider = new();
    private readonly Mock<IPaymentRepository> _paymentRepoMock = new();

    private ILogger<PaymentGateway> CreateLogger() =>
        new Logger<PaymentGateway>(new LoggerFactory(new[] { _loggerProvider }));

    private Payment CreatePendingPayment() => new()
    {
        Id = Guid.NewGuid(),
        Status = PaymentStatus.Pending,
        Amount = new Money(100m, "USD"),
        PaymentMethod = CardNumberCanary
    };

    /// <summary>
    /// A declined authorization must produce a Warning carrying the OrderId and DeclineCode,
    /// and nothing in the output may contain the card number.
    /// </summary>
    [Fact]
    public async Task AuthorizeAsync_DeclinedPayment_LogsWarningWithOrderIdAndDeclineCodeWithoutSensitiveValues()
    {
        // Arrange
        var payment = CreatePendingPayment();
        _paymentRepoMock.Setup(r => r.GetByIdAsync(payment.Id)).ReturnsAsync(payment);

        var gateway = new PaymentGateway(_paymentRepoMock.Object, CreateLogger(), new DefaultPaymentProvider(failureRate: 1.0));

        // Act
        Func<Task> act = () => gateway.AuthorizeAsync(payment.Id);

        // Assert: the decline is still surfaced to the caller
        await act.Should().ThrowAsync<MarketplaceException>();

        var entries = _loggerProvider.Entries;
        var warning = entries.Should().ContainSingle(e => e.EventId.Name == "AuthorizationDeclined").Subject;

        warning.Level.Should().Be(LogLevel.Warning);
        warning.Properties["OrderId"].Should().Be(payment.Id);
        warning.Properties["DeclineCode"].Should().Be("simulated_decline");
        warning.Message.Should().Contain(payment.Id.ToString()).And.Contain("simulated_decline");

        entries.Should().NotContain(e =>
            e.Message.Contains(CardNumberCanary) ||
            e.Properties.Values.Any(v => v != null && v.ToString()!.Contains(CardNumberCanary)));
    }

    /// <summary>
    /// A successful authorization logs the attempt and the outcome under the same idempotency key.
    /// </summary>
    [Fact]
    public async Task AuthorizeAsync_SuccessfulPayment_LogsAttemptAndSuccessWithIdempotencyKey()
    {
        // Arrange
        var payment = CreatePendingPayment();
        _paymentRepoMock.Setup(r => r.GetByIdAsync(payment.Id)).ReturnsAsync(payment);
        _paymentRepoMock.Setup(r => r.UpdateAsync(It.IsAny<Payment>())).ReturnsAsync((Payment p) => p);

        var gateway = new PaymentGateway(_paymentRepoMock.Object, CreateLogger(), new DefaultPaymentProvider());

        // Act
        var authorized = await gateway.AuthorizeAsync(payment.Id);

        // Assert
        var entries = _loggerProvider.Entries;

        var attempted = entries.Should().ContainSingle(e => e.EventId.Name == "AuthorizationAttempted").Subject;
        attempted.Level.Should().Be(LogLevel.Information);
        attempted.Properties["OrderId"].Should().Be(payment.Id);
        attempted.Properties["IdempotencyKey"].Should().Be(payment.Id);

        var succeeded = entries.Should().ContainSingle(e => e.EventId.Name == "AuthorizationSucceeded").Subject;
        succeeded.Level.Should().Be(LogLevel.Information);
        succeeded.Properties["OrderId"].Should().Be(payment.Id);
        succeeded.Properties["IdempotencyKey"].Should().Be(payment.Id);
        succeeded.Properties["TransactionId"].Should().Be(authorized.ExternalTransactionId);

        entries.Should().NotContain(e => e.Level >= LogLevel.Warning);
    }
}
