namespace YenMay_web.Areas.Admin.ViewModels
{
    public class ProductAdminImageViewModel
    {
        public int Id { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public bool IsMain { get; set; }
    }
}
