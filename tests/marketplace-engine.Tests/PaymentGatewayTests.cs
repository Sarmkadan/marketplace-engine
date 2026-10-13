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
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace MarketplaceEngine.Tests;

/// <summary>
/// Contains integration tests for the <see cref="PaymentGateway"/> class.
/// Tests resilience scenarios including transient failures, timeouts, and refunds.
/// </summary>
public class PaymentGatewayTests
{
    private readonly Mock<IPaymentRepository> _paymentRepoMock;
    private readonly PaymentGateway _gateway;
    private readonly TestPaymentProvider _testProvider;

    public PaymentGatewayTests()
    {
        _paymentRepoMock = new Mock<IPaymentRepository>();
        _testProvider = new TestPaymentProvider();
        _gateway = new PaymentGateway(_paymentRepoMock.Object, NullLogger<PaymentGateway>.Instance, _testProvider);
    }

    /// <summary>
    /// Test payment gateway resilience: transient failure on first authorization attempt followed by success.
    /// Verifies that retrying results in exactly one successful charge (no double-charge).
    /// </summary>
    [Fact]
    public async Task AuthorizeAsync_TransientFailureThenSuccess_RetriesResultInSingleCharge()
    {
        // Arrange
        var paymentId = Guid.NewGuid();
        var payment = new Payment
        {
            Id = paymentId,
            Status = PaymentStatus.Pending,
            Amount = new Money(100m, "USD")
        };

        _paymentRepoMock.Setup(r => r.GetByIdAsync(paymentId))
            .ReturnsAsync(payment);
        _paymentRepoMock.Setup(r => r.UpdateAsync(It.IsAny<Payment>()))
            .ReturnsAsync((Payment p) => p);

        // Simulate transient failure: first authorization attempt fails, second succeeds
        _testProvider.SetFailOnFirstAttempt(paymentId, true);

        // Act & Assert: First authorization attempt fails
        Func<Task> firstAuthorize = async () => await _gateway.AuthorizeAsync(paymentId);
        await firstAuthorize.Should().ThrowAsync<MarketplaceException>()
            .WithMessage("Authorization declined by provider (simulated).*");

        // Verify payment state after failed authorization (should remain Pending)
        // Note: On authorization failure, we do NOT update the repository
        _paymentRepoMock.Verify(r => r.UpdateAsync(It.IsAny<Payment>()), Times.Never);

        // Act & Assert: Second authorization attempt succeeds (simulating retry)
        var authorizedPayment = await _gateway.AuthorizeAsync(paymentId);
        authorizedPayment.Should().NotBeNull();
        authorizedPayment.Status.Should().Be(PaymentStatus.Processing);
        authorizedPayment.ExternalTransactionId.Should().NotBeNullOrEmpty();

        // Act: Capture the authorized payment
        var capturedPayment = await _gateway.CaptureAsync(paymentId);
        capturedPayment.Status.Should().Be(PaymentStatus.Completed);

        // Assert: Provider methods called expected number of times
        _testProvider.AuthorizeAttempts[paymentId].Should().Be(2, "because first attempt failed, second succeeded");
        _testProvider.CaptureAttempts[paymentId].Should().Be(1, "because capture called once after successful authorization");
    }

    /// <summary>
    /// Test that a timeout produces a distinct failure outcome from a declined card.
    /// </summary>
    [Fact]
    public async Task AuthorizeAsync_TimeoutVsDeclinedCard_DistinctExceptions()
    {
        // Arrange
        var paymentId = Guid.NewGuid();
        var payment = new Payment
        {
            Id = paymentId,
            Status = PaymentStatus.Pending,
            Amount = new Money(100m, "USD")
        };

        _paymentRepoMock.Setup(r => r.GetByIdAsync(paymentId))
            .ReturnsAsync(payment);
        _paymentRepoMock.Setup(r => r.UpdateAsync(It.IsAny<Payment>()))
            .ReturnsAsync((Payment p) => p);

        // Act & Assert: Declined card throws MarketplaceException with specific message
        _testProvider.SetFailOnFirstAttempt(paymentId, true); // Simulate decline
        Func<Task> declinedAct = async () => await _gateway.AuthorizeAsync(paymentId);
        await declinedAct.Should().ThrowAsync<MarketplaceException>()
            .WithMessage("Authorization declined by provider (simulated).*");

        // Reset for timeout test
        _testProvider.Reset();

        // Act & Assert: Timeout throws TimeoutException (or similar)
        _testProvider.SetTimeoutOnFirstAttempt(paymentId, true);
        Func<Task> timeoutAct = async () => await _gateway.AuthorizeAsync(paymentId);
        await timeoutAct.Should().ThrowAsync<TimeoutException>()
            .WithMessage("Payment gateway timeout.*");
    }

    /// <summary>
    /// Test that refund of a captured payment works and refund of an unknown payment id fails with a clear error.
    /// </summary>
    [Fact]
    public async Task RefundAsync_RefundCapturedPaymentWorks_RefundUnknownPaymentFails()
    {
        // Arrange
        var paymentId = Guid.NewGuid();
        var unknownPaymentId = Guid.NewGuid();

        var capturedPayment = new Payment
        {
            Id = paymentId,
            Status = PaymentStatus.Completed,
            Amount = new Money(100m, "USD"),
            ExternalTransactionId = "txn_123"
        };

        _paymentRepoMock.Setup(r => r.GetByIdAsync(paymentId))
            .ReturnsAsync(capturedPayment);
        _paymentRepoMock.Setup(r => r.GetByIdAsync(unknownPaymentId))
            .ReturnsAsync((Payment?)null);
        _paymentRepoMock.Setup(r => r.UpdateAsync(It.IsAny<Payment>()))
            .ReturnsAsync((Payment p) => p);

        // Act & Assert: Refund of captured payment succeeds
        var refundedPayment = await _gateway.RefundAsync(paymentId, "Test refund");
        refundedPayment.Status.Should().Be(PaymentStatus.Refunded);
        refundedPayment.RefundReason.Should().Be("Test refund");

        // Act & Assert: Refund of unknown payment throws ResourceNotFoundException
        Func<Task> refundUnknownAct = async () => await _gateway.RefundAsync(unknownPaymentId, "Test refund");
        await refundUnknownAct.Should().ThrowAsync<MarketplaceException>()
            .WithMessage($"Payment {unknownPaymentId} not found.*");
    }
}

/// <summary>
/// Test double for IPaymentProvider that simulates transient failures, timeouts, and tracks call counts.
/// </summary>
public class TestPaymentProvider : IPaymentProvider
{
    // Tracks number of authorization attempts per paymentId
    public Dictionary<Guid, int> AuthorizeAttempts { get; } = new();
    // Tracks number of capture attempts per paymentId
    public Dictionary<Guid, int> CaptureAttempts { get; } = new();
    // Tracks number of refund attempts per paymentId
    public Dictionary<Guid, int> RefundAttempts { get; } = new();

    // Flags to control behavior per paymentId
    private readonly Dictionary<Guid, bool> _failOnFirstAttempt = new();
    private readonly Dictionary<Guid, bool> _timeoutOnFirstAttempt = new();

    public void SetFailOnFirstAttempt(Guid paymentId, bool fail)
    {
        _failOnFirstAttempt[paymentId] = fail;
    }

    public void SetTimeoutOnFirstAttempt(Guid paymentId, bool timeout)
    {
        _timeoutOnFirstAttempt[paymentId] = timeout;
    }

    public void Reset()
    {
        AuthorizeAttempts.Clear();
        CaptureAttempts.Clear();
        RefundAttempts.Clear();
        _failOnFirstAttempt.Clear();
        _timeoutOnFirstAttempt.Clear();
    }

    public Task<Payment> AuthorizeAsync(Payment payment)
    {
        IncrementAndTrack(AuthorizeAttempts, payment.Id);

        if (_timeoutOnFirstAttempt.TryGetValue(payment.Id, out var timeout) && timeout && AuthorizeAttempts[payment.Id] == 1)
        {
            throw new TimeoutException("Payment gateway timeout.");
        }

        if (_failOnFirstAttempt.TryGetValue(payment.Id, out var fail) && fail && AuthorizeAttempts[payment.Id] == 1)
        {
            throw new MarketplaceException("Authorization declined by provider (simulated).");
        }

        // Simulate successful authorization
        payment.Status = PaymentStatus.Processing;
        payment.ExternalTransactionId = $"txn_{Guid.NewGuid():N}"[..24];
        payment.UpdatedAt = DateTime.UtcNow;
        return Task.FromResult(payment);
    }

    public Task<Payment> CaptureAsync(Payment payment)
    {
        IncrementAndTrack(CaptureAttempts, payment.Id);

        if (payment.Status != PaymentStatus.Processing)
            throw new MarketplaceException($"Payment must be in Processing state to capture (current state: {payment.Status}).");

        payment.Status = PaymentStatus.Completed;
        payment.CompletedAt = DateTime.UtcNow;
        payment.UpdatedAt = DateTime.UtcNow;
        return Task.FromResult(payment);
    }

    public Task<Payment> RefundAsync(Payment payment, string reason)
    {
        IncrementAndTrack(RefundAttempts, payment.Id);

        if (string.IsNullOrWhiteSpace(reason))
            throw new ValidationException("Reason", "Refund reason is required.");

        if (payment.Status != PaymentStatus.Completed)
            throw new MarketplaceException($"Only completed payments can be refunded (current state: {payment.Status}).");

        payment.Status = PaymentStatus.Refunded;
        payment.RefundReason = reason;
        payment.RefundedAt = DateTime.UtcNow;
        payment.UpdatedAt = DateTime.UtcNow;
        return Task.FromResult(payment);
    }

    private void IncrementAndTrack(Dictionary<Guid, int> dictionary, Guid key)
    {
        if (!dictionary.ContainsKey(key))
            dictionary[key] = 0;
        dictionary[key]++;
    }
}
