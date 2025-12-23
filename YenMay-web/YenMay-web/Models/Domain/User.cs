using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace YenMay_web.Models.Domain
{
    public class User : IdentityUser<int>
    {
        // Thông tin mở rộng
        public string? Address { get; set; }

        // Quan hệ
        public Cart Cart { get; set; }
        public ICollection<Order> Orders { get; set; }
        public ICollection<ProductReview> Reviews { get; set; }
    }
}
