using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using YenMay_web.Enums;

namespace YenMay_web.Models.Domain
{
    public class Order
    {
        public int Id { get; set; }

        [Required, StringLength(50)]
        public string OrderCode { get; set; } = string.Empty;

        // Cho phép null để hỗ trợ khách vãng lai
        public int? UserId { get; set; }
        public User? User { get; set; }

        public OrderStatus Status { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal SubTotal { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal ShippingFee { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        // Lưu thông tin quy tắc vận chuyển
        public int? ShippingRuleId { get; set; }
        public string? ShippingRuleName { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập địa chỉ giao hàng")]
        [StringLength(255)]
        public string ShippingAddress { get; set; } = string.Empty;

        // Thông tin khách hàng (Bắt buộc nếu là Guest)
        public string? CustomerName { get; set; }
        public string? CustomerEmail { get; set; }
        public string? CustomerPhone { get; set; }

        // Phương thức thanh toán
        public PaymentMethod PaymentMethod { get; set; }

        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();

        [NotMapped]
        public bool IsGuestOrder => UserId == null;
    }
}