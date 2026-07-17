#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.AspNetCore.Mvc;
using MarketplaceEngine.DTOs;
using MarketplaceEngine.Services;

namespace MarketplaceEngine.Controllers;

/// <summary>
/// Handles payment lifecycle operations including initiation, processing,
/// escrow management, completion, cancellation, and refunds.
/// </summary>
[ApiController]
[Route("api/v1/payments")]
public class PaymentsController : ControllerBase
{
    private readonly PaymentService _paymentService;
    private readonly ILogger<PaymentsController> _logger;

    public PaymentsController(PaymentService paymentService, ILogger<PaymentsController> logger)
    {
        _paymentService = paymentService;
        _logger = logger;
    }

    /// <summary>
    /// Gets the PaymentService dependency for validation purposes.
    /// </summary>
    internal PaymentService GetPaymentService() => _paymentService;

    /// <summary>
    /// Gets the logger dependency for validation purposes.
    /// </summary>
    internal ILogger<PaymentsController> GetLogger() => _logger;

    /// <summary>
    /// Initiates a new payment for a listing purchase.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(PaymentDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> InitiatePayment([FromBody] InitiatePaymentRequest request)
    {
        _logger.LogInformation("Initiating payment for listing {ListingId} by buyer {BuyerId}",
            request.ListingId, request.BuyerId);

        if (request.ListingId == Guid.Empty || request.BuyerId == Guid.Empty)
            return BadRequest("ListingId and BuyerId are required.");

        if (string.IsNullOrWhiteSpace(request.PaymentMethod))
            return BadRequest("PaymentMethod is required.");

        var payment = await _paymentService.InitiatePaymentAsync(
            request.ListingId, request.BuyerId, request.PaymentMethod, request.Currency);

        _logger.LogInformation("Payment {PaymentId} initiated", payment.Id);
        return CreatedAtAction(nameof(GetPayment), new { id = payment.Id }, new PaymentDto(payment));
    }

    /// <summary>
    /// Retrieves a payment by ID.
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(PaymentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPayment(Guid id)
    {
        var payment = await _paymentService.GetPaymentAsync(id);
        return Ok(new PaymentDto(payment));
    }

    /// <summary>
    /// Transitions a payment to the processing state before charging the provider.
    /// </summary>
    [HttpPost("{id}/process")]
    [ProducesResponseType(typeof(PaymentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> StartProcessing(Guid id, [FromQuery] Guid requesterId)
    {
        var payment = await _paymentService.StartProcessingAsync(id, requesterId);
        return Ok(new PaymentDto(payment));
    }

    /// <summary>
    /// Completes a payment after the external provider confirms the charge.
    /// </summary>
    [HttpPost("{id}/complete")]
    [ProducesResponseType(typeof(PaymentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CompletePayment(Guid id, [FromBody] CompletePaymentRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.ExternalTransactionId))
            return BadRequest("ExternalTransactionId is required.");

        var payment = await _paymentService.CompletePaymentAsync(id, request.ExternalTransactionId);
        _logger.LogInformation("Payment {PaymentId} completed with transaction {TxId}", id, request.ExternalTransactionId);
        return Ok(new PaymentDto(payment));
    }

    /// <summary>
    /// Moves a processing payment into escrow pending delivery confirmation.
    /// </summary>
    [HttpPost("{id}/escrow")]
    [ProducesResponseType(typeof(PaymentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MoveToEscrow(Guid id)
    {
        var payment = await _paymentService.MoveToEscrowAsync(id);
        return Ok(new PaymentDto(payment));
    }

    /// <summary>
    /// Releases escrowed funds to the seller after buyer confirms delivery.
    /// </summary>
    [HttpPost("{id}/escrow/release")]
    [ProducesResponseType(typeof(PaymentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ReleaseEscrow(Guid id, [FromBody] CompletePaymentRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.ExternalTransactionId))
            return BadRequest("ExternalTransactionId is required.");

        var payment = await _paymentService.ReleaseEscrowAsync(id, request.ExternalTransactionId);
        return Ok(new PaymentDto(payment));
    }

    /// <summary>
    /// Cancels a pending or processing payment.
    /// </summary>
    [HttpPost("{id}/cancel")]
    [ProducesResponseType(typeof(PaymentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CancelPayment(Guid id, [FromQuery] Guid requesterId)
    {
        var payment = await _paymentService.CancelPaymentAsync(id, requesterId);
        return Ok(new PaymentDto(payment));
    }

    /// <summary>
    /// Refunds a completed payment to the buyer.
    /// </summary>
    [HttpPost("{id}/refund")]
    [ProducesResponseType(typeof(PaymentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RefundPayment(Guid id, [FromBody] RefundPaymentRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Reason))
            return BadRequest("Refund reason is required.");

        var payment = await _paymentService.RefundPaymentAsync(id, request.Reason);
        _logger.LogInformation("Payment {PaymentId} refunded", id);
        return Ok(new PaymentDto(payment));
    }

    /// <summary>
    /// Retrieves all payments made by a specific buyer.
    /// </summary>
    [HttpGet("buyer/{buyerId}")]
    [ProducesResponseType(typeof(List<PaymentDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetBuyerPayments(Guid buyerId)
    {
        var payments = await _paymentService.GetBuyerPaymentsAsync(buyerId);
        return Ok(payments.Select(p => new PaymentDto(p)).ToList());
    }

    /// <summary>
    /// Retrieves all payments received by a specific seller.
    /// </summary>
    [HttpGet("seller/{sellerId}")]
    [ProducesResponseType(typeof(List<PaymentDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSellerPayments(Guid sellerId)
    {
        var payments = await _paymentService.GetSellerPaymentsAsync(sellerId);
        return Ok(payments.Select(p => new PaymentDto(p)).ToList());
    }
}
