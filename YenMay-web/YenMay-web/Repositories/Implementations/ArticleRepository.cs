using Microsoft.EntityFrameworkCore;
using YenMay_web.Data;
using YenMay_web.Models.Domain;
using YenMay_web.Repositories.Interfaces;

namespace YenMay_web.Repositories.Implementations
{
    public class ArticleRepository : IArticleRepository
    {
        private readonly YMDbContext _context;
        public ArticleRepository(YMDbContext context) => _context = context;

        public async Task<IEnumerable<Article>> GetAllAsync()
        {
            // Sắp xếp bài mới nhất lên đầu
            return await _context.Articles
                .Include(a => a.CategoryArticle)
                .OrderByDescending(a => a.CreatedDate).ToListAsync();
        }

        public async Task<Article?> GetByIdAsync(int id)
        {
            return await _context.Articles
                .Include(a => a.CategoryArticle)
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task AddAsync(Article article)
        {
            await _context.Articles.AddAsync(article);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Article article)
        {
            _context.Articles.Update(article);
        }

        public async Task DeleteAsync(int id)
        {
            var article = await _context.Articles.FindAsync(id);
            if (article != null)
            {
                _context.Articles.Remove(article);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Articles.AnyAsync(e => e.Id == id);
        }
        public async Task<IEnumerable<Article>> GetPublishedArticlesAsync()
        {
            return await _context.Articles
                .Include(a => a.CategoryArticle)
                .Where(a => a.IsPublished)
                .OrderByDescending(a => a.CreatedDate)
                .ToListAsync();
        }

        // Lấy bài viết phân trang (cho danh sách)
        public async Task<(IEnumerable<Article> Articles, int TotalCount)> GetPublishedArticlesPagedAsync(
            int page = 1,
            int pageSize = 12,
            string? searchTerm = null,
            int? categoryId = null)
        {
            IQueryable<Article> query = _context.Articles
                .Include(a => a.CategoryArticle)
                .Where(a => a.IsPublished);

            // Filter by category
            if (categoryId.HasValue)
            {
                query = query.Where(a => a.CategoryArticleId == categoryId.Value);
            }

            // Filter by search term
            if (!string.IsNullOrEmpty(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                query = query.Where(a =>
                    a.Title.ToLower().Contains(searchTerm) ||
                    a.ShortDescription.ToLower().Contains(searchTerm) ||
                    (a.CategoryArticle != null && a.CategoryArticle.Name.ToLower().Contains(searchTerm))
                );
            }

            // Order by newest
            query = query.OrderByDescending(a => a.CreatedDate);

            // Get total count
            int totalCount = await query.CountAsync();

            // Get paged results
            var articles = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (articles, totalCount);
        }

        // Lấy bài viết theo slug
        public async Task<Article?> GetBySlugAsync(string slug)
        {
            return await _context.Articles
                .Include(a => a.CategoryArticle)
                .FirstOrDefaultAsync(a => a.Slug == slug && a.IsPublished);
        }

        // Lấy danh sách 5 bài viết mới nhất
        public async Task<IEnumerable<Article>> GetRecentArticlesAsync(int count = 5)
        {
            return await _context.Articles
                .Include(a => a.CategoryArticle)
                .Where(a => a.IsPublished)
                .OrderByDescending(a => a.CreatedDate)
                .Take(count)
                .ToListAsync();
        }

        // Lấy bài viết liên quan (cùng category)
        public async Task<IEnumerable<Article>> GetRelatedArticlesAsync(int currentArticleId, int? categoryId, int count = 4)
        {
            var query = _context.Articles
                .Include(a => a.CategoryArticle)
                .Where(a => a.IsPublished && a.Id != currentArticleId);

            if (categoryId.HasValue)
            {
                query = query.Where(a => a.CategoryArticleId == categoryId.Value);
            }

            return await query
                .OrderByDescending(a => a.CreatedDate)
                .Take(count)
                .ToListAsync();
        }

        // Tìm bài viết theo search term
        public async Task<IEnumerable<Article>> SearchArticlesAsync(string searchTerm)
        {
            return await _context.Articles
                .Include(a => a.CategoryArticle)
                .Where(a => a.IsPublished &&
                    (a.Title.Contains(searchTerm) ||
                     a.ShortDescription.Contains(searchTerm) ||
                     a.Content.Contains(searchTerm)))
                .OrderByDescending(a => a.CreatedDate)
                .ToListAsync();
        }
    }
}
