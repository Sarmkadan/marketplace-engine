using System;

namespace MarketplaceEngine.DTOs
{
    public static class ReviewDtoExtensions
    {
        /// <summary>
        /// Determines whether the review was created within the specified number of days.
        /// </summary>
        /// <param name="dto">The review DTO.</param>
        /// <param name="days">The number of days to look back.</param>
        /// <returns>True if the review was created within the specified days; otherwise, false.</returns>
        public static bool IsRecent(this ReviewDto dto, int days)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));
            if (days < 0) throw new ArgumentOutOfRangeException(nameof(days), "Days must be non-negative.");

            // Assume ReviewDto has a CreatedAt property of type DateTime
            return (DateTime.UtcNow - dto.CreatedAt).TotalDays <= days;
        }

        /// <summary>
        /// Returns a string representation of the review score using Unicode star characters.
        /// </summary>
        /// <param name="dto">The review DTO.</param>
        /// <returns>A string of stars corresponding to the score.</returns>
        public static string StarString(this ReviewDto dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            // Clamp score to 0-5 to avoid invalid star counts
            int score = Math.Max(0, Math.Min(5, dto.Score));
            return new string('★', score);
        }

        /// <summary>
        /// Checks whether the review score is within the valid rating bounds (1-5 inclusive).
        /// </summary>
        /// <param name="dto">The review DTO.</param>
        /// <returns>True if the score is between 1 and 5; otherwise, false.</returns>
        public static bool WithinRatingBounds(this ReviewDto dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            return dto.Score >= 1 && dto.Score <= 5;
        }
    }
}
