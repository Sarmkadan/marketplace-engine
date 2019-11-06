#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using MarketplaceEngine.Domain.Enums;
using MarketplaceEngine.DTOs;
using MarketplaceEngine.Exceptions;
using MarketplaceEngine.Repositories;

namespace MarketplaceEngine.Services;

/// <summary>
/// Provides aggregated dashboard metrics for seller accounts including
/// revenue breakdowns, listing performance, and review summaries.
/// </summary>
public class SellerDashboardService
{
    private readonly IUserRepository _userRepository;
    private readonly IListingRepository _listingRepository;
    private readonly IPaymentRepository _paymentRepository;
    private readonly IReviewRepository _reviewRepository;
    private readonly IMessageRepository _messageRepository;

    public SellerDashboardService(
        IUserRepository userRepository,
        IListingRepository listingRepository,
        IPaymentRepository paymentRepository,
        IReviewRepository reviewRepository,
        IMessageRepository messageRepository)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _listingRepository = listingRepository ?? throw new ArgumentNullException(nameof(listingRepository));
        _paymentRepository = paymentRepository ?? throw new ArgumentNullException(nameof(paymentRepository));
        _reviewRepository = reviewRepository ?? throw new ArgumentNullException(nameof(reviewRepository));
        _messageRepository = messageRepository ?? throw new ArgumentNullException(nameof(messageRepository));
    }

    /// <summary>
    /// Returns an overview dashboard for the given seller including listing counts,
    /// total revenue, pending payout, rating and unread message count.
    /// </summary>
    public async Task<SellerDashboardDto> GetDashboardAsync(Guid sellerId)
    {
        var seller = await _userRepository.GetByIdAsync(sellerId);
        if (seller is null)
            throw new ResourceNotFoundException("User", sellerId);

        var listings = await _listingRepository.GetBySellerIdAsync(sellerId);
        var payments = await _paymentRepository.GetBySellerIdAsync(sellerId);
        var reviews = await _reviewRepository.GetBySellerIdAsync(sellerId);
        var unreadMessages = await _messageRepository.GetUnreadMessagesAsync(sellerId);

        var activeListings = listings.Count(l => l.Status == ListingStatus.Active);
        var totalRevenue = payments
            .Where(p => p.Status == PaymentStatus.Completed)
            .Sum(p => p.SellerPayout?.Amount ?? 0);
        var pendingPayout = payments
            .Where(p => p.Status == PaymentStatus.InEscrow)
            .Sum(p => p.SellerPayout?.Amount ?? 0);

        return new SellerDashboardDto
        {
            SellerId = sellerId,
            SellerName = seller.FullName,
            ActiveListings = activeListings,
            TotalListings = listings.Count,
            TotalSales = seller.TotalSales,
            TotalRevenue = totalRevenue,
            PendingPayout = pendingPayout,
            AverageRating = seller.Rating?.AverageRating ?? 0,
            TotalReviews = reviews.Count,
            UnreadMessages = unreadMessages.Count,
            LastActivityAt = seller.LastActiveAt
        };
    }

    /// <summary>
    /// Returns detailed revenue breakdown with monthly buckets for charting.
    /// </summary>
    public async Task<SellerRevenueDto> GetRevenueAsync(Guid sellerId)
    {
        var seller = await _userRepository.GetByIdAsync(sellerId);
        if (seller is null)
            throw new ResourceNotFoundException("User", sellerId);

        var payments = await _paymentRepository.GetBySellerIdAsync(sellerId);
        var completed = payments.Where(p => p.Status == PaymentStatus.Completed).ToList();

        var grossRevenue = completed.Sum(p => p.Amount.Amount);
        var fees = completed.Sum(p => p.PlatformFee?.Amount ?? 0);
        var netRevenue = completed.Sum(p => p.SellerPayout?.Amount ?? 0);
        var pendingPayout = payments
            .Where(p => p.Status == PaymentStatus.InEscrow)
            .Sum(p => p.SellerPayout?.Amount ?? 0);

        var monthly = completed
            .GroupBy(p => new { p.CompletedAt!.Value.Year, p.CompletedAt.Value.Month })
            .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
            .Select(g => new MonthlyRevenueDto
            {
                Year = g.Key.Year,
                Month = g.Key.Month,
                GrossRevenue = g.Sum(p => p.Amount.Amount),
                NetRevenue = g.Sum(p => p.SellerPayout?.Amount ?? 0),
                SalesCount = g.Count()
            })
            .ToList();

        return new SellerRevenueDto
        {
            SellerId = sellerId,
            TotalGrossRevenue = grossRevenue,
            TotalPlatformFees = fees,
            TotalNetRevenue = netRevenue,
            PendingPayout = pendingPayout,
            MonthlyBreakdown = monthly
        };
    }

    /// <summary>
    /// Returns listing performance statistics including views, interest counts and a top-listings ranking.
    /// </summary>
    public async Task<SellerListingStatsDto> GetListingStatsAsync(Guid sellerId)
    {
        var seller = await _userRepository.GetByIdAsync(sellerId);
        if (seller is null)
            throw new ResourceNotFoundException("User", sellerId);

        var listings = await _listingRepository.GetBySellerIdAsync(sellerId);

        var topListings = listings
            .OrderByDescending(l => l.ViewCount)
            .Take(10)
            .Select(l => new TopListingDto
            {
                ListingId = l.Id,
                Title = l.Title,
                ViewCount = l.ViewCount,
                InterestCount = l.InterestCount,
                Price = l.Price?.Amount ?? 0,
                Status = l.Status.ToString()
            })
            .ToList();

        return new SellerListingStatsDto
        {
            SellerId = sellerId,
            ActiveListings = listings.Count(l => l.Status == ListingStatus.Active),
            InactiveListings = listings.Count(l => l.Status == ListingStatus.Inactive),
            FeaturedListings = listings.Count(l => l.IsFeatured),
            TotalViews = listings.Sum(l => (long)l.ViewCount),
            TotalInterestCount = listings.Sum(l => (long)l.InterestCount),
            TopListings = topListings
        };
    }
}
