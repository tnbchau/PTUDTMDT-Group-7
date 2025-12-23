using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using YenMay_web.Models.Domain;
using YenMay_web.Models.ViewModels.Checkout;
using YenMay_web.Models.ViewModels.Orders;
using YenMay_web.Services.Interfaces;

namespace YenMay_web.Controllers
{
    public class OrderController : Controller
    {
        private readonly IOrderService _orderService;
        private readonly ICartService _cartService;
        private readonly IShippingService _shippingService;

        public OrderController(IOrderService orderService, ICartService cartService, IShippingService shippingService)
        {
            _orderService = orderService;
            _cartService = cartService;
            _shippingService = shippingService;
        }

        // Bước 1: Hiển thị trang nhập thông tin thanh toán
        [HttpGet]
        public async Task<IActionResult> Checkout()
        {
            var cartVM = await _cartService.GetCartIndexViewModelAsync();

            if (cartVM == null || cartVM.IsCartEmpty)
            {
                return RedirectToAction("Index", "Cart");
            }

            var model = new CheckoutViewModel
            {
                CartSummary = cartVM,
                SubTotal = cartVM.SubTotal,
                ShippingFee = cartVM.ShippingFee,
                OrderInfo = new CheckoutRequestViewModel
                {
                    // Tự động điền Email nếu đã đăng nhập
                    CustomerEmail = User.Identity?.IsAuthenticated == true
                        ? User.FindFirstValue(ClaimTypes.Email) ?? "" : ""
                }
            };

            return View(model);
        }

        // Bước 2: Xử lý khi nhấn nút "Đặt hàng"
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout(CheckoutViewModel model)
        {
            var cartVM = await _cartService.GetCartIndexViewModelAsync();

            if (cartVM.IsCartEmpty)
            {
                ModelState.AddModelError("", "Giỏ hàng của bạn đang trống.");
                return RedirectToAction("Index", "Cart");
            }

            if (!ModelState.IsValid)
            {
                model.CartSummary = cartVM;
                model.SubTotal = cartVM.SubTotal;
                model.ShippingFee = cartVM.ShippingFee;
                return View(model);
            }

            try
            {
                // Mapping dữ liệu từ CartViewModel (View) sang Cart (Domain) để truyền vào Service
                var cartDomain = new Cart
                {
                    Items = cartVM.Items.Select(i => new CartItem
                    {
                        ProductId = i.ProductId,
                        Quantity = i.Quantity,
                        Price = i.Price
                    }).ToList()
                };

                int? userId = null;
                if (User.Identity?.IsAuthenticated == true)
                {
                    if (int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int id))
                        userId = id;
                }

                // Thực hiện lưu đơn hàng vào Database qua OrderService
                var order = await _orderService.PlaceOrderAsync(
                    model.OrderInfo,
                    cartDomain,
                    userId,
                    cartVM.ShippingFee,
                    cartVM.AppliedShippingRule?.Name
                );

                // Đặt hàng thành công -> Xóa giỏ hàng trong Session/Db
                await _cartService.ClearCartAsync();

                return RedirectToAction("Success", new { orderCode = order.OrderCode });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Đã xảy ra lỗi khi xử lý đơn hàng: " + ex.Message);
                model.CartSummary = cartVM;
                return View(model);
            }
        }

        // Bước 3: Hiển thị trang hoàn tất đơn hàng
        [HttpGet]
        public async Task<IActionResult> Success(string orderCode)
        {
            if (string.IsNullOrEmpty(orderCode)) return RedirectToAction("Index", "Home");

            var model = await _orderService.GetOrderSuccessViewAsync(orderCode);
            if (model == null) return NotFound();

            return View(model);
        }

        //TRACKING
        [HttpGet]
        public IActionResult Track()
        {
            return View(new OrderTrackViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TrackResult(OrderTrackViewModel model)
        {
            if (!ModelState.IsValid)
                return View("Track", model);

            var orderDetail = await _orderService.TrackOrderAsync(
                model.OrderCode.Trim(),
                model.Phone.Trim()
            );

            if (orderDetail == null)
            {
                ModelState.AddModelError("", "Không tìm thấy đơn hàng hoặc thông tin không khớp.");
                return View("Track", model);
            }

            model.OrderDetail = orderDetail;
            return View("Track", model);
        }
    }
}