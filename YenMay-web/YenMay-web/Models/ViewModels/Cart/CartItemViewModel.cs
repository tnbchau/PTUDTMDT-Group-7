namespace YenMay_web.Models.ViewModels.Cart
{
    public class CartItemViewModel
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string? ProductImage { get; set; }
        public string CategorySlug { get; set; }
        public string Slug { get; set; } = string.Empty; 
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public decimal Total => Price * Quantity;

        public int Stock { get; set; } // Giúp kiểm soát logic tăng/giảm số lượng

        public bool IsAvailable => Stock >= Quantity;
        public string StockStatus => Stock >= Quantity ? "Còn hàng" : $"Chỉ còn {Stock} sản phẩm";
    }
}