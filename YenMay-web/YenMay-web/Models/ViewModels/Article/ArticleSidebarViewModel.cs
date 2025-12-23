namespace YenMay_web.Models.ViewModels.Article
{
    public class ArticleSidebarViewModel
    {
        public List<ArticleCardViewModel> RecentArticles { get; set; } = new();

        public List<ArticleCategoryViewModel> Categories { get; set; } = new();

        // Search term (for search form)
    }
}
