namespace YenMay_web.Models.ViewModels.Products
{
    public class ProductCardViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;

        public decimal Price { get; set; }
        public string PriceFormatted => Price.ToString("N0") + "₫";

        public string ThumbnailUrl { get; set; } = "/images/no-image.png";

        public string CategoryName { get; set; } = string.Empty;
        public string CategorySlug { get; set; } = string.Empty;
        public string ShortDescription { get; set; }
        public int StockQuantity { get; set; }
        public bool InStock => StockQuantity > 0;
        public string StockStatus => InStock ? "Còn hàng" : "Hết hàng";

        public double AverageRating { get; set; }
        public int ReviewCount { get; set; }
        public int SoldCount { get; set; }

        public bool IsNew { get; set; }
        public bool IsBestSeller => SoldCount > 50;
        public string RatingDisplay => AverageRating > 0
            ? AverageRating.ToString("0.0")
            : "Chưa có đánh giá";

        public string StarRatingClass
        {
            get
            {
                if (AverageRating == 0) return "rating-0";
                var rating = Math.Round(AverageRating * 2) / 2; // Làm tròn đến 0.5
                return $"rating-{rating.ToString("0-0", System.Globalization.CultureInfo.InvariantCulture)}";
            }
        }
        public List<string> Badges
        {
            get
            {
                var badges = new List<string>();
                if (IsNew) badges.Add("Mới");
                if (IsBestSeller) badges.Add("Bán chạy");
                if (!InStock) badges.Add("Hết hàng");
                return badges;
            }
        }

    }
}