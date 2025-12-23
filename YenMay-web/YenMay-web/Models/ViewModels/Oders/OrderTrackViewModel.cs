using System.ComponentModel.DataAnnotations;

namespace YenMay_web.Models.ViewModels.Orders
{
    public class OrderTrackViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập mã đơn hàng")]
        public string OrderCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        public string Phone { get; set; } = string.Empty;

        // Kết quả (nullable)
        public OrderDetailViewModel? OrderDetail { get; set; }
    }
}
