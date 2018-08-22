using System.Collections.Generic;

namespace CrunchyIndex.Models
{
    public class Rating
    {
        public decimal AverageStars { get; set; }
        public int FiveStarReviewCount { get; set; }
        public int FourStarReviewCount { get; set; }
        public int ThreeStarReviewCount { get; set; }
        public int TwoStarReviewCount { get; set; }
        public int OneStarReviewCount { get; set; }
        public int RatingCount { get; set; }
        public decimal AverageRatingCalculated { get; set; }
    }
}
