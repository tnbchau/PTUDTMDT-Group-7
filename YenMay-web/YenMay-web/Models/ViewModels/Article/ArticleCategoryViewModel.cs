namespace YenMay_web.Models.ViewModels.Article
{
    public class ArticleCategoryViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Slug { get; set; } = null!;
        public int ArticleCount { get; set; }
    }
}
