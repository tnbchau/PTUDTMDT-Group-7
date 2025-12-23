using YenMay_web.Models.Domain;

namespace YenMay_web.Repositories.Interfaces
{
    public interface ICategoryArticleRepository
    {
        Task<IEnumerable<CategoryArticle>> GetAllAsync();
        Task<CategoryArticle?> GetByIdAsync(int id);
        Task AddAsync(CategoryArticle category);
        Task UpdateAsync(CategoryArticle category);
        Task DeleteAsync(int id);
        Task<IEnumerable<CategoryArticle>> GetAllWithCountAsync();
        Task<int> CountArticlesInCategoryAsync(int categoryId);
    }
}