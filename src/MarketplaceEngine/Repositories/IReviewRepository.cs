#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using MarketplaceEngine.Domain.Enums;
using MarketplaceEngine.Domain.Models;

namespace MarketplaceEngine.Repositories;

/// <summary>
/// Repository interface for review-specific operations.
/// </summary>
public interface IReviewRepository : IRepository<Review>
{
    // Retrieves all reviews written by a specific user
    Task<List<Review>> GetByReviewerIdAsync(Guid reviewerId);

    // Retrieves all reviews for a specific seller
    Task<List<Review>> GetBySellerIdAsync(Guid sellerId);

    // Retrieves all reviews for a specific listing
    Task<List<Review>> GetByListingIdAsync(Guid listingId);

    // Checks whether a reviewer has already reviewed a seller for a given listing
    Task<bool> ExistsForTransactionAsync(Guid reviewerId, Guid sellerId, Guid? listingId);

    // Retrieves the average score for a seller
    Task<double> GetAverageScoreAsync(Guid sellerId);

    // Retrieves active reviews for a seller with optional pagination
    Task<(List<Review> items, int total)> GetPagedBySellerAsync(Guid sellerId, int pageNumber, int pageSize);
}
