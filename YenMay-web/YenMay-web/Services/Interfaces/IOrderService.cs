using YenMay_web.Models.Domain;
using YenMay_web.Models.ViewModels.Checkout;
using YenMay_web.Models.ViewModels.Orders;

namespace YenMay_web.Services.Interfaces
{
    public interface IOrderService
    {
        // --- 1. XỬ LÝ GIAO DỊCH (Transaction Logic) ---

        // [ĐÃ SỬA]: Thêm tham số int? userId vào đây để khớp với OrderService
        Task<Order> PlaceOrderAsync(
            CheckoutRequestViewModel model,
            Cart cart,
            int? userId,
            decimal shippingFee,
            string? shippingRuleName = null);

        // Hủy đơn
        Task<bool> CancelOrderAsync(int orderId, string? reason = null);

        // Cập nhật trạng thái
        Task UpdateOrderStatusAsync(int orderId, int statusId, string? trackingNumber = null);

        // --- 2. LẤY DỮ LIỆU HIỂN THỊ ---
        Task<OrderDetailViewModel?> GetOrderDetailViewAsync(string orderCode, int? userId = null);
        Task<OrderSuccessViewModel?> GetOrderSuccessViewAsync(string orderCode);
        Task<OrderHistoryViewModel> GetOrderHistoryViewAsync(int? userId, int page = 1, int pageSize = 10);

        // --- 3. TIỆN ÍCH ---
        Task<Order?> GetOrderEntityByIdAsync(int id);
        Task<OrderDetailViewModel?> TrackOrderAsync(string orderCode, string phone);
    }
}