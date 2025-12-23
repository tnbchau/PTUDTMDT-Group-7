using Microsoft.EntityFrameworkCore;
using YenMay_web.Data;
using YenMay_web.Models.Domain;
using YenMay_web.Repositories.Interfaces;

namespace YenMay_web.Repositories
{
    public class ShippingRepository : IShippingRepository
    {
        private readonly YMDbContext _context;

        public ShippingRepository(YMDbContext context)
        {
            _context = context;
        }

        // --- 1. LẤY DỮ LIỆU ---

        public async Task<ShippingRule?> GetByIdAsync(int id)
        {
            return await _context.ShippingRules.FindAsync(id);
        }

        public async Task<List<ShippingRule>> GetAllAsync()
        {
            return await _context.ShippingRules
                .OrderBy(r => r.MinOrderValue) // Sắp xếp cho dễ nhìn
                .ToListAsync();
        }

        public async Task<List<ShippingRule>> GetActiveRulesAsync()
        {
            return await _context.ShippingRules
                .Where(r => r.IsActive)
                .OrderBy(r => r.MinOrderValue)
                .ToListAsync();
        }

        // --- 2. TÁC ĐỘNG DỮ LIỆU ---

        public async Task CreateAsync(ShippingRule rule)
        {
            _context.ShippingRules.Add(rule);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(ShippingRule rule)
        {
            _context.ShippingRules.Update(rule);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var rule = await _context.ShippingRules.FindAsync(id);
            if (rule != null)
            {
                _context.ShippingRules.Remove(rule);
                await _context.SaveChangesAsync();
            }
        }
    }
}