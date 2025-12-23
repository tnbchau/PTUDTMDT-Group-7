using YenMay_web.Models.ViewModels.Dashboard;

namespace YenMay_web.Areas.Admin.ViewModels.Dashboard
{
    public class AdminDashboardViewModel : DashboardStatsViewModel
    {
        // Phục vụ click chart
        public Dictionary<string, int> OrdersByDate { get; set; } = new();
    }
}
