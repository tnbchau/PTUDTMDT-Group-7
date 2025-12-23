using Microsoft.AspNetCore.Mvc;
using YenMay_web.Areas.Admin.ViewModels.Shipping;
using YenMay_web.Models.ViewModels.Common;
using YenMay_web.Services.Interfaces;

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
        // 2. CREATE / THÊM MỚI
        // =========================
        public IActionResult Create()
        {
            var model = new ShippingRuleFormViewModel();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ShippingRuleFormViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                var ruleViewModel = new YenMay_web.Models.ViewModels.Shipping.ShippingRuleViewModel
                {
                    Id = 0,
                    Name = model.Name,
                    Description = model.Description,
                    MinOrderValue = model.MinOrderValue,
                    MaxOrderValue = model.MaxOrderValue,
                    ShippingFee = model.ShippingFee,
                    IsActive = model.IsActive
                };

                await _shippingService.CreateRuleAsync(ruleViewModel);
                TempData["Success"] = "Tạo quy tắc vận chuyển thành công.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Lỗi: " + ex.Message);
                return View(model);
            }
        }

        // =========================
        // 3. EDIT / CHỈNH SỬA
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ShippingRuleFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Dữ liệu không hợp lệ.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var ruleViewModel = MapToViewModel(model);
                await _shippingService.UpdateRuleAsync(ruleViewModel);
                TempData["Success"] = "Cập nhật thành công.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Lỗi: " + ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }
                
        // =========================
        // 4. DELETE / XÓA
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _shippingService.DeleteRuleAsync(id);
                TempData["Success"] = "Xóa quy tắc vận chuyển thành công.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Lỗi: " + ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        // =========================
        // 5. TOGGLE STATUS / BẬT/TẮT
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            try
            {
                await _shippingService.ToggleRuleStatusAsync(id);
                TempData["Success"] = "Cập nhật trạng thái quy tắc thành công.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Lỗi: " + ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }
        private YenMay_web.Models.ViewModels.Shipping.ShippingRuleViewModel MapToViewModel(ShippingRuleFormViewModel model)
        {
            return new YenMay_web.Models.ViewModels.Shipping.ShippingRuleViewModel
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
