using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YenMay_web.Models.Domain
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public string ShortDescriptionHTML { get; set; }
        public string FullDescriptionHTML { get; set; }
        public string AdditionalInfoHTML { get; set; }
        public string SKU { get; set; }
        public string? Slug { get; set; }
        public int StockQuantity { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [NotMapped]
        public int SoldCount { get; set; }

        [Display(Name = "Danh mục sản phẩm")]

        public int CategoryId { get; set; }
        public Category Category { get; set; }

        public ICollection<ProductImage> Images { get; set; }
        public ICollection<ProductReview> Reviews { get; set; }
    }
}
