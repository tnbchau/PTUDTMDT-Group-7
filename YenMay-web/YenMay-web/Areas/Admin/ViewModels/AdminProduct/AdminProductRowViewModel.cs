using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace YenMay_web.Areas.Admin.ViewModels.AdminProduct
{
    public class AdminProductRowViewModel
    {
        public int Id { get; set; }

        public string? ImageUrl { get; set; }
        public string Name { get; set; } = string.Empty;
        public string SKU { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public int StockQuantity { get; set; }
        public bool IsLowStock { get; set; }
        public int SoldCount { get; set; }
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public decimal Price { get; set; }
    }
}
