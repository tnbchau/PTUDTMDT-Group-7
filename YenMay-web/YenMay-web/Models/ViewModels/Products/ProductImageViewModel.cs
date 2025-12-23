namespace YenMay_web.Models.ViewModels.Products
{
    public class ProductImageViewModel
    {
        public int Id { get; set; }
        public string ImageUrl { get; set; } = "/images/no-image.png";
        public string AltText { get; set; } = string.Empty;
    }
}
