using Microsoft.EntityFrameworkCore;
using YenMay_web.Data;
using YenMay_web.Models.Domain;
using YenMay_web.Repositories.Interfaces;

namespace YenMay_web.Repositories.Implementations
{
    public class CategoryArticleRepository : ICategoryArticleRepository
    {
        private readonly YMDbContext _context;

        public CategoryArticleRepository(YMDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<CategoryArticle>> GetAllAsync()
        {
            return await _context.CategoryArticles.ToListAsync(); 
        }

        public async Task<CategoryArticle?> GetByIdAsync(int id)
        {
            return await _context.CategoryArticles.FindAsync(id);
        }

        public async Task AddAsync(CategoryArticle category)
        {
            await _context.CategoryArticles.AddAsync(category);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(CategoryArticle category)
        {
            _context.CategoryArticles.Update(category);
        }

        public async Task DeleteAsync(int id)
        {
            var category = await _context.CategoryArticles.FindAsync(id);
            if (category != null)
            {
                _context.CategoryArticles.Remove(category);
                await _context.SaveChangesAsync();
            }
        }
        public async Task<IEnumerable<CategoryArticle>> GetAllWithCountAsync()
        {
            return await _context.CategoryArticles
                .Select(c => new CategoryArticle
                {
                    Id = c.Id,
                    Name = c.Name,
                    Slug = c.Slug,
                }).ToListAsync();
        }
        public async Task<int> CountArticlesInCategoryAsync(int categoryId)
        {
                return await _context.Articles
                .CountAsync(a => a.CategoryArticleId == categoryId && a.IsPublished);
        }

    }
}