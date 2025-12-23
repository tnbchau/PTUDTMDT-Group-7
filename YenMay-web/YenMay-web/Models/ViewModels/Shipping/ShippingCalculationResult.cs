namespace YenMay_web.Models.ViewModels.Shipping
{
    public class ShippingCalculationResult
    {
        public bool Success { get; set; }
        public decimal ShippingFee { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? AppliedRuleName { get; set; }
        public DateTime EstimatedDelivery { get; set; }

        public string FormattedShippingFee => ShippingFee == 0 ? "Miễn phí" : $"{ShippingFee:N0} đ";
        public string FormattedDeliveryDate => EstimatedDelivery.ToString("dd/MM/yyyy");
    }
}