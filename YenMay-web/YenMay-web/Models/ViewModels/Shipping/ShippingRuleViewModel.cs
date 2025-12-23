using System.ComponentModel.DataAnnotations;

namespace YenMay_web.Models.ViewModels.Shipping
{
    public class ShippingRuleViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Tên quy tắc là bắt buộc")]
        [Display(Name = "Tên quy tắc")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "Mô tả")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Giá trị đơn hàng tối thiểu là bắt buộc")]
        [Display(Name = "Đơn hàng tối thiểu")]
        [Range(0, double.MaxValue, ErrorMessage = "Giá trị phải lớn hơn hoặc bằng 0")]
        public decimal MinOrderValue { get; set; }

        [Display(Name = "Đơn hàng tối đa")]
        [Range(0, double.MaxValue, ErrorMessage = "Giá trị phải lớn hơn hoặc bằng 0")]
        public decimal MaxOrderValue { get; set; }

        [Required(ErrorMessage = "Phí vận chuyển là bắt buộc")]
        [Display(Name = "Phí vận chuyển")]
        [Range(0, double.MaxValue, ErrorMessage = "Phí vận chuyển phải lớn hơn hoặc bằng 0")]
        public decimal ShippingFee { get; set; }

        [Display(Name = "Kích hoạt")]
        public bool IsActive { get; set; }
                
        [Display(Name = "Điều kiện")]
        public string ConditionText
        {
            get
            {
                if (MinOrderValue > 0 && MaxOrderValue > 0)
                    return $"Đơn từ {MinOrderValue:N0}đ đến {MaxOrderValue:N0}đ";
                if (MinOrderValue > 0)
                    return $"Đơn từ {MinOrderValue:N0}đ trở lên";
                return "Áp dụng cho mọi đơn hàng";
            }
        }

        // Helper property
        public bool IsFreeShipping => ShippingFee == 0;
    }
}