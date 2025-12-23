using YenMay_web.Models.ViewModels.Cart;    

namespace YenMay_web.Models.ViewModels.Checkout
{
    public class CheckoutViewModel
    {
        // 1. Dữ liệu để hứng Input (Form)
        public CheckoutRequestViewModel OrderInfo { get; set; } = new();

        // 2. Dữ liệu để hiển thị (Read-only)
        public CartIndexViewModel? CartSummary { get; set; }

        public decimal SubTotal { get; set; }
        public decimal ShippingFee { get; set; }
        public decimal TotalAmount => SubTotal + ShippingFee;
    }
}