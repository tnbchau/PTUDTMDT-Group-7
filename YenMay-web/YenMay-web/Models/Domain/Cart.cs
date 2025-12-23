using System.ComponentModel.DataAnnotations.Schema;

namespace YenMay_web.Models.Domain
{
    public class Cart
    {
        public int Id { get; set; }

        // Cho user đã đăng nhập
        public int? UserId { get; set; }
        public User? User { get; set; }

        // Cho user chưa đăng nhập
        public string? SessionId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Quan hệ
        public ICollection<CartItem> Items { get; set; } = new List<CartItem>();

        // Tính tổng
        [NotMapped]
        public decimal TotalAmount => Items?.Sum(item => item.Price * item.Quantity) ?? 0;

        [NotMapped]
        public int TotalItems => Items?.Sum(item => item.Quantity) ?? 0;
    }
}