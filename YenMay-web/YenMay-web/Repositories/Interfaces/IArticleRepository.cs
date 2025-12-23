using YenMay_web.Models.Domain;

namespace YenMay_web.Repositories.Interfaces
{
    public interface IArticleRepository
    {
        Task<IEnumerable<Article>> GetAllAsync();
        Task<Article?> GetByIdAsync(int id);
        Task AddAsync(Article article);
        Task UpdateAsync(Article article);
        Task DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);

        // USER
        Task<IEnumerable<Article>> GetPublishedArticlesAsync();

        Task<(IEnumerable<Article> Articles, int TotalCount)>
            GetPublishedArticlesPagedAsync(
                int page,
                int pageSize,
                string? searchTerm = null,
                int? categoryId = null);

        Task<Article?> GetBySlugAsync(string slug);

        Task<IEnumerable<Article>> GetRecentArticlesAsync(int count = 5);

        Task<IEnumerable<Article>> GetRelatedArticlesAsync(
            int currentArticleId,
            int? categoryId,
            int count = 4);

        Task<IEnumerable<Article>> SearchArticlesAsync(string searchTerm);
        public interface IArticleRepository
        {
            Task<IEnumerable<Article>> GetRecentArticlesAsync(int count = 5);
        }

    }
}
