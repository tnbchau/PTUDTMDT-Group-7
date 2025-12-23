namespace YenMay_web.Models.ViewModels.Common
{
    public class BreadcrumbItemViewModel
    {
        public string Name { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }
}