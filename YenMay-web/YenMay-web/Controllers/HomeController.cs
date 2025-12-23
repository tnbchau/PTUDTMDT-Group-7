using Microsoft.AspNetCore.Mvc;
using YenMay_web.Models.Domain;
using YenMay_web.Models.ViewModels.Article;
using YenMay_web.Models.ViewModels.Home;
using YenMay_web.Models.ViewModels.Products;
using YenMay_web.Repositories.Interfaces;
using YenMay_web.Utilities;

namespace YenMay_web.Controllers
{
    public class HomeController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public HomeController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IActionResult> Index()
        {
            var newestProducts = await _unitOfWork.Product.GetNewestProductsAsync(8);

            var allCategories = await _unitOfWork.Category.GetAllAsync();
            var categoriesWithProducts = new List<CategoryProductsViewModel>();

            foreach (var cat in allCategories)
            {
                var products = await _unitOfWork.Product.GetAllAsync("Category,Images,Reviews");
                var productsByCat = products
                    .Where(p => p.CategoryId == cat.Id)
                    .OrderByDescending(p => p.CreatedDate)
                    .Take(8) // Lấy tối đa 8 sp mỗi danh mục
                    .ToList();

                categoriesWithProducts.Add(new CategoryProductsViewModel
                {
                    Name = cat.Name,
                    Slug = cat.Slug,
                    Products = productsByCat.Select(MapProductCard).ToList()
                });
            }

            var recentArticles = await _unitOfWork.Article.GetRecentArticlesAsync(3); // Lấy 3 bài mới nhất

            var model = new HomeIndexViewModel
            {
                HeroTitle = "Yến Sào Yến Mây",
                HeroSubtitle = "Tinh hoa từ thiên nhiên - Sức khỏe cho gia đình",
                HeroImageUrl = "/img-sp/banner.png",

                NewProducts = newestProducts.Select(MapProductCard).ToList(),
                CategoriesWithProducts = categoriesWithProducts,

                LatestArticles = recentArticles.Select(a => new ArticleCardViewModel
                {
                    Id = a.Id.ToString(),
                    Title = a.Title,
                    Slug = a.Slug,
                    CategoryName = a.CategoryArticle?.Name ?? "Tin tức",
                    CategorySlug = a.CategoryArticle?.Slug,
                    ShortDescription = a.ShortDescription,
                    ImageUrl = a.ImageUrl,
                    CreatedDate = a.CreatedDate
                }).ToList()
            };

            return View(model);
        }

        private ProductCardViewModel MapProductCard(Product p)
        {
            var firstImage = p.Images?.FirstOrDefault();

            return new ProductCardViewModel
            {
                Id = p.Id,
                Name = p.Name,
                Slug = p.Slug ?? string.Empty,
                Price = p.Price,

                ThumbnailUrl = firstImage != null
                    ? ImageHelper.GetImageUrl(firstImage.ImageUrl)
                    : "/images/no-image.png",

                ShortDescription = p.ShortDescriptionHTML,

                CategoryName = p.Category?.Name ?? string.Empty,
                CategorySlug = p.Category?.Slug ?? string.Empty,

                StockQuantity = p.StockQuantity,

                // Rating
                AverageRating = p.Reviews != null && p.Reviews.Any()
                    ? p.Reviews.Average(r => r.Rating)
                    : 0,

                ReviewCount = p.Reviews?.Count ?? 0,

                SoldCount = p.SoldCount,

                IsNew = (DateTime.Now - p.CreatedDate).TotalDays <= 30
            };
        }
    }
}
