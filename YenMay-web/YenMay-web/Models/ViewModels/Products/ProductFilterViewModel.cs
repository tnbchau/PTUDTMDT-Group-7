using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace YenMay_web.Models.ViewModels.Products
{
    public class ProductFilterViewModel
    {
        [Display(Name = "Tìm kiếm")]
        public string? SearchTerm { get; set; }

        [Display(Name = "Danh mục")]
        public string? CategorySlug { get; set; }

        [Display(Name = "Giá từ")]
        [Range(0, double.MaxValue, ErrorMessage = "Giá phải lớn hơn 0")]
        public decimal? MinPrice { get; set; }

        [Display(Name = "Giá đến")]
        [Range(0, double.MaxValue, ErrorMessage = "Giá phải lớn hơn 0")]
        public decimal? MaxPrice { get; set; }

        [Display(Name = "Sắp xếp")]
        public string SortBy { get; set; } = "default";

        [Display(Name = "Xem theo")]
        public string ViewMode { get; set; } = "grid"; // grid, list

        [Display(Name = "Hiển thị")]
        public int PageSize { get; set; } = 12;

        // Options
        public List<SelectListItem> SortOptions { get; } = new()
        {
            new SelectListItem { Text = "Mặc định", Value = "default" },
            new SelectListItem { Text = "Giá thấp đến cao", Value = "price_asc" },
            new SelectListItem { Text = "Giá cao đến thấp", Value = "price_desc" },
            new SelectListItem { Text = "Tên A-Z", Value = "name_asc" },
            new SelectListItem { Text = "Tên Z-A", Value = "name_desc" },
            new SelectListItem { Text = "Mới nhất", Value = "newest" },
            new SelectListItem { Text = "Bán chạy", Value = "best_selling" },
            new SelectListItem { Text = "Đánh giá cao", Value = "top_rated" }
        };

        public bool HasActiveFilters =>
            !string.IsNullOrEmpty(SearchTerm) ||
            !string.IsNullOrEmpty(CategorySlug) ||
            MinPrice.HasValue ||
            MaxPrice.HasValue ||
            SortBy != "default";
    }
}