using System.ComponentModel.DataAnnotations;

namespace YenMay_web.Models.ViewModels.Products
{
    public class ProductReviewViewModel
    {
        public int Id { get; set; }

        [Range(1, 5, ErrorMessage = "Đánh giá phải từ 1 đến 5 sao")]
        public int Rating { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập nội dung đánh giá")]
        [StringLength(1000, ErrorMessage = "Đánh giá không quá 1000 ký tự")]
        public string Comment { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }
        public string CreatedAtFormatted => CreatedAt.ToString("dd/MM/yyyy HH:mm");

        // Người dùng
        public string UserName { get; set; } = string.Empty;
        public string UserAvatar { get; set; } = "/images/default-avatar.png";

        // Hiển thị sao
        public string StarClass => $"star-{Rating}";
        public string RatingText => $"{Rating}/5 sao";

        // Cho form submit
        public int ProductId { get; set; }
    }
}