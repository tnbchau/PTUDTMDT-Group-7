namespace YenMay_web.Models.ViewModels.Article
{
    public class ArticleDetailViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Slug { get; set; }
        public string ShortDescription { get; set; }
        public string Content { get; set; }
        public string ImageUrl { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }

        // Category info
        public string? CategoryName { get; set; }
        public string? CategorySlug { get; set; }

        // Format properties
        public string DisplayDate => UpdatedDate.HasValue
            ? UpdatedDate.Value.ToString("dd/MM/yyyy HH:mm")
            : CreatedDate.ToString("dd/MM/yyyy HH:mm");

        public string DisplayUpdatedDate => UpdatedDate.HasValue
            ? "Cập nhật: " + UpdatedDate.Value.ToString("dd/MM/yyyy")
            : "Xuất bản: " + CreatedDate.ToString("dd/MM/yyyy");

        // Related articles
        public List<ArticleCardViewModel> RelatedArticles { get; set; } = new();

        // Sidebar
        public ArticleSidebarViewModel Sidebar { get; set; }
    }
}