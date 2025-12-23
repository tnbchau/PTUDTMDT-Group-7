using YenMay_web.Models.Domain;

namespace YenMay_web.Repositories.Interfaces
{
    public interface IOrderRepository
    {
        // --- 1. CORE CRUD (Thao tác cơ bản) ---
        Task<Order> AddAsync(Order order);

        Task<Order?> GetByIdAsync(int id);
        Task<Order?> GetByCodeAsync(string orderCode);
        Task<Order?> GetByCodeAndPhoneAsync(string orderCode, string phone);


        // --- 2. QUERY & FILTER (Lọc dữ liệu) ---
        Task<List<Order>> GetByUserIdAsync(int userId);
        Task<(List<Order> Orders, int TotalCount)> GetByUserIdPagedAsync(int userId, int pageIndex, int pageSize);

        // --- 3. STATUS UPDATE (Chỉ update DB) ---
        Task UpdateStatusAsync(int orderId, int statusId);

        Task CancelOrderDbAsync(int orderId, string cancelReason);

        Task<bool> IsOrderCodeExistsAsync(string orderCode);

        Task<int> CountTotalOrdersAsync();
        Task<decimal> ComputeTotalRevenueAsync(); // Chỉ tính đơn thành công
        Task<List<Product>> GetTopSellingProductsAsync(int topN);
        Task<(List<Order> Orders, int TotalCount)> GetForAdminAsync(
        string? searchTerm,
        int? statusId,
        DateTime? fromDate,
        DateTime? toDate,
        int pageIndex,
        int pageSize);
    }
}