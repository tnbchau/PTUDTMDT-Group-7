using System.ComponentModel.DataAnnotations;
using YenMay_web.Enums;

namespace YenMay_web.Models.ViewModels.Checkout
{
    public class CheckoutRequestViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập họ tên người nhận")]
        [Display(Name = "Họ và tên")]
        [StringLength(100)]
        public string CustomerName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        [Display(Name = "Số điện thoại")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        public string CustomerPhone { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập địa chỉ email")]
        [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
        public string CustomerEmail { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập địa chỉ nhận hàng")]
        [Display(Name = "Địa chỉ giao hàng")]
        [StringLength(255)]
        public string ShippingAddress { get; set; } = string.Empty;

        [Display(Name = "Ghi chú đơn hàng")]
        public string? Notes { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn phương thức thanh toán")]
        public PaymentMethod SelectedPaymentMethod { get; set; } = PaymentMethod.COD;
    }
}