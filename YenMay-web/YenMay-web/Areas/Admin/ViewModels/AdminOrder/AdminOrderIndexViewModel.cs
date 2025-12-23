using YenMay_web.Models.ViewModels.Common;

namespace YenMay_web.Areas.Admin.ViewModels.AdminOrder
{
    public class AdminOrderIndexViewModel
    {
        public List<AdminOrderRowViewModel> Orders { get; set; } = new();
        public PaginationViewModel Pagination { get; set; } = new();

        public string? SearchTerm { get; set; }
        public int? StatusId { get; set; }
        public DateTime? FromDate { get; set; }
    }
}
