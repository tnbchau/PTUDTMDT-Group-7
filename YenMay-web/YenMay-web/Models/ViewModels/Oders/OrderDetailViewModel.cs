using YenMay_web.Enums;

namespace YenMay_web.Models.ViewModels.Orders
{
    public class OrderDetailViewModel
    {
        // --- 1. Thông tin chung ---
        public int Id { get; set; }
        public string OrderCode { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }

        // Trạng thái đơn hàng
        public int OrderStatusId { get; set; }
        public string OrderStatus { get; set; } = string.Empty;

        // --- 2. Thông tin người nhận (Map từ CustomerName, CustomerPhone...) ---
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerPhone { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public string ShippingAddress { get; set; } = string.Empty;
        public string? Notes { get; set; }

        // --- 3. Tài chính & Vận chuyển ---
        public decimal SubTotal { get; set; }      // Tiền hàng
        public decimal ShippingFee { get; set; }   // Phí ship
        public string? ShippingRuleName { get; set; } // Tên gói ship (VD: Nhanh/Tiêu chuẩn)
        public decimal TotalAmount { get; set; }   // Tổng cộng

        public PaymentMethod PaymentMethod { get; set; }

        // --- 4. Danh sách sản phẩm ---
        public List<OrderItemViewModel> Items { get; set; } = new List<OrderItemViewModel>();

        // --- 5. Logic hiển thị nút bấm ---
        // Chỉ cho phép hủy khi đơn mới tạo (Chờ xác nhận - Id=1) hoặc Đã xác nhận (Id=2)
        public bool CanCancel => OrderStatusId == 1 || OrderStatusId == 2;

        // Kiểm tra xem đơn đã giao thành công chưa (Id=4)
        public bool IsCompleted => OrderStatusId == 4;
    }

    public class OrderItemViewModel
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string? ProductImage { get; set; } // Ảnh đại diện sản phẩm

        public decimal Price { get; set; }
        public int Quantity { get; set; }

        // Thành tiền của dòng này
        public decimal TotalPrice => Price * Quantity;
    }
}