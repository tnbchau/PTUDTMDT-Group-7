using System.ComponentModel.DataAnnotations;

namespace YenMay_web.Enums
{
    public enum PaymentMethod
    {
        [Display(Name = "Thanh toán khi nhận hàng (COD)")]
        COD = 0,
        [Display(Name = "Chuyển khoản ngân hàng")]
        BankTransfer = 1,
        [Display(Name = "Thanh toán qua ví điện tử VNPay")]
        VNPAY = 2
    }
}