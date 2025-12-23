using System;
using YenMay_web.Utilities;

namespace YenMay_web.Areas.Admin.ViewModels.Shipping
{
    public class ShippingRuleRowViewModel
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }

        public decimal MinOrderValue { get; set; }
        public string MinOrderValueText => OrderHelper.FormatCurrency(MinOrderValue);

        public decimal MaxOrderValue { get; set; }
        public string MaxOrderValueText => OrderHelper.FormatCurrency(MaxOrderValue);

        public decimal ShippingFee { get; set; }
        public string ShippingFeeText => OrderHelper.FormatCurrency(ShippingFee);

        public bool IsActive { get; set; }
        public string StatusText => IsActive ? "Hoạt động" : "Ngừng hoạt động";
    }
}
