using YenMay_web.Models.Domain;

namespace YenMay_web.Repositories.Interfaces
{
    public interface IShippingRepository
    {
        Task<ShippingRule?> GetByIdAsync(int id);
        Task<List<ShippingRule>> GetAllAsync();

        Task<List<ShippingRule>> GetActiveRulesAsync();

        Task CreateAsync(ShippingRule rule);
        Task UpdateAsync(ShippingRule rule);
        Task DeleteAsync(int id);
    }
}