using System.Collections.Generic;
using YenMay_web.Models.ViewModels.Common;

namespace YenMay_web.Areas.Admin.ViewModels.Shipping
{
    public class ShippingRuleIndexViewModel
    {
        public List<ShippingRuleRowViewModel> Rules { get; set; } = new();

        public PaginationViewModel Pagination { get; set; }

        // Filter (nếu muốn)
        public string? SearchTerm { get; set; }
        public bool? IsActive { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
