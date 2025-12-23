using YenMay_web.Enums;
using YenMay_web.Models.Domain;
using YenMay_web.Models.ViewModels.Checkout;
using YenMay_web.Models.ViewModels.Common;
using YenMay_web.Models.ViewModels.Orders;
using YenMay_web.Repositories.Interfaces;
using YenMay_web.Services.Interfaces;
using YenMay_web.Utilities;

namespace YenMay_web.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;

        public OrderService(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        // =====================================================
        // 1. CREATE / UPDATE TRANSACTION
        // =====================================================

        public async Task<Order> PlaceOrderAsync(
    CheckoutRequestViewModel model,
    Cart cart,
    int? userId,
    decimal shippingFee,
    string? shippingRuleName = null)
        {
            if (cart == null || !cart.Items.Any())
                throw new InvalidOperationException("Giỏ hàng trống, không thể tạo đơn hàng.");

            string orderCode;
            do
            {
                orderCode = OrderHelper.GenerateOrderCode();
            }
            while (await _orderRepository.IsOrderCodeExistsAsync(orderCode));

            var subTotal = cart.Items.Sum(i => i.Price * i.Quantity);

            var order = new Order
            {
                OrderCode = orderCode,
                UserId = userId,

                CustomerName = model.CustomerName,
                CustomerPhone = model.CustomerPhone,
                CustomerEmail = model.CustomerEmail,
                ShippingAddress = model.ShippingAddress,
                Notes = model.Notes,

                PaymentMethod = model.SelectedPaymentMethod,
                Status = OrderStatus.Pending,

                SubTotal = subTotal,
                ShippingFee = shippingFee,
                TotalAmount = subTotal + shippingFee,
                ShippingRuleName = shippingRuleName,

                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,

                Items = cart.Items.Select(i => new OrderItem
                {
                    ProductId = i.ProductId,
                    Price = i.Price,
                    Quantity = i.Quantity,
                }).ToList()
            };

            return await _orderRepository.AddAsync(order);
        }


        public async Task<bool> CancelOrderAsync(int orderId, string? reason = null)
        {
            var order = await _orderRepository.GetByIdAsync(orderId);
            if (order == null)
                return false;

            // Chỉ cho phép hủy khi chưa giao
            if (order.Status == OrderStatus.Shipping ||
                order.Status == OrderStatus.Completed)
                return false;

            await _orderRepository.CancelOrderDbAsync(
                orderId,
                reason ?? "Khách hàng yêu cầu hủy"
            );

            return true;
        }

        public async Task UpdateOrderStatusAsync(
            int orderId,
            int statusId,
            string? trackingNumber = null)
        {
            await _orderRepository.UpdateStatusAsync(orderId, statusId);
            // TrackingNumber: mở rộng sau nếu cần
        }

        // =====================================================
        // 2. VIEW MODELS (READ)
        // =====================================================

        public async Task<OrderDetailViewModel?> GetOrderDetailViewAsync(
            string orderCode,
            int? userId = null)
        {
            var order = await _orderRepository.GetByCodeAsync(orderCode);
            if (order == null)
                return null;

            // Security check
            if (userId.HasValue && order.UserId != userId.Value)
                return null;

            return new OrderDetailViewModel
            {
                Id = order.Id,
                OrderCode = order.OrderCode,
                CreatedAt = order.CreatedAt,

                OrderStatusId = (int)order.Status,
                OrderStatus = OrderHelper.GetOrderStatusText(order.Status),

                CustomerName = order.CustomerName,
                CustomerPhone = order.CustomerPhone,
                CustomerEmail = order.CustomerEmail,
                ShippingAddress = order.ShippingAddress,
                Notes = order.Notes,

                PaymentMethod = order.PaymentMethod,

                SubTotal = order.SubTotal,
                ShippingFee = order.ShippingFee,
                ShippingRuleName = order.ShippingRuleName,
                TotalAmount = order.TotalAmount,

                Items = order.Items.Select(i =>
                {
                    var firstImage = i.Product?.Images?.FirstOrDefault();

                    return new OrderItemViewModel
                    {
                        ProductId = i.ProductId,
                        ProductName = i.Product!.Name,
                        ProductImage = firstImage != null
                            ? ImageHelper.GetImageUrl(firstImage.ImageUrl)
                            : "/images/no-image.png",
                        Price = i.Price,
                        Quantity = i.Quantity
                    };
                }).ToList()

            };
        }

        public async Task<OrderSuccessViewModel?> GetOrderSuccessViewAsync(string orderCode)
        {
            var order = await _orderRepository.GetByCodeAsync(orderCode);
            if (order == null)
                return null;

            return new OrderSuccessViewModel
            {
                OrderId = order.Id,
                OrderCode = order.OrderCode,
                TotalAmount = order.TotalAmount,
                CustomerEmail = order.CustomerEmail,
                CustomerPhone = order.CustomerPhone,
                PaymentMethod = order.PaymentMethod
            };
        }

        public async Task<OrderHistoryViewModel> GetOrderHistoryViewAsync(int? userId, int page = 1, int pageSize = 10)
        {
            if (!userId.HasValue) return new OrderHistoryViewModel();

            var (orders, totalCount) = await _orderRepository.GetByUserIdPagedAsync(userId.Value, page, pageSize);

            return new OrderHistoryViewModel
            {
                Orders = orders.Select(o => new OrderSummaryViewModel
                {
                    Id = o.Id,
                    OrderCode = o.OrderCode,
                    CreatedAt = o.CreatedAt,
                    OrderStatusId = (int)o.Status,
                    OrderStatus = OrderHelper.GetOrderStatusText(o.Status),
                    TotalItems = o.Items.Count,
                    TotalAmount = o.TotalAmount
                }).ToList(),
                Pagination = new PaginationViewModel
                {
                    PageIndex = page,
                    PageSize = pageSize,
                    TotalCount = totalCount
                }
            };
        }

        // =====================================================
        // 3. INTERNAL USE
        // =====================================================

        public async Task<Order?> GetOrderEntityByIdAsync(int id)
        {
            return await _orderRepository.GetByIdAsync(id);
        }
        public async Task<OrderDetailViewModel?> TrackOrderAsync(string orderCode, string phone)
        {
            var order = await _orderRepository.GetByCodeAndPhoneAsync(orderCode, phone);
            if (order == null) return null;

            return new OrderDetailViewModel
            {
                Id = order.Id,
                OrderCode = order.OrderCode,
                CreatedAt = order.CreatedAt,

                OrderStatusId = (int)order.Status,
                OrderStatus = OrderHelper.GetOrderStatusText(order.Status),

                CustomerName = order.CustomerName,
                CustomerPhone = order.CustomerPhone,
                CustomerEmail = order.CustomerEmail,
                ShippingAddress = order.ShippingAddress,
                Notes = order.Notes,

                PaymentMethod = order.PaymentMethod,

                SubTotal = order.SubTotal,
                ShippingFee = order.ShippingFee,
                ShippingRuleName = order.ShippingRuleName,
                TotalAmount = order.TotalAmount,

                Items = order.Items.Select(i =>
                {
                    var firstImage = i.Product?.Images?.FirstOrDefault();
                    return new OrderItemViewModel
                    {
                        ProductId = i.ProductId,
                        ProductName = i.Product!.Name,
                        ProductImage = firstImage != null
                            ? ImageHelper.GetImageUrl(firstImage.ImageUrl)
                            : "/images/no-image.png",
                        Price = i.Price,
                        Quantity = i.Quantity
                    };
                }).ToList()
            };
        }

    }
}
