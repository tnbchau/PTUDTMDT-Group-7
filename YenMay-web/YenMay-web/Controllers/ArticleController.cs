using Microsoft.AspNetCore.Mvc;
using YenMay_web.Models.ViewModels.Article;
using YenMay_web.Models.ViewModels.Common;
using YenMay_web.Repositories.Interfaces;
using YenMay_web.Utilities;

namespace YenMay_web.Controllers
{
    public class ArticleController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public ArticleController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IActionResult> Index(string? categorySlug, string? searchTerm, int page = 1)
        {
            int pageSize = 9;
            int? categoryId = null;
            string? currentCategoryName = null;

            if (!string.IsNullOrEmpty(categorySlug))
            {
                var categories = await _unitOfWork.CategoryArticle.GetAllAsync();
                var cat = categories.FirstOrDefault(c => c.Slug == categorySlug);
                if (cat != null)
                {
                    categoryId = cat.Id;
                    currentCategoryName = cat.Name;
                }
            }

            var (articles, totalCount) = await _unitOfWork.Article.GetPublishedArticlesPagedAsync(
                page, pageSize, searchTerm, categoryId);

            var sidebar = await GetSidebarData();

            var viewModel = new ArticleIndexViewModel
            {
                Articles = articles.Select(a => new ArticleCardViewModel
                {
                    Id = a.Id.ToString(),
                    Title = a.Title,
                    Slug = a.Slug,
                    ShortDescription = a.ShortDescription,
                    ImageUrl = ImageHelper.GetImageUrl(a.ImageUrl),
                    CategoryName = a.CategoryArticle?.Name ?? "Tin tức",
                    CategorySlug = a.CategoryArticle?.Slug ?? "mac-dinh",
                    CreatedDate = a.CreatedDate
                }).ToList(),

                // ViewModel yêu cầu List nên ta gán vào List
                Sidebar = new List<ArticleSidebarViewModel> { sidebar },

                SearchTerm = searchTerm,
                CategoryName = currentCategoryName,
                CategorySlug = categorySlug,
                Pagination = new PaginationViewModel
                {
                    PageIndex = page,
                    PageSize = pageSize,
                    TotalCount = totalCount
                }
            };

            return View(viewModel);
        }

        public async Task<IActionResult> Details(string categorySlug, string articleSlug)
        {
            var article = await _unitOfWork.Article.GetBySlugAsync(articleSlug);

            if (article == null) return NotFound();

            var sidebar = await GetSidebarData();

            var viewModel = new ArticleDetailViewModel
            {
                Id = article.Id,
                Title = article.Title,
                Slug = article.Slug,
                Content = article.Content,
                ShortDescription = article.ShortDescription,
                ImageUrl = ImageHelper.GetImageUrl(article.ImageUrl),
                CreatedDate = article.CreatedDate,
                UpdatedDate = article.UpdatedDate,
                CategoryName = article.CategoryArticle?.Name,
                CategorySlug = article.CategoryArticle?.Slug,
                Sidebar = sidebar // Ở đây Detail dùng single object
            };

            var related = await _unitOfWork.Article.GetRelatedArticlesAsync(article.Id, article.CategoryArticleId, 4);
            viewModel.RelatedArticles = related.Select(r => new ArticleCardViewModel
            {
                Title = r.Title,
                Slug = r.Slug,
                CategorySlug = r.CategoryArticle?.Slug ?? "tin-tuc",
                ImageUrl = ImageHelper.GetImageUrl(r.ImageUrl),
                CreatedDate = r.CreatedDate
            }).ToList();

            return View(viewModel);
        }

        private async Task<ArticleSidebarViewModel> GetSidebarData()
        {
            var recentPosts = await _unitOfWork.Article.GetRecentArticlesAsync(5);
            var categories = await _unitOfWork.CategoryArticle.GetAllAsync();

            var sidebar = new ArticleSidebarViewModel
            {
                RecentArticles = recentPosts.Select(r => new ArticleCardViewModel
                {
                    Title = r.Title,
                    Slug = r.Slug,
                    CategorySlug = r.CategoryArticle?.Slug ?? "tin-tuc"
                }).ToList(),

                Categories = new List<ArticleCategoryViewModel>()
            };

            foreach (var cat in categories)
            {
                sidebar.Categories.Add(new ArticleCategoryViewModel
                {
                    Id = cat.Id,
                    Name = cat.Name,
                    Slug = cat.Slug,
                    ArticleCount = await _unitOfWork.CategoryArticle.CountArticlesInCategoryAsync(cat.Id)
                });
            }

            return sidebar;
        }
    }
}