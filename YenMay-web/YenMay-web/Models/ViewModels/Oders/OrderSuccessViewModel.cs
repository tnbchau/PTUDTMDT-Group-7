using YenMay_web.Enums; 

namespace YenMay_web.Models.ViewModels.Orders
{
    public class OrderSuccessViewModel
    {
        public int OrderId { get; set; }
        public string OrderCode { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public string CustomerEmail { get; set; } = string.Empty;
        public string CustomerPhone { get; set; } = string.Empty;

        public PaymentMethod PaymentMethod { get; set; }

        public bool IsBankTransfer => PaymentMethod == PaymentMethod.BankTransfer;
    }
}