namespace YenMay_web.Models.ViewModels.Products
{
    public class CategoryFilterViewModel
    {
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }
}