using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using YenMay_web.Models.Domain;

namespace YenMay_web.Data
{
    public class YMDbContext
        : IdentityDbContext<User, IdentityRole<int>, int>
    {
        public YMDbContext(DbContextOptions<YMDbContext> options)
            : base(options)
        {
        }

        // KHÔNG cần DbSet<User>
        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductImage> ProductImages { get; set; }
        public DbSet<ProductReview> ProductReviews { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<EditorImage> EditorImages { get; set; }
        public DbSet<Article> Articles { get; set; }
        public DbSet<CategoryArticle> CategoryArticles { get; set; }
        public DbSet<ShippingRule> ShippingRules { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Product>()
                .Property(p => p.Price)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<OrderItem>()
                .Property(oi => oi.Price)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Product>()
                .Property(p => p.CreatedDate)
                .HasDefaultValueSql("GETDATE()");

            modelBuilder.Entity<ShippingRule>()
                .Property(s => s.MinOrderValue)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<ShippingRule>()
                .Property(s => s.MaxOrderValue)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<ShippingRule>()
                .Property(s => s.ShippingFee)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<CartItem>()
                .Property(c => c.Price)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<ShippingRule>().HasData(
                new ShippingRule
                {
                    Id = 1,
                    Name = "Miễn phí vận chuyển",
                    Description = "Miễn phí vận chuyển cho đơn hàng từ 500.000đ",
                    MinOrderValue = 500000,
                    MaxOrderValue = 0,
                    ShippingFee = 0,
                    IsActive = true,
                },
                new ShippingRule
                {
                    Id = 2,
                    Name = "Phí vận chuyển tiêu chuẩn",
                    Description = "Phí vận chuyển cho đơn hàng dưới 500.000đ",
                    MinOrderValue = 0,
                    MaxOrderValue = 500000,
                    ShippingFee = 30000,
                    IsActive = true,
                }
            );
        }
    }
}
