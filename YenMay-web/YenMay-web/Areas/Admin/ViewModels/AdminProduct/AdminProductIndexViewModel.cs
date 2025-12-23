using Microsoft.AspNetCore.Mvc.Rendering;
using YenMay_web.Models.ViewModels.Common;

namespace YenMay_web.Areas.Admin.ViewModels.AdminProduct
{
    public class AdminProductIndexViewModel
    {
        public List<AdminProductRowViewModel> Products { get; set; } = new();

        public PaginationViewModel Pagination { get; set; }

        // --- BỘ LỌC (FILTER) ---
        public string? SearchTerm { get; set; }
        public int SelectedCategoryId { get; set; }
        public IEnumerable<SelectListItem>? CategoryOptions { get; set; }

        public string? SortBy { get; set; } = "name"; // Mặc định sắp xếp theo tên
        public string? SortOrder { get; set; } = "asc";
        public int Page { get; set; } = 1;
    }
}