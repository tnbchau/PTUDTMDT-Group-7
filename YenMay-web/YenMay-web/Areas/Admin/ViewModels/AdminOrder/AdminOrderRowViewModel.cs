using System;
using YenMay_web.Enums;
using YenMay_web.Utilities;

namespace YenMay_web.Areas.Admin.ViewModels.AdminOrder
{
    public class AdminOrderRowViewModel
    {
        public int Id { get; set; }

        // --- 1. Mã đơn hàng ---
        public string OrderCode { get; set; } = string.Empty;

        // --- 2. Trạng thái đơn ---
        public OrderStatus Status { get; set; }
        public string StatusText => OrderHelper.GetOrderStatusText(Status);
        public string StatusColor => OrderHelper.GetOrderStatusColor(Status); // Badge class

        // --- 3. Số lượng sản phẩm ---
        public int TotalItems { get; set; }
        public string TotalItemsText => TotalItems > 0 ? $"{TotalItems} sản phẩm" : "Đang cập nhật...";

        // --- 4. Tổng tiền ---
        public decimal TotalAmount { get; set; }
        public string TotalAmountText => OrderHelper.FormatCurrency(TotalAmount);

        // --- 5. Phương thức thanh toán ---
        public PaymentMethod PaymentMethod { get; set; }
        public string PaymentMethodText => OrderHelper.GetPaymentMethodText(PaymentMethod);

        // --- 6. Ngày tạo đơn ---
        public DateTime CreatedAt { get; set; }
        public string CreatedAtText => CreatedAt.ToString("dd/MM/yyyy HH:mm"); // Format đẹp
    }
}
