using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using YenMay_web.Enums;
using YenMay_web.Models.ViewModels.Orders;

namespace YenMay_web.Areas.Admin.ViewModels.AdminOrder
{
    public class AdminOrderFormViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Chi tiết đơn hàng")]
        public OrderDetailViewModel Order { get; set; } = new OrderDetailViewModel();

        [Display(Name = "Trạng thái đơn hàng")]
        public OrderStatus Status { get; set; }

        // Dropdown select các trạng thái để admin thay đổi
        public List<OrderStatus> AvailableStatuses { get; set; } = new List<OrderStatus>
        {
            OrderStatus.Pending,
            OrderStatus.Confirmed,
            OrderStatus.Shipping,
            OrderStatus.Completed,
            OrderStatus.Cancelled
        };
    }
}
