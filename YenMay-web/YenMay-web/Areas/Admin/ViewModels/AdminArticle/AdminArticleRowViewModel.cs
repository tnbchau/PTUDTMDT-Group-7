namespace YenMay_web.Areas.Admin.ViewModels.AdminArticle
{
    public class AdminArticleRowViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string ShortDescription { get; set; }
        public string ImageUrl { get; set; }
        public string CategoryName { get; set; }
        public bool IsPublished { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }

        public string DisplayDate => UpdatedDate.HasValue
            ? UpdatedDate.Value.ToString("dd/MM/yyyy HH:mm")
            : CreatedDate.ToString("dd/MM/yyyy HH:mm");
    }
}
