#nullable enable

using Microsoft.AspNetCore.Mvc;
using MarketplaceEngine.DTOs;
using System.Security.Claims;

namespace MarketplaceEngine.Controllers;

/// <summary>
/// Extension methods for <see cref="PaymentsController"/> that provide additional convenience methods
/// for common payment operations including batch processing, status queries, and bulk operations.
/// </summary>
public static class PaymentsControllerExtensions
{
    private sealed class BatchOperationResult
    {
        public int SuccessCount { get; init; }
        public int ErrorCount { get; init; }
        public List<string> Errors { get; init; } = new();
        public List<Guid>? PaymentIds { get; init; }

        public BatchOperationResult(int successCount, int errorCount, List<string> errors, List<Guid>? paymentIds = null)
        {
            SuccessCount = successCount;
            ErrorCount = errorCount;
            Errors = errors;
            PaymentIds = paymentIds;
        }
    }

    /// <summary>
    /// Initiates multiple payments in a single request for batch purchases.
    /// </summary>
    /// <param name="controller">The payments controller instance.</param>
    /// <param name="requests">Collection of payment initiation requests.</param>
    /// <returns>Collection of created payment results with status codes.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="requests"/> is <see langword="null"/></exception>
    public static async Task<IActionResult> InitiateBatchPayments(
        this PaymentsController controller,
        [FromBody] List<InitiatePaymentRequest> requests)
    {
        ArgumentNullException.ThrowIfNull(requests);

        if (requests.Count == 0)
        {
            return new BadRequestObjectResult("At least one payment request is required.");
        }

        var paymentDtos = new List<PaymentDto>();
        var errors = new List<string>();

        foreach (var request in requests)
        {
            ArgumentNullException.ThrowIfNull(request);

            try
            {
                var result = await controller.InitiatePayment(request);
                if (result is ObjectResult objectResult && objectResult.Value is PaymentDto paymentDto)
                {
                    paymentDtos.Add(paymentDto);
                }
            }
            catch (Exception ex)
            {
                errors.Add($"Failed to initiate payment for listing {request.ListingId}: {ex.Message}");
            }
        }

        if (errors.Count > 0)
        {
            return new BadRequestObjectResult(new BatchOperationResult(
                successCount: paymentDtos.Count,
                errorCount: errors.Count,
                errors: errors,
                paymentIds: paymentDtos.Select(p => p.Id).ToList()));
        }

        return new ObjectResult(paymentDtos.Select(p => p.Id).ToList())
        {
            StatusCode = 201
        };
    }

    /// <summary>
    /// Retrieves payment status information including current state and timestamps.
    /// </summary>
    /// <param name="controller">The payments controller instance.</param>
    /// <param name="id">Payment ID to check status for.</param>
    /// <returns>Payment status information.</returns>
    public static async Task<IActionResult> GetPaymentStatus(
        this PaymentsController controller,
        Guid id)
    {
        ArgumentNullException.ThrowIfNull(controller);

        var paymentResult = await controller.GetPayment(id);

        return paymentResult switch
        {
            NotFoundResult => new NotFoundResult(),
            ObjectResult { Value: PaymentDto paymentDto } => new OkObjectResult(new
            {
                PaymentId = paymentDto.Id,
                Status = paymentDto.Status,
                CreatedAt = paymentDto.CreatedAt,
                CompletedAt = paymentDto.CompletedAt,
                Amount = paymentDto.Amount,
                Currency = paymentDto.Currency,
                BuyerId = paymentDto.BuyerId,
                SellerId = paymentDto.SellerId
            }),
            _ => new NotFoundResult()
        };
    }

    /// <summary>
    /// Cancels multiple payments in a single request.
    /// </summary>
    /// <param name="controller">The payments controller instance.</param>
    /// <param name="paymentIds">Collection of payment IDs to cancel.</param>
    /// <param name="requesterId">ID of the user requesting cancellation.</param>
    /// <returns>Summary of cancellation results.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="paymentIds"/> is <see langword="null"/></exception>
    public static async Task<IActionResult> CancelBatchPayments(
        this PaymentsController controller,
        [FromBody] List<Guid> paymentIds,
        [FromQuery] Guid requesterId)
    {
        ArgumentNullException.ThrowIfNull(paymentIds);

        if (paymentIds.Count == 0)
        {
            return new BadRequestObjectResult("At least one payment ID is required.");
        }

        var successCount = 0;
        var errors = new List<string>();

        foreach (var paymentId in paymentIds)
        {
            try
            {
                await controller.CancelPayment(paymentId, requesterId);
                successCount++;
            }
            catch (Exception ex)
            {
                errors.Add($"Failed to cancel payment {paymentId}: {ex.Message}");
            }
        }

        if (errors.Count > 0)
        {
            return new BadRequestObjectResult(new BatchOperationResult(
                successCount: successCount,
                errorCount: errors.Count,
                errors: errors));
        }

        return new OkObjectResult(new { SuccessCount = successCount });
    }

    /// <summary>
    /// Retrieves payments filtered by status and date range.
    /// </summary>
    /// <param name="controller">The payments controller instance.</param>
    /// <param name="status">Optional payment status filter.</param>
    /// <param name="startDate">Optional start date filter.</param>
    /// <param name="endDate">Optional end date filter.</param>
    /// <returns>Filtered list of payments.</returns>
    public static async Task<IActionResult> GetPaymentsByStatusAndDate(
        this PaymentsController controller,
        [FromQuery] string? status,
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate)
    {
        ArgumentNullException.ThrowIfNull(controller);

        // This would ideally use a service method, but since we're extending the controller
        // we'll implement a basic version that queries all payments and filters in-memory
        // In a real implementation, this would call a service method

        var allPaymentsTask = controller.GetBuyerPayments(Guid.Empty); // Empty buyer gets all
        var allSellerPaymentsTask = controller.GetSellerPayments(Guid.Empty); // Empty seller gets all

        await Task.WhenAll(allPaymentsTask, allSellerPaymentsTask);

        if (allPaymentsTask.Result is ObjectResult buyerResult &&
            allSellerPaymentsTask.Result is ObjectResult sellerResult &&
            buyerResult.Value is List<PaymentDto> buyerPayments &&
            sellerResult.Value is List<PaymentDto> sellerPayments)
        {
            var allPayments = buyerPayments.Concat(sellerPayments).ToList();

            var filteredPayments = allPayments.AsEnumerable();

            if (!string.IsNullOrEmpty(status))
            {
                filteredPayments = filteredPayments.Where(p => p.Status.Equals(status, StringComparison.OrdinalIgnoreCase));
            }

            if (startDate.HasValue)
            {
                filteredPayments = filteredPayments.Where(p => p.CreatedAt >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                filteredPayments = filteredPayments.Where(p => p.CreatedAt <= endDate.Value);
            }

            return new OkObjectResult(filteredPayments.ToList());
        }

        return new NotFoundResult();
    }
}
