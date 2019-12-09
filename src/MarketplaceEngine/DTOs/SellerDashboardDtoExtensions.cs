namespace MarketplaceEngine.DTOs
{
    public static class SellerDashboardDtoExtensions
    {
        /// <summary>
        /// Calculates the conversion rate as a percentage of total sales to total listings.
        /// </summary>
        public static double GetConversionRate(this SellerDashboardDto dashboard)
        {
            if (dashboard.TotalListings == 0)
            {
                return 0.0;
            }

            return (double)dashboard.TotalSales / dashboard.TotalListings * 100.0;
        }

        /// <summary>
        /// Determines if the seller has been active within the last 30 days.
        /// </summary>
        public static bool IsSellerActive(this SellerDashboardDto dashboard)
        {
            if (!dashboard.LastActivityAt.HasValue)
            {
                return false;
            }

            var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);
            return dashboard.LastActivityAt.Value > thirtyDaysAgo;
        }

        /// <summary>
        /// Returns a formatted summary string of the seller's financial status.
        /// </summary>
        public static string GetFinancialSummary(this SellerDashboardDto dashboard)
        {
            return $"Total Revenue: {dashboard.TotalRevenue:C}, Pending Payout: {dashboard.PendingPayout:C}";
        }

        /// <summary>
        /// Returns a user-friendly string representation of the seller's rating and review count.
        /// </summary>
        public static string GetRatingDisplayText(this SellerDashboardDto dashboard)
        {
            if (dashboard.TotalReviews == 0)
            {
                return "No reviews yet";
            }

            return $"{dashboard.AverageRating:F1} stars ({dashboard.TotalReviews} reviews)";
        }
    }
}
