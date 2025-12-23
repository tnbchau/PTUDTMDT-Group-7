using Microsoft.AspNetCore.Identity;
using YenMay_web.Models.Domain;
using YenMay_web.Models.ViewModels.Cart;
using YenMay_web.Repositories.Interfaces;
using YenMay_web.Services.Interfaces;
using YenMay_web.Utilities;

namespace YenMay_web.Services
{
    public class CartService : ICartService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly UserManager<User> _userManager;
        private readonly ILogger<CartService> _logger;

        public CartService(
            IUnitOfWork unitOfWork,
            IHttpContextAccessor httpContextAccessor,
            UserManager<User> userManager,
            ILogger<CartService> logger)
        {
            _unitOfWork = unitOfWork;
            _httpContextAccessor = httpContextAccessor;
            _userManager = userManager;
            _logger = logger;
        }

        #region Helpers

        private string GetSessionId()
        {
            try
            {
                var context = _httpContextAccessor.HttpContext;
                if (context == null)
                {
                    _logger.LogWarning("⚠️ HttpContext is null in GetSessionId");
                    return Guid.NewGuid().ToString();
                }

                // ⚠️ QUAN TRỌNG: Đảm bảo Session đã được khởi tạo
                if (context.Session == null)
                {
                    _logger.LogError("❌ Session is null! Check if app.UseSession() is configured");
                    return Guid.NewGuid().ToString();
                }

                var sessionId = context.GetOrCreateCartSessionId();
                _logger.LogDebug("✅ Session ID: {SessionId}", sessionId);
                return sessionId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error getting session ID: {Message}", ex.Message);
                return Guid.NewGuid().ToString();
            }
        }

        private async Task<int?> GetCurrentUserIdAsync()
        {
            try
            {
                var httpContext = _httpContextAccessor.HttpContext;
                if (httpContext == null)
                {
                    _logger.LogWarning("⚠️ HttpContext is null in GetCurrentUserIdAsync");
                    return null;
                }

                if (httpContext.User == null || !httpContext.User.Identity?.IsAuthenticated == true)
                {
                    _logger.LogDebug("ℹ️ User is not authenticated");
                    return null;
                }

                var user = await _userManager.GetUserAsync(httpContext.User);
                var userId = user?.Id;

                _logger.LogDebug("✅ User ID: {UserId}", userId);
                return userId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error getting current user ID: {Message}", ex.Message);
                return null;
            }
        }

        private string GetProductThumbnail(Product? product)
        {
            if (product == null) return "/images/no-image.png";

            var firstImage = product.Images?.FirstOrDefault();
            return firstImage != null
                ? ImageHelper.GetImageUrl(firstImage.ImageUrl)
                : "/images/no-image.png";
        }

        #endregion

        // ==========================================
        // ADD TO CART - ENHANCED DEBUG
        // ==========================================
        public async Task<CartOperationResult> AddToCartAsync(int productId, int quantity)
        {
            try
            {
                _logger.LogInformation("🛒 START AddToCartAsync - ProductId: {ProductId}, Quantity: {Quantity}",
                    productId, quantity);

                // STEP 1: Validate Product
                _logger.LogDebug("📦 Step 1: Fetching product...");
                var product = await _unitOfWork.Product.GetByIdAsync(productId);

                if (product == null)
                {
                    _logger.LogWarning("⚠️ Product not found - ProductId: {ProductId}", productId);
                    return new CartOperationResult
                    {
                        Success = false,
                        Message = "Sản phẩm không tồn tại"
                    };
                }

                _logger.LogDebug("✅ Product found: {ProductName}, Price: {Price}, Stock: {Stock}",
                    product.Name, product.Price, product.StockQuantity);

                // STEP 2: Validate Stock
                _logger.LogDebug("📊 Step 2: Validating stock...");
                if (product.StockQuantity < quantity)
                {
                    _logger.LogWarning("⚠️ Insufficient stock - Available: {Available}, Requested: {Requested}",
                        product.StockQuantity, quantity);
                    return new CartOperationResult
                    {
                        Success = false,
                        Message = $"Số lượng trong kho không đủ (còn {product.StockQuantity})"
                    };
                }

                _logger.LogDebug("✅ Stock validation passed");

                // STEP 3: Get User ID and Session ID
                _logger.LogDebug("👤 Step 3: Getting user and session info...");
                var userId = await GetCurrentUserIdAsync();
                var sessionId = GetSessionId();

                _logger.LogInformation("📋 User Info - UserId: {UserId}, SessionId: {SessionId}",
                    userId?.ToString() ?? "null", sessionId);

                // STEP 4: Get or Create Cart
                _logger.LogDebug("🛒 Step 4: Getting or creating cart...");
                Cart? cart = null;

                try
                {
                    cart = await _unitOfWork.Cart.GetOrCreateCartAsync(userId, sessionId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ Failed to get or create cart: {Message}", ex.Message);
                    return new CartOperationResult
                    {
                        Success = false,
                        Message = "Không thể tạo giỏ hàng. Vui lòng thử lại."
                    };
                }

                if (cart == null)
                {
                    _logger.LogError("❌ Cart is null after GetOrCreateCartAsync");
                    return new CartOperationResult
                    {
                        Success = false,
                        Message = "Không thể tạo giỏ hàng"
                    };
                }

                _logger.LogDebug("✅ Cart obtained - CartId: {CartId}", cart.Id);

                // STEP 5: Check if item already exists
                _logger.LogDebug("🔍 Step 5: Checking existing cart item...");
                var existingItem = await _unitOfWork.Cart.GetCartItemAsync(cart.Id, productId);

                if (existingItem != null)
                {
                    var newTotalQuantity = existingItem.Quantity + quantity;
                    _logger.LogDebug("ℹ️ Item exists - Current: {Current}, Adding: {Adding}, Total: {Total}",
                        existingItem.Quantity, quantity, newTotalQuantity);

                    if (newTotalQuantity > product.StockQuantity)
                    {
                        _logger.LogWarning("⚠️ Total quantity exceeds stock - Stock: {Stock}, Total: {Total}",
                            product.StockQuantity, newTotalQuantity);
                        return new CartOperationResult
                        {
                            Success = false,
                            Message = $"Tổng số lượng vượt quá kho ({product.StockQuantity}). Bạn đã có {existingItem.Quantity} trong giỏ."
                        };
                    }
                }

                // STEP 6: Add or Update Cart Item
                _logger.LogDebug("💾 Step 6: Adding/updating cart item...");
                try
                {
                    await _unitOfWork.Cart.AddOrUpdateCartItemAsync(cart.Id, productId, quantity, product.Price);
                    _logger.LogDebug("✅ Cart item added/updated successfully");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ Failed to add/update cart item: {Message}", ex.Message);
                    return new CartOperationResult
                    {
                        Success = false,
                        Message = "Không thể thêm sản phẩm vào giỏ"
                    };
                }

                // STEP 7: Get new count
                _logger.LogDebug("🔢 Step 7: Getting new cart count...");
                int newCount = 0;
                try
                {
                    newCount = await _unitOfWork.Cart.GetCartItemCountAsync(userId, sessionId);
                    _logger.LogDebug("✅ New cart count: {NewCount}", newCount);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ Failed to get cart count: {Message}", ex.Message);
                    // Không return lỗi vì item đã thêm thành công
                    newCount = existingItem != null ? existingItem.Quantity + quantity : quantity;
                }

                _logger.LogInformation("✅ SUCCESS AddToCartAsync - ProductId: {ProductId}, NewCount: {NewCount}",
                    productId, newCount);

                return new CartOperationResult
                {
                    Success = true,
                    Message = "Đã thêm vào giỏ hàng",
                    NewCount = newCount
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ EXCEPTION in AddToCartAsync - ProductId: {ProductId}, Exception: {Exception}",
                    productId, ex.ToString());

                return new CartOperationResult
                {
                    Success = false,
                    Message = "Có lỗi xảy ra khi thêm vào giỏ hàng"
                };
            }
        }

        // ==========================================
        // GET CART INDEX VIEW MODEL
        // ==========================================
        public async Task<CartIndexViewModel> GetCartIndexViewModelAsync()
        {
            try
            {
                _logger.LogInformation("📋 START GetCartIndexViewModelAsync");

                var userId = await GetCurrentUserIdAsync();
                var sessionId = GetSessionId();

                _logger.LogInformation("User: {UserId}, Session: {SessionId}", userId, sessionId);

                var cart = await _unitOfWork.Cart.GetOrCreateCartAsync(userId, sessionId);

                if (cart == null)
                {
                    _logger.LogWarning("⚠️ Cart is null");
                    return new CartIndexViewModel
                    {
                        Items = new List<CartItemViewModel>(),
                        SubTotal = 0,
                        ShippingFee = 0
                    };
                }

                _logger.LogInformation("✅ Cart found - CartId: {CartId}", cart.Id);

                var cartItems = await _unitOfWork.Cart.GetCartItemsAsync(cart.Id);

                _logger.LogInformation("✅ Cart items count: {Count}", cartItems.Count);

                var viewModel = new CartIndexViewModel
                {
                    Items = cartItems.Select(ci => new CartItemViewModel
                    {
                        Id = ci.Id,
                        ProductId = ci.ProductId,
                        ProductName = ci.Product?.Name ?? "Sản phẩm không tồn tại",
                        ProductImage = GetProductThumbnail(ci.Product),
                        Slug = ci.Product?.Slug ?? string.Empty,
                        CategorySlug = ci.Product?.Category?.Slug ?? "san-pham",
                        Price = ci.Price,
                        Quantity = ci.Quantity,
                        Stock = ci.Product?.StockQuantity ?? 0
                    }).ToList()
                };

                viewModel.SubTotal = viewModel.Items.Sum(x => x.Total);
                viewModel.ShippingFee = CalculateShippingFee(viewModel.SubTotal);

                _logger.LogInformation("✅ SUCCESS GetCartIndexViewModelAsync - Items: {Items}, SubTotal: {SubTotal}",
                    viewModel.TotalItems, viewModel.SubTotal);

                return viewModel;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ EXCEPTION in GetCartIndexViewModelAsync: {Exception}", ex.ToString());
                return new CartIndexViewModel
                {
                    Items = new List<CartItemViewModel>(),
                    SubTotal = 0,
                    ShippingFee = 0,
                    Message = "Có lỗi xảy ra khi tải giỏ hàng"
                };
            }
        }

        // ==========================================
        // GET CART SUMMARY
        // ==========================================
        public async Task<CartSummaryViewModel> GetCartSummaryAsync()
        {
            try
            {
                var userId = await GetCurrentUserIdAsync();
                var sessionId = GetSessionId();

                var cart = await _unitOfWork.Cart.GetOrCreateCartAsync(userId, sessionId);

                if (cart == null || cart.Items == null || !cart.Items.Any())
                {
                    return new CartSummaryViewModel
                    {
                        TotalItems = 0,
                        SubTotal = 0
                    };
                }

                var cartItems = await _unitOfWork.Cart.GetCartItemsAsync(cart.Id);

                return new CartSummaryViewModel
                {
                    TotalItems = cartItems.Sum(i => i.Quantity),
                    SubTotal = cartItems.Sum(i => i.Quantity * i.Price)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error in GetCartSummaryAsync: {Message}", ex.Message);
                return new CartSummaryViewModel
                {
                    TotalItems = 0,
                    SubTotal = 0
                };
            }
        }

        // ==========================================
        // UPDATE QUANTITY
        // ==========================================
        public async Task<CartOperationResult> UpdateQuantityAsync(int cartItemId, int quantity)
        {
            try
            {
                _logger.LogInformation("🔄 Updating quantity - CartItemId: {CartItemId}, Quantity: {Quantity}",
                    cartItemId, quantity);

                if (quantity < 0)
                {
                    return new CartOperationResult
                    {
                        Success = false,
                        Message = "Số lượng không hợp lệ"
                    };
                }

                if (quantity == 0)
                {
                    return await RemoveItemAsync(cartItemId);
                }

                await _unitOfWork.Cart.UpdateCartItemQuantityAsync(cartItemId, quantity);

                var userId = await GetCurrentUserIdAsync();
                var sessionId = GetSessionId();
                var newCount = await _unitOfWork.Cart.GetCartItemCountAsync(userId, sessionId);

                _logger.LogInformation("✅ Quantity updated - NewCount: {NewCount}", newCount);

                return new CartOperationResult
                {
                    Success = true,
                    Message = "Đã cập nhật số lượng",
                    NewCount = newCount
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error updating quantity - CartItemId: {CartItemId}", cartItemId);
                return new CartOperationResult
                {
                    Success = false,
                    Message = "Có lỗi xảy ra khi cập nhật số lượng"
                };
            }
        }

        // ==========================================
        // REMOVE ITEM
        // ==========================================
        public async Task<CartOperationResult> RemoveItemAsync(int cartItemId)
        {
            try
            {
                _logger.LogInformation("🗑️ Removing item - CartItemId: {CartItemId}", cartItemId);

                await _unitOfWork.Cart.RemoveCartItemAsync(cartItemId);

                var userId = await GetCurrentUserIdAsync();
                var sessionId = GetSessionId();
                var newCount = await _unitOfWork.Cart.GetCartItemCountAsync(userId, sessionId);

                _logger.LogInformation("✅ Item removed - NewCount: {NewCount}", newCount);

                return new CartOperationResult
                {
                    Success = true,
                    Message = "Đã xóa sản phẩm khỏi giỏ hàng",
                    NewCount = newCount
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error removing item - CartItemId: {CartItemId}", cartItemId);
                return new CartOperationResult
                {
                    Success = false,
                    Message = "Có lỗi xảy ra khi xóa sản phẩm"
                };
            }
        }

        // ==========================================
        // CLEAR CART
        // ==========================================
        public async Task<CartOperationResult> ClearCartAsync()
        {
            try
            {
                _logger.LogInformation("🧹 Clearing cart");

                var userId = await GetCurrentUserIdAsync();
                var sessionId = GetSessionId();
                var cart = await _unitOfWork.Cart.GetOrCreateCartAsync(userId, sessionId);

                if (cart != null)
                {
                    await _unitOfWork.Cart.ClearCartAsync(cart.Id);
                    _logger.LogInformation("✅ Cart cleared - CartId: {CartId}", cart.Id);
                }

                return new CartOperationResult
                {
                    Success = true,
                    Message = "Đã xóa toàn bộ giỏ hàng",
                    NewCount = 0
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error clearing cart");
                return new CartOperationResult
                {
                    Success = false,
                    Message = "Có lỗi xảy ra khi xóa giỏ hàng"
                };
            }
        }

        // ==========================================
        // MERGE CART AFTER LOGIN
        // ==========================================
        public async Task MergeCartAfterLoginAsync(int userId)
        {
            try
            {
                var sessionId = GetSessionId();
                _logger.LogInformation("🔀 Merging cart - UserId: {UserId}, SessionId: {SessionId}",
                    userId, sessionId);

                await _unitOfWork.Cart.MergeCartsAsync(userId, sessionId);

                _logger.LogInformation("✅ Cart merged successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error merging cart - UserId: {UserId}", userId);
                throw;
            }
        }

        // ==========================================
        // PRIVATE HELPERS
        // ==========================================
        private decimal CalculateShippingFee(decimal subTotal)
        {
            if (subTotal >= 500000)
                return 0;

            return 30000;
        }
    }
}