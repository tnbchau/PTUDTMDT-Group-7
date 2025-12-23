using YenMay_web.Models.ViewModels.Shipping;

namespace YenMay_web.Services.Interfaces
{
    public interface IShippingService
    {        
        Task<ShippingCalculationResult> CalculateShippingAsync(decimal orderValue, string? province = null, string? district = null);

        Task<List<string>> GetProvincesAsync();
        Task<List<ShippingRuleViewModel>> GetAllRulesAsync();
        Task<ShippingRuleViewModel?> GetRuleByIdAsync(int id);
        Task CreateRuleAsync(ShippingRuleViewModel model);
        Task UpdateRuleAsync(ShippingRuleViewModel model);
        Task DeleteRuleAsync(int id);
        Task ToggleRuleStatusAsync(int id);
    }
}