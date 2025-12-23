using YenMay_web.Models.Domain;
using YenMay_web.Models.ViewModels.Cart;

namespace YenMay_web.Services.Interfaces
{
    public interface ICartService
    {
        // Lấy dữ liệu cho trang Giỏ hàng đầy đủ
        Task<CartIndexViewModel> GetCartIndexViewModelAsync();

        // Lấy dữ liệu tóm tắt (số lượng, tổng tiền) cho Header/Icon
        Task<CartSummaryViewModel> GetCartSummaryAsync();

        // Các thao tác nghiệp vụ
        Task<CartOperationResult> AddToCartAsync(int productId, int quantity);
        Task<CartOperationResult> UpdateQuantityAsync(int cartItemId, int quantity);
        Task<CartOperationResult> RemoveItemAsync(int cartItemId);
        Task<CartOperationResult> ClearCartAsync();

        // Đồng bộ giỏ hàng
        Task MergeCartAfterLoginAsync(int userId);
    }

    public class CartOperationResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int NewCount { get; set; } // Để cập nhật số trên icon giỏ hàng
    }
}