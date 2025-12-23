using System.ComponentModel.DataAnnotations;

namespace YenMay_web.Areas.Admin.ViewModels.Shipping
{
    public class ShippingRuleFormViewModel
    {
        public int Id { get; set; } // 0 nếu thêm mới

        [Required]
        [Display(Name = "Tên quy tắc")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "Mô tả")]
        public string? Description { get; set; }

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Giá trị tối thiểu phải >= 0")]
        [Display(Name = "Giá trị đơn hàng tối thiểu")]
        public decimal MinOrderValue { get; set; }

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Giá trị tối đa phải >= 0")]
        [Display(Name = "Giá trị đơn hàng tối đa")]
        public decimal MaxOrderValue { get; set; }

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Phí vận chuyển phải >= 0")]
        [Display(Name = "Phí vận chuyển")]
        public decimal ShippingFee { get; set; }

        [Display(Name = "Kích hoạt")]
        public bool IsActive { get; set; } = true;
    }
}
