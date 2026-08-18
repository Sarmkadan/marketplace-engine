using System;

namespace MarketplaceEngine.DTOs
{
    public static class ReviewDtoExtensions
    {
        public static bool IsRecent(this ReviewDto dto, int days)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));
            return dto.CreatedAt >= DateTime.UtcNow.AddDays(-days);
        }

        public static string StarString(this ReviewDto dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));
            
            int score = dto.Score;
            if (score < 1) score = 1;
            if (score > 5) score = 5;
            
            return new string('★', score) + new string('☆', 5 - score);
        }

        public static bool WithinRatingBounds(this ReviewDto dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));
            return dto.Score >= 1 && dto.Score <= 5;
        }
    }
}
