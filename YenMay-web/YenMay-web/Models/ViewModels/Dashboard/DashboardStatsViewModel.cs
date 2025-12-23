namespace YenMay_web.Models.ViewModels.Dashboard
{
    // ViewModel chính chứa toàn bộ dữ liệu thống kê
    public class DashboardStatsViewModel
    {
        public GeneralStats General { get; set; } = new();
        public RevenueStats Revenue { get; set; } = new();
        public OrderStats Orders { get; set; } = new();
        public ProductStats Products { get; set; } = new();
        public List<RecentActivity> RecentActivities { get; set; } = new();
        public SalesChartData ChartData { get; set; } = new();
    }

    // 1. Thống kê tổng quan (4 ô trên cùng)
    public class GeneralStats
    {
        public int TotalOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public int TotalCustomers { get; set; }
        public int TotalProducts { get; set; }
    }

    // 2. Thống kê doanh thu chi tiết
    public class RevenueStats
    {
        public decimal TodayRevenue { get; set; }
        public decimal ThisWeekRevenue { get; set; }
        public decimal ThisMonthRevenue { get; set; }
        public decimal ThisYearRevenue { get; set; }
        public decimal AvgOrderValue { get; set; } // Giá trị đơn trung bình
    }

    // 3. Thống kê tình trạng đơn hàng
    public class OrderStats
    {
        public int PendingOrders { get; set; }    // Chờ xử lý
        public int ProcessingOrders { get; set; } // Đang xử lý/Đã xác nhận
        public int ShippedOrders { get; set; }    // Đang giao
        public int DeliveredOrders { get; set; }  // Hoàn thành
        public int CancelledOrders { get; set; }  // Đã hủy
        public int TodayOrders { get; set; }      // Đơn hôm nay
        public decimal ConversionRate { get; set; } // Tỷ lệ chuyển đổi (nếu cần)
    }

    // 4. Thống kê sản phẩm
    public class ProductStats
    {
        public int LowStockProducts { get; set; }   // Sắp hết hàng
        public int OutOfStockProducts { get; set; } // Đã hết hàng
        public List<TopProduct> TopSellingProducts { get; set; } = new();
    }

    // Class con cho Top sản phẩm bán chạy
    public class TopProduct
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public int SoldQuantity { get; set; }
        public decimal Revenue { get; set; }
    }

    // 5. Hoạt động gần đây (Recent Activities)
    public class RecentActivity
    {
        public string Type { get; set; } = string.Empty; // Loại: "New Order", "New User"...
        public string Description { get; set; } = string.Empty;
        public DateTime Time { get; set; }
        public string Icon { get; set; } = "fas fa-circle"; // Icon fontawesome
        public string Color { get; set; } = "primary"; // Màu bootstrap: primary, success...
    }

    // 6. Dữ liệu biểu đồ doanh số (Chart.js)
    public class SalesChartData
    {
        public string Period { get; set; } = "7d";
        public List<string> Labels { get; set; } = new(); // Nhãn ngày: "01/01", "02/01"...
        public List<decimal> RevenueData { get; set; } = new(); // Cột Doanh thu
        public List<int> OrderData { get; set; } = new(); // Cột Số đơn hàng
    }
}