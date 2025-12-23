using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using YenMay_web.Enums; // Nhớ using namespace này

namespace YenMay_web.Utilities
{
    public static class OrderHelper
    {
        // 1. Sinh mã đơn: YM-20231225-AKJ72H
        public static string GenerateOrderCode(string prefix = "YM")
        {
            var date = DateTime.Now.ToString("yyyyMMdd");
            var random = GenerateRandomString(6);
            return $"{prefix}-{date}-{random}";
        }

        // Dùng Random.Shared (Hiệu năng tốt hơn, tránh trùng lặp khi gọi liên tục)
        private static string GenerateRandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[Random.Shared.Next(s.Length)]).ToArray());
        }

        // 2. Format tiền tệ chuẩn Việt Nam (tự động thêm dấu chấm phân cách ngàn)
        public static string FormatCurrency(decimal amount)
        {
            var cultureInfo = new CultureInfo("vi-VN");
            return amount.ToString("C0", cultureInfo); // C0: Currency không có số thập phân
            // Output: 1.250.000 ₫ (Ký hiệu đ tự động)
        }

        // 3. Dùng Enum để lấy Text
        public static string GetOrderStatusText(OrderStatus status)
        {
            return status switch
            {
                OrderStatus.Pending => "Chờ xác nhận",
                OrderStatus.Confirmed => "Đã xác nhận",
                OrderStatus.Shipping => "Đang giao hàng",
                OrderStatus.Completed => "Giao hàng thành công",
                OrderStatus.Cancelled => "Đã hủy",
                _ => "Không xác định"
            };
        }

        // 4. Dùng Enum để lấy màu Badge (Bootstrap class)
        public static string GetOrderStatusColor(OrderStatus status)
        {
            return status switch
            {
                OrderStatus.Pending => "warning",     // Vàng
                OrderStatus.Confirmed => "info",      // Xanh dương nhạt
                OrderStatus.Shipping => "primary",    // Xanh dương đậm
                OrderStatus.Completed => "success",   // Xanh lá
                OrderStatus.Cancelled => "danger",    // Đỏ
                _ => "secondary"
            };
        }
        public static string GetStatusBadgeClass(OrderStatus status)
        {
            var color = GetOrderStatusColor(status);
            // Trả về full class Bootstrap: vd "bg-warning text-dark"
            if (status == OrderStatus.Pending)
            {
                return $"bg-{color} text-dark";
            }
            return $"bg-{color} text-white";
        }

        public static string GetPaymentMethodText(PaymentMethod method)
        {
            return method switch
            {
                PaymentMethod.COD => "Thanh toán khi nhận hàng (COD)",
                PaymentMethod.BankTransfer => "Chuyển khoản ngân hàng",
                PaymentMethod.VNPAY => "Thanh toán qua VNPAY",
                _ => "Khác"
            };
        }

        public static string CalculateEstimatedDelivery(DateTime orderDate)
        {
            // Cộng 3 ngày (Logic đơn giản)
            return orderDate.AddDays(3).ToString("dd/MM/yyyy");
        }

        // Tạo Tracking Number giả lập
        public static string GenerateTrackingNumber()
        {
            var timestamp = DateTime.Now.Ticks.ToString("x").ToUpper(); // Hex string
            var random = Random.Shared.Next(1000, 9999);
            return $"YMTN{timestamp}{random}";
        }

        // Hash dữ liệu (Ví dụ dùng để verify tính toàn vẹn)
        public static string HashOrderData(string orderCode, decimal amount, DateTime date)
        {
            var rawData = $"{orderCode}|{amount}|{date:yyyyMMddHHmmss}";
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(rawData));

            // Chuyển sang Hex String thay vì Base64 để an toàn trên URL
            return Convert.ToHexString(bytes).ToLower();
        }
    }
}