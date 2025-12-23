using YenMay_web.Models.ViewModels.Shipping;

namespace YenMay_web.Models.ViewModels.Cart
{
    public class CartIndexViewModel
    {
        public List<CartItemViewModel> Items { get; set; } = new();
        public decimal SubTotal { get; set; }
        public decimal ShippingFee { get; set; }
        public decimal TotalAmount => SubTotal + ShippingFee;
        public int TotalItems => Items.Sum(i => i.Quantity);

        // Thông tin shipping
        public ShippingRuleViewModel? AppliedShippingRule { get; set; }

        // Helper properties
        public bool IsCartEmpty => !Items.Any();
        public bool HasShippingRule => AppliedShippingRule != null;
        public bool IsFreeShipping => ShippingFee == 0;

        // Thông báo
        public string? Message { get; set; }
        public bool HasMessage => !string.IsNullOrEmpty(Message);
    }
}