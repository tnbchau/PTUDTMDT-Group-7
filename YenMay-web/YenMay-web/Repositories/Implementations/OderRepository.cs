using Microsoft.EntityFrameworkCore;
using YenMay_web.Data;
using YenMay_web.Enums;
using YenMay_web.Models.Domain;
using YenMay_web.Repositories.Interfaces;
using YenMay_web.Utilities;

namespace YenMay_web.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly YMDbContext _context;

        public OrderRepository(YMDbContext context)
        {
            _context = context;
        }

        // --- 1. CORE CRUD ---
        public async Task<Order> AddAsync(Order order)
        {
            await _context.Orders.AddAsync(order);
            await _context.SaveChangesAsync();
            return order;
        }

        public async Task<Order?> GetByIdAsync(int id)
        {
            return await _context.Orders
                .Include(o => o.Items).ThenInclude(i => i.Product).ThenInclude(p =>p.Images)
                .FirstOrDefaultAsync(o => o.Id == id);
        }

        public async Task<Order?> GetByCodeAsync(string orderCode)
        {
            return await _context.Orders
                .Include(o => o.Items).ThenInclude(i => i.Product).ThenInclude(p => p.Images)
                .FirstOrDefaultAsync(o => o.OrderCode == orderCode);
        }

        // --- 2. QUERY & FILTER ---

        public async Task<List<Order>> GetByUserIdAsync(int userId)
        {
            return await _context.Orders
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.CreatedAt)
                .Include(o => o.Items) // Include items nếu cần hiển thị sơ bộ
                .ToListAsync();
        }

        public async Task<(List<Order> Orders, int TotalCount)> GetByUserIdPagedAsync(int userId, int pageIndex, int pageSize)
        {
            var query = _context.Orders.Where(o => o.UserId == userId);

            int totalCount = await query.CountAsync();

            var orders = await query
                .OrderByDescending(o => o.CreatedAt)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .Include(o => o.Items)
                .ToListAsync();

            return (orders, totalCount);
        }

        public async Task<(List<Order> Orders, int TotalCount)> GetForAdminAsync(
            string? searchTerm, int? statusId, DateTime? fromDate, DateTime? toDate, int pageIndex, int pageSize)
        {
            var query = _context.Orders
                .Include(o => o.Items)
                .ThenInclude(i => i.Product)
                .ThenInclude(p => p.Images)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
                query = query.Where(o => o.OrderCode.Contains(searchTerm) || (o.CustomerName != null && o.CustomerName.Contains(searchTerm)));

            if (statusId.HasValue)
                query = query.Where(o => (int)o.Status == statusId.Value);

            if (fromDate.HasValue)
                query = query.Where(o => o.CreatedAt >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(o => o.CreatedAt <= toDate.Value);

            int totalCount = await query.CountAsync();

            var orders = await query
                .OrderByDescending(o => o.CreatedAt)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (orders, totalCount);
        }

        // --- 3. STATUS UPDATE ---
        public async Task UpdateStatusAsync(int orderId, int statusId)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order != null)
            {
                order.Status = (OrderStatus)statusId;
                order.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }

        public async Task CancelOrderDbAsync(int orderId, string cancelReason)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order != null)
            {
                order.Status = OrderStatus.Cancelled;
                order.Notes = string.IsNullOrEmpty(order.Notes)
                    ? $"Lý do hủy: {cancelReason}"
                    : $"{order.Notes} | Lý do hủy: {cancelReason}";
                order.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }

        // --- 4. CHECK & STATS ---
        public async Task<bool> IsOrderCodeExistsAsync(string orderCode)
        {
            return await _context.Orders.AnyAsync(o => o.OrderCode == orderCode);
        }

        public async Task<int> CountTotalOrdersAsync() => await _context.Orders.CountAsync();

        public async Task<decimal> ComputeTotalRevenueAsync()
        {
            return await _context.Orders
                .Where(o => o.Status == OrderStatus.Completed)
                .SumAsync(o => o.TotalAmount);
        }

        public async Task<List<Product>> GetTopSellingProductsAsync(int topN)
        {
            return await _context.OrderItems
                .Where(oi => oi.Order.Status == OrderStatus.Completed)
                .GroupBy(oi => oi.ProductId)
                .Select(g => new { Product = g.First().Product, TotalSold = g.Sum(oi => oi.Quantity) })
                .OrderByDescending(x => x.TotalSold)
                .Take(topN)
                .Select(x => x.Product!) // Dấu chấm than để bỏ qua warning null
                .ToListAsync();
        }
        public async Task<Order?> GetByCodeAndPhoneAsync(string orderCode, string phone)
        {
            var normalizedCode = orderCode.Trim().ToLower();
            var normalizedPhone = PhoneHelper.Normalize(phone);

            return await Task.Run(() =>
                _context.Orders
                    .Include(o => o.Items)
                        .ThenInclude(i => i.Product)
                        .ThenInclude(p => p.Images)
                    .AsEnumerable()
                    .FirstOrDefault(o =>
                        o.OrderCode.ToLower() == normalizedCode &&
                        PhoneHelper.Normalize(o.CustomerPhone) == normalizedPhone
                    )
            );
        }
    }
}