namespace YenMay_web.Enums
{
    public enum OrderStatus
    {
        Pending = 1,        // Chờ xác nhận (Mới đặt)
        Confirmed = 2,      // Đã xác nhận (Admin đã duyệt)
        Shipping = 3,       // Đang giao hàng (Đã đưa cho Shipper)
        Completed = 4,      // Giao thành công (Khách đã nhận)
        Cancelled = 5       // Đã hủy
    }
}