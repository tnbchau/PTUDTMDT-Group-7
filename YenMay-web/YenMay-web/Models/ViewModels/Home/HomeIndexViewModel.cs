using YenMay_web.Models.ViewModels.Products;
using YenMay_web.Models.ViewModels.Article;

namespace YenMay_web.Models.ViewModels.Home
{
    public class HomeIndexViewModel
    {
        // Hero
        public string? HeroTitle { get; set; } 
        public string? HeroSubtitle { get; set; } 
        public string? HeroImageUrl { get; set; }

        // Sản phẩm
        public List<ProductCardViewModel> NewProducts { get; set; } = new();
        public List<CategoryProductsViewModel> CategoriesWithProducts { get; set; } = new();

        // Bài viết
        public List<ArticleCardViewModel> LatestArticles { get; set; } = new();
    }
    public class CategoryProductsViewModel
    {
        public string Name { get; set; }
        public string Slug { get; set; }
        public List<ProductCardViewModel> Products { get; set; } = new();
    }
}
