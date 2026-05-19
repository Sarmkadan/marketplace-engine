#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using MarketplaceEngine.Domain.Enums;
using MarketplaceEngine.Domain.Models;

namespace MarketplaceEngine.Repositories;

/// <summary>
/// Repository interface for payment-specific operations.
/// </summary>
public interface IPaymentRepository : IRepository<Payment>
{
    // Retrieves payments for a specific buyer
    Task<List<Payment>> GetByBuyerIdAsync(Guid buyerId);

    // Retrieves payments for a specific seller
    Task<List<Payment>> GetBySellerIdAsync(Guid sellerId);

    // Retrieves payments for a specific listing
    Task<List<Payment>> GetByListingIdAsync(Guid listingId);

    // Retrieves payments filtered by status
    Task<List<Payment>> GetByStatusAsync(PaymentStatus status);

    // Retrieves paginated payments
    Task<(List<Payment> items, int total)> GetPagedAsync(int pageNumber, int pageSize);

    // Retrieves total revenue for a seller (sum of completed payments)
    Task<decimal> GetTotalRevenueAsync(Guid sellerId);
}
