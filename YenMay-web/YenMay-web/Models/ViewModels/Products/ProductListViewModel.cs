using YenMay_web.Models.Domain;
using YenMay_web.Models.ViewModels.Common;
using YenMay_web.Models.ViewModels.Products;

namespace YenMay_web.Models.ViewModels.Products
{
    public class ProductListViewModel
    {
        // Dữ liệu chính
        public IEnumerable<ProductCardViewModel> Products { get; set; } = new List<ProductCardViewModel>();
        public IEnumerable<Category> Categories { get; set; } = new List<Category>();

        // Phân trang và lọc
        public PaginationViewModel Pagination { get; set; } = new();
        public ProductFilterViewModel Filter { get; set; } = new();

        // Thống kê
        public int TotalProducts { get; set; }
        public decimal? AppliedMinPrice { get; set; }
        public decimal? AppliedMaxPrice { get; set; }

        // UI
        public bool HasProducts => Products.Any();
        public bool ShowFilter => Categories.Any() || Filter.HasActiveFilters;

        // URL cho phân trang
        public string GetPageUrl(int page)
        {
            var queryParams = new List<string>();

            if (!string.IsNullOrEmpty(Filter.SearchTerm))
                queryParams.Add($"searchTerm={Uri.EscapeDataString(Filter.SearchTerm)}");

            if (!string.IsNullOrEmpty(Filter.CategorySlug))
                queryParams.Add($"categorySlug={Uri.EscapeDataString(Filter.CategorySlug)}");

            if (Filter.MinPrice.HasValue)
                queryParams.Add($"minPrice={Filter.MinPrice}");

            if (Filter.MaxPrice.HasValue)
                queryParams.Add($"maxPrice={Filter.MaxPrice}");

            queryParams.Add($"sortBy={Filter.SortBy}");
            queryParams.Add($"viewMode={Filter.ViewMode}");
            queryParams.Add($"pageSize={Filter.PageSize}");
            queryParams.Add($"page={page}");

            return $"/san-pham?{string.Join("&", queryParams)}";
        }
    }
}