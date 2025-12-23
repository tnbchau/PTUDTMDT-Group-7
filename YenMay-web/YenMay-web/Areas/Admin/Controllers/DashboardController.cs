using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using YenMay_web.Areas.Admin.ViewModels.Dashboard;
using YenMay_web.Data;
using YenMay_web.Enums;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace YenMay_web.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class DashboardController : Controller
    {
        private readonly YMDbContext _db;

        public DashboardController(YMDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> Index()
        {
            var today = DateTime.UtcNow.AddHours(7).Date; // Giả định múi giờ VN
            var sevenDaysAgo = today.AddDays(-6);

            // Lấy toàn bộ đơn hàng cần thiết để tính toán (tránh query nhiều lần)
            var orders = await _db.Orders
                .Where(o => o.CreatedAt >= sevenDaysAgo.AddMonths(-1)) // Lấy dư ra để tính tháng này
                .ToListAsync();

            var model = new AdminDashboardViewModel();

            /* =========================
             * 1. GENERAL STATS
             * ========================= */
            model.General.TotalOrders = await _db.Orders.CountAsync();
            model.General.TotalRevenue = await _db.Orders.SumAsync(o => o.TotalAmount);
            model.General.TotalCustomers = await _db.Users.CountAsync();
            model.General.TotalProducts = await _db.Products.CountAsync();

            /* =========================
             * 2. REVENUE STATS
             * ========================= */
            model.Revenue.TodayRevenue = orders
                .Where(o => o.CreatedAt.Date == today)
                .Sum(o => o.TotalAmount);

            model.Revenue.ThisMonthRevenue = orders
                .Where(o => o.CreatedAt.Month == today.Month && o.CreatedAt.Year == today.Year)
                .Sum(o => o.TotalAmount);

            model.Revenue.AvgOrderValue = orders.Any() ? orders.Average(o => o.TotalAmount) : 0;

            /* =========================
             * 3. ORDER STATS
             * ========================= */
            model.Orders.PendingOrders = orders.Count(o => o.Status == OrderStatus.Pending);
            model.Orders.ProcessingOrders = orders.Count(o => o.Status == OrderStatus.Confirmed); // Confirmed tương đương Processing
            model.Orders.ShippedOrders = orders.Count(o => o.Status == OrderStatus.Shipping);
            model.Orders.DeliveredOrders = orders.Count(o => o.Status == OrderStatus.Completed);
            model.Orders.CancelledOrders = orders.Count(o => o.Status == OrderStatus.Cancelled);
            model.Orders.TodayOrders = orders.Count(o => o.CreatedAt.Date == today);

            /* =========================
             * 4. SALES CHART DATA (7 ngày gần nhất)
             * ========================= */
            for (int i = 0; i < 7; i++)
            {
                var date = sevenDaysAgo.AddDays(i);
                var dayOrders = orders.Where(o => o.CreatedAt.Date == date).ToList();

                model.ChartData.Labels.Add(date.ToString("dd/MM"));
                model.ChartData.RevenueData.Add(dayOrders.Sum(o => o.TotalAmount));
                model.ChartData.OrderData.Add(dayOrders.Count);

                // Lưu vào Dictionary để phục vụ logic khác nếu cần
                model.OrdersByDate[date.ToString("dd/MM/yyyy")] = dayOrders.Count;
            }

            return View(model);
        }
    }
}