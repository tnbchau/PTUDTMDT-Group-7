using YenMay_web.Models.ViewModels.Common; 
namespace YenMay_web.Models.ViewModels.Orders
{
    public class OrderHistoryViewModel
    {
        public List<OrderSummaryViewModel> Orders { get; set; } = new();
        public PaginationViewModel Pagination { get; set; } = new();
    }

    // Class con: Chỉ chứa thông tin tóm tắt để hiện dạng bảng danh sách
    public class OrderSummaryViewModel
    {
        public int Id { get; set; }
        public string OrderCode { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }

        public string OrderStatus { get; set; } = string.Empty;
        public int OrderStatusId { get; set; } // Dùng để đổi màu badge (Xanh/Đỏ/Vàng)

        public int TotalItems { get; set; }    // Số lượng món hàng
        public decimal TotalAmount { get; set; }
    }
}