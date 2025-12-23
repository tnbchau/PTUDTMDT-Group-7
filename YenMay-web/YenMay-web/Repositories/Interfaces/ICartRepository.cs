using YenMay_web.Models.Domain;

namespace YenMay_web.Repositories.Interfaces
{
    public interface ICartRepository
    {
        // Lấy giỏ hàng
        Task<Cart?> GetCartByUserIdAsync(int? userId);
        Task<Cart?> GetCartBySessionIdAsync(string sessionId);
        Task<Cart> GetOrCreateCartAsync(int? userId, string sessionId);

        // Cart items
        Task<CartItem?> GetCartItemAsync(int cartId, int productId);
        Task<List<CartItem>> GetCartItemsAsync(int cartId);
        Task<CartItem> AddOrUpdateCartItemAsync(int cartId, int productId, int quantity, decimal price);
        Task UpdateCartItemQuantityAsync(int cartItemId, int quantity);
        Task RemoveCartItemAsync(int cartItemId);
        Task ClearCartAsync(int cartId);

        // Thống kê
        Task<int> GetCartItemCountAsync(int? userId, string sessionId);

        // Merge carts (khi user đăng nhập)
        Task MergeCartsAsync(int userId, string sessionId);

        // Kiểm tra tồn kho
        Task<bool> ValidateStockAsync(int productId, int quantity);
    }
}