using Microsoft.EntityFrameworkCore;
using YenMay_web.Data;
using YenMay_web.Models.Domain;
using YenMay_web.Repositories.Interfaces;

namespace YenMay_web.Repositories.Implementations
{
    public class EditorImageRepository : IEditorImageRepository
    {
        private readonly YMDbContext _context;

        public EditorImageRepository(YMDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(EditorImage image)
        {
            await _context.EditorImages.AddAsync(image);
        }

        public async Task<EditorImage?> GetByIdAsync(int id)
        {
            return await _context.EditorImages.FindAsync(id);
        }

        public async Task<IEnumerable<EditorImage>> GetByProductIdAsync(int productId)
        {
            return await _context.EditorImages
                .Where(ei => ei.ProductId == productId)
                .ToListAsync();
        }

        public void Delete(EditorImage image)
        {
            _context.EditorImages.Remove(image);
        }

        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}