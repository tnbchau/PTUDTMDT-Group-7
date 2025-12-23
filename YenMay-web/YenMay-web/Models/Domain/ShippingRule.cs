namespace YenMay_web.Models.Domain
{
    public class ShippingRule
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public decimal MinOrderValue { get; set; }
        public decimal MaxOrderValue { get; set; }
        public decimal ShippingFee { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
