using Microsoft.AspNetCore.Mvc;
using YenMay_web.Services.Interfaces;

namespace YenMay_web.Controllers
{
    public class CartController : Controller
    {
        private readonly ICartService _cartService;
        private readonly ILogger<CartController> _logger;

        public CartController(
            ICartService cartService,
            ILogger<CartController> logger)
        {
            _cartService = cartService;
            _logger = logger;
        }

        // ==========================================
        // GET: /Cart (Trang giỏ hàng)
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                var viewModel = await _cartService.GetCartIndexViewModelAsync();
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading cart index");
                TempData["Error"] = "Có lỗi xảy ra khi tải giỏ hàng";
                return View("Error");
            }
        }

        // ==========================================
        // GET: /Cart/Summary (Cho header dropdown)
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> Summary()
        {
            try
            {
                var summary = await _cartService.GetCartSummaryAsync();

                // Nếu giỏ hàng trống
                if (summary.TotalItems <= 0)
                {
                    return PartialView("_CartSummaryEmpty");
                }

                return PartialView("_CartSummary", summary);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading cart summary");
                return PartialView("_CartSummaryEmpty");
            }
        }

        // ==========================================
        // POST: /Cart/Add (Thêm sản phẩm vào giỏ)
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(int productId, int quantity = 1)
        {
            try
            {
                // Validate input
                if (productId <= 0)
                {
                    return Json(new
                    {
                        success = false,
                        message = "Sản phẩm không hợp lệ",
                        totalItems = 0
                    });
                }

                if (quantity <= 0)
                {
                    return Json(new
                    {
                        success = false,
                        message = "Số lượng phải lớn hơn 0",
                        totalItems = 0
                    });
                }

                // Thực hiện thêm vào giỏ
                var result = await _cartService.AddToCartAsync(productId, quantity);

                return Json(new
                {
                    success = result.Success,
                    message = result.Message ?? (result.Success ? "Đã thêm vào giỏ hàng" : "Không thể thêm vào giỏ hàng"),
                    totalItems = result.NewCount
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding product {ProductId} to cart", productId);
                return Json(new
                {
                    success = false,
                    message = "Có lỗi xảy ra khi thêm sản phẩm vào giỏ hàng",
                    totalItems = 0
                });
            }
        }

        // ==========================================
        // POST: /Cart/UpdateQuantity (Cập nhật số lượng)
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateQuantity(int cartItemId, int quantity)
        {
            try
            {
                // Validate
                if (cartItemId <= 0)
                {
                    return Json(new
                    {
                        success = false,
                        message = "ID giỏ hàng không hợp lệ",
                        totalItems = 0
                    });
                }

                if (quantity < 0)
                {
                    return Json(new
                    {
                        success = false,
                        message = "Số lượng không hợp lệ",
                        totalItems = 0
                    });
                }

                // Nếu quantity = 0 thì xóa luôn
                if (quantity == 0)
                {
                    return await Remove(cartItemId);
                }

                // Cập nhật số lượng
                var result = await _cartService.UpdateQuantityAsync(cartItemId, quantity);

                return Json(new
                {
                    success = result.Success,
                    message = result.Message ?? (result.Success ? "Đã cập nhật số lượng" : "Không thể cập nhật số lượng"),
                    totalItems = result.NewCount
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating cart item {CartItemId} quantity to {Quantity}", cartItemId, quantity);
                return Json(new
                {
                    success = false,
                    message = "Có lỗi xảy ra khi cập nhật số lượng",
                    totalItems = 0
                });
            }
        }

        // ==========================================
        // POST: /Cart/Remove (Xóa 1 sản phẩm)
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Remove(int cartItemId)
        {
            try
            {
                if (cartItemId <= 0)
                {
                    return Json(new
                    {
                        success = false,
                        message = "ID giỏ hàng không hợp lệ",
                        totalItems = 0
                    });
                }

                var result = await _cartService.RemoveItemAsync(cartItemId);

                return Json(new
                {
                    success = result.Success,
                    message = result.Message ?? (result.Success ? "Đã xóa sản phẩm" : "Không thể xóa sản phẩm"),
                    totalItems = result.NewCount
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing cart item {CartItemId}", cartItemId);
                return Json(new
                {
                    success = false,
                    message = "Có lỗi xảy ra khi xóa sản phẩm",
                    totalItems = 0
                });
            }
        }

        // ==========================================
        // POST: /Cart/Clear (Xóa toàn bộ giỏ hàng)
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Clear()
        {
            try
            {
                var result = await _cartService.ClearCartAsync();

                return Json(new
                {
                    success = result.Success,
                    message = result.Message ?? (result.Success ? "Đã xóa toàn bộ giỏ hàng" : "Không thể xóa giỏ hàng"),
                    totalItems = result.NewCount
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing cart");
                return Json(new
                {
                    success = false,
                    message = "Có lỗi xảy ra khi xóa giỏ hàng",
                    totalItems = 0
                });
            }
        }

        // ==========================================
        // GET: /Cart/Count (Lấy số lượng item - Optional)
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> Count()
        {
            try
            {
                var summary = await _cartService.GetCartSummaryAsync();
                return Json(new
                {
                    success = true,
                    totalItems = summary.TotalItems
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cart count");
                return Json(new
                {
                    success = false,
                    totalItems = 0
                });
            }
        }
    }
}