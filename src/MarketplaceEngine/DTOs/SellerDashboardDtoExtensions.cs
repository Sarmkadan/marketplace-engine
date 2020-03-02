namespace MarketplaceEngine.DTOs
{
    public static class SellerDashboardDtoExtensions
    {
        /// <summary>
        /// Calculates the conversion rate as a percentage of total sales to total listings.
        /// </summary>
        /// <param name="dashboard">The seller dashboard data to calculate conversion rate for.</param>
        /// <returns>The conversion rate percentage (0-100).</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="dashboard"/> is null.</exception>
        public static double GetConversionRate(this SellerDashboardDto dashboard)
        {
            ArgumentNullException.ThrowIfNull(dashboard);

            if (dashboard.TotalListings == 0)
            {
                return 0.0;
            }

            return (double)dashboard.TotalSales / dashboard.TotalListings * 100.0;
        }

        /// <summary>
        /// Determines if the seller has been active within the last 30 days.
        /// </summary>
        /// <param name="dashboard">The seller dashboard data to check activity for.</param>
        /// <returns>True if the seller has been active within the last 30 days; otherwise, false.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="dashboard"/> is null.</exception>
        public static bool IsSellerActive(this SellerDashboardDto dashboard)
        {
            ArgumentNullException.ThrowIfNull(dashboard);

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
        /// <param name="dashboard">The seller dashboard data to format financial summary for.</param>
        /// <returns>A formatted string representing the seller's financial status.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="dashboard"/> is null.</exception>
        public static string GetFinancialSummary(this SellerDashboardDto dashboard)
        {
            ArgumentNullException.ThrowIfNull(dashboard);

            return $"Total Revenue: {dashboard.TotalRevenue:C}, Pending Payout: {dashboard.PendingPayout:C}";
        }

        /// <summary>
        /// Returns a user-friendly string representation of the seller's rating and review count.
        /// </summary>
        /// <param name="dashboard">The seller dashboard data to format rating display for.</param>
        /// <returns>A formatted string representing the seller's rating and review count.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="dashboard"/> is null.</exception>
        public static string GetRatingDisplayText(this SellerDashboardDto dashboard)
        {
            ArgumentNullException.ThrowIfNull(dashboard);

            if (dashboard.TotalReviews == 0)
            {
                return "No reviews yet";
            }

            return $"{dashboard.AverageRating:F1} stars ({dashboard.TotalReviews} reviews)";
        }
    }
}
