using YenMay_web.Models.ViewModels.Common;

namespace YenMay_web.Models.ViewModels.Article
{
    public class ArticleIndexViewModel
    {
        // Grid bài viết
        public List<ArticleCardViewModel> Articles { get; set; } = new();

        // Sidebar
        public List<ArticleSidebarViewModel> Sidebar { get; set; } 

        // Filter
        public string? SearchTerm { get; set; }
        public string? CategoryId { get; set; }
        public string? CategoryName { get; set; }
        public string? CategorySlug { get; set; }

        // Pagination
        public PaginationViewModel Pagination { get; set; } = new();

    }
}
