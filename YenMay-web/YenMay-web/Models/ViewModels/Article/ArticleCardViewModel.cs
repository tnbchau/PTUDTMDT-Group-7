namespace YenMay_web.Models.ViewModels.Article
{
    public class ArticleCardViewModel
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Slug { get; set; }

        public string CategoryName { get; set; }
        public string CategorySlug { get; set; }

        public string ShortDescription { get; set; }
        public string ImageUrl { get; set; }

        public DateTime CreatedDate { get; set; }
    }
}
