using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using YenMay_web.Data;
using YenMay_web.Models.Domain;
using YenMay_web.Repositories.Interfaces;

namespace YenMay_web.Repositories.Implementations
{
    public class CartRepository : ICartRepository
    {
        private readonly YMDbContext _context;
        private readonly ILogger<CartRepository>? _logger;

        // Constructor với logger optional
        public CartRepository(
            YMDbContext context,
            ILogger<CartRepository>? logger = null)
        {
            _context = context;
            _logger = logger;
        }

        // Helper method để log an toàn
        private void LogDebug(string message, params object[] args)
        {
            _logger?.LogDebug(message, args);
        }

        private void LogInformation(string message, params object[] args)
        {
            _logger?.LogInformation(message, args);
        }

        private void LogWarning(string message, params object[] args)
        {
            _logger?.LogWarning(message, args);
        }

        private void LogError(Exception ex, string message, params object[] args)
        {
            _logger?.LogError(ex, message, args);
        }

        public async Task<Cart?> GetCartByUserIdAsync(int? userId)
        {
            try
            {
                if (!userId.HasValue)
                {
                    LogDebug("GetCartByUserIdAsync: UserId is null");
                    return null;
                }

                LogDebug("GetCartByUserIdAsync: Fetching cart for UserId: {UserId}", userId);

                var cart = await _context.Carts
                    .Include(c => c.Items)
                        .ThenInclude(ci => ci.Product)
                            .ThenInclude(p => p.Images)
                    .FirstOrDefaultAsync(c => c.UserId == userId);

                LogDebug("GetCartByUserIdAsync: Result = {Result}", cart != null ? "Found" : "Not Found");

                return cart;
            }
            catch (Exception ex)
            {
                LogError(ex, "Error in GetCartByUserIdAsync for UserId: {UserId}", userId);
                throw;
            }
        }

        public async Task<Cart?> GetCartBySessionIdAsync(string sessionId)
        {
            try
            {
                if (string.IsNullOrEmpty(sessionId))
                {
                    LogDebug("GetCartBySessionIdAsync: SessionId is empty");
                    return null;
                }

                LogDebug("GetCartBySessionIdAsync: Fetching cart for SessionId: {SessionId}", sessionId);

                var cart = await _context.Carts
                    .Include(c => c.Items)
                        .ThenInclude(ci => ci.Product)
                            .ThenInclude(p => p.Images)
                    .FirstOrDefaultAsync(c => c.SessionId == sessionId);

                LogDebug("GetCartBySessionIdAsync: Result = {Result}", cart != null ? "Found" : "Not Found");

                return cart;
            }
            catch (Exception ex)
            {
                LogError(ex, "Error in GetCartBySessionIdAsync for SessionId: {SessionId}", sessionId);
                throw;
            }
        }

        public async Task<Cart> GetOrCreateCartAsync(int? userId, string sessionId)
        {
            try
            {
                LogInformation("🛒 GetOrCreateCartAsync - UserId: {UserId}, SessionId: {SessionId}",
                    userId, sessionId);

                // USER ĐÃ ĐĂNG NHẬP
                if (userId.HasValue)
                {
                    LogDebug("✅ User is logged in - UserId: {UserId}", userId);

                    // Kiểm tra session cart để merge
                    if (!string.IsNullOrEmpty(sessionId))
                    {
                        LogDebug("🔍 Checking for session cart...");
                        var sessionCart = await GetCartBySessionIdAsync(sessionId);

                        if (sessionCart != null)
                        {
                            LogInformation("🔀 Found session cart - Merging...");
                            try
                            {
                                await MergeCartsAsync(userId.Value, sessionId);
                                LogInformation("✅ Cart merged successfully");
                            }
                            catch (Exception ex)
                            {
                                LogError(ex, "❌ Error merging carts");
                            }
                        }
                    }

                    // Lấy hoặc tạo User Cart
                    LogDebug("🔍 Getting user cart...");
                    var userCart = await GetCartByUserIdAsync(userId);

                    if (userCart != null)
                    {
                        LogInformation("✅ User cart found - CartId: {CartId}", userCart.Id);
                        return userCart;
                    }

                    // Tạo mới User Cart
                    LogInformation("➕ Creating new user cart...");
                    userCart = new Cart
                    {
                        UserId = userId,
                        SessionId = null,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    try
                    {
                        await _context.Carts.AddAsync(userCart);
                        await _context.SaveChangesAsync();

                        LogInformation("✅ User cart created - CartId: {CartId}", userCart.Id);
                        return userCart;
                    }
                    catch (Exception ex)
                    {
                        LogError(ex, "❌ Failed to create user cart");
                        throw new Exception("Không thể tạo giỏ hàng cho người dùng", ex);
                    }
                }

                // KHÁCH VÃNG LAI
                if (string.IsNullOrEmpty(sessionId))
                {
                    var error = "SessionId không được để trống cho khách vãng lai";
                    LogError(new ArgumentException(error), "❌ {Error}", error);
                    throw new ArgumentException(error);
                }

                LogDebug("👤 Guest user - SessionId: {SessionId}", sessionId);

                // Kiểm tra Session Cart có sẵn không
                var cart = await GetCartBySessionIdAsync(sessionId);

                if (cart != null)
                {
                    LogInformation("✅ Session cart found - CartId: {CartId}", cart.Id);
                    return cart;
                }

                // Tạo mới Session Cart
                LogInformation("➕ Creating new session cart...");
                cart = new Cart
                {
                    UserId = null,
                    SessionId = sessionId,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                try
                {
                    await _context.Carts.AddAsync(cart);
                    await _context.SaveChangesAsync();

                    LogInformation("✅ Session cart created - CartId: {CartId}", cart.Id);
                    return cart;
                }
                catch (Exception ex)
                {
                    LogError(ex, "❌ Failed to create session cart");
                    throw new Exception("Không thể tạo giỏ hàng tạm", ex);
                }
            }
            catch (Exception ex)
            {
                LogError(ex, "❌ EXCEPTION in GetOrCreateCartAsync - UserId: {UserId}, SessionId: {SessionId}",
                    userId, sessionId);
                throw;
            }
        }

        public async Task<CartItem?> GetCartItemAsync(int cartId, int productId)
        {
            try
            {
                return await _context.CartItems
                    .FirstOrDefaultAsync(ci => ci.CartId == cartId && ci.ProductId == productId);
            }
            catch (Exception ex)
            {
                LogError(ex, "Error in GetCartItemAsync - CartId: {CartId}, ProductId: {ProductId}",
                    cartId, productId);
                throw;
            }
        }

        public async Task<List<CartItem>> GetCartItemsAsync(int cartId)
        {
            try
            {
                return await _context.CartItems
                    .Include(ci => ci.Product)
                        .ThenInclude(p => p.Images)
                    .Include(ci => ci.Product)
                        .ThenInclude(x => x.Category)
                    .Where(ci => ci.CartId == cartId)
                    .OrderByDescending(ci => ci.AddedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                LogError(ex, "Error in GetCartItemsAsync - CartId: {CartId}", cartId);
                throw;
            }
        }

        public async Task<CartItem> AddOrUpdateCartItemAsync(int cartId, int productId, int quantity, decimal price)
        {
            try
            {
                LogDebug("AddOrUpdateCartItemAsync - CartId: {CartId}, ProductId: {ProductId}, Qty: {Qty}",
                    cartId, productId, quantity);

                var item = await _context.CartItems
                    .FirstOrDefaultAsync(x => x.CartId == cartId && x.ProductId == productId);

                if (item != null)
                {
                    LogDebug("Item exists - Updating quantity from {Old} to {New}",
                        item.Quantity, item.Quantity + quantity);

                    item.Quantity += quantity;
                    item.Price = price;
                    _context.CartItems.Update(item);
                }
                else
                {
                    LogDebug("Item doesn't exist - Creating new");

                    item = new CartItem
                    {
                        CartId = cartId,
                        ProductId = productId,
                        Quantity = quantity,
                        Price = price,
                        AddedAt = DateTime.UtcNow
                    };
                    await _context.CartItems.AddAsync(item);
                }

                var cart = await _context.Carts.FindAsync(cartId);
                if (cart != null)
                {
                    cart.UpdatedAt = DateTime.UtcNow;
                    _context.Carts.Update(cart);
                }

                await _context.SaveChangesAsync();

                LogInformation("✅ Cart item saved - ItemId: {ItemId}", item.Id);

                return item;
            }
            catch (Exception ex)
            {
                LogError(ex, "Error in AddOrUpdateCartItemAsync");
                throw;
            }
        }

        public async Task UpdateCartItemQuantityAsync(int cartItemId, int quantity)
        {
            try
            {
                var cartItem = await _context.CartItems.FindAsync(cartItemId);
                if (cartItem == null)
                {
                    LogWarning("CartItem not found - Id: {CartItemId}", cartItemId);
                    return;
                }

                if (quantity <= 0)
                {
                    await RemoveCartItemAsync(cartItemId);
                    return;
                }

                cartItem.Quantity = quantity;

                var cart = await _context.Carts.FindAsync(cartItem.CartId);
                if (cart != null)
                {
                    cart.UpdatedAt = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                LogError(ex, "Error in UpdateCartItemQuantityAsync - CartItemId: {CartItemId}", cartItemId);
                throw;
            }
        }

        public async Task RemoveCartItemAsync(int cartItemId)
        {
            try
            {
                var cartItem = await _context.CartItems
                    .Include(ci => ci.Cart)
                    .FirstOrDefaultAsync(ci => ci.Id == cartItemId);

                if (cartItem != null)
                {
                    _context.CartItems.Remove(cartItem);

                    if (cartItem.Cart != null)
                    {
                        cartItem.Cart.UpdatedAt = DateTime.UtcNow;
                    }

                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                LogError(ex, "Error in RemoveCartItemAsync - CartItemId: {CartItemId}", cartItemId);
                throw;
            }
        }

        public async Task ClearCartAsync(int cartId)
        {
            try
            {
                var cartItems = await _context.CartItems
                    .Where(ci => ci.CartId == cartId)
                    .ToListAsync();

                if (cartItems.Any())
                {
                    _context.CartItems.RemoveRange(cartItems);

                    var cart = await _context.Carts.FindAsync(cartId);
                    if (cart != null)
                    {
                        cart.UpdatedAt = DateTime.UtcNow;
                    }

                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                LogError(ex, "Error in ClearCartAsync - CartId: {CartId}", cartId);
                throw;
            }
        }

        public async Task<int> GetCartItemCountAsync(int? userId, string sessionId)
        {
            try
            {
                Cart? cart = null;

                if (userId.HasValue)
                {
                    cart = await GetCartByUserIdAsync(userId);
                }

                if (cart == null && !string.IsNullOrEmpty(sessionId))
                {
                    cart = await GetCartBySessionIdAsync(sessionId);
                }

                if (cart == null)
                    return 0;

                return await _context.CartItems
                    .Where(ci => ci.CartId == cart.Id)
                    .SumAsync(ci => ci.Quantity);
            }
            catch (Exception ex)
            {
                LogError(ex, "Error in GetCartItemCountAsync");
                return 0;
            }
        }

        public async Task MergeCartsAsync(int userId, string sessionId)
        {
            try
            {
                LogInformation("🔀 MergeCartsAsync - UserId: {UserId}, SessionId: {SessionId}",
                    userId, sessionId);

                var userCart = await GetCartByUserIdAsync(userId);
                var sessionCart = await GetCartBySessionIdAsync(sessionId);

                if (sessionCart == null)
                {
                    LogDebug("No session cart to merge");
                    return;
                }

                if (userCart == null)
                {
                    LogInformation("Converting session cart to user cart");

                    sessionCart.UserId = userId;
                    sessionCart.SessionId = null;
                    sessionCart.UpdatedAt = DateTime.UtcNow;

                    _context.Carts.Update(sessionCart);
                }
                else
                {
                    LogInformation("Merging items from session cart to user cart");

                    var sessionItems = await _context.CartItems
                        .Where(ci => ci.CartId == sessionCart.Id)
                        .ToListAsync();

                    foreach (var sessionItem in sessionItems)
                    {
                        var existingItem = await _context.CartItems
                            .FirstOrDefaultAsync(ci => ci.CartId == userCart.Id && ci.ProductId == sessionItem.ProductId);

                        if (existingItem != null)
                        {
                            existingItem.Quantity += sessionItem.Quantity;
                            _context.CartItems.Update(existingItem);
                        }
                        else
                        {
                            var newItem = new CartItem
                            {
                                CartId = userCart.Id,
                                ProductId = sessionItem.ProductId,
                                Quantity = sessionItem.Quantity,
                                Price = sessionItem.Price,
                                AddedAt = DateTime.UtcNow
                            };
                            await _context.CartItems.AddAsync(newItem);
                        }
                    }

                    _context.Carts.Remove(sessionCart);
                    userCart.UpdatedAt = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();
                LogInformation("✅ Merge completed successfully");
            }
            catch (Exception ex)
            {
                LogError(ex, "Error in MergeCartsAsync - UserId: {UserId}", userId);
                throw;
            }
        }

        public async Task<bool> ValidateStockAsync(int productId, int quantity)
        {
            try
            {
                var product = await _context.Products.FindAsync(productId);
                return product != null && product.StockQuantity >= quantity;
            }
            catch (Exception ex)
            {
                LogError(ex, "Error in ValidateStockAsync - ProductId: {ProductId}", productId);
                return false;
            }
        }
    }
}