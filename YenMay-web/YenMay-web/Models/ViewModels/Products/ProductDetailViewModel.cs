using YenMay_web.Areas.Admin.ViewModels;
using YenMay_web.Models.ViewModels.Products;
using YenMay_web.Models.ViewModels.Common;  

namespace YenMay_web.Models.ViewModels.Products
{
    public class ProductDetailViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string SKU { get; set; } = string.Empty;

        public decimal Price { get; set; }
        public string PriceFormatted => Price.ToString("N0") + "₫";

        public string ShortDescriptionHtml { get; set; } = string.Empty;
        public string FullDescriptionHtml { get; set; } = string.Empty;
        public string AdditionalInfoHtml { get; set; } = string.Empty;

        public int StockQuantity { get; set; }
        public bool InStock => StockQuantity > 0;

        public string CategoryName { get; set; } = string.Empty;
        public string CategorySlug { get; set; } = string.Empty;

        public IEnumerable<ProductImageViewModel> Images { get; set; }
            = Enumerable.Empty<ProductImageViewModel>();

        public IEnumerable<ProductReviewViewModel> Reviews { get; set; }
            = Enumerable.Empty<ProductReviewViewModel>();

        public IEnumerable<ProductCardViewModel> RelatedProducts { get; set; }
            = Enumerable.Empty<ProductCardViewModel>();

        public IEnumerable<BreadcrumbItemViewModel> Breadcrumbs { get; set; }
            = Enumerable.Empty<BreadcrumbItemViewModel>();
    }
}