using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using YenMay_web.Areas.Admin.ViewModels; // Để dùng ProductAdminImageViewModel

namespace YenMay_web.Areas.Admin.ViewModels.AdminProduct
{
    public class AdminProductFormViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Tên sản phẩm")]
        [Required(ErrorMessage = "Vui lòng nhập tên sản phẩm")]
        [StringLength(200, ErrorMessage = "Tên sản phẩm không được quá 200 ký tự")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "Mã SKU")]
        [Required(ErrorMessage = "Vui lòng nhập mã SKU")]
        [StringLength(50, ErrorMessage = "SKU không được quá 50 ký tự")]
        public string SKU { get; set; } = string.Empty;

        [Display(Name = "Giá bán")]
        [Required(ErrorMessage = "Vui lòng nhập giá bán")]
        [Range(0, double.MaxValue, ErrorMessage = "Giá bán phải lớn hơn hoặc bằng 0")]
        public decimal Price { get; set; }

        [Display(Name = "Số lượng tồn kho")]
        [Required(ErrorMessage = "Vui lòng nhập số lượng")]
        [Range(0, int.MaxValue, ErrorMessage = "Số lượng phải là số nguyên dương")]
        public int StockQuantity { get; set; }

        [Display(Name = "Danh mục sản phẩm")]
        [Required(ErrorMessage = "Vui lòng chọn danh mục")]
        public int CategoryId { get; set; } 

        public IEnumerable<SelectListItem>? CategoryList { get; set; }

        [Display(Name = "Mô tả ngắn")]
        public string? ShortDescriptionHTML { get; set; }

        [Display(Name = "Mô tả chi tiết")]
        public string? FullDescriptionHTML { get; set; }

        [Display(Name = "Thông tin bổ sung")]
        public string? AdditionalInfoHTML { get; set; }


        [Display(Name = "Tải ảnh mới")]
        public List<IFormFile>? ImageFiles { get; set; }

        public List<ProductAdminImageViewModel> ExistingImages { get; set; } = new List<ProductAdminImageViewModel>();

        public List<int>? DeletedImageIds { get; set; }
    }
}