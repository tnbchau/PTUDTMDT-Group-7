using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using YenMay_web.Areas.Admin.ViewModels.AdminOrder;
using YenMay_web.Enums;
using YenMay_web.Models.ViewModels.Common;
using YenMay_web.Repositories.Interfaces;
using YenMay_web.Services.Interfaces;
using YenMay_web.Utilities;

namespace YenMay_web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")] // Đảm bảo chỉ Admin mới truy cập được
    public class AdminOrderController : Controller
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IOrderService _orderService;

        public AdminOrderController(IOrderRepository orderRepository, IOrderService orderService)
        {
            _orderRepository = orderRepository;
            _orderService = orderService;
        }

        // --- 1. DANH SÁCH ĐƠN HÀNG ---
        public async Task<IActionResult> Index(string? searchTerm, int? statusId, DateTime? fromDate, int page = 1)
        {
            int pageSize = 10;
            var (orders, totalCount) = await _orderRepository.GetForAdminAsync(
                searchTerm, statusId, fromDate, null, page, pageSize);
            var statusItems = Enum.GetValues(typeof(OrderStatus))
                    .Cast<OrderStatus>()
                    .Select(s => new SelectListItem
                    {
                        Value = ((int)s).ToString(),
                        Text = OrderHelper.GetOrderStatusText(s),
                        Selected = statusId == (int)s
                    }).ToList();

            statusItems.Insert(0, new SelectListItem { Value = "", Text = "--- Tất cả trạng thái ---" });

            ViewBag.StatusList = statusItems;

            var model = new AdminOrderIndexViewModel
            {
                Orders = orders.Select(o => new AdminOrderRowViewModel
                {
                    Id = o.Id,
                    OrderCode = o.OrderCode,
                    Status = o.Status,
                    TotalItems = o.Items.Count,
                    TotalAmount = o.TotalAmount,
                    PaymentMethod = o.PaymentMethod,
                    CreatedAt = o.CreatedAt
                }).ToList(),
                Pagination = new PaginationViewModel
                {
                    PageIndex = page,
                    PageSize = pageSize,
                    TotalCount = totalCount
                },
                SearchTerm = searchTerm,
                StatusId = statusId,
                FromDate = fromDate
            };

            return View(model);
        }

        // --- 2. CHI TIẾT & CẬP NHẬT TRẠNG THÁI ---
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var orderEntity = await _orderRepository.GetByIdAsync(id);
            if (orderEntity == null) return NotFound();

            // Lấy DetailView từ Service (đã định dạng sẵn tiền tệ, ảnh sản phẩm...)
            var orderDetailView = await _orderService.GetOrderDetailViewAsync(orderEntity.OrderCode);

            if (orderDetailView == null) return NotFound();

            var model = new AdminOrderFormViewModel
            {
                Id = orderEntity.Id,
                Order = orderDetailView,
                Status = orderEntity.Status
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(AdminOrderFormViewModel model)
        {
            // Cập nhật trạng thái thông thông qua Service
            await _orderService.UpdateOrderStatusAsync(model.Id, (int)model.Status);

            TempData["Success"] = $"Đã cập nhật trạng thái đơn hàng #{model.Id} thành công.";

            // Quay lại trang chi tiết hoặc danh sách
            return RedirectToAction(nameof(Details), new { id = model.Id });
        }

        // --- 3. HỦY ĐƠN (DÀNH CHO ADMIN) ---
        [HttpPost]
        public async Task<IActionResult> CancelOrder(int id, string reason)
        {
            var result = await _orderService.CancelOrderAsync(id, $"Admin hủy: {reason}");

            if (result)
                TempData["Success"] = "Đã hủy đơn hàng thành công.";
            else
                TempData["Error"] = "Không thể hủy đơn hàng này (có thể đơn đã giao hoặc hoàn thành).";

            return RedirectToAction(nameof(Index));
        }
    }
}