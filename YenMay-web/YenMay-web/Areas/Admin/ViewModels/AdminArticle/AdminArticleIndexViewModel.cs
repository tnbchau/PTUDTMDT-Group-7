using Microsoft.AspNetCore.Mvc.Rendering;
using YenMay_web.Models.Domain;
using YenMay_web.Models.ViewModels.Common;

namespace YenMay_web.Areas.Admin.ViewModels.AdminArticle
{
    public class AdminArticleIndexViewModel
    {
        public List<AdminArticleRowViewModel> Articles { get; set; } = new List<AdminArticleRowViewModel>();
        public List<CategoryArticle> Categories { get; set; } = new List<CategoryArticle>();
        public PaginationViewModel Pagination { get; set; }

        // Các trường cho bộ lọc
        public string? SearchTerm { get; set; }
        public string? SortOrder { get; set; }
        public int? SelectedCategoryId { get; set; }
        public IEnumerable<SelectListItem>? CategoryOptions { get; set; }
    }
}
