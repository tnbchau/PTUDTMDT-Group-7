using Microsoft.AspNetCore.Mvc;
using YenMay_web.Areas.Admin.ViewModels.Shipping;
using YenMay_web.Models.ViewModels.Common;
using YenMay_web.Services.Interfaces;
using YenMay_web.Models.ViewModels.Shipping; 

namespace YenMay_web.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class AdminShippingRuleController : Controller
    {
        private readonly IShippingService _shippingService;
        private const int PageSize = 10;

        public AdminShippingRuleController(IShippingService shippingService)
        {
            _shippingService = shippingService;
        }

        // =========================
        // 1. INDEX / DANH SÁCH RULES
        // =========================
        public async Task<IActionResult> Index(string? searchTerm, bool? isActive, int page = 1)
        {
            var allRules = await _shippingService.GetAllRulesAsync();
            var query = allRules.AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
                query = query.Where(r => r.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase));

            if (isActive.HasValue)
                query = query.Where(r => r.IsActive == isActive.Value);

            var totalCount = query.Count();
            var rulesPaged = query
                .OrderBy(r => r.MinOrderValue)
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .Select(r => new ShippingRuleRowViewModel
                {
                    Id = r.Id,
                    Name = r.Name,
                    Description = r.Description,
                    MinOrderValue = r.MinOrderValue,
                    MaxOrderValue = r.MaxOrderValue,
                    ShippingFee = r.ShippingFee,
                    IsActive = r.IsActive
                })
                .ToList();

            var model = new ShippingRuleIndexViewModel
            {
                Rules = rulesPaged,
                Page = page,
                PageSize = PageSize,
                SearchTerm = searchTerm,
                IsActive = isActive,
                Pagination = new PaginationViewModel
                {
                    PageIndex = page,
                    PageSize = PageSize,
                    TotalCount = totalCount
                }
            };

            return View(model);
        }

        // =========================
        // 2. CREATE / THÊM MỚI (AJAX)
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ShippingRuleFormViewModel model)
        {
            if (!ModelState.IsValid)
                return Json(new { success = false, message = "Dữ liệu nhập vào không hợp lệ." });

            try
            {
                var ruleViewModel = MapToViewModel(model);
                ruleViewModel.Id = 0;
                await _shippingService.CreateRuleAsync(ruleViewModel);

                return Json(new { success = true, message = "Tạo quy tắc vận chuyển thành công." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        // =========================
        // 3. EDIT / CHỈNH SỬA (AJAX)
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ShippingRuleFormViewModel model)
        {
            if (!ModelState.IsValid)
                return Json(new { success = false, message = "Dữ liệu chỉnh sửa không hợp lệ." });

            try
            {
                var ruleViewModel = MapToViewModel(model);
                await _shippingService.UpdateRuleAsync(ruleViewModel);

                return Json(new { success = true, message = "Cập nhật thành công." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        // =========================
        // 4. DELETE / XÓA (AJAX)
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _shippingService.DeleteRuleAsync(id);
                return Json(new { success = true, message = "Xóa quy tắc vận chuyển thành công." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        // =========================
        // 5. TOGGLE STATUS / BẬT-TẮT (AJAX)
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            try
            {
                await _shippingService.ToggleRuleStatusAsync(id);
                return Json(new { success = true, message = "Cập nhật trạng thái thành công." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        // =========================
        // MAPPING HELPER
        // =========================
        private ShippingRuleViewModel MapToViewModel(ShippingRuleFormViewModel model)
        {
            return new ShippingRuleViewModel
            {
                Id = model.Id,
                Name = model.Name,
                Description = model.Description,
                MinOrderValue = model.MinOrderValue,
                MaxOrderValue = model.MaxOrderValue,
                ShippingFee = model.ShippingFee,
                IsActive = model.IsActive
            };
        }
    }
}
