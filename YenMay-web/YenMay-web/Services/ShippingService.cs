using YenMay_web.Models.Domain;
using YenMay_web.Models.ViewModels.Shipping;
using YenMay_web.Repositories.Interfaces;
using YenMay_web.Services.Interfaces; // Sửa namespace trùng khớp interface

namespace YenMay_web.Services
{
    // Helper Class: Chuyển đổi dữ liệu (Mapping)
    public static class ShippingMappingExtensions
    {
        public static ShippingRuleViewModel ToViewModel(this ShippingRule rule)
        {
            return new ShippingRuleViewModel
            {
                Id = rule.Id,
                Name = rule.Name,
                Description = rule.Description,
                MinOrderValue = rule.MinOrderValue,
                MaxOrderValue = rule.MaxOrderValue,
                ShippingFee = rule.ShippingFee,
                IsActive = rule.IsActive
            };
        }

        public static ShippingRule ToEntity(this ShippingRuleViewModel model)
        {
            return new ShippingRule
            {
                Id = model.Id,
                Name = model.Name,
                Description = model.Description,
                MinOrderValue = model.MinOrderValue,
                MaxOrderValue = model.MaxOrderValue,
                ShippingFee = model.ShippingFee,
                IsActive = model.IsActive
            };
        }
    }

    // Main Service Implementation
    public class ShippingService : IShippingService
    {
        private readonly IShippingRepository _shippingRepository;
        private const decimal DEFAULT_FEE = 30000; // Phí mặc định nếu không khớp rule nào

        public ShippingService(IShippingRepository shippingRepository)
        {
            _shippingRepository = shippingRepository;
        }

        // ==========================================================
        // 1. LOGIC TÍNH PHÍ VẬN CHUYỂN
        // ==========================================================
        public async Task<ShippingCalculationResult> CalculateShippingAsync(decimal orderValue, string? province = null, string? district = null)
        {
            try
            {
                var activeRules = await _shippingRepository.GetActiveRulesAsync();

                // Logic: Tìm rule mà giá trị đơn hàng nằm trong khoảng [Min, Max)
                // Ưu tiên rule có phí thấp nhất nếu các khoảng chồng chéo nhau
                var matchedRule = activeRules
                    .Where(r => orderValue >= r.MinOrderValue && orderValue < r.MaxOrderValue)
                    .OrderBy(r => r.ShippingFee)
                    .FirstOrDefault();

                decimal fee = matchedRule != null ? matchedRule.ShippingFee : DEFAULT_FEE;
                string ruleName = matchedRule != null ? matchedRule.Name : "Phí vận chuyển tiêu chuẩn";

                // Logic tính ngày giao hàng dựa trên Tỉnh/Thành
                int deliveryDays = 4; // Mặc định: Liên tỉnh
                if (!string.IsNullOrEmpty(province) && (province.Contains("Hà Nội") || province.Contains("Hồ Chí Minh")))
                {
                    deliveryDays = 2; // Nội thành/Thành phố lớn
                }

                return new ShippingCalculationResult
                {
                    Success = true,
                    ShippingFee = fee,
                    Message = fee == 0 ? "Miễn phí vận chuyển" : ruleName,
                    AppliedRuleName = ruleName,
                    EstimatedDelivery = DateTime.Now.AddDays(deliveryDays)
                };
            }
            catch (Exception ex)
            {
                // Fallback an toàn
                return new ShippingCalculationResult
                {
                    Success = false,
                    ShippingFee = DEFAULT_FEE,
                    Message = "Lỗi tính phí: " + ex.Message,
                    EstimatedDelivery = DateTime.Now.AddDays(5)
                };
            }
        }

        // ==========================================================
        // 2. TIỆN ÍCH (DANH SÁCH TỈNH THÀNH)
        // ==========================================================
        public Task<List<string>> GetProvincesAsync()
        {
            var provinces = new List<string>
            {
                "An Giang", "Bắc Ninh", "Cà Mau", "Cao Bằng", "Đắk Lắk",
                "Điện Biên", "Đồng Nai", "Đồng Tháp", "Gia Lai", "Hà Tĩnh",
                "Hưng Yên", "Khánh Hoà", "Lai Châu", "Lâm Đồng", "Lạng Sơn",
                "Lào Cai", "Nghệ An", "Ninh Bình", "Phú Thọ", "Quảng Ngãi",
                "Quảng Ninh", "Quảng Trị", "Sơn La", "Tây Ninh", "Thái Nguyên",
                "Thanh Hóa", "TP. Cần Thơ", "TP. Đà Nẵng", "TP. Hà Nội",
                "TP. Hải Phòng", "TP. Hồ Chí Minh", "TP. Huế", "Tuyên Quang",
                "Vĩnh Long"
            };

            return Task.FromResult(provinces.OrderBy(p => p).ToList());
        }

        // ==========================================================
        // 3. QUẢN LÝ RULES (CRUD CHO ADMIN)
        // ==========================================================
        public async Task<List<ShippingRuleViewModel>> GetAllRulesAsync()
        {
            var rules = await _shippingRepository.GetAllAsync();
            return rules.Select(r => r.ToViewModel()).ToList();
        }

        public async Task<ShippingRuleViewModel?> GetRuleByIdAsync(int id)
        {
            var rule = await _shippingRepository.GetByIdAsync(id);
            return rule?.ToViewModel();
        }

        public async Task CreateRuleAsync(ShippingRuleViewModel model)
        {
            // Validate Logic (Optional)
            if (model.MinOrderValue >= model.MaxOrderValue)
            {
                throw new ArgumentException("Giá trị đơn hàng tối đa phải lớn hơn tối thiểu.");
            }

            var entity = model.ToEntity();
            await _shippingRepository.CreateAsync(entity);
        }

        public async Task UpdateRuleAsync(ShippingRuleViewModel model)
        {
            var existingRule = await _shippingRepository.GetByIdAsync(model.Id);
            if (existingRule != null)
            {
                existingRule.Name = model.Name;
                existingRule.Description = model.Description;
                existingRule.MinOrderValue = model.MinOrderValue;
                existingRule.MaxOrderValue = model.MaxOrderValue;
                existingRule.ShippingFee = model.ShippingFee;
                existingRule.IsActive = model.IsActive;

                await _shippingRepository.UpdateAsync(existingRule);
            }
        }

        public async Task DeleteRuleAsync(int id)
        {
            await _shippingRepository.DeleteAsync(id);
        }

        public async Task ToggleRuleStatusAsync(int id)
        {
            var rule = await _shippingRepository.GetByIdAsync(id);
            if (rule != null)
            {
                rule.IsActive = !rule.IsActive;
                await _shippingRepository.UpdateAsync(rule);
            }
        }
    }
}