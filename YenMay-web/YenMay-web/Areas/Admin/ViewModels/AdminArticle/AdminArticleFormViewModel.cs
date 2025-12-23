using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace YenMay_web.Areas.Admin.ViewModels.AdminArticle
{
    public class AdminArticleFormViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Tiêu đề bài viết")]
        [Required(ErrorMessage = "Vui lòng nhập tiêu đề")]
        [MaxLength(500, ErrorMessage = "Tiêu đề không được quá 500 ký tự")]
        public string Title { get; set; }
        [Display(Name = "Danh mục")]
        public int? CategoryArticleId { get; set; } // ID được chọn

        // Danh sách đổ vào Dropdown
        public IEnumerable<SelectListItem>? CategoryList { get; set; }

        [Display(Name = "Mô tả ngắn")]
        [Required(ErrorMessage = "Vui lòng nhập mô tả ngắn")]
        [MaxLength(300, ErrorMessage = "Mô tả ngắn không được quá 300 ký tự")]
        public string ShortDescription { get; set; }

        [Display(Name = "Nội dung bài viết")]
        [Required(ErrorMessage = "Vui lòng nhập nội dung")]
        public string Content { get; set; } // TinyMCE HTML

        [Display(Name = "Hiển thị bài viết")]
        public bool IsPublished { get; set; } = true;

        // --- Xử lý ảnh ---

        [Display(Name = "Ảnh đại diện")]
        public IFormFile? ImageFile { get; set; } // Input upload file

        public string? CurrentImageUrl { get; set; } // Để hiển thị ảnh cũ khi Edit
    }
}
